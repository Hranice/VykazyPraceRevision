using Microsoft.Office.Interop.Outlook;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Models.OutlookEvents;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Core.Logging;
using VykazyPrace.Core.Services;
using VykazyPrace.UserControls.Outlook;
using Exception = System.Exception;

namespace VykazyPrace.Dialogs
{
    public partial class OutlookEvents : Form
    {
        private User? _currentUser;
        private int _currentUserId => _currentUser?.Id ?? 0;
        private DateTime _fromUtc, _toUtc;

        private bool _syncInProgress;
        private CancellationTokenSource _syncCts;

        private readonly CalendarRepository _calendarRepo = new CalendarRepository();

        private Panel _overlay;
        private Label _overlayLabel;

        public OutlookEvents(User currentUser)
        {
            InitializeComponent();

            _currentUser = currentUser;
            if (_currentUser == null)
            {
                AppLogger.Error("Nelze načíst aktuálního uživatele.");
                return;
            }
        }

        private async void OutlookEvents_Load(object sender, EventArgs e)
        {
            // Výchozí období: dnes => +7 dní (UTC)
            (_fromUtc, _toUtc) = GetDefaultDateRangeUtc(7);

            await RenderOutlookEventsAsync(flowLayoutPanel1);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _ = StartWarmSyncAsync();
        }

        private async Task StartWarmSyncAsync()
        {
            if (_syncInProgress) return;
            _syncInProgress = true;
            _syncCts = new CancellationTokenSource();
            ShowOverlay("Aktualizuji kalendář…");

            try
            {
                await RunStaAsync(async () => { await InitializeEvents(); });
                await RenderOutlookEventsAsync(flowLayoutPanel1);
            }
            catch (Exception ex)
            {
                AppLogger.Error("Warm sync selhal.", ex);
            }
            finally
            {
                HideOverlay();
                _syncInProgress = false;
                _syncCts?.Dispose();
                _syncCts = null;
            }
        }

        /// <summary>Spustí async akci na novém STA vlákně a počká na dokončení.</summary>
        private static Task RunStaAsync(Func<Task> action)
        {
            var tcs = new TaskCompletionSource<object?>();
            var th = new Thread(() =>
            {
                try { action().GetAwaiter().GetResult(); tcs.SetResult(null); }
                catch (Exception ex) { tcs.SetException(ex); }
            });
            th.IsBackground = true;
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
            return tcs.Task;
        }

        /// <summary>
        /// Načte události z výchozího kalendáře přes Outlook Interop,
        /// udělá UPSERT do DB a při nezměněných položkách přeskočí čtení Recipients.
        /// </summary>
        private async Task InitializeEvents()
        {
            Microsoft.Office.Interop.Outlook.Application outlook = null;
            Microsoft.Office.Interop.Outlook.MAPIFolder calendarFolder = null;
            Microsoft.Office.Interop.Outlook.Items items = null;

            try
            {
                outlook = new Microsoft.Office.Interop.Outlook.Application();
                var session = outlook.Session;
                calendarFolder = session.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);

                items = calendarFolder.Items;
                items.Sort("[Start]", Type.Missing);
                items.IncludeRecurrences = true;

                var en = CultureInfo.GetCultureInfo("en-US");
                var baseFromLocal = DateTime.Now.Date.AddDays(-30);
                var baseToLocal = DateTime.Now.Date.AddDays(30);

                string filter =
                    "[Start] <= '" + baseToLocal.ToString("g", en) + "' AND " +
                    "[End] >= '" + baseFromLocal.ToString("g", en) + "'";

                var restricted = items.Restrict(filter);

                var appts = new List<AppointmentItem>();
                foreach (object raw in restricted)
                {
                    if (raw is AppointmentItem a)
                        appts.Add(a);
                }

                var fromUtc = _fromUtc;
                var toUtc = _toUtc;

                var scopedAppts = appts.Where(a =>
                {
                    var su = a.Start.ToUniversalTime();
                    var eu = a.End.ToUniversalTime();
                    return su <= toUtc && eu >= fromUtc;
                }).ToList();

                AppLogger.Information(
                    $"Outlook sync: OutlookReturned={appts.Count}, InMyRange={scopedAppts.Count}, RangeUTC={fromUtc:o}..{toUtc:o}",
                    false
                );

                var userRepo = new UserRepository();
                var userCache = new Dictionary<string, int?>(StringComparer.OrdinalIgnoreCase);

                const string PR_SMTP_ADDRESS = "http://schemas.microsoft.com/mapi/proptag/0x39FE001E";

                foreach (var appt in scopedAppts)
                {
                    string storeId = calendarFolder.StoreID;
                    string folderEntryId = calendarFolder.EntryID;
                    string entryId = appt.EntryID;

                    bool isRecurringSeries = appt.RecurrenceState == OlRecurrenceState.olApptMaster;
                    bool isException = appt.RecurrenceState == OlRecurrenceState.olApptException;

                    DateTime? startUtc = appt.Start == DateTime.MinValue ? null : appt.Start.ToUniversalTime();
                    DateTime? endUtc = appt.End == DateTime.MinValue ? null : appt.End.ToUniversalTime();

                    DateTime? occurrenceStartUtc = startUtc;

                    string subject = string.IsNullOrWhiteSpace(appt.Subject) ? "(bez názvu)" : appt.Subject;
                    string location = string.IsNullOrWhiteSpace(appt.Location) ? null : appt.Location;
                    string organizer = string.IsNullOrWhiteSpace(appt.Organizer) ? null : appt.Organizer;

                    bool isAllDay = appt.AllDayEvent;
                    DateTime lastModifiedUtc = appt.LastModificationTime.ToUniversalTime();

                    // ⬇⬇⬇ NOVÉ: Outlook říká, že meeting byl zrušen
                    bool isCanceled = false;
                    try
                    {
                        isCanceled = (appt.MeetingStatus == OlMeetingStatus.olMeetingCanceled);
                    }
                    catch { /* defensivně, některé položky nemusí být "Meeting" */ }

                    var keyInfo = await _calendarRepo.TryGetItemKeyInfoAsync(storeId, entryId, occurrenceStartUtc);
                    string prevHash = keyInfo?.LastHash;

                    string currHash = ComputeStableHash(
                        subject ?? "",
                        startUtc?.ToString("o") ?? "",
                        endUtc?.ToString("o") ?? "",
                        location ?? "",
                        appt.BusyStatus.ToString(),
                        folderEntryId ?? ""
                    );

                    bool changed = !string.Equals(prevHash, currHash, StringComparison.Ordinal);

                    var ciInput = new CalendarItem
                    {
                        StoreId = storeId,
                        EntryId = entryId,
                        OccurrenceStartUtc = occurrenceStartUtc,

                        GlobalAppointmentId = SafeGet(() => appt.GlobalAppointmentID),
                        ICalUid = null,

                        LastSeenAtUtc = DateTime.UtcNow,
                        LastModifiedUtc = lastModifiedUtc,
                        LastFolderEntryId = folderEntryId,
                        LastHash = currHash,

                        Subject = subject,
                        Location = location,
                        Organizer = organizer,
                        StartUtc = startUtc,
                        EndUtc = endUtc,

                        IsAllDay = isAllDay,
                        IsRecurringSeries = isRecurringSeries,
                        IsException = isException
                    };

                    // uložíme/updatneme položku a získáme její DB Id
                    var ci = await _calendarRepo.UpsertCalendarItemAsync(ciInput).ConfigureAwait(false);

                    // teď řešíme účastníky
                    if (changed)
                    {
                        Microsoft.Office.Interop.Outlook.Recipients recipients = null;
                        var attendees = new List<ItemAttendee>();
                        try
                        {
                            recipients = appt.Recipients;

                            foreach (Microsoft.Office.Interop.Outlook.Recipient r in recipients)
                            {
                                string display = SafeGet(() => r.Name);

                                string email = null;
                                try
                                {
                                    email = r.PropertyAccessor?.GetProperty(PR_SMTP_ADDRESS) as string;
                                }
                                catch
                                {
                                    email = SafeGet(() => r.AddressEntry?.GetExchangeUser()?.PrimarySmtpAddress)
                                            ?? SafeGet(() => r.Address);
                                }

                                string role = r.Type switch
                                {
                                    (int)OlMailRecipientType.olTo => "Required",
                                    (int)OlMailRecipientType.olCC => "Optional",
                                    (int)OlMailRecipientType.olBCC => "Bcc",
                                    _ => null
                                };

                                string resp = SafeGet(() => r.MeetingResponseStatus.ToString());

                                // resolvedUserId = náš interní User.Id pokud ho známe
                                int? resolvedUserId = null;
                                if (!string.IsNullOrWhiteSpace(email))
                                {
                                    var key = email.Trim();
                                    if (!userCache.TryGetValue(key, out resolvedUserId))
                                    {
                                        try
                                        {
                                            var u = await userRepo.ResolveByEmailOrWindowsAsync(key);
                                            resolvedUserId = u?.Id;
                                        }
                                        catch { resolvedUserId = null; }
                                        userCache[key] = resolvedUserId;
                                    }
                                }

                                attendees.Add(new ItemAttendee
                                {
                                    ItemId = ci.Id,
                                    DisplayName = display,
                                    Email = email,
                                    UserId = resolvedUserId,
                                    Role = role,
                                    ResponseStatus = resp
                                });

                                // ⬇⬇⬇ NOVÉ:
                                // pokud meeting je zrušený, rovnou ho tomu uživateli schováme
                                if (isCanceled && resolvedUserId.HasValue)
                                {
                                    await _calendarRepo.SetUserStateAsync(
                                        resolvedUserId.Value,
                                        ci.Id,
                                        UserItemStateEnum.IgnoreTombstone,
                                        note: "Canceled in Outlook"
                                    ).ConfigureAwait(false);
                                }

                                System.Runtime.InteropServices.Marshal.ReleaseComObject(r);
                            }
                        }
                        catch { }
                        finally
                        {
                            if (recipients != null)
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(recipients);
                        }

                        if (attendees.Count > 0)
                            await _calendarRepo.UpsertAttendeesAsync(ci.Id, attendees).ConfigureAwait(false);

                        await _calendarRepo.LogChangeAsync(ci.Id, "SYNC_UPSERT", userId: null, detailsJson: null).ConfigureAwait(false);
                    }

                    System.Runtime.InteropServices.Marshal.ReleaseComObject(appt);
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("Chyba při čtení kalendáře z Outlooku", ex);
                AppLogger.Information(ex?.InnerException?.Message, false);
            }
            finally
            {
                if (items != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(items);
                if (calendarFolder != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(calendarFolder);
                if (outlook != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(outlook);
            }
        }

        /// <summary>Vykreslí karty pro přihlášeného uživatele.</summary>
        private async Task RenderOutlookEventsAsync(FlowLayoutPanel host)
        {
            if (_currentUser == null) return;

            if (host.InvokeRequired)
            {
                host.Invoke(new System.Action(async () => await RenderOutlookEventsAsync(host)));
                return;
            }

            host.SuspendLayout();
            var oldAutoScroll = host.AutoScroll;
            host.AutoScroll = false;
            host.Controls.Clear();

            var itemsWithState = await _calendarRepo
                .GetVisibleItemsForUserByAttendanceAsync(_currentUserId, _fromUtc, _toUtc);



            var toShow = itemsWithState
                .Where(x => x.Item != null)
                .Where(x => x.Item.StartUtc.HasValue || !x.Item.IsRecurringSeries)
                .OrderBy(x => x.Item.StartUtc ?? DateTime.MaxValue)
                .ToList();

            Func<Task> onChanged = async () => await RenderOutlookEventsAsync(host);

            var cards = new List<Control>(toShow.Count);
            foreach (var (item, state) in toShow)
            {
                DateTime dateLocal;
                DateTime? timeFromLocal = null, timeToLocal = null;

                if (item.StartUtc.HasValue)
                {
                    var s = item.StartUtc.Value.ToLocalTime();
                    dateLocal = s.Date; timeFromLocal = s;
                }
                else
                {
                    dateLocal = DateTime.Now.Date;
                }

                if (item.EndUtc.HasValue) timeToLocal = item.EndUtc.Value.ToLocalTime();

                var subject = string.IsNullOrWhiteSpace(item.Subject) ? "(bez názvu)" : item.Subject;

                var card = new OutlookEvent(
                    currentUserId: _currentUserId,
                    itemId: item.Id,
                    dateLocal: dateLocal,
                    timeFromLocal: timeFromLocal,
                    timeToLocal: timeToLocal,
                    subject: subject,
                    stateForUser: state,
                    onChanged: onChanged)
                {
                    Width = host.ClientSize.Width - (host.Padding.Left + host.Padding.Right) - 4,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
                };

                cards.Add(card);
            }

            if (cards.Count > 0) host.Controls.AddRange(cards.ToArray());

            host.AutoScroll = oldAutoScroll;
            host.ResumeLayout();
        }


        private async Task AddAllVisibleEventsAsync()
        {
            if (_currentUser == null) return;

            var service = new OutlookMeetingImportService();

            var (added, conflicts, duplicates) = await service.AddAllVisibleAsync(_currentUserId, _fromUtc, _toUtc);

            MessageBox.Show(this,
                $"Přidáno: {added}\nPřeskočeno (konflikt): {conflicts}\nPřeskočeno (duplicitní): {duplicates}",
                "Hromadné přidání",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            await RenderOutlookEventsAsync(flowLayoutPanel1);
        }

        private async void buttonAddAllEvents_Click(object sender, EventArgs e)
            => await AddAllVisibleEventsAsync();

    
        /// <summary>Výchozí interval: dnes 00:00 UTC => +N dní.</summary>
        private static (DateTime fromUtc, DateTime toUtc) GetDefaultDateRangeUtc(int daysForward = 7)
        {
            var from = DateTime.UtcNow.Date;
            var to = from.AddDays(daysForward);
            return (from, to);
        }

        /// <summary>Bezpečný getter pro COM vlastnosti (vrací null místo výjimky).</summary>
        private static T SafeGet<T>(Func<T> getter) where T : class
        {
            try { return getter(); }
            catch { return null; }
        }

        /// <summary>Stabilní SHA-256 hash pro detekci změn položky.</summary>
        private static string ComputeStableHash(params string[] parts)
        {
            using var sha = SHA256.Create();
            var joined = string.Join("|", parts.Select(p => p?.Replace("|", "||") ?? ""));
            var bytes = Encoding.UTF8.GetBytes(joined);
            var hash = sha.ComputeHash(bytes);
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        private void ShowOverlay(string text)
        {
            if (_overlay != null) return;
            _overlay = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(100, Color.Black) };
            _overlayLabel = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font(Font.FontFamily, 12f, FontStyle.Bold),
                Text = text
            };
            _overlay.Controls.Add(_overlayLabel);
            Controls.Add(_overlay);
            _overlay.BringToFront();
        }
        private void HideOverlay()
        {
            if (_overlay == null) return;
            Controls.Remove(_overlay);
            _overlay.Dispose();
            _overlay = null;
            _overlayLabel = null;
        }

        // trochu omezí blikání při přerenderu (double-buffer-like)
        protected override CreateParams CreateParams
        {
            get { var cp = base.CreateParams; cp.ExStyle |= 0x02000000; return cp; } // WS_EX_COMPOSITED
        }
    }
}

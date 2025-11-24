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
        /// Načte události z VÝCHOZÍHO kalendáře aktuálně přihlášeného profilu,
        /// udělá UPSERT do DB a při nezměněných položkách přeskočí čtení Recipients.
        /// Robustní proti COM anomáliím: používá primárně Find/FindNext, fallbacky formátu/okna,
        /// a v nejhorším ruční enumeraci přes GetFirst/GetNext bez Restrict.
        /// </summary>
        private async Task InitializeEvents()
        {
            Microsoft.Office.Interop.Outlook.Application outlook = null;
            Microsoft.Office.Interop.Outlook.NameSpace mapi = null;
            Microsoft.Office.Interop.Outlook.MAPIFolder calendarFolder = null;
            Microsoft.Office.Interop.Outlook.Items items = null;

            // ⚠ DbContext/Repo uvnitř metody -> žádné sdílení přes vlákna
            var calendarRepo = new CalendarRepository();
            var userRepo = new UserRepository();

            try
            {
                // 1) Outlook + MAPI + přihlášení profilu (bez UI)
                outlook = new Microsoft.Office.Interop.Outlook.Application();
                mapi = outlook.GetNamespace("MAPI");
                try { mapi.Logon(Type.Missing, Type.Missing, /*ShowDialog*/ false, /*NewSession*/ false); } catch { }

                // 2) Výchozí kalendář aktuálního profilu
                calendarFolder = mapi.GetDefaultFolder(OlDefaultFolders.olFolderCalendar);

                var current = mapi?.CurrentUser;
                AppLogger.Information(
                    $"Profile: '{current?.Name}' <{SafeGet(() => current?.AddressEntry?.GetExchangeUser()?.PrimarySmtpAddress) ?? current?.Address}>; " +
                    $"FolderPath='{calendarFolder?.FolderPath}', Store='{calendarFolder?.StoreID?.Substring(0, 8)}...'",
                    false
                );

                // 3) Items – rekurence + řazení (kvůli Find/Restrict)
                items = calendarFolder.Items;
                items.IncludeRecurrences = true;
                items.Sort("[Start]", Type.Missing);

                // --- definice časových oken/filtrů ---
                var fromUtc = _fromUtc; // např. dnes 00:00Z
                var toUtc = _toUtc;   // např. +7 dní

                var localFrom = fromUtc.ToLocalTime();
                var localTo = toUtc.ToLocalTime();

                // en-US: "M/d/yyyy h:mm tt" (ToString("g", en-US))
                var en = CultureInfo.GetCultureInfo("en-US");
                string enFilter(DateTime f, DateTime t) =>
                    "[Start] <= '" + t.ToString("g", en) + "' AND [End] >= '" + f.ToString("g", en) + "'";

                // invariantní 24h fallback "MM/dd/yyyy HH:mm"
                string inv(DateTime dt) => dt.ToString("MM'/'dd'/'yyyy HH':'mm", CultureInfo.InvariantCulture);
                string invFilter(DateTime f, DateTime t) =>
                    "[Start] <= '" + inv(t) + "' AND [End] >= '" + inv(f) + "'";

                // --- 4) PRIMARY: Find/FindNext s en-US a aktuálním oknem ---
                var collected = new List<AppointmentItem>();
                AppointmentItem found = null;
                string filter = enFilter(localFrom, localTo);

                try
                {
                    found = items.Find(filter) as AppointmentItem;
                    int preview = 0;
                    while (found != null)
                    {
                        collected.Add(found);
                        if (preview < 3)
                        {
                            AppLogger.Information($"Find preview: {found.Start:yyyy-MM-dd HH:mm} | {found.Subject}", false);
                            preview++;
                        }
                        found = items.FindNext() as AppointmentItem;
                    }
                }
                catch (System.Runtime.InteropServices.COMException ex)
                {
                    AppLogger.Information($"Find/FindNext COMException: 0x{ex.HResult:X8} {ex.Message}", false);
                }

                // --- 5) Fallback A: širší okno (±180d) + en-US ---
                if (collected.Count == 0)
                {
                    var wideFrom = DateTime.Now.Date.AddDays(-180);
                    var wideTo = DateTime.Now.Date.AddDays(180);
                    string wide = enFilter(wideFrom, wideTo);

                    try
                    {
                        found = items.Find(wide) as AppointmentItem;
                        while (found != null)
                        {
                            collected.Add(found);
                            found = items.FindNext() as AppointmentItem;
                        }
                        AppLogger.Information($"Find fallback en-US ±180d -> Found={collected.Count}", false);
                    }
                    catch (System.Runtime.InteropServices.COMException ex)
                    {
                        AppLogger.Information($"Find fallback en-US COMException: 0x{ex.HResult:X8} {ex.Message}", false);
                    }
                }

                // --- 6) Fallback B: invariantní formát + aktuální okno ---
                if (collected.Count == 0)
                {
                    string invF = invFilter(localFrom, localTo);
                    try
                    {
                        found = items.Find(invF) as AppointmentItem;
                        while (found != null)
                        {
                            collected.Add(found);
                            found = items.FindNext() as AppointmentItem;
                        }
                        AppLogger.Information($"Find fallback invariant ±{(toUtc - localFrom).TotalDays:0}d -> Found={collected.Count}", false);
                    }
                    catch (System.Runtime.InteropServices.COMException ex)
                    {
                        AppLogger.Information($"Find fallback invariant COMException: 0x{ex.HResult:X8} {ex.Message}", false);
                    }
                }

                // --- 7) Fallback C: RUČNÍ ENUMERACE přes GetFirst/GetNext bez filtru ---
                if (collected.Count == 0)
                {
                    AppLogger.Information("Fallback C: enumeruji celý kalendář přes GetFirst/GetNext…", false);

                    object cur = null;
                    try
                    {
                        cur = items.GetFirst();
                        while (cur != null)
                        {
                            if (cur is AppointmentItem ai)
                                collected.Add(ai);
                            cur = items.GetNext();
                        }
                    }
                    catch (System.Runtime.InteropServices.COMException ex)
                    {
                        AppLogger.Information($"Items.GetFirst/GetNext COMException: 0x{ex.HResult:X8} {ex.Message}", false);
                    }
                }

                // --- 8) Na závěr aplikuj náš UTC rozsah (když Fallback C nasbírá vše) ---
                var appts = collected
                    .Where(a =>
                    {
                        var su = a.Start == DateTime.MinValue ? DateTime.MinValue : a.Start.ToUniversalTime();
                        var eu = a.End == DateTime.MinValue ? DateTime.MinValue : a.End.ToUniversalTime();
                        return su <= toUtc && eu >= fromUtc;
                    })
                    .ToList();

                AppLogger.Information(
                    $"Outlook sync: Collected={collected.Count}, InMyRange={appts.Count}, RangeUTC={fromUtc:o}..{toUtc:o}",
                    false
                );

                // --- 9) Persist – hash, UPSERT, (podmíněně) Recipients ---
                var userCache = new Dictionary<string, int?>(StringComparer.OrdinalIgnoreCase);
                const string PR_SMTP_ADDRESS = "http://schemas.microsoft.com/mapi/proptag/0x39FE001E";

                foreach (var appt in appts)
                {
                    // ⚠ Bezpečné načtení EntryID – pokud se položka mezitím smazala/zlomila, getter hodí COMException.
                    string entryId = SafeGet(() => appt.EntryID);

                    if (string.IsNullOrEmpty(entryId))
                    {
                        AppLogger.Information(
                            "Položka kalendáře nemá platné EntryID (pravděpodobně smazaná nebo neplatná). Přeskakuji.",
                            false);

                        System.Runtime.InteropServices.Marshal.ReleaseComObject(appt);
                        continue;
                    }

                    // ⚠ StoreID a FolderEntryID také mohou selhat při COM chybách
                    string storeId = SafeGet(() => calendarFolder.StoreID) ?? "";
                    string folderEntryId = SafeGet(() => calendarFolder.EntryID) ?? "";

                    // Často bezpečné i bez wrapperu, ale obalíme — Outlook někdy vrací COM výjimky při broken items
                    bool isRecurringSeries = false;
                    bool isException = false;

                    try
                    {
                        isRecurringSeries = (appt.RecurrenceState == OlRecurrenceState.olApptMaster);
                        isException = (appt.RecurrenceState == OlRecurrenceState.olApptException);
                    }
                    catch { /* Ignorovat – item může být broken */ }

                    // Bezpečné převody času
                    DateTime? startUtc2 = null;
                    DateTime? endUtc2 = null;
                    DateTime? occurrenceStartUtc = null;

                    try
                    {
                        if (appt.Start != DateTime.MinValue)
                            startUtc2 = appt.Start.ToUniversalTime();

                        if (appt.End != DateTime.MinValue)
                            endUtc2 = appt.End.ToUniversalTime();

                        occurrenceStartUtc = startUtc2;
                    }
                    catch { }

                    // Ostatní textové properties obalené přes SafeGet
                    string subject = SafeGet(() => appt.Subject) ?? "(bez názvu)";
                    string location = SafeGet(() => appt.Location);
                    string organizer = SafeGet(() => appt.Organizer);

                    bool isAllDay = false;
                    DateTime lastModifiedUtc = DateTime.MinValue;
                    bool isCanceled = false;

                    try { isAllDay = appt.AllDayEvent; } catch { }
                    try { lastModifiedUtc = appt.LastModificationTime.ToUniversalTime(); } catch { }
                    try { isCanceled = (appt.MeetingStatus == OlMeetingStatus.olMeetingCanceled); } catch { }

                    // --- HASH & UPSERT --------------------------------------------------------
                    var keyInfo = await calendarRepo.TryGetItemKeyInfoAsync(storeId, entryId, occurrenceStartUtc);
                    string prevHash = keyInfo?.LastHash;

                    string currHash = ComputeStableHash(
                        subject ?? "",
                        startUtc2?.ToString("o") ?? "",
                        endUtc2?.ToString("o") ?? "",
                        location ?? "",
                        SafeGet(() => appt.BusyStatus.ToString()) ?? "",
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
                        StartUtc = startUtc2,
                        EndUtc = endUtc2,

                        IsAllDay = isAllDay,
                        IsRecurringSeries = isRecurringSeries,
                        IsException = isException
                    };

                    CalendarItem ci;
                    try
                    {
                        ci = await calendarRepo.UpsertCalendarItemAsync(ciInput).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        AppLogger.Error($"DB UpsertCalendarItemAsync selhal pro EntryId={entryId}", e);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(appt);
                        continue;
                    }

                    // --- RECIPIENTS ----------------------------------------------------------
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
                                try { email = r.PropertyAccessor?.GetProperty(PR_SMTP_ADDRESS) as string; }
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
                                        catch
                                        {
                                            resolvedUserId = null;
                                        }

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

                                if (isCanceled && resolvedUserId.HasValue)
                                {
                                    await calendarRepo.SetUserStateAsync(
                                        resolvedUserId.Value, ci.Id, UserItemStateEnum.IgnoreTombstone,
                                        note: "Canceled in Outlook"
                                    ).ConfigureAwait(false);
                                }

                                System.Runtime.InteropServices.Marshal.ReleaseComObject(r);
                            }
                        }
                        catch (Exception e)
                        {
                            AppLogger.Error($"Čtení Recipients selhalo (EntryId={entryId})", e);
                        }
                        finally
                        {
                            if (recipients != null)
                                System.Runtime.InteropServices.Marshal.ReleaseComObject(recipients);
                        }

                        if (attendees.Count > 0)
                        {
                            try { await calendarRepo.UpsertAttendeesAsync(ci.Id, attendees).ConfigureAwait(false); }
                            catch (Exception e) { AppLogger.Error($"DB UpsertAttendeesAsync selhal (ItemId={ci.Id})", e); }
                        }

                        try { await calendarRepo.LogChangeAsync(ci.Id, "SYNC_UPSERT", userId: null, detailsJson: null).ConfigureAwait(false); }
                        catch (Exception e) { AppLogger.Error($"DB LogChangeAsync selhal (ItemId={ci.Id})", e); }
                    }

                    // --- Uvolnění COM objektu ------------------------------------------------
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(appt);
                }

            }
            catch (Exception ex)
            {
                AppLogger.Error("Chyba při čtení kalendáře (výchozí profil)", ex);
                AppLogger.Information(ex?.InnerException?.Message, false);
            }
            finally
            {
                if (items != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(items);
                if (calendarFolder != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(calendarFolder);
                if (mapi != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(mapi);
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

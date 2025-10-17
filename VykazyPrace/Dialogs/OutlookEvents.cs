using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VykazyPrace.Core.Database.Models.OutlookEvents;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Core.Logging;

namespace VykazyPrace.Dialogs
{
    public partial class OutlookEvents : Form
    {
        public OutlookEvents()
        {
            InitializeComponent();
        }

        private async void OutlookEvents_Load(object sender, EventArgs e)
        {
            await InitializeEvents();
        }


        private async Task InitializeEvents()
        {
            // ===== ČTENÍ KALENDÁŘE PŘES OUTLOOK INTEROP + ULOŽENÍ DO SQLITE (EF Core) =====
            // Cíl: Načíst nadcházející události (např. na 7 dní dopředu) z výchozího kalendáře,
            //      pro každou spočítat klíč (StoreId + EntryId + OccurrenceStartUtc), naplnit metadata,
            //      provést UPSERT do tabulek a zapsat účastníky.

            Microsoft.Office.Interop.Outlook.Application outlook = null;
            Microsoft.Office.Interop.Outlook.MAPIFolder calendarFolder = null;
            Microsoft.Office.Interop.Outlook.Items items = null;

            var repo = new CalendarRepository();

            try
            {
                // 1) Spustíme/napojíme se na instanci Outlooku (Interop)
                outlook = new Microsoft.Office.Interop.Outlook.Application();

                // 2) Získáme MAPI session a výchozí kalendář
                var session = outlook.Session; // Current Session (MAPI)
                calendarFolder = session.GetDefaultFolder(
                    Microsoft.Office.Interop.Outlook.OlDefaultFolders.olFolderCalendar);

                // 3) Získáme kolekci položek a připravíme ji pro filtrování
                items = calendarFolder.Items;

                // Včetně opakovaných výskytů a seřadíme podle Start
                items.IncludeRecurrences = true;
                items.Sort("[Start]");

                // 4) Omezíme rozsah – od teď do +7 dní (Outlook Restrict používá EN-US formát datumu)
                var start = DateTime.Now;
                var end = DateTime.Now.AddDays(7);

                string en = "en-US";
                string filter =
                    $"[Start] >= '{start.ToString("g", System.Globalization.CultureInfo.GetCultureInfo(en))}' AND " +
                    $"[Start] <= '{end.ToString("g", System.Globalization.CultureInfo.GetCultureInfo(en))}'";

                var restricted = items.Restrict(filter);

                // 5) Enumerace výsledků
                foreach (object obj in restricted)
                {
                    if (obj is not Microsoft.Office.Interop.Outlook.AppointmentItem appt)
                        continue;

                    // ---------- IDENTITA & METADATA Z OUTLOOKU ----------
                    string storeId = calendarFolder.StoreID;
                    string folderEntryId = calendarFolder.EntryID;

                    string entryId = appt.EntryID;


                    // Rozlišení série/instance: pro occurrence použijeme start výskytu v UTC;
                    // pro master sérii necháváme OccurrenceStartUtc = NULL.
                    bool isRecurringSeries =
                        appt.RecurrenceState == Microsoft.Office.Interop.Outlook.OlRecurrenceState.olApptMaster;
                    bool isException =
                        appt.RecurrenceState == Microsoft.Office.Interop.Outlook.OlRecurrenceState.olApptException;

                    DateTime? occurrenceStartUtc =
                        isRecurringSeries ? (DateTime?)null : appt.Start.ToUniversalTime();

                    // Bezpečné načtení zobrazovaných hodnot
                    string subject = string.IsNullOrWhiteSpace(appt.Subject) ? "(bez názvu)" : appt.Subject;
                    string location = string.IsNullOrWhiteSpace(appt.Location) ? null : appt.Location;
                    string organizer = string.IsNullOrWhiteSpace(appt.Organizer) ? null : appt.Organizer;

                    DateTime? startUtc = appt.Start == DateTime.MinValue ? null : appt.Start.ToUniversalTime();
                    DateTime? endUtc = appt.End == DateTime.MinValue ? null : appt.End.ToUniversalTime();

                    bool isAllDay = appt.AllDayEvent;
                    DateTime lastModifiedUtc = appt.LastModificationTime.ToUniversalTime();

                    // ---------- HASH PRO DETEKCI ZMĚN OBSAHU ----------
                    // Doporučený mix: Subject|StartUtc|EndUtc|Location|BusyStatus|FolderEntryId
                    string lastHash = ComputeStableHash(
                        subject ?? "",
                        startUtc?.ToString("o") ?? "",
                        endUtc?.ToString("o") ?? "",
                        location ?? "",
                        appt.BusyStatus.ToString(),
                        folderEntryId ?? ""
                    );

                    // ---------- NAPLNĚNÍ CalendarItem A UPSERT DO DB ----------
                    var ciInput = new CalendarItem
                    {
                        StoreId = storeId,
                        EntryId = entryId,
                        OccurrenceStartUtc = occurrenceStartUtc,

                        GlobalAppointmentId = SafeGet(() => appt.GlobalAppointmentID),
                        ICalUid = null, // Interop nic takového neposkytuje (pro Graph by se hodilo)

                        LastSeenAtUtc = DateTime.UtcNow,
                        LastModifiedUtc = lastModifiedUtc,
                        LastFolderEntryId = folderEntryId,
                        LastHash = lastHash,

                        Subject = subject,
                        Location = location,
                        Organizer = organizer,
                        StartUtc = startUtc,
                        EndUtc = endUtc,

                        IsAllDay = isAllDay,
                        IsRecurringSeries = isRecurringSeries,
                        IsException = isException
                    };

                    // UPSERT (podle unikátního klíče StoreId + EntryId + OccurrenceStartUtc)
                    var ci = await repo.UpsertCalendarItemAsync(ciInput).ConfigureAwait(false);

                    // ---------- ÚČASTNÍCI (Recipients) ----------
                    // Organizer většinou NENÍ v Recipients; Recipients = pozvaní účastníci.
                    // Pokusíme se zjistit SMTP; když nejde, použijeme Address.
                    Microsoft.Office.Interop.Outlook.Recipients recipients = null;
                    var attendees = new List<ItemAttendee>();

                    try
                    {
                        recipients = appt.Recipients;
                        foreach (Microsoft.Office.Interop.Outlook.Recipient r in recipients)
                        {
                            string display = SafeGet(() => r.Name);
                            string email =
                                SafeGet(() => r.AddressEntry?.GetExchangeUser()?.PrimarySmtpAddress)
                                ?? SafeGet(() => r.Address);

                            string role = r.Type switch
                            {
                                (int)Microsoft.Office.Interop.Outlook.OlMailRecipientType.olTo => "Required",
                                (int)Microsoft.Office.Interop.Outlook.OlMailRecipientType.olCC => "Optional",
                                (int)Microsoft.Office.Interop.Outlook.OlMailRecipientType.olBCC => "Bcc",
                                _ => null
                            };

                            string resp = SafeGet(() => r.MeetingResponseStatus.ToString());

                            attendees.Add(new ItemAttendee
                            {
                                ItemId = ci.Id,          // jistota – i když UpsertAttendees to přepíše
                                DisplayName = display,
                                Email = email,
                                UserId = null,           // když umíš mapovat email -> Users.ID, doplníš jindy
                                Role = role,
                                ResponseStatus = resp
                            });

                            // Uvolnění konkrétního recipienta (COM)
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(r);
                        }
                    }
                    catch
                    {
                        // Účastníci jsou best-effort; když čtení selže, nevadí.
                    }
                    finally
                    {
                        if (recipients != null)
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(recipients);
                    }

                    // Nahraď účastníky pro daný ItemId (replace strategie)
                    if (attendees.Count > 0)
                    {
                        await repo.UpsertAttendeesAsync(ci.Id, attendees).ConfigureAwait(false);
                    }

                    // (Volitelné) zalogovat sync akci
                    await repo.LogChangeAsync(ci.Id, "SYNC_UPSERT", userId: null, detailsJson: null).ConfigureAwait(false);

                    // ---------- (Volitelné) UI výpis ----------
                    // string where = string.IsNullOrEmpty(location) ? "" : $" @ {location}";
                    // listBoxEvents.Items.Add($"{startUtc} | {subject}{where}");
                }
            }
            catch (System.Exception ex)
            {
                AppLogger.Error("Chyba při čtení kalendáře z Outlooku", ex);
                AppLogger.Information(ex?.InnerException?.Message, true);
            }
            finally
            {
                // Uvolníme COM objekty – snížíme riziko „visících“ procesů OUTLOOK.EXE
                if (items != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(items);
                if (calendarFolder != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(calendarFolder);
                if (outlook != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(outlook);
            }
        }

        #region Pomocné metody (hash, safe-get)

        /// <summary>
        /// Vypočítá stabilní hash (SHA-256) z kombinace důležitých polí.
        /// Slouží k rychlé detekci změn obsahu mezi syncy.
        /// </summary>
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
        
        /// <summary>
        /// Bezpečně získá vlastnost z COM objektu – pokud COM vyhodí výjimku, vrátí null.
        /// Hodí se pro r.AddressEntry?.GetExchangeUser()?.PrimarySmtpAddress apod.
        /// </summary>
        private static T SafeGet<T>(Func<T> getter) where T : class
        {
            try { return getter(); }
            catch { return null; }
        }

        #endregion

        //private void InitializeEvents()
        //{
        //    // ===== ČTENÍ KALENDÁŘE PŘES OUTLOOK OOM =====
        //    // Cíl: Načíst nadcházející události (např. na 7 dní dopředu) z výchozího kalendáře.

        //    Microsoft.Office.Interop.Outlook.Application outlook = null;
        //    Microsoft.Office.Interop.Outlook.MAPIFolder calendarFolder = null;
        //    Microsoft.Office.Interop.Outlook.Items items = null;

        //    try
        //    {
        //        // 1) Spustíme/napojíme se na instanci Outlooku
        //        outlook = new Microsoft.Office.Interop.Outlook.Application();

        //        // 2) Získáme session a výchozí kalendář
        //        var session = outlook.Session; // Current Session (MAPI)
        //        calendarFolder = session.GetDefaultFolder(Microsoft.Office.Interop.Outlook.OlDefaultFolders.olFolderCalendar);

        //        // 3) Získáme kolekci položek a připravíme ji pro filtrování
        //        items = calendarFolder.Items;

        //        // Včetně opakovaných (recurrence) a seřadíme podle startu
        //        items.IncludeRecurrences = true;
        //        items.Sort("[Start]");

        //        // 4) Omezíme rozsah – třeba od teď do +7 dní (Outlook očekává US formát datumu)
        //        var start = DateTime.Now;
        //        var end = DateTime.Now.AddDays(7);

        //        // Pozor na formát dat – Outlook Restrict používá M/d/yyyy h:mm tt (EN-US)
        //        string filter = $"[Start] >= '{start.ToString("g", System.Globalization.CultureInfo.GetCultureInfo("en-US"))}' AND " +
        //                        $"[Start] <= '{end.ToString("g", System.Globalization.CultureInfo.GetCultureInfo("en-US"))}'";

        //        var restricted = items.Restrict(filter);



        //        foreach (object obj in restricted)
        //        {
        //            if (obj is Microsoft.Office.Interop.Outlook.AppointmentItem appt)
        //            {
        //                // Bezpečně přečteme základní info
        //                string subject = string.IsNullOrWhiteSpace(appt.Subject) ? "(bez názvu)" : appt.Subject;
        //                string when = $"{appt.Start:g} – {appt.End:t}";
        //                string where = string.IsNullOrWhiteSpace(appt.Location) ? "" : $" @ {appt.Location}";

        //                // ...
        //            }
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        MessageBox.Show(this, "Chyba při čtení kalendáře z Outlooku:\n" + ex.Message,
        //            "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //    finally
        //    {
        //        // Uvolníme COM objekty – snížíme riziko „visících“ procesů
        //        if (items != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(items);
        //        if (calendarFolder != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(calendarFolder);
        //        if (outlook != null) System.Runtime.InteropServices.Marshal.ReleaseComObject(outlook);
        //    }
        //}
    }
}

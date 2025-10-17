using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VykazyPrace.Core.Database.Models.OutlookEvents;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Core.Logging;
using VykazyPrace.Core.Services;

namespace VykazyPrace.UserControls.Outlook
{
    public partial class OutlookEvent : UserControl
    {
        private readonly int _currentUserId;
        private readonly int _itemId;
        private readonly CalendarRepository _repo;
        private readonly Func<Task>? _onChanged;

        private readonly UserItemStateEnum? _stateForUser;

        /// <param name="currentUserId">ID aktuálního uživatele</param>
        /// <param name="itemId">ID CalendarItem v DB</param>
        /// <param name="dateLocal">Datum (lokální čas) pro horní lištu</param>
        /// <param name="timeFromLocal">Začátek (lokální čas) – může být null (celodenní)</param>
        /// <param name="timeToLocal">Konec (lokální čas) – může být null</param>
        /// <param name="subject">Předmět schůzky</param>
        /// <param name="stateForUser">Aktuální stav pro uživatele (kvůli UI) – může být null</param>
        /// <param name="onChanged">Callback po změně (např. aby rodič znovunačetl seznam)</param>
        public OutlookEvent(
            int currentUserId,
            int itemId,
            DateTime dateLocal,
            DateTime? timeFromLocal,
            DateTime? timeToLocal,
            string subject,
            UserItemStateEnum? stateForUser = null,
            Func<Task>? onChanged = null)
        {
            InitializeComponent();

            _currentUserId = currentUserId;
            _itemId = itemId;
            _onChanged = onChanged;
            _stateForUser = stateForUser;
            _repo = new CalendarRepository();

            // ====== Naplnění UI ======
            // Datum vlevo nahoře
            labelDate.Text = dateLocal.ToString("dd.MM.yyyy");

            // Čas vpravo nahoře
            if (timeFromLocal.HasValue && timeToLocal.HasValue)
            {
                labelTime.Text = $"{timeFromLocal.Value:HH\\:mm} - {timeToLocal.Value:HH\\:mm}";
            }
            else if (timeFromLocal.HasValue)
            {
                labelTime.Text = $"{timeFromLocal.Value:HH\\:mm}";
            }
            else
            {
                // Celodenní nebo bez času
                labelTime.Text = "Celý den";
            }

            // Předmět
            labelSubject.Text = string.IsNullOrWhiteSpace(subject) ? "(bez názvu)" : subject;

            // ====== Přepnutí UI podle aktuálního stavu ======
            // Logika: pokud už je "IgnoreTombstone" → skrýt kartu obvykle nechceš vůbec zobrazit,
            // ale kdyby se zobrazila, oba buttony disablujeme.
            // Pokud je "Zapsano" → disablujeme Přidat.
            switch (_stateForUser)
            {
                case UserItemStateEnum.IgnoreTombstone:
                    buttonAdd.Enabled = false;
                    buttonAdd.Text = "Přidat";
                    buttonDelete.Enabled = false;
                    break;

                case UserItemStateEnum.Written:
                    buttonAdd.Enabled = false; // už je zapsáno
                    buttonAdd.Text = "✓ Přidáno";
                    buttonDelete.Enabled = true;
                    break;

                default:
                    buttonAdd.Enabled = true;
                    buttonAdd.Text = "Přidat";
                    buttonDelete.Enabled = true;
                    break;
            }

            // Události tlačítek

        }

        /// <summary>
        /// Akce pro tlačítko "Přidat" – nastavíme stav pro uživatele na Written.
        /// </summary>
        private async Task OnAddAsync()
        {
            try
            {
                // 1) Založ TimeEntry pro meeting
                var import = new CalendarImportService();

                // Pozn.: nemáme tu celý CalendarItem objekt – máme jen jeho ID (_itemId) a data pro UI.
                // Vyřešíme to tak, že si CalendarItem načteme z DB podle _itemId.
                var itemFromDb = await new CalendarRepository().GetByKeyAsync(
                    storeId: null, entryId: null, occurrenceStartUtc: null
                );
                // ↑ To byla původní helper metoda podle klíče.
                // Máme ale pouze ItemId – tak přidej do CalendarRepository jednoduché:
                // public Task<CalendarItem?> GetByIdAsync(int id) => _context.CalendarItems.FindAsync(id).AsTask();

                // Lepší varianta – přidej do repo:
                // public async Task<CalendarItem?> GetByIdAsync(int id) => await _context.CalendarItems.FindAsync(id);

                var repo = new CalendarRepository();
                var calItem = await repo.GetByIdAsync(_itemId); // ⬅️ přidej do repo jednoduchý getter

                // Fallback (nemělo by nastat)
                if (calItem == null)
                {
                    MessageBox.Show(this, "Nepodařilo se načíst kalendářovou položku z DB.", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 2) Vytvoř TimeEntry (předáme data, která už control má)
                //    - dateLocal, timeFromLocal, timeToLocal, subject jsme dávali v konstruktoru
                var created = await import.CreateTimeEntryForMeetingAsync(
                    userId: _currentUserId,
                    item: calItem,
                    subject: labelSubject.Text,
                    startLocal: TryParseTimeFromLabel(labelDate.Text, labelTime.Text, fromPart: true),
                    endLocal: TryParseTimeFromLabel(labelDate.Text, labelTime.Text, fromPart: false)
                );

                // 3) Nastav stav pro uživatele na Zapsáno
                await _repo.SetUserStateAsync(_currentUserId, _itemId, UserItemStateEnum.Written);

                // 4) Oznámení + refresh
                // if (created != null)
                //     MessageBox.Show(this, "Záznam přidán do časových záznamů.", "Hotovo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                if (_onChanged != null) await _onChanged();
            }
            catch (System.Exception ex)
            {
                AppLogger.Error("Nepodařilo se vytvořit časový záznam z meetingu.", ex);
            }
        }

        /// <summary>
        /// Parsuje z labelů datum a čas. 
        /// Pokud nelze jednoznačně určit (např. „Celý den“), vrací null.
        /// </summary>
        private static DateTime? TryParseTimeFromLabel(string dateText, string timeText, bool fromPart)
        {
            if (!DateTime.TryParseExact(dateText, "dd.MM.yyyy",
                System.Globalization.CultureInfo.GetCultureInfo("cs-CZ"),
                System.Globalization.DateTimeStyles.None, out var date))
                return null;

            if (string.Equals(timeText, "Celý den", StringComparison.OrdinalIgnoreCase))
                return null;

            // formát "HH:mm - HH:mm" nebo jen "HH:mm"
            var parts = timeText.Split('-', StringSplitOptions.TrimEntries);
            if (parts.Length == 2)
            {
                if (fromPart)
                {
                    if (TimeSpan.TryParse(parts[0], out var ts))
                        return date.Date.Add(ts);
                }
                else
                {
                    if (TimeSpan.TryParse(parts[1], out var ts))
                        return date.Date.Add(ts);
                }
            }
            else if (parts.Length == 1 && fromPart)
            {
                if (TimeSpan.TryParse(parts[0], out var ts))
                    return date.Date.Add(ts);
            }

            return null;
        }

        /// <summary>
        /// Akce pro tlačítko "Smazat" – NEMAŽEME z Outlooku ani z CalendarItems!
        /// Nastavíme stav pro uživatele na IgnoreTombstone, aby už se nezobrazovala.
        /// </summary>
        private async Task OnDeleteAsync()
        {
            try
            {
                await _repo.SetUserStateAsync(_currentUserId, _itemId, UserItemStateEnum.IgnoreTombstone);
                // MessageBox.Show("Událost byla skryta pro tohoto uživatele.", "Hotovo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (_onChanged != null) await _onChanged();
            }
            catch (System.Exception ex)
            {
                AppLogger.Error("Nepodařilo se 'Smazat' (tombstone).", ex);
            }
        }

        private async void buttonAdd_Click(object sender, EventArgs e)
        {
            await OnAddAsync();
        }

        private async void buttonDelete_Click(object sender, EventArgs e)
        {
            await OnDeleteAsync();
        }
    }
}

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
                await _repo.SetUserStateAsync(_currentUserId, _itemId, UserItemStateEnum.Written);
                // (volitelně) notifikace uživateli
                // MessageBox.Show("Událost byla přidána do seznamu.", "Hotovo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (_onChanged != null) await _onChanged();
            }
            catch (System.Exception ex)
            {
                AppLogger.Error("Nepodařilo se označit jako 'Přidat'.", ex);
            }
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

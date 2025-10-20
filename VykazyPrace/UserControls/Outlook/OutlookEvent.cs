using VykazyPrace.Core.Database.Models.OutlookEvents;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Core.Logging;
using VykazyPrace.Core.Services;
using Exception = System.Exception;

namespace VykazyPrace.UserControls.Outlook
{
    public partial class OutlookEvent : UserControl
    {
        private readonly int _currentUserId;
        private readonly int _itemId;
        private readonly CalendarRepository _repo;
        private readonly Func<Task>? _onChanged; // callback k obnově UI po změně
        private readonly UserItemStateEnum? _stateForUser; // aktuální per-user stav (kvůli UI)

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

            labelDate.Text = dateLocal.ToString("dd.MM.yyyy");

            if (timeFromLocal.HasValue && timeToLocal.HasValue)
                labelTime.Text = $"{timeFromLocal.Value:HH\\:mm} - {timeToLocal.Value:HH\\:mm}";
            else if (timeFromLocal.HasValue)
                labelTime.Text = $"{timeFromLocal.Value:HH\\:mm}";
            else
                labelTime.Text = "Celý den";

            labelSubject.Text = string.IsNullOrWhiteSpace(subject) ? "(bez názvu)" : subject;

            switch (_stateForUser)
            {
                case UserItemStateEnum.IgnoreTombstone:
                    buttonAdd.Enabled = false;
                    buttonAdd.Text = "Přidat";
                    buttonDelete.Enabled = false;
                    break;

                case UserItemStateEnum.Written:
                    buttonAdd.Enabled = false;
                    buttonAdd.Text = "✓ Přidáno";
                    buttonDelete.Enabled = true;
                    break;

                default:
                    buttonAdd.Enabled = true;
                    buttonAdd.Text = "Přidat";
                    buttonDelete.Enabled = true;
                    break;
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

        private async Task OnAddAsync()
        {
            try
            {
                var service = new OutlookMeetingImportService();

                var r = await service.AddSingleFromUiAsync(_currentUserId, _itemId, labelDate.Text, labelTime.Text, labelSubject.Text);

                if (r.Status is ImportStatus.Added or ImportStatus.SkippedDuplicate)
                {
                    this.Parent?.Controls.Remove(this);
                    this.Dispose();
                    if (_onChanged != null) await _onChanged();
                }
                else if (r.Status == ImportStatus.SkippedConflict)
                {
                    MessageBox.Show(this, "Nelze přidat – v tomto čase už existuje jiný časový záznam.", "Konflikt",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (r.Status == ImportStatus.InvalidInput)
                {
                    MessageBox.Show(this, "Neplatné datum/čas v kartě události.", "Chyba",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(this, "Akci se nepodařilo dokončit.", "Chyba",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("Nepodařilo se vytvořit časový záznam z meetingu.", ex);
            }
        }


        /// <summary>
        /// Klik na Smazat:
        /// - nastaví stav IgnoreTombstone (trvale skrýt)
        /// - skryje kartu a refreshne seznam
        /// </summary>
        private async Task OnDeleteAsync()
        {
            try
            {
                await _repo.SetUserStateAsync(_currentUserId, _itemId, UserItemStateEnum.IgnoreTombstone);

                var parent = this.Parent;
                parent?.Controls.Remove(this);
                this.Dispose();

                if (_onChanged != null) await _onChanged();
            }
            catch (System.Exception ex)
            {
                AppLogger.Error("Smazání se nezdařilo.", ex);
            }
        }
    }
}

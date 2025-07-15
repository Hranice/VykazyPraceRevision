using System.Diagnostics;
using VykazyPrace.Core.Configuration;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Core.Helpers;
using VykazyPrace.Core.Logging;
using VykazyPrace.Dialogs;
using Timer = System.Windows.Forms.Timer;

namespace VykazyPrace.UserControls.CalendarV2
{
    public partial class CalendarV2 : UserControl
    {
        // Constants
        private const int ResizeThreshold = 12;
        private const int TimeSlotLengthInMinutes = 30;
        private const int DefaultProjectType = 0;

        // UI + State
        private readonly Timer _resizeTimer = new() { Interval = 50 };
        private readonly Dictionary<DayPanel, Timer> _uiTimers = new();
        private bool userHasScrolled = false;

        // Repositories
        private readonly TimeEntryRepository _timeEntryRepo;
        private readonly TimeEntryTypeRepository _timeEntryTypeRepo;
        private readonly TimeEntrySubTypeRepository _timeEntrySubTypeRepo;
        private readonly ProjectRepository _projectRepo;
        private readonly SpecialDayRepository _specialDayRepo;
        private readonly ArrivalDepartureRepository _arrivalDepartureRepo;

        // Data cache
        private static List<TimeEntryType>? _cacheTypes;
        private static List<TimeEntrySubType>? _cacheSubTypes;
        private static List<Project>? _cacheProjects;

        private List<Project> _projects = new();
        private List<TimeEntryType> _timeEntryTypes = new();
        private List<TimeEntrySubType> _timeEntrySubTypes = new();
        private List<SpecialDay> _specialDays = new();
        private List<ArrivalDeparture> _arrivalDepartures = new();
        private List<DayPanel> panels = new();
        private List<TimeEntry> _currentEntries = new();


        // Context
        private User _selectedUser;
        private User _loggedUser;
        private DateTime _selectedDate;
        private int _selectedTimeEntryId = -1;
        private int _currentProjectType;

        // Drag & drop
        private DayPanel? activePanel = null;
        private DayPanel? lastPanel = null;
        private bool isResizing = false;
        private bool isMoving = false;
        private bool isResizingLeft = false;
        private int startMouseX;
        private int originalColumn;
        private int originalColumnSpan;

        // Copy & paste
        private TimeEntry? copiedEntry;
        private TableLayoutPanelCellPosition? pasteTargetCell;
        private ToolTip copyToolTip = new();

        // Right click context
        private ContextMenuStrip dayPanelMenu;
        private ContextMenuStrip tableLayoutMenu;

        // Configuration
        private AppConfig _config;


        public CalendarV2(User currentUser,
                          TimeEntryRepository timeEntryRepo,
                          TimeEntryTypeRepository timeEntryTypeRepo,
                          TimeEntrySubTypeRepository timeEntrySubTypeRepo,
                          ProjectRepository projectRepo,
                          UserRepository userRepo,
                          SpecialDayRepository specialDayRepo,
                          ArrivalDepartureRepository arrivalDepartureRepo)
        {
            InitializeComponent();
            DoubleBuffered = true;

            _selectedDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
            _selectedUser = currentUser;
            _loggedUser = currentUser;

            _timeEntryRepo = timeEntryRepo;
            _timeEntryTypeRepo = timeEntryTypeRepo;
            _timeEntrySubTypeRepo = timeEntrySubTypeRepo;
            _projectRepo = projectRepo;
            _specialDayRepo = specialDayRepo;
            _arrivalDepartureRepo = arrivalDepartureRepo;

            _resizeTimer.Tick += async (_, _) =>
            {
                _resizeTimer.Stop();
                await AdjustIndicatorsAsync(panelContainer.AutoScrollPosition, _selectedUser.Id, _selectedDate);
            };

            _specialDayRepo = specialDayRepo;

            _config = ConfigService.Load();
        }

        private void InitializeContextMenus()
        {
            dayPanelMenu = new ContextMenuStrip();
            dayPanelMenu.Items.Add("Kopírovat", null, (_, _) => CopySelectedPanel());
            dayPanelMenu.Items.Add("Odstranit", null, async (_, _) => await DeleteRecord());

            tableLayoutMenu = new ContextMenuStrip();
            tableLayoutMenu.Items.Add("Vložit", null, (_, _) => PasteCopiedPanel());
        }


        public async Task ForceReloadAsync()
        {
            await LoadInitialDataAsync();
            await LoadSidebar();
        }

        public async Task ForceReloadIndicators()
        {
            await AdjustIndicatorsAsync(panelContainer.AutoScrollPosition, _selectedUser.Id, _selectedDate);
        }

        private void SafeInvoke(Action action)
        {
            if (InvokeRequired) Invoke(action);
            else action();
        }

        private void CalendarV2_Resize(object sender, EventArgs e)
        {
            _resizeTimer.Stop();
            _resizeTimer.Start();
        }

        private void CalendarV2_Load(object sender, EventArgs e)
        {
            InitializeContextMenus();

            panelContainer.Scroll += PanelContainer_Scroll;
            _ = LoadInitialDataAsync();
        }

        private void PanelContainer_Scroll(object? sender, ScrollEventArgs e)
        {
            userHasScrolled = true;
        }

        private async Task LoadInitialDataAsync()
        {
            await LoadReferenceDataAsync();

            SafeInvoke(() =>
            {
                customComboBoxProjects.SetItems(_projects
                    .Select(FormatHelper.FormatProjectToString)
                    .ToArray());

                UpdateEntryTypeControls(_currentProjectType);

                customComboBoxSubTypes.SetItems(_timeEntrySubTypes
                    .Where(t => t.IsArchived == 0)
                    .Select(FormatHelper.FormatTimeEntrySubTypeToString)
                    .ToArray());
            });

            var specialTask = LoadSpecialDaysAsync();
            var arrivalTask = LoadArrivalDeparturesAsync();

            await Task.WhenAll(specialTask, arrivalTask);

            await RenderCalendar();
            await AdjustIndicatorsAsync(panelContainer.AutoScrollPosition, _selectedUser.Id, _selectedDate);
        }



        private async Task LoadReferenceDataAsync()
        {
            if (_cacheTypes == null)
                _cacheTypes = await _timeEntryTypeRepo.GetAllTimeEntryTypesByProjectTypeAsync(DefaultProjectType);
            _timeEntryTypes = _cacheTypes;

            if (_cacheSubTypes == null)
                _cacheSubTypes = await _timeEntrySubTypeRepo.GetAllTimeEntrySubTypesByUserIdAsync(_selectedUser.Id);
            _timeEntrySubTypes = _cacheSubTypes;

            if (_cacheProjects == null)
                _cacheProjects = DefaultProjectType == 1
                    ? await _projectRepo.GetAllFullProjectsAndPreProjectsAsync(checkBoxArchivedProjects.Checked)
                    : await _projectRepo.GetAllProjectsAsyncByProjectType(DefaultProjectType);
            _projects = _cacheProjects;
        }


        private async Task LoadArrivalDeparturesAsync()
        {
            try
            {
                _arrivalDepartures = await _arrivalDepartureRepo.GetWeekEntriesForUserAsync(_selectedUser.Id, _selectedDate);
            }
            catch (Exception ex)
            {
                SafeInvoke(() => AppLogger.Error("Chyba při načítání speciálních dnů.", ex));
            }
        }

        private async Task LoadSpecialDaysAsync()
        {
            try
            {
                _specialDays = await _specialDayRepo.GetSpecialDaysForWeekAsync(_selectedDate);
            }
            catch (Exception ex)
            {
                SafeInvoke(() => AppLogger.Error("Chyba při načítání speciálních dnů.", ex));
            }
        }

        public async Task ChangeUser(User newUser)
        {
            _selectedUser = newUser;

            // 1) paralelně spusť načtení docházky a vykreslení kalendáře
            var arrivalTask = LoadArrivalDeparturesAsync();
            var renderTask = RenderCalendar();

            // 2) počkej, až oba dokončí (docházku i vykreslení)
            await Task.WhenAll(arrivalTask, renderTask);

            // 3) až po dokončení načtení docházky i vykreslení kalendáře spusť indikátory
            await AdjustIndicatorsAsync(panelContainer.AutoScrollPosition, _selectedUser.Id, _selectedDate);

            // 4) úklid UI
            DeactivateAllPanels();
            _selectedTimeEntryId = -1;

            // 5) sidebar může jít asynchronně na pozadí
            _ = LoadSidebar();
        }



        internal async Task<DateTime> ChangeToPreviousWeek()
        {
            _selectedDate = _selectedDate.AddDays(-7);
            await LoadArrivalDeparturesAsync();
            await RenderCalendar();
            await AdjustIndicatorsAsync(panelContainer.AutoScrollPosition, _selectedUser.Id, _selectedDate);
            this.Focus();
            return _selectedDate;
        }

        internal async Task<DateTime> ChangeToNextWeek()
        {
            _selectedDate = _selectedDate.AddDays(7);
            await LoadArrivalDeparturesAsync();
            await RenderCalendar();
            await AdjustIndicatorsAsync(panelContainer.AutoScrollPosition, _selectedUser.Id, _selectedDate);
            this.Focus();
            return _selectedDate;
        }

        internal async Task<DateTime> ChangeToTodaysWeek()
        {
            DateTime today = DateTime.Today;
            int offset = ((int)today.DayOfWeek + 6) % 7;
            _selectedDate = today.AddDays(-offset);
            await LoadArrivalDeparturesAsync();
            await RenderCalendar();
            await AdjustIndicatorsAsync(panelContainer.AutoScrollPosition, _selectedUser.Id, _selectedDate);
            this.Focus();
            return _selectedDate;
        }

        private async Task LoadTimeEntryTypesAsync(int projectType)
        {
            try
            {
                _currentProjectType = projectType;

                var entry = _currentEntries.FirstOrDefault(e => e.Id == _selectedTimeEntryId);
                bool isArchived = entry?.AfterCare == 1;

                _timeEntryTypes = await _timeEntryTypeRepo.GetAllTimeEntryTypesByProjectTypeAsync(projectType);

                SafeInvoke(() => UpdateEntryTypeControls(projectType));
            }
            catch (Exception ex)
            {
                SafeInvoke(() => AppLogger.Error("Chyba při načítání typů časových záznamů.", ex));
            }
        }

        private void UpdateEntryTypeControls(int projectType)
        {
            bool useRadioButtons = projectType is 0 or 1 or 2;

            comboBoxEntryType.Visible = !useRadioButtons;
            comboBoxEntryType.Enabled = !useRadioButtons;
            comboBoxEntryType.Items.Clear();

            ClearPreviousEntryTypeControls();

            if (useRadioButtons)
            {
                AddRadioButtonsForEntryTypes();
            }
            else
            {
                FillComboBoxWithEntryTypes();
            }
        }

        private void AddRadioButtonsForEntryTypes()
        {
            var layout = CreateRadioButtonLayout();

            for (int i = 0; i < _timeEntryTypes.Count; i++)
            {
                var entryType = _timeEntryTypes[i];
                var radio = CreateRadioButton(entryType.Title);

                if (i == 0)
                {
                    radio.Checked = true;
                }

                layout.Controls.Add(radio);
            }

            tableLayoutPanelEntryType.Controls.Add(layout);
        }


        private TableLayoutPanel CreateRadioButtonLayout()
        {
            var panel = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 1,
                Dock = DockStyle.Fill,
                Height = comboBoxEntryType.Height,
                Padding = Padding.Empty,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = Padding.Empty
            };

            for (int i = 0; i < panel.ColumnCount; i++)
            {
                panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            }

            return panel;
        }

        private RadioButton CreateRadioButton(string text)
        {
            return new RadioButton
            {
                Text = text,
                TextAlign = ContentAlignment.MiddleCenter,
                Appearance = Appearance.Button,
                Dock = DockStyle.Fill,
                AutoSize = true,
                Font = new Font(this.Font.FontFamily, 9.0f)
            };
        }

        private void FillComboBoxWithEntryTypes()
        {
            var items = _timeEntryTypes.Select(type =>
                checkBoxArchivedProjects.Checked
                    ? FormatHelper.FormatTimeEntryTypeWithAfterCareToString(type)
                    : FormatHelper.FormatTimeEntryTypeToString(type));

            comboBoxEntryType.Items.AddRange(items.ToArray());
            comboBoxEntryType.Text = string.Empty;
        }

        private void ClearPreviousEntryTypeControls()
        {
            var tablePanels = tableLayoutPanelEntryType.Controls
                .OfType<TableLayoutPanel>()
                .ToList();

            foreach (var ctrl in tablePanels)
            {
                tableLayoutPanelEntryType.Controls.Remove(ctrl);
                ctrl.Dispose();
            }
        }

        private async Task LoadTimeEntrySubTypesAsync()
        {
            try
            {
                _timeEntrySubTypes = await _timeEntrySubTypeRepo.GetAllTimeEntrySubTypesByUserIdAsync(_selectedUser.Id);

                SafeInvoke(() =>
                {
                    customComboBoxSubTypes.SetItems(_timeEntrySubTypes
                                .Where(t => t.IsArchived == 0)
                                .Select(FormatHelper.FormatTimeEntrySubTypeToString)
                                .ToArray());
                });
            }
            catch (Exception ex)
            {
                SafeInvoke(() => AppLogger.Error("Chyba při načítání sub-typů (indexů) časových záznamů.", ex));
            }
        }

        private async Task LoadProjectsAsync(int projectType)
        {
            try
            {
                bool includeArchived = checkBoxArchivedProjects.Checked;

                if (projectType == 1)
                {
                    _projects = await _projectRepo.GetAllFullProjectsAndPreProjectsAsync(checkBoxArchivedProjects.Checked);
                }
                else
                {
                    _projects = await _projectRepo.GetAllProjectsAsyncByProjectType(projectType);
                }

                SafeInvoke(() =>
                {
                    customComboBoxProjects.SetItems(_projects
                            .Select(FormatHelper.FormatProjectToString)
                            .ToArray());
                });
            }
            catch (Exception ex)
            {
                SafeInvoke(() => AppLogger.Error("Chyba při načítání projektů.", ex));
            }
        }

        private async Task LoadSidebar()
        {
            string[] days = { "Neděle", "Pondělí", "Úterý", "Středa", "Čtvrtek", "Pátek", "Sobota" };
            flowLayoutPanel2.Visible = _selectedTimeEntryId > -1;

            var timeEntry = _currentEntries.FirstOrDefault(e => e.Id == _selectedTimeEntryId);
            if (timeEntry == null) return;

            // pokud je svačina, schovej sidebar
            if (timeEntry.ProjectId == 132 && timeEntry.EntryTypeId == 24)
            {
                flowLayoutPanel2.Visible = false;
                return;
            }

            DateTime timeStamp = timeEntry.Timestamp ?? _selectedDate;
            int minutesStart = timeStamp.Hour * 60 + timeStamp.Minute;
            int minutesEnd = minutesStart + timeEntry.EntryMinutes;
            flowLayoutPanel2.Enabled = timeEntry.IsLocked == 0 && timeEntry.UserId == _loggedUser.Id;

            if (timeEntry.IsValid != 1)
            {
                // NEvalidní záznam – jen základní ovládací prvky
                BeginInvoke((Action)(() =>
                {
                    comboBoxStart.SelectedIndex = minutesStart / 30;
                    comboBoxEnd.SelectedIndex = Math.Min(minutesEnd / 30, comboBoxEnd.Items.Count - 1);
                    customComboBoxSubTypes.SetText(string.Empty);
                    textBoxNote.Text = string.Empty;
                    comboBoxEntryType.Text = string.Empty;

                    foreach (var radio in flowLayoutPanel2.Controls.OfType<RadioButton>())
                        radio.Checked = false;

                    tableLayoutPanel4.Visible = false;
                    tableLayoutPanel6.Visible = false;
                    tableLayoutPanelProject.Visible = false;
                    tableLayoutPanelEntryType.Visible = false;
                    tableLayoutPanelEntrySubType.Visible = false;
                    panel4.Visible = false;
                }));
                return;
            }

            // ---- tady už je validní záznam ----

            // 1) Dotáhni projekt, pokud ještě nemáš navigaci
            Project proj = timeEntry.Project
                           ?? await _projectRepo.GetProjectByIdAsync(timeEntry.ProjectId ?? 0);
            if (proj == null) return;
            timeEntry.Project = proj;  // ulož si to, ať to můžeš dál používat

            // 2) Checkbox archivace
            checkBoxArchivedProjects.Checked = proj.IsArchived == 1;

            // 3) Načti typy podle projectType
            await LoadTimeEntryTypesAsync(proj.ProjectType);

            // 4) Speciální projekty – vyber radio button
            switch (proj.Id)
            {
                case 25: SelectRadioButtonByText("OSTATNÍ"); break;
                case 23: SelectRadioButtonByText("NEPŘÍTOMNOST"); break;
                case 26: SelectRadioButtonByText("ŠKOLENÍ"); break;
                default:
                    int idx = proj.ProjectType + 1;
                    if (idx == 2 || idx == 3) idx = 2;
                    if (flowLayoutPanel2.Controls.Find($"radioButton{idx}", false).FirstOrDefault() is RadioButton rb)
                        rb.Checked = true;
                    break;
            }

            // 5) Naplň Comboboxy / poznámku / čas začátku a konce
            BeginInvoke((Action)(() =>
            {
                comboBoxStart.SelectedIndex = minutesStart / 30;
                comboBoxEnd.SelectedIndex = Math.Min(minutesEnd / 30, comboBoxEnd.Items.Count - 1);

                customComboBoxSubTypes.SetText(timeEntry.Description);
                customComboBoxProjects.SetText(FormatHelper.FormatProjectToString(proj));
                textBoxNote.Text = timeEntry.Note;

                // výběr EntryType: radio vs combobox
                if (proj.ProjectType is 0 or 1 or 2)
                {
                    int baseId = proj.ProjectType switch
                    {
                        0 => 1,
                        1 => 10,
                        2 => 13,
                        _ => 0
                    };
                    int radioIndex = (int)(timeEntry.EntryTypeId - baseId);
                    var radioPanel = tableLayoutPanelEntryType
                                     .Controls
                                     .OfType<TableLayoutPanel>()
                                     .FirstOrDefault();
                    var radios = radioPanel?.Controls.OfType<RadioButton>().ToList();
                    if (radios != null && radioIndex >= 0 && radioIndex < radios.Count)
                        radios[radioIndex].Checked = true;
                }
                else
                {
                    var selectedType = _timeEntryTypes.FirstOrDefault(x => x.Id == timeEntry.EntryTypeId);
                    comboBoxEntryType.Text = timeEntry.AfterCare == 1
                        ? FormatHelper.FormatTimeEntryTypeWithAfterCareToString(selectedType)
                        : FormatHelper.FormatTimeEntryTypeToString(selectedType);
                }
            }));
        }


        private void SelectRadioButtonByText(string text)
        {
            var rb = flowLayoutPanel2.Controls
                .OfType<RadioButton>()
                .FirstOrDefault(r => r.Text.Equals(text, StringComparison.InvariantCultureIgnoreCase));

            if (rb != null) rb.Checked = true;
        }

        private TableLayoutPanelCellPosition GetCellAt(TableLayoutPanel panel, Point clickPosition)
        {
            int width = panel.Width / panel.ColumnCount;
            int height = panel.Height / panel.RowCount;

            int col = Math.Min(clickPosition.X / width, panel.ColumnCount - 1);
            int row = Math.Min(clickPosition.Y / height, panel.RowCount - 1);

            return new TableLayoutPanelCellPosition(col, row);
        }

        private void tableLayoutPanel1_MouseClick(object sender, MouseEventArgs e)
        {
            var cell = GetCellAt(tableLayoutPanelCalendar, e.Location);
            pasteTargetCell = cell;
            DeactivateAllPanels();
            _selectedTimeEntryId = -1;
            _ = LoadSidebar();
        }

        private void tableLayoutPanel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                pasteTargetCell = GetCellAt(tableLayoutPanelCalendar, e.Location);

                if (tableLayoutMenu.Items.Count > 0)
                {
                    var pasteItem = tableLayoutMenu.Items[0];
                    pasteItem.Enabled = copiedEntry != null;
                }

                tableLayoutMenu.Show(tableLayoutPanelCalendar, e.Location);
            }
        }

        /// <summary>
        /// Zpracuje kolize v řádku před vložením záznamu:
        /// – Cancel = nic,
        /// – No = posun (ShiftRightFrom),
        /// – Yes = nahradit (RemoveOverlappingPanels), ale ne svačinu.
        /// </summary>
        /// <returns>
        /// false = zrušeno nebo nelze (svačina), true = můžeš vložit.
        /// </returns>
        private async Task<bool> HandleOverlapAsync(int column, int row, int span)
        {
            // 1) Najdi všechny překrývající se panely ve stejném řádku
            var overlaps = tableLayoutPanelCalendar.Controls
                .OfType<DayPanel>()
                .Where(p => tableLayoutPanelCalendar.GetRow(p) == row)
                .Where(p =>
                {
                    int c = tableLayoutPanelCalendar.GetColumn(p);
                    int s = tableLayoutPanelCalendar.GetColumnSpan(p);
                    return !(column + span - 1 < c || column > c + s - 1);
                })
                .ToList();

            // 2) Pokud žádné, můžeš rovnou vložit
            if (!overlaps.Any())
                return true;

            // 3) Dialog Replace/Move/Cancel
            var result = new ReplaceOrMoveDialog().ShowDialog();
            if (result == DialogResult.Cancel)
                return false;

            if (result == DialogResult.No)
            {
                // Posun ostatní doprava (DB + UI)
                return await ShiftRightFrom(column, row, span);
            }
            else // DialogResult.Yes = Replace
            {
                // Nejprve zkontroluj, jestli mezi overlaps není svačina
                bool hasSnack = overlaps.Any(p => p.Tag as string == "snack");
                if (hasSnack)
                {
                    MessageBox.Show(
                        "Nelze nahradit záznam svačiny.",
                        "Chyba",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return false;
                }

                // Smaž kolizní položky (DB + kolekce + UI)
                await RemoveOverlappingPanels(column, span, row);
                return true;
            }
        }


        /// <summary>
        /// Posune v řádku <paramref name="row"/> pouze ty panely, které by se překrývaly
        /// s vložením o šířce <paramref name="requiredSpan"/> od sloupce <paramref name="fromCol"/>,
        /// a poté kaskádovitě i další, pokud by to bylo potřeba.
        /// </summary>
        /// <returns>true, pokud posun proběhl; false, pokud by došlo k přetečení dne.</returns>
        private async Task<bool> ShiftRightFrom(int fromCol, int row, int requiredSpan)
        {
            // 1) Seřazení všech panelů v tom řádku podle startu
            var panelsInRow = tableLayoutPanelCalendar.Controls
                .OfType<DayPanel>()
                .Where(p => tableLayoutPanelCalendar.GetRow(p) == row)
                .Select(p => new
                {
                    Panel = p,
                    Start = tableLayoutPanelCalendar.GetColumn(p),
                    Span = tableLayoutPanelCalendar.GetColumnSpan(p)
                })
                .OrderBy(x => x.Start)
                .ToList();

            int layoutWidth = tableLayoutPanelCalendar.ColumnCount;
            int cursor = fromCol + requiredSpan;
            var updateTasks = new List<Task>();

            foreach (var item in panelsInRow)
            {
                int origStart = item.Start;
                int span = item.Span;
                int origEnd = origStart + span;

                // 2) Pokud panel končí před vložením, nezasahuje a přeskočí se
                if (origEnd <= fromCol)
                    continue;

                // 3) Zjistíme, zda ho musíme posunout:
                //    posuneme ho jen, pokud by začínal před nebo v cursoru
                if (origStart < cursor)
                {
                    // kontrola přetečení
                    if (cursor + span > layoutWidth)
                    {
                        AppLogger.Error("Posun není možný, došlo by k přetečení dne.");
                        return false;
                    }

                    // --- UI: změň pozici panelu ---
                    tableLayoutPanelCalendar.SuspendLayout();
                    tableLayoutPanelCalendar.SetColumn(item.Panel, cursor);
                    tableLayoutPanelCalendar.ResumeLayout();

                    // --- DB: najdi záznam a aktualizuj Timestamp ---
                    var entry = _currentEntries.FirstOrDefault(e => e.Id == item.Panel.EntryId);
                    if (entry != null)
                    {
                        entry.Timestamp = _selectedDate
                            .AddDays(row)
                            .AddMinutes(cursor * TimeSlotLengthInMinutes);
                        updateTasks.Add(_timeEntryRepo.UpdateTimeEntryAsync(entry));
                    }

                    // posuneme kurzor za tento panel
                    cursor += span;
                }
                else
                {
                    // pokud začíná za cursor, netkneme ho – ale posuneme kurzor na jeho konec
                    cursor = origStart + span;
                }
            }

            // 4) Ulož změny do DB
            if (updateTasks.Any())
                await Task.WhenAll(updateTasks);

            return true;
        }


        /// <summary>
        /// Smaže všechny kolidující panely od fromCol v řádku row (DB + _currentEntries + UI).
        /// </summary>
        private async Task RemoveOverlappingPanels(int fromCol, int span, int row)
        {
            var toRemove = tableLayoutPanelCalendar.Controls
                .OfType<DayPanel>()
                .Where(p => tableLayoutPanelCalendar.GetRow(p) == row)
                .Where(p =>
                {
                    int c = tableLayoutPanelCalendar.GetColumn(p);
                    int s = tableLayoutPanelCalendar.GetColumnSpan(p);
                    return !(fromCol + span - 1 < c || fromCol > c + s - 1);
                })
                .ToList();

            // 1) DB delete
            var deletes = toRemove.Select(p => _timeEntryRepo.DeleteTimeEntryAsync(p.EntryId)).ToList();
            await Task.WhenAll(deletes);

            // 2) lokální kolekce + UI
            foreach (var panel in toRemove)
            {
                _currentEntries.RemoveAll(e => e.Id == panel.EntryId);
                RemoveEntryPanel(panel.EntryId);
            }
        }

        /// <summary>
        /// Vloží (Ctrl+V) zkopírovaný záznam s kompletním ošetřením kolizí.
        /// </summary>
        private async void PasteCopiedPanel()
        {
            if (copiedEntry == null || pasteTargetCell == null) return;
            if (_selectedUser.Id != _loggedUser.Id) return;

            int column = pasteTargetCell.Value.Column;
            int row = pasteTargetCell.Value.Row;
            int span = copiedEntry.EntryMinutes / TimeSlotLengthInMinutes;
            if (column + span > tableLayoutPanelCalendar.ColumnCount) return;

            // 1) ošetři Replace/Move
            if (!await HandleOverlapAsync(column, row, span))
                return;

            // 2) sestav a vlož nový
            DateTime ts = _selectedDate.AddDays(row)
                                       .AddMinutes(column * TimeSlotLengthInMinutes);
            if (_specialDays.Any(d => d.Date.Date == ts.Date && d.Locked)) return;

            var newEntry = new TimeEntry
            {
                EntryTypeId = copiedEntry.EntryTypeId,
                ProjectId = copiedEntry.ProjectId,
                Description = copiedEntry.Description,
                Note = copiedEntry.Note,
                EntryMinutes = copiedEntry.EntryMinutes,
                AfterCare = copiedEntry.AfterCare,
                UserId = _selectedUser.Id,
                Timestamp = ts,
                IsValid = copiedEntry.IsValid,
                IsLocked = 0
            };

            await OnNewEntryCreated(newEntry);
            await LoadSidebar();
        }

        /// <summary>
        /// Dvojklikem vloží nový záznam mezi stávající, s Replace/Move dialogem.
        /// </summary>
        private async void TableLayoutPanel1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (_selectedUser.Id != _loggedUser.Id) return;

            var cell = GetCellAt(tableLayoutPanelCalendar, e.Location);
            if (_projects.Count == 0 || _timeEntryTypes.Count == 0) return;

            int column = cell.Column;
            int row = cell.Row;
            int span = 1;

            // Najdi volné místo, ale pak ošetři kolizi
            while (column + span <= tableLayoutPanelCalendar.ColumnCount)
            {
                bool ov = tableLayoutPanelCalendar.Controls
                           .OfType<DayPanel>()
                           .Any(p =>
                           {
                               int r = tableLayoutPanelCalendar.GetRow(p);
                               if (r != row) return false;
                               int c = tableLayoutPanelCalendar.GetColumn(p);
                               int s = tableLayoutPanelCalendar.GetColumnSpan(p);
                               return !(column + span - 1 < c || column > c + s - 1);
                           });
                if (!ov) break;
                column++;
            }
            if (column + span > tableLayoutPanelCalendar.ColumnCount) return;

            if (!await HandleOverlapAsync(column, row, span))
                return;

            DateTime ts = _selectedDate.AddDays(row)
                                       .AddMinutes(column * TimeSlotLengthInMinutes);
            if (_specialDays.Any(d => d.Date.Date == ts.Date && d.Locked)) return;

            int idx = customComboBoxProjects.SelectedIndex >= 0
                ? customComboBoxProjects.SelectedIndex
                : 0;
            int projId = _projects[idx].Id;

            var newEntry = new TimeEntry
            {
                ProjectId = projId,
                EntryTypeId = _timeEntryTypes[0].Id,
                UserId = _selectedUser.Id,
                Timestamp = ts,
                EntryMinutes = 30,
                AfterCare = _projects.First(p => p.Id == projId).IsArchived,
                IsLocked = 0
            };

            await OnNewEntryCreated(newEntry);
            await LoadSidebar();
        }


        private void UpdateHourLabels()
        {
            Label[] hourLabels = { labelHours01, labelHours02, labelHours03, labelHours04, labelHours05, labelHours06, labelHours07 };

            for (int row = 0; row < 7; row++)
            {
                DateTime day = _selectedDate.AddDays(row);

                int totalMinutes = _currentEntries
                    .Where(entry =>
                        entry.Timestamp.HasValue &&
                        entry.Timestamp.Value.Date == day.Date &&
                        entry.IsValid == 1 &&
                        !(entry.ProjectId == 132 && entry.EntryTypeId == 24) && // není svačina
                        !(entry.ProjectId == 23)) // není nepřítomnost
                    .Sum(entry => entry.EntryMinutes);

                double vykazanoHodin = totalMinutes / 60.0;

                var dochazka = _arrivalDepartures.FirstOrDefault(a => a.WorkDate.Date == day.Date);
                double hoursWorked = 0;

                if (dochazka != null)
                {
                    hoursWorked = dochazka.HoursWorked;
                }

                switch (_config.PanelDayView)
                {
                    case PanelDayView.Default:
                        hourLabels[row].Text = $"{vykazanoHodin:F1}";
                        hourLabels[row].ForeColor = Color.Black;
                        break;
                    case PanelDayView.Range:
                        hourLabels[row].Text = $"{vykazanoHodin:F1} / {hoursWorked:F1} h";
                        hourLabels[row].ForeColor = Color.Black;
                        break;
                    case PanelDayView.ColorWithinRange:
                        hourLabels[row].Text = $"{vykazanoHodin:F1}";

                        if (Math.Abs(vykazanoHodin - hoursWorked) < 0.01)
                            hourLabels[row].ForeColor = Color.Green;
                        else
                            hourLabels[row].ForeColor = Color.Red;
                        break;
                    case PanelDayView.ColorOvertime:
                        hourLabels[row].Text = $"{vykazanoHodin:F1}";

                        if (vykazanoHodin == 7.5)
                            hourLabels[row].ForeColor = Color.Green;
                        else if (vykazanoHodin > 7.5)
                            hourLabels[row].ForeColor = Color.Blue;
                        else
                            hourLabels[row].ForeColor = Color.Red;
                        break;
                }

                if (dochazka == null)
                {
                    hourLabels[row].ForeColor = Color.Black;
                }
            }
        }

        private int GetColumnBasedOnTimeEntry(DateTime? timeStamp)
        {
            var minutes = timeStamp.Value.Hour * 60 + timeStamp.Value.Minute;
            return minutes / TimeSlotLengthInMinutes;
        }

        private int GetColumnSpanBasedOnTimeEntry(int entryMinutes)
        {
            return Math.Max(1, entryMinutes / TimeSlotLengthInMinutes);
        }

        private int GetRowBasedOnTimeEntry(DateTime? timeStamp)
        {
            return ((int)timeStamp.Value.DayOfWeek + 6) % 7;
        }

        private void UpdateDateLabels()
        {
            Color special = Color.FromArgb(255, 98, 92);
            Color regular = Color.FromArgb(0, 0, 0);

            Label[] dateLabels = { labelDate01, labelDate02, labelDate03, labelDate04, labelDate05, labelDate06, labelDate07 };
            Label[] dayLabels = { labelDay01, labelDay02, labelDay03, labelDay04, labelDay05, labelDay06, labelDay07 };

            for (int i = 0; i < 7; i++)
            {
                DateTime date = _selectedDate.AddDays(i);
                bool isSpecial = _specialDays.Any(x => x.Date.Date == date);

                dateLabels[i].Text = date.ToString("d.M.yyyy");
                dateLabels[i].ForeColor = isSpecial ? special : regular;
                dayLabels[i].ForeColor = isSpecial ? special : regular;
            }
        }


        private async Task AdjustIndicatorsAsync(Point scrollPosition, int userId, DateTime weekStart)
        {
            var oldIndicators = panelContainer.Controls
                .OfType<Panel>()
                .Where(p => p.Name == "indicator")
                .ToList();
            foreach (var ctrl in oldIndicators)
            {
                panelContainer.Controls.Remove(ctrl);
                ctrl.Dispose();
            }

            var entries = await _arrivalDepartureRepo
                .GetWeekEntriesForUserAsync(userId, weekStart);

            int[] rowHeights = tableLayoutPanelCalendar.GetRowHeights();
            int[] columnWidths = tableLayoutPanelCalendar.GetColumnWidths();
            int[] headerRowHeights = customTableLayoutPanel1.GetRowHeights();
            const int minutesPerColumn = 30;
            var toolTip = new ToolTip();

            foreach (var e in entries)
            {
                if (!e.ArrivalTimestamp.HasValue || !e.DepartureTimestamp.HasValue)
                    continue;

                TimeSpan rawArrival = e.ArrivalTimestamp.Value.TimeOfDay;
                TimeSpan rawDeparture = e.DepartureTimestamp.Value.TimeOfDay;

                (TimeSpan roundedArrival, TimeSpan roundedDeparture)
                    = RoundWorkTimeToNearestHalfHour(rawArrival, rawDeparture);

                // Přepočet na sloupcové indexy a pozice
                int arrivalCol = GetColumnIndexFromTime(roundedArrival, minutesPerColumn);
                int leaveCol = GetColumnIndexFromTime(roundedDeparture, minutesPerColumn);
                int dayIndex = ((int)e.WorkDate.DayOfWeek - 1 + 7) % 7;
                int rowHeight = dayIndex < rowHeights.Length ? rowHeights[dayIndex] : 69;
                int yPos = rowHeights.Take(dayIndex).Sum() + headerRowHeights[0];
                int arrivalX = columnWidths[0] * arrivalCol - Math.Abs(scrollPosition.X);
                int leaveX = columnWidths[0] * leaveCol - Math.Abs(scrollPosition.X);

                var arrivalIndicator = new Panel
                {
                    Name = "indicator",
                    Size = new Size(2, rowHeight),
                    Location = new Point(arrivalX, yPos),
                    BackColor = Color.Green
                };
                toolTip.SetToolTip(arrivalIndicator, $"{rawArrival:hh\\:mm} – {rawDeparture:hh\\:mm}");

                var leaveIndicator = new Panel
                {
                    Name = "indicator",
                    Size = new Size(2, rowHeight),
                    Location = new Point(leaveX, yPos),
                    BackColor = Color.Red
                };
                toolTip.SetToolTip(leaveIndicator, $"{rawArrival:hh\\:mm} – {rawDeparture:hh\\:mm}");

                panelContainer.Controls.Add(arrivalIndicator);
                panelContainer.Controls.Add(leaveIndicator);
                arrivalIndicator.BringToFront();
                leaveIndicator.BringToFront();
            }
        }

        /// <summary>
        /// Zaokrouhlí příchod na nejbližší půlhodinu a k odpracované době přičte 5 minut, 
        /// pak tuto dobu zaokrouhlí dolů na půlhodinu.
        /// </summary>
        private (TimeSpan roundedArrival, TimeSpan roundedDeparture)
            RoundWorkTimeToNearestHalfHour(TimeSpan rawArrival, TimeSpan rawDeparture)
        {
            var roundedArrival = TimeSpan.FromMinutes(
                Math.Round(rawArrival.TotalMinutes / 30.0, 0, MidpointRounding.AwayFromZero)
                * 30
            );

            var realDuration = rawDeparture - rawArrival;

            double durWithOffset = realDuration.TotalMinutes + 5;
            double roundedDurMin = Math.Floor(durWithOffset / 30.0) * 30;
            var roundedDeparture = roundedArrival + TimeSpan.FromMinutes(roundedDurMin);

            return (roundedArrival, roundedDeparture);
        }

        private int GetColumnIndexFromTime(TimeSpan timeOfDay, int minutesPerColumn)
        {
            return (int)(timeOfDay.TotalMinutes / minutesPerColumn);
        }


        #region DayPanels
        private readonly Queue<DayPanel> _panelPool = new();
        private readonly List<DayPanel> _activePanels = new();

        private Dictionary<int, Color> _colorCache = new Dictionary<int, Color>();

        private readonly ToolTip _sharedTooltip = new ToolTip()
        {
            AutoPopDelay = 5000,
            InitialDelay = 300,
            ReshowDelay = 100,
            ShowAlways = true
        };

        // Metoda pro získání panelu (pool first):
        private DayPanel GetPooledPanel()
        {
            DayPanel panel;
            if (_panelPool.Count > 0)
            {
                panel = _panelPool.Dequeue();
                panel.Visible = true;
            }
            else
            {
                panel = new DayPanel
                {
                    Dock = DockStyle.Fill,
                    BorderStyle = BorderStyle.FixedSingle
                };
                // jednorázová registrace handlerů
                panel.MouseMove += dayPanel_MouseMove;
                panel.MouseDown += dayPanel_MouseDown;
                panel.MouseUp += dayPanel_MouseUp;
                panel.MouseLeave += dayPanel_MouseLeave;
                panel.MouseClick += dayPanel_MouseClick;
                panel.ContextMenuStrip = dayPanelMenu;
            }
            return panel;
        }

        // Na konci RenderCalendar, vraťte přebytečné:
        private void ReleaseUnusedPanels()
        {
            foreach (var panel in _activePanels)
            {
                // zastavíme a zrušíme timer pro tento panel, pokud existuje
                if (_uiTimers.TryGetValue(panel, out var t))
                {
                    t.Stop();
                    t.Dispose();
                    _uiTimers.Remove(panel);
                }

                panel.Visible = false;
                tableLayoutPanelCalendar.Controls.Remove(panel);
                _panelPool.Enqueue(panel);
            }
            _activePanels.Clear();
        }

        private async Task RenderCalendar()
        {
            var swTotal = Stopwatch.StartNew();
            AppLogger.Information("RenderCalendar – start");

            tableLayoutPanelCalendar.SuspendLayout();
            panelContainer.SuspendLayout();

            // 1) Load special days
            var swSpecialDays = Stopwatch.StartNew();
            await LoadSpecialDaysAsync();
            swSpecialDays.Stop();
            AppLogger.Information($"LoadSpecialDaysAsync: {swSpecialDays.ElapsedMilliseconds} ms");

            tableLayoutPanelCalendar.SetDate(_selectedDate);

            // 2) Load/create snacks + entries
            var swSnackAndEntries = Stopwatch.StartNew();
            var weekEntries = await _timeEntryRepo.GetTimeEntriesByUserAndCurrentWeekAsync(_selectedUser, _selectedDate);

            var snackDates = new HashSet<DateTime>(
                weekEntries
                    .Where(e => e.ProjectId == 132 && e.EntryTypeId == 24 && e.Timestamp.HasValue)
                    .Select(e => e.Timestamp.Value.Date)
            );

            var toCreate = Enumerable.Range(0, 7)
                .Select(i => _selectedDate.AddDays(i))
                .Where(d => !snackDates.Contains(d))
                .Select(day => new TimeEntry
                {
                    ProjectId = 132,
                    EntryTypeId = 24,
                    UserId = _selectedUser.Id,
                    Timestamp = day.AddMinutes(18 * TimeSlotLengthInMinutes),
                    EntryMinutes = TimeSlotLengthInMinutes,
                    IsValid = 1,
                    IsLocked = 1
                })
                .ToList();

            if (toCreate.Any())
            {
                await Task.WhenAll(toCreate.Select(snack => _timeEntryRepo.CreateTimeEntryAsync(snack)));
                weekEntries = await _timeEntryRepo.GetTimeEntriesByUserAndCurrentWeekAsync(_selectedUser, _selectedDate);
            }
            _currentEntries = weekEntries;
            swSnackAndEntries.Stop();
            AppLogger.Information($"Load/Create snacks + GetTimeEntries: {swSnackAndEntries.ElapsedMilliseconds} ms");

            // 3) UI reset + lookup + cache
            var swUISetup = Stopwatch.StartNew();
            ReleaseUnusedPanels();

            var allProjects = await _projectRepo.GetAllProjectsAsync();
            var projectDict = allProjects.ToDictionary(p => p.Id);
            var allEntryTypes = await _timeEntryTypeRepo.GetAllTimeEntryTypesAsync();
            _colorCache = allEntryTypes.ToDictionary(
                t => t.Id,
                t => ColorTranslator.FromHtml(t.Color ?? "#ADD8E6")
            );
            swUISetup.Stop();
            AppLogger.Information($"UI reset + load lookup data: {swUISetup.ElapsedMilliseconds} ms");

            // 4) Create panels
            var swPanels = Stopwatch.StartNew();
            foreach (var entry in _currentEntries)
                CreateOrUpdatePanel(entry);

            swPanels.Stop();
            AppLogger.Information($"CreatePanelForEntry loop: {swPanels.ElapsedMilliseconds} ms");

            // 5) Final UI update
            var swFinalUI = Stopwatch.StartNew();
            BeginInvoke((Action)(() =>
            {
                UpdateDateLabels();
                UpdateHourLabels();

                panelContainer.SuspendLayout();

                if (!userHasScrolled)
                {
                    // pokud uživatel nescrolloval, centrum na aktuální čas
                    var widths = tableLayoutPanelCalendar.GetColumnWidths();
                    int currentCol = (DateTime.Now.Hour * 60 + DateTime.Now.Minute) / TimeSlotLengthInMinutes;
                    int scrollX = widths.Take(currentCol).Sum() - panelContainer.ClientSize.Width / 2;
                    panelContainer.HorizontalScroll.Value = Math.Max(0, Math.Min(scrollX, panelContainer.HorizontalScroll.Maximum));
                }
                // pokud uživatel scrolloval, nezasahujeme do pozice

                DeactivateAllPanels();
                var toActivate = tableLayoutPanelCalendar.Controls
                    .OfType<DayPanel>()
                    .FirstOrDefault(p => p.EntryId == _selectedTimeEntryId);
                toActivate?.Activate();

                tableLayoutPanelCalendar.ResumeLayout(true);
                panelContainer.ResumeLayout(true);

                swFinalUI.Stop();
                AppLogger.Information($"Final UI update: {swFinalUI.ElapsedMilliseconds} ms");
            }));
            swTotal.Stop();
            AppLogger.Information($"RenderCalendar – total: {swTotal.ElapsedMilliseconds} ms");
        }


        private void dayPanel_MouseClick(object? sender, MouseEventArgs e)
        {
            if (mouseMoved) return;

            if (sender is not DayPanel panel) return;

            DeactivateAllPanels();
            panel.Activate();

            pasteTargetCell = new TableLayoutPanelCellPosition(
                tableLayoutPanelCalendar.GetColumn(panel),
                tableLayoutPanelCalendar.GetRow(panel)
            );

            tableLayoutPanelCalendar.ClearSelection();
        }

        private void DeactivateAllPanels()
        {
            foreach (var ctrl in tableLayoutPanelCalendar.Controls)
            {
                if (ctrl is DayPanel pan)
                {
                    pan.Deactivate();
                }
            }
        }

        private void dayPanel_MouseMove(object? sender, MouseEventArgs e)
        {
            if (sender is not DayPanel panel) return;

            if (panel.OwnerId != _loggedUser.Id) return;

            int rowHeight = tableLayoutPanelCalendar.Height / tableLayoutPanelCalendar.RowCount;
            int currentMouseY = tableLayoutPanelCalendar.PointToClient(Cursor.Position).Y;
            int newRow = Math.Max(0, Math.Min(currentMouseY / rowHeight, tableLayoutPanelCalendar.RowCount - 1));

            int currentMouseX = Cursor.Position.X;
            int deltaX = currentMouseX - startMouseX;
            int columnWidth = tableLayoutPanelCalendar.Width / tableLayoutPanelCalendar.ColumnCount;

            if (isResizing && activePanel == panel)
            {
                HandleResize(panel, deltaX, columnWidth);
            }
            else if (isMoving && activePanel == panel)
            {
                HandleMove(panel, deltaX, columnWidth);
            }
            else
            {
                UpdateCursor(e, panel);
            }
        }

        private bool mouseMoved = false;
        private void dayPanel_MouseDown(object? sender, MouseEventArgs e)
        {
            if (sender is not DayPanel panel) return;

            if (panel.Tag as string == "locked") return;

            if (panel.OwnerId != _loggedUser.Id) return;

            mouseMoved = false;
            DeactivateAllPanels();
            panel.Activate();

            isResizing = Cursor == Cursors.SizeWE;
            isMoving = !isResizing;

            activePanel = panel;
            startMouseX = Cursor.Position.X;
            originalColumn = tableLayoutPanelCalendar.GetColumn(panel);
            originalColumnSpan = tableLayoutPanelCalendar.GetColumnSpan(panel);

            panel.Capture = true;
        }

        private void dayPanel_MouseLeave(object? sender, EventArgs e)
        {
            if (!isResizing && !isMoving)
            {
                Cursor = Cursors.Default;
            }
        }

        private void HandleResize(DayPanel panel, int deltaX, int columnWidth)
        {
            if (panel.Tag as string == "snack") return;

            if (panel.OwnerId != _loggedUser.Id) return;

            if (isResizingLeft)
            {
                int newColumn = originalColumn + deltaX / columnWidth;
                int newSpan = originalColumnSpan - (newColumn - originalColumn);
                int minColumn = GetNearestLeftColumn(originalColumn, tableLayoutPanelCalendar.GetRow(panel), panel);

                if (newColumn >= minColumn && newSpan > 0 && newColumn + newSpan <= tableLayoutPanelCalendar.ColumnCount && !IsOverlapping(newColumn, newSpan, tableLayoutPanelCalendar.GetRow(panel), panel))
                {
                    tableLayoutPanelCalendar.SuspendLayout();
                    tableLayoutPanelCalendar.SetColumn(panel, newColumn);
                    tableLayoutPanelCalendar.SetColumnSpan(panel, newSpan);
                    tableLayoutPanelCalendar.ResumeLayout();
                }
            }
            else
            {
                int newSpan = originalColumnSpan + deltaX / columnWidth;
                int maxSpan = GetNearestRightColumn(originalColumn, originalColumnSpan, tableLayoutPanelCalendar.GetRow(panel), panel);

                if (newSpan > 0 && originalColumn + newSpan <= maxSpan && !IsOverlapping(originalColumn, newSpan, tableLayoutPanelCalendar.GetRow(panel), panel))
                {
                    tableLayoutPanelCalendar.SuspendLayout();
                    tableLayoutPanelCalendar.SetColumnSpan(panel, newSpan);
                    tableLayoutPanelCalendar.ResumeLayout();
                }
            }
        }

        private void HandleMove(DayPanel panel, int deltaX, int columnWidth)
        {
            if (panel.OwnerId != _loggedUser.Id) return;

            // 1) náš původní řádek
            int originalRow = tableLayoutPanelCalendar.GetRow(panel);

            // 2) novou pozici sloupce počítáme pořád z deltaX
            int targetColumn = originalColumn + deltaX / columnWidth;

            // 3) pro snack zafixujeme řádek, jinak si ho spočítáme
            int targetRow;
            if (panel.Tag as string == "snack")
            {
                targetRow = originalRow;
            }
            else
            {
                int rowHeight = tableLayoutPanelCalendar.Height / tableLayoutPanelCalendar.RowCount;
                int currentMouseY = tableLayoutPanelCalendar.PointToClient(Cursor.Position).Y;
                targetRow = Math.Max(0, Math.Min(currentMouseY / rowHeight, tableLayoutPanelCalendar.RowCount - 1));
            }

            int span = originalColumnSpan;

            // 4) zkontrolujeme, že se vejde do sloupců
            if (targetColumn < 0 || targetColumn + span > tableLayoutPanelCalendar.ColumnCount)
                return;

            // 5) kolize kontrolujeme vždy proti cílovému řádku
            if (!IsOverlapping(targetColumn, span, targetRow, panel))
            {
                // 6) jestli se změnila pozice, aplikuju
                bool hasMoved =
                    tableLayoutPanelCalendar.GetColumn(panel) != targetColumn ||
                    tableLayoutPanelCalendar.GetRow(panel) != targetRow;

                if (hasMoved)
                {
                    tableLayoutPanelCalendar.SuspendLayout();

                    tableLayoutPanelCalendar.SetColumn(panel, targetColumn);
                    // snack zůstává ve svém řádku
                    if (panel.Tag as string != "snack")
                        tableLayoutPanelCalendar.SetRow(panel, targetRow);

                    tableLayoutPanelCalendar.SetColumnSpan(panel, span);
                    mouseMoved = true;

                    tableLayoutPanelCalendar.ResumeLayout();
                }
            }
        }


        private void UpdateCursor(MouseEventArgs e, DayPanel panel)
        {
            if (panel.Tag as string == "snack")
            {
                Cursor = Cursors.SizeAll;
                return;
            }

            else if (panel.Tag as string == "locked")
                return;

            if (e.X <= ResizeThreshold)
            {
                Cursor = Cursors.SizeWE;
                isResizingLeft = true;
            }
            else if (e.X >= panel.Width - ResizeThreshold)
            {
                Cursor = Cursors.SizeWE;
                isResizingLeft = false;
            }
            else
            {
                Cursor = Cursors.SizeAll;
            }
        }

        private async void dayPanel_MouseUp(object? sender, MouseEventArgs e)
        {
            if (sender is not DayPanel panel) return;

            mouseMoved = false;
            isResizing = false;
            isMoving = false;
            activePanel = null;
            Cursor = Cursors.Default;

            var previousTimeEntryId = _selectedTimeEntryId;
            _selectedTimeEntryId = panel.EntryId;

            var allEntryTypes = await _timeEntryTypeRepo.GetAllTimeEntryTypesAsync();

            var entry = _currentEntries.FirstOrDefault(e => e.Id == _selectedTimeEntryId);
            if (entry == null) return;

            var newTimestamp = _selectedDate
                .AddDays(tableLayoutPanelCalendar.GetRow(panel))
                .AddMinutes(tableLayoutPanelCalendar.GetColumn(panel) * TimeSlotLengthInMinutes);

            var newDuration = GetEntryMinutesBasedOnColumnSpan(tableLayoutPanelCalendar.GetColumnSpan(panel));

            if (entry.Timestamp != newTimestamp || entry.EntryMinutes != newDuration)
            {
                entry.Timestamp = newTimestamp;
                entry.EntryMinutes = newDuration;
                await _timeEntryRepo.UpdateTimeEntryAsync(entry);

                UpdateHourLabels();
            }

            int minutesStart = newTimestamp.Hour * 60 + newTimestamp.Minute;
            int minutesEnd = minutesStart + entry.EntryMinutes;

            comboBoxStart.SelectedIndex = minutesStart / 30;
            comboBoxEnd.SelectedIndex = Math.Min(minutesEnd / 30, comboBoxEnd.Items.Count - 1);

            if (_selectedTimeEntryId != previousTimeEntryId)
            {
                await LoadSidebar();
            }
        }

        private bool IsOverlapping(int column, int span, int row, DayPanel currentPanel)
        {
            int start = column;
            int end = column + span - 1;

            foreach (DayPanel p in _activePanels)
            {
                if (p == currentPanel) continue;
                if (!p.Visible) continue;

                int pRow = tableLayoutPanelCalendar.GetRow(p);
                if (pRow != row) continue;

                int pStart = tableLayoutPanelCalendar.GetColumn(p);
                int pEnd = pStart + tableLayoutPanelCalendar.GetColumnSpan(p) - 1;

                // kontrola překryvu (jakékoliv překrytí mezi start–end a pStart–pEnd)
                if (start <= pEnd && end >= pStart)
                    return true;
            }

            return false;
        }


        private int GetNearestLeftColumn(int currentColumn, int row, DayPanel currentPanel)
        {
            int minColumn = 0;
            foreach (DayPanel p in panels)
            {
                if (p == currentPanel || tableLayoutPanelCalendar.GetRow(p) != row) continue;

                int pCol = tableLayoutPanelCalendar.GetColumn(p);
                int pSpan = tableLayoutPanelCalendar.GetColumnSpan(p);
                int rightEdge = pCol + pSpan;

                if (rightEdge <= currentColumn)
                {
                    minColumn = Math.Max(minColumn, rightEdge);
                }
            }
            return minColumn;
        }

        private int GetNearestRightColumn(int currentColumn, int currentSpan, int row, DayPanel currentPanel)
        {
            int maxColumn = tableLayoutPanelCalendar.ColumnCount;
            int panelRightEdge = currentColumn + currentSpan;

            foreach (DayPanel p in panels)
            {
                if (p == currentPanel || tableLayoutPanelCalendar.GetRow(p) != row) continue;

                int pCol = tableLayoutPanelCalendar.GetColumn(p);
                if (pCol >= panelRightEdge)
                {
                    maxColumn = Math.Min(maxColumn, pCol);
                }
            }
            return maxColumn;
        }

        private int GetEntryMinutesBasedOnColumnSpan(int columnSpan)
        {
            return columnSpan * 30;
        }
        #endregion


        public async Task DeleteRecord()
        {
            var timeEntry = _currentEntries.FirstOrDefault(e => e.Id == _selectedTimeEntryId);
            if (timeEntry == null) return;
            if (timeEntry.IsLocked == 1 ||
                (timeEntry.ProjectId == 132 && timeEntry.EntryTypeId == 24)) return;
            if (!ShowDeleteConfirmation(timeEntry)) return;

            bool success = await _timeEntryRepo.DeleteTimeEntryAsync(_selectedTimeEntryId);
            if (!success) return;
            AppLogger.Information($"Záznam smazán.");

            // 1) ulož scroll
            int scrollX = panelContainer.HorizontalScroll.Value;

            // 2) aktualizuj kolekci a UI
            _currentEntries.Remove(timeEntry);
            _selectedTimeEntryId = -1;
            BeginInvoke((Action)(() =>
            {
                RemoveEntryPanel(timeEntry.Id);
                panelContainer.HorizontalScroll.Value =
                    Math.Max(0, Math.Min(scrollX, panelContainer.HorizontalScroll.Maximum));
            }));

            await LoadSidebar();
        }



        private bool ShowDeleteConfirmation(TimeEntry entry)
        {
            var result = MessageBox.Show(
                $"Smazat záznam {(entry.IsValid == 1 ? FormatHelper.FormatTimeEntryToString(entry) : "")}?",
                "Smazat?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Exclamation);

            return result == DialogResult.Yes;
        }



        private Control? GetFocusedControl(Control control)
        {
            foreach (Control child in control.Controls)
            {
                if (child.ContainsFocus)
                {
                    if (child.HasChildren)
                        return GetFocusedControl(child);
                    else
                        return child;
                }
            }

            return control.Focused ? control : null;
        }

        private async void CopySelectedPanel()
        {
            if (_selectedTimeEntryId <= 0) return;

            if (_selectedUser.Id != _loggedUser.Id) return;

            var entry = _currentEntries.FirstOrDefault(e => e.Id == _selectedTimeEntryId);

            // svačina
            if (entry.ProjectId == 132 && entry.EntryTypeId == 24) return;

            if (entry != null)
            {
                copiedEntry = new TimeEntry
                {
                    EntryTypeId = entry.EntryTypeId,
                    ProjectId = entry.ProjectId,
                    Description = entry.Description,
                    Note = entry.Note,
                    EntryMinutes = entry.EntryMinutes,
                    AfterCare = entry.AfterCare,
                    UserId = entry.UserId,
                    IsValid = entry.IsValid
                };

                var panel = panels.FirstOrDefault(p => p.EntryId == _selectedTimeEntryId);
                if (panel != null)
                {
                    copyToolTip.ToolTipTitle = "Zkopírováno";
                    copyToolTip.Show("Záznam byl zkopírován", panel, panel.Width / 2, panel.Height / 2, 2000);
                }
            }
        }

      


        /// <summary>
        /// Vytvoří (nebo z poolu vyzvedne) panel pro jeden TimeEntry a přidá ho do kalendáře.
        /// </summary>
        private DayPanel CreateOrUpdatePanel(TimeEntry entry)
        {
            // 1) vezmi panel z poolu nebo vytvoř nový
            var panel = GetPooledPanel();
            panel.EntryId = entry.Id;
            panel.OwnerId = _selectedUser.Id;
            panel.Tag = (entry.ProjectId == 132 && entry.EntryTypeId == 24) ? "snack" : string.Empty;

            // 2) barva podle typu a validity
            if (!_colorCache.TryGetValue((int)entry.EntryTypeId, out var baseColor))
                baseColor = ColorTranslator.FromHtml("#ADD8E6");
            var finalColor = entry.IsValid == 1
                ? baseColor
                : ColorTranslator.FromHtml("#FF6957");
            panel.SetAssignedColor(finalColor);

            // 3) společný tooltip
            _sharedTooltip.SetToolTip(
                panel,
                $"{entry.Project?.ProjectTitle ?? "Projekt neznámý"}\n{entry.Note ?? "Bez poznámky"}"
            );

            // 4) pozice ve TableLayoutPanel
            int col = GetColumnBasedOnTimeEntry(entry.Timestamp);
            int row = GetRowBasedOnTimeEntry(entry.Timestamp);
            int span = GetColumnSpanBasedOnTimeEntry(entry.EntryMinutes);
            tableLayoutPanelCalendar.Controls.Add(panel, col, row);
            tableLayoutPanelCalendar.SetColumnSpan(panel, span);
            _activePanels.Add(panel);

            // 5) odložené naplnění textů (čeká na správné rozměry)
            if (entry.IsValid == 1)
            {
                var timer = new Timer { Interval = 10 };
                timer.Tick += (s, e) =>
                {
                    if (panel.Width > 10)
                    {
                        timer.Stop();
                        timer.Dispose();
                        panel.UpdateUi(
                            (entry.Project?.IsArchived == 1 ? "(AFTERCARE) " : "") +
                            (entry.Project?.ProjectType == 1
                                ? entry.Project?.ProjectDescription
                                : entry.Project?.ProjectTitle),
                            entry.Description
                        );
                    }
                };
                timer.Start();
            }
            else
            {
                // nevalidní: žádný text
                panel.UpdateUi(null, null);
            }

            return panel;
        }

        /// <summary>
        /// Odstraní panel s daným EntryId z kalendáře a vrátí ho zpět do poolu.
        /// </summary>
        private void RemoveEntryPanel(int entryId)
        {
            var panel = _activePanels.FirstOrDefault(p => p.EntryId == entryId);
            if (panel == null) return;

            // zruš případný timer
            if (_uiTimers.TryGetValue(panel, out var t))
            {
                t.Stop();
                t.Dispose();
                _uiTimers.Remove(panel);
            }

            tableLayoutPanelCalendar.Controls.Remove(panel);
            panel.Visible = false;
            _activePanels.Remove(panel);
            _panelPool.Enqueue(panel);
        }

        /// <summary>
        /// Po vytvoření nového záznamu vloží panel a obnoví původní scroll.
        /// </summary>
        private async Task OnNewEntryCreated(TimeEntry newEntry)
        {
            newEntry.Id = 0;
            var created = await _timeEntryRepo.CreateTimeEntryAsync(newEntry);
            if (created == null) return;

            _currentEntries.Add(created);
            if (_colorCache == null || _colorCache.Count == 0)
                await LoadCachesAsync();

            // 1) ulož aktuální scroll
            int scrollX = panelContainer.HorizontalScroll.Value;

            // 2) na UI vlákne
            BeginInvoke((Action)(() =>
            {
                CreateOrUpdatePanel(created);
                // 3) obnov scroll (omezen rozsahy)
                panelContainer.HorizontalScroll.Value =
                    Math.Max(0, Math.Min(scrollX, panelContainer.HorizontalScroll.Maximum));
            }));

            _selectedTimeEntryId = created.Id;
            _ = LoadSidebar();
        }



        /// <summary>
        /// Načte nebo obnoví cache projektů a typů záznamů (pro CreateOrUpdatePanel mimo RenderCalendar).
        /// </summary>
        private async Task LoadCachesAsync()
        {
            // projekty
            var allProjects = await _projectRepo.GetAllProjectsAsync();
            _projects = allProjects; // nebo do samostatné proměnné cache

            // typy záznamů
            var allEntryTypes = await _timeEntryTypeRepo.GetAllTimeEntryTypesAsync();
            _timeEntryTypes = allEntryTypes;

            // připrav barvy
            _colorCache = allEntryTypes.ToDictionary(
                t => t.Id,
                t => ColorTranslator.FromHtml(t.Color ?? "#ADD8E6")
            );
        }

    

        private void flowLayoutPanel2_SizeChanged(object sender, EventArgs e)
        {
            int newWidth = flowLayoutPanel2.ClientSize.Width - 10;
            tableLayoutPanelProject.Width = newWidth;
            tableLayoutPanelEntryType.Width = newWidth;
            tableLayoutPanelEntrySubType.Width = newWidth;
            tableLayoutPanel6.Width = newWidth;

            ClearComboBoxSelections(flowLayoutPanel2);
        }

        private void ClearComboBoxSelections(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is ComboBox cb)
                {
                    cb.SelectionStart = cb.Text.Length;
                    cb.SelectionLength = 0;
                }
                else
                {
                    ClearComboBoxSelections(control);
                }
            }
        }

        private void panelDay_Click(object sender, EventArgs e)
        {
            var current = _config.PanelDayView;
            var enumValues = Enum.GetValues(typeof(PanelDayView)).Cast<PanelDayView>().ToArray();
            int index = Array.IndexOf(enumValues, current);
            int nextIndex = (index + 1) % enumValues.Length;
            _config.PanelDayView = enumValues[nextIndex];

            UpdateHourLabels();
            ConfigService.Save(_config);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Zjisti aktuálně fokusovaný prvek uvnitř tohoto UserControl
            Control? focused = this.ContainsFocus ? this.GetFocusedControl(this) : null;

            if (focused is TextBoxBase or ComboBox)
                return base.ProcessCmdKey(ref msg, keyData);

            if (keyData == (Keys.Control | Keys.C))
            {
                CopySelectedPanel();
                return true;
            }

            if (keyData == (Keys.Control | Keys.V))
            {
                PasteCopiedPanel();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private async void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton rb && rb.Checked)
            {
                int index = 0;
                label4.Text = "Poznámka";

                tableLayoutPanel4.Visible = true;
                tableLayoutPanel6.Visible = true;
                tableLayoutPanelProject.Visible = true;
                tableLayoutPanelEntryType.Visible = true;
                tableLayoutPanelEntrySubType.Visible = true;
                customComboBoxSubTypes.SetText(string.Empty);
                customComboBoxProjects.SetText(string.Empty);
                panel4.Visible = true;

                switch (rb.Text)
                {
                    case "PROVOZ":
                        index = 0;
                        labelProject.Text = "Nákladové středisko*";
                        labelType.Text = "Typ záznamu*";
                        tableLayoutPanelProject.Visible = true;
                        tableLayoutPanelEntryType.Visible = true;
                        tableLayoutPanelEntrySubType.Visible = true;
                        checkBoxArchivedProjects.Visible = false;
                        break;
                    case "PROJEKT":
                        index = 1;
                        labelProject.Text = "Projekt*";
                        labelType.Text = "Typ záznamu*";
                        tableLayoutPanelProject.Visible = true;
                        tableLayoutPanelEntryType.Visible = true;
                        tableLayoutPanelEntrySubType.Visible = true;
                        checkBoxArchivedProjects.Visible = true;
                        break;
                    case "ŠKOLENÍ":
                        index = 3;
                        tableLayoutPanelProject.Visible = false;
                        tableLayoutPanelEntryType.Visible = false;
                        tableLayoutPanelEntrySubType.Visible = false;
                        checkBoxArchivedProjects.Visible = false;
                        label4.Text = "Poznámka*";
                        break;
                    case "NEPŘÍTOMNOST":
                        labelType.Text = "Důvod*";
                        tableLayoutPanelProject.Visible = false;
                        tableLayoutPanelEntryType.Visible = true;
                        tableLayoutPanelEntrySubType.Visible = false;
                        checkBoxArchivedProjects.Visible = false;
                        index = 4;
                        break;
                    default:
                        index = 5;
                        labelType.Text = "Činnost*";
                        tableLayoutPanelProject.Visible = false;
                        tableLayoutPanelEntryType.Visible = true;
                        tableLayoutPanelEntrySubType.Visible = true;
                        checkBoxArchivedProjects.Visible = false;
                        break;
                }

                await LoadProjectsAsync(index);
                await LoadTimeEntryTypesAsync(index);
            }
        }

        private async void checkBoxArchivedProjects_CheckedChanged(object sender, EventArgs e)
        {
            await LoadProjectsAsync(1);
            await LoadTimeEntryTypesAsync(1);
        }

        private async void buttonConfirm_Click(object sender, EventArgs e)
        {
            var (valid, reason) = CheckForEmptyOrIncorrectFields();
            if (!valid)
            {
                AppLogger.Error($"Je třeba správně vyplnit všechna potřebná data! Chybný parametr: {reason}");
                return;
            }

            int selectedEntryTypeId = 0;

            if (_currentProjectType is 0 or 1 or 2)
            {
                // Projektový typ s radio buttony
                var radioButtons = tableLayoutPanelEntryType.Controls
                    .OfType<TableLayoutPanel>()
                    .SelectMany(panel => panel.Controls.OfType<RadioButton>())
                    .ToList();

                for (int i = 0; i < radioButtons.Count; i++)
                {
                    if (radioButtons[i].Checked)
                    {
                        selectedEntryTypeId = _currentProjectType switch
                        {
                            0 => 1 + i,
                            1 => 10 + i,
                            2 => 13 + i,
                            _ => 0
                        };
                        break;
                    }
                }
            }
            else if (comboBoxEntryType.SelectedIndex > -1)
            {
                selectedEntryTypeId = _timeEntryTypes[comboBoxEntryType.SelectedIndex].Id;
            }

            var newSubType = new TimeEntrySubType
            {
                Title = customComboBoxSubTypes.GetText(),
                UserId = _selectedUser.Id
            };

            var addedTimeEntrySubType = await _timeEntrySubTypeRepo.CreateTimeEntrySubTypeAsync(newSubType);

            var timeEntry = _currentEntries.FirstOrDefault(e => e.Id == _selectedTimeEntryId);
            if (timeEntry == null) return;

            timeEntry.Description = addedTimeEntrySubType.Title;
            timeEntry.EntryTypeId = selectedEntryTypeId;
            timeEntry.Note = textBoxNote.Text;

            var selectedProject = _projects.FirstOrDefault(p =>
                FormatHelper.FormatProjectToString(p).Equals(customComboBoxProjects.SelectedItem, StringComparison.InvariantCultureIgnoreCase));

            if (selectedProject != null)
            {
                timeEntry.ProjectId = selectedProject.Id;
            }

            if (radioButton5.Checked)
            {
                timeEntry.ProjectId = 25;
            }
            else if (radioButton4.Checked)
            {
                timeEntry.ProjectId = 23;
            }
            else if (radioButton3.Checked)
            {
                timeEntry.ProjectId = 26;
                timeEntry.EntryTypeId = 16;
            }

            timeEntry.AfterCare = _projects.FirstOrDefault(x => x.Id == timeEntry.ProjectId)?.IsArchived ?? 0;
            timeEntry.IsValid = 1;

            bool success = await _timeEntryRepo.UpdateTimeEntryAsync(timeEntry);
            if (success)
            {
                AppLogger.Information($"Záznam {FormatHelper.FormatTimeEntryToString(timeEntry)} byl úspěšně aktualizován.");
                await LoadTimeEntrySubTypesAsync();
                await RenderCalendar();
                await LoadSidebar();
            }
        }

        private async void buttonRemove_Click(object sender, EventArgs e)
        {
            await DeleteRecord();
        }


        private (bool valid, string reason) CheckForEmptyOrIncorrectFields()
        {
            var rb = flowLayoutPanel2.Controls
               .OfType<RadioButton>()
               .FirstOrDefault(r => r.Checked);

            bool ProjectTextMatches = _projects.Any(p =>
                FormatHelper.FormatProjectToString(p).Equals(customComboBoxProjects.SelectedItem, StringComparison.InvariantCultureIgnoreCase));

            bool EntryTypeMatches = _timeEntryTypes.Any(t =>
                FormatHelper.FormatTimeEntryTypeToString(t).Equals(comboBoxEntryType.Text, StringComparison.InvariantCultureIgnoreCase) ||
                FormatHelper.FormatTimeEntryTypeWithAfterCareToString(t).Equals(comboBoxEntryType.Text, StringComparison.InvariantCultureIgnoreCase));

            switch (rb?.Text)
            {
                case "PROVOZ":
                    if (string.IsNullOrWhiteSpace(customComboBoxProjects.SelectedItem) || !ProjectTextMatches)
                        return (false, "Nákladové středisko neodpovídá žádné možnosti");
                    break;
                case "PROJEKT":
                case "PŘEDPROJEKT":
                    if (string.IsNullOrWhiteSpace(customComboBoxProjects.SelectedItem) || !ProjectTextMatches)
                        return (false, "Projekt neodpovídá žádné možnosti");
                    break;
                case "ŠKOLENÍ":
                    if (string.IsNullOrWhiteSpace(textBoxNote.Text))
                        return (false, "Poznámka");
                    break;
                case "NEPŘÍTOMNOST":
                    if (string.IsNullOrWhiteSpace(comboBoxEntryType.Text) || !EntryTypeMatches)
                        return (false, "Důvod neodpovídá žádné možnosti");
                    break;
                default:
                    if (string.IsNullOrWhiteSpace(comboBoxEntryType.Text) || !EntryTypeMatches)
                        return (false, "Činnost neodpovídá žádné možnosti");
                    break;
            }

            return (true, "");
        }
    }
}
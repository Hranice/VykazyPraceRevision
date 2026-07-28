using VykazyPrace.Core.Configuration;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Helpers;
using Timer = System.Windows.Forms.Timer;

namespace VykazyPrace.UserControls.CalendarV2
{
    public partial class CalendarV2
    {
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

                panel.MouseMove += dayPanel_MouseMove;
                panel.MouseDown += dayPanel_MouseDown;
                panel.MouseUp += dayPanel_MouseUp;
                panel.MouseLeave += dayPanel_MouseLeave;
                panel.MouseClick += dayPanel_MouseClick;
                panel.ContextMenuStrip = dayPanelMenu;
            }
            return panel;
        }

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
            tableLayoutPanelCalendar.SuspendLayout();
            panelContainer.SuspendLayout();

            await LoadSpecialDaysAsync();

            tableLayoutPanelCalendar.SetDate(_selectedDate);

            // Load/create snacks + entries
            var weekEntries = await _timeEntryRepo.GetTimeEntriesByUserAndCurrentWeekAsync(_selectedUser, _selectedDate);

            var snackDates = new HashSet<DateTime>(
                weekEntries
                    .Where(e => e.ProjectId == WorkLogIds.Projects.Snack && e.EntryTypeId == WorkLogIds.EntryTypes.Snack && e.Timestamp.HasValue)
                    .Select(e => e.Timestamp.Value.Date)
            );

            var toCreate = Enumerable.Range(0, 7)
                .Select(i => _selectedDate.AddDays(i))
                .Where(d => !snackDates.Contains(d))
                .Select(day => new TimeEntry
                {
                    ProjectId = WorkLogIds.Projects.Snack,
                    EntryTypeId = WorkLogIds.EntryTypes.Snack,
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

            ReleaseUnusedPanels();

            var allProjects = await _projectRepo.GetAllProjectsAsync();
            var projectDict = allProjects.ToDictionary(p => p.Id);
            var allEntryTypes = await _timeEntryTypeRepo.GetAllTimeEntryTypesAsync();
            _colorCache = allEntryTypes.ToDictionary(
                t => t.Id,
                t => ColorTranslator.FromHtml(t.Color ?? "#ADD8E6")
            );

            foreach (var entry in _currentEntries)
                CreateOrUpdatePanel(entry);


            foreach (var p in _activePanels)
                p.Selected = _selectedEntryIds.Contains(p.EntryId);

            BeginInvoke((Action)(() =>
            {
                UpdateDateLabels();
                UpdateHourLabels();

                panelContainer.SuspendLayout();

                if (!userHasScrolled)
                {
                    // pokud uživatel nescrolloval, center na aktuální čas
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
            }));
        }

        private void SyncColumns()
        {
            var widths = customTableLayoutPanel1.GetStableColumnWidths();

            tableLayoutPanelCalendar.ApplyColumnPixelWidths(widths);

            //customHeader.Invalidate();
        }

        private DayPanel CreateOrUpdatePanel(TimeEntry entry)
        {
            var panel = GetPooledPanel();
            panel.EntryId = entry.Id;
            panel.OwnerId = _selectedUser.Id;
            panel.Tag = null;

            if (entry.ProjectId == WorkLogIds.Projects.Snack && entry.EntryTypeId == WorkLogIds.EntryTypes.Snack)
                panel.Tag = "snack";
            else if (entry.IsLocked == 1)
                panel.Tag = "locked";

            // barva podle typu a validity
            if (!_colorCache.TryGetValue((int)entry.EntryTypeId, out var baseColor))
                baseColor = ColorTranslator.FromHtml("#ADD8E6");

            var finalColor = entry.IsValid == 1
                ? baseColor
                : ColorTranslator.FromHtml("#FF6957");
            panel.SetAssignedColor(finalColor);

            _sharedTooltip.SetToolTip(
                panel,
                $"{entry.Project?.ProjectTitle ?? "Projekt neznámý"}\n{entry.Note ?? "Bez poznámky"}"
            );

            int col = GetColumnBasedOnTimeEntry(entry.Timestamp);
            int row = GetRowBasedOnTimeEntry(entry.Timestamp);
            int span = GetColumnSpanBasedOnTimeEntry(entry.EntryMinutes);
            tableLayoutPanelCalendar.Controls.Add(panel, col, row);
            tableLayoutPanelCalendar.SetColumnSpan(panel, span);
            _activePanels.Add(panel);

            // odložené naplnění textů (čeká na správné rozměry)
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

        private async Task OnNewEntryCreated(TimeEntry newEntry)
        {
            newEntry.Id = 0;

            var created = await _timeEntryRepo.CreateTimeEntryAsync(newEntry);
            if (created == null) return;
            _currentEntries.Add(created);

            if (_colorCache == null || _colorCache.Count == 0)
                await LoadCachesAsync();

            // ulož scroll pozici
            int scrollX = panelContainer.HorizontalScroll.Value;
            BeginInvoke((Action)(() =>
            {
                CreateOrUpdatePanel(created);

                // obnov scroll tam, kde byl
                panelContainer.HorizontalScroll.Value =
                    Math.Max(0, Math.Min(scrollX, panelContainer.HorizontalScroll.Maximum));

                UpdateHourLabels();
            }));

            _selectedTimeEntryId = created.Id;
            _ = LoadSidebar();
        }

        /// <summary>
        /// Načte nebo obnoví cache projektů, entrytypes a barev.
        /// </summary>
        private async Task LoadCachesAsync()
        {
            var allProjects = await _projectRepo.GetAllProjectsAsync();
            _projects = allProjects;

            var allEntryTypes = await _timeEntryTypeRepo.GetAllTimeEntryTypesAsync();
            _timeEntryTypes = allEntryTypes;

            _colorCache = allEntryTypes.ToDictionary(
                t => t.Id,
                t => ColorTranslator.FromHtml(t.Color ?? "#ADD8E6")
            );
        }

        /// <summary>
        /// Událost překliknutí vyobrazení levého sidebaru
        /// (počet odpracovaných hodin vůči reálným apod.).
        /// </summary>
        private void panelDay_Click(object sender, EventArgs e)
        {
            var config = _configService.Current;

            var panelDayView = config.PanelDayView;
            var enumValues = Enum.GetValues(typeof(PanelDayView)).Cast<PanelDayView>().ToArray();
            int index = Array.IndexOf(enumValues, panelDayView);
            int nextIndex = (index + 1) % enumValues.Length;
            config.PanelDayView = enumValues[nextIndex];

            UpdateHourLabels();
            _configService.Save();
        }

        /// <summary>
        /// Aktualizuje datumové a denní popisky (labelDateXX a labelDayXX).
        /// Pokud je den uveden v kolekci _specialDays (svátky), použije se zvýrazněná barva.
        /// </summary>
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

        /// <summary>
        /// Aktualizuje hodinové součty pro každý den v týdnu a zobrazuje je
        /// v příslušných labelech (labelHours01–labelHours07).
        /// </summary>
        private void UpdateHourLabels()
        {
            var config = _configService.Current;

            // Pole labelů pro 7 dní (Po–Ne)
            Label[] hourLabels =
            {
        labelHours01, labelHours02, labelHours03,
        labelHours04, labelHours05, labelHours06, labelHours07
    };

            for (int row = 0; row < 7; row++)
            {
                DateTime day = _selectedDate.AddDays(row);

                int totalMinutes = _currentEntries
                    .Where(entry =>
                        entry.Timestamp.HasValue &&
                        entry.Timestamp.Value.Date == day.Date &&
                        entry.IsValid == 1 &&                   // jen validní záznamy
                        !(entry.ProjectId == WorkLogIds.Projects.Snack && entry.EntryTypeId == WorkLogIds.EntryTypes.Snack) && // vynechává svačiny
                        entry.ProjectId != WorkLogIds.Projects.Absence &&            // vynechává nepřítomnosti
                        entry.EntryTypeId != WorkLogIds.EntryTypes.OutlookEvent)            // vynechává outlook události
                    .Sum(entry => entry.EntryMinutes);

                double reportedHours = totalMinutes / 60.0;

                // Získání docházkových dat (pokud existují)
                var dochazka = _arrivalDepartures
                    .FirstOrDefault(a => a.WorkDate.Date == day.Date);

                double hoursWorked = dochazka?.HoursWorked ?? 0;

                switch (config.PanelDayView)
                {
                    case PanelDayView.Default:
                        // Pouze vykázané hodiny (černá)
                        hourLabels[row].Text = $"{reportedHours:F1}";
                        hourLabels[row].ForeColor = Color.Black;
                        break;

                    case PanelDayView.Range:
                        // Vykázané / skutečně odpracované z docházky
                        hourLabels[row].Text = $"{reportedHours:F1} / {hoursWorked:F1} h";
                        hourLabels[row].ForeColor = Color.Black;
                        break;

                    case PanelDayView.ColorWithinRange:
                        // Barva podle shody s docházkou
                        hourLabels[row].Text = $"{reportedHours:F1}";

                        if (Math.Abs(reportedHours - hoursWorked) < 0.01)
                            hourLabels[row].ForeColor = Color.Green;   // shoduje se
                        else
                            hourLabels[row].ForeColor = Color.Red;     // nesouhlasí
                        break;

                    case PanelDayView.ColorOvertime:
                        // Barevné zvýraznění podle odpracovaného času
                        hourLabels[row].Text = $"{reportedHours:F1}";

                        if (reportedHours == 7.5)
                            hourLabels[row].ForeColor = Color.Green;   // přesně 7.5 h
                        else if (reportedHours > 7.5)
                            hourLabels[row].ForeColor = Color.Blue;    // přesčas
                        else
                            hourLabels[row].ForeColor = Color.Red;     // méně než norma
                        break;
                }

                if (dochazka == null)
                {
                    // bez docházkových dat se nepoužívá barevné zvýraznění
                    hourLabels[row].ForeColor = Color.Black;
                }
            }
        }
    }
}

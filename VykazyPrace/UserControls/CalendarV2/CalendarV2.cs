using VykazyPrace.Core.Configuration;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Core.Helpers;
using VykazyPrace.Core.Logging;
using VykazyPrace.Core.Services.TimeEntry;
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
        private readonly Queue<DayPanel> _panelPool = new();
        private readonly List<DayPanel> _activePanels = new();
        private bool userHasScrolled = false;

        // Repositories
        private readonly TimeEntryRepository _timeEntryRepo;
        private readonly TimeEntryTypeRepository _timeEntryTypeRepo;
        private readonly TimeEntrySubTypeRepository _timeEntrySubTypeRepo;
        private readonly ProjectRepository _projectRepo;
        private readonly SpecialDayRepository _specialDayRepo;
        private readonly ArrivalDepartureRepository _arrivalDepartureRepo;

        // Services
        private readonly TimeEntryUpdateService _timeEntryUpdateService;

        // Data cache
        private static List<TimeEntryType>? _cacheTypes;
        private static List<TimeEntrySubType>? _cacheSubTypes;
        private static List<Project>? _cacheProjects;
        private Dictionary<int, Color> _colorCache = new Dictionary<int, Color>();

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
        private bool mouseMoved = false;
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

        // Right click context and tooltips
        private ContextMenuStrip dayPanelMenu;
        private ContextMenuStrip tableLayoutMenu;
        private readonly ToolTip _sharedTooltip = new ToolTip()
        {
            AutoPopDelay = 5000,
            InitialDelay = 300,
            ReshowDelay = 100,
            ShowAlways = true
        };

        // Configuration
        private readonly IConfigService _configService;

        public CalendarV2(
            User currentUser,
            IConfigService configService,
            TimeEntryRepository timeEntryRepo,
            TimeEntryTypeRepository timeEntryTypeRepo,
            TimeEntrySubTypeRepository timeEntrySubTypeRepo,
            ProjectRepository projectRepo,
            SpecialDayRepository specialDayRepo,
            ArrivalDepartureRepository arrivalDepartureRepo)
        {
            InitializeComponent();
            DoubleBuffered = true;

            _selectedDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
            _selectedUser = currentUser;
            _loggedUser = currentUser;

            _configService = configService;

            _timeEntryRepo = timeEntryRepo;
            _timeEntryTypeRepo = timeEntryTypeRepo;
            _timeEntrySubTypeRepo = timeEntrySubTypeRepo;
            _projectRepo = projectRepo;
            _specialDayRepo = specialDayRepo;
            _arrivalDepartureRepo = arrivalDepartureRepo;

            _timeEntryUpdateService = new TimeEntryUpdateService(
                _timeEntryRepo,
                _timeEntrySubTypeRepo);

            _resizeTimer.Tick += async (_, _) =>
            {
                _resizeTimer.Stop();

                int headerH = customTableLayoutPanel1.Height;
                int newH = ClientSize.Height - headerH;

                tableLayoutPanelCalendar.Height = Math.Max(0, newH);
                tableLayoutPanelCalendar.PerformLayout();
                tableLayoutPanelCalendar.Invalidate();

                await AdjustIndicatorsAsync(
                    panelContainer.AutoScrollPosition,
                    _selectedUser.Id,
                    _selectedDate);
            };

            Resize += (_, __) => SyncColumns();
            Load += (_, __) => SyncColumns();

            InitializeContextMenus();
        }

        private readonly HashSet<int> _selectedEntryIds = new();

        private DayPanel? GetPanelByEntryId(int entryId)
            => _activePanels.FirstOrDefault(p => p.EntryId == entryId);


        private void InitializeContextMenus()
        {
            dayPanelMenu = new ContextMenuStrip();
            dayPanelMenu.Items.Add("Kopírovat", null, (_, _) => CopySelectedPanel());
            dayPanelMenu.Items.Add("Odstranit", null, async (_, _) => await DeleteRecord());

            tableLayoutMenu = new ContextMenuStrip();
            tableLayoutMenu.Items.Add("Vložit", null, (_, _) => PasteCopiedPanel());
        }

        private void ClearSelection()
        {
            foreach (var id in _selectedEntryIds.ToList())
                SetSelectedUi(id, false);

            _selectedEntryIds.Clear();
        }


        public async Task ForceReloadAsync()
        {
            // 1) Vymažeme všechny cache
            _cacheTypes = null;
            _cacheSubTypes = null;
            _cacheProjects = null;

            // 2) Obecné referenční datové sady
            await LoadReferenceDataAsync();

            // 3) Explicitně znovu nahraj projekty & typy pro právě vybraný projectType
            await LoadProjectsAsync(_currentProjectType);
            await LoadTimeEntryTypesAsync(_currentProjectType);

            // 4) Aktualizuj UI ComboBoxy
            SafeInvoke(() =>
            {
                customComboBoxProjects.SetItems(
                    _projects.Select(FormatHelper.FormatProjectToString).ToArray()
                );
                UpdateEntryTypeControls(_currentProjectType);
                customComboBoxSubTypes.SetItems(
                    _timeEntrySubTypes
                        .Where(t => t.IsArchived == 0)
                        .Select(FormatHelper.FormatTimeEntrySubTypeToString)
                        .ToArray()
                );
            });

            // 5) Načti znovu týdenní data
            var specialTask = LoadSpecialDaysAsync();
            var arrivalTask = LoadArrivalDeparturesAsync();
            await Task.WhenAll(specialTask, arrivalTask);

            // 6) Překresli kalendář a indikátory
            await RenderCalendar();
            await AdjustIndicatorsAsync(panelContainer.AutoScrollPosition, _selectedUser.Id, _selectedDate);
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
            panelContainer.Scroll += PanelContainer_Scroll;
            _ = LoadInitialDataAsync();
        }

        private void PanelContainer_Scroll(object? sender, ScrollEventArgs e)
        {
            userHasScrolled = true;
        }

    }
}

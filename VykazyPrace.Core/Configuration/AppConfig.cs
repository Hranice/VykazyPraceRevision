using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VykazyPrace.Core.Configuration
{
    public class AppConfig
    {
        public string DatabasePath { get; set; } = @"Z:\TS\jprochazka-sw\WorkLog\Db\WorkLog.db";
        public PanelDayView PanelDayView { get; set; } = PanelDayView.Default;
        public WindowConfig MainWindow { get; set; } = new();
        public bool RememberLastResolutionPosition { get; set; }
        public bool MinimizeToTray { get; set; } = true;
        public bool NotificationOn { get; set; } = true;
        public DateTime NotificationTime { get; set; } = new DateTime(2000, 1, 1, 13, 30, 0);
        public string NotificationTitle { get; set; } = "Už je čas!";
        public string NotificationText { get; set; } = "Čas vykázat hodiny!";
        public string LogLevel { get; set; } = "Information";
        public ExportSelectionConfig ExportSelection { get; set; } = new();
        public UserViewSelectionConfig UserViewSelection { get; set; } = new();
    }

    public class UserViewSelectionConfig
    {
        public int? SelectedUserId { get; set; }
        public List<int> SelectedUserGroupIds { get; set; } = new();
    }

    public class ExportSelectionConfig
    {
        public List<int> SelectedUserGroupIds { get; set; } = new();
        public List<int> SelectedUserIds { get; set; } = new();

        public ExportRangeType SelectedRangeType { get; set; } = ExportRangeType.TimePeriod;

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public int? Week { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public bool BuildEvaluationSheet { get; set; }
    }

    public class WindowConfig
    {
        public int Width { get; set; } = 1200;
        public int Height { get; set; } = 800;
        public int X { get; set; } = -1;
        public int Y { get; set; } = -1;
        public bool HasPosition { get; set; }
        public string? ScreenDeviceName { get; set; }
        public int? ScreenOffsetX { get; set; }
        public int? ScreenOffsetY { get; set; }
        public bool Maximized { get; set; } = false;
    }


    public enum PanelDayView
    {
        Default,
        Range,
        ColorWithinRange,
        ColorOvertime
    }

    public enum ExportRangeType
    {
        TimePeriod,
        Week,
        Month,
        Year
    }
}

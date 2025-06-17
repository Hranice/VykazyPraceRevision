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
        public bool AppMaximized { get; set; } = false;
        public PanelDayView PanelDayView { get; set; } = PanelDayView.Default;
        public bool MinimizeToTray { get; set; } = true;
        public bool NotificationOn { get; set; } = true;
        public DateTime NotificationTime { get; set; } = new DateTime(2000, 1, 1, 13, 30, 0);
        public string NotificationTitle { get; set; } = "Už je čas!";
        public string NotificationText { get; set; } = "Čas vykázat hodiny!";
        public string LogLevel { get; set; } = "Information";
    }

    public enum PanelDayView
    {
        Default,
        Range,
        ColorWithinRange,
        ColorOvertime
    }

}

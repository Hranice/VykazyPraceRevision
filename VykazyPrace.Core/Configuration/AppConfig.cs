﻿using System;
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
    }

    public enum PanelDayView
    {
        Default,
        Range,
        ColorWithinRange,
        ColorOvertime
    }

}

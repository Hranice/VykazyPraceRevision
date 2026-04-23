using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VykazyPrace.Updater
{
    public class UpdateInfo
    {
        public Version CurrentVersion { get; set; } = new Version(0, 0, 0, 0);
        public Version? LatestVersion { get; set; }
        public bool UpdateAvailable => LatestVersion != null && LatestVersion > CurrentVersion;
        public bool UpdateFilesAvailable { get; set; }
        public string? ChangelogPath { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

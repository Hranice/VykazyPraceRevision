using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace VykazyPrace.Core.Configuration
{
    public static class ConfigService
    {
        private static string ConfigDir => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WorkLog");
        private static string ConfigFile => Path.Combine(ConfigDir, "config.json");

        public static AppConfig Load()
        {
            if (!File.Exists(ConfigFile))
            {
                Directory.CreateDirectory(ConfigDir);
                var defaultConfig = new AppConfig();
                Save(defaultConfig);
                return defaultConfig;
            }

            return JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(ConfigFile))!;
        }

        public static void Save(AppConfig config)
        {
            Directory.CreateDirectory(ConfigDir);
            File.WriteAllText(ConfigFile, JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
        }
    }

}

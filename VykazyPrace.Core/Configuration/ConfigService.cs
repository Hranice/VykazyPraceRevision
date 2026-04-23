using System.Text.Json;
using VykazyPrace.Core.Helpers;

namespace VykazyPrace.Core.Configuration
{
    public static class ConfigService
    {
        public static AppConfig Load()
        {
            if (!File.Exists(AppPaths.ConfigFile))
            {
                Directory.CreateDirectory(AppPaths.RoamingDirectory);
                var defaultConfig = new AppConfig();
                Save(defaultConfig);
                return defaultConfig;
            }

            return JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(AppPaths.ConfigFile))!;
        }

        public static void Save(AppConfig config)
        {
            Directory.CreateDirectory(AppPaths.RoamingDirectory);
            File.WriteAllText(
                AppPaths.ConfigFile,
                JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
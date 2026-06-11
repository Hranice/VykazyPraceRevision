using System.Text.Json;
using VykazyPrace.Core.Helpers;

namespace VykazyPrace.Core.Configuration
{
    public class ConfigService : IConfigService
    {
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        public AppConfig Current { get; private set; }

        public ConfigService()
        {
            Current = Load();
        }

        public AppConfig Load()
        {
            Directory.CreateDirectory(AppPaths.RoamingDirectory);

            if (!File.Exists(AppPaths.ConfigFile))
            {
                Current = new AppConfig();
                Save(Current);
                return Current;
            }

            var json = File.ReadAllText(AppPaths.ConfigFile);

            Current = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();

            return Current;
        }

        public void Save()
        {
            Save(Current);
        }

        public void Save(AppConfig config)
        {
            Directory.CreateDirectory(AppPaths.RoamingDirectory);

            File.WriteAllText(
                AppPaths.ConfigFile,
                JsonSerializer.Serialize(config, _jsonOptions));

            Current = config;
        }
    }
}
namespace VykazyPrace.Core.Configuration
{
    public interface IConfigService
    {
        AppConfig Current { get; }

        AppConfig Load();
        void Save();
        void Save(AppConfig config);
    }
}
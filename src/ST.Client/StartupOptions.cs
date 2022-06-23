using System.Application.Settings;

namespace System.Application
{
    public sealed class StartupOptions
    {
        public bool HasMainProcessRequired { get; set; }

        public bool HasNotifyIcon { get; set; }

        public bool HasGUI { get; set; }

        public bool HasServerApiClient { get; set; }

        public bool HasHttpClientFactory { get; set; }

        public bool HasHttpProxy { get; set; }

        public bool HasHosts { get; set; }

        public bool HasSteam { get; set; }

        public StartupOptions(DILevel level)
        {
            mValue = this;
            HasMainProcessRequired = level.HasFlag(DILevel.MainProcessRequired);
            HasNotifyIcon = HasMainProcessRequired;
            HasGUI = level.HasFlag(DILevel.GUI);
            HasServerApiClient = level.HasFlag(DILevel.ServerApiClient);
            HasSteam = level.HasFlag(DILevel.Steam);
            HasHttpClientFactory = HasSteam || level.HasFlag(DILevel.HttpClientFactory);
            HasHttpProxy = level.HasFlag(DILevel.HttpProxy);
            HasHosts = level.HasFlag(DILevel.Hosts);
        }

        static StartupOptions? mValue;

        public static StartupOptions Value => mValue ?? throw new NullReferenceException("StartupOptions init fail.");
    }
}

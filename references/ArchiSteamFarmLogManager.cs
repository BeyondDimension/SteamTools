// https://github.com/NLog/NLog/blob/v4.7.10/src/NLog/LogManager.cs
using NLog;
using NLog.Config;
using System;

namespace ArchiSteamFarm
{
    public static class LogManager
    {
        static readonly LogFactory factory = new();

        [CLSCompliant(false)]
        public static Logger GetLogger(string name)
        {
            return factory.GetLogger(name);
        }

        public static void Flush()
        {
            factory.Flush();
        }

        public static void ReconfigExistingLoggers()
        {
            factory.ReconfigExistingLoggers();
        }

        public static LoggingConfiguration Configuration
        {
            get => factory.Configuration;
            set => factory.Configuration = value;
        }

        public static event EventHandler<LoggingConfigurationChangedEventArgs> ConfigurationChanged
        {
            add => factory.ConfigurationChanged += value;
            remove => factory.ConfigurationChanged -= value;
        }

        public static void Shutdown()
        {
            factory.Shutdown();
        }
    }
}
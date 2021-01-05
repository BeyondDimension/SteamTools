using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Win32;
using Titanium.Web.Proxy.Models;

// Helper classes for setting system proxy settings
namespace Titanium.Web.Proxy.Helpers
{
    internal class HttpSystemProxyValue
    {
        internal string HostName { get; }

        internal int Port { get; }

        internal ProxyProtocolType ProtocolType { get; }

        public HttpSystemProxyValue(string hostName, int port, ProxyProtocolType protocolType)
        {
            HostName = hostName;
            Port = port;
            ProtocolType = protocolType;
        }

        public override string ToString()
        {
            string protocol;
            switch (ProtocolType)
            {
                case ProxyProtocolType.Http:
                    protocol = ProxyServer.UriSchemeHttp;
                    break;
                case ProxyProtocolType.Https:
                    protocol = ProxyServer.UriSchemeHttps;
                    break;
                default:
                    throw new Exception("Unsupported protocol type");
            }

            return $"{protocol}={HostName}:{Port}";
        }
    }

    /// <summary>
    ///     Manage system proxy settings
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleType", Justification = "Reviewed.")]
    internal class SystemProxyManager
    {
        private const string regKeyInternetSettings = "Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings";
        private const string regAutoConfigUrl = "AutoConfigURL";
        private const string regProxyEnable = "ProxyEnable";
        private const string regProxyServer = "ProxyServer";
        private const string regProxyOverride = "ProxyOverride";

        internal const int InternetOptionSettingsChanged = 39;
        internal const int InternetOptionRefresh = 37;

        private ProxyInfo? originalValues;

        public SystemProxyManager()
        {
            AppDomain.CurrentDomain.ProcessExit += (o, args) => RestoreOriginalSettings();
            if (Environment.UserInteractive && NativeMethods.GetConsoleWindow() != IntPtr.Zero)
            {
                var handler = new NativeMethods.ConsoleEventDelegate(eventType =>
                {
                    if (eventType != 2)
                    {
                        return false;
                    }

                    RestoreOriginalSettings();
                    return false;
                });
                NativeMethods.Handler = handler;

                // On Console exit make sure we also exit the proxy
                NativeMethods.SetConsoleCtrlHandler(handler, true);
            }
        }

        /// <summary>
        ///     Set the HTTP and/or HTTPS proxy server for current machine
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="protocolType"></param>
        internal void SetProxy(string hostname, int port, ProxyProtocolType protocolType)
        {
            using (var reg = openInternetSettingsKey())
            {
                if (reg == null)
                {
                    return;
                }

                saveOriginalProxyConfiguration(reg);
                prepareRegistry(reg);

                string? existingContent = reg.GetValue(regProxyServer) as string;
                var existingSystemProxyValues = ProxyInfo.GetSystemProxyValues(existingContent);
                existingSystemProxyValues.RemoveAll(x => (protocolType & x.ProtocolType) != 0);
                if ((protocolType & ProxyProtocolType.Http) != 0)
                {
                    existingSystemProxyValues.Add(new HttpSystemProxyValue(hostname, port, ProxyProtocolType.Http));
                }

                if ((protocolType & ProxyProtocolType.Https) != 0)
                {
                    existingSystemProxyValues.Add(new HttpSystemProxyValue(hostname, port, ProxyProtocolType.Https));
                }

                reg.DeleteValue(regAutoConfigUrl, false);
                reg.SetValue(regProxyEnable, 1);
                reg.SetValue(regProxyServer,
                    string.Join(";", existingSystemProxyValues.Select(x => x.ToString()).ToArray()));

                refresh();
            }
        }

        /// <summary>
        ///     Remove the HTTP and/or HTTPS proxy setting from current machine
        /// </summary>
        internal void RemoveProxy(ProxyProtocolType protocolType, bool saveOriginalConfig = true)
        {
            using (var reg = openInternetSettingsKey())
            {
                if (reg == null)
                {
                    return;
                }

                if (saveOriginalConfig)
                {
                    saveOriginalProxyConfiguration(reg);
                }

                if (reg.GetValue(regProxyServer) != null)
                {
                    string? existingContent = reg.GetValue(regProxyServer) as string;

                    var existingSystemProxyValues = ProxyInfo.GetSystemProxyValues(existingContent);
                    existingSystemProxyValues.RemoveAll(x => (protocolType & x.ProtocolType) != 0);

                    if (existingSystemProxyValues.Count != 0)
                    {
                        reg.SetValue(regProxyEnable, 1);
                        reg.SetValue(regProxyServer,
                            string.Join(";", existingSystemProxyValues.Select(x => x.ToString()).ToArray()));
                    }
                    else
                    {
                        reg.SetValue(regProxyEnable, 0);
                        reg.SetValue(regProxyServer, string.Empty);
                    }
                }

                refresh();
            }
        }

        /// <summary>
        ///     Removes all types of proxy settings (both http and https)
        /// </summary>
        internal void DisableAllProxy()
        {
            using (var reg = openInternetSettingsKey())
            {
                if (reg == null)
                {
                    return;
                }

                saveOriginalProxyConfiguration(reg);

                reg.SetValue(regProxyEnable, 0);
                reg.SetValue(regProxyServer, string.Empty);

                refresh();
            }
        }

        internal void SetAutoProxyUrl(string url)
        {
            using (var reg = openInternetSettingsKey())
            {
                if (reg == null)
                {
                    return;
                }

                saveOriginalProxyConfiguration(reg);
                reg.SetValue(regAutoConfigUrl, url);
                refresh();
            }
        }

        internal void SetProxyOverride(string proxyOverride)
        {
            using (var reg = openInternetSettingsKey())
            {
                if (reg == null)
                {
                    return;
                }

                saveOriginalProxyConfiguration(reg);
                reg.SetValue(regProxyOverride, proxyOverride);
                refresh();
            }
        }

        internal void RestoreOriginalSettings()
        {
            if (originalValues == null)
            {
                return;
            }

            using (var reg = Registry.CurrentUser.OpenSubKey(regKeyInternetSettings, true))
            {
                if (reg == null)
                {
                    return;
                }

                var ov = originalValues;
                if (ov.AutoConfigUrl != null)
                {
                    reg.SetValue(regAutoConfigUrl, ov.AutoConfigUrl);
                }
                else
                {
                    reg.DeleteValue(regAutoConfigUrl, false);
                }

                if (ov.ProxyEnable.HasValue)
                {
                    reg.SetValue(regProxyEnable, ov.ProxyEnable.Value);
                }
                else
                {
                    reg.DeleteValue(regProxyEnable, false);
                }

                if (ov.ProxyServer != null)
                {
                    reg.SetValue(regProxyServer, ov.ProxyServer);
                }
                else
                {
                    reg.DeleteValue(regProxyServer, false);
                }

                if (ov.ProxyOverride != null)
                {
                    reg.SetValue(regProxyOverride, ov.ProxyOverride);
                }
                else
                {
                    reg.DeleteValue(regProxyOverride, false);
                }

                // This should not be needed, but sometimes the values are not stored into the registry
                // at system shutdown without flushing.
                reg.Flush();

                originalValues = null;

                const int SM_SHUTTINGDOWN = 0x2000;
                Version windows7Version = new Version(6, 1);
                if (Environment.OSVersion.Version > windows7Version ||
                    NativeMethods.GetSystemMetrics(SM_SHUTTINGDOWN) == 0)
                {
                    // Do not call refresh() in Windows 7 or earlier at system shutdown.
                    // SetInternetOption in the refresh method re-enables ProxyEnable registry value
                    // in Windows 7 or earlier at system shutdown.
                    refresh();
                }
            }
        }

        internal ProxyInfo? GetProxyInfoFromRegistry()
        {
            using (var reg = openInternetSettingsKey())
            {
                if (reg == null)
                {
                    return null;
                }

                return getProxyInfoFromRegistry(reg);
            }
        }

        private ProxyInfo getProxyInfoFromRegistry(RegistryKey reg)
        {
            var pi = new ProxyInfo(null,
                reg.GetValue(regAutoConfigUrl) as string,
                reg.GetValue(regProxyEnable) as int?,
                reg.GetValue(regProxyServer) as string,
                reg.GetValue(regProxyOverride) as string);

            return pi;
        }

        private void saveOriginalProxyConfiguration(RegistryKey reg)
        {
            if (originalValues != null)
            {
                return;
            }

            originalValues = getProxyInfoFromRegistry(reg);
        }

        /// <summary>
        ///     Prepares the proxy server registry (create empty values if they don't exist)
        /// </summary>
        /// <param name="reg"></param>
        private static void prepareRegistry(RegistryKey reg)
        {
            if (reg.GetValue(regProxyEnable) == null)
            {
                reg.SetValue(regProxyEnable, 0);
            }

            if (reg.GetValue(regProxyServer) == null || reg.GetValue(regProxyEnable) as int? == 0)
            {
                reg.SetValue(regProxyServer, string.Empty);
            }
        }

        /// <summary>
        ///     Refresh the settings so that the system know about a change in proxy setting
        /// </summary>
        private static void refresh()
        {
            NativeMethods.InternetSetOption(IntPtr.Zero, InternetOptionSettingsChanged, IntPtr.Zero, 0);
            NativeMethods.InternetSetOption(IntPtr.Zero, InternetOptionRefresh, IntPtr.Zero, 0);
        }

        /// <summary>
        ///     Opens the registry key with the internet settings
        /// </summary>
        private static RegistryKey? openInternetSettingsKey()
        {
            return Registry.CurrentUser?.OpenSubKey(regKeyInternetSettings, true);
        }
    }
}

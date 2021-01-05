using System;
using System.Net;
using System.ServiceProcess;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.Exceptions;
using Titanium.Web.Proxy.Models;

namespace WindowsServiceExample
{
    partial class ProxyService : ServiceBase
    {
        private static ProxyServer _proxyServerInstance;

        public ProxyService()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.UnhandledException += UnhandledDomainException;
        }

        protected override void OnStart(string[] args)
        {
            // we do all this in here so we can reload settings with a simple restart

            _proxyServerInstance = new ProxyServer(false);

            if (Properties.Settings.Default.ListeningPort <= 0 ||
                Properties.Settings.Default.ListeningPort > 65535)
                throw new Exception("Invalid listening port");

            _proxyServerInstance.CheckCertificateRevocation = Properties.Settings.Default.CheckCertificateRevocation;
            _proxyServerInstance.ConnectionTimeOutSeconds = Properties.Settings.Default.ConnectionTimeOutSeconds;
            _proxyServerInstance.Enable100ContinueBehaviour = Properties.Settings.Default.Enable100ContinueBehaviour;
            _proxyServerInstance.EnableConnectionPool = Properties.Settings.Default.EnableConnectionPool;
            _proxyServerInstance.EnableTcpServerConnectionPrefetch = Properties.Settings.Default.EnableTcpServerConnectionPrefetch;
            _proxyServerInstance.EnableWinAuth = Properties.Settings.Default.EnableWinAuth;
            _proxyServerInstance.ForwardToUpstreamGateway = Properties.Settings.Default.ForwardToUpstreamGateway;
            _proxyServerInstance.MaxCachedConnections = Properties.Settings.Default.MaxCachedConnections;
            _proxyServerInstance.ReuseSocket = Properties.Settings.Default.ReuseSocket;
            _proxyServerInstance.TcpTimeWaitSeconds = Properties.Settings.Default.TcpTimeWaitSeconds;
            _proxyServerInstance.CertificateManager.SaveFakeCertificates = Properties.Settings.Default.SaveFakeCertificates;
            _proxyServerInstance.EnableHttp2 = Properties.Settings.Default.EnableHttp2;
            _proxyServerInstance.NoDelay = Properties.Settings.Default.NoDelay;

            if (Properties.Settings.Default.ThreadPoolWorkerThreads < 0)
            {
                _proxyServerInstance.ThreadPoolWorkerThread = Environment.ProcessorCount;
            }
            else
            {
                _proxyServerInstance.ThreadPoolWorkerThread = Properties.Settings.Default.ThreadPoolWorkerThreads;
            }

            if (Properties.Settings.Default.ThreadPoolWorkerThreads < Environment.ProcessorCount)
            {
                ProxyServiceEventLog.WriteEntry(
                    $"Worker thread count of {Properties.Settings.Default.ThreadPoolWorkerThreads} is below the " +
                    $"processor count of {Environment.ProcessorCount}. This may be on purpose.", System.Diagnostics.EventLogEntryType.Warning);
            }

            var explicitEndPointV4 = new ExplicitProxyEndPoint(IPAddress.Any, Properties.Settings.Default.ListeningPort, Properties.Settings.Default.DecryptSsl);

            _proxyServerInstance.AddEndPoint(explicitEndPointV4);

            if (Properties.Settings.Default.EnableIpV6)
            {
                var explicitEndPointV6 = new ExplicitProxyEndPoint(IPAddress.IPv6Any, Properties.Settings.Default.ListeningPort, Properties.Settings.Default.DecryptSsl);

                _proxyServerInstance.AddEndPoint(explicitEndPointV6);
            }

            if (Properties.Settings.Default.LogErrors)
                _proxyServerInstance.ExceptionFunc = ProxyException;

            _proxyServerInstance.Start();

            ProxyServiceEventLog.WriteEntry($"Service Listening on port {Properties.Settings.Default.ListeningPort}", System.Diagnostics.EventLogEntryType.Information);
        }

        protected override void OnStop()
        {
            _proxyServerInstance.Stop();

            // clean up here since we make a new instance when starting
            _proxyServerInstance.Dispose();
        }

        private void ProxyException(Exception exception)
        {
            string message;
            if (exception is ProxyHttpException pEx)
            {
                message = $"Unhandled Proxy Exception in ProxyServer, UserData = {pEx.Session?.UserData}, URL = {pEx.Session?.HttpClient.Request.RequestUri} Exception = {pEx}";
            }
            else
            {
                message = $"Unhandled Exception in ProxyServer, Exception = {exception}";
            }

            ProxyServiceEventLog.WriteEntry(message, System.Diagnostics.EventLogEntryType.Error);
        }

        private void UnhandledDomainException(object sender, UnhandledExceptionEventArgs e)
        {
            ProxyServiceEventLog.WriteEntry($"Unhandled Exception in AppDomain, Exception = {e}", System.Diagnostics.EventLogEntryType.Error);
        }
    }
}

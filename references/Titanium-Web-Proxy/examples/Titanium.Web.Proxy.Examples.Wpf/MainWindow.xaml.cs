using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.StreamExtended.Network;

namespace Titanium.Web.Proxy.Examples.Wpf
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly DependencyProperty ClientConnectionCountProperty = DependencyProperty.Register(
            nameof(ClientConnectionCount), typeof(int), typeof(MainWindow), new PropertyMetadata(default(int)));

        public static readonly DependencyProperty ServerConnectionCountProperty = DependencyProperty.Register(
            nameof(ServerConnectionCount), typeof(int), typeof(MainWindow), new PropertyMetadata(default(int)));

        private readonly ProxyServer proxyServer;

        private readonly Dictionary<HttpWebClient, SessionListItem> sessionDictionary =
            new Dictionary<HttpWebClient, SessionListItem>();

        private int lastSessionNumber;
        private SessionListItem selectedSession;

        public MainWindow()
        {
            proxyServer = new ProxyServer();

            //proxyServer.EnableHttp2 = true;

            //proxyServer.CertificateManager.CertificateEngine = CertificateEngine.DefaultWindows;

            ////Set a password for the .pfx file
            //proxyServer.CertificateManager.PfxPassword = "PfxPassword";

            ////Set Name(path) of the Root certificate file
            //proxyServer.CertificateManager.PfxFilePath = @"C:\NameFolder\rootCert.pfx";

            ////do you want Replace an existing Root certificate file(.pfx) if password is incorrect(RootCertificate=null)?  yes====>true
            //proxyServer.CertificateManager.OverwritePfxFile = true;

            ////save all fake certificates in folder "crts"(will be created in proxy dll directory)
            ////if create new Root certificate file(.pfx) ====> delete folder "crts"
            //proxyServer.CertificateManager.SaveFakeCertificates = true;

            proxyServer.ForwardToUpstreamGateway = true;

            //increase the ThreadPool (for server prod)
            //proxyServer.ThreadPoolWorkerThread = Environment.ProcessorCount * 6;

            ////if you need Load or Create Certificate now. ////// "true" if you need Enable===> Trust the RootCertificate used by this proxy server
            //proxyServer.CertificateManager.EnsureRootCertificate(true);

            ////or load directly certificate(As Administrator if need this)
            ////and At the same time chose path and password
            ////if password is incorrect and (overwriteRootCert=true)(RootCertificate=null) ====> replace an existing .pfx file
            ////note : load now (if existed)
            //proxyServer.CertificateManager.LoadRootCertificate(@"C:\NameFolder\rootCert.pfx", "PfxPassword");

            //var explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Any, 8000, true);

            //proxyServer.AddEndPoint(explicitEndPoint);
            var transparentEndPoint = new TransparentProxyEndPoint(IPAddress.Any, 443, true)
            {
            };

            proxyServer.AddEndPoint(transparentEndPoint);
            //proxyServer.UpStreamHttpProxy = new ExternalProxy
            //{
            //    HostName = "158.69.115.45",
            //    Port = 3128,
            //    UserName = "Titanium",
            //    Password = "Titanium",
            //};

            //var socksEndPoint = new SocksProxyEndPoint(IPAddress.Any, 1080, true)
            //{
            //    // Generic Certificate hostname to use
            //    // When SNI is disabled by client
            //    //GenericCertificateName = "google.com"
            //};

            //proxyServer.AddEndPoint(socksEndPoint);

            proxyServer.BeforeRequest += ProxyServer_BeforeRequest;
            proxyServer.BeforeResponse += ProxyServer_BeforeResponse;
            proxyServer.AfterResponse += ProxyServer_AfterResponse;
            //explicitEndPoint.BeforeTunnelConnectRequest += ProxyServer_BeforeTunnelConnectRequest;
            //explicitEndPoint.BeforeTunnelConnectResponse += ProxyServer_BeforeTunnelConnectResponse;
            proxyServer.ClientConnectionCountChanged += delegate
            {
                Dispatcher.Invoke(() => { ClientConnectionCount = proxyServer.ClientConnectionCount; });
            };
            proxyServer.ServerConnectionCountChanged += delegate
            {
                Dispatcher.Invoke(() => { ServerConnectionCount = proxyServer.ServerConnectionCount; });
            };
            proxyServer.Start();

            //proxyServer.SetAsSystemProxy(explicitEndPoint, ProxyProtocolType.AllHttp);

            InitializeComponent();
        }

        public ObservableCollectionEx<SessionListItem> Sessions { get; } = new ObservableCollectionEx<SessionListItem>();

        public SessionListItem SelectedSession
        {
            get => selectedSession;
            set
            {
                if (value != selectedSession)
                {
                    selectedSession = value;
                    selectedSessionChanged();
                }
            }
        }

        public int ClientConnectionCount
        {
            get => (int)GetValue(ClientConnectionCountProperty);
            set => SetValue(ClientConnectionCountProperty, value);
        }

        public int ServerConnectionCount
        {
            get => (int)GetValue(ServerConnectionCountProperty);
            set => SetValue(ServerConnectionCountProperty, value);
        }

        private async Task ProxyServer_BeforeTunnelConnectRequest(object sender, TunnelConnectSessionEventArgs e)
        {
            string hostname = e.HttpClient.Request.RequestUri.Host;
            if (hostname.EndsWith("webex.com"))
            {
                e.DecryptSsl = false;
            }

            await Dispatcher.InvokeAsync(() => { addSession(e); });
        }

        private async Task ProxyServer_BeforeTunnelConnectResponse(object sender, TunnelConnectSessionEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                if (sessionDictionary.TryGetValue(e.HttpClient, out var item))
                {
                    item.Update(e);
                }
            });
        }

        private async Task ProxyServer_BeforeRequest(object sender, SessionEventArgs e)
        {
            //if (e.HttpClient.Request.HttpVersion.Major != 2) return;
            if (e.HttpClient.Request.RequestUri.AbsoluteUri.Contains("steamcommunity.com"))
            {
                var ip = Dns.GetHostAddresses("steamcommunity-a.akamaihd.net");
                e.HttpClient.UpStreamEndPoint = new IPEndPoint(IPAddress.Parse(ip[0].ToString()), 443);
                if (e.HttpClient.ConnectRequest?.ClientHelloInfo != null)
                {
                    e.HttpClient.ConnectRequest.ClientHelloInfo.Extensions.Remove("server_name");
                }
            }
            if (e.HttpClient.ConnectRequest?.TunnelType == TunnelType.Websocket)
            {
                e.DataSent += WebSocket_DataSent;
            }
            SessionListItem item = null;
            await Dispatcher.InvokeAsync(() => { item = addSession(e); });

            if (e.HttpClient.Request.HasBody)
            {
                e.HttpClient.Request.KeepBody = true;
                await e.GetRequestBody();

                if (item == SelectedSession)
                {
                    await Dispatcher.InvokeAsync(selectedSessionChanged);
                }
            }
        }
        private void WebSocket_DataSent(object sender, DataEventArgs e)
        {
            var args = (SessionEventArgs)sender;

        }
        private async Task ProxyServer_BeforeResponse(object sender, SessionEventArgs e)
        {

            SessionListItem item = null;
            await Dispatcher.InvokeAsync(() =>
            {
                if (sessionDictionary.TryGetValue(e.HttpClient, out item))
                {
                    item.Update(e);
                }
            });

            //e.HttpClient.Response.Headers.AddHeader("X-Titanium-Header", "HTTP/2 works");

            //e.SetResponseBody(Encoding.ASCII.GetBytes("TITANIUMMMM!!!!"));

            if (item != null)
            {
                if (e.HttpClient.Response.HasBody)
                {
                    e.HttpClient.Response.KeepBody = true;
                    await e.GetResponseBody();

                    await Dispatcher.InvokeAsync(() => { item.Update(e); });
                    if (item == SelectedSession)
                    {
                        await Dispatcher.InvokeAsync(selectedSessionChanged);
                    }
                }
            }
        }

        private async Task ProxyServer_AfterResponse(object sender, SessionEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                if (sessionDictionary.TryGetValue(e.HttpClient, out var item))
                {
                    item.Exception = e.Exception;
                }
            });
        }

        private SessionListItem addSession(SessionEventArgsBase e)
        {
            var item = createSessionListItem(e);
            Sessions.Add(item);
            sessionDictionary.Add(e.HttpClient, item);
            return item;
        }

        private SessionListItem createSessionListItem(SessionEventArgsBase e)
        {
            lastSessionNumber++;
            bool isTunnelConnect = e is TunnelConnectSessionEventArgs;
            var item = new SessionListItem
            {
                Number = lastSessionNumber,
                ClientConnectionId = e.ClientConnectionId,
                ServerConnectionId = e.ServerConnectionId,
                HttpClient = e.HttpClient,
                ClientRemoteEndPoint = e.ClientRemoteEndPoint,
                ClientLocalEndPoint = e.ClientLocalEndPoint,
                IsTunnelConnect = isTunnelConnect
            };

            //if (isTunnelConnect || e.HttpClient.Request.UpgradeToWebSocket)
            e.DataReceived += (sender, args) =>
            {
                var session = (SessionEventArgsBase)sender;
                if (sessionDictionary.TryGetValue(session.HttpClient, out var li))
                {
                    var connectRequest = session.HttpClient.ConnectRequest;
                    var tunnelType = connectRequest?.TunnelType ?? TunnelType.Unknown;
                    if (tunnelType != TunnelType.Unknown)
                    {
                        li.Protocol = TunnelTypeToString(tunnelType);
                    }

                    li.ReceivedDataCount += args.Count;

                    //if (tunnelType == TunnelType.Http2)
                    AppendTransferLog(session.GetHashCode() + (isTunnelConnect ? "_tunnel" : "") + "_received",
                        args.Buffer, args.Offset, args.Count);
                }
            };

            e.DataSent += (sender, args) =>
            {
                var session = (SessionEventArgsBase)sender;
                if (sessionDictionary.TryGetValue(session.HttpClient, out var li))
                {
                    var connectRequest = session.HttpClient.ConnectRequest;
                    var tunnelType = connectRequest?.TunnelType ?? TunnelType.Unknown;
                    if (tunnelType != TunnelType.Unknown)
                    {
                        li.Protocol = TunnelTypeToString(tunnelType);
                    }

                    li.SentDataCount += args.Count;

                    //if (tunnelType == TunnelType.Http2)
                    AppendTransferLog(session.GetHashCode() + (isTunnelConnect ? "_tunnel" : "") + "_sent",
                        args.Buffer, args.Offset, args.Count);
                }
            };

            if (e is TunnelConnectSessionEventArgs te)
            {
                te.DecryptedDataReceived += (sender, args) =>
                {
                    var session = (SessionEventArgsBase)sender;
                    //var tunnelType = session.HttpClient.ConnectRequest?.TunnelType ?? TunnelType.Unknown;
                    //if (tunnelType == TunnelType.Http2)
                    AppendTransferLog(session.GetHashCode() + "_decrypted_received", args.Buffer, args.Offset,
                        args.Count);
                };

                te.DecryptedDataSent += (sender, args) =>
                {
                    var session = (SessionEventArgsBase)sender;
                    //var tunnelType = session.HttpClient.ConnectRequest?.TunnelType ?? TunnelType.Unknown;
                    //if (tunnelType == TunnelType.Http2)
                    AppendTransferLog(session.GetHashCode() + "_decrypted_sent", args.Buffer, args.Offset, args.Count);
                };
            }

            item.Update(e);
            return item;
        }

        private void AppendTransferLog(string fileName, byte[] buffer, int offset, int count)
        {
            //string basePath = @"c:\!titanium\";
            //using (var fs = new FileStream(basePath + fileName, FileMode.Append, FileAccess.Write, FileShare.Read))
            //{
            //    fs.Write(buffer, offset, count);
            //}
        }

        private string TunnelTypeToString(TunnelType tunnelType)
        {
            switch (tunnelType)
            {
                case TunnelType.Https:
                    return "https";
                case TunnelType.Websocket:
                    return "websocket";
                case TunnelType.Http2:
                    return "http2";
            }

            return null;
        }

        private void ListViewSessions_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                bool isSelected = false;
                var selectedItems = ((ListView)sender).SelectedItems;
                Sessions.SuppressNotification = true;
                foreach (var item in selectedItems.Cast<SessionListItem>().ToArray())
                {
                    if (item == SelectedSession)
                    {
                        isSelected = true;
                    }

                    Sessions.Remove(item);
                    sessionDictionary.Remove(item.HttpClient);
                }

                Sessions.SuppressNotification = false;

                if (isSelected)
                {
                    SelectedSession = null;
                }
            }
        }

        private void selectedSessionChanged()
        {
            if (SelectedSession == null)
            {
                TextBoxRequest.Text = null;
                TextBoxResponse.Text = string.Empty;
                ImageResponse.Source = null;
                return;
            }

            const int truncateLimit = 1024;

            var session = SelectedSession.HttpClient;
            var request = session.Request;
            var fullData = (request.IsBodyRead ? request.Body : null) ?? Array.Empty<byte>();
            var data = fullData;
            bool truncated = data.Length > truncateLimit;
            if (truncated)
            {
                data = data.Take(truncateLimit).ToArray();
            }

            //string hexStr = string.Join(" ", data.Select(x => x.ToString("X2")));
            var sb = new StringBuilder();
            sb.AppendLine("URI: " + request.RequestUri);
            sb.Append(request.HeaderText);
            sb.Append(request.Encoding.GetString(data));
            if (truncated)
            {
                sb.AppendLine();
                sb.Append($"Data is truncated after {truncateLimit} bytes");
            }

            sb.Append((request as ConnectRequest)?.ClientHelloInfo);
            TextBoxRequest.Text = sb.ToString();

            var response = session.Response;
            fullData = (response.IsBodyRead ? response.Body : null) ?? Array.Empty<byte>();
            data = fullData;
            truncated = data.Length > truncateLimit;
            if (truncated)
            {
                data = data.Take(truncateLimit).ToArray();
            }

            //hexStr = string.Join(" ", data.Select(x => x.ToString("X2")));
            sb = new StringBuilder();
            sb.Append(response.HeaderText);
            sb.Append(response.Encoding.GetString(data));
            if (truncated)
            {
                sb.AppendLine();
                sb.Append($"Data is truncated after {truncateLimit} bytes");
            }

            sb.Append((response as ConnectResponse)?.ServerHelloInfo);
            if (SelectedSession.Exception != null)
            {
                sb.Append(Environment.NewLine);
                sb.Append(SelectedSession.Exception);
            }

            TextBoxResponse.Text = sb.ToString();

            try
            {
                if (fullData.Length > 0)
                {
                    using (var stream = new MemoryStream(fullData))
                    {
                        ImageResponse.Source =
                            BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                }
            }
            catch
            {
                ImageResponse.Source = null;
            }
        }

        private void ButtonProxyOnOff_OnClick(object sender, RoutedEventArgs e)
        {
            var button = (ToggleButton)sender;
            if (button.IsChecked == true)
            {
                proxyServer.SetAsSystemProxy((ExplicitProxyEndPoint)proxyServer.ProxyEndPoints[0],
                    ProxyProtocolType.AllHttp);
            }
            else
            {
                proxyServer.RestoreOriginalProxySettings();
            }
        }
    }
}

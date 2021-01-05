using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Examples.Wpf.Annotations;
using Titanium.Web.Proxy.Http;

namespace Titanium.Web.Proxy.Examples.Wpf
{
    public class SessionListItem : INotifyPropertyChanged
    {
        private long? bodySize;
        private Exception exception;
        private string host;
        private int processId;
        private string protocol;
        private long receivedDataCount;
        private long sentDataCount;
        private string statusCode;
        private string url;
        private Guid clientConnectionId;
        private Guid serverConnectionId;

        public int Number { get; set; }

        public Guid ClientConnectionId
        {
            get => clientConnectionId;
            set => SetField(ref clientConnectionId, value);
        }

        public Guid ServerConnectionId
        {
            get => serverConnectionId;
            set => SetField(ref serverConnectionId, value);
        }

        public HttpWebClient HttpClient { get; set; }

        public IPEndPoint ClientLocalEndPoint { get; set; }

        public IPEndPoint ClientRemoteEndPoint { get; set; }

        public bool IsTunnelConnect { get; set; }

        public string StatusCode
        {
            get => statusCode;
            set => SetField(ref statusCode, value);
        }

        public string Protocol
        {
            get => protocol;
            set => SetField(ref protocol, value);
        }

        public string Host
        {
            get => host;
            set => SetField(ref host, value);
        }

        public string Url
        {
            get => url;
            set => SetField(ref url, value);
        }

        public long? BodySize
        {
            get => bodySize;
            set => SetField(ref bodySize, value);
        }

        public int ProcessId
        {
            get => processId;
            set
            {
                if (SetField(ref processId, value))
                {
                    OnPropertyChanged(nameof(Process));
                }
            }
        }

        public string Process
        {
            get
            {
                try
                {
                    var process = System.Diagnostics.Process.GetProcessById(processId);
                    return process.ProcessName + ":" + processId;
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
        }

        public long ReceivedDataCount
        {
            get => receivedDataCount;
            set => SetField(ref receivedDataCount, value);
        }

        public long SentDataCount
        {
            get => sentDataCount;
            set => SetField(ref sentDataCount, value);
        }

        public Exception Exception
        {
            get => exception;
            set => SetField(ref exception, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, value))
            {
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }

            return false;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Update(SessionEventArgsBase args)
        {
            var request = HttpClient.Request;
            var response = HttpClient.Response;
            int statusCode = response?.StatusCode ?? 0;
            StatusCode = statusCode == 0 ? "-" : statusCode.ToString();
            Protocol = request.RequestUri.Scheme;
            ClientConnectionId = args.ClientConnectionId;
            ServerConnectionId = args.ServerConnectionId;

            if (IsTunnelConnect)
            {
                Host = "Tunnel to";
                Url = request.RequestUri.Host + ":" + request.RequestUri.Port;
            }
            else
            {
                Host = request.RequestUri.Host;
                Url = request.RequestUri.AbsolutePath;
            }

            if (!IsTunnelConnect)
            {
                long responseSize = -1;
                if (response != null)
                {
                    if (response.ContentLength != -1)
                    {
                        responseSize = response.ContentLength;
                    }
                    else if (response.IsBodyRead && response.Body != null)
                    {
                        responseSize = response.Body.Length;
                    }
                }

                BodySize = responseSize;
            }

            ProcessId = HttpClient.ProcessId.Value;
        }
    }
}

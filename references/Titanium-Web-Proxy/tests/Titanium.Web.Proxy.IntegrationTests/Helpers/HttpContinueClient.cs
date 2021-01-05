using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Titanium.Web.Proxy.Helpers;
using Titanium.Web.Proxy.Http;

namespace Titanium.Web.Proxy.IntegrationTests.Helpers
{
    class HttpContinueClient
    {
        private const int waitTimeout = 200;

        private static Encoding MsgEncoding = HttpHelper.GetEncodingFromContentType(null);

        public async Task<Response> Post(string server, int port, string content)
        {
            var message = MsgEncoding.GetBytes(content);
            var client = new TcpClient(server, port);
            client.SendTimeout = client.ReceiveTimeout = 500;

            var request = new Request
            {
                Method = "POST",
                RequestUriString = "/",
                HttpVersion = new Version(1, 1)
            };
            request.Headers.AddHeader(KnownHeaders.Host, server);
            request.Headers.AddHeader(KnownHeaders.ContentLength, message.Length.ToString());
            request.Headers.AddHeader(KnownHeaders.Expect, KnownHeaders.Expect100Continue);

            var header = MsgEncoding.GetBytes(request.HeaderText);
            await client.GetStream().WriteAsync(header, 0, header.Length);

            var buffer = new byte[1024];
            var responseMsg = string.Empty;
            Response response;

            while ((response = HttpMessageParsing.ParseResponse(responseMsg)) == null)
            {
                var readTask = client.GetStream().ReadAsync(buffer, 0, 1024);
                if (!readTask.Wait(waitTimeout))
                    return null;

                responseMsg += MsgEncoding.GetString(buffer, 0, readTask.Result);
            }

            if (response.StatusCode == 100)
            {
                await client.GetStream().WriteAsync(message);

                responseMsg = string.Empty;

                while ((response = HttpMessageParsing.ParseResponse(responseMsg)) == null)
                {
                    var readTask = client.GetStream().ReadAsync(buffer, 0, 1024);
                    if (!readTask.Wait(waitTimeout))
                        return null;
                    responseMsg += MsgEncoding.GetString(buffer, 0, readTask.Result);
                }

                return response;
            }

            return response;
        }
    }
}

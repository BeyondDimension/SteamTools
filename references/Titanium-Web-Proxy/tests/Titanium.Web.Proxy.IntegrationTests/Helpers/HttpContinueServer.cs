using System;
using System.IO.Pipelines;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Titanium.Web.Proxy.Helpers;
using Titanium.Web.Proxy.Http;

namespace Titanium.Web.Proxy.IntegrationTests.Helpers
{
    class HttpContinueServer
    {
        public HttpStatusCode ExpectationResponse;
        public string ResponseBody;

        private static Encoding MsgEncoding = HttpHelper.GetEncodingFromContentType(null);

        public async Task HandleRequest(ConnectionContext context)
        {
            var request = await ReadHeaders(context.Transport.Input);

            if (request.ExpectContinue)
            {
                var respondContinue = new Response
                {
                    HttpVersion = request.HttpVersion,
                    StatusCode = (int)ExpectationResponse,
                    StatusDescription = ExpectationResponse.ToString()
                };
                await context.Transport.Output.WriteAsync(MsgEncoding.GetBytes(respondContinue.HeaderText));

                if (ExpectationResponse != HttpStatusCode.Continue)
                    return;
            }

            request = await ReadBody(request, context.Transport.Input);

            var responseMsg = MsgEncoding.GetBytes(ResponseBody);
            var respondOk = new Response(responseMsg)
            {
                HttpVersion = new Version(1, 1),
                StatusCode = (int)HttpStatusCode.OK,
                StatusDescription = HttpStatusCode.OK.ToString()
            };
            await context.Transport.Output.WriteAsync(MsgEncoding.GetBytes(respondOk.HeaderText));
            await context.Transport.Output.WriteAsync(responseMsg);
            context.Transport.Output.Complete();
        }

        private async Task<Request> ReadHeaders(PipeReader input)
        {
            Request request = null;
            try
            {
                var requestMsg = string.Empty;
                while ((request = HttpMessageParsing.ParseRequest(requestMsg, false)) == null)
                {
                    var result = await input.ReadAsync();
                    foreach (var seg in result.Buffer)
                        requestMsg += MsgEncoding.GetString(seg.Span);
                    input.AdvanceTo(result.Buffer.End);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.GetType()}: {ex.Message}");
            }

            return request;
        }

        private async Task<Request> ReadBody(Request request, PipeReader input)
        {
            var msg = request.HeaderText;
            try
            {

                while ((request = HttpMessageParsing.ParseRequest(msg, true)) == null)
                {
                    var result = await input.ReadAsync();
                    foreach (var seg in result.Buffer)
                        msg += MsgEncoding.GetString(seg.Span);
                    input.AdvanceTo(result.Buffer.End);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.GetType()}: {ex.Message}");
            }

            return request;
        }
    }
}

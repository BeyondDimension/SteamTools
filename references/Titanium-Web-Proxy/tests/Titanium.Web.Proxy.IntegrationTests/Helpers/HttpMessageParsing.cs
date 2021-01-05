using System.IO;
using System.Text;
using Titanium.Web.Proxy.Http;

namespace Titanium.Web.Proxy.IntegrationTests.Helpers
{
    internal static class HttpMessageParsing
    {
        private static readonly char[] colonSplit = { ':' };

        /// <summary>
        /// This is a terribly inefficient way of reading & parsing an
        /// http request, but it's good enough for testing purposes.
        /// </summary>
        /// <param name="messageText">The request message</param>
        /// <param name="requireBody"></param>
        /// <returns>Request object if message complete, null otherwise</returns>
        internal static Request ParseRequest(string messageText, bool requireBody)
        {
            var reader = new StringReader(messageText);
            var line = reader.ReadLine();
            if (string.IsNullOrEmpty(line))
                return null;

            try
            {
                Request.ParseRequestLine(line, out var method, out var url, out var version);
                var request = new Request
                {
                    Method = method,
                    RequestUriString8 = url,
                    HttpVersion = version
                };
                while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                {
                    var header = line.Split(colonSplit, 2);
                    request.Headers.AddHeader(header[0].Trim(), header[1].Trim());
                }

                // First zero-length line denotes end of headers. If we
                // didn't get one, then we're not done with request
                if (line?.Length != 0)
                    return null;

                if (!requireBody)
                    return request;

                if (parseBody(reader, request))
                    return request;
            }
            catch
            {
                // ignore
            }

            return null;
        }

        /// <summary>
        /// This is a terribly inefficient way of reading & parsing an
        /// http response, but it's good enough for testing purposes.
        /// </summary>
        /// <param name="messageText">The response message</param>
        /// <returns>Response object if message complete, null otherwise</returns>
        internal static Response ParseResponse(string messageText)
        {
            var reader = new StringReader(messageText);
            var line = reader.ReadLine();
            if (string.IsNullOrEmpty(line))
                return null;

            try
            {
                Response.ParseResponseLine(line, out var version, out var status, out var desc);
                var response = new Response
                {
                    HttpVersion = version,
                    StatusCode = status,
                    StatusDescription = desc
                };

                while (!string.IsNullOrEmpty(line = reader.ReadLine()))
                {
                    var header = line.Split(colonSplit, 2);
                    response.Headers.AddHeader(header[0], header[1]);
                }

                // First zero-length line denotes end of headers. If we
                // didn't get one, then we're not done with response
                if (line?.Length != 0)
                    return null;

                if (parseBody(reader, response))
                    return response;
            }
            catch
            {
                // ignore
            }

            return null;
        }

        private static bool parseBody(StringReader reader, RequestResponseBase obj)
        {
            obj.OriginalContentLength = obj.ContentLength;
            if (obj.ContentLength <= 0)
            {
                // no body, done
                return true;
            }

            obj.Body = Encoding.ASCII.GetBytes(reader.ReadToEnd());

            // done reading body
            return obj.ContentLength == obj.OriginalContentLength;
        }
    }
}

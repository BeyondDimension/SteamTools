using System;
using System.Threading;
using System.Threading.Tasks;
using Titanium.Web.Proxy.StreamExtended.Network;

namespace Titanium.Web.Proxy.Http
{
    internal static class HeaderParser
    {
        internal static async ValueTask ReadHeaders(ILineStream reader, HeaderCollection headerCollection,
            CancellationToken cancellationToken)
        {
            string? tmpLine;
            while (!string.IsNullOrEmpty(tmpLine = await reader.ReadLineAsync(cancellationToken)))
            {
                int colonIndex = tmpLine!.IndexOf(':');
                if (colonIndex == -1)
                {
                    throw new Exception("Header line should contain a colon character.");
                }

                string headerName = tmpLine.AsSpan(0, colonIndex).ToString();
                string headerValue = tmpLine.AsSpan(colonIndex + 1).TrimStart().ToString();
                headerCollection.AddHeader(headerName, headerValue);
            }
        }
    }
}

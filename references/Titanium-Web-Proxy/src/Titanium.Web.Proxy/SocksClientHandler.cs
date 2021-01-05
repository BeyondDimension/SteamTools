using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Titanium.Web.Proxy.Extensions;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy.Network.Tcp;

namespace Titanium.Web.Proxy
{
    public partial class ProxyServer
    {
        /// <summary>
        ///     This is called when this proxy acts as a reverse proxy (like a real http server).
        ///     So for HTTPS requests we would start SSL negotiation right away without expecting a CONNECT request from client
        /// </summary>
        /// <param name="endPoint">The transparent endpoint.</param>
        /// <param name="clientConnection">The client connection.</param>
        /// <returns></returns>
        private async Task handleClient(SocksProxyEndPoint endPoint, TcpClientConnection clientConnection)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var stream = clientConnection.GetStream();
            var buffer = BufferPool.GetBuffer();
            int port = 0;
            try
            {
                int read = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (read < 3)
                {
                    return;
                }

                if (buffer[0] == 4)
                {
                    if (read < 9 || buffer[1] != 1)
                    {
                        // not a connect request
                        return;
                    }

                    port = (buffer[2] << 8) + buffer[3];

                    buffer[0] = 0;
                    buffer[1] = 90; // request granted
                    await stream.WriteAsync(buffer, 0, 8, cancellationToken);
                }
                else if (buffer[0] == 5)
                {
                    int authenticationMethodCount = buffer[1];
                    if (read < authenticationMethodCount + 2)
                    {
                        return;
                    }

                    int acceptedMethod = 255;
                    for (int i = 0; i < authenticationMethodCount; i++)
                    {
                        int method = buffer[i + 2];
                        if (method == 0 && ProxyBasicAuthenticateFunc == null)
                        {
                            acceptedMethod = 0;
                            break;
                        }

                        if (method == 2)
                        {
                            acceptedMethod = 2;
                            break;
                        }
                    }

                    buffer[1] = (byte)acceptedMethod;
                    await stream.WriteAsync(buffer, 0, 2, cancellationToken);

                    if (acceptedMethod == 255)
                    {
                        // no acceptable method
                        return;
                    }

                    if (acceptedMethod == 2)
                    {
                        read = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                        if (read < 3 || buffer[0] != 1)
                        {
                            // authentication version should be 1
                            return;
                        }

                        int userNameLength = buffer[1];
                        if (read < 3 + userNameLength)
                        {
                            return;
                        }

                        string userName = Encoding.ASCII.GetString(buffer, 2, userNameLength);

                        int passwordLength = buffer[2 + userNameLength];
                        if (read < 3 + userNameLength + passwordLength)
                        {
                            return;
                        }

                        string password = Encoding.ASCII.GetString(buffer, 3 + userNameLength, passwordLength);
                        bool success = true;
                        if (ProxySchemeAuthenticateFunc != null)
                        {
                            success = await ProxyBasicAuthenticateFunc.Invoke(null, userName, password);
                        }

                        buffer[1] = success ? (byte)0 : (byte)1;
                        await stream.WriteAsync(buffer, 0, 2, cancellationToken);
                        if (!success)
                        {
                            return;
                        }
                    }

                    read = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (read < 10 || buffer[1] != 1)
                    {
                        return;
                    }

                    int portIdx;
                    switch (buffer[3])
                    {
                        case 1:
                            // IPv4
                            portIdx = 8;
                            break;
                        case 3:
                            // Domainname
                            portIdx = buffer[4] + 5;

#if DEBUG
                            var hostname = new ByteString(buffer.AsMemory(5, buffer[4]));
                            string hostnameStr = hostname.GetString();
#endif
                            break;
                        case 4:
                            // IPv6
                            portIdx = 20;
                            break;
                        default:
                            return;
                    }

                    if (read < portIdx + 2)
                    {
                        return;
                    }

                    port = (buffer[portIdx] << 8) + buffer[portIdx + 1];
                    buffer[1] = 0; // succeeded
                    await stream.WriteAsync(buffer, 0, read, cancellationToken);
                }
                else
                {
                    return;
                }
            }
            finally
            {
                BufferPool.ReturnBuffer(buffer);
            }

            await handleClient(endPoint, clientConnection, port, cancellationTokenSource, cancellationToken);
        }
    }
}

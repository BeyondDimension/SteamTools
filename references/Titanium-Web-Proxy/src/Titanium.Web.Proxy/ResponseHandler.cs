using System;
using System.Net;
using System.Threading.Tasks;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Extensions;
using Titanium.Web.Proxy.Network.WinAuth.Security;

namespace Titanium.Web.Proxy
{
    /// <summary>
    ///     Handle the response from server.
    /// </summary>
    public partial class ProxyServer
    {
        /// <summary>
        ///     Called asynchronously when a request was successful and we received the response.
        /// </summary>
        /// <param name="args">The session event arguments.</param>
        /// <returns> The task.</returns>
        private async Task handleHttpSessionResponse(SessionEventArgs args)
        {
            var cancellationToken = args.CancellationTokenSource.Token;

            // read response & headers from server
            await args.HttpClient.ReceiveResponse(cancellationToken);

            // Server may send expect-continue even if not asked for it in request.
            // According to spec "the client can simply discard this interim response."
            if (args.HttpClient.Response.StatusCode == (int)HttpStatusCode.Continue)
            {
                await args.ClearResponse(cancellationToken);
                await args.HttpClient.ReceiveResponse(cancellationToken);
            }

            args.TimeLine["Response Received"] = DateTime.UtcNow;

            var response = args.HttpClient.Response;
            args.ReRequest = false;

            // check for windows authentication
            if (args.EnableWinAuth)
            {
                if (response.StatusCode == (int)HttpStatusCode.Unauthorized)
                {
                    await handle401UnAuthorized(args);
                }
                else
                {
                    WinAuthEndPoint.AuthenticatedResponse(args.HttpClient.Data);
                }
            }

            // save original values so that if user changes them
            // we can still use original values when syphoning out data from attached tcp connection.
            response.SetOriginalHeaders();

            // if user requested call back then do it
            if (!response.Locked)
            {
                await onBeforeResponse(args);
            }

            // it may changed in the user event
            response = args.HttpClient.Response;

            var clientStream = args.ClientStream;

            // user set custom response by ignoring original response from server.
            if (response.Locked)
            {
                // write custom user response with body and return.
                await clientStream.WriteResponseAsync(response, cancellationToken);

                if (args.HttpClient.HasConnection && !args.HttpClient.CloseServerConnection)
                {
                    // syphon out the original response body from server connection
                    // so that connection will be good to be reused.
                    await args.SyphonOutBodyAsync(false, cancellationToken);
                }

                return;
            }

            // if user requested to send request again
            // likely after making modifications from User Response Handler
            if (args.ReRequest)
            {
                if (args.HttpClient.HasConnection)
                {
                    await tcpConnectionFactory.Release(args.HttpClient.Connection);
                }

                // clear current response
                await args.ClearResponse(cancellationToken);
                await handleHttpSessionRequest(args, null, args.ClientConnection.NegotiatedApplicationProtocol,
                            cancellationToken, args.CancellationTokenSource);
                return;
            }

            response.Locked = true;

            if (!args.IsTransparent && !args.IsSocks)
            {
                response.Headers.FixProxyHeaders();
            }

            await clientStream.WriteResponseAsync(response, cancellationToken);

            if (response.OriginalHasBody)
            {
                if (response.IsBodySent)
                {
                    // syphon out body
                    await args.SyphonOutBodyAsync(false, cancellationToken);
                }
                else
                {
                    // Copy body if exists
                    var serverStream = args.HttpClient.Connection.Stream;
                    await serverStream.CopyBodyAsync(response, false, clientStream, TransformationMode.None,
                        args.OnDataReceived, cancellationToken);
                }
            }

            args.TimeLine["Response Sent"] = DateTime.UtcNow;
        }

        /// <summary>
        ///     Invoke before response if it is set.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task onBeforeResponse(SessionEventArgs args)
        {
            if (BeforeResponse != null)
            {
                await BeforeResponse.InvokeAsync(this, args, ExceptionFunc);
            }
        }

        /// <summary>
        ///     Invoke after response if it is set.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task onAfterResponse(SessionEventArgs args)
        {
            if (AfterResponse != null)
            {
                await AfterResponse.InvokeAsync(this, args, ExceptionFunc);
            }
        }
    }
}

using System;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Network.WinAuth.Security;

namespace Titanium.Web.Proxy.Network.WinAuth
{
    /// <summary>
    ///     A handler for NTLM/Kerberos windows authentication challenge from server
    ///     NTLM process details below
    ///     https://blogs.msdn.microsoft.com/chiranth/2013/09/20/ntlm-want-to-know-how-it-works/
    /// </summary>
    internal static class WinAuthHandler
    {
        /// <summary>
        ///     Get the initial client token for server
        ///     using credentials of user running the proxy server process
        /// </summary>
        /// <param name="serverHostname"></param>
        /// <param name="authScheme"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static string GetInitialAuthToken(string serverHostname, string authScheme, InternalDataStore data)
        {
            var tokenBytes = WinAuthEndPoint.AcquireInitialSecurityToken(serverHostname, authScheme, data);
            return string.Concat(" ", Convert.ToBase64String(tokenBytes));
        }

        /// <summary>
        ///     Get the final token given the server challenge token
        /// </summary>
        /// <param name="serverHostname"></param>
        /// <param name="serverToken"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static string GetFinalAuthToken(string serverHostname, string serverToken, InternalDataStore data)
        {
            var tokenBytes =
                WinAuthEndPoint.AcquireFinalSecurityToken(serverHostname, Convert.FromBase64String(serverToken),
                    data);

            return string.Concat(" ", Convert.ToBase64String(tokenBytes));
        }
    }
}

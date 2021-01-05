// http://pinvoke.net/default.aspx/secur32/InitializeSecurityContext.html

using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Titanium.Web.Proxy.Http;

namespace Titanium.Web.Proxy.Network.WinAuth.Security
{
    using static Common;

    internal class WinAuthEndPoint
    {
        private const string authStateKey = "AuthState";

        /// <summary>
        ///     Acquire the intial client token to send
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="authScheme"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static byte[]? AcquireInitialSecurityToken(string hostname, string authScheme, InternalDataStore data)
        {
            byte[]? token;

            // null for initial call
            var serverToken = new SecurityBufferDesciption();

            var clientToken = new SecurityBufferDesciption(MaximumTokenSize);

            try
            {
                var state = new State();

                int result = AcquireCredentialsHandle(
                    WindowsIdentity.GetCurrent().Name,
                    authScheme,
                    SecurityCredentialsOutbound,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    0,
                    IntPtr.Zero,
                    ref state.Credentials,
                    ref NewLifeTime);

                if (result != SuccessfulResult)
                {
                    return null;
                }

                result = InitializeSecurityContext(ref state.Credentials,
                    IntPtr.Zero,
                    hostname,
                    StandardContextAttributes,
                    0,
                    SecurityNativeDataRepresentation,
                    ref serverToken,
                    0,
                    out state.Context,
                    out clientToken,
                    out NewContextAttributes,
                    out NewLifeTime);

                if (result != IntermediateResult)
                {
                    return null;
                }

                state.AuthState = State.WinAuthState.INITIAL_TOKEN;
                token = clientToken.GetBytes();
                data.Add(authStateKey, state);
            }
            finally
            {
                clientToken.Dispose();
                serverToken.Dispose();
            }

            return token;
        }

        /// <summary>
        ///     Acquire the final token to send
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="serverChallenge"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal static byte[]? AcquireFinalSecurityToken(string hostname, byte[] serverChallenge, InternalDataStore data)
        {
            byte[]? token;

            // user server challenge
            var serverToken = new SecurityBufferDesciption(serverChallenge);

            var clientToken = new SecurityBufferDesciption(MaximumTokenSize);

            try
            {
                var state = data.GetAs<State>(authStateKey);

                state.UpdatePresence();

                int result = InitializeSecurityContext(ref state.Credentials,
                    ref state.Context,
                    hostname,
                    StandardContextAttributes,
                    0,
                    SecurityNativeDataRepresentation,
                    ref serverToken,
                    0,
                    out state.Context,
                    out clientToken,
                    out NewContextAttributes,
                    out NewLifeTime);

                if (result != SuccessfulResult)
                {
                    return null;
                }

                state.AuthState = State.WinAuthState.FINAL_TOKEN;
                token = clientToken.GetBytes();
            }
            finally
            {
                clientToken.Dispose();
                serverToken.Dispose();
            }

            return token;
        }

        /// <summary>
        ///     Validates that the current WinAuth state of the connection matches the
        ///     expectation, used to detect failed authentication
        /// </summary>
        /// <param name="data"></param>
        /// <param name="expectedAuthState"></param>
        /// <returns></returns>
        internal static bool ValidateWinAuthState(InternalDataStore data, State.WinAuthState expectedAuthState)
        {
            bool stateExists = data.TryGetValueAs(authStateKey, out State? state);

            if (expectedAuthState == State.WinAuthState.UNAUTHORIZED)
            {
                return !stateExists ||
                       state!.AuthState == State.WinAuthState.UNAUTHORIZED ||
                       state.AuthState == State.WinAuthState.AUTHORIZED; // Server may require re-authentication on an open connection
            }

            if (expectedAuthState == State.WinAuthState.INITIAL_TOKEN)
            {
                return stateExists &&
                       (state!.AuthState == State.WinAuthState.INITIAL_TOKEN ||
                        state.AuthState == State.WinAuthState.AUTHORIZED); // Server may require re-authentication on an open connection
            }

            throw new Exception("Unsupported validation of WinAuthState");
        }

        /// <summary>
        ///     Set the AuthState to authorized and update the connection state lifetime
        /// </summary>
        /// <param name="data"></param>
        internal static void AuthenticatedResponse(InternalDataStore data)
        {
            if (data.TryGetValueAs(authStateKey, out State? state))
            {
                state!.AuthState = State.WinAuthState.AUTHORIZED;
                state.UpdatePresence();
            }
        }

        #region Native calls to secur32.dll

        [DllImport("secur32.dll", SetLastError = true)]
        private static extern int InitializeSecurityContext(ref SecurityHandle phCredential, // PCredHandle
            IntPtr phContext, // PCtxtHandle
            string pszTargetName,
            int fContextReq,
            int reserved1,
            int targetDataRep,
            ref SecurityBufferDesciption pInput, // PSecBufferDesc SecBufferDesc
            int reserved2,
            out SecurityHandle phNewContext, // PCtxtHandle
            out SecurityBufferDesciption pOutput, // PSecBufferDesc SecBufferDesc
            out uint pfContextAttr, // managed ulong == 64 bits!!!
            out SecurityInteger ptsExpiry); // PTimeStamp

        [DllImport("secur32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int InitializeSecurityContext(ref SecurityHandle phCredential, // PCredHandle
            ref SecurityHandle phContext, // PCtxtHandle
            string pszTargetName,
            int fContextReq,
            int reserved1,
            int targetDataRep,
            ref SecurityBufferDesciption secBufferDesc, // PSecBufferDesc SecBufferDesc
            int reserved2,
            out SecurityHandle phNewContext, // PCtxtHandle
            out SecurityBufferDesciption pOutput, // PSecBufferDesc SecBufferDesc
            out uint pfContextAttr, // managed ulong == 64 bits!!!
            out SecurityInteger ptsExpiry); // PTimeStamp

        [DllImport("secur32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern int AcquireCredentialsHandle(
            string pszPrincipal, // SEC_CHAR*
            string pszPackage, // SEC_CHAR* // "Kerberos","NTLM","Negotiative"
            int fCredentialUse,
            IntPtr pAuthenticationId, // _LUID AuthenticationID,//pvLogonID, // PLUID
            IntPtr pAuthData, // PVOID
            int pGetKeyFn, // SEC_GET_KEY_FN
            IntPtr pvGetKeyArgument, // PVOID
            ref SecurityHandle phCredential, // SecHandle // PCtxtHandle ref
            ref SecurityInteger ptsExpiry); // PTimeStamp // TimeStamp ref

        #endregion
    }
}

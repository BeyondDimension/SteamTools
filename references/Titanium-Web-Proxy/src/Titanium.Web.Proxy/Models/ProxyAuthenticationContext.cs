namespace Titanium.Web.Proxy.Models
{
    public enum ProxyAuthenticationResult
    {
        /// <summary>
        /// Indicates the authentication request was successful
        /// </summary>
        Success,
        /// <summary>
        /// Indicates the authentication request failed
        /// </summary>
        Failure,
        /// <summary>
        /// Indicates that this stage of the authentication request succeeded
        /// And a second pass of the handshake needs to occur
        /// </summary>
        ContinuationNeeded
    }

    /// <summary>
    /// A context container for authentication flows
    /// </summary>
    public class ProxyAuthenticationContext
    {
        /// <summary>
        /// The result of the current authentication request
        /// </summary>
        public ProxyAuthenticationResult Result { get; set; }

        /// <summary>
        /// An optional continuation token to return to the caller if set
        /// </summary>
        public string? Continuation { get; set; }

        public static ProxyAuthenticationContext Failed()
        {
            return new ProxyAuthenticationContext
            {
                Result = ProxyAuthenticationResult.Failure,
                Continuation = null
            };
        }

        public static ProxyAuthenticationContext Succeeded()
        {
            return new ProxyAuthenticationContext
            {
                Result = ProxyAuthenticationResult.Success,
                Continuation = null
            };
        }
    }
}

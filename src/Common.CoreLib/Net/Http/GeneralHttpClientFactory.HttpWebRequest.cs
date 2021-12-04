namespace System.Net.Http
{
    partial class GeneralHttpClientFactory
    {
        public static HttpWebRequest Create(string requestUriString)
        {
            var request = WebRequest.CreateHttp(requestUriString);
            ConfigHttpWebRequest(request);
            return request;
        }

        public static HttpWebRequest Create(Uri requestUri)
        {
            var request = WebRequest.CreateHttp(requestUri);
            ConfigHttpWebRequest(request);
            return request;
        }

        static void ConfigHttpWebRequest(HttpWebRequest request)
        {
            request.AllowAutoRedirect = true;
            request.MaximumAutomaticRedirections = 1000;
            request.Timeout = DefaultTimeoutTotalMilliseconds;
            var proxy = DefaultProxy;
            if (proxy != null)
            {
                request.Proxy = proxy;
            }
        }
    }
}

using System.Net;
using System.Threading.Tasks;
using Titanium.Web.Proxy.EventArguments;

namespace Titanium.Web.Proxy.Models
{
    public abstract class TransparentBaseProxyEndPoint : ProxyEndPoint
    {
        public abstract string GenericCertificateName { get; set; }

        protected TransparentBaseProxyEndPoint(IPAddress ipAddress, int port, bool decryptSsl) : base(ipAddress, port, decryptSsl)
        {
        }

        internal abstract Task InvokeBeforeSslAuthenticate(ProxyServer proxyServer,
            BeforeSslAuthenticateEventArgs connectArgs, ExceptionHandler exceptionFunc);
    }
}

using Titanium.Web.Proxy.EventArguments;

namespace System.Application.Proxy
{
    public static class ProxyEventArgsBaseExtensions
    {
        public static SampleClientState GetState(this ProxyEventArgsBase args)
        {
            if (args.ClientUserData == null)
            {
                args.ClientUserData = new SampleClientState();
            }

            return (SampleClientState)args.ClientUserData;
        }
    }
}
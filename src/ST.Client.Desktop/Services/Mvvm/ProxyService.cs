using ReactiveUI;

namespace System.Application.Services
{
    public class ProxyService : ReactiveObject
    {
        public static ProxyService Current { get; } = new ProxyService();



    }
}

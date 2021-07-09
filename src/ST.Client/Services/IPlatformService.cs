using System.Threading.Tasks;

namespace System.Application.Services
{
    public interface IPlatformService
    {
        protected const string TAG = "PlatformS";

        public static IPlatformService Instance => DI.Get<IPlatformService>();

        bool AdminShell(string str) => throw new PlatformNotSupportedException();
    }
}
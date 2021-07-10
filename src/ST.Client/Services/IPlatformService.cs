using System.Threading.Tasks;

namespace System.Application.Services
{
    public interface IPlatformService
    {
        protected const string TAG = "PlatformS";

        public static IPlatformService Instance => DI.Get<IPlatformService>();

        string[] GetMacNetworksetup() => throw new PlatformNotSupportedException();
        async void AdminShell(string str,bool admin=false) => await AdminShellAsync(str,admin);

        ValueTask AdminShellAsync(string str, bool admin = false) => throw new PlatformNotSupportedException();
    }
}
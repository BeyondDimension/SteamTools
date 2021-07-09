using System.Threading.Tasks;

namespace System.Application.Services
{
    public interface IPlatformService
    {
        protected const string TAG = "PlatformS";

        public static IPlatformService Instance => DI.Get<IPlatformService>();

        async void AdminShell(string str) => await AdminShellAsync(str);

        Task AdminShellAsync(string str) => throw new PlatformNotSupportedException();
    }
}
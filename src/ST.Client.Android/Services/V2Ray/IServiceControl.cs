using Android.App;

namespace System.Application.Services.V2Ray
{
    interface IServiceControl
    {
        Service Service { get; }

        void StartService(string parameters);

        void StopService();

        bool VpnProtect(int socket);
    }
}

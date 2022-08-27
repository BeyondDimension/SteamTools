#if !NET6_0_MAUI_LIB
using System.Application.Services;
using System.Application.UI.Activities;
using System.Collections.Generic;
using System.Text;

namespace System.Application.UI
{
    partial class MainApplication : IAndroidApplication, IApplication.IProgramHost
    {
        int? IAndroidApplication.NotificationSmallIconResId => Resource.Drawable.ic_stat_notify_msg;

        Type IAndroidApplication.MainActivityType =>
#if __XAMARIN_FORMS__
            typeof(MainActivity2);
#else
            typeof(MainActivity);
#endif

        IApplication.IProgramHost IApplication.ProgramHost => this;

        public bool IsMainProcess { get; private set; }

        void IApplication.IProgramHost.InitVisualStudioAppCenterSDK()
        {
            VisualStudioAppCenterSDK.Init();
        }

        void IApplication.IProgramHost.ConfigureServices(DILevel level, bool isTrace) => ConfigureServices(this, level, isTrace: isTrace);

        void IApplication.IProgramHost.OnStartup() => OnStartup(this);

        IApplication IApplication.IProgramHost.Application => this;
    }
}
#endif

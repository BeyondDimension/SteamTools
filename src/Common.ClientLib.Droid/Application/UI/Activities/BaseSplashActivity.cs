using Android.OS;
using System.Application.Security;
using System.Properties;

namespace System.Application.UI.Activities
{
    /// <summary>
    /// 页面 - 启动屏幕
    /// </summary>
    public abstract class BaseSplashActivity : BaseActivity
    {
        /// <summary>
        /// 是否检测不支持的设备，默认检测
        /// </summary>
        protected virtual bool EnableUnsupportedDeviceDetection => true;

        /// <summary>
        /// 是否允许在模拟器中运行，默认仅在Debug模式中允许
        /// </summary>
        protected virtual bool EnableEmulator => ThisAssembly.Debuggable;

        protected override bool BackToHome => true;

        protected override int? LayoutResource => null;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (EnableUnsupportedDeviceDetection && !DeviceSecurityCheckUtil.IsSupported(EnableEmulator))
            {
                Finish();
                return;
            }
        }
    }
}
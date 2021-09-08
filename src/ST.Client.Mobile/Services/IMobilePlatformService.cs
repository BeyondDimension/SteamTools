namespace System.Application.Services
{
    public interface IMobilePlatformService : IPlatformService
    {
        protected new const string TAG = "MobilePlatformS";

        public new static IMobilePlatformService Instance => DI.Get<IMobilePlatformService>();

        /// <summary>
        /// 获取当前平台 UI Host
        /// <para>reference to the ViewController (if using Xamarin.iOS), Activity (if using Xamarin.Android) IWin32Window or IntPtr (if using .Net Framework).</para>
        /// </summary>
        object CurrentPlatformUIHost { get; }
    }
}
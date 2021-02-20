using Android.Content;
using Android.Locations;
using AndroidX.Core.Location;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class LocationManagerExtensions
    {
        /// <summary>
        /// 获取位置管理服务
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static LocationManager GetLocationManager(this Context context)
            => context.GetSystemService<LocationManager>(Context.LocationService);

        /// <summary>
        /// 位置开关是否打开
        /// </summary>
        /// <param name="locationManager"></param>
        /// <returns></returns>
        public static bool IsLocationEnabledCompat(this LocationManager locationManager)
            => LocationManagerCompat.IsLocationEnabled(locationManager);
    }
}
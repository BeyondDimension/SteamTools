using Xamarin.Essentials;

// ReSharper disable once CheckNamespace
namespace System
{
    /// <summary>
    /// Enum 扩展 <see cref="PermissionStatus"/>
    /// </summary>
    public static class PermissionStatusEnumExtensions
    {
        /// <summary>
        /// 权限是否已授予
        /// </summary>
        /// <param name="permissionStatus"></param>
        /// <returns></returns>
        public static bool IsOk(this PermissionStatus permissionStatus)
            => permissionStatus == PermissionStatus.Granted;
    }
}
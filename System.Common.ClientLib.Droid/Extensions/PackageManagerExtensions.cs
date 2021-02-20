using Android.Content;
using Android.Content.PM;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class PackageManagerExtensions
    {
        /// <summary>
        /// 根据包名获取包信息
        /// </summary>
        /// <param name="pkgManager"></param>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static PackageInfo? GetPackageInfo(this PackageManager? pkgManager, string packageName)
        {
            if (pkgManager == null) return null;
            try
            {
                var info = pkgManager.GetPackageInfo(packageName, 0);
                return info;
            }
            catch (PackageManager.NameNotFoundException)
            {
                return null;
            }
            catch (Exception)
            {
                try
                {
                    var packages = pkgManager.GetInstalledPackages(0);
                    return packages.FirstOrDefault(x => x.PackageName == packageName);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        /// <inheritdoc cref="GetPackageInfo(PackageManager?, string)"/>
        public static PackageInfo? GetPackageInfo(this Context context, string packageName) => GetPackageInfo(context.PackageManager, packageName);
    }
}
using Xamarin.Essentials;

namespace System.Application.Services
{
    public interface IDeviceInfoPlatformService
    {
        string Model { get; }

        string Manufacturer { get; }

        string Name { get; }

        string VersionString { get; }

        DeviceType DeviceType { get; }

        bool IsChromeOS { get; }
    }
}


namespace System.Application.Services.Implementation
{
    public abstract class DeviceInfoPlatformServiceImpl : IDeviceInfoPlatformService
    {
        public /*virtual*/ string Model => DeviceInfo.Model;

        public /*virtual*/ string Manufacturer => DeviceInfo.Manufacturer;

        public /*virtual*/ string Name => DeviceInfo.Name;

        public /*virtual*/ string VersionString => DeviceInfo.VersionString;

        public virtual DeviceType DeviceType => DeviceInfo.DeviceType;

        public virtual bool IsChromeOS => false;
    }
}

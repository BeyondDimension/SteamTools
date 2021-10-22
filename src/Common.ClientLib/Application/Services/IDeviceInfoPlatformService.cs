using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

namespace System.Application.Services
{
    public interface IDeviceInfoPlatformService
    {
        string Model => string.Empty;

        string Manufacturer => string.Empty;

        string Name => string.Empty;

        string VersionString => string.Empty;

        DeviceType DeviceType => DeviceType.Unknown;
    }
}

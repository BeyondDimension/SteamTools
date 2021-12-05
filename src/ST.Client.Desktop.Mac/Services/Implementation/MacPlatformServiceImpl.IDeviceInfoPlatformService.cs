//#if MONO_MAC
//using System;
//using System.Collections.Generic;
//using System.Runtime.InteropServices;
//using System.Text;
//#if MONO_MAC
//using MonoMac.CoreFoundation;
//using MonoMac.Foundation;
//using MonoMac.ObjCRuntime;
//using ObjCRuntimeConstants = MonoMac.ObjCRuntime.Constants;
//#elif XAMARIN_MAC
//using CoreFoundation;
//using Foundation;
//using ObjCRuntime;
//using ObjCRuntimeConstants = ObjCRuntime.Constants;
//#endif

//namespace System.Application.Services.Implementation
//{
//    partial class MacPlatformServiceImpl : IDeviceInfoPlatformService
//    {
//        // https://github.com/xamarin/Essentials/blob/1.7.0/Xamarin.Essentials/DeviceInfo/DeviceInfo.macos.cs

//        [DllImport(ObjCRuntimeConstants.SystemConfigurationLibrary)]
//        static extern IntPtr SCDynamicStoreCopyComputerName(IntPtr store, IntPtr encoding);

//        [DllImport(ObjCRuntimeConstants.CoreFoundationLibrary)]
//        static extern void CFRelease(IntPtr cf);

//        static string GetModel() => IOKit.GetPlatformExpertPropertyValue<NSData>("model")?.ToString() ?? string.Empty;

//        static string GetManufacturer() => "Apple";

//        static string? GetDeviceName()
//        {
//            var computerNameHandle = SCDynamicStoreCopyComputerName(IntPtr.Zero, IntPtr.Zero);

//            if (computerNameHandle == IntPtr.Zero)
//                return null;

//            try
//            {
//#pragma warning disable CS0618 // Type or member is obsolete
//                return NSString.FromHandle(computerNameHandle);
//#pragma warning restore CS0618 // Type or member is obsolete
//            }
//            finally
//            {
//                CFRelease(computerNameHandle);
//            }
//        }

//        static string GetVersionString()
//        {

//            using var info = new NSProcessInfo();
//#if MONO_MAC
//            return info.OperatingSystemVersionString;
//#elif XAMARIN_MAC
//            return info.OperatingSystemVersion.ToString();
//#else
//            throw new PlatformNotSupportedException();
//#endif
//        }
//    }
//}

//#if MONO_MAC
//namespace MonoMac.ObjCRuntime
//{
//    public static partial class Constants
//    {
//        public const string SystemConfigurationLibrary = "/System/Library/Frameworks/SystemConfiguration.framework/SystemConfiguration";

//        public const string CoreFoundationLibrary = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
//    }
//}
//#endif

//internal static partial class IOKit
//{
//    const string IOKitLibrary = "/System/Library/Frameworks/IOKit.framework/IOKit";
//    const string IOPlatformExpertDeviceClassName = "IOPlatformExpertDevice";

//    [DllImport(IOKitLibrary)]
//    static extern uint IOServiceGetMatchingService(uint masterPort, IntPtr matching);

//    [DllImport(IOKitLibrary)]
//    static extern IntPtr IOServiceMatching(string s);

//    [DllImport(IOKitLibrary)]
//    static extern IntPtr IORegistryEntryCreateCFProperty(uint entry, IntPtr key, IntPtr allocator, uint options);

//    [DllImport(IOKitLibrary)]
//    static extern int IOObjectRelease(uint o);

//    internal static T? GetPlatformExpertPropertyValue<T>(CFString property) where T : NSObject
//    {
//        uint platformExpertRef = 0;
//        try
//        {
//            platformExpertRef = IOServiceGetMatchingService(0, IOServiceMatching(IOPlatformExpertDeviceClassName));
//            if (platformExpertRef == 0)
//                return default;

//            var propertyRef = IORegistryEntryCreateCFProperty(platformExpertRef, property.Handle, IntPtr.Zero, 0);
//            if (propertyRef == IntPtr.Zero)
//                return default;
//#if MONO_MAC
//            return (T)Runtime.GetNSObject(propertyRef);
//#elif XAMARIN_MAC
//            return Runtime.GetNSObject<T>(propertyRef, true);
//#else
//            throw new PlatformNotSupportedException();
//#endif
//        }
//        finally
//        {
//            if (platformExpertRef != 0)
//                IOObjectRelease(platformExpertRef);
//        }
//    }
//}
//#endif
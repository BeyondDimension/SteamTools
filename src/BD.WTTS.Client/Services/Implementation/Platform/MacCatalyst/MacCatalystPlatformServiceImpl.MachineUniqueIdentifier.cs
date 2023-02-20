#if MACOS || MACCATALYST || IOS
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class MacCatalystPlatformServiceImpl
{
    // https://blog.csdn.net/lipingqingqing/article/details/8843606
    // https://forums.xamarin.com/discussion/54210/iokit-framework
    // https://gist.github.com/chamons/82ab06f5e83d2cb10193

    [LibraryImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
    private static partial uint IOServiceGetMatchingService(uint masterPort, IntPtr matching);

    [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
    static extern IntPtr IOServiceMatching(string s);

    [LibraryImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
    private static partial IntPtr IORegistryEntryCreateCFProperty(uint entry, IntPtr key, IntPtr allocator, uint options);

    [LibraryImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
    private static partial int IOObjectRelease(uint o);

    static string GetIOPlatformExpertDevice(string keyName)
    {
        var value = string.Empty;
        var platformExpert = IOServiceGetMatchingService(0, IOServiceMatching("IOPlatformExpertDevice"));
        if (platformExpert != 0)
        {
            var key = (NSString)keyName;
            var valueIntPtr = IORegistryEntryCreateCFProperty(platformExpert, key.Handle, IntPtr.Zero, 0);
            if (valueIntPtr != IntPtr.Zero)
            {
#pragma warning disable CS0618 // 类型或成员已过时
                value = NSString.FromHandle(valueIntPtr) ?? value;
#pragma warning restore CS0618 // 类型或成员已过时
            }
            _ = IOObjectRelease(platformExpert);
        }

        return value;
    }

    static string GetSerialNumber() => GetIOPlatformExpertDevice("IOPlatformSerialNumber");

#if DEBUG
    static string GetPlatformUUID() => GetIOPlatformExpertDevice("IOPlatformUUID");
#endif

    static string GetMachineSecretKey()
    {
        var value = GetSerialNumber();
        return value;
    }

    static readonly Lazy<(byte[] key, byte[] iv)> mMachineSecretKey = IPlatformService.GetMachineSecretKey(GetMachineSecretKey);

    public (byte[] key, byte[] iv) MachineSecretKey => mMachineSecretKey.Value;
}
#endif
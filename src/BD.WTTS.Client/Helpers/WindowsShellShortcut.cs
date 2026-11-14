#if WINDOWS
using System.Runtime.InteropServices.Marshalling;

namespace BD.WTTS.Helpers;

internal static partial class WindowsShellShortcut
{
    static readonly StrategyBasedComWrappers wrappers = new();

    public static void Create(string path, string target, string? arguments, string? description,
        string? hotkey, string? iconLocation, string? workingDirectory)
    {
        var initialized = CoInitializeEx(0, 2 /* COINIT_APARTMENTTHREADED */);
        if (initialized < 0 && initialized != unchecked((int)0x80010106) /* RPC_E_CHANGED_MODE */)
            Marshal.ThrowExceptionForHR(initialized);

        ComObject? shell = null;
        ComObject? shortcut = null;
        try
        {
            var classId = new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8");
            var interfaceId = new Guid("F935DC21-1CF0-11D0-ADB9-00C04FD58A0B");
            Marshal.ThrowExceptionForHR(CoCreateInstance(in classId, 0, 1, in interfaceId, out var pointer));
            shell = WrapOwnedPointer(pointer);
            shortcut = WrapOwnedPointer(((IWshShellNative)(object)shell).CreateShortcut(path));
            var link = (IWshShortcutNative)(object)shortcut;
            link.SetTargetPath(target);
            if (!string.IsNullOrEmpty(description)) link.SetDescription(description);
            if (!string.IsNullOrEmpty(arguments)) link.SetArguments(arguments);
            if (!string.IsNullOrEmpty(hotkey)) link.SetHotkey(hotkey);
            if (!string.IsNullOrEmpty(iconLocation)) link.SetIconLocation(iconLocation);
            if (!string.IsNullOrEmpty(workingDirectory)) link.SetWorkingDirectory(workingDirectory);
            link.Save();
        }
        finally
        {
            shortcut?.FinalRelease();
            shell?.FinalRelease();
            if (initialized >= 0) CoUninitialize();
        }
    }

    static ComObject WrapOwnedPointer(nint pointer)
    {
        try
        {
            return (ComObject)wrappers.GetOrCreateObjectForComInstance(pointer, CreateObjectFlags.UniqueInstance);
        }
        finally
        {
            Marshal.Release(pointer);
        }
    }

    [LibraryImport("ole32.dll")]
    private static partial int CoInitializeEx(nint reserved, uint flags);

    [LibraryImport("ole32.dll")]
    private static partial void CoUninitialize();

    [LibraryImport("ole32.dll")]
    private static partial int CoCreateInstance(in Guid classId, nint outer, uint context, in Guid interfaceId, out nint instance);
}

// WSH exposes dual interfaces. Include the IDispatch slots explicitly so the
// generated IUnknown-based wrappers use the native vtable without dynamic IL.
[GeneratedComInterface]
[Guid("00020400-0000-0000-C000-000000000046")]
internal partial interface IDispatchNative
{
    void GetTypeInfoCount(out uint count);
    void GetTypeInfo(uint index, uint locale, out nint typeInfo);
    void GetIDsOfNames(in Guid interfaceId, nint names, uint count, uint locale, nint dispatchIds);
    void Invoke(int dispatchId, in Guid interfaceId, uint locale, ushort flags,
        nint parameters, nint result, nint exceptionInfo, nint argumentError);
}

[GeneratedComInterface]
[Guid("F935DC21-1CF0-11D0-ADB9-00C04FD58A0B")]
internal partial interface IWshShellNative : IDispatchNative
{
    nint GetSpecialFolders();
    nint GetEnvironment(nint type);
    int Run([MarshalAs(UnmanagedType.BStr)] string command, nint windowStyle, nint waitOnReturn);
    int Popup([MarshalAs(UnmanagedType.BStr)] string text, nint secondsToWait, nint title, nint type);
    nint CreateShortcut([MarshalAs(UnmanagedType.BStr)] string path);
}

[GeneratedComInterface]
[Guid("F935DC23-1CF0-11D0-ADB9-00C04FD58A0B")]
internal partial interface IWshShortcutNative : IDispatchNative
{
    [return: MarshalAs(UnmanagedType.BStr)] string GetFullName();
    [return: MarshalAs(UnmanagedType.BStr)] string GetArguments();
    void SetArguments([MarshalAs(UnmanagedType.BStr)] string value);
    [return: MarshalAs(UnmanagedType.BStr)] string GetDescription();
    void SetDescription([MarshalAs(UnmanagedType.BStr)] string value);
    [return: MarshalAs(UnmanagedType.BStr)] string GetHotkey();
    void SetHotkey([MarshalAs(UnmanagedType.BStr)] string value);
    [return: MarshalAs(UnmanagedType.BStr)] string GetIconLocation();
    void SetIconLocation([MarshalAs(UnmanagedType.BStr)] string value);
    void SetRelativePath([MarshalAs(UnmanagedType.BStr)] string value);
    [return: MarshalAs(UnmanagedType.BStr)] string GetTargetPath();
    void SetTargetPath([MarshalAs(UnmanagedType.BStr)] string value);
    int GetWindowStyle();
    void SetWindowStyle(int value);
    [return: MarshalAs(UnmanagedType.BStr)] string GetWorkingDirectory();
    void SetWorkingDirectory([MarshalAs(UnmanagedType.BStr)] string value);
    void Load([MarshalAs(UnmanagedType.BStr)] string path);
    void Save();
}
#endif

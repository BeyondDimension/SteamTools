#if WINDOWS && WTTS_NATIVE_AOT
using BD.Common;
using System.Runtime.InteropServices.Marshalling;
using System.Security.Cryptography;
using Windows.UI.Notifications;

namespace BD.WTTS.Services.Implementation;

internal static partial class NativeToastNotificationManager
{
    static readonly Lazy<string> desktopAppId = new(ResolveDesktopAppId);
    static readonly StrategyBasedComWrappers wrappers = new();
    static int registrationRevoked;
    static readonly Lazy<(uint Cookie, ToastNotifier Notifier)> registration = new(Register);

    internal static NativeToastHistory History { get; } = new();

    internal static void Initialize()
    {
        if (DesktopBridge.IsRunningAsUwp) return;
        try
        {
            _ = registration.Value;
        }
        catch (COMException ex)
        {
            Log.Error(nameof(NativeToastNotificationManager), ex, "Could not register Windows notifications for the current app identity.");
        }
    }

    internal static ToastNotifier CreateToastNotifier() => DesktopBridge.IsRunningAsUwp
        ? ToastNotificationManager.CreateToastNotifier()
        : registration.Value.Notifier;

    // Use the same Shell identity and CLSID as ToastNotificationManagerCompat in JIT.
    // 7.1.3: Toasts/Compat/Desktop/Win32AppInfo.cs (Windows Community Toolkit).
    static string ResolveDesktopAppId()
    {
        var resolverClass = new Guid("660b90c8-73a9-4b58-8cae-355b7f55341b");
        var resolverInterface = typeof(INativeApplicationResolver).GUID;
        Marshal.ThrowExceptionForHR(CoCreateInstance(in resolverClass, 0, 1 /* CLSCTX_INPROC_SERVER */, in resolverInterface, out var pointer));
        ComObject resolver;
        try
        {
            resolver = (ComObject)wrappers.GetOrCreateObjectForComInstance(pointer, CreateObjectFlags.UniqueInstance);
        }
        finally
        {
            Marshal.Release(pointer);
        }
        try
        {
            Marshal.ThrowExceptionForHR(((INativeApplicationResolver)(object)resolver).GetAppIdForProcess((uint)Environment.ProcessId,
                out var appId, out _, out _, out _));
            try
            {
                return Marshal.PtrToStringUni(appId) is { Length: > 0 } value ? value : throw new COMException("The Shell did not resolve an AppUserModelID.");
            }
            finally
            {
                Marshal.FreeCoTaskMem(appId);
            }
        }
        finally
        {
            resolver.FinalRelease();
        }
    }

    static Guid GetActivatorId(string appId)
    {
        var length = Encoding.UTF8.GetByteCount(appId);
        Span<byte> data = stackalloc byte[16];
        if (length < data.Length)
        {
            data.Clear();
            Encoding.UTF8.GetBytes(appId, data);
            return new Guid(data, bigEndian: true);
        }
        // Toolkit's hash is for GUID compatibility, not used for authentication.
        Span<byte> hash = stackalloc byte[SHA1.HashSizeInBytes];
        SHA1.HashData(Encoding.UTF8.GetBytes(appId), hash);
        return new Guid(hash[..16], bigEndian: true);
    }

    static (uint Cookie, ToastNotifier Notifier) Register()
    {
        var executable = Environment.ProcessPath ?? throw new InvalidOperationException("The notification server executable is unavailable.");
        var appId = desktopAppId.Value;
        var activatorId = GetActivatorId(appId);
        using (var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\CLSID\{activatorId:B}\LocalServer32"))
            key.SetValue(null, $"\"{executable}\" -clt c");
        using (var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\AppUserModelId\{appId}"))
        {
            key.SetValue("DisplayName", AssemblyInfo.Trademark);
            key.SetValue("CustomActivator", activatorId.ToString("B"));
        }

        var factory = wrappers.GetOrCreateComInterfaceForObject(new NativeNotificationFactory(), CreateComInterfaceFlags.None);
        try
        {
            Marshal.ThrowExceptionForHR(CoRegisterClassObject(in activatorId, factory, 4 /* CLSCTX_LOCAL_SERVER */, 1 /* REGCLS_MULTIPLEUSE */, out var cookie));
            try
            {
                var notifier = ToastNotificationManager.CreateToastNotifier(appId);
                if (!Environment.IsPrivilegedProcess)
                {
                    try
                    {
                        _ = notifier.Setting;
                    }
                    catch (COMException ex) when (ex.HResult == unchecked((int)0x80070490))
                    {
                        // Identity-less apps acquire notification settings after their first toast.
                        // Match the toolkit's PreRegisterIdentityLessApp without emitting an activator.
                        var xml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
                        xml.GetElementsByTagName("text")[0].InnerText = AssemblyInfo.Trademark;
                        var first = new ToastNotification(xml)
                        {
                            SuppressPopup = true,
                            Tag = "wtts-first",
                            Group = "registration",
                            ExpirationTime = DateTimeOffset.Now.AddSeconds(15),
                        };
                        notifier.Show(first);
                        ToastNotificationManager.History.Remove(first.Tag, first.Group, appId);
                    }
                }
                return (cookie, notifier);
            }
            catch
            {
                CoRevokeClassObject(cookie);
                throw;
            }
        }
        finally
        {
            Marshal.Release(factory);
        }
    }

    internal static void Shutdown()
    {
        if (registration.IsValueCreated && Interlocked.Exchange(ref registrationRevoked, 1) == 0)
            Marshal.ThrowExceptionForHR(CoRevokeClassObject(registration.Value.Cookie));
    }

    internal static void Uninstall()
    {
        History.Clear();
        Shutdown();
        if (DesktopBridge.IsRunningAsUwp) return;
        var appId = desktopAppId.Value;
        Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\AppUserModelId\{appId}", false);
        Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\CLSID\{GetActivatorId(appId):B}", false);
    }

    internal sealed class NativeToastHistory
    {
        internal void Remove(string tag, string group)
        {
            if (DesktopBridge.IsRunningAsUwp)
                ToastNotificationManager.History.Remove(tag, group);
            else
                ToastNotificationManager.History.Remove(tag, group, desktopAppId.Value);
        }

        internal void Clear()
        {
            if (DesktopBridge.IsRunningAsUwp)
                ToastNotificationManager.History.Clear();
            else
                ToastNotificationManager.History.Clear(desktopAppId.Value);
        }
    }

    [LibraryImport("ole32.dll")]
    private static partial int CoCreateInstance(in Guid classId, nint outer, uint context, in Guid interfaceId, out nint instance);

    [LibraryImport("ole32.dll")]
    private static partial int CoRegisterClassObject(in Guid classId, nint factory, uint context, uint flags, out uint cookie);

    [LibraryImport("ole32.dll")]
    private static partial int CoRevokeClassObject(uint cookie);

    [GeneratedComClass]
    internal sealed partial class NativeNotificationFactory : INativeNotificationClassFactory
    {
        readonly object gate = new();
        int locks;
        GCHandle keepAlive;

        public int CreateInstance(nint outer, in Guid interfaceId, out nint instance)
        {
            instance = 0;
            if (outer != 0) return unchecked((int)0x80040110); // CLASS_E_NOAGGREGATION
            var unknown = wrappers.GetOrCreateComInterfaceForObject(new NativeNotificationActivator(), CreateComInterfaceFlags.None);
            try
            {
                var requestedInterface = interfaceId;
                return Marshal.QueryInterface(unknown, ref requestedInterface, out instance);
            }
            finally
            {
                Marshal.Release(unknown);
            }
        }

        public int LockServer(int @lock)
        {
            lock (gate)
            {
                if (@lock != 0)
                {
                    if (locks++ == 0) keepAlive = GCHandle.Alloc(this);
                }
                else if (locks > 0 && --locks == 0)
                {
                    keepAlive.Free();
                }
            }
            return 0;
        }
    }

    [GeneratedComClass]
    internal sealed partial class NativeNotificationActivator : INativeNotificationActivationCallback
    {
        public void Activate(string appUserModelId, string? invokedArgs, nint userInput, uint count)
        {
            if (!string.Equals(appUserModelId, desktopAppId.Value, StringComparison.Ordinal)) return;
            MainThread2.BeginInvokeOnMainThread(() => IApplication.Instance.RestoreMainWindow());
        }
    }
}

[GeneratedComInterface]
[Guid("00000001-0000-0000-C000-000000000046")]
internal partial interface INativeNotificationClassFactory
{
    [PreserveSig] int CreateInstance(nint outer, in Guid interfaceId, out nint instance);
    [PreserveSig] int LockServer(int @lock);
}

[GeneratedComInterface(StringMarshalling = StringMarshalling.Utf16)]
[Guid("53E31837-6600-4A81-9395-75CFFE746F94")]
internal partial interface INativeNotificationActivationCallback
{
    void Activate(string appUserModelId, string? invokedArgs, nint userInput, uint count);
}

[GeneratedComInterface]
[Guid("DE25675A-72DE-44B4-9373-05170450C140")]
internal partial interface INativeApplicationResolver
{
    [PreserveSig] int GetAppIdForShortcut(nint shellItem, out nint appId);
    [PreserveSig] int GetAppIdForShortcutObject(nint shellLink, nint shellItem, out nint appId);
    [PreserveSig] int GetAppIdForWindow(nint hwnd, out nint appId, out int pinningPrevented, out int explicitId, out int embeddedShortcutValid);
    [PreserveSig] int GetAppIdForProcess(uint processId, out nint appId, out int pinningPrevented, out int explicitId, out int embeddedShortcutValid);
}
#endif

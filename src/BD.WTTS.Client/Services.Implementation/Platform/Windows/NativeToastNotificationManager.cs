#if WINDOWS && WTTS_NATIVE_AOT
using BD.Common;
using System.Runtime.InteropServices.Marshalling;
using Windows.UI.Notifications;

namespace BD.WTTS.Services.Implementation;

internal static partial class NativeToastNotificationManager
{
    const string AppId = AssemblyInfo.APPLICATION_ID + ".Native";
    static readonly Guid activatorId = new("4D2F4EF0-BECA-4DE7-8683-EF4A30E4155C");
    static readonly StrategyBasedComWrappers wrappers = new();
    static int registrationRevoked;
    static readonly Lazy<(uint Cookie, ToastNotifier Notifier)> registration = new(Register);

    internal static NativeToastHistory History { get; } = new();

    internal static void Initialize() => _ = registration.Value;

    internal static ToastNotifier CreateToastNotifier()
    {
        Initialize();
        return registration.Value.Notifier;
    }

    static (uint Cookie, ToastNotifier Notifier) Register()
    {
        var executable = Environment.ProcessPath ?? throw new InvalidOperationException("The notification server executable is unavailable.");
        using (var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\CLSID\{activatorId:B}\LocalServer32"))
            key.SetValue(null, $"\"{executable}\" -clt c");
        using (var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\AppUserModelId\{AppId}"))
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
                var notifier = ToastNotificationManager.CreateToastNotifier(AppId);
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
                        ToastNotificationManager.History.Remove(first.Tag, first.Group, AppId);
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
        Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\AppUserModelId\{AppId}", false);
        Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\CLSID\{activatorId:B}", false);
    }

    internal sealed class NativeToastHistory
    {
        internal void Remove(string tag, string group) => ToastNotificationManager.History.Remove(tag, group, AppId);
        internal void Clear() => ToastNotificationManager.History.Clear(AppId);
    }

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
            if (!string.Equals(appUserModelId, AppId, StringComparison.Ordinal)) return;
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
#endif

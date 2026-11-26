#if WINDOWS
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace BD.WTTS.Services.Implementation;

internal static partial class DesktopJumpList
{
    const int AccessDenied = unchecked((int)0x80070005);
    const int NoMatchingAssociationHandler = unchecked((int)0x80040F03);
    const int NoInterface = unchecked((int)0x80004002);
    const int ChangedApartmentMode = unchecked((int)0x80010106);
    const int LinkTextCapacity = 32768;

    static readonly Guid destinationListClassId = new("77F10CF0-3DB5-4966-B520-B7C54FD35ED6");
    static readonly Guid objectCollectionClassId = new("2D3468C1-36A7-43B6-AC24-D3F02FD9607A");
    static readonly Guid shellLinkClassId = new("00021401-0000-0000-C000-000000000046");
    static readonly DesktopPropertyKey titleProperty = new(new("F29F85E0-4FF9-1068-AB91-08002B27B3D9"), 2);
    static readonly StrategyBasedComWrappers wrappers = new();
    static readonly HashSet<(string Path, string Arguments)> removedLinks = [];

    internal static void Apply(IEnumerable<(string title, string applicationPath, string iconResourcePath,
        string arguments, string description, string customCategory)> items)
    {
        var initialized = CoInitializeEx(0, 2 /* COINIT_APARTMENTTHREADED */);
        if (initialized != ChangedApartmentMode)
            Marshal.ThrowExceptionForHR(initialized);

        try
        {
            lock (removedLinks)
            {
                using var destinationList = Create<IDesktopDestinationList>(destinationListClassId);
                var list = destinationList.As<IDesktopDestinationList>();
                SetExplicitAppId(list);

                var objectArrayInterfaceId = typeof(IDesktopObjectArray).GUID;
                list.BeginList(out _, in objectArrayInterfaceId, out var removedPointer);
                var committed = false;
                try
                {
                    RetrieveRemovedLinks(removedPointer);
                    using var tasks = Create<IDesktopObjectCollection>(objectCollectionClassId);
                    var tasksCollection = tasks.As<IDesktopObjectCollection>();
                    foreach (var category in items.GroupBy(item => item.customCategory))
                    {
                        using var collection = Create<IDesktopObjectCollection>(objectCollectionClassId);
                        var group = collection.As<IDesktopObjectCollection>();
                        foreach (var (title, applicationPath, iconResourcePath, arguments, description, _) in category)
                        {
                            if (removedLinks.Contains((applicationPath.ToUpperInvariant(), arguments)))
                                continue;

                            using var link = Create<IDesktopShellLink>(shellLinkClassId);
                            var shellLink = link.As<IDesktopShellLink>();
                            shellLink.SetPath(applicationPath);
                            shellLink.SetArguments(arguments);
                            shellLink.SetDescription(description);
                            shellLink.SetIconLocation(iconResourcePath, 0);
                            if (Path.GetDirectoryName(applicationPath) is { Length: > 0 } workingDirectory)
                                shellLink.SetWorkingDirectory(workingDirectory);
                            SetTitle(link.As<IDesktopPropertyStore>(), title);
                            group.AddObject(link.Pointer);
                        }

                        group.GetCount(out var count);
                        if (count == 0) continue;
                        if (string.IsNullOrWhiteSpace(category.Key))
                        {
                            tasksCollection.AddFromArray(collection.Pointer);
                            continue;
                        }

                        var result = list.AppendCategory(category.Key, collection.Pointer);
                        // Custom destinations depend on Shell privacy settings and file associations.
                        if (result is AccessDenied or NoMatchingAssociationHandler)
                            tasksCollection.AddFromArray(collection.Pointer);
                        else
                            Marshal.ThrowExceptionForHR(result);
                    }

                    tasksCollection.GetCount(out var taskCount);
                    if (taskCount > 0)
                        list.AddUserTasks(tasks.Pointer);
                    list.AppendKnownCategory(2 /* KDC_RECENT */);
                    list.CommitList();
                    committed = true;
                }
                finally
                {
                    if (!committed) _ = list.AbortList();
                }
            }
        }
        finally
        {
            if (initialized >= 0) CoUninitialize();
        }
    }

    static void SetExplicitAppId(IDesktopDestinationList list)
    {
        if (GetCurrentProcessExplicitAppUserModelID(out var appId) < 0 || appId == 0)
            return;

        try
        {
            list.SetAppID(appId);
        }
        finally
        {
            Marshal.FreeCoTaskMem(appId);
        }
    }

    static void RetrieveRemovedLinks(nint pointer)
    {
        if (pointer == 0) return;
        using var removed = new ComReference(pointer);
        var array = removed.As<IDesktopObjectArray>();
        array.GetCount(out var count);
        if (count == 0) return;

        var shellLinkInterfaceId = typeof(IDesktopShellLink).GUID;
        var buffer = Marshal.AllocHGlobal(LinkTextCapacity * sizeof(char));
        try
        {
            for (uint index = 0; index < count; index++)
            {
                var result = array.GetAt(index, in shellLinkInterfaceId, out var linkPointer);
                if (result == NoInterface) continue;
                Marshal.ThrowExceptionForHR(result);
                using var link = new ComReference(linkPointer);
                var shellLink = link.As<IDesktopShellLink>();
                Marshal.WriteInt16(buffer, 0);
                shellLink.GetPath(buffer, LinkTextCapacity, 0, 4 /* SLGP_RAWPATH */);
                if (Marshal.PtrToStringUni(buffer) is not { Length: > 0 } path)
                    continue;

                Marshal.WriteInt16(buffer, 0);
                shellLink.GetArguments(buffer, LinkTextCapacity);
                if (Marshal.PtrToStringUni(buffer) is { Length: > 0 } arguments)
                    removedLinks.Add((path.ToUpperInvariant(), arguments));
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    static void SetTitle(IDesktopPropertyStore properties, string title)
    {
        var text = Marshal.StringToCoTaskMemUni(title);
        try
        {
            var value = new DesktopPropertyVariant(31 /* VT_LPWSTR */, text);
            properties.SetValue(in titleProperty, in value);
            properties.Commit();
        }
        finally
        {
            Marshal.FreeCoTaskMem(text);
        }
    }

    static ComReference Create<T>(in Guid classId)
    {
        var interfaceId = typeof(T).GUID;
        Marshal.ThrowExceptionForHR(CoCreateInstance(in classId, 0, 1 /* CLSCTX_INPROC_SERVER */, in interfaceId, out var pointer));
        return new ComReference(pointer);
    }

    sealed class ComReference(nint pointer) : IDisposable
    {
        readonly ComObject instance = Wrap(pointer);

        internal nint Pointer => pointer;

        internal T As<T>() where T : class => (T)(object)instance;

        static ComObject Wrap(nint pointer)
        {
            try
            {
                return (ComObject)wrappers.GetOrCreateObjectForComInstance(pointer, CreateObjectFlags.UniqueInstance);
            }
            catch
            {
                Marshal.Release(pointer);
                throw;
            }
        }

        public void Dispose()
        {
            instance.FinalRelease();
            Marshal.Release(pointer);
        }
    }

    [LibraryImport("ole32.dll")]
    private static partial int CoInitializeEx(nint reserved, uint flags);

    [LibraryImport("ole32.dll")]
    private static partial void CoUninitialize();

    [LibraryImport("ole32.dll")]
    private static partial int CoCreateInstance(in Guid classId, nint outer, uint context, in Guid interfaceId, out nint instance);

    [LibraryImport("shell32.dll")]
    private static partial int GetCurrentProcessExplicitAppUserModelID(out nint appId);
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper, StringMarshalling = StringMarshalling.Utf16)]
[Guid("6332DEBF-87B5-4670-90C0-5E57B408A49E")]
internal partial interface IDesktopDestinationList
{
    void SetAppID(nint appId);
    void BeginList(out uint minSlots, in Guid interfaceId, out nint removed);
    [PreserveSig] int AppendCategory(string category, nint items);
    void AppendKnownCategory(int category);
    void AddUserTasks(nint tasks);
    void CommitList();
    void GetRemovedDestinations(in Guid interfaceId, out nint removed);
    void DeleteList(nint appId);
    [PreserveSig] int AbortList();
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("92CA9DCD-5622-4BBA-A805-5E9F541BD8C9")]
internal partial interface IDesktopObjectArray
{
    void GetCount(out uint count);
    [PreserveSig] int GetAt(uint index, in Guid interfaceId, out nint instance);
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("5632B1A4-E38A-400A-928A-D4CD63230295")]
internal partial interface IDesktopObjectCollection : IDesktopObjectArray
{
    void AddObject(nint instance);
    void AddFromArray(nint items);
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper, StringMarshalling = StringMarshalling.Utf16)]
[Guid("000214F9-0000-0000-C000-000000000046")]
internal partial interface IDesktopShellLink
{
    void GetPath(nint path, int length, nint findData, uint flags);
    void GetIDList(out nint identifier);
    void SetIDList(nint identifier);
    void GetDescription(nint description, int length);
    void SetDescription(string description);
    void GetWorkingDirectory(nint directory, int length);
    void SetWorkingDirectory(string directory);
    void GetArguments(nint arguments, int length);
    void SetArguments(string arguments);
    void GetHotkey(out ushort hotkey);
    void SetHotkey(ushort hotkey);
    void GetShowCmd(out int command);
    void SetShowCmd(int command);
    void GetIconLocation(nint path, int length, out int iconIndex);
    void SetIconLocation(string path, int iconIndex);
    void SetRelativePath(string path, uint reserved);
    void Resolve(nint window, uint flags);
    void SetPath(string path);
}

[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
[Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
internal partial interface IDesktopPropertyStore
{
    void GetCount(out uint count);
    void GetAt(uint index, out DesktopPropertyKey key);
    void GetValue(in DesktopPropertyKey key, out DesktopPropertyVariant value);
    void SetValue(in DesktopPropertyKey key, in DesktopPropertyVariant value);
    void Commit();
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct DesktopPropertyKey(Guid format, uint id)
{
    readonly Guid format = format;
    readonly uint id = id;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct DesktopPropertyVariant(ushort type, nint value)
{
    readonly ushort type = type;
    readonly ushort reserved1;
    readonly ushort reserved2;
    readonly ushort reserved3;
    readonly nint value = value;
    // The second pointer-sized slot accounts for the PROPVARIANT union's largest member.
    readonly nint reservedUnion;
}
#endif

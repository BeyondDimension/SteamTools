#if WINDOWS
// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
// https://github.com/microsoft/nodejstools/blob/main/Nodejs/Product/Nodejs/SharedProject/SystemUtilities.cs
using BD.WTTS.Helpers;
using System.Runtime.InteropServices.Marshalling;

// ReSharper disable once CheckNamespace
namespace Microsoft.NodejsTools.SharedProject;

/// <summary>
/// Utility for accessing window IShell* interfaces in order to use them to launch a process unelevated
/// </summary>
static partial class SystemUtility
{
    static readonly StrategyBasedComWrappers wrappers = new();

    internal static void ExecuteProcessUnElevated(string process, string args, string currentDirectory = "")
    {
        var initialized = CoInitializeEx(0, 2);
        if (initialized < 0 && initialized != unchecked((int)0x80010106))
            Marshal.ThrowExceptionForHR(initialized);

        ComObject? windows = null, provider = null, browser = null, view = null, folder = null, dispatch = null;
        try
        {
            var classId = new Guid("9BA05972-F6A8-11CF-A442-00A0C90A8F39");
            var interfaceId = new Guid("85CB6900-4D95-11CF-960C-0080C7F4EE85");
            Marshal.ThrowExceptionForHR(CoCreateInstance(in classId, 0, 4, in interfaceId, out var pointer));
            windows = WrapOwnedPointer(pointer);
            using var location = ComVariant.Create(0); // CSIDL_DESKTOP
            var nativeLocation = AutomationVariant.Borrow(in location);
            var root = default(AutomationVariant);
            provider = WrapOwnedPointer(((IShellWindowsNative)(object)windows).FindWindowSW(in nativeLocation, in root, 8, out _, 1));

            var serviceId = new Guid("4C96BE40-915C-11CF-99D3-00AA004AE837");
            interfaceId = new Guid("000214E2-0000-0000-C000-000000000046");
            browser = WrapOwnedPointer(((IShellServiceProviderNative)(object)provider).QueryService(in serviceId, in interfaceId));
            view = WrapOwnedPointer(((IShellBrowserNative)(object)browser).QueryActiveShellView());
            interfaceId = new Guid("00020400-0000-0000-C000-000000000046");
            folder = WrapOwnedPointer(((IShellViewNative)(object)view).GetItemObject(0, in interfaceId));
            dispatch = WrapOwnedPointer(((IShellFolderViewDualNative)(object)folder).GetApplication());

            using var arguments = ComVariant.Create(args);
            using var directory = ComVariant.Create(currentDirectory);
            using var operation = ComVariant.Create(string.Empty);
            using var show = ComVariant.Create(1);
            ((IShellDispatch2Native)(object)dispatch).ShellExecute(process,
                AutomationVariant.Borrow(in arguments), AutomationVariant.Borrow(in directory),
                AutomationVariant.Borrow(in operation), AutomationVariant.Borrow(in show));
        }
        finally
        {
            dispatch?.FinalRelease();
            folder?.FinalRelease();
            view?.FinalRelease();
            browser?.FinalRelease();
            provider?.FinalRelease();
            windows?.FinalRelease();
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

// Blittable view of VARIANT: 8-byte header followed by its largest union member
// (two pointers). Borrowed BSTR/interface pointers remain owned by ComVariant.
[StructLayout(LayoutKind.Sequential)]
internal struct AutomationVariant
{
    ulong header;
    nint data;
    nint recordInfo;

    internal static AutomationVariant Borrow(in ComVariant value)
        => System.Runtime.CompilerServices.Unsafe.As<ComVariant, AutomationVariant>(
            ref System.Runtime.CompilerServices.Unsafe.AsRef(in value));
}

// Dual automation interfaces include their IDispatch slots through the shared base.
[GeneratedComInterface]
[Guid("85CB6900-4D95-11CF-960C-0080C7F4EE85")]
internal partial interface IShellWindowsNative : IDispatchNative
{
    int GetCount();
    nint Item(AutomationVariant index);
    nint NewEnum();
    void Register(nint dispatch, int window, int shellClass, out int cookie);
    void RegisterPending(int thread, in AutomationVariant location, in AutomationVariant root, int shellClass, out int cookie);
    void Revoke(int cookie);
    void OnNavigate(int cookie, in AutomationVariant location);
    void OnActivated(int cookie, short active);
    nint FindWindowSW(in AutomationVariant location, in AutomationVariant root, int shellClass, out int window, int options);
}

[GeneratedComInterface]
[Guid("6D5140C1-7436-11CE-8034-00AA006009FA")]
internal partial interface IShellServiceProviderNative
{
    nint QueryService(in Guid service, in Guid interfaceId);
}

[GeneratedComInterface]
[Guid("000214E2-0000-0000-C000-000000000046")]
internal partial interface IShellBrowserNative
{
    void GetWindow(out nint window);
    void ContextSensitiveHelp(int enterMode);
    void InsertMenusSB(nint menu, nint widths);
    void SetMenuSB(nint menu, nint oleMenu, nint activeObjectWindow);
    void RemoveMenusSB(nint menu);
    void SetStatusTextSB(nint text);
    void EnableModelessSB(int enable);
    void TranslateAcceleratorSB(nint message, ushort id);
    void BrowseObject(nint itemIdList, uint flags);
    void GetViewStateStream(uint mode, out nint stream);
    void GetControlWindow(uint id, out nint window);
    void SendControlMsg(uint id, uint message, nuint wParam, nint lParam, out nint result);
    nint QueryActiveShellView();
}

[GeneratedComInterface]
[Guid("000214E3-0000-0000-C000-000000000046")]
internal partial interface IShellViewNative
{
    void GetWindow(out nint window);
    void ContextSensitiveHelp(int enterMode);
    void TranslateAccelerator(nint message);
    void EnableModeless(int enable);
    void UIActivate(uint state);
    void Refresh();
    void CreateViewWindow(nint previous, nint settings, nint browser, nint rectangle, out nint window);
    void DestroyViewWindow();
    void GetCurrentInfo(nint settings);
    void AddPropertySheetPages(uint reserved, nint callback, nint parameter);
    void SaveViewState();
    void SelectItem(nint itemIdList, uint flags);
    nint GetItemObject(uint aspect, in Guid interfaceId);
}

[GeneratedComInterface]
[Guid("E7A1AF80-4D96-11CF-960C-0080C7F4EE85")]
internal partial interface IShellFolderViewDualNative : IDispatchNative
{
    nint GetApplication();
}

[GeneratedComInterface]
[Guid("A4C6892C-3BA9-11D2-9DEA-00C04FB16162")]
internal partial interface IShellDispatch2Native : IDispatchNative
{
    nint GetApplication();
    nint GetParent();
    nint NameSpace(AutomationVariant directory);
    nint BrowseForFolder(int window, [MarshalAs(UnmanagedType.BStr)] string title, int options, AutomationVariant root);
    nint Windows();
    void Open(AutomationVariant directory);
    void Explore(AutomationVariant directory);
    void MinimizeAll();
    void UndoMinimizeAll();
    void FileRun();
    void CascadeWindows();
    void TileVertically();
    void TileHorizontally();
    void ShutdownWindows();
    void Suspend();
    void EjectPC();
    void SetTime();
    void TrayProperties();
    void Help();
    void FindFiles();
    void FindComputer();
    void RefreshMenu();
    void ControlPanelItem([MarshalAs(UnmanagedType.BStr)] string directory);
    int IsRestricted([MarshalAs(UnmanagedType.BStr)] string group, [MarshalAs(UnmanagedType.BStr)] string restriction);
    void ShellExecute([MarshalAs(UnmanagedType.BStr)] string file, AutomationVariant args, AutomationVariant directory,
        AutomationVariant operation, AutomationVariant show);
}
#endif
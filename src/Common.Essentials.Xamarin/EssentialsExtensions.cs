#if MAUI
using GdiPlusColor = System.Drawing.Color;
using MauiColor = Microsoft.Maui.Graphics.Color;
using XEBrowserLaunchFlags = Microsoft.Maui.ApplicationModel.BrowserLaunchFlags;
using XEBrowserLaunchMode = Microsoft.Maui.ApplicationModel.BrowserLaunchMode;
using XEBrowserLaunchOptions = Microsoft.Maui.ApplicationModel.BrowserLaunchOptions;
using XEBrowserTitleMode = Microsoft.Maui.ApplicationModel.BrowserTitleMode;
using XEDeviceIdiom = Microsoft.Maui.Devices.DeviceIdiom;
using XEDeviceType = Microsoft.Maui.Devices.DeviceType;
using XEEmailAttachment = Microsoft.Maui.ApplicationModel.Communication.EmailAttachment;
using XEEmailBodyFormat = Microsoft.Maui.ApplicationModel.Communication.EmailBodyFormat;
using XEEmailMessage = Microsoft.Maui.ApplicationModel.Communication.EmailMessage;
using XEFileBase = Microsoft.Maui.Storage.FileBase;
using XEFilePickerFileType = Microsoft.Maui.Storage.FilePickerFileType;
using XEFileResult = Microsoft.Maui.Storage.FileResult;
using XENetworkAccess = Microsoft.Maui.Networking.NetworkAccess;
using XEPermissionStatus = Microsoft.Maui.ApplicationModel.PermissionStatus;
using XEPickOptions = Microsoft.Maui.Storage.PickOptions;
#else
using XEBrowserLaunchFlags = Xamarin.Essentials.BrowserLaunchFlags;
using XEBrowserLaunchMode = Xamarin.Essentials.BrowserLaunchMode;
using XEBrowserLaunchOptions = Xamarin.Essentials.BrowserLaunchOptions;
using XEBrowserTitleMode = Xamarin.Essentials.BrowserTitleMode;
using XEDeviceIdiom = Xamarin.Essentials.DeviceIdiom;
using XEDeviceType = Xamarin.Essentials.DeviceType;
using XEEmailAttachment = Xamarin.Essentials.EmailAttachment;
using XEEmailBodyFormat = Xamarin.Essentials.EmailBodyFormat;
using XEEmailMessage = Xamarin.Essentials.EmailMessage;
using XEFileBase = Xamarin.Essentials.FileBase;
using XEFilePickerFileType = Xamarin.Essentials.FilePickerFileType;
using XEFileResult = Xamarin.Essentials.FileResult;
using XENetworkAccess = Xamarin.Essentials.NetworkAccess;
using XEPermissionStatus = Xamarin.Essentials.PermissionStatus;
using XEPickOptions = Xamarin.Essentials.PickOptions;
#endif
using System.Application.Services;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Application;

public static partial class EssentialsExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XEBrowserLaunchMode Convert(this BrowserLaunchMode value)
        => (XEBrowserLaunchMode)value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XEBrowserTitleMode Convert(this BrowserTitleMode value)
        => (XEBrowserTitleMode)value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XEBrowserLaunchFlags Convert(this BrowserLaunchFlags value)
        => (XEBrowserLaunchFlags)value;

#if MAUI
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MauiColor ConvertMaui(this GdiPlusColor value) => new(value.R, value.G, value.B, value.A);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MauiColor? ConvertMaui(this GdiPlusColor? value) => value.HasValue ? value.ConvertMaui() : null;
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XEBrowserLaunchOptions Convert(this BrowserLaunchOptions value)
       => new()
       {
#if MAUI
           PreferredToolbarColor = value.PreferredToolbarColor.ConvertMaui(),
           PreferredControlColor = value.PreferredControlColor.ConvertMaui(),
#else
           PreferredToolbarColor = value.PreferredToolbarColor,
           PreferredControlColor = value.PreferredControlColor,
#endif
           LaunchMode = value.LaunchMode.Convert(),
           TitleMode = value.TitleMode.Convert(),
           Flags = value.Flags.Convert(),
       };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DeviceType Convert(this XEDeviceType value)
        => (DeviceType)value;

    /// <summary>
    /// 将 <see cref="XEDeviceIdiom"/> 转换为 <see cref="DeviceIdiom"/>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DeviceIdiom Convert(this XEDeviceIdiom value) => value.ToString() switch
    {
        nameof(XEDeviceIdiom.Phone) => DeviceIdiom.Phone,
        nameof(XEDeviceIdiom.Tablet) => DeviceIdiom.Tablet,
        nameof(XEDeviceIdiom.Desktop) => DeviceIdiom.Desktop,
        nameof(XEDeviceIdiom.TV) => DeviceIdiom.TV,
        nameof(XEDeviceIdiom.Watch) => DeviceIdiom.Watch,
        _ => DeviceIdiom.Unknown,
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XEEmailBodyFormat Convert(this EmailBodyFormat value)
        => (XEEmailBodyFormat)value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XEEmailAttachment Convert(this IEmailAttachment value)
    {
        if (value is XEEmailAttachment value2) return value2;
        if (value is XEFileBase value3) return new(value3);
        var contentType = value.ContentType;
        return contentType == null ? new(value.FullPath) : new(value.FullPath, contentType);
    }

    [return: NotNullIfNotNull("value")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XEEmailMessage? Convert(this EmailMessage? value)
       => value == null ? null : new()
       {
           Subject = value.Subject,
           Body = value.Body,
           BodyFormat = value.BodyFormat.Convert(),
           To = value.To,
           Cc = value.Cc,
           Bcc = value.Bcc,
           Attachments = value.Attachments.Select(Convert).ToList(),
       };

    /// <summary>
    /// 将 <see cref="DevicePlatform"/> 转换为 <see cref="Platform"/>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Platform Convert(this DevicePlatform value) => value.ToString() switch
    {
        nameof(DevicePlatform.Android) => Platform.Android,
#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable CS0618 // 类型或成员已过时
#pragma warning disable CS0612 // 类型或成员已过时
        nameof(DevicePlatform.UWP) => Platform.UWP,
#pragma warning restore CS0612 // 类型或成员已过时
#pragma warning restore CS0618 // 类型或成员已过时
#pragma warning restore IDE0079 // 请删除不必要的忽略
        nameof(DevicePlatform.iOS) or
        nameof(DevicePlatform.tvOS) or
        nameof(DevicePlatform.watchOS) => Platform.Apple,
        nameof(DevicePlatform.Tizen) => Platform.Linux,
#if MAUI
        nameof(DevicePlatform.WinUI) => Platform.WinUI,
        nameof(DevicePlatform.MacCatalyst) => Platform.Apple,
#endif
        _ => Platform.Unknown,
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PermissionStatus Convert(this XEPermissionStatus value)
        => (PermissionStatus)value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NetworkAccess Convert(this XENetworkAccess value)
        => (NetworkAccess)value;

    [return: NotNullIfNotNull("value")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XEFilePickerFileType? Convert(this IFilePickerFileType? value)
    {
        if (value == null) return null;
        if (value is FilePickerFileTypeWrapper.ClassWrapper wrapper) return wrapper.Value;
        return new FilePickerFileTypeWrapper.InterfaceWrapper(value);
    }

    [return: NotNullIfNotNull("value")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IFilePickerFileType? Convert(this XEFilePickerFileType? value)
        => value == null ? null : new FilePickerFileTypeWrapper.ClassWrapper(value);

    static class FilePickerFileTypeWrapper
    {
        public sealed class ClassWrapper : IFilePickerFileType
        {
            public XEFilePickerFileType Value { get; }

            public ClassWrapper(XEFilePickerFileType value)
            {
                Value = value;
            }

            IEnumerable<string>? IFilePickerFileType.GetPlatformFileType(Platform _) => Value.Value;
        }

        public sealed class InterfaceWrapper : XEFilePickerFileType
        {
            public new IFilePickerFileType Value { get; }

            public InterfaceWrapper(IFilePickerFileType value)
            {
                Value = value;
            }

            protected override IEnumerable<string> GetPlatformFileType(DevicePlatform _)
            {
                var platform = IDeviceInfoPlatformService.Platform;
                return Value.GetPlatformFileType(platform)!;
            }
        }
    }

    [return: NotNullIfNotNull("value")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XEPickOptions? Convert(this PickOptions? value)
       => value == null ? null : new()
       {
           PickerTitle = value.PickerTitle,
           FileTypes = value.FileTypes.Convert(),
       };

    [return: NotNullIfNotNull("value")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IFileResult? Convert(this XEFileResult? value)
    {
        if (value == null) return null;
        return new FileResultWrapper(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<IFileResult> Convert(this IEnumerable<XEFileResult> value)
    {
        foreach (var item in value)
        {
            yield return item.Convert();
        }
    }

    sealed class FileResultWrapper : IFileResult
    {
        public FileResultWrapper(XEFileResult value)
        {
            Value = value;
        }

        public XEFileResult Value { get; }

        string IFileBase.FullPath => Value.FullPath;

        string? IFileBase.ContentType
        {
            get => Value.ContentType;
            set => Value.ContentType = value!;
        }

        string IFileBase.FileName
        {
            get => Value.FileName;
            set => Value.FileName = value;
        }

        Task<Stream> IFileBase.OpenReadAsync() => Value.OpenReadAsync();
    }
}
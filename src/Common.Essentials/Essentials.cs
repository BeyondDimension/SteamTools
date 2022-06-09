using System.Application.Services;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace System.Application;

public static class Essentials
{
    public static bool IsSupported { get; internal set; }

    /// <summary>
    /// 是否支持保存文件对话框。
    /// </summary>
    public static bool IsSupportedSaveFileDialog { get; internal set; }
}

/// <summary>
/// 打开浏览器的可选设置。
/// </summary>
public sealed class BrowserLaunchOptions
{
    /// <summary>
    /// 背景工具条的首选颜色。
    /// </summary>
    public Color? PreferredToolbarColor { get; set; }

    /// <summary>
    /// 浏览器上控件的首选颜色。
    /// </summary>
    public Color? PreferredControlColor { get; set; }

    /// <summary>
    /// 浏览器的启动类型。
    /// </summary>
    public BrowserLaunchMode LaunchMode { get; set; } = BrowserLaunchMode.SystemPreferred;

    /// <summary>
    /// 标题显示的首选模式。
    /// </summary>
    public BrowserTitleMode TitleMode { get; set; } = BrowserTitleMode.Default;

    /// <summary>
    /// 额外的启动标志，根据设备和启动模式可能生效，也可能不生效。
    /// </summary>
    public BrowserLaunchFlags Flags { get; set; } = BrowserLaunchFlags.None;
}

/// <summary>
/// 标题模式
/// </summary>
public enum BrowserTitleMode
{
    /// <summary>
    /// 使用系统默认显示。
    /// </summary>
    Default,

    /// <summary>
    /// 显示标题。
    /// </summary>
    Show,

    /// <summary>
    /// 隐藏标题。
    /// </summary>
    Hide,
}

/// <summary>
/// 可以设置的额外标志，以控制浏览器的打开方式。
/// <para>这个枚举支持其成员值的位法组合。</para>
/// </summary>
[Flags]
public enum BrowserLaunchFlags
{
    /// <summary>
    /// 没有额外的标志。这是默认的。
    /// </summary>
    None = 0,

    /// <summary>
    /// 在安卓系统上，如果有的话，在当前活动旁边启动新活动。
    /// </summary>
    LaunchAdjacent = 1,

    /// <summary>
    /// 在iOS上，在支持的情况下，用系统首选的浏览器启动浏览器作为页面表。
    /// </summary>
    PresentAsPageSheet = 2,

    /// <summary>
    /// 在iOS上，在支持的情况下，用系统首选的浏览器启动浏览器作为表单。
    /// </summary>
    PresentAsFormSheet = 4,
}

/// <summary>
/// 打开浏览器的启动类型。
/// </summary>
public enum BrowserLaunchMode
{
    /// <summary>
    /// 启动优化的系统浏览器，并留在你的应用程序内。(Chrome Custom Tabs 和 SFSafariViewController)。
    /// </summary>
    SystemPreferred,

    /// <summary>
    /// 使用默认的外部启动器，在应用程序之外打开浏览器。
    /// </summary>
    External,
}

public enum EmailBodyFormat
{
    PlainText,
    Html,
}

public class EmailMessage
{
    public EmailMessage()
    {
    }

    public EmailMessage(string subject, string body, params string[] to)
    {
        Subject = subject;
        Body = body;
        To = to?.ToList() ?? new List<string>();
    }

    public string? Subject { get; set; }

    public string? Body { get; set; }

    public EmailBodyFormat BodyFormat { get; set; }

    public List<string> To { get; set; } = new List<string>();

    public List<string> Cc { get; set; } = new List<string>();

    public List<string> Bcc { get; set; } = new List<string>();

    public List<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();
}

public interface IFileBase
{
    string FullPath { get; }

    string? ContentType { get; set; }

    string FileName { get; set; }

    Task<Stream> OpenReadAsync();
}

public abstract class FileBase : IFileBase
{
    public FileBase(string fullPath)
    {
        FullPath = fullPath;
    }

    public FileBase(string fullPath, string contentType) : this(fullPath)
    {
        ContentType = contentType;
    }

    public FileBase(FileBase file)
    {
        FullPath = file.FullPath;
        ContentType = file.ContentType;
        FileName = file.FileName;
    }

    public string FullPath { get; }

    public string? ContentType { get; set; }

    public virtual Task<Stream> OpenReadAsync()
    {
        var fileStream = IOPath.OpenRead(FullPath);
        return Task.FromResult<Stream>(fileStream.ThrowIsNull());
    }

    string? fileName;

    public string FileName
    {
        get => GetFileName();
        set => fileName = value;
    }

    internal string GetFileName()
    {
        // try the provided file name
        if (!string.IsNullOrWhiteSpace(fileName))
            return fileName;

        // try get from the path
        if (!string.IsNullOrWhiteSpace(FullPath))
            return Path.GetFileName(FullPath);

        // this should never happen as the path is validated in the constructor
        throw new InvalidOperationException($"Unable to determine the file name from '{FullPath}'.");
    }
}

public interface IEmailAttachment : IFileBase
{

}

public class EmailAttachment : FileBase, IEmailAttachment
{
    public EmailAttachment(string fullPath) : base(fullPath)
    {

    }

    public EmailAttachment(string fullPath, string contentType) : base(fullPath, contentType)
    {

    }

    public EmailAttachment(FileBase file) : base(file)
    {
    }
}

public interface IFileResult : IFileBase
{
}

public class FileResult : FileBase, IFileResult
{
    public FileResult(string fullPath) : base(fullPath)
    {

    }

    public FileResult(string fullPath, string contentType) : base(fullPath, contentType)
    {

    }

    public FileResult(FileBase file) : base(file)
    {
    }
}

public sealed class SaveFileResult : IDisposable
{
    readonly string? fullPath;
    readonly Stream? stream;
    readonly string? @string;
    bool disposedValue;

    public SaveFileResult(string fullPath)
    {
        this.fullPath = fullPath;
    }

    public SaveFileResult(Stream stream, string? @string = null)
    {
        this.stream = stream;
        this.@string = @string;
    }

    /// <summary>
    /// 打开写入流，在桌面平台上为 <see cref="FileStream"/>，在 Android 上为 Java.IO.OutputStream
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public Stream OpenWrite()
    {
        if (fullPath != null)
            return new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.Write);
        if (stream != null)
            return stream;
        throw new NotSupportedException();
    }

    public override string ToString()
    {
        if (fullPath != null)
            return fullPath;
        if (@string != null)
            return @string;
        return base.ToString();
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: 释放托管状态(托管对象)
                stream?.Dispose();
            }

            // TODO: 释放未托管的资源(未托管的对象)并重写终结器
            // TODO: 将大型字段设置为 null
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

public class PickOptions
{
    public static PickOptions Default =>
        new()
        {
            FileTypes = null,
        };

    public static PickOptions Images =>
        new()
        {
            FileTypes = IFilePickerFileType.Images(),
        };

    public string? PickerTitle { get; set; }

    public IFilePickerFileType? FileTypes { get; set; }
}

public interface IFilePickerFileType
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static IFilePickerFileType? Images() => IFilePickerPlatformService.Instance?.Images;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static IFilePickerFileType? Png() => IFilePickerPlatformService.Instance?.Png;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static IFilePickerFileType? Jpeg() => IFilePickerPlatformService.Instance?.Jpeg;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static IFilePickerFileType? Videos() => IFilePickerPlatformService.Instance?.Videos;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static IFilePickerFileType? Pdf() => IFilePickerPlatformService.Instance?.Pdf;

    IEnumerable<string>? GetPlatformFileType(Platform platform);
}

public class SaveOptions : PickOptions
{
    public string? InitialFileName { get; set; }
}

public abstract class FilePickerFileType : IFilePickerFileType
{
    IEnumerable<string>? IFilePickerFileType.GetPlatformFileType(Platform platform) => GetPlatformFileType(platform);

    protected abstract IEnumerable<string>? GetPlatformFileType(Platform platform);

    sealed class Fixed : FilePickerFileType
    {
        readonly IEnumerable<string> fileTypes;

        public Fixed(IEnumerable<string> fileTypes)
        {
            this.fileTypes = fileTypes;
        }

        protected override IEnumerable<string>? GetPlatformFileType(Platform _) => fileTypes;
    }

    [return: NotNullIfNotNull("values")]
    public static FilePickerFileType? Parse(IEnumerable<string>? values) => values == null ? null : new Fixed(values);

    [return: NotNullIfNotNull("values")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator FilePickerFileType?(HashSet<string>? values) => Parse(values);

    [return: NotNullIfNotNull("values")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator FilePickerFileType?(string[]? values) => Parse(values);

    [return: NotNullIfNotNull("values")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator FilePickerFileType?(Collection<string>? values) => Parse(values);

    [return: NotNullIfNotNull("values")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator FilePickerFileType?(List<string>? values) => Parse(values);

    sealed class Platform<TValue> : FilePickerFileType where TValue : IEnumerable<string>
    {
        readonly IEnumerable<KeyValuePair<Platform, TValue?>> fileTypes;

        public Platform(IEnumerable<KeyValuePair<Platform, TValue?>> fileTypes)
        {
            this.fileTypes = fileTypes;
        }

        protected override IEnumerable<string>? GetPlatformFileType(Platform platform)
        {
            foreach (var fileType in fileTypes)
            {
                if (fileType.Key.HasFlag(platform))
                    return fileType.Value;
            }
            return null;
        }
    }

    [return: NotNullIfNotNull("values")]
    public static FilePickerFileType? Parse<TValue>(IEnumerable<KeyValuePair<Platform, TValue?>> values) where TValue : IEnumerable<string> => values == null ? null : new Platform<TValue>(values);

    [return: NotNullIfNotNull("values")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator FilePickerFileType?(Dictionary<Platform, IEnumerable<string>?> values) => Parse(values);

    [return: NotNullIfNotNull("values")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator FilePickerFileType?(Dictionary<Platform, string[]?> values) => Parse(values);

    [return: NotNullIfNotNull("values")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator FilePickerFileType?(Dictionary<Platform, List<string>?> values) => Parse(values);

    [return: NotNullIfNotNull("values")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator FilePickerFileType?(Dictionary<Platform, HashSet<string>?> values) => Parse(values);

    [return: NotNullIfNotNull("values")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator FilePickerFileType?(Dictionary<Platform, Collection<string>?> values) => Parse(values);

    public interface IFilePickerFileTypeWithName
    {
        IEnumerable<(string, IEnumerable<string>)> GetFileTypes();
    }

    sealed class Fixed<TValue> : FilePickerFileType, IFilePickerFileTypeWithName where TValue : IEnumerable<string>
    {
        readonly IEnumerable<(string name, TValue extensions)> fileTypes;

        public Fixed(IEnumerable<(string name, TValue extensions)> fileTypes)
        {
            this.fileTypes = fileTypes;
        }

        protected override IEnumerable<string> GetPlatformFileType(Platform _) => fileTypes.SelectMany(x => x.extensions);

        IEnumerable<(string, IEnumerable<string>)> IFilePickerFileTypeWithName.GetFileTypes()
        {
            foreach (var item in fileTypes)
            {
                yield return item;
            }
        }
    }

    [return: NotNullIfNotNull("values")]
    public static FilePickerFileType? Parse<TValue>(IEnumerable<(string name, TValue extensions)> values) where TValue : IEnumerable<string> => values == null ? null : new Fixed<TValue>(values);

    [return: NotNullIfNotNull("values")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator FilePickerFileType?((string name, IEnumerable<string> extensions)[] values) => Parse(values);

    [return: NotNullIfNotNull("values")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator FilePickerFileType?((string name, List<string> extensions)[] values) => Parse(values);

    [return: NotNullIfNotNull("values")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator FilePickerFileType?((string name, string[] extensions)[] values) => Parse(values);
}

public enum PermissionStatus
{
    Unknown,
    Denied,
    Disabled,
    Granted,
    Restricted,
}

public enum NetworkAccess
{
    Unknown,
    None,
    Local,
    ConstrainedInternet,
    Internet,
}

public interface IBasePermission
{
    Task<PermissionStatus> CheckStatusAsync();

    Task<PermissionStatus> RequestAsync();

    void EnsureDeclared();

    bool ShouldShowRationale();
}

public interface IBasePermission<TPermission> : IBasePermission where TPermission : notnull
{
    static TPermission Instance => DI.Get<TPermission>();
}
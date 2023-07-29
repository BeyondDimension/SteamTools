using dotnetCampus.Ipc.CompilerServices.Attributes;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// 用于 IPC 的吐司通知管理
/// </summary>
[IpcPublic(Timeout = AssemblyInfo.IpcTimeout, IgnoresIpcException = true)]
public interface IPCToastService
{
    enum ToastText
    {
        CreateCertificateFaild = 1,
        CommunityFix_DNSErrorNotify,

        [Obsolete]
        CommunityFix_OnRunCatch,
    }

    /// <summary>
    /// Toast 显示的图标
    /// </summary>
    enum ToastIcon : byte
    {
        /// <summary>
        /// 无图标
        /// </summary>
        None,

        /// <summary>
        /// ℹ
        /// </summary>
        Info,

        /// <summary>
        /// ✅
        /// </summary>
        Success,

        /// <summary>
        /// ⚠️
        /// </summary>
        Warning,

        /// <summary>
        /// ❌
        /// </summary>
        Error,
    }

    void Show(ToastIcon icon, ToastText text, int? duration = null);

    void Show(ToastIcon icon, ToastText text, ToastLength duration);

    void Show(ToastIcon icon, ToastText text, int? duration = null, params string?[] args);

    void Show(ToastIcon icon, ToastText text, ToastLength duration, params string?[] args);

    void Show(ToastIcon icon, ToastText text, params string?[] args);

    void ShowAppend(ToastIcon icon, ToastText text, int? duration = null, string? appendText = null);

    void ShowAppend(ToastIcon icon, ToastText text, ToastLength duration, string? appendText);

    void ShowAppend(ToastIcon icon, ToastText text, string? appendText);
}
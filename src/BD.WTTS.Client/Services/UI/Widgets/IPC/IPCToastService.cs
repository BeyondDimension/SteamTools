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
        CommunityFix_OnRunCatch,
    }

    void Show(ToastText text, int? duration = null);

    void Show(ToastText text, ToastLength duration);

    void Show(ToastText text, int? duration = null, params object?[] args);

    void Show(ToastText text, ToastLength duration, params object?[] args);

    void Show(ToastText text, params object?[] args);

    void ShowAppend(ToastText text, int? duration = null, string? appendText = null);

    void ShowAppend(ToastText text, ToastLength duration, string? appendText);

    void ShowAppend(ToastText text, string? appendText);
}
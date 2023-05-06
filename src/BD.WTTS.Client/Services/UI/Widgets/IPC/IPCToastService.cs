#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable SA1209 // Using alias directives should be placed after other using directives
global using ToastLength = BD.Common.Enums.ToastLength;
#pragma warning restore SA1209 // Using alias directives should be placed after other using directives
#pragma warning restore IDE0079 // 请删除不必要的忽略
using dotnetCampus.Ipc.CompilerServices.Attributes;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// 用于 IPC 的 <see cref="IToastService"/>
/// </summary>
[IpcPublic(Timeout = 1000, IgnoresIpcException = true)]
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
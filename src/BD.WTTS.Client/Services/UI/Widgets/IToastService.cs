// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// 吐司通知管理
/// </summary>
[Mobius(
"""
Mobius.UI.Services.ToastService
""")]
public interface IToastService : IToast
{
    static IToastService Instance => Ioc.Get<IToastService>();

}
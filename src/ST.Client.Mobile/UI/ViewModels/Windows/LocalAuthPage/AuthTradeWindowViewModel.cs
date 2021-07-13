using ReactiveUI;
using System.Application.UI.Resx;

namespace System.Application.UI.ViewModels
{
    partial class AuthTradeWindowViewModel : PageViewModel
    {
        private string? _LoadingText;
        public string? LoadingText
        {
            get => _LoadingText;
            set => this.RaiseAndSetIfChanged(ref _LoadingText, value);
        }

        public enum ActionItem
        {
            ConfirmAll = 1,
            CancelAll,
            Refresh,
            Logout,
        }

        public static string ToString2(ActionItem action) => action switch
        {
            ActionItem.ConfirmAll => AppResources.LocalAuth_AuthTrade_ConfirmAll,
            ActionItem.CancelAll => AppResources.LocalAuth_AuthTrade_CancelAll,
            ActionItem.Refresh => AppResources.Refresh,
            ActionItem.Logout => AppResources.Logout,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };
    }
}
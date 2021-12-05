using System.Application.Services;
using System.Application.UI.Resx;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    partial class LocalAuthPageViewModel : IActionItem<LocalAuthPageViewModel.ActionItem>
    {
        public enum ActionItem
        {
            Add = 1,
            Encrypt,
            Export,
            Lock,
            Refresh,
        }

        string IActionItem<ActionItem>.ToString2(ActionItem action) => ToString2(action);

        public static string ToString2(ActionItem action) => action switch
        {
            ActionItem.Add => AppResources.Add,
            ActionItem.Encrypt => AppResources.Encrypt,
            ActionItem.Export => AppResources.Export,
            ActionItem.Lock => AppResources.Lock,
            ActionItem.Refresh => AppResources.Refresh,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };

        string IActionItem<ActionItem>.GetIcon(ActionItem action) => GetIcon(action);

        public static string GetIcon(ActionItem action) => action switch
        {
            ActionItem.Add => "baseline_add_black_24",
            ActionItem.Encrypt => "baseline_enhanced_encryption_black_24",
            ActionItem.Export => "baseline_save_alt_black_24",
            ActionItem.Lock => "baseline_lock_open_black_24",
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };

        public void MenuItemClick(ActionItem id)
        {
            switch (id)
            {
                case ActionItem.Add:
                    AddAuthCommand.Invoke();
                    break;
                case ActionItem.Encrypt:
                    EncryptionAuthCommand.Invoke();
                    break;
                case ActionItem.Export:
                    ExportAuthCommand.Invoke();
                    break;
                case ActionItem.Lock:
                    LockCommand.Invoke();
                    break;
                case ActionItem.Refresh:
                    RefreshAuthCommand.Invoke();
                    break;
            }
        }

        bool IActionItem<ActionItem>.IsPrimary(ActionItem action) => action switch
        {
            ActionItem.Add => true,
            _ => false,
        };

        public static LocalAuthPageViewModel Current
            => IViewModelManager.Instance.GetMainPageViewModel<LocalAuthPageViewModel>();
    }
}
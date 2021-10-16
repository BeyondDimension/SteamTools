using System.Application.UI.Resx;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    partial class LocalAuthPageViewModel
    {
        public enum ActionItem
        {
            Add = 1,
            Encrypt,
            Export,
            Lock,
            Refresh,
        }

        public static string ToString2(ActionItem action) => action switch
        {
            ActionItem.Add => AppResources.Add,
            ActionItem.Encrypt => AppResources.Encrypt,
            ActionItem.Export => AppResources.Export,
            ActionItem.Lock => AppResources.Lock,
            ActionItem.Refresh => AppResources.Refresh,
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
    }
}
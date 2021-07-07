using System.Application.UI.Resx;

namespace System.Application.UI.ViewModels
{
    partial class LocalAuthPageViewModel : ViewModelBase
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
    }
}
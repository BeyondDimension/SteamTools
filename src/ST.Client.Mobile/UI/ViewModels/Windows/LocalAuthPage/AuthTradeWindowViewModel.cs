using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.UI.Resx;
using System.Linq;
using System.Reactive.Linq;

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

        public void MenuItemClick(ActionItem id)
        {
            switch (id)
            {
                case ActionItem.ConfirmAll:
                    ConfirmAllButton_Click();
                    break;
                case ActionItem.CancelAll:
                    CancelAllButton_Click();
                    break;
                case ActionItem.Refresh:
                    Refresh_Click();
                    break;
                case ActionItem.Logout:
                    Logout_Click();
                    break;
            }
        }

        public static string ToString2(ActionItem action) => action switch
        {
            ActionItem.ConfirmAll => AppResources.LocalAuth_AuthTrade_ConfirmAll,
            ActionItem.CancelAll => AppResources.LocalAuth_AuthTrade_CancelAll,
            ActionItem.Refresh => AppResources.Refresh,
            ActionItem.Logout => AppResources.Logout,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };

        private bool _UnselectAll;
        /// <summary>
        /// 全不选
        /// </summary>
        public bool UnselectAll
        {
            get => _UnselectAll;
            set
            {
                if (this.RaiseAndSetIfChanged2(ref _UnselectAll, value)) return;
                foreach (var item in Confirmations)
                {
                    item.NotChecked = value;
                }
            }
        }

        private string _SelectAllText = string.Empty;
        /// <summary>
        /// 全选文字
        /// </summary>
        public string SelectAllText
        {
            get => _SelectAllText;
            set => this.RaiseAndSetIfChanged(ref _SelectAllText, value);
        }

        /// <summary>
        /// 注册全选监听
        /// </summary>
        void RegisterSelectAllObservable() => this.WhenAnyValue(x => x.Confirmations)
            .Subscribe(x => x?
                .ToObservableChangeSet()
                .AutoRefresh(x => x.NotChecked)
                .ToCollection()
                .Select(x =>
                {
                    var select_count = x.Count(y => !y.NotChecked);
                    return (select_count, count: x.Count);
                })
                .Subscribe(x =>
                {
                    var unselectAll = x.select_count != x.count;
                    if (_UnselectAll != unselectAll)
                    {
                        _UnselectAll = unselectAll;
                        this.RaisePropertyChanged(nameof(UnselectAll));
                    }
                    SelectAllText = AppResources.SelectAllText_.Format(x.select_count, x.count);
                })
                .AddTo(this)
            ).AddTo(this);
    }
}
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    partial class AuthTradeWindowViewModel
    {
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

        private bool isUnselectAllChangeing;
        private bool _UnselectAll;
        /// <summary>
        /// 全不选
        /// </summary>
        public bool UnselectAll
        {
            get => _UnselectAll;
            set
            {
                if (this.RaiseAndSetIfChangedReturnIsNotChange(ref _UnselectAll, value)) return;
                isUnselectAllChangeing = true;
                foreach (var item in Confirmations)
                {
                    if (item.IsOperate != 0) continue;
                    item.NotChecked = value;
                }
                isUnselectAllChangeing = false;
                SelectAllText = AppResources.SelectAllText_.Format(value ? 0 : Confirmations.Count, Confirmations.Count);
            }
        }

        private string? _SelectAllText;
        /// <summary>
        /// 全选文字，当值为 <see langword="null"/> 时应隐藏底部操作区域
        /// </summary>
        public string? SelectAllText
        {
            get => _SelectAllText;
            set => this.RaiseAndSetIfChanged(ref _SelectAllText, value);
        }

        /// <summary>
        /// 注册全选监听
        /// </summary>
        void RegisterSelectAllObservable() => this.WhenAnyValue(x => x.Confirmations)
            .SubscribeInMainThread(x => x?
                .ToObservableChangeSet()
                .AutoRefresh(x => x.NotChecked)
                .ToCollection()
                .Select(x =>
                {
                    int select_count, count;
                    if (isUnselectAllChangeing)
                    {
                        select_count = 0;
                        count = 0;
                    }
                    else
                    {
                        select_count = x.Count(y => y.IsOperate == 0 && !y.NotChecked);
                        count = x.Count(y => y.IsOperate == 0);
                    }
                    return (select_count, count);
                })
                .Subscribe(x =>
                {
                    if (isUnselectAllChangeing) return;
                    if (x.count > 0)
                    {
                        var unselectAll = x.select_count != x.count;
                        if (_UnselectAll != unselectAll)
                        {
                            _UnselectAll = unselectAll;
                            this.RaisePropertyChanged(nameof(UnselectAll));
                        }
                        SelectAllText = AppResources.SelectAllText_.Format(x.select_count, x.count);
                    }
                    else
                    {
                        SelectAllText = null;
                    }
                }).AddTo(this))
            .AddTo(this);
    }
}

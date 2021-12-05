using System;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Xamarin.Forms;
using ReactiveUI;
using System.Application.UI.Resx;

namespace System.Application.UI.Views
{
    /// <inheritdoc cref="IActionItem{TEnum}"/>
    public static class ToolbarItemHelper
    {
        /// <summary>
        /// 初始化右上角 Toolbar Menu
        /// </summary>
        /// <typeparam name="TEnum">视图模型中定义的Menu枚举类型</typeparam>
        /// <param name="viewModel">当前视图模型</param>
        /// <param name="except">可选排除枚举项</param>
        /// <returns></returns>
        public static IReadOnlyDictionary<TEnum, ToolbarItem> InitToolbarItems<TEnum, TViewModel>(
            this TViewModel viewModel,
            IEnumerable<TEnum>? except = null)
            where TEnum : struct, Enum
            where TViewModel : ViewModelBase, IActionItem<TEnum>
        {
            var query = Enum2.GetAll<TEnum>().AsEnumerable();
            if (except.Any_Nullable()) query = query.Except(except);
            var command = ReactiveCommand.Create<TEnum>(viewModel.MenuItemClick);
            var dict = query.ToDictionary(x => x, x =>
            {
                var item = new ToolbarItem
                {
                    IconImageSource = viewModel.GetIcon(x),
                    Command = command,
                    CommandParameter = x,
                    Order = viewModel.IsPrimary(x) ? ToolbarItemOrder.Primary : ToolbarItemOrder.Secondary,
                };

                return item;
            });
            R.Subscribe(() =>
            {
                foreach (var item in dict)
                {
                    item.Value.Text = viewModel.ToString2(item.Key);
                }
            }).AddTo(viewModel);
            return dict;
        }

        /// <inheritdoc cref="InitToolbarItems{TEnum, TViewModel}(TViewModel, IEnumerable{TEnum}?)"/>
        public static IReadOnlyDictionary<TEnum, ToolbarItem> InitToolbarItems<TEnum, TViewModel>(
           this TViewModel viewModel,
           params TEnum[] except)
            where TEnum : struct, Enum
            where TViewModel : ViewModelBase, IActionItem<TEnum>
        {
            IEnumerable<TEnum> except_ = except;
            return InitToolbarItems(viewModel, except_);
        }

        public static IReadOnlyDictionary<TEnum, ToolbarItem> AddToPage<TEnum>(this IReadOnlyDictionary<TEnum, ToolbarItem> actions, Page page)
        {
            page.ToolbarItems.AddRange(actions.Values);
            return actions;
        }
    }

    public interface ILocalAuthPage : IViewFor<LocalAuthPageViewModel>, IPage
    {
        IReadOnlyDictionary<LocalAuthPageViewModel.ActionItem, ToolbarItem> Actions { get; }

        protected static IReadOnlyDictionary<LocalAuthPageViewModel.ActionItem, ToolbarItem> InitToolbarItems(ILocalAuthPage page) => page.ViewModel!.InitToolbarItems(LocalAuthPageViewModel.ActionItem.Refresh).AddToPage(page.@this);
    }

    public interface IArchiSteamFarmPlusPage : IViewFor<ArchiSteamFarmPlusPageViewModel>, IPage
    {
        IReadOnlyDictionary<ArchiSteamFarmPlusPageViewModel.ActionItem, ToolbarItem> Actions { get; }

        protected static IReadOnlyDictionary<ArchiSteamFarmPlusPageViewModel.ActionItem, ToolbarItem> InitToolbarItems(IArchiSteamFarmPlusPage page) => page.ViewModel!.InitToolbarItems(ArchiSteamFarmPlusPageViewModel.ActionItem.Refresh).AddToPage(page.@this);
    }
}

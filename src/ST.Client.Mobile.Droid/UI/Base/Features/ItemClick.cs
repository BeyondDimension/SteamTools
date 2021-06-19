// RecycleView 列表项点击(ItemClick)与长按点击(ItemLongClick)事件(event)

using Android.Views;
using AndroidX.RecyclerView.Widget;
using ReactiveUI;
using System.Application.UI.Adapters.Internals;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Adapters
{
    partial class BaseRecycleViewAdapter<TViewHolder, TViewModel, TViewType> : IItemClickViewAdapter<TViewHolder, TViewModel>
    {
        protected virtual (bool isSuccess, TViewModel? viewModel) TryGetViewModelByPosition(int position)
        {
            if (position < 0 || position >= ViewModels.Count)
            {
                return (false, default);
            }
            var viewModel = ViewModels[position];
            return (true, viewModel);
        }

        public event EventHandler<ItemClickEventArgs<TViewModel>>? ItemClick;

        public event EventHandler<ItemClickEventArgs<TViewModel>>? ItemLongClick;

        /// <summary>
        /// 是否禁用点击事件绑定(包含[点击]与[长按点击])
        /// <para>默认值：<see langword="false"/></para>
        /// </summary>
        protected virtual bool DisableSetItemClickListener => false;

        protected virtual ItemClickEventArgs<TViewModel>? GetItemClickEventArgs(View view, TViewHolder holder, View.LongClickEventArgs? longClickEventArgs = null)
            => IItemClickViewAdapter<TViewHolder, TViewModel>.GetItemClickEventArgs(view, holder, longClickEventArgs, TryGetViewModelByPosition);
        ItemClickEventArgs<TViewModel>? IItemClickViewAdapter<TViewHolder, TViewModel>.GetItemClickEventArgs(View view, TViewHolder holder, View.LongClickEventArgs? longClickEventArgs)
            => GetItemClickEventArgs(view, holder, longClickEventArgs);

        /// <inheritdoc cref="IItemClickViewAdapter{TViewHolder, TViewModel}.SetItemClicks(IItemClickViewAdapter{TViewHolder, TViewModel}, TViewHolder, bool, EventHandler{ItemClickEventArgs{TViewModel}}?, EventHandler{ItemClickEventArgs{TViewModel}}?)"/>
        protected virtual void SetItemClicks(TViewHolder holder)
            => IItemClickViewAdapter<TViewHolder, TViewModel>.SetItemClicks(this, holder, DisableSetItemClickListener, ItemClick, ItemLongClick);
    }

    partial class BaseReactiveRecycleViewAdapter<TViewHolder, TViewModel, TViewType> : IItemClickViewAdapter<TViewHolder, TViewModel>
    {
        public event EventHandler<ItemClickEventArgs<TViewModel>>? ItemClick;

        public event EventHandler<ItemClickEventArgs<TViewModel>>? ItemLongClick;

        /// <summary>
        /// 是否禁用点击事件绑定(包含[点击]与[长按点击])
        /// <para>默认值：<see langword="false"/></para>
        /// </summary>
        protected virtual bool DisableSetItemClickListener => false;

        protected virtual ItemClickEventArgs<TViewModel>? GetItemClickEventArgs(View view, TViewHolder holder, View.LongClickEventArgs? longClickEventArgs = null)
            => IItemClickViewAdapter<TViewHolder, TViewModel>.GetItemClickEventArgs(view, holder, longClickEventArgs);
        ItemClickEventArgs<TViewModel>? IItemClickViewAdapter<TViewHolder, TViewModel>.GetItemClickEventArgs(View view, TViewHolder holder, View.LongClickEventArgs? longClickEventArgs)
            => GetItemClickEventArgs(view, holder, longClickEventArgs);

        /// <inheritdoc cref="IItemClickViewAdapter{TViewHolder, TViewModel}.SetItemClicks(IItemClickViewAdapter{TViewHolder, TViewModel}, TViewHolder, bool, EventHandler{ItemClickEventArgs{TViewModel}}?, EventHandler{ItemClickEventArgs{TViewModel}}?)"/>
        protected virtual void SetItemClicks(TViewHolder holder)
            => IItemClickViewAdapter<TViewHolder, TViewModel>.SetItemClicks(this, holder, DisableSetItemClickListener, ItemClick, ItemLongClick);
    }

    partial class BaseViewHolder : IItemClickViewHolder
    {
        public virtual bool ItemClickable => IItemClickViewHolder.DefaultItemClickable;

        public virtual View? ItemClickView => null;

        public virtual bool ItemLongClickable => false;

        public virtual View? ItemLongClickView => null;
    }

    partial class BaseReactiveViewHolder<TViewModel> : IItemClickViewHolder
    {
        public virtual bool ItemClickable => IItemClickViewHolder.DefaultItemClickable;

        public virtual View? ItemClickView => null;

        public virtual bool ItemLongClickable => false;

        public virtual View? ItemLongClickView => null;
    }

    public static partial class RecycleViewAdapterHelper
    {
        public static View? GetView<TViewModel>(this ItemClickEventArgs<TViewModel> eventArgs)
        {
            if (eventArgs is PlatformItemClickEventArgs<TViewModel> eventArgs_) return eventArgs_.View;
            return null;
        }
    }
}

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Adapters.Internals
{
    public interface IItemClickViewHolder
    {
        protected const bool DefaultItemClickable = true;

        /// <summary>
        /// 是否需要绑定列表项点击事件
        /// <para>默认值：<see langword="true"/></para>
        /// </summary>
        bool ItemClickable { get; }

        /// <summary>
        /// 绑定列表项点击事件的 <see cref="View"/>
        /// </summary>
        View? ItemClickView { get; }

        /// <summary>
        /// 是否需要绑定列表项[长按]点击事件
        /// <para>默认值：<see langword="false"/></para>
        /// </summary>
        bool ItemLongClickable { get; }

        /// <summary>
        /// 绑定列表项[长按]点击事件的 <see cref="View"/>
        /// </summary>
        View? ItemLongClickView { get; }
    }

    public interface IItemClickViewAdapter<TViewHolder, TViewModel>
        where TViewHolder : RecyclerView.ViewHolder, IItemClickViewHolder
    {
        ItemClickEventArgs<TViewModel>? GetItemClickEventArgs(View view, TViewHolder holder, View.LongClickEventArgs? longClickEventArgs = null);

        internal static ItemClickEventArgs<TViewModel>? GetItemClickEventArgs(
            View view, TViewHolder holder,
            View.LongClickEventArgs? longClickEventArgs = null,
            Func<int, (bool isSuccess, TViewModel? viewModel)>? tryGetViewModelByPosition = null)
        {
            var position = holder.BindingAdapterPosition;
            TViewModel current;
            if (holder is IViewFor viewForHolder && viewForHolder.ViewModel is TViewModel viewModel)
            {
                current = viewModel;
            }
            else
            {
                var current_ = tryGetViewModelByPosition?.Invoke(position);
                if (!current_.HasValue || !current_.Value.isSuccess) return null;
                current = current_.Value.viewModel!;
            }
            return new PlatformItemClickEventArgs<TViewModel>(view, position, current, longClickEventArgs);
        }

        /// <summary>
        /// 设置列表项的点击事件(ItemClick/ItemLongClick)
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="holder"></param>
        /// <param name="disableSetItemClickListener"></param>
        /// <param name="itemClick"></param>
        /// <param name="itemLongClick"></param>
        internal static void SetItemClicks(
            IItemClickViewAdapter<TViewHolder, TViewModel> adapter,
            TViewHolder holder,
            bool disableSetItemClickListener,
            EventHandler<ItemClickEventArgs<TViewModel>>? itemClick,
            EventHandler<ItemClickEventArgs<TViewModel>>? itemLongClick)
        {
            if (disableSetItemClickListener) return;

            View? itemClickView = null;

            #region ItemClick

            if (holder.ItemClickable)
            {
                itemClickView = holder.ItemClickView ?? holder.ItemView;
                if (!itemClickView.Focusable) itemClickView.Focusable = true;
                if (!itemClickView.Clickable) itemClickView.Clickable = true;
                itemClickView.Click += (sender, _) =>
                {
                    var eventArgs = adapter.GetItemClickEventArgs(itemClickView, holder);
                    if (eventArgs != null)
                    {
                        itemClick?.Invoke(sender, eventArgs);
                    }
                };
            }

            #endregion

            #region ItemLongClick

            if (holder.ItemLongClickable)
            {
                View? itemLongClickView;
                if (itemClickView != null && holder.ItemClickView == holder.ItemLongClickView)
                {
                    itemLongClickView = itemClickView;
                }
                else
                {
                    itemLongClickView = holder.ItemLongClickView ?? holder.ItemView;
                }
                if (!itemLongClickView.Focusable) itemLongClickView.Focusable = true;
                if (!itemLongClickView.LongClickable) itemLongClickView.LongClickable = true;
                itemLongClickView.LongClick += (sender, e) =>
                {
                    var eventArgs = adapter.GetItemClickEventArgs(itemLongClickView, holder, e);
                    if (eventArgs != null)
                    {
                        itemLongClick?.Invoke(sender, eventArgs);
                    }
                };
            }

            #endregion
        }
    }

    internal sealed class PlatformItemClickEventArgs<TViewModel> : EventArgs, ItemClickEventArgs<TViewModel>
    {
        readonly View.LongClickEventArgs? longClickEventArgs;

        public PlatformItemClickEventArgs(View view, int position, TViewModel current, View.LongClickEventArgs? longClickEventArgs)
        {
            View = view;
            Position = position;
            Current = current;
            this.longClickEventArgs = longClickEventArgs;
        }

        public View View { get; }

        public int Position { get; }

        public TViewModel Current { get; }

        public bool Handled
        {
            get
            {
                return longClickEventArgs?.Handled ?? default;
            }
            set
            {
                if (longClickEventArgs != null) longClickEventArgs.Handled = value;
            }
        }
    }
}
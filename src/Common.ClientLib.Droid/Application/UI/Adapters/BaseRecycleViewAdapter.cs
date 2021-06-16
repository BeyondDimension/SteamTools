using Android.Views;
using AndroidX.RecyclerView.Widget;
using System.Collections.Generic;

namespace System.Application.UI.Adapters
{
    /// <summary>
    /// <see cref="RecyclerView.Adapter"/> 的通用基类
    /// </summary>
    public abstract class BaseRecycleViewAdapter<TViewHolder, TViewModel, TViewType> :
        RecyclerView.Adapter, IAdapter<TViewModel>
        where TViewHolder : BaseViewHolder
        where TViewType : struct, IConvertible
    {
        protected IList<TViewModel>? viewModels;

        public virtual IList<TViewModel> ViewModels
        {
            get
            {
                if (viewModels == null)
                {
                    ICreateViewModels<TViewModel> i = this;
                    viewModels = i.CreateViewModels();
                }
                return viewModels;
            }
            set => viewModels = value;
        }

        public IAdapter<TViewModel> Interface => this;

        public event EventHandler<ItemClickEventArgs<TViewModel>>? ItemClick;

        public event EventHandler<ItemClickEventArgs<TViewModel>>? ItemLongClick;

        public override int ItemCount => ViewModels.Count;

        #region ItemClick/ItemLongClick

        /// <summary>
        /// 是否禁用点击事件绑定(包含[点击]与[长按点击])
        /// <para>默认值：<see langword="false"/></para>
        /// </summary>
        protected virtual bool DisableSetItemClickListener => false;

        protected virtual ItemClickEventArgs<TViewModel>? GetItemClickEventArgs(View view, TViewHolder holder, View.LongClickEventArgs? longClickEventArgs = null)
        {
            var position = holder.BindingAdapterPosition;
            if (position < 0 || position >= ViewModels.Count) return null;
            var current = ViewModels[position];
            return new PlatformItemClickEventArgs<TViewModel>(view, position, current, longClickEventArgs);
        }

        /// <summary>
        /// 设置列表项的点击事件(ItemClick/ItemLongClick)
        /// </summary>
        /// <param name="holder"></param>
        protected virtual void SetItemClicks(TViewHolder holder)
        {
            if (DisableSetItemClickListener) return;

            View? itemClickView = null;

            #region ItemClick

            if (holder.ItemClickable)
            {
                itemClickView = holder.ItemView.FindViewById(holder.ItemClickId) ?? holder.ItemView;
                if (!itemClickView.Focusable) itemClickView.Focusable = true;
                if (!itemClickView.Clickable) itemClickView.Clickable = true;
                itemClickView.Click += (sender, _) =>
                {
                    var eventArgs = GetItemClickEventArgs(itemClickView, holder);
                    if (eventArgs != null)
                    {
                        ItemClick?.Invoke(sender, eventArgs);
                    }
                };
            }

            #endregion

            #region ItemLongClick

            if (holder.ItemLongClickable)
            {
                View? itemLongClickView;
                if (itemClickView != null && holder.ItemClickId == holder.ItemLongClickId)
                {
                    itemLongClickView = itemClickView;
                }
                else
                {
                    itemLongClickView = holder.ItemView.FindViewById(holder.ItemLongClickId) ?? holder.ItemView;
                }
                if (!itemLongClickView.Focusable) itemLongClickView.Focusable = true;
                if (!itemLongClickView.LongClickable) itemLongClickView.LongClickable = true;
                itemLongClickView.LongClick += (sender, e) =>
                {
                    var eventArgs = GetItemClickEventArgs(itemLongClickView, holder, e);
                    if (eventArgs != null)
                    {
                        ItemLongClick?.Invoke(sender, eventArgs);
                    }
                };
            }

            #endregion
        }

        #endregion

        #region OnCreateViewHolder

        public virtual int? GetLayoutResource(TViewType viewType) => null;

        public virtual TViewHolder OnCreateViewHolder(View itemView, TViewType viewType)
        {
            var holder = (TViewHolder)Activator.CreateInstance(typeof(TViewHolder), itemView);
            return holder;
        }

        public virtual TViewHolder OnCreateViewHolder(TViewType viewType, ViewGroup parent)
        {
            int layoutResource = GetLayoutResource(viewType).ThrowIsNull(nameof(layoutResource));
            var layoutInflater = LayoutInflater.From(parent.Context);
            if (layoutInflater == null) throw new ArgumentNullException(nameof(layoutInflater));
            var view = layoutInflater.Inflate(layoutResource, parent, false);
            if (view == null) throw new ArgumentNullException(nameof(view));
            var holder = OnCreateViewHolder(view, viewType);
            return holder;
        }

        public sealed override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var viewType2 = viewType.ConvertToEnum<TViewType>();
            var holder = OnCreateViewHolder(viewType2, parent);
            SetItemClicks(holder);
            return holder;
        }

        #endregion

        #region OnBindViewHolder

        public abstract void OnBindViewHolder(TViewHolder holder, TViewModel item, int position);

        public sealed override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var item = ViewModels[position];
            var viewHolder = holder.Cast<TViewHolder>();
            OnBindViewHolder(viewHolder, item, position);
        }

        #endregion

        #region CreateViewModels

        protected abstract IList<TViewModel> CreateViewModels();

        protected abstract IList<TViewModel> CreateViewModels(IEnumerable<TViewModel> newViewModels);

        IList<TViewModel> ICreateViewModels<TViewModel>.CreateViewModels() => CreateViewModels();

        IList<TViewModel> ICreateViewModels<TViewModel>.CreateViewModels(IEnumerable<TViewModel> newViewModels) => CreateViewModels(newViewModels);

        #endregion
    }
}
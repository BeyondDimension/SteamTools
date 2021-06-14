using Android.Views;
using AndroidX.RecyclerView.Widget;
using ReactiveUI.AndroidX;
using System.Application.UI.ViewModels;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Adapters
{
    /// <summary>
    /// <see cref="RecyclerView.Adapter"/> 的通用基类
    /// </summary>
    public abstract class BaseViewHolder<TViewModel> : ReactiveRecyclerViewViewHolder<TViewModel>
        where TViewModel : ViewModelBase
    {
        public BaseViewHolder(View itemView) : base(itemView)
        {
        }

        /// <summary>
        /// 是否需要绑定列表项点击事件
        /// <para>默认值：<see langword="true"/></para>
        /// </summary>
        public virtual bool ItemClickable => true;

        /// <summary>
        /// 绑定列表项点击事件的 <see cref="View"/>
        /// </summary>
        public virtual View? ItemClickView => null;

        /// <summary>
        /// 是否需要绑定列表项[长按]点击事件
        /// <para>默认值：<see langword="false"/></para>
        /// </summary>
        public virtual bool ItemLongClickable => false;

        /// <summary>
        /// 绑定列表项[长按]点击事件的 <see cref="View"/>
        /// </summary>
        public virtual View? ItemLongClickView => null;
    }

    /// <inheritdoc cref="BaseViewHolder{TViewModel}"/>
    public abstract class BaseViewHolder<TViewModel, TViewType> : BaseViewHolder<TViewModel>
        where TViewModel : ViewModelBase
        where TViewType : struct, IConvertible
    {
        public BaseViewHolder(View itemView) : base(itemView)
        {
        }

        public new TViewType ItemViewType => base.ItemViewType.ConvertToEnum<TViewType>();
    }
}
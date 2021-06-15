using Android.Views;
using AndroidX.RecyclerView.Widget;
using System.Application.UI.ViewModels;
using System.Common;

namespace System.Application.UI.Adapters
{
    /// <summary>
    /// <see cref="RecyclerView.ViewHolder"/> 的通用基类
    /// </summary>
    public abstract class BaseViewHolder : RecyclerView.ViewHolder, IReadOnlyItemViewType
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
        /// 绑定列表项点击事件的Id
        /// </summary>
        public virtual int ItemClickId => R.id.adapter_item_click;

        /// <summary>
        /// 是否需要绑定列表项[长按]点击事件
        /// <para>默认值：<see langword="false"/></para>
        /// </summary>
        public virtual bool ItemLongClickable => false;

        /// <summary>
        /// 绑定列表项[长按]点击事件的Id
        /// </summary>
        public virtual int ItemLongClickId => R.id.adapter_item_long_click;
    }

    /// <inheritdoc cref="BaseViewHolder"/>
    public abstract class BaseViewHolder<TViewType> : BaseViewHolder,
        IReadOnlyItemViewType<TViewType>
        where TViewType : struct, IConvertible
    {
        public BaseViewHolder(View itemView) : base(itemView)
        {
        }

        public new TViewType ItemViewType => base.ItemViewType.ConvertToEnum<TViewType>();
    }
}
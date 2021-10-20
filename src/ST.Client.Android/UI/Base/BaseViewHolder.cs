using Android.Views;
using AndroidX.RecyclerView.Widget;
using ReactiveUI;
using ReactiveUI.AndroidX;
using System.Application.Mvvm;
using System.Collections.Generic;
using System.Reactive.Disposables;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Adapters
{
    /// <summary>
    /// <see cref="RecyclerView.ViewHolder"/> 的通用基类
    /// <list type="bullet">
    ///     <item><see href="https://developer.android.google.cn/guide/topics/ui/layout/recyclerview"/></item>
    ///     <item><see href="https://docs.microsoft.com/zh-cn/xamarin/android/user-interface/layouts/recycler-view"/></item>
    ///     <item>RecyclerView 可以让您轻松高效地显示大量数据。</item>
    ///     <item>您提供数据并定义每个列表项的外观，而 RecyclerView 库会根据需要动态创建元素。</item>
    ///     <item>顾名思义，RecyclerView 会回收这些单个的元素。</item>
    ///     <item>当列表项滚动出屏幕时，RecyclerView 不会销毁其视图。</item>
    ///     <item>相反，RecyclerView 会对屏幕上滚动的新列表项重用该视图。</item>
    ///     <item>这种重用可以显著提高性能，改善应用响应能力并降低功耗。</item>
    /// </list>
    /// </summary>
    public abstract partial class BaseViewHolder : RecyclerView.ViewHolder
    {
        public BaseViewHolder(View itemView) : base(itemView)
        {
        }
    }

    public abstract class BaseViewHolder<TViewType> : BaseViewHolder
        where TViewType : struct, IConvertible
    {
        public BaseViewHolder(View itemView) : base(itemView)
        {
        }

        public new TViewType ItemViewType => base.ItemViewType.ConvertToEnum<TViewType>();
    }

    /// <summary>
    /// <see cref="RecyclerView.ViewHolder"/> 的 ReactiveUI 基类
    /// <list type="bullet">
    ///     <item><see href="https://developer.android.google.cn/guide/topics/ui/layout/recyclerview"/></item>
    ///     <item><see href="https://docs.microsoft.com/zh-cn/xamarin/android/user-interface/layouts/recycler-view"/></item>
    ///     <item>RecyclerView 可以让您轻松高效地显示大量数据。</item>
    ///     <item>您提供数据并定义每个列表项的外观，而 RecyclerView 库会根据需要动态创建元素。</item>
    ///     <item>顾名思义，RecyclerView 会回收这些单个的元素。</item>
    ///     <item>当列表项滚动出屏幕时，RecyclerView 不会销毁其视图。</item>
    ///     <item>相反，RecyclerView 会对屏幕上滚动的新列表项重用该视图。</item>
    ///     <item>这种重用可以显著提高性能，改善应用响应能力并降低功耗。</item>
    /// </list>
    /// </summary>
    /// <typeparam name="TViewModel"></typeparam>
    public abstract partial class BaseReactiveViewHolder<TViewModel> : ReactiveRecyclerViewViewHolder<TViewModel>, IDisposableHolder, IReadOnlyViewFor<TViewModel>
        where TViewModel : class, IReactiveObject
    {
        CompositeDisposable? disposables;
        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => disposables!;

        public BaseReactiveViewHolder(View itemView) : base(itemView)
        {
        }

        public virtual void OnBind()
        {
            disposables?.Dispose();
            disposables = new();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                disposables?.Dispose();
                disposables = null;
            }
            base.Dispose(disposing);
        }
    }

    /// <inheritdoc cref="BaseReactiveViewHolder{TViewModel}"/>
    public abstract class BaseReactiveViewHolder<TViewModel, TViewType> : BaseReactiveViewHolder<TViewModel>
        where TViewModel : class, IReactiveObject
        where TViewType : struct, IConvertible
    {
        public BaseReactiveViewHolder(View itemView) : base(itemView)
        {
        }

        public new TViewType ItemViewType => base.ItemViewType.ConvertToEnum<TViewType>();
    }
}
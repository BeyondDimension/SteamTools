using AndroidX.RecyclerView.Widget;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.AndroidX;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Adapters
{
    /// <summary>
    /// <see cref="RecyclerView.Adapter"/> 的通用基类
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
    /// <typeparam name="TViewHolder"></typeparam>
    /// <typeparam name="TViewModel"></typeparam>
    /// <typeparam name="TViewType"></typeparam>
    public abstract partial class BaseRecycleViewAdapter<TViewHolder, TViewModel, TViewType> :
        RecyclerView.Adapter, IAdapter<TViewModel>
        where TViewHolder : BaseViewHolder
        where TViewType : struct, IConvertible
    {
        IList<TViewModel>? viewModels;

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

        public BaseRecycleViewAdapter(IList<TViewModel> viewModels)
        {
            this.viewModels = viewModels;
        }

        #region CreateViewModels

        protected virtual IList<TViewModel> CreateViewModels() => new List<TViewModel>();

        protected virtual IList<TViewModel> CreateViewModels(IEnumerable<TViewModel> newViewModels) => new List<TViewModel>(newViewModels);

        IList<TViewModel> ICreateViewModels<TViewModel>.CreateViewModels() => CreateViewModels();

        IList<TViewModel> ICreateViewModels<TViewModel>.CreateViewModels(IEnumerable<TViewModel> newViewModels) => CreateViewModels(newViewModels);

        #endregion
    }

    /// <inheritdoc cref="BaseRecycleViewAdapter{TViewHolder, TViewModel, TViewType}"/>
    public abstract partial class BaseRecycleViewAdapter<TViewHolder, TViewModel> : BaseRecycleViewAdapter<TViewHolder, TViewModel, int>
        where TViewHolder : BaseViewHolder
    {
        public BaseRecycleViewAdapter(IList<TViewModel> viewModels) : base(viewModels)
        {
        }
    }

    /// <summary>
    /// <see cref="RecyclerView.Adapter"/> 的 ReactiveUI 基类
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
    /// <typeparam name="TViewHolder"></typeparam>
    /// <typeparam name="TViewModel"></typeparam>
    /// <typeparam name="TViewType"></typeparam>
    public abstract partial class BaseReactiveRecycleViewAdapter<TViewHolder, TViewModel, TViewType> : ReactiveRecyclerViewAdapter<TViewModel>, IReadOnlyViewModels<TViewModel>
        where TViewHolder : BaseReactiveViewHolder<TViewModel>
        where TViewModel : class, IReactiveObject
        where TViewType : struct, IConvertible
    {
        readonly IList<TViewModel> viewModels;

        public IList<TViewModel> ViewModels => viewModels;

        IReadOnlyList<TViewModel> IReadOnlyViewModels<TViewModel>.ViewModels => IAdapter<TViewModel>.AsReadOnly(ViewModels);

        public BaseReactiveRecycleViewAdapter(ObservableCollection<TViewModel> collection) : base(collection.ToObservableChangeSet())
        {
            viewModels = collection;
        }

        public BaseReactiveRecycleViewAdapter(ReadOnlyObservableCollection<TViewModel> collection) : base(collection.ToObservableChangeSet())
        {
            viewModels = collection;
        }
    }

    /// <inheritdoc cref="BaseReactiveRecycleViewAdapter{TViewHolder, TViewModel, TViewType}"/>
    public abstract partial class BaseReactiveRecycleViewAdapter<TViewHolder, TViewModel> : BaseReactiveRecycleViewAdapter<TViewHolder, TViewModel, int>
        where TViewHolder : BaseReactiveViewHolder<TViewModel>
        where TViewModel : class, IReactiveObject
    {
        public BaseReactiveRecycleViewAdapter(ObservableCollection<TViewModel> collection) : base(collection)
        {
        }

        public BaseReactiveRecycleViewAdapter(ReadOnlyObservableCollection<TViewModel> collection) : base(collection)
        {
        }
    }
}
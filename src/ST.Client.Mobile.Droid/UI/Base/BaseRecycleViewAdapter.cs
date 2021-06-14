using AndroidX.RecyclerView.Widget;
using DynamicData;
using ReactiveUI.AndroidX;
using System.Application.UI.ViewModels;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Adapters
{
    /// <summary>
    /// <see cref="RecyclerView.Adapter"/> 的通用基类
    /// </summary>
    /// <typeparam name="TViewHolder"></typeparam>
    /// <typeparam name="TViewModel"></typeparam>
    /// <typeparam name="TViewType"></typeparam>
    public abstract class BaseRecycleViewAdapter<TViewHolder, TViewModel, TViewType> : ReactiveRecyclerViewAdapter<TViewModel>
        where TViewHolder : BaseViewHolder<TViewModel>
        where TViewModel : ViewModelBase
        where TViewType : struct, IConvertible
    {
        public BaseRecycleViewAdapter(IObservable<IChangeSet<TViewModel>> backingList) : base(backingList)
        {
        }
    }

    /// <inheritdoc cref="BaseRecycleViewAdapter{TViewHolder, TViewModel, TViewType}"/>
    public abstract class BaseRecycleViewAdapter<TViewHolder, TViewModel> : BaseRecycleViewAdapter<TViewHolder, TViewModel, int>
        where TViewHolder : BaseViewHolder<TViewModel>
        where TViewModel : ViewModelBase
    {
        public BaseRecycleViewAdapter(IObservable<IChangeSet<TViewModel>> backingList) : base(backingList)
        {
        }
    }
}
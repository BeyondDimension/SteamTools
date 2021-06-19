using Android.Views;
using AndroidX.RecyclerView.Widget;
using System.Application.UI.Adapters.Internals;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Adapters
{
    partial class BaseRecycleViewAdapter<TViewHolder, TViewModel, TViewType>
    {
        public virtual int? GetLayoutResource(TViewType viewType) => null;

        public virtual TViewHolder OnCreateViewHolder(View itemView, TViewType viewType)
        {
            var holder = RecycleViewAdapterHelper.CreateViewHolder<TViewHolder>(itemView);
            return holder;
        }

        public virtual TViewHolder OnCreateViewHolder(TViewType viewType, ViewGroup parent)
        {
            var holder = RecycleViewAdapterHelper.OnCreateViewHolder<TViewHolder, TViewType>(GetLayoutResource, OnCreateViewHolder, viewType, parent);
            return holder;
        }

        public sealed override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var holder = RecycleViewAdapterHelper.OnCreateViewHolder<TViewHolder, TViewType>(OnCreateViewHolder, SetItemClicks, parent, viewType);
            return holder;
        }
    }

    partial class BaseReactiveRecycleViewAdapter<TViewHolder, TViewModel, TViewType>
    {
        public virtual int? GetLayoutResource(TViewType viewType) => null;

        public virtual TViewHolder OnCreateViewHolder(View itemView, TViewType viewType)
        {
            var holder = RecycleViewAdapterHelper.CreateViewHolder<TViewHolder>(itemView);
            return holder;
        }

        public virtual TViewHolder OnCreateViewHolder(TViewType viewType, ViewGroup parent)
        {
            var holder = RecycleViewAdapterHelper.OnCreateViewHolder<TViewHolder, TViewType>(GetLayoutResource, OnCreateViewHolder, viewType, parent);
            return holder;
        }

        public sealed override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var holder = RecycleViewAdapterHelper.OnCreateViewHolder<TViewHolder, TViewType>(OnCreateViewHolder, SetItemClicks, parent, viewType);
            return holder;
        }
    }

    public static partial class RecycleViewAdapterHelper
    {
        public static TViewHolder CreateViewHolder<TViewHolder>(View itemView)
            where TViewHolder : RecyclerView.ViewHolder
        {
            var holder = (TViewHolder)Activator.CreateInstance(typeof(TViewHolder), itemView);
            return holder;
        }

        public static TViewHolder OnCreateViewHolder<TViewHolder, TViewType>(
            Func<TViewType, int?> getLayoutResource,
            Func<View, TViewType, TViewHolder> onCreateViewHolder,
            TViewType viewType,
            ViewGroup parent)
            where TViewHolder : RecyclerView.ViewHolder
        {
            int layoutResource = getLayoutResource(viewType).ThrowIsNull(nameof(layoutResource));
            var layoutInflater = LayoutInflater.From(parent.Context);
            if (layoutInflater == null) throw new ArgumentNullException(nameof(layoutInflater));
            var view = layoutInflater.Inflate(layoutResource, parent, false);
            if (view == null) throw new ArgumentNullException(nameof(view));
            var holder = onCreateViewHolder(view, viewType);
            return holder;
        }

        public static RecyclerView.ViewHolder OnCreateViewHolder<TViewHolder, TViewType>(
            Func<TViewType, ViewGroup, TViewHolder> onCreateViewHolder,
            Action<TViewHolder>? setItemClicks,
            ViewGroup parent,
            int viewType)
            where TViewHolder : RecyclerView.ViewHolder
            where TViewType : struct, IConvertible
        {
            var viewType2 = viewType.ConvertToEnum<TViewType>();
            var holder = onCreateViewHolder(viewType2, parent);
            setItemClicks?.Invoke(holder);
            return holder;
        }
    }
}
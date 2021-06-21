using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.UI.ViewModels;
using System.Collections.ObjectModel;

namespace System.Application.UI.Adapters
{
    internal sealed class SmallPreferenceButtonAdapter<TViewModel, TId> : BaseReactiveRecycleViewAdapter<SmallPreferenceButtonViewHolder<TViewModel, TId>, TViewModel>
        where TViewModel : RIdTitleViewModel<TId>
    {
        public SmallPreferenceButtonAdapter(ObservableCollection<TViewModel> collection) : base(collection)
        {
        }

        public SmallPreferenceButtonAdapter(ReadOnlyObservableCollection<TViewModel> collection) : base(collection)
        {
        }

        public override int? GetLayoutResource(int viewType)
        {
            return Resource.Layout.layout_preference_small_button;
        }
    }

    internal sealed class SmallPreferenceButtonViewHolder<TViewModel, TId> : BaseReactiveViewHolder<TViewModel>
       where TViewModel : RIdTitleViewModel<TId>
    {
        readonly layout_preference_small_button binding;

        public SmallPreferenceButtonViewHolder(View itemView) : base(itemView)
        {
            binding = new(itemView);
        }

        public override void OnBind()
        {
            base.OnBind();

            ViewModel!.WhenAnyValue(x => x.Title).Subscribe(value =>
            {
                binding.tvTitle.Text = value;
            }).AddTo(this);
        }
    }
}
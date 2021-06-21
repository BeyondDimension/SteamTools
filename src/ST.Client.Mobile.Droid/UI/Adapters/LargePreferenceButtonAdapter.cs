using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.UI.ViewModels;
using System.Collections.ObjectModel;

namespace System.Application.UI.Adapters
{
    internal sealed class LargePreferenceButtonAdapter<TViewModel, TId> : BaseReactiveRecycleViewAdapter<LargePreferenceButtonViewHolder<TViewModel, TId>, TViewModel>
        where TViewModel : RIdTitleIconViewModel<TId, ResIcon>
    {
        public LargePreferenceButtonAdapter(ObservableCollection<TViewModel> collection) : base(collection)
        {
        }

        public LargePreferenceButtonAdapter(ReadOnlyObservableCollection<TViewModel> collection) : base(collection)
        {
        }

        public override int? GetLayoutResource(int viewType)
        {
            return Resource.Layout.layout_preference_large_button;
        }
    }

    internal sealed class LargePreferenceButtonViewHolder<TViewModel, TId> : BaseReactiveViewHolder<TViewModel>
         where TViewModel : RIdTitleIconViewModel<TId, ResIcon>
    {
        readonly layout_preference_large_button binding;

        public LargePreferenceButtonViewHolder(View itemView) : base(itemView)
        {
            binding = new(itemView);
        }

        public override void OnBind()
        {
            base.OnBind();

            ViewModel!.WhenAnyValue(x => x.Icon).Subscribe(value =>
            {
                binding.ivIcon.SetImageResourceIcon(value);
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.Title).Subscribe(value =>
            {
                binding.tvTitle.Text = value;
            }).AddTo(this);
        }
    }
}
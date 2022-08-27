using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.UI.ViewModels;
#if NET6_0_OR_GREATER
using Microsoft.Maui.ApplicationModel;
#else
using Xamarin.Essentials;
#endif
using TViewHolder = System.Application.UI.Adapters.FastLoginChannelViewHolder;
using TViewModel = System.Application.UI.ViewModels.LoginOrRegisterWindowViewModel.FastLoginChannelViewModel;

namespace System.Application.UI.Adapters
{
    internal sealed class FastLoginChannelAdapter : BaseReactiveRecycleViewAdapter<TViewHolder, TViewModel>
    {
        public FastLoginChannelAdapter(LoginOrRegisterWindowViewModel viewModel) : base(viewModel.FastLoginChannels)
        {
        }

        public override int? GetLayoutResource(int viewType)
        {
            return Resource.Layout.layout_login_and_register_by_fast_channel;
        }
    }

    internal sealed class FastLoginChannelViewHolder : BaseReactiveViewHolder<TViewModel>
    {
        readonly layout_login_and_register_by_fast_channel binding;

        public FastLoginChannelViewHolder(View itemView) : base(itemView)
        {
            binding = new(itemView);
        }

        public override void OnBind()
        {
            base.OnBind();

            ViewModel!.WhenAnyValue(x => x.Icon).SubscribeInMainThread(value =>
            {
                binding.ivIcon.SetImageResourceIcon(value);
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.Title).SubscribeInMainThread(value =>
            {
                binding.tvTitle.Text = value;
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.IconBgColor).SubscribeInMainThread(value =>
            {
                if (value == default)
                {
                    binding.bgIcon.Background = null;
                }
                else
                {
                    binding.bgIcon.SetBackgroundColor(value.ToPlatformColor());
                }
            }).AddTo(this);
        }
    }
}
using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.UI.ViewModels;
using TViewHolder = System.Application.UI.Adapters.SteamAuthTradeConfirmationViewHolder;
using TViewModel = WinAuth.WinAuthSteamClient.Confirmation;

namespace System.Application.UI.Adapters
{
    internal sealed class SteamAuthTradeConfirmationAdapter : BaseReactiveRecycleViewAdapter<TViewHolder, TViewModel>, IReadOnlyViewFor<AuthTradeWindowViewModel>
    {
        public AuthTradeWindowViewModel ViewModel { get; }

        public SteamAuthTradeConfirmationAdapter(AuthTradeWindowViewModel viewModel) : base(viewModel.Confirmations)
        {
            ViewModel = viewModel;
        }

        public override int? GetLayoutResource(int viewType)
        {
            return Resource.Layout.layout_card_steam_auth_trade_confirmation;
        }
    }

    internal sealed class SteamAuthTradeConfirmationViewHolder : BaseReactiveViewHolder<TViewModel>, View.IOnClickListener
    {
        readonly layout_card_steam_auth_trade_confirmation binding;

        public SteamAuthTradeConfirmationViewHolder(View itemView) : base(itemView)
        {
            binding = new(itemView);
        }

        void GetDataContext(Action<AuthTradeWindowViewModel> action)
        {
            if (BindingAdapter is IReadOnlyViewFor<AuthTradeWindowViewModel> vf
                && vf.ViewModel != null)
            {
                action(vf.ViewModel);
            }
        }

        public override void OnBind()
        {
            base.OnBind();
            binding.btnCancelTrade.SetOnClickListener(this);
            binding.btnConfirmTrade.SetOnClickListener(this);
            ViewModel.WhenAnyValue(x => x.Image)
                .Subscribe(value => binding.ivImage.SetImageSource(value,
                    targetResId: Resource.Dimension.steam_auth__trade_confirmation_img_size))
                        .AddTo(this);
            ViewModel.WhenAnyValue(x => x.Details)
                .Subscribe(value => binding.tvDetails.Text = value).AddTo(this);
            ViewModel.WhenAnyValue(x => x.Traded)
                .Subscribe(value => binding.tvTraded.Text = value).AddTo(this);
            ViewModel.WhenAnyValue(x => x.When)
                .Subscribe(value => binding.tvWhen.Text = value).AddTo(this);
        }

        void View.IOnClickListener.OnClick(View? view)
        {
            if (view == null) return;
            var vm = ViewModel;
            if (vm == null) return;
            if (view.Id == Resource.Id.btnCancelTrade)
            {
                GetDataContext(dataContext =>
                {
                    dataContext.CancelTrade_Click(vm);
                });
            }
            else if (view.Id == Resource.Id.btnConfirmTrade)
            {
                GetDataContext(dataContext =>
                {
                    dataContext.ConfirmTrade_Click(vm);
                });
            }
        }
    }
}
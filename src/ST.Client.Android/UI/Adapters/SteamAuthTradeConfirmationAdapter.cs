using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using TViewHolder = System.Application.UI.Adapters.SteamAuthTradeConfirmationViewHolder;
using TViewModel = WinAuth.WinAuthSteamClient.Confirmation;
using static System.Application.UI.Resx.AppResources;

namespace System.Application.UI.Adapters
{
    internal sealed class SteamAuthTradeConfirmationAdapter : BaseReactiveRecycleViewAdapter<TViewHolder, TViewModel>/*, IReadOnlyViewFor<AuthTradeWindowViewModel>*/
    {
        //public AuthTradeWindowViewModel ViewModel { get; }

        public SteamAuthTradeConfirmationAdapter(AuthTradeWindowViewModel viewModel) : base(viewModel.Confirmations, viewModel.ConfirmationsSourceList)
        {
            //ViewModel = viewModel;
        }

        public override int? GetLayoutResource(int viewType)
        {
            return Resource.Layout.layout_card_steam_auth_trade_confirmation;
        }
    }

    internal sealed class SteamAuthTradeConfirmationViewHolder : BaseReactiveViewHolder<TViewModel>/*, View.IOnClickListener*/
    {
        readonly layout_card_steam_auth_trade_confirmation binding;

        public SteamAuthTradeConfirmationViewHolder(View itemView) : base(itemView)
        {
            binding = new(itemView);
        }

        //void GetDataContext(Action<AuthTradeWindowViewModel> action)
        //{
        //    if (BindingAdapter is IReadOnlyViewFor<AuthTradeWindowViewModel> vf
        //        && vf.ViewModel != null)
        //    {
        //        action(vf.ViewModel);
        //    }
        //}

        void SetOperateText(int isOperate)
        {
            var text = isOperate switch
            {
                1 => Confirmed,
                2 => Cancelled,
                _ => string.Empty,
            };
            binding.tvOperate.Text = text;
        }

        void SetOperatePanel(int isOperate)
        {
            var state = isOperate switch
            {
                1 or 2 => ViewStates.Invisible,
                _ => ViewStates.Visible,
            };
            //binding.btnCancelTrade.Visibility = state;
            //binding.btnConfirmTrade.Visibility = state;
            binding.checkbox.Visibility = state;
            binding.tvOperate.Visibility = state == ViewStates.Visible ? ViewStates.Gone : ViewStates.Visible;
        }

        public override void OnBind()
        {
            base.OnBind();

            R.Subscribe(() =>
            {
                //binding.btnCancelTrade.Text = LocalAuth_AuthTrade_Cancel;
                //binding.btnConfirmTrade.Text = LocalAuth_AuthTrade_Confirm;
                SetOperateText(ViewModel!.IsOperate);
            }).AddTo(this);

            //binding.btnCancelTrade.SetOnClickListener(this);
            //binding.btnConfirmTrade.SetOnClickListener(this);

            ViewModel.WhenAnyValue(x => x.IsOperate)
                .Subscribe(value =>
                {
                    SetOperateText(value);
                    SetOperatePanel(value);
                }).AddTo(this);
            binding.ivImage.SetImageSource(ViewModel!.Image, Resource.Dimension.steam_auth__trade_confirmation_img_size);
            binding.tvDetails.Text = ViewModel.Details;
            binding.tvTraded.Text = ViewModel.Traded;
            binding.tvWhen.Text = ViewModel.When;
            ViewModel.WhenAnyValue(x => x.NotChecked)
                .Subscribe(value => binding.checkbox.Checked = !value).AddTo(this);
            //ViewModel.WhenAnyValue(x => x.ButtonEnable)
            //    .SubscribeInMainThread(value =>
            //    {
            //        binding.btnCancelTrade.Enabled = value;
            //        binding.btnConfirmTrade.Enabled = value;
            //    }).AddTo(this);
        }

        //void View.IOnClickListener.OnClick(View? view)
        //{
        //    if (view == null) return;
        //    var vm = ViewModel;
        //    if (vm == null) return;
        //    if (view.Id == Resource.Id.btnCancelTrade)
        //    {
        //        GetDataContext(dataContext =>
        //        {
        //            dataContext.CancelTrade_Click(vm);
        //        });
        //    }
        //    else if (view.Id == Resource.Id.btnConfirmTrade)
        //    {
        //        GetDataContext(dataContext =>
        //        {
        //            dataContext.ConfirmTrade_Click(vm);
        //        });
        //    }
        //}
    }
}
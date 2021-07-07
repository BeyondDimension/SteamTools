using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.UI.ViewModels;
using TViewHolder = System.Application.UI.Adapters.GAPAuthenticatorViewHolder;
using TViewModel = System.Application.Models.MyAuthenticator;

namespace System.Application.UI.Adapters
{
    internal sealed class GAPAuthenticatorAdapter : BaseReactiveRecycleViewAdapter<TViewHolder, TViewModel>, IReadOnlyViewFor<LocalAuthPageViewModel>
    {
        public LocalAuthPageViewModel ViewModel { get; }

        public GAPAuthenticatorAdapter(LocalAuthPageViewModel viewModel) : base(viewModel.Authenticators)
        {
            ViewModel = viewModel;
        }

        public override int? GetLayoutResource(int viewType)
        {
            return Resource.Layout.layout_card_gap_authenticator;
        }
    }

    internal sealed class GAPAuthenticatorViewHolder : BaseReactiveViewHolder<TViewModel>, View.IOnClickListener
    {
        readonly layout_card_gap_authenticator binding;

        public GAPAuthenticatorViewHolder(View itemView) : base(itemView)
        {
            binding = new(itemView);
        }

        public override void OnBind()
        {
            base.OnBind();
            binding.btnEditName.SetOnClickListener(this);
            binding.btnConfirmTrade.SetOnClickListener(this);
            binding.btnCopy.SetOnClickListener(this);
            binding.btnDelete.SetOnClickListener(this);
            binding.btnSeeDetail.SetOnClickListener(this);
            binding.btnSeeValue.SetOnClickListener(this);
        }

        void GetDataContext(Action<LocalAuthPageViewModel> action)
        {
            if (BindingAdapter is IReadOnlyViewFor<LocalAuthPageViewModel> vf
                && vf.ViewModel != null)
            {
                action(vf.ViewModel);
            }
        }

        static async void EditName(TViewModel vm)
        {
            var value = await TextBoxWindowViewModel.ShowDialog(new()
            {
                Value = vm.Name,
            });
            vm.Name = value ?? string.Empty;
        }

        void View.IOnClickListener.OnClick(View? view)
        {
            if (view == null) return;
            var vm = ViewModel;
            if (vm == null) return;
            if (view.Id == Resource.Id.btnEditName)
            {
                EditName(vm);
            }
            else if (view.Id == Resource.Id.btnConfirmTrade)
            {
                GetDataContext(dataContext =>
                {
                    dataContext.ShowSteamAuthTrade(vm);
                });
            }
            else if (view.Id == Resource.Id.btnCopy)
            {
                GetDataContext(dataContext =>
                {
                    dataContext.CopyCodeCilp(vm);
                });
            }
            else if (view.Id == Resource.Id.btnDelete)
            {
                GetDataContext(dataContext =>
                {
                    dataContext.DeleteAuth(vm);
                });
            }
            else if (view.Id == Resource.Id.btnSeeDetail)
            {
                GetDataContext(dataContext =>
                {
                    dataContext.ShowSteamAuthData(vm);
                });
            }
            else if (view.Id == Resource.Id.btnSeeValue)
            {
                GetDataContext(dataContext =>
                {
                    dataContext.ShowAuthCode(vm);
                });
            }
        }
    }
}
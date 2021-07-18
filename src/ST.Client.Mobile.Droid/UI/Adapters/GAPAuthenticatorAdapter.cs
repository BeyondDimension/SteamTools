using Android.Views;
using AndroidX.Lifecycle;
using Binding;
using Java.Interop;
using ReactiveUI;
using System.Application.UI.ViewModels;
using System.Threading;
using TViewHolder = System.Application.UI.Adapters.GAPAuthenticatorViewHolder;
using TViewModel = System.Application.Models.MyAuthenticator;

namespace System.Application.UI.Adapters
{
    internal sealed class GAPAuthenticatorAdapter : BaseReactiveRecycleViewAdapter<TViewHolder, TViewModel>, /*IReadOnlyViewFor<LocalAuthPageViewModel>,*/ ILifecycleOwner
    {
        //public LocalAuthPageViewModel ViewModel { get; }

        public readonly ILifecycleOwner lifecycleOwner;
        Lifecycle ILifecycleOwner.Lifecycle => lifecycleOwner.Lifecycle;

        public GAPAuthenticatorAdapter(ILifecycleOwner lifecycleOwner, LocalAuthPageViewModel viewModel) : base(viewModel.Authenticators)
        {
            //ViewModel = viewModel;
            this.lifecycleOwner = lifecycleOwner;
        }

        public override int? GetLayoutResource(int viewType)
        {
            return Resource.Layout.layout_card_gap_authenticator;
        }
    }

    internal sealed class GAPAuthenticatorViewHolder : BaseReactiveViewHolder<TViewModel>, View.IOnClickListener, ILifecycleObserver, TViewModel.IAutoRefreshCode
    {
        readonly layout_card_gap_authenticator binding;

        public CancellationTokenSource? AutoRefreshCode { get; set; }

        public GAPAuthenticatorViewHolder(View itemView) : base(itemView)
        {
            binding = new(itemView);
        }

        [Export]
        [Lifecycle.Event.OnPause]
        public void OnPause()
        {
            TViewModel.StopAutoRefreshCode(this);
        }

        [Export]
        [Lifecycle.Event.OnResume]
        public void OnResume()
        {
            if (AutoRefreshCode == null)
            {
                TViewModel.StartAutoRefreshCode(this, noStop: true);
            }
        }

        public override void OnBind()
        {
            if (BindingAdapter is not ILifecycleOwner lifecycleOwner)
                throw new NotSupportedException(
                    "BindingAdapter is not implements AndroidX.Lifecycle。ILifecycleOwner.");
            lifecycleOwner.Lifecycle.RemoveObserver(this);
            base.OnBind();

            lifecycleOwner.Lifecycle.AddObserver(this);
            TViewModel.StartAutoRefreshCode(this);
            ViewModel.WhenAnyValue(x => x.CurrentCode).Subscribe(value =>
            {
                binding.tvValue.Text = TViewModel.CodeFormat(value);
            }).AddTo(this);
            ViewModel.WhenAnyValue(x => x.Name).Subscribe(value =>
            {
                binding.tvName.Text = string.IsNullOrEmpty(value) ? BindingAdapterPosition.ToString("000") : value;
            }).AddTo(this);
            binding.tvValue.SetOnClickListener(this);
            //binding.btnEditName.SetOnClickListener(this);
            //binding.btnConfirmTrade.SetOnClickListener(this);
            //binding.btnCopy.SetOnClickListener(this);
            //binding.btnDelete.SetOnClickListener(this);
            //binding.btnSeeDetail.SetOnClickListener(this);
            //binding.btnSeeValue.SetOnClickListener(this);
        }

        //void GetDataContext(Action<LocalAuthPageViewModel> action)
        //{
        //    if (BindingAdapter is IReadOnlyViewFor<LocalAuthPageViewModel> vf
        //        && vf.ViewModel != null)
        //    {
        //        action(vf.ViewModel);
        //    }
        //}

        void View.IOnClickListener.OnClick(View? view)
        {
            if (view == null) return;
            var vm = ViewModel;
            if (vm == null) return;
            if (view.Id == Resource.Id.tvValue)
            {
                vm.CopyCodeCilp();
            }
            //else if (view.Id == Resource.Id.btnEditName)
            //{
            //    await vm.EditNameAsync();
            //}
            //else if (view.Id == Resource.Id.btnConfirmTrade)
            //{
            //    GetDataContext(dataContext =>
            //    {
            //        dataContext.ShowSteamAuthTrade(vm);
            //    });
            //}
            //else if (view.Id == Resource.Id.btnCopy)
            //{
            //    GetDataContext(dataContext =>
            //    {
            //        dataContext.CopyCodeCilp(vm);
            //    });
            //}
            //else if (view.Id == Resource.Id.btnDelete)
            //{
            //    GetDataContext(dataContext =>
            //    {
            //        dataContext.DeleteAuth(vm);
            //    });
            //}
            //else if (view.Id == Resource.Id.btnSeeDetail)
            //{
            //    GetDataContext(dataContext =>
            //    {
            //        dataContext.ShowSteamAuthData(vm);
            //    });
            //}
            //else if (view.Id == Resource.Id.btnSeeValue)
            //{
            //    GetDataContext(dataContext =>
            //    {
            //        dataContext.ShowAuthCode(vm);
            //    });
            //}
        }
    }
}
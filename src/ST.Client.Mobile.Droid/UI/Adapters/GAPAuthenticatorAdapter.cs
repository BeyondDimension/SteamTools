using Android.Views;
using AndroidX.Lifecycle;
using AndroidX.RecyclerView.Widget;
using Binding;
using Java.Interop;
using ReactiveUI;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Threading;
using TViewHolder = System.Application.UI.Adapters.GAPAuthenticatorViewHolder;
using TViewModel = System.Application.Models.MyAuthenticator;

namespace System.Application.UI.Adapters
{
    internal sealed class GAPAuthenticatorAdapter : BaseReactiveRecycleViewAdapter<TViewHolder, TViewModel>, ILifecycleObserver, TViewModel.IAutoRefreshCodeHost
    {
        RecyclerView? recyclerView;

        Timer? TViewModel.IAutoRefreshCodeHost.Timer { get; set; }

        IEnumerable<TViewModel> TViewModel.IAutoRefreshCodeHost.ViewModels
        {
            get
            {
                if (recyclerView != null)
                {
                    for (int i = 0; i < recyclerView.ChildCount; i++)
                    {
                        if (recyclerView.GetChildViewHolder(recyclerView.GetChildAt(i))
                            is IReadOnlyViewFor<TViewModel> holder && holder.ViewModel != null)
                        {
                            yield return holder.ViewModel;
                        }
                    }
                }
            }
        }

        public GAPAuthenticatorAdapter(ILifecycleOwner lifecycleOwner, LocalAuthPageViewModel viewModel) : base(viewModel.Authenticators)
        {
            lifecycleOwner.Lifecycle.AddObserver(this);
        }

        public override int? GetLayoutResource(int viewType)
        {
            return Resource.Layout.layout_card_gap_authenticator;
        }

        public override void OnAttachedToRecyclerView(RecyclerView recyclerView)
        {
            base.OnAttachedToRecyclerView(recyclerView);
            this.recyclerView = recyclerView;
        }

        [Export]
        [Lifecycle.Event.OnStop]
        public void OnStop()
        {
            TViewModel.IAutoRefreshCodeHost host = this;
            host.StopTimer();
        }

        [Export]
        [Lifecycle.Event.OnResume]
        public void OnResume()
        {
            TViewModel.IAutoRefreshCodeHost host = this;
            host.StartTimer();
        }
    }

    internal sealed class GAPAuthenticatorViewHolder : BaseReactiveViewHolder<TViewModel>, View.IOnClickListener, ILifecycleObserver
    {
        readonly layout_card_gap_authenticator binding;

        public GAPAuthenticatorViewHolder(View itemView) : base(itemView)
        {
            binding = new(itemView);
        }

        public override void OnBind()
        {
            base.OnBind();

            ViewModel.WhenAnyValue(x => x.CurrentCode).SubscribeInMainThread(value =>
            {
                binding.tvValue.Text = TViewModel.CodeFormat(value);
            }).AddTo(this);
            ViewModel.WhenAnyValue(x => x.Name).SubscribeInMainThread(value =>
            {
                binding.tvName.Text = string.IsNullOrEmpty(value) ? BindingAdapterPosition.ToString("000") : value;
            }).AddTo(this);
            ViewModel.WhenAnyValue(x => x.Period).SubscribeInMainThread(value =>
            {
                binding.progress.Max = value;
            }).AddTo(this);
            ViewModel.WhenAnyValue(x => x.AutoRefreshCodeTimingCurrent).SubscribeInMainThread(value =>
            {
                var visibility = value < 0 ? ViewStates.Gone : ViewStates.Visible;
                binding.progress.Visibility = visibility;
                binding.tvProgress.Visibility = visibility;
                binding.progress.Progress = value;
                binding.tvProgress.Text = value.ToString();
            }).AddTo(this);
            binding.tvValue.SetOnClickListener(this);
        }

        void View.IOnClickListener.OnClick(View? view)
        {
            if (view == null) return;
            var vm = ViewModel;
            if (vm == null) return;
            if (view.Id == Resource.Id.tvValue)
            {
                vm.CopyCodeCilp();
            }
        }
    }
}
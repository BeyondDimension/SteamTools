using Android.Runtime;
using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.Mvvm;
using System.Application.UI.Activities;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Reactive.Disposables;

namespace System.Application.UI.Fragments
{
    [Register(JavaPackageConstants.Fragments + nameof(MyFragment))]
    internal sealed class MyFragment : BaseFragment<fragment_my, MyPageViewModel>, IDisposableHolder
    {
        bool disposeViewModel;
        readonly CompositeDisposable disposables = new();
        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => disposables;

        protected override int? LayoutResource => Resource.Layout.fragment_my;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            if (Activity is MainActivity mainActivity)
            {
                ViewModel = mainActivity.MyPageViewModel;
            }
            else
            {
                ViewModel = new();
                disposeViewModel = true;
            }

            ViewModel.WhenAnyValue(x => x.NickName).Subscribe(value =>
            {
                if (binding == null) return;
                binding.tvNickName.Text = value;
            }).AddTo(this);

            SetOnClickListener(binding!.layoutUser);
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();
            disposables.Dispose();
            if (ViewModel != null)
            {
                if (disposeViewModel) ViewModel.Dispose();
                ViewModel = null;
            }
        }

        protected override bool OnClick(View view)
        {
            if (view.Id == Resource.Id.layoutUser)
            {
                this.StartActivity<LoginOrRegisterActivity>();
            }
            //else if (view.Id == Resource.Id.???)
            //{
            //}

            return base.OnClick(view);
        }
    }
}
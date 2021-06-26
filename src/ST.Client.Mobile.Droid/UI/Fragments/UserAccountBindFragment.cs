using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using static System.Application.UI.ViewModels.UserProfilePageViewModel;

namespace System.Application.UI.Fragments
{
    internal sealed class UserAccountBindFragment : BaseFragment<fragment_account_binding, UserProfilePageViewModel>
    {
        protected override int? LayoutResource => Resource.Layout.fragment_account_binding;

        static readonly Dictionary<int, FastLoginChannel> btnAccountBindDict = new()
        {
            { Resource.Id.btnAccountBindSteam, FastLoginChannel.Steam },
            { Resource.Id.btnAccountBindMicrosoft, FastLoginChannel.Microsoft },
            { Resource.Id.btnAccountBindApple, FastLoginChannel.Apple },
            { Resource.Id.btnAccountBindQQ, FastLoginChannel.QQ },
        };
        static readonly Dictionary<int, FastLoginChannel> tbAccountBindDict = new()
        {
            { Resource.Id.tbAccountBindSteam, FastLoginChannel.Steam },
            { Resource.Id.tbAccountBindMicrosoft, FastLoginChannel.Microsoft },
            { Resource.Id.tbAccountBindApple, FastLoginChannel.Apple },
            { Resource.Id.tbAccountBindQQ, FastLoginChannel.QQ },
        };

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            var btnAccountBinds = new[] { binding!.btnAccountBindSteam, binding.btnAccountBindMicrosoft, binding.btnAccountBindApple, binding.btnAccountBindQQ, };
            var tbAccountBinds = new[] { binding.tbAccountBindSteam, binding.tbAccountBindMicrosoft, binding.tbAccountBindApple, binding.tbAccountBindQQ, };

            UserService.Current.WhenAnyValue(x => x.HasPhoneNumber).Subscribe(value =>
            {
                if (binding == null) return;
                foreach (var item in btnAccountBinds)
                {
                    item.Enabled = value;
                }
            }).AddTo(this);

            UserService.Current.WhenAnyValue(x => x.User).Subscribe(value =>
            {
                if (binding == null) return;
                foreach (var item in tbAccountBinds)
                {
                    item.Text = GetIsBindOrUnbundleTbText(tbAccountBindDict[item.Id]) ?? string.Empty;
                }
            }).AddTo(this);

            R.Current.WhenAnyValue(x => x.Res).Subscribe(_ =>
            {
                if (binding == null) return;
                foreach (var item in tbAccountBinds)
                {
                    item.Hint = AppResources.Unbound;
                }

                binding.tvAccountBindTip.Text = AppResources.User_AccountBindTip;

                foreach (var item in btnAccountBinds)
                {
                    item.Text = GetIsBindOrUnbundleBtnText(btnAccountBindDict[item.Id]);
                }
            }).AddTo(this);

            SetOnClickListener(btnAccountBinds);
        }

        protected override bool OnClick(View view)
        {
            if (btnAccountBindDict.ContainsKey(view.Id))
            {
                ViewModel!.OnBindOrUnbundleFastLoginClick(btnAccountBindDict[view.Id]);
                return true;
            }
            return base.OnClick(view);
        }
    }
}
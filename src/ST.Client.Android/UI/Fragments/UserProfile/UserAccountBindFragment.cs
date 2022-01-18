using Android.Views;
using Android.Widget;
using Binding;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using static System.Application.UI.ViewModels.UserProfileWindowViewModel;

namespace System.Application.UI.Fragments
{
    [Authorize]
    internal sealed class UserAccountBindFragment : BaseFragment<fragment_account_binding, UserProfileWindowViewModel>
    {
        protected override int? LayoutResource => Resource.Layout.fragment_account_binding;

        Button[]? btnAccountBinds;
        EditText[]? tbAccountBinds;
        ImageView[]? ivIconAccountBinds;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            btnAccountBinds = new[] {
                binding!.btnAccountBind0,
                binding.btnAccountBind1,
                binding.btnAccountBind2,
                binding.btnAccountBind3,
            };
            tbAccountBinds = new[] {
                binding.tbAccountBind0,
                binding.tbAccountBind1,
                binding.tbAccountBind2,
                binding.tbAccountBind3,
            };
            ivIconAccountBinds = new[]
            {
                binding.ivIconAccountBind0,
                binding.ivIconAccountBind1,
                binding.ivIconAccountBind2,
                binding.ivIconAccountBind3,
            };

            for (int i = 0; i < ivIconAccountBinds.Length; i++) // 设置 Icon
            {
                var iconResId = ThirdPartyLoginHelper.FastLoginChannels[i]
                    .ToIcon().ToImageResource();
                if (iconResId.HasValue)
                {
                    ivIconAccountBinds[i].SetImageResource(iconResId.Value);
                }
            }

            UserService.Current.WhenAnyValue(x => x.HasPhoneNumber).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                for (int i = 0; i < btnAccountBinds.Length; i++)
                {
                    var item = btnAccountBinds[i];
                    var cha = ThirdPartyLoginHelper.FastLoginChannels[i];
                    if (cha.IsSupported())
                    {
                        if (!value && !IsBindOrUnbundle(cha)) // 无手机号不能解绑
                        {
                            if (item.Enabled)
                            {
                                item.Enabled = false;
                            }
                        }
                        else
                        {
                            if (!item.Enabled) // 绑定时启用按钮
                            {
                                item.Enabled = true;
                            }
                        }
                    }
                }
            }).AddTo(this);

            for (int i = 0; i < ThirdPartyLoginHelper.FastLoginChannels.Length; i++)
            {
                var cha = ThirdPartyLoginHelper.FastLoginChannels[i];
                if (cha.IsSupported()) // 绑定文本框值
                {
                    SubscribeIsBindOrUnbundle(i, cha);
                }
                else
                {
                    var item = btnAccountBinds[i];
                    if (item.Enabled) // 禁用尚未支持的渠道按钮
                    {
                        item.Enabled = false;
                    }
                }
            }

            R.Subscribe(() =>
            {
                if (binding == null) return;
                foreach (var item in tbAccountBinds)
                {
                    item.Hint = AppResources.Unbound;
                }

                binding.tvAccountBindTip.Text = AppResources.User_AccountBindTip;

                for (int i = 0; i < btnAccountBinds.Length; i++) // 设置按钮文本
                {
                    var item = btnAccountBinds[i];
                    var cha = ThirdPartyLoginHelper.FastLoginChannels[i];
                    if (cha.IsSupported())
                    {
                        item.Text = GetIsBindOrUnbundleBtnText(cha);
                    }
                    else
                    {
                        item.Text = AppResources.UnderConstruction;
                    }
                }
            }).AddTo(this);

            SetOnClickListener(btnAccountBinds);
        }

        void SubscribeIsBindOrUnbundle(int i, FastLoginChannel cha)
        {
            if (tbAccountBinds == null) return;
            var expression = GetIsBindOrUnbundleExpression(cha);
            UserService.Current.WhenAnyValue(expression)
                .SubscribeInMainThread(value =>
                {
                    if (binding == null) return;
                    var item = tbAccountBinds[i];
                    item.Text = value?.ToString() ?? string.Empty;
                }).AddTo(this);
        }

        protected override bool OnClick(View view)
        {
            for (int i = 0; i < btnAccountBinds!.Length; i++)
            {
                var item = btnAccountBinds[i];
                if (item.Id == view.Id)
                {
                    var cha = ThirdPartyLoginHelper.FastLoginChannels[i];
                    if (cha.IsSupported())
                    {
                        ViewModel!.OnBindOrUnbundleFastLoginClick(cha);
                        return true;
                    }
                }
            }
            return base.OnClick(view);
        }
    }
}
using ReactiveUI;
using System.Application.Mvvm;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace System.Application.UI.ViewModels
{
    /// <summary>
    /// 我的页面视图模型
    /// </summary>
    public class MyPageViewModel : ViewModelBase
    {
        public static string DisplayName => AppResources.My;

        public MyPageViewModel()
        {
            preferenceButtons = new()
            {
                PreferenceButtonViewModel.Create(PreferenceButton.UserProfile, this),
                PreferenceButtonViewModel.Create(PreferenceButton.BindPhoneNumber, this),
                PreferenceButtonViewModel.Create(PreferenceButton.Settings, this, 1),
                PreferenceButtonViewModel.Create(PreferenceButton.About, this, 1),
            };

            UserService.Current.WhenAnyValue(x => x.User).Subscribe(value =>
            {
                if (value == null)
                {
                    nickNameNullValLangChangeSubscribe?.RemoveTo(this);
                    nickNameNullValLangChangeSubscribe = R.Current.WhenAnyValue(x => x.Res)
                    .Subscribe(_ =>
                    {
                        // 未登录时显示的文本，多语言绑定
                        NickName = NickNameNullVal;
                    }).AddTo(this);

                    //PreferenceButtonViewModel.RemoveAuthorized(preferenceButtons, this);
                }
                else
                {
                    nickNameNullValLangChangeSubscribe?.RemoveTo(this);
                    nickNameNullValLangChangeSubscribe = null;
                    NickName = value.NickName;

                    //var editProfile = preferenceButtons.FirstOrDefault(x => x.Id == PreferenceButton.EditProfile);
                    //if (editProfile == null)
                    //{
                    //    editProfile = PreferenceButtonViewModel.Create(PreferenceButton.EditProfile, this);
                    //    preferenceButtons.Insert(0, editProfile);
                    //}

                    //var phoneNumber = preferenceButtons.FirstOrDefault(x => PreferenceButtonViewModel.IsPhoneNumber(x.Id));
                    //if (phoneNumber == null)
                    //{
                    //    phoneNumber = PreferenceButtonViewModel.Create(PreferenceButtonViewModel.GetPhoneNumberId(UserService.Current.HasPhoneNumber), this);
                    //    preferenceButtons.Insert(1, phoneNumber);
                    //}
                }
            }).AddTo(this);

            UserService.Current.WhenAnyValue(x => x.HasPhoneNumber).Subscribe(value =>
            {
                var id = PreferenceButtonViewModel.GetPhoneNumberId(value);
                foreach (var item in preferenceButtons)
                {
                    if (item.Id != id && PreferenceButtonViewModel.IsPhoneNumber(item.Id))
                    {
                        item.Id = id;
                    }
                }
            }).AddTo(this);
        }

        IDisposable? nickNameNullValLangChangeSubscribe;
        static string NickNameNullVal => AppResources.LoginAndRegister;
        string nickName = NickNameNullVal;
        public string NickName
        {
            get => nickName;
            set => this.RaiseAndSetIfChanged(ref nickName, value);
        }

        ObservableCollection<PreferenceButtonViewModel> preferenceButtons;
        /// <summary>
        /// 我的选项按钮组
        /// </summary>
        public ObservableCollection<PreferenceButtonViewModel> PreferenceButtons
        {
            get => preferenceButtons;
            set => this.RaiseAndSetIfChanged(ref preferenceButtons, value);
        }

        /// <summary>
        /// 我的选项按钮组唯一键
        /// </summary>
        public enum PreferenceButton
        {
            UserProfile = 1,
            BindPhoneNumber,
            ChangePhoneNumber,
            Settings,
            About,
        }

        /// <summary>
        /// 我的选项按钮视图模型
        /// </summary>
        public sealed class PreferenceButtonViewModel : RIdTitleIconViewModel<PreferenceButton, ResIcon>, IReadOnlyItemViewGroup
        {
            PreferenceButtonViewModel()
            {
            }

            public int ItemViewGroup { get; set; }

            protected override string GetTitleById(PreferenceButton id)
            {
                var title = id switch
                {
                    PreferenceButton.UserProfile => AppResources.UserProfile,
                    PreferenceButton.BindPhoneNumber => AppResources.User_BindPhoneNum,
                    PreferenceButton.ChangePhoneNumber => AppResources.User_ChangePhoneNum,
                    PreferenceButton.Settings => AppResources.Settings,
                    PreferenceButton.About => AppResources.About,
                    _ => string.Empty,
                };
                return title;
            }

            protected override ResIcon GetIconById(PreferenceButton id)
            {
                var icon = id switch
                {
                    PreferenceButton.UserProfile => ResIcon.baseline_account_box_black_24,
                    PreferenceButton.BindPhoneNumber => ResIcon.baseline_phone_black_24,
                    PreferenceButton.ChangePhoneNumber => ResIcon.baseline_phone_black_24,
                    PreferenceButton.Settings => ResIcon.baseline_settings_black_24,
                    PreferenceButton.About => ResIcon.baseline_info_black_24,
                    _ => default,
                };
                return icon;
            }

            /// <summary>
            /// 判断键是否为手机号相关，绑定手机、换绑手机
            /// </summary>
            /// <param name="id"></param>
            /// <returns></returns>
            public static bool IsPhoneNumber(PreferenceButton id) => id == PreferenceButton.BindPhoneNumber || id == PreferenceButton.ChangePhoneNumber;

#if DEBUG
            /// <summary>
            /// 从选项按钮组中移除已登录才有的选项组
            /// </summary>
            /// <param name="collection"></param>
            /// <param name="vm"></param>
            [Obsolete("未登录时不隐藏选项，点击相关选项跳转登录", true)]
            public static void RemoveAuthorized(ICollection<PreferenceButtonViewModel> collection, IDisposableHolder vm)
            {
                var removeArray = collection.Where(x => IsPhoneNumber(x.Id) || x.Id == PreferenceButton.UserProfile).ToArray();
                Array.ForEach(removeArray, x =>
                {
                    collection.Remove(x);
                    x.OnUnbind(vm);
                });
            }
#endif

            /// <summary>
            /// 根据是否有手机号码确定键为[绑定手机]还是[换绑手机]
            /// </summary>
            /// <param name="hasPhoneNumber"></param>
            /// <returns></returns>
            public static PreferenceButton GetPhoneNumberId(bool hasPhoneNumber)
            {
                return hasPhoneNumber ? PreferenceButton.ChangePhoneNumber : PreferenceButton.BindPhoneNumber;
            }

            /// <summary>
            /// 创建实例
            /// </summary>
            /// <param name="id"></param>
            /// <param name="vm"></param>
            /// <returns></returns>
            public static PreferenceButtonViewModel Create(PreferenceButton id, IDisposableHolder vm, int groupId = default)
            {
                PreferenceButtonViewModel r = new() { Id = id, ItemViewGroup = groupId, };
                r.OnBind(vm);
                return r;
            }

            static bool GetAuthentication(PreferenceButton id)
            {
                var r = id switch
                {
                    PreferenceButton.UserProfile or
                    PreferenceButton.BindPhoneNumber or
                    PreferenceButton.ChangePhoneNumber => true,
                    _ => false,
                };
                return r;
            }

            /// <summary>
            /// 是否需要已登录的用户
            /// </summary>
            public bool Authentication => GetAuthentication(id);
        }
    }
}
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
    public class MyPageViewModel : PageViewModel
    {
        public MyPageViewModel()
        {
            Title = AppResources.My;

            preferenceButtons = new ObservableCollection<PreferenceButtonViewModel>
            {
                PreferenceButtonViewModel.Create(PreferenceButton.Settings, this),
                PreferenceButtonViewModel.Create(PreferenceButton.About, this),
            };

            UserService.Current.WhenAnyValue(x => x.User).Subscribe(value =>
            {
                if (value == null)
                {
                    nickNameNullValLangChangeSubscribe?.RemoveTo(this);
                    nickNameNullValLangChangeSubscribe = R.Current.WhenAnyValue(x => x)
                    .Subscribe(_ =>
                    {
                        // 未登录时显示的文本，多语言绑定
                        NickName = NickNameNullVal;
                    }).AddTo(this);

                    PreferenceButtonViewModel.RemoveAuthorized(preferenceButtons, this);
                }
                else
                {
                    nickNameNullValLangChangeSubscribe?.RemoveTo(this);
                    nickNameNullValLangChangeSubscribe = null;
                    NickName = value.NickName;

                    var editProfile = preferenceButtons.FirstOrDefault(x => x.Id == PreferenceButton.EditProfile);
                    if (editProfile == null)
                    {
                        editProfile = PreferenceButtonViewModel.Create(PreferenceButton.EditProfile, this);
                        preferenceButtons.Insert(0, editProfile);
                    }

                    var phoneNumber = preferenceButtons.FirstOrDefault(x => PreferenceButtonViewModel.IsPhoneNumber(x.Id));
                    if (phoneNumber == null)
                    {
                        phoneNumber = PreferenceButtonViewModel.Create(PreferenceButtonViewModel.GetPhoneNumberId(UserService.Current.HasPhoneNumber), this);
                        preferenceButtons.Insert(1, phoneNumber);
                    }
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
            EditProfile,
            BindPhoneNum,
            ChangePhoneNum,
            Settings,
            About,
        }

        /// <summary>
        /// 我的选项按钮视图模型
        /// </summary>
        public sealed class PreferenceButtonViewModel : ReactiveObject, IDisposable
        {
            PreferenceButtonViewModel()
            {
            }

            /// <summary>
            /// 根据键获取标题文本
            /// </summary>
            /// <param name="id"></param>
            /// <returns></returns>
            static string GetTitleById(PreferenceButton id)
            {
                var title = id switch
                {
                    PreferenceButton.EditProfile => AppResources.User_EditProfile,
                    PreferenceButton.BindPhoneNum => AppResources.User_BindPhoneNum,
                    PreferenceButton.ChangePhoneNum => AppResources.User_ChangePhoneNum,
                    PreferenceButton.Settings => AppResources.Settings,
                    PreferenceButton.About => AppResources.About,
                    _ => string.Empty,
                };
                return title;
            }

            /// <summary>
            /// 根据键获取图标
            /// </summary>
            /// <param name="id"></param>
            /// <returns></returns>
            static ResIcon GetIconById(PreferenceButton id)
            {
                var icon = id switch
                {
                    PreferenceButton.EditProfile => ResIcon.baseline_account_box_black_24,
                    PreferenceButton.BindPhoneNum => ResIcon.baseline_phone_black_24,
                    PreferenceButton.ChangePhoneNum => ResIcon.baseline_phone_black_24,
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
            public static bool IsPhoneNumber(PreferenceButton id) => id == PreferenceButton.BindPhoneNum || id == PreferenceButton.ChangePhoneNum;

            /// <summary>
            /// 从选项按钮组中移除已登录才有的选项组
            /// </summary>
            /// <param name="collection"></param>
            /// <param name="vm"></param>
            public static void RemoveAuthorized(ICollection<PreferenceButtonViewModel> collection, IDisposableHolder vm)
            {
                var removeArray = collection.Where(x => IsPhoneNumber(x.Id) || x.Id == PreferenceButton.EditProfile).ToArray();
                Array.ForEach(removeArray, x =>
                {
                    collection.Remove(x);
                    x.OnUnbind(vm);
                });
            }

            /// <summary>
            /// 根据是否有手机号码确定键为[绑定手机]还是[换绑手机]
            /// </summary>
            /// <param name="hasPhoneNumber"></param>
            /// <returns></returns>
            public static PreferenceButton GetPhoneNumberId(bool hasPhoneNumber)
            {
                return hasPhoneNumber ? PreferenceButton.ChangePhoneNum : PreferenceButton.BindPhoneNum;
            }

            PreferenceButton id;
            /// <summary>
            /// 唯一键
            /// </summary>
            public PreferenceButton Id
            {
                get => id;
                set
                {
                    if (this.RaiseAndSetIfChanged2(ref id, value)) return;
                    Title = GetTitleById(value);
                    Icon = GetIconById(value);
                }
            }

            string title = string.Empty;
            /// <summary>
            /// 标题文本
            /// </summary>
            public string Title
            {
                get => title;
                set => this.RaiseAndSetIfChanged(ref title, value);
            }

            ResIcon icon;
            /// <summary>
            /// 图标
            /// </summary>
            public ResIcon Icon
            {
                get => icon;
                set => this.RaiseAndSetIfChanged(ref icon, value);
            }

            IDisposable? disposable;

            /// <summary>
            /// 绑定父视图模型
            /// </summary>
            /// <param name="vm"></param>
            /// <returns></returns>
            PreferenceButtonViewModel OnBind(IDisposableHolder vm)
            {
                disposable = R.Current.WhenAnyValue(x => x).Subscribe(_ =>
                {
                    Title = GetTitleById(id);
                });
                disposable.AddTo(vm);
                return this;
            }

            /// <summary>
            /// 解绑父视图模型
            /// </summary>
            /// <param name="vm"></param>
            /// <returns></returns>
            PreferenceButtonViewModel OnUnbind(IDisposableHolder vm)
            {
                if (disposable != null)
                {
                    disposable.RemoveTo(vm);
                }
                return this;
            }

            /// <summary>
            /// 创建实例
            /// </summary>
            /// <param name="id"></param>
            /// <param name="vm"></param>
            /// <returns></returns>
            public static PreferenceButtonViewModel Create(PreferenceButton id, IDisposableHolder vm)
            {
                PreferenceButtonViewModel r = new() { Id = id };
                r.OnBind(vm);
                return r;
            }

            public void Dispose() => disposable?.Dispose();
        }
    }
}
using ReactiveUI;
using System.Application.Mvvm;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using Xamarin.Essentials;

namespace System.Application.UI.ViewModels
{
    partial class AboutPageViewModel : ViewModelBase
    {
        public string Title => TitleName;

        ObservableCollection<PreferenceButtonViewModel> preferenceButtons;
        public ObservableCollection<PreferenceButtonViewModel> PreferenceButtons
        {
            get => preferenceButtons;
            set => this.RaiseAndSetIfChanged(ref preferenceButtons, value);
        }

        public enum PreferenceButton
        {
            检查更新 = 1,
            更新日志,
            常见问题疑难解答,
            开放源代码许可,
            源码仓库,
            产品官网,
            联系我们,
            Bug反馈,
            账号注销,
        }

        public sealed class PreferenceButtonViewModel : RIdTitleViewModel<PreferenceButton>
        {
            PreferenceButtonViewModel()
            {
            }

            protected override string GetTitleById(PreferenceButton id)
            {
                var title = id switch
                {
                    PreferenceButton.检查更新 => AppResources.CheckUpdate,
                    PreferenceButton.更新日志 => AppResources.About_UpdateLog,
                    PreferenceButton.常见问题疑难解答 => AppResources.About_FAQ,
                    PreferenceButton.开放源代码许可 => AppResources.About_OpenSource,
                    PreferenceButton.源码仓库 => AppResources.SourceRepository,
                    PreferenceButton.产品官网 => AppResources.ProductOfficialWebsite,
                    PreferenceButton.联系我们 => AppResources.About_Contactus,
                    PreferenceButton.Bug反馈 => AppResources.BugFeedback,
                    PreferenceButton.账号注销 => AppResources.DelAccount,
                    _ => string.Empty,
                };
                return title;
            }

            public static void RemoveAuthorized(ICollection<PreferenceButtonViewModel> collection, IDisposableHolder vm)
            {
                var removeArray = collection.Where(x => x.Id == PreferenceButton.账号注销).ToArray();
                Array.ForEach(removeArray, x =>
                {
                    collection.Remove(x);
                    x.OnUnbind(vm);
                });
            }

            /// <summary>
            /// 创建实例
            /// </summary>
            /// <param name="id"></param>
            /// <param name="vm"></param>
            /// <returns></returns>
            public static PreferenceButtonViewModel Create(PreferenceButton id, IDisposableHolder vm)
            {
                PreferenceButtonViewModel r = new() { Id = id, };
                r.OnBind(vm);
                return r;
            }
        }

        public const string GitHub = "GitHub";
        public const string Gitee = "Gitee";
        public static readonly string[] SourceRepositories = new[] { GitHub, Gitee };
        public const string Title_0 = "Steam++ Tools ";
        public const string Title_1 = "2.0";
        public const string Developers_0 = "Developers: ";
        public const string Separator = " - ";
        public const string Separator2 = "   |   ";
        public const string At_Rmbadmin = "@软妹币玩家";
        public const string At_AigioL = "@AigioL";
        public const string At_Mossimos = "@Mossimos";
        public const string At_Cliencer = "@Cliencer克总";
        public const string BusinessCooperationContact_0 = "Business Cooperation Contact: ";
        public const string OpenSourceLicensed_0 = "This open source software is licensed with ";
        public const string OpenSourceLicensed_1 = "GPLv3 License";
        const string _ThemeAccentBrushKey = "#FF0078D7";
        public static readonly Color ThemeAccentBrushKey = ColorConverters.FromHex(_ThemeAccentBrushKey);
    }
}
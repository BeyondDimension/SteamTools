using DynamicData.Binding;
using ReactiveUI;
using System.Application.Models.Settings;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace System.Application.UI.ViewModels
{
    public class SettingsPageViewModel :
#if !__MOBILE__
        TabItemViewModel
#else
        ViewModelBase
#endif
    {
        public static SettingsPageViewModel Instance { get; } = new();

        public
#if !__MOBILE__
            override
#endif
            string Name
        {
            get => AppResources.Settings;
            protected set { throw new NotImplementedException(); }
        }

        KeyValuePair<string, string> _SelectLanguage;
        public KeyValuePair<string, string> SelectLanguage
        {
            get => _SelectLanguage;
            set => this.RaiseAndSetIfChanged(ref _SelectLanguage, value);
        }

#if !__MOBILE__
        KeyValuePair<string, string> _SelectFont;
        public KeyValuePair<string, string> SelectFont
        {
            get => _SelectFont;
            set => this.RaiseAndSetIfChanged(ref _SelectFont, value);
        }

        IReadOnlyCollection<UpdateChannelType>? _UpdateChannels;
        public IReadOnlyCollection<UpdateChannelType>? UpdateChannels
        {
            get => _UpdateChannels;
            set => this.RaiseAndSetIfChanged(ref _UpdateChannels, value);
        }
#endif

        public SettingsPageViewModel()
        {
#if !__MOBILE__
            IconKey = nameof(SettingsPageViewModel).Replace("ViewModel", "Svg");
#endif

            SelectLanguage = R.Languages.FirstOrDefault(x => x.Key == UISettings.Language.Value);
            this.WhenValueChanged(x => x.SelectLanguage, false)
                  .Subscribe(x => UISettings.Language.Value = x.Key);

#if !__MOBILE__
            SelectFont = R.Fonts.FirstOrDefault(x => x.Value == UISettings.FontName.Value);
            this.WhenValueChanged(x => x.SelectFont, false)
                  .Subscribe(x => UISettings.FontName.Value = x.Value);


            UpdateChannels = Enum.GetValues<UpdateChannelType>();
#endif
        }

#if __MOBILE__
        public static string[] GetThemes() => new[]
        {
            AppResources.Settings_UI_SystemDefault,
            AppResources.Settings_UI_Light,
            AppResources.Settings_UI_Dark,
        };
#endif

#if !__MOBILE__
        const double clickInterval = 3d;
        readonly Dictionary<string, DateTime> clickTimeRecord = new();
        public void OpenFolder(string tag)
        {
            var path = tag switch
            {
                IOPath.DirName_AppData => IOPath.AppDataDirectory,
                IOPath.DirName_Cache => IOPath.CacheDirectory,
                AppHelper.LogDirName => AppHelper.LogDirPath,
                _ => null,
            };
            if (path != null)
            {
                var hasKey = clickTimeRecord.TryGetValue(path, out var dt);
                var now = DateTime.Now;
                if (hasKey && (now - dt).TotalSeconds <= clickInterval) return;
                IDesktopPlatformService.Instance.OpenFolder(path);
                if (!clickTimeRecord.TryAdd(path, now)) clickTimeRecord[path] = now;
            }
        }
#endif

        /// <summary>
        /// 开始计算指定文件夹大小
        /// </summary>
        /// <param name="isStartCacheSizeCalc"></param>
        /// <param name="sizeFormat"></param>
        /// <param name="action"></param>
        public static void StartCacheSizeCalc(ref bool isStartCacheSizeCalc, string sizeFormat, Action<string> action)
        {
            if (isStartCacheSizeCalc) return;
            isStartCacheSizeCalc = true;
            action(AppResources.Settings_General_Calcing);
            string? dirPath;
            if (sizeFormat == AppResources.Settings_General_CacheSize)
            {
                dirPath = IOPath.CacheDirectory;
            }
            else if (sizeFormat == AppResources.Settings_General_LogSize)
            {
                dirPath = AppHelper.LogDirPath;
            }
            else
            {
                dirPath = null;
            }
            if (dirPath != null)
            {
                Task.Run(async () =>
                {
                    var length = IOPath.GetDirectorySize(dirPath);
                    var lenString = IOPath.GetSizeString(length);
                    await MainThread2.InvokeOnMainThreadAsync(() =>
                    {
                        action(sizeFormat.Format(lenString));
                    });
                });
            }
        }
    }
}
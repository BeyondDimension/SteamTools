using DynamicData.Binding;
using ReactiveUI;
using System.Application.Settings;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using R = System.Application.UI.Resx.Abstractions.R;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels.Abstractions
{
    public partial class SettingsPageViewModel
    {
        public SettingsPageViewModel()
        {
            IconKey = nameof(SettingsPageViewModel);


            SelectLanguage = R.Languages.FirstOrDefault(x => x.Key == UISettings.Language.Value);
            this.WhenValueChanged(x => x.SelectLanguage, false)
                  .Subscribe(x => UISettings.Language.Value = x.Key);

            UpdateChannels = Enum2.GetAll<UpdateChannelType>();
        }

        public static SettingsPageViewModel Instance => DI.Get<SettingsPageViewModel>();

        KeyValuePair<string, string> _SelectLanguage;
        public KeyValuePair<string, string> SelectLanguage
        {
            get => _SelectLanguage;
            set => this.RaiseAndSetIfChanged(ref _SelectLanguage, value);
        }

        IReadOnlyCollection<UpdateChannelType>? _UpdateChannels;
        public IReadOnlyCollection<UpdateChannelType>? UpdateChannels
        {
            get => _UpdateChannels;
            set => this.RaiseAndSetIfChanged(ref _UpdateChannels, value);
        }

        public static string[] GetThemes() => new[]
        {
            AppResources.Settings_UI_SystemDefault,
            AppResources.Settings_UI_Light,
            AppResources.Settings_UI_Dark,
        };

        /// <summary>
        /// 开始计算指定文件夹大小
        /// </summary>
        /// <param name="isStartCacheSizeCalc"></param>
        /// <param name="sizeFormat"></param>
        /// <param name="action"></param>
        public static void StartCacheSizeCalc(ref bool isStartCacheSizeCalc, string sizeFormat, Action<string> action, CancellationToken cancellationToken = default)
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
                dirPath = IApplication.LogDirPath;
            }
            else
            {
                dirPath = null;
            }
            if (dirPath != null)
            {
                try
                {
                    Task.Run(async () =>
                    {
                        var length = IOPath.GetDirectorySize(dirPath);
                        var lenString = IOPath.GetSizeString(length);
                        await MainThread2.InvokeOnMainThreadAsync(() =>
                        {
                            action(sizeFormat.Format(lenString));
                        });
                    }, cancellationToken);
                }
                catch (OperationCanceledException)
                {

                }
            }
        }
    }
}

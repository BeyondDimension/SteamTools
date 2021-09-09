using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.Models;
using System.Application.Models.Settings;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static System.Application.UI.Resx.AppResources;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(SettingsActivity))]
    [Activity(Theme = ManifestConstants.MainTheme_NoActionBar,
         LaunchMode = LaunchMode.SingleTask,
         ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class SettingsActivity : BaseActivity<activity_settings, SettingsPageViewModel>
    {
        protected override int? LayoutResource => Resource.Layout.activity_settings;

        protected override SettingsPageViewModel? OnCreateViewModel() => SettingsPageViewModel.Instance;

        readonly Dictionary<View, ComboBoxHelper.ListPopupWindowWrapper> comboBoxs = new();

        void SetIsAutoCheckUpdateChecked() => binding!.swGeneralSettingsIsAutoCheckUpdate.Checked = GeneralSettings.IsAutoCheckUpdate.Value;
        void SetUpdateChannelText() => binding!.tvGeneralSettingsUpdateChannelValue.Text = GeneralSettings.UpdateChannel.Value.ToString();
        void SetThemeText() => binding!.tvUISettingsThemeValue.Text = ((AppTheme)UISettings.Theme.Value).ToString3();

        bool isStartCacheSizeCalc;
        bool isStartLogSizeCalc;
        readonly CancellationTokenSource cts = new();
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this.SetSupportActionBarWithNavigationClick(binding!.toolbar, true);

#if IS_STORE_PACKAGE // 渠道包隐藏下载更新渠道，更新通过应用商店分发
            binding.layoutRootGeneralSettingsUpdateChannel.Visibility = ViewStates.Gone;
#endif

            R.Current.WhenAnyValue(x => x.Res).SubscribeInMainThread(_ =>
            {
                Title = ViewModel!.Name;
                if (binding == null) return;
                binding.tvUISettings.Text = Settings_UI;
                binding.tvUISettingsLanguage.Text = Settings_Language;
                binding.tvUISettingsTheme.Text = Settings_Theme;
                binding.tvGeneralSettings.Text = Settings_General;
                binding.tvGeneralSettingsIsAutoCheckUpdate.Text = Settings_General_AutoCheckUpdate;
                binding.tvGeneralSettingsUpdateChannel.Text = Settings_General_UpdateChannel;
                binding.tvGeneralSettingsStorageSpace.Text = Settings_General_StorageSpace;
                binding.tvOSAppDetailsSettings.Text = Settings_General_AppDetailsSettings;
                binding.tvOSAppNotificationSettings.Text = Settings_General_AppNotificationSettings;
                if (comboBoxs.TryGetValue(binding.layoutRootUISettingsTheme, out var comboBoxUISettingsTheme))
                {
                    comboBoxUISettingsTheme.Items = SettingsPageViewModel.GetThemes();
                }
                binding.tvGeneralSettingsCaptureScreen.Text = Settings_General_CaptureScreen;
                binding.tvGeneralSettingsCaptureScreenDesc.Text = Settings_General_CaptureScreen_Desc;
            }).AddTo(this);

            ViewModel!.WhenAnyValue(x => x.SelectLanguage).SubscribeInMainThread(x =>
            {
                if (binding == null) return;
                binding.tvUISettingsLanguageValue.Text = x.Value;
            }).AddTo(this);

            comboBoxs.Add(binding.layoutRootUISettingsLanguage, ComboBoxHelper.Popup(this, R.Languages.Select(x => x.Value).ToArray(), x =>
            {
                ViewModel!.SelectLanguage = R.Languages.FirstOrDefault(y => y.Value == x);
            }, binding.layoutUISettingsLanguage));
            comboBoxs.Add(binding.layoutRootUISettingsTheme, ComboBoxHelper.Popup(this, SettingsPageViewModel.GetThemes(), x =>
            {
                if (comboBoxs.TryGetValue(binding.layoutRootUISettingsTheme, out var comboBoxUISettingsTheme))
                {
                    var index = Array.IndexOf(comboBoxUISettingsTheme.Items, x);
                    if (index >= 0)
                    {
                        UISettings.Theme.Value = (short)index;
                        SetThemeText();
                    }
                }
            }, binding.layoutUISettingsTheme));
            comboBoxs.Add(binding.layoutRootGeneralSettingsUpdateChannel, ComboBoxHelper.Popup(this, Enum2.GetAllStrings<UpdateChannelType>(), x =>
            {
                if (!Enum.TryParse<UpdateChannelType>(x, out var value)) return;
                GeneralSettings.UpdateChannel.Value = value;
                SetUpdateChannelText();
            }, binding.layoutUISettingsTheme));

            SetOnClickListener(comboBoxs.Keys);
            SetOnClickListener(
                binding.layoutRootGeneralSettingsIsAutoCheckUpdate,
                binding.layoutRootGeneralSettingsStorageSpace,
                binding.layoutRootOSAppDetailsSettings,
                binding.layoutRootOSAppNotificationSettings,
                binding.layoutRootGeneralSettingsCaptureScreen);

            SetIsAutoCheckUpdateChecked();
            SetCaptureScreenChecked();
            SetUpdateChannelText();
            SetThemeText();

            SettingsPageViewModel.StartCacheSizeCalc(ref isStartCacheSizeCalc, Settings_General_CacheSize, x =>
            {
                if (binding == null) return;
                binding.tvGeneralSettingsStorageSpaceValue.Text = x;
            }, cts.Token);
            SettingsPageViewModel.StartCacheSizeCalc(ref isStartLogSizeCalc, Settings_General_LogSize, x =>
            {
                if (binding == null) return;
                binding.tvGeneralSettingsStorageSpaceValue2.Text = x;
            }, cts.Token);
        }

        protected override void OnResume()
        {
            base.OnResume();
            var enabledNotification = INotificationService.Instance.AreNotificationsEnabled();
            binding!.swOSAppNotificationSettings.Checked = enabledNotification;
        }

        protected override void OnClick(View view)
        {
            foreach (var item in comboBoxs)
            {
                if (view.Id == item.Key.Id)
                {
                    item.Value.Show();
                    return;
                }
            }

            if (view.Id == Resource.Id.layoutRootOSAppDetailsSettings)
            {
                GoToPlatformPages.AppDetailsSettings(this);
            }
            else if (view.Id == Resource.Id.layoutRootOSAppNotificationSettings)
            {
                GoToPlatformPages.NotificationSettings(this);
            }
            else if (view.Id == Resource.Id.layoutRootGeneralSettingsIsAutoCheckUpdate)
            {
                GeneralSettings.IsAutoCheckUpdate.Value = !GeneralSettings.IsAutoCheckUpdate.Value;
                SetIsAutoCheckUpdateChecked();
            }
            else if (view.Id == Resource.Id.layoutRootGeneralSettingsStorageSpace)
            {
                this.StartActivity<ExplorerActivity>();
            }
            else if (view.Id == Resource.Id.layoutRootGeneralSettingsCaptureScreen)
            {
                GeneralSettings.CaptureScreen.Value = !GeneralSettings.CaptureScreen.Value;
                SetCaptureScreenChecked();
            }

            base.OnClick(view);
        }

        void SetCaptureScreenChecked() => binding!.swGeneralSettingsCaptureScreen.Checked = GeneralSettings.CaptureScreen.Value;

        protected override void OnDestroy()
        {
            cts.Cancel();
            base.OnDestroy();
            foreach (var item in comboBoxs)
            {
                item.Value.Dispose();
            }
            comboBoxs.Clear();
        }
    }
}
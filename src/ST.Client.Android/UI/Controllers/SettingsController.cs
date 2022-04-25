using Android.Views;
using ReactiveUI;
using System.Application.Models;
using System.Application.Settings;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Linq;
using static System.Application.UI.Resx.AppResources;
using DynamicData;
using System.Application.UI.Activities;
using System.Application.Services;
using Android.Runtime;
using Android.App;
using Android.Content.PM;
using M = System.Application.UI.ViewModels.SettingsPageViewModel;
using V = Binding.fragment_settings;
using C = System.Application.UI.Controllers.SettingsController;

namespace System.Application.UI.Controllers
{
    internal sealed class SettingsController : ControllerBase<V, M>
    {
        public SettingsController(IHost host, V binding) : base(host, binding)
        {

        }

        public override M? OnCreateViewModel() => M.Instance;

        readonly Dictionary<View, ComboBoxHelper.ListPopupWindowWrapper<string>> comboBoxs = new();

        void SetIsAutoCheckUpdateChecked() => binding.swGeneralSettingsIsAutoCheckUpdate.Checked = GeneralSettings.IsAutoCheckUpdate.Value;

        void SetUpdateChannelText() => binding.tvGeneralSettingsUpdateChannelValue.Text = GeneralSettings.UpdateChannel.Value.ToString();

        void SetThemeText() => binding.tvUISettingsThemeValue.Text = ((AppTheme)UISettings.Theme.Value).ToString3();

        void SetCaptureScreenChecked() => binding.swGeneralSettingsCaptureScreen.Checked = GeneralSettings.CaptureScreen.Value;

        public override void OnCreate()
        {
            if (IsActivity)
            {
                SetSupportActionBarWithNavigationClick(true);
            }

#if IS_STORE_PACKAGE // 渠道包隐藏下载更新渠道，更新通过应用商店分发
            binding.layoutRootGeneralSettingsUpdateChannel.Visibility = ViewStates.Gone;
#endif

            R.Subscribe(() =>
            {
                if (IsActivity)
                {
                    Activity.Title = ViewModel.Name;
                }
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
                    comboBoxUISettingsTheme.Items = M.GetThemes();
                }
                binding.tvGeneralSettingsCaptureScreen.Text = Settings_General_CaptureScreen;
                binding.tvGeneralSettingsCaptureScreenDesc.Text = Settings_General_CaptureScreen_Desc;
                binding.tvSystemSettings.Text = Settings_System;
            }).AddTo(this);

            ViewModel!.WhenAnyValue(x => x.SelectLanguage).SubscribeInMainThread(x =>
            {
                if (binding == null) return;
                binding.tvUISettingsLanguageValue.Text = x.Value;
            }).AddTo(this);

            comboBoxs.Add(binding.layoutRootUISettingsLanguage, ComboBoxHelper.Popup(Context, R.Languages.Select(x => x.Value).ToJavaList(), x =>
            {
                ViewModel!.SelectLanguage = R.Languages.FirstOrDefault(y => y.Value == x);
            }, binding.layoutUISettingsLanguage));
            comboBoxs.Add(binding.layoutRootUISettingsTheme, ComboBoxHelper.Popup(Context, M.GetThemes(), x =>
            {
                if (comboBoxs.TryGetValue(binding.layoutRootUISettingsTheme, out var comboBoxUISettingsTheme))
                {
                    var index = comboBoxUISettingsTheme.Items.IndexOf(x);
                    if (index >= 0)
                    {
                        UISettings.Theme.Value = (short)index;
                        SetThemeText();
                    }
                }
            }, binding.layoutUISettingsTheme));
            comboBoxs.Add(binding.layoutRootGeneralSettingsUpdateChannel, ComboBoxHelper.Popup(Context, Enum2.GetAllStrings<UpdateChannelType>(), x =>
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

            M.StartSizeCalcByCacheSize(x =>
            {
                if (binding == null) return;
                binding.tvGeneralSettingsStorageSpaceValue.Text = x;
            });
            M.StartSizeCalcByLogSize(x =>
            {
                if (binding == null) return;
                binding.tvGeneralSettingsStorageSpaceValue2.Text = x;
            });
        }

        public override void OnResume()
        {
            base.OnResume();
            var enabledNotification = INotificationService.Instance.AreNotificationsEnabled();
            binding!.swOSAppNotificationSettings.Checked = enabledNotification;
        }

        public override bool OnClick(View view)
        {
            foreach (var item in comboBoxs)
            {
                if (view.Id == item.Key.Id)
                {
                    item.Value.Show();
                    return true;
                }
            }

            if (view.Id == Resource.Id.layoutRootOSAppDetailsSettings)
            {
                GoToPlatformPages.AppDetailsSettings(Context);
                return true;
            }
            else if (view.Id == Resource.Id.layoutRootOSAppNotificationSettings)
            {
                GoToPlatformPages.NotificationSettings(Context);
                return true;
            }
            else if (view.Id == Resource.Id.layoutRootGeneralSettingsIsAutoCheckUpdate)
            {
                GeneralSettings.IsAutoCheckUpdate.Value = !GeneralSettings.IsAutoCheckUpdate.Value;
                SetIsAutoCheckUpdateChecked();
                return true;
            }
            else if (view.Id == Resource.Id.layoutRootGeneralSettingsStorageSpace)
            {
                Activity.StartActivity<ExplorerActivity>();
                return true;
            }
            else if (view.Id == Resource.Id.layoutRootGeneralSettingsCaptureScreen)
            {
                GeneralSettings.CaptureScreen.Value = !GeneralSettings.CaptureScreen.Value;
                SetCaptureScreenChecked();
                return true;
            }

            return base.OnClick(view);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (IsActivity)
            {
                // Fragment 中执行会导致闪退
                comboBoxs.Clear();
            }
        }
    }
}

namespace System.Application.UI.Fragments
{
#if __XAMARIN_FORMS__
    internal sealed class SettingsFragment : BaseMvcFragment<V, M, C>
    {
        protected override int? LayoutResource => Resource.Layout.fragment_settings;
    }
#endif
}

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(SettingsActivity))]
    [Activity(Theme = ManifestConstants.MainTheme2_NoActionBar,
         LaunchMode = LaunchMode.SingleTask,
         ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class SettingsActivity : BaseMvcActivity<V, M, C>
    {
        protected override int? LayoutResource => Resource.Layout.activity_settings_not_binding;
    }
}
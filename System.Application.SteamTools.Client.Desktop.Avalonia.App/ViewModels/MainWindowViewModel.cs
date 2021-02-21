using ReactiveUI;
using System.Properties;

namespace System.Application.UI.ViewModels
{
    public class MainWindowViewModel : WindowViewModel
    {
        bool mTopmost;

        public bool Topmost
        {
            get => mTopmost;
            set => this.RaiseAndSetIfChanged(ref mTopmost, value);
        }

        public MainWindowViewModel() : base()
        {
            Title = ThisAssembly.AssemblyTrademark;
            SettingsPage = GetViewModel<SettingsPageViewModel>();
        }

        #region ResStrings

        string mWelcome = string.Empty;

        [ResString]
        public string Welcome
        {
            get => mWelcome;
            set => this.RaiseAndSetIfChanged(ref mWelcome, value);
        }

        string mCommunityFix = string.Empty;

        [ResString]
        public string CommunityFix
        {
            get => mCommunityFix;
            set => this.RaiseAndSetIfChanged(ref mCommunityFix, value);
        }

        string mUserFastChange = string.Empty;

        [ResString]
        public string UserFastChange
        {
            get => mUserFastChange;
            set => this.RaiseAndSetIfChanged(ref mUserFastChange, value);
        }

        string mSteamApps = string.Empty;

        [ResString]
        public string SteamApps
        {
            get => mSteamApps;
            set => this.RaiseAndSetIfChanged(ref mSteamApps, value);
        }

        string mSteamAuth = string.Empty;

        [ResString]
        public string SteamAuth
        {
            get => mSteamAuth;
            set => this.RaiseAndSetIfChanged(ref mSteamAuth, value);
        }

        string mGameRelated = string.Empty;

        [ResString]
        public string GameRelated
        {
            get => mGameRelated;
            set => this.RaiseAndSetIfChanged(ref mGameRelated, value);
        }

        string mSettings = string.Empty;

        [ResString]
        public string Settings
        {
            get => mSettings;
            set => this.RaiseAndSetIfChanged(ref mSettings, value);
        }

        string mAbout = string.Empty;

        [ResString]
        public string About
        {
            get => mAbout;
            set => this.RaiseAndSetIfChanged(ref mAbout, value);
        }

        #endregion

        public SettingsPageViewModel SettingsPage { get; }
    }
}
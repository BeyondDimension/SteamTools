using ReactiveUI;
using System.Application.Models;
using System.Application.Models.Settings;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Properties;
using System.Threading.Tasks;
using System.Windows;

namespace System.Application.UI.ViewModels
{
    public class SteamAccountPageViewModel : TabItemViewModel
    {
        readonly ISteamService steamService = DI.Get<ISteamService>();
        readonly IHttpService httpService = DI.Get<IHttpService>();
        readonly ISteamworksWebApiService webApiService = DI.Get<ISteamworksWebApiService>();

        public override string Name
        {
            get => AppResources.UserFastChange;
            protected set { throw new NotImplementedException(); }
        }

        private IList<MenuItemViewModel> _MenuItems = new[]
    {
        new MenuItemViewModel
        {
            Header = "_File",
            Items = new[]
            {
                new MenuItemViewModel { Header = "_Open...",IconKey="CloseDrawing" },
                new MenuItemViewModel { Header = "Save" },
                new MenuItemViewModel { Header = "-" },
                new MenuItemViewModel
                {
                    Header = "Recent",
                    Items = new[]
                    {
                        new MenuItemViewModel
                        {
                            Header = "File1.txt",
                        },
                        new MenuItemViewModel
                        {
                            Header = "File2.txt",
                        },
                    }
                },
            }
        },
        new MenuItemViewModel
        {
            Header = "_Edit",
            Items = new[]
            {
                new MenuItemViewModel { Header = "_Copy" },
                new MenuItemViewModel { Header = "_Paste" },
            }
        }
    };
        public override IList<MenuItemViewModel> MenuItems
        {
            get => _MenuItems;
            set => this.RaiseAndSetIfChanged(ref _MenuItems, value);
        }

        /// <summary>
        /// steam记住的用户列表
        /// </summary>
        private IList<SteamUser>? _steamUsers;
        public IList<SteamUser>? SteamUsers
        {
            get => _steamUsers;
            set => this.RaiseAndSetIfChanged(ref _steamUsers, value);
        }

        internal async override Task Initialize()
        {
            SteamUsers = new ObservableCollection<SteamUser>(steamService.GetRememberUserList());

            if (!SteamUsers.Any_Nullable())
            {
                //Toast.Show("");
                return;
            }

#if DEBUG
            for (var i = 0; i < 10; i++)
            {
                SteamUsers.Add(SteamUsers[0]);
            }
#endif

            var users = SteamUsers.ToArray();
            for (var i = 0; i < SteamUsers.Count; i++)
            {
                var temp = users[i];
                users[i] = await webApiService.GetUserInfo(SteamUsers[i].SteamId64);
                users[i].AccountName = temp.AccountName;
                users[i].SteamID = temp.SteamID;
                users[i].PersonaName = temp.PersonaName;
                users[i].RememberPassword = temp.RememberPassword;
                users[i].MostRecent = temp.MostRecent;
                users[i].Timestamp = temp.Timestamp;
                users[i].LastLoginTime = temp.LastLoginTime;
                users[i].WantsOfflineMode = temp.WantsOfflineMode;
                users[i].SkipOfflineModeWarning = temp.SkipOfflineModeWarning;
                users[i].OriginVdfString = temp.OriginVdfString;
                users[i].AvatarStream = string.IsNullOrEmpty(users[i].AvatarFull) ? null : await httpService.GetImageAsync(users[i].AvatarFull, ImageChannelType.SteamAvatars);
            }
            SteamUsers = new ObservableCollection<SteamUser>(users.OrderByDescending(o => o.RememberPassword).ThenByDescending(o => o.LastLoginTime).ToList());
        }

        public void SteamId_Click(SteamUser user)
        {
            if (user.WantsOfflineMode)
            {
                UserModeChange(user, false);
            }
            steamService.SetCurrentUser(user.AccountName);
            steamService.TryKillSteamProcess();
            steamService.StartSteam(SteamSettings.SteamStratParameter.Value);
        }

        public void OfflineModeButton_Click(SteamUser user)
        {
            if (user.WantsOfflineMode == false)
            {
                UserModeChange(user, true);
            }
            SteamId_Click(user);
        }

        private void UserModeChange(SteamUser user, bool OfflineMode)
        {
            user.WantsOfflineMode = OfflineMode;
            steamService.UpdateLocalUserData(user);
            user.OriginVdfString = user.CurrentVdfString;
        }

        public void DeleteUserButton_Click(SteamUser user)
        {
            var result = MessageBoxCompat.ShowAsync(@AppResources.UserChange_DeleteUserTip, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel).ContinueWith(s =>
            {
                if (s.Result == MessageBoxResultCompat.OK)
                {
                    steamService.DeleteLocalUserData(user);
                    SteamUsers.Remove(user);
                }
            });
        }
    }
}
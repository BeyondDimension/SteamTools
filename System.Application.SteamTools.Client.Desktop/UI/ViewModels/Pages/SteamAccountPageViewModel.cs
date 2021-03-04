using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace System.Application.UI.ViewModels
{
    public class SteamAccountPageViewModel : TabItemViewModel
    {

        public override string Name
        {
            get => AppResources.UserFastChange;
            protected set { throw new NotImplementedException(); }
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
            ISteamService steamService = DI.Get<ISteamService>();
            IHttpService httpService = DI.Get<IHttpService>();
            ISteamworksWebApiService webApiService = DI.Get<ISteamworksWebApiService>();

            SteamUsers = new ObservableCollection<SteamUser>(steamService.GetRememberUserList());
            if (!SteamUsers.Any_Nullable())
            {
                return;
            }
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
    }
}
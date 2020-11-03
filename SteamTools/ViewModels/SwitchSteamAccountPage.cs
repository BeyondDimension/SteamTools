using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetroRadiance.UI;
using SteamTool.Core.Model;
using SteamTool.Core;
using SteamTool.Service;
using SteamTool.WebApi.Service.SteamDb;
using SteamTools.Models;
using Microsoft.Xaml.Behaviors.Core;
using System.Diagnostics;

namespace SteamTools.ViewModels
{
    public class SwitchSteamAccountPage : TabItemViewModel
    {
        private readonly SteamToolService steamService = SteamToolCore.Instance.Get<SteamToolService>();
        private readonly SteamDbApiService webApiService = WebApiService.Instance.Get<SteamDbApiService>();

        public override string Name
        {
            get { return Properties.Resources.UserFastChange; }
            protected set { throw new NotImplementedException(); }
        }

        public SwitchSteamAccountPage()
        {
            SteamUsers = GlobalVariable.Instance.LocalSteamUser;
            Task.Run(() =>
             {
                 SteamUser[] users = new SteamUser[SteamUsers.Count];
                 SteamUsers.CopyTo(users);
                 for (var i = 0; i < SteamUsers.Count; i++)
                 {
                     var temp = users[i];
                     users[i] = webApiService.GetUserInfo(SteamUsers[i].SteamId64);
                     users[i].AccountName = temp.AccountName;
                     users[i].RememberPassword = temp.RememberPassword;
                     users[i].MostRecent = temp.MostRecent;
                     users[i].Timestamp = temp.Timestamp;
                     users[i].LastLoginTime = temp.LastLoginTime;
                 }
                 SteamUsers = users.ToList();
             }).ContinueWith(s => s.Dispose());
        }

        /// <summary>
        /// steam记住的用户列表
        /// </summary>
        private List<SteamUser> _steamUsers;

        public List<SteamUser> SteamUsers
        {
            get => this._steamUsers;
            set
            {
                if (this._steamUsers != value)
                {
                    this._steamUsers = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public void SteamId_OnClick(string parameter)
        {
            steamService.SetCurrentUser(parameter);
            steamService.KillSteamProcess();
            steamService.StartSteam();
        }

        public void HeadImage_OnClick(string parameter)
        {
            Process.Start(parameter);
        }
    }
}

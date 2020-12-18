using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetroRadiance.UI;
using SteamTool.Model;
using SteamTool.Core;
using SteamTools.Models;
using Microsoft.Xaml.Behaviors.Core;
using System.Diagnostics;
using SteamTool.Steam.Service.Web;
using SteamTool.Steam.Service;
using SteamTools.Services;

namespace SteamTools.ViewModels
{
    public class SteamAccountPageViewModel : TabItemViewModel
    {
        private readonly SteamToolService steamService = SteamToolCore.Instance.Get<SteamToolService>();
        private readonly SteamDbApiService webApiService = SteamService.Instance.Get<SteamDbApiService>();
        private readonly VdfService vdfService = SteamToolCore.Instance.Get<VdfService>();

        public override string Name
        {
            get { return Properties.Resources.UserFastChange; }
            protected set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// steam记住的用户列表
        /// </summary>
        private BindingList<SteamUser> _steamUsers;

        public BindingList<SteamUser> SteamUsers
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

        internal override void Initialize()
        {
            StatusService.Current.Notify("加载本地Steam用户数据");
            Task.Run(async () =>
            {
                SteamUsers = new BindingList<SteamUser>(steamService.GetAllUser());
                var users = SteamUsers.ToArray();
                for (var i = 0; i < SteamUsers.Count; i++)
                {
                    var temp = users[i];
                    users[i] = await webApiService.GetUserInfo(SteamUsers[i].SteamId64);
                    users[i].AccountName = temp.AccountName;
                    users[i].RememberPassword = temp.RememberPassword;
                    users[i].MostRecent = temp.MostRecent;
                    users[i].Timestamp = temp.Timestamp;
                    users[i].LastLoginTime = temp.LastLoginTime;
                }
                if (SteamUsers.Count > 0)
                {
                    SteamUsers = new BindingList<SteamUser>(users.OrderByDescending(o => o.MostRecent).ThenByDescending(o => o.RememberPassword).ThenByDescending(o => o.LastLoginTime).ToList());
                    StatusService.Current.Notify("加载本地Steam用户数据完成");
                }
                else
                {
                    StatusService.Current.Notify("没有检测到Steam用户数据");
                }
            }).ContinueWith(s => s.Dispose());
        }


        public void SteamId_OnClick(string parameter)
        {
            steamService.SetCurrentUser(parameter);
            steamService.KillSteamProcess();
            steamService.StartSteam();
            SteamConnectService.Current.IsConnectToSteam = false;
        }

        public static void HeadImage_OnClick(string parameter)
        {
            Process.Start(parameter);
        }

        public void DeleteAccount_OnClick(SteamUser user)
        {
            var result = WindowService.Current.MainWindow.Dialog("确定要删除这条本地记录帐户数据吗？");
            if (result)
            {
                vdfService.DeleteVdfValueByKey(steamService.SteamPath + SteamToolService.UserVdfPath, user.SteamId64.ToString());
                SteamUsers.Remove(user);
            }
        }
    }
}

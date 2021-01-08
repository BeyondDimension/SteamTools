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
using SteamTool.Core.Common;
using SteamTools.Models.Settings;

namespace SteamTools.ViewModels
{
    public class SteamAccountPageViewModel : TabItemViewModel
    {
        private readonly SteamToolService steamService = SteamToolCore.Instance.Get<SteamToolService>();
        private readonly SteamDbApiService webApiService = SteamService.Instance.Get<SteamDbApiService>();

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

        internal async override Task Initialize()
        {
            StatusService.Current.Notify("加载本地Steam用户数据");
            await Task.Run(async () =>
            {
                SteamUsers = new BindingList<SteamUser>(steamService.GetRememberUserList());
                if (SteamUsers?.Count < 1)
                {
                    StatusService.Current.Notify("没有检测到Steam用户数据");
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
                }
                SteamUsers = new BindingList<SteamUser>(users.OrderByDescending(o => o.MostRecent).ThenByDescending(o => o.RememberPassword).ThenByDescending(o => o.LastLoginTime).ToList());
                StatusService.Current.Notify("加载本地Steam用户数据完成");
            }).ContinueWith(s => { Logger.Error(s.Exception); WindowService.Current.ShowDialogWindow(s.Exception.Message); }, TaskContinuationOptions.OnlyOnFaulted).ContinueWith(s => s.Dispose());
        }


        public void SteamId_OnClick(string parameter)
        {
            steamService.SetCurrentUser(parameter);
            steamService.KillSteamProcess();
            steamService.StartSteam(GeneralSettings.SteamStratParameter.Value);
            SteamConnectService.Current.IsConnectToSteam = false;
        }

        public static void HeadImage_OnClick(string parameter)
        {
            Process.Start(parameter);
        }

        public void DeleteAccount_OnClick(SteamUser user)
        {
            var result = WindowService.Current.MainWindow.Dialog("确定要删除这条本地记录帐户数据吗？" + Environment.NewLine + "这将会删除此账户在本地的Steam缓存数据。");
            if (result)
            {
                steamService.DeleteSteamLocalUserData(user);
                SteamUsers.Remove(user);
            }
        }

        public void AccountOfflineMode_Checked(SteamUser user)
        {
            steamService.UpdateSteamLocalUserData(user);
            user.OriginVdfString = user.CurrentVdfString;
        }
    }
}

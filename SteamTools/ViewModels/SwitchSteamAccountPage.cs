using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetroRadiance.UI;
using SteamTool.Core.Model;
using SteamTool.Core;

namespace SteamTools.ViewModels
{
    public class SwitchSteamAccountPage : TabItemViewModel
    {
        private readonly SteamToolService steamService = SteamToolCore.Instance.Get<SteamToolService>();

        public override string Name
        {
            get { return Properties.Resources.UserFastChange; }
            protected set { throw new NotImplementedException(); }
        }

        public SwitchSteamAccountPage()
        {
            SteamUsers = steamService.GetAllUser();
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
    }
}

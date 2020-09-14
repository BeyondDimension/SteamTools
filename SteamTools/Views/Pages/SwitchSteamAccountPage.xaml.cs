using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SteamTool.Core.Model;

namespace SteamTools.Views.Pages
{
    /// <summary>
    /// SwitchSteamAccount.xaml 的交互逻辑
    /// </summary>
    public partial class SwitchSteamAccountPage : UserControl
    {
        public SwitchSteamAccountPage()
        {
            InitializeComponent();
        }

        private void SwitchButton_OnClick(object sender, RoutedEventArgs e)
        {
            //if (UserList.SelectedValue == null)
            //{
            //    MessageBox.Show("请选择帐号");
            //    return;
            //}

            //SteamUser user = UserList.SelectedValue as SteamUser;

            //steamService.SetCurrentUser(user.AccountName);
            //steamService.KillSteamProcess();
            //steamService.StartSteam();

        }
    }
}

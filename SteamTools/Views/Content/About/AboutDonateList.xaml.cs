using SteamTool.Core;
using SteamTool.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace SteamTools.Views.Content
{
    /// <summary>
    /// AboutDonate.xaml 的交互逻辑
    /// </summary>
    public partial class AboutDonateList : UserControl
    {
        private readonly HttpServices httpServices = SteamToolCore.Instance.Get<HttpServices>();

        public AboutDonateList()
        {
            InitializeComponent();
        }

        //private void UserControl_Loaded(object sender, RoutedEventArgs e)
        //{
        //    httpServices.Get(Const.REWARDMELIST_URL).ContinueWith(s =>
        //    {
        //        web.Dispatcher.Invoke(() =>
        //        {
        //            web.NavigateToString(s.Result);
        //        });
        //    });
        //}
    }
}

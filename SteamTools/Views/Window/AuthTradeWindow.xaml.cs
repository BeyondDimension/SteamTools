using SteamTool.Model;
using SteamTools.Models;
using SteamTools.Services;
using SteamTools.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SteamTools
{
    public partial class AuthTradeWindow
    {
        private AuthTradeWindowViewModel ViewModel => this.DataContext as AuthTradeWindowViewModel;

        public AuthTradeWindow()
        {
            InitializeComponent();
            App.Current.MainWindow.Closed += (sender, args) => this.Close();

            //web.Navigate(new Uri(Const.STEAM_LOGIN_URL));
        }

        //public void SuppressScriptErrors(WebBrowser webBrowser, bool Hide)
        //{
        //    FieldInfo fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
        //    if (fiComWebBrowser == null) return;

        //    object objComWebBrowser = fiComWebBrowser.GetValue(webBrowser);
        //    if (objComWebBrowser == null) return;

        //    objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { Hide });
        //}

        //private void web_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        //{
        //    var url = web.Source.AbsoluteUri;
        //    SuppressScriptErrors(web, true);
        //    if (url.StartsWith("https://steamcommunity.com/id/") == true)
        //    {
        //        ViewModel.ExtractSteamCookies();
        //    }
        //}

        //private void web_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        //{
        //    var url = web.Source.AbsoluteUri;
        //    if (url.StartsWith("https://steamcommunity.com/login/home/?goto=my/profile"))
        //    {
        //        web.InvokeScript("eval", new object[] { "function V_SetCookie() {} function V_GetCookie() {}" });
        //        web.InvokeScript("LoginUsingSteamClient", new object[] { "https://steamcommunity.com/" });
        //    }
        //}
    }
}

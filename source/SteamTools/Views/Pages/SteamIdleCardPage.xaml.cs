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
using MetroRadiance.UI;

namespace SteamTools.Views.Pages
{
    /// <summary>
    /// SteamIdleCardPage.xaml 的交互逻辑
    /// </summary>
    public partial class SteamIdleCardPage : UserControl
    {
        public SteamIdleCardPage()
        {
            InitializeComponent();
        }
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

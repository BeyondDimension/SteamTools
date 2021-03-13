using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.Options;
using System.Application.Models;
using System.Windows;
using System.Windows.Shell;
using WpfApplication = System.Windows.Application;

namespace System.Application.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class WpfApp : WpfApplication
    {
        void JumpList_JumpItemsRejected(object sender, JumpItemsRejectedEventArgs e)
        {
        }

        void JumpList_JumpItemsRemovedByUser(object sender, JumpItemsRemovedEventArgs e)
        {
        }
    }
}
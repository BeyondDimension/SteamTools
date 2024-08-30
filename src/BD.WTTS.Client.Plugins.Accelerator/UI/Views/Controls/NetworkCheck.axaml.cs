using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Org.BouncyCastle.Asn1.Utilities;

namespace BD.WTTS.UI.Views.Pages;

public partial class NetworkCheck : UserControl
{
    public NetworkCheck()
    {
        InitializeComponent();

        STUNServer.SelectedItem = "stun.syncthing.net";
        this.dummy.SelectedItem = ProxySettings.ProxyMasterDns.Value;
        this.dummy.Text = ProxySettings.ProxyMasterDns.Value;
    }
}
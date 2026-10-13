using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.Views.Pages;

public partial class AcceleratorPage : PageBase<AcceleratorPageViewModel>
{
    public AcceleratorPage()
    {
        if (OperatingSystem.IsLinux())
        {
            try { System.IO.File.AppendAllText("/tmp/wt-ui-debug.log", $"CTOR-ENTER {System.DateTime.Now:O}\n"); } catch { }
        }
        InitializeComponent();
        this.SetViewModel<AcceleratorPageViewModel>(true);
        if (OperatingSystem.IsLinux())
        {
            try { System.IO.File.AppendAllText("/tmp/wt-ui-debug.log", $"CTOR-AFTER-VM {System.DateTime.Now:O}\n"); } catch { }
            // NixOS：加速模式由系统配置（programs.watt-toolkit.proxyMode）统一管理，
            // UI 上全部禁用，避免与系统配置冲突。
            bool nixosMode = false;
            try
            {
                nixosMode = BD.WTTS.Services.Implementation.LinuxPlatformServiceImpl.IsNixOS;
            }
            catch (Exception __ex)
            {
                try { System.IO.File.AppendAllText("/tmp/wt-ui-debug.log", "IsNixOS EXCEPTION: " + __ex + "\n"); } catch { }
            }
            try
            {
                System.IO.File.AppendAllText("/tmp/wt-ui-debug.log",
                    $"IsNixOS={nixosMode} ST={System.Environment.StackTrace}\n");
            }
            catch { }
            if (nixosMode)
            {
                try
                {
                    ProxyModeTabStrip.IsEnabled = false;
                    foreach (var item in ProxyModeTabStrip.Items)
                    {
                        if (item is Avalonia.Controls.Control c) c.IsEnabled = false;
                    }
                    Avalonia.Controls.ToolTip.SetTip(ProxyModeTabStrip,
                        "NixOS 下加速模式请在系统配置（programs.watt-toolkit.proxyMode）中统一修改：hosts 或 dns");
                    // 只读显示系统配置的代理模式（/etc/watt-toolkit/proxy-mode 由 NixOS 声明式生成）
                    var mode = System.IO.File.ReadAllText("/etc/watt-toolkit/proxy-mode").Trim();
                    var target = ProxySettings.ProxyModes.FirstOrDefault(pm =>
                        (mode == "dns" && pm == BD.WTTS.Enums.ProxyMode.DNS) ||
                        (mode == "hosts" && pm == BD.WTTS.Enums.ProxyMode.Hosts));
                    if (!EqualityComparer<BD.WTTS.Enums.ProxyMode>.Default.Equals(target, default))
                    {
                        ProxyModeTabStrip.SelectedItem = target;
                    }
                    System.IO.File.AppendAllText("/tmp/wt-ui-debug.log", $"DISABLED ok items={ProxyModeTabStrip.Items.Count} mode={mode}\n");
                }
                catch (Exception __ex2)
                {
                    try { System.IO.File.AppendAllText("/tmp/wt-ui-debug.log", "DISABLE EXCEPTION: " + __ex2 + "\n"); } catch { }
                }
            }
            else
            {
                try { ProxyModeTabStrip.IsEnabled = !ProxyService.Current.ProxyStatus; } catch { }
            }
        }
    }
}

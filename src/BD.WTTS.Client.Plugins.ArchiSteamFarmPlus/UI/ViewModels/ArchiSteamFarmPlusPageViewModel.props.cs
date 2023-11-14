using ArchiSteamFarm.Steam;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class ArchiSteamFarmPlusPageViewModel
{
    /// <summary>
    /// 启动或停止 ASF 文本展示
    /// </summary>
    [Reactive]
    public string? RunOrStopText { get; set; }

    /// <summary>
    /// ASF bots
    /// </summary>
    private readonly ReadOnlyObservableCollection<Bot> _SteamBots;

    /// <summary>
    /// ASF bots
    /// </summary>
    public ReadOnlyObservableCollection<Bot> SteamBots => _SteamBots;

    /// <summary>
    /// 通过配置文件导入 ASF Bot
    /// </summary>
    public ICommand SelectBotFiles { get; }

    /// <summary>
    /// 通过配置文件导入 ASF 全局配置
    /// </summary>
    public ICommand SelectGlobalFiles { get; }

    /// <summary>
    /// 选择 ASF 可执行文件路径
    /// </summary>
    public ICommand SelectASFExePath { get; }

    /// <summary>
    /// 启动或停止
    /// </summary>
    public ICommand RunOrStop { get; }

    /// <summary>
    /// 添加 ASF Bot
    /// </summary>
    public ICommand AddBot { get; }

    /// <summary>
    /// 刷新 ASF Bots
    /// </summary>
    public ICommand RefreshBots { get; }

    /// <summary>
    /// 打开 ASF WEBUI 控制台
    /// </summary>
    public ICommand OpenWebUIConsole { get; }

    /// <summary>
    /// 打开 ASF 文件夹
    /// </summary>
    public ICommand OpenASFFolder { get; }

    /// <summary>
    /// 设置 ASF 加密密钥
    /// </summary>
    public ICommand SetEncryptionKey { get; }

    /// <summary>
    /// 打开 ASF 相关网址
    /// </summary>
    public ICommand OpenASFBrowser { get; }

    public ICommand ShellMessageInput { get; }

    private bool _IsRedeemKeyDialogOpen;

    public bool IsRedeemKeyDialogOpen
    {
        get => _IsRedeemKeyDialogOpen;
        set => this.RaiseAndSetIfChanged(ref _IsRedeemKeyDialogOpen, value);
    }
}

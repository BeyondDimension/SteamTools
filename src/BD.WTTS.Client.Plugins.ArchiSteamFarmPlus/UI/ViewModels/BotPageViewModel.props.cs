namespace BD.WTTS.UI.ViewModels;

partial class BotPageViewModel
{
    /// <summary>
    /// ASF bots
    /// </summary>
    private readonly ReadOnlyObservableCollection<BotViewModel> _SteamBots;

    /// <summary>
    /// ASF bots
    /// </summary>
    public ReadOnlyObservableCollection<BotViewModel> SteamBots => _SteamBots;

    /// <summary>
    /// 添加 ASF Bot
    /// </summary>
    public ICommand AddBot { get; }

    /// <summary>
    /// 刷新 ASF Bots
    /// </summary>
    public ICommand RefreshBots { get; }

    /// <summary>
    /// 通过配置文件导入 ASF Bot
    /// </summary>
    public ICommand SelectBotFiles { get; }

    /// <summary>
    /// 暂停或恢复 Bot
    /// </summary>
    public ICommand PauseOrResumeBotFarming { get; }

    /// <summary>
    /// 启用或禁用 Bot
    /// </summary>
    public ICommand EnableOrDisableBot { get; }

    /// <summary>
    /// 打开 Bot 配置页面
    /// </summary>
    public ICommand GoToBotSettings { get; }

    /// <summary>
    /// 删除 Bot
    /// </summary>
    public ICommand DeleteBot { get; }

    /// <summary>
    /// 编辑 Bot Json 文件
    /// </summary>
    public ICommand EditBotFile { get; }
}

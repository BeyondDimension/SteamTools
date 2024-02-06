namespace BD.WTTS.Models;

public sealed class BotViewModel : ReactiveObject
{
    public Bot Bot { get; set; }

    public BotViewModel(Bot bot)
    {
        Bot = bot;
        BotStatus = GetStatus();
    }

    public bool StatusVisible
    {
        get
        {
            if (BotStatus == BotStatus.Disabled || BotStatus == BotStatus.OffLine)
                return false;
            return true;
        }
    }

    private BotStatus _botStatus;

    public BotStatus BotStatus
    {
        get
        {
            return _botStatus;
        }

        set
        {
            this.RaiseAndSetIfChanged(ref _botStatus, value);
            BotStatusText = GetStatusText();
        }
    }

    private string _botStatusText = string.Empty;

    public string BotStatusText
    {
        get { return _botStatusText; }
        set { this.RaiseAndSetIfChanged(ref _botStatusText, value); }
    }

    public string Tags
    {
        get => $"昵称：{Bot.Nickname}\r\n钱包余额：{Bot.WalletCurrency} {Math.Round((decimal)Bot.WalletBalance / 100)}\r\n挂卡游戏：{string.Join(',', Bot.CardsFarmer?.CurrentGamesFarming?.Select(s => s.GameName) ?? Enumerable.Empty<string>())}";
    }

    // https://github.com/JustArchiNET/ASF-ui/blob/9105e9becd86d610418532b73852ae22e19b3587/src/models/Bot.js#L6

    /// <summary>
    /// 获取 <see cref="Bot"/> 状态
    /// </summary>
    /// <param name="bot"></param>
    /// <returns></returns>
    public BotStatus GetStatus()
    {
        if (!Bot.KeepRunning) return BotStatus.Disabled;
        if (!Bot.IsConnectedAndLoggedOn) return BotStatus.OffLine;
        if (Bot.CardsFarmer.Paused || Bot.CardsFarmer.TimeRemaining == default) return BotStatus.Online;
        if ((Bot.CardsFarmer?.CurrentGamesFarming?.Count ?? 0) <= 0) return BotStatus.Online;
        return BotStatus.Farming;
    }

    /// <summary>
    /// 获取 <see cref="BotStatus"/> 展示文本
    /// </summary>
    /// <param name="bot"></param>
    /// <returns></returns>
    public string GetStatusText()
    {
        var statusText = BotStatus.GetDescription();
        //挂卡详情不展示在状态中，名称太长
        //if (BotStatus == BotStatus.Farming && Bot.CardsFarmer?.CurrentGamesFarming?.Count == 1) return $"{statusText} - {Bot.CardsFarmer.CurrentGamesFarming.First().GameName}";
        //if (BotStatus == BotStatus.Farming && Bot.CardsFarmer?.CurrentGamesFarming?.Count > 1) return $"{statusText} - 多个游戏";
        if (BotStatus == BotStatus.Disabled && Bot.RequiredInput != ASF.EUserInputType.None) return $"等待输入";  // 等待用户控制台输入
        return statusText!;
    }

    public static implicit operator BotViewModel(Bot bot) => new(bot);
}

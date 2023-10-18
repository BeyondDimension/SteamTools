using BD.SteamClient.Models;
using BD.SteamClient.Models.Idle;
using BD.SteamClient.Services;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class IdleCardPageViewModel
{
    public ICommand IdleRunStartOrStop { get; }

    public ICommand IdleManualRunNext { get; }

    [Reactive]
    public bool RunLoaingState { get; set; }

    [Reactive]
    public bool RunState { get; set; }

    [Reactive]
    public string? RuningCountTxt { get; set; }

    [Reactive]
    public ObservableCollection<SteamApp> IdleGameList { get; set; } = new();

    /// <summary>
    /// 当前挂卡游戏 
    /// </summary>
    [Reactive]
    public SteamApp? CurrentIdle { get; set; }

    /// <summary>
    /// 挂卡规则
    /// </summary>
    [Reactive]
    public IdleRule IdleRule { get; set; }

    /// <summary>
    /// 挂卡顺序
    /// </summary>
    [Reactive]
    public IdleSequentital IdleSequentital { get; set; }

    /// <summary>
    /// 自动运行下一个游戏
    /// </summary>
    [Reactive]
    public bool IsAutoNextOn { get; set; }

    /// <summary>
    /// 自动运行下一个游戏文本展示
    /// </summary>
    [Reactive]
    public string? IsAutoNextOnTxt { get; set; }

    #region 魔改

    /// <summary>
    /// 最少游戏时间 hours
    /// </summary>
    [Reactive]
    private double MinRunTime { get; set; } = 2;

    /// <summary>
    /// 自动切换游戏时间间隔 ms
    /// </summary>
    [Reactive]
    private double SwitchTime { get; set; } = 500;
    #endregion
}

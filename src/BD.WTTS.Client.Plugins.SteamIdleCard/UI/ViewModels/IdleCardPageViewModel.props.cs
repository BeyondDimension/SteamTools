using BD.SteamClient.Models;
using BD.SteamClient.Models.Idle;
using BD.SteamClient.Services;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class IdleCardPageViewModel : TabItemViewModel
{
    public override string Name => Strings.SteamIdleCard;

    /// <summary>
    /// 有限运行此游戏
    /// </summary>
    public ICommand PriorityRunIdle { get; }

    public ICommand ToggleBlacklistIdle { get; }

    public ICommand LoginSteamCommand { get; }

    public ICommand IdleRunStartOrStop { get; }

    public ICommand IdleManualRunNext { get; }

    //public ICommand ChangeState { get; }

    public ICommand NavAppToSteamViewCommand { get; }

    public ICommand OpenLinkUrlCommand { get; }

    //[Reactive]
    //public SteamUser CurrentUser { get; set; }

    [Reactive]
    public bool IsLogin { get; set; }

    [Reactive]
    public bool IsCheckingLogin { get; set; } = true;

    [Reactive]
    public bool IsLoadingBadges { get; set; }

    public bool IsPageLoading => IsCheckingLogin || IsLoadingBadges;

    public bool CanStartIdle => !IsPageLoading && ViewState != 0;

    public string PageLoadingText => IsCheckingLogin ? "正在检测 Steam 登录状态…" : "正在加载徽章数据…";

    [Reactive]
    public bool RunLoaingState { get; set; }

    [Reactive]
    public bool RunState { get; set; }

    [Reactive]
    public int RuningCount { get; set; }

    /// <summary>
    /// 用户等级信息
    /// </summary>
    [Reactive]
    public UserIdleInfo? UserIdleInfo { get; set; }

    /// <summary>
    /// 正在挂卡游戏
    /// </summary>
    [Reactive]
    public ObservableCollection<IdleApp> IdleGameList { get; set; } = new();

    /// <summary>
    /// 用户徽章和卡片数据
    /// </summary>
    [Reactive]
    public ObservableCollection<Badge> Badges { get; set; } = new();

    [Reactive]
    public TimeSpan IdleTime { get; set; }

    [Reactive]
    public int DroppedCardsCount { get; set; }

    [Reactive]
    public int TotalCardsRemaining { get; set; }

    [Reactive]
    public int ViewState { get; set; } = 1;

    [Reactive]
    public IdleSteamLoginPageViewModel? LoginViewModel { get; set; }

    public int DropCardsCount => Math.Max(DroppedCardsCount - TotalCardsRemaining, 0);

    [Reactive]
    public decimal TotalCardsAvgPrice { get; set; }

    [Reactive]
    public bool HasPrivateGameExcluded { get; set; }

    [Reactive]
    public int ExcludedPrivateGameCount { get; set; }

    [Reactive]
    public int ExcludedBlacklistGameCount { get; set; }

    [Reactive]
    public string PrivateGameExcludeTip { get; set; } = string.Empty;

    /// <summary>
    /// 当前挂卡游戏 
    /// </summary>
    [Reactive]
    public IdleApp? CurrentIdle { get; set; }
}

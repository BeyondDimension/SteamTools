using BD.SteamClient.Models;
using BD.SteamClient.Models.Idle;

namespace BD.WTTS.Models;

public class IdleApp : ReactiveObject
{
    public SteamApp App { get; }

    public Badge Badge { get; }

    public uint AppId
    {
        get
        {
            return App.AppId;
        }

        set
        {
            App.AppId = value;
            this.RaisePropertyChanged();
        }
    }

    public string? AppName
    {
        get
        {
            return App.Name;
        }

        set
        {
            App.Name = value;
            this.RaisePropertyChanged();
        }
    }

    public string? Tags
    {
        get
        {
            return $"剩余掉落卡片:{Badge.CardsRemaining} | 游玩时间:{Badge.HoursPlayed}";
        }
    }

    public IdleApp(Badge badge)
    {
        Badge = badge;
        App = new SteamApp(badge.AppId) { Name = badge.AppName };
    }
}

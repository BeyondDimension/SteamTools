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
            return Strings.SteamIdle_IdleAppTags_.Format(Badge.HoursPlayed, Badge.CardsRemaining, Badge.RegularAvgPrice.ToString("0.00"));
        }
    }

    public IdleApp(Badge badge)
    {
        Badge = badge;
        if (SteamConnectService.Current.RuningSteamApps.TryGetValue(badge.AppId, out var app))
        {
            App = app;
        }
        else
        {
            App = new SteamApp(badge.AppId) { Name = badge.AppName };
        }
    }
}

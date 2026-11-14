using BD.SteamClient.Models;
using BD.SteamClient.Models.Idle;

namespace BD.WTTS.Models;

public class IdleApp : ReactiveObject
{
    private bool isBlacklisted;
    private bool isPrivateGame;

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

    public bool IsBlacklisted
    {
        get => isBlacklisted;
        set
        {
            if (isBlacklisted != value)
            {
                this.RaiseAndSetIfChanged(ref isBlacklisted, value);
                this.RaisePropertyChanged(nameof(IsExcludedFromIdle));
                this.RaisePropertyChanged(nameof(ExcludedReasonText));
            }
        }
    }

    public bool IsPrivateGame
    {
        get => isPrivateGame;
        set
        {
            if (isPrivateGame != value)
            {
                this.RaiseAndSetIfChanged(ref isPrivateGame, value);
                this.RaisePropertyChanged(nameof(IsExcludedFromIdle));
                this.RaisePropertyChanged(nameof(ExcludedReasonText));
            }
        }
    }

    public bool IsExcludedFromIdle => IsBlacklisted || IsPrivateGame;

    public string ExcludedReasonText
    {
        get
        {
            if (IsPrivateGame && IsBlacklisted)
                return Strings.L10nAuthIdle_Text104;
            if (IsPrivateGame)
                return Strings.L10nAuthIdle_Text105;
            if (IsBlacklisted)
                return Strings.L10nAuthIdle_Text106;
            return string.Empty;
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

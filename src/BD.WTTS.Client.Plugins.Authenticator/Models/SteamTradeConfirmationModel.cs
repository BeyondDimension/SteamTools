using AppResources = BD.WTTS.Client.Resources.Strings;
using WinAuth;

namespace BD.WTTS.Models;

public class SteamTradeConfirmationModel : ReactiveObject
{
    public int Type { get; set; }

    public string? TypeName { get; set; }

    public string Id { get; set; }

    public string? CreatorId { get; set; }

    public string Nonce { get; set; }

    long CreationTime { get; set; }

    public string? Cancel { get; set; }

    public string? Accept { get; set; }

    public string? Icon { get; set; }

    public bool Multi { get; set; }

    public string? Headline { get; set; }

    string[]? Summary { get; set; }

    public string? Warn { get; set; }

    public string? SelfIcon { get; set; }

    public bool IsTrade => Type == 2;

    public string? SendSummary => Summary.Any_Nullable() ? Summary[0] : null;

    public string? ReceiveSummary => Summary?.Length >= 2 ? Summary[1] : null;

    public string TypeNameText => TypeName + $"({Headline})";

    public DateTime CreateTime => DateTimeOffset.FromUnixTimeSeconds(CreationTime).ToLocalTime().DateTime;

    public bool ReceiveNoItems => (ReceiveSummary?.Contains("You will receive nothing") == true ||
                                   ReceiveSummary?.Contains("不会收到任何物品") == true) && !ReceiveItemImageUrls.Any();

    [Reactive]
    public ObservableCollection<string> SendItemImageUrls { get; set; }

    [Reactive]
    public ObservableCollection<string> ReceiveItemImageUrls { get; set; }

    [Reactive]
    public bool IsSelected { get; set; }

    WinAuth.SteamClient _steamClient;

    //public SteamTradeConfirmationModel(SteamAuthenticator steamAuthenticator)
    //{
    //    SendItemImageUrls = new();
    //    ReceiveItemImageUrls = new();
    //    _steamClient = steamAuthenticator.GetClient(ResourceService.GetCurrentCultureSteamLanguageName());
    //}

    public SteamTradeConfirmationModel(SteamAuthenticator steamAuthenticator, SteamMobileTradeConf conf)
    {
        _steamClient = steamAuthenticator.GetClient(ResourceService.GetCurrentCultureSteamLanguageName());
        Type = conf.Type;
        TypeName = conf.TypeName;
        Id = conf.Id;
        CreatorId = conf.CreatorId;
        Nonce = conf.Nonce;
        CreationTime = conf.CreationTime;
        Cancel = conf.Cancel;
        Accept = conf.Accept;
        Icon = conf.Icon;
        Multi = conf.Multi;
        Headline = conf.Headline;
        Summary = conf.Summary;
        if (conf.Warn != null)
            foreach (var warn in conf.Warn)
            {
                Warn += $"{warn}\r\n";
            }
        SendItemImageUrls = new();
        ReceiveItemImageUrls = new();

        if (IsTrade)
            GetConfirmationDeatil();
    }

    private async void GetConfirmationDeatil()
    {
        var imageUrls = await _steamClient.GetConfirmationItemImageUrls(Id);
        SelfIcon = _steamClient.SteamUserImageUrl;
        ReceiveItemImageUrls.Clear();
        foreach (var item in imageUrls.receiveItems)
        {
            ReceiveItemImageUrls.Add(item);
        }

        SendItemImageUrls.Clear();
        foreach (var item in imageUrls.sendItems)
        {
            SendItemImageUrls.Add(item);
        }
    }

    //void ExceptionHandling(Exception exception, bool allowRetry = true)
    //{
    //    //可能是启用了家庭监护功能
    //    if (exception is WinAuthUnauthorisedSteamRequestException unauthorisedSteamRequestException)
    //    {
    //        Toast.Show(ToastIcon.Error, Strings.LocalAuth_AuthTrade_GetError);
    //        return;
    //    }
    //    exception.LogAndShowT();
    //}
}
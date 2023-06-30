using WinAuth;

namespace BD.WTTS.Models;

public class SteamTradeConfirmationModel : ReactiveObject
{
    public int Type { get; set; }

    public string TypeName { get; set; } = string.Empty;
    
    public string Id { get; set; } = string.Empty;

    public string CreatorId { get; set; } = string.Empty;

    public string Nonce { get; set; } = string.Empty;
    
    long CreationTime { get; set; }

    public string Cancel { get; set; } = string.Empty;

    public string Accept { get; set; } = string.Empty;
    
    public string Icon { get; set; } = string.Empty;
    
    public bool Multi { get; set; }
    
    public string Headline { get; set; } = string.Empty;
    
    string[]? Summary { get; set; }
    
    public string? Warn { get; set; }

    public string? SelfIcon { get; set; }
    
    public string SendSummary => Summary![0];

    public string ReceiveSummary => Summary![1];

    public string TypeNameText => TypeName + $"({Headline})";

    public DateTime CreateTime => DateTimeOffset.FromUnixTimeSeconds(CreationTime).ToLocalTime().DateTime;

    public bool ReceiveNoItems => ReceiveSummary.Contains("不会收到") && ReceiveItemImageUrls.Count < 1;

    [Reactive]
    public ObservableCollection<string> SendItemImageUrls { get; set; }
    
    [Reactive]
    public ObservableCollection<string> ReceiveItemImageUrls { get; set; }

    WinAuth.SteamClient _steamClient;

    public SteamTradeConfirmationModel(SteamAuthenticator steamAuthenticator)
    {
        SendItemImageUrls = new();
        ReceiveItemImageUrls = new();
        _steamClient = steamAuthenticator.GetClient();
    }

    public SteamTradeConfirmationModel(SteamAuthenticator steamAuthenticator, SteamMobileTradeConf conf)
    {
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
        Warn = conf.Warn;

        SendItemImageUrls = new();
        ReceiveItemImageUrls = new();
        _steamClient = steamAuthenticator.GetClient();
        GetItemImages();
    }

    private void GetItemImages()
    {
        var imageUrls = _steamClient.GetConfirmationItemImageUrls(Id);
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
}
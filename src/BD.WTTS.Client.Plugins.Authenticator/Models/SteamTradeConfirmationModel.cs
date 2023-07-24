using AppResources = BD.WTTS.Client.Resources.Strings;
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

    [Reactive]
    public bool IsSelected { get; set; }

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
        if (conf.Warn != null)
            foreach (var warn in conf.Warn)
            {
                Warn += $"{warn}\r\n";
            }
        SendItemImageUrls = new();
        ReceiveItemImageUrls = new();
        _steamClient = steamAuthenticator.GetClient();
        Initialize();
    }

    async void Initialize()
    {
        await GetItemImages();
    }

    private async Task GetItemImages()
    {
        var imageUrls =
            await RunTaskAndExceptionHandlingAsync(
                new Task<(string[] receiveItems, string[] sendItems)>(() =>
                    _steamClient.GetConfirmationItemImageUrls(Id)));
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
    
    async Task<T?> RunTaskAndExceptionHandlingAsync<T>(Task<T> task)
    {
        try
        {
            task.Start();
            var result = await task;
            return result;
        }
        catch (Exception e)
        {
            ExceptionHandling(e);
            return default;
        }
    }

    void ExceptionHandling(Exception exception, bool allowRetry = true)
    {
        //可能是启用了家庭监护功能
        if (exception is WinAuthUnauthorisedSteamRequestException unauthorisedSteamRequestException)
        {
            Toast.Show(ToastIcon.Error, Strings.LocalAuth_AuthTrade_GetError);
            return;
        }
        exception.LogAndShowT();
    }
}
using WinAuth;

namespace BD.WTTS.Models;

public partial class SteamTradeConfirmationModel : ReactiveObject
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
    public partial ObservableCollection<string> SendItemImageUrls { get; set; }

    [Reactive]
    public partial ObservableCollection<string> ReceiveItemImageUrls { get; set; }

    [Reactive]
    public partial bool IsSelected { get; set; }

    [Reactive]
    public partial bool HasDetailLoadError { get; private set; }

    public SteamTradeConfirmationModel(SteamMobileTradeConf conf)
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
    }

    public async Task LoadDetailsAsync(WinAuth.SteamClient steamClient, CancellationToken cancellationToken)
    {
        if (!IsTrade || cancellationToken.IsCancellationRequested)
            return;

        try
        {
            var imageUrls = await steamClient.GetConfirmationItemImageUrls(Id).WaitAsync(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            SelfIcon = steamClient.SteamUserImageUrl;
            ReceiveItemImageUrls = new(imageUrls.receiveItems);
            SendItemImageUrls = new(imageUrls.sendItems);
            HasDetailLoadError = false;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch
        {
            if (!cancellationToken.IsCancellationRequested)
                HasDetailLoadError = true;
        }
    }
}

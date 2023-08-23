using BD.WTTS.Client.Resources;
using static BD.WTTS.Services.IMicroServiceClient;

namespace BD.WTTS.UI.ViewModels;

public sealed class HomePageViewModel : TabItemViewModel
{
    public override string Name => Strings.Welcome;

    public override string IconKey => "avares://BD.WTTS.Client.Avalonia/UI/Assets/Icons/home.ico";

    [Reactive]
    public ObservableCollection<ArticleItemDTO> Articles { get; set; }

    [Reactive]
    public ObservableCollection<AdvertisementDTO> NavigationBanners { get; set; }

    public HomePageViewModel()
    {
        Articles = new ObservableCollection<ArticleItemDTO>();
        NavigationBanners = new ObservableCollection<AdvertisementDTO>();

        GetServerContent();
    }

    public async void GetServerContent()
    {
        var result = await Instance.Article.Order(null, ArticleOrderBy.DateTime);
        if (result.IsSuccess && result.Content != null)
        {
            Articles.Clear();
            Articles.Add(result.Content.DataSource);
        }

        var result2 = await Instance.Advertisement.All(AdvertisementType.DeskTopHomeBanner);
        if (result2.IsSuccess && result2.Content != null)
        {
            NavigationBanners.Clear();
            NavigationBanners.Add(result2.Content);
        }
    }
}

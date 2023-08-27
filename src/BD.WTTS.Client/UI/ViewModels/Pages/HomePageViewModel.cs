using BD.WTTS.Client.Resources;
using static BD.WTTS.Services.IMicroServiceClient;

namespace BD.WTTS.UI.ViewModels;

public sealed class HomePageViewModel : TabItemViewModel
{
    public const string OfficialWebsite_Article = Constants.Urls.OfficialWebsite_Article;
    public const string OfficialWebsite_Article_Detail_ = Constants.Urls.OfficialWebsite_Article_Detail_;
    public const string WattGameUrl = Constants.Urls.WattGame;
    public const string WattGame_Goods_Detail_ = Constants.Urls.WattGame_Goods_Detail_;

    public override string Name => Strings.Welcome;

    public override string IconKey => "avares://BD.WTTS.Client.Avalonia/UI/Assets/Icons/home.ico";

    [Reactive]
    public ObservableCollection<ArticleItemDTO> Articles { get; set; }

    [Reactive]
    public ObservableCollection<AdvertisementDTO> NavigationBanners { get; set; }

    [Reactive]
    public ObservableCollection<ShopRecommendGoodItem> Shops { get; set; }

    public ICommand NavgationToMenuPageCommand { get; }

    public HomePageViewModel()
    {
        Articles = new ObservableCollection<ArticleItemDTO>();
        NavigationBanners = new ObservableCollection<AdvertisementDTO>();
        Shops = new ObservableCollection<ShopRecommendGoodItem>();

        NavgationToMenuPageCommand = ReactiveCommand.Create<TabItemViewModel>(NavgationToMenuPage);

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

        var result2 = await Instance.Shop.RecommendGoods();
        if (result2.IsSuccess && result2.Content != null)
        {
            Shops.Clear();
            Shops.Add(result2.Content);
        }

        var result3 = await Instance.Advertisement.All(AdvertisementType.DeskTopHomeBanner);
        if (result3.IsSuccess && result3.Content != null)
        {
            NavigationBanners.Clear();
            NavigationBanners.Add(result3.Content);
        }
    }

    public void NavgationToMenuPage(TabItemViewModel tabItem)
    {
        if (tabItem.PageType != null)
            INavigationService.Instance.Navigate(tabItem.PageType, NavigationTransitionEffect.FromBottom);
    }
}

using BD.WTTS.Client.Resources;
using DynamicData;
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
    //public ObservableCollection<List<ShopRecommendGoodItem>> Shops { get; set; }
    public ObservableCollection<ShopRecommendGoodItem> Shops { get; set; }

    [Reactive]
    public bool IsArticlesLoading { get; set; } = true;

    [Reactive]
    public bool IsShopsLoading { get; set; } = true;

    [Reactive]
    public bool IsBannersLoading { get; set; } = true;

    bool isLoading;

    public ICommand NavgationToMenuPageCommand { get; }

    public HomePageViewModel()
    {
        Articles = new ObservableCollection<ArticleItemDTO>();
        NavigationBanners = new ObservableCollection<AdvertisementDTO>();
        //Shops = new ObservableCollection<List<ShopRecommendGoodItem>>();
        Shops = new ObservableCollection<ShopRecommendGoodItem>();

        NavgationToMenuPageCommand = ReactiveCommand.Create<TabItemViewModel>(NavgationToMenuPage);

        //GetServerContent();
    }

    //public override void Activation()
    //{
    //    base.Activation();
    //    GetServerContent();
    //}

    public async Task GetServerContent()
    {
        if (isLoading)
            return;

        isLoading = true;
        IsArticlesLoading = IsShopsLoading = IsBannersLoading = true;
        try
        {
            await Task.WhenAll(LoadArticles(), LoadShops(), LoadBanners());
        }
        finally
        {
            isLoading = false;
        }
    }

    async Task LoadArticles()
    {
        try
        {
            var result = await Instance.Article.Order(null, ArticleOrderBy.DateTime);
            if (result.IsSuccess && result.Content != null)
            {
                Articles = new ObservableCollection<ArticleItemDTO>(result.Content.DataSource.Take(3));
            }
        }
        catch (Exception ex) { ex.LogAndShowT(); }
        finally { IsArticlesLoading = false; }
    }

    async Task LoadShops()
    {
        try
        {
            // var result2 = await Instance.Shop.RecommendGoods();
            // if (result2.IsSuccess && result2.Content != null)
            // {
            //     Shops.Clear();
            //     Shops.Add(result2.Content);
            // }
            await Task.CompletedTask;
        }
        catch (Exception ex) { ex.LogAndShowT(); }
        finally { IsShopsLoading = false; }
    }

    async Task LoadBanners()
    {
        try
        {
            var result3 = await Instance.Advertisement.All(AdvertisementType.DeskTopHomeBanner);
            if (result3.IsSuccess && result3.Content != null)
            {
                NavigationBanners.Clear();
                NavigationBanners.Add(result3.Content);
            }
        }
        catch (Exception ex) { ex.LogAndShowT(); }
        finally { IsBannersLoading = false; }
    }

    public void NavgationToMenuPage(TabItemViewModel tabItem)
    {
        INavigationService.Instance.Navigate(tabItem.PageType, NavigationTransitionEffect.FromBottom);
    }
}

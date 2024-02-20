using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;

namespace BD.WTTS.UI.Views.Controls;

public partial class GameDetail : UserControl
{
    public GameDetail()
    {
        InitializeComponent();

#if DEBUG
        var data = new XunYouGameViewModel()
        {
            Id = 26276,
            Model = new XunYouGame
            {
                Id = 26276,
                Name = "VALORANT瓦洛兰特-Riot|Epic",
                IconUrl = "http://update.xunyou.com/gamelogo2014/26276.ico",
                LogoUrl = "https://image.xunyou.com/images/client/7/logo/26276.png",
                LogoMD5 = "https://www.xunyou.com/client/7/cover/26276.json",
                PicUrl = "https://image.xunyou.com/images/client/7/cover/26276.png",
                PicMD5 = "https://www.xunyou.com/client/7/logo/26276.json",
            },
            IsAccelerated = true,
            LastAccelerateTime = DateTime.Now.AddHours(-12),
            SelectedArea = new XunYouGameArea { Id = 1 },
            SelectedServer = new XunYouGameServer { Id = 2 },
        };

        Design.SetDataContext(this, data);
        this.Content = data;
#endif
    }
}

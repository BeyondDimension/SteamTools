using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.ReactiveUI;
using System.CommandLine;

namespace BD.WTTS.UI.Views.Controls;

public partial class GameAppItem : UserControl
{
    public static readonly StyledProperty<ICommand?> ButtonCommandProperty =
        AvaloniaProperty.Register<GameAppItem, ICommand?>(nameof(Command), null, inherits: false, BindingMode.OneWay, null, null, enableDataValidation: true);

    public ICommand? ButtonCommand
    {
        get
        {
            return GetValue(ButtonCommandProperty);
        }

        set
        {
            SetValue(ButtonCommandProperty, value);
        }
    }

    public GameAppItem()
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
            IsAccelerated = false,
            LastAccelerateTime = DateTime.Now.AddHours(-12),
            SelectedArea = new XunYouGameArea { Id = 1 },
            SelectedServer = new XunYouGameServer { Id = 2 },
        };

        Design.SetDataContext(this, data);
#endif
    }
}

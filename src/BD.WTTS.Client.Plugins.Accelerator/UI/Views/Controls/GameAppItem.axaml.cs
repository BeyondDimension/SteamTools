using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using ReactiveUI.Avalonia.Reactive;
using System.CommandLine;

namespace BD.WTTS.UI.Views.Controls;

public partial class GameAppItem : UserControl
{
    private static readonly ImmutableSolidColorBrush HighPingBrush = new(0xFFFF4F58);
    private static readonly ImmutableSolidColorBrush NormalPingBrush = new(0xFF47A42B);

    public static IValueConverter PingBrushConverter { get; } = new FuncValueConverter<uint, IBrush>(
        static ping => ping > 120 ? HighPingBrush : NormalPingBrush);

    public static IValueConverter AccelerateOrStopTextConverter { get; } = new FuncValueConverter<bool, string>(
        static accelerated => accelerated ? Strings.GameAccelerator_Stop : Strings.GameAccelerator_Accelerate);

    public static readonly StyledProperty<ICommand?> ButtonCommandProperty =
        AvaloniaProperty.Register<GameAppItem, ICommand?>(nameof(ButtonCommand), null, inherits: false, BindingMode.OneWay, null, null, enableDataValidation: true);

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

using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Splat;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class AuthenticatorPageViewModel : TabItemViewModel
{
    public override string Name => "AuthenticatorPage";
    
    [Reactive]
    public ObservableCollection<AuthenticatorItemModel> Auths { get; set; }

    public AuthenticatorPageViewModel()
    {
        var temp = new AuthenticatorItemModel();
        Auths = new(new[] { temp, temp, temp, temp, temp, temp, temp, temp });
        // using HttpClient client = new HttpClient();
        // var imagebytes = client.GetByteArrayAsync(
        //         new Uri("https://www.toopic.cn/public/uploads/small/163420343597163420343525.jpg"))
        //     .GetAwaiter()
        //     .GetResult();
        // using (MemoryStream stream = new MemoryStream(imagebytes))
        // {
        //     BottomImage = new Bitmap(stream);
        // }
    }
}

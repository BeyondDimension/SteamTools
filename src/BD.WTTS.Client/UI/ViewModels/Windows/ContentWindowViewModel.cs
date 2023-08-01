namespace BD.WTTS.UI.ViewModels;

public class ContentWindowViewModel : WindowViewModel
{

    public double Width { get; init; }

    public double MaxWidth { get; init; }

    public double Height { get; init; }

    public double MaxHeight { get; init; }

    //[Reactive]
    //public bool IsShowSearchBox { get; set; }

    /// <summary>
    /// 可以是 Page 也可以是 ViewModel
    /// </summary>
    [Reactive]
    public object? PageViewModel { get; set; }

    //[Reactive]
    //public object? ActionContent { get; set; }

    public ContentWindowViewModel()
    {

    }
}
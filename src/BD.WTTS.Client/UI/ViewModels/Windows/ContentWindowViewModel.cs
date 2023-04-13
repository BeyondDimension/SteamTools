namespace BD.WTTS.UI.ViewModels;

public class ContentWindowViewModel : WindowViewModel
{
    [Reactive]
    public ViewModelBase? PageViewModel { get; set; }

    public ContentWindowViewModel()
    {

    }
}
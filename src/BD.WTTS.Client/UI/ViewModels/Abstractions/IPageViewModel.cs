namespace BD.WTTS.UI.ViewModels.Abstractions;

public interface IPageViewModel : IViewModelBase
{
    string Title { get; set; }

    void Initialize();
}
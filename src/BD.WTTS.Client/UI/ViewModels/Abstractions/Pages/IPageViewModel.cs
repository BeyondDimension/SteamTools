// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels.Abstractions;

public interface IPageViewModel : IViewModelBase
{
    string Title { get; set; }

    Task Initialize();
}
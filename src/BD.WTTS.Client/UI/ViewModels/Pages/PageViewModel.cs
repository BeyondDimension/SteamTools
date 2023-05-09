// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels;

public partial class PageViewModel : ViewModelBase, IPageViewModel
{
    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    protected string title = string.Empty;

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public virtual string Title
    {
        get => title;
        set => this.RaiseAndSetIfChanged(ref title, value);
    }

    public static string GetTitleByDisplayName(string displayName)
    {
        if (IApplication.IsDesktop())
        {
            const string s = $"{AssemblyInfo.Trademark} | {{0}}";
            return string.Format(s, displayName);
        }
        else
            return displayName;
    }

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public bool IsInitialized { get; protected set; }

    public virtual Task Initialize()
    {
        return Task.CompletedTask;
    }
}

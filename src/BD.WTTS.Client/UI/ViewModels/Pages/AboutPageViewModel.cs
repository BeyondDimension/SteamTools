using Strings = BD.WTTS.Client.Resources.Strings;

namespace BD.WTTS.UI.ViewModels;

[MP2Obj]
public sealed partial class AboutPageViewModel : TabItemViewModel
{
    public static AboutPageViewModel Instance => IViewModelManager.Instance.Get<AboutPageViewModel>();

    protected override bool IsSingleInstance => true;

    public override string Name => AboutMenuTabItemViewModel.DisplayName;

    public const string Title_2_ = "({0} Steam++)";

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public string AppName => AssemblyInfo.Trademark;

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public string FormerAppName => string.Format(Title_2_, Strings.About_FormerName);

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public string Copyright => AssemblyInfo.Copyright;

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public string VersionDisplay => $"{AssemblyInfo.Version} for {DeviceInfo2.OSName()} ({RuntimeInformation.ProcessArchitecture.ToString().ToLower()})";
}

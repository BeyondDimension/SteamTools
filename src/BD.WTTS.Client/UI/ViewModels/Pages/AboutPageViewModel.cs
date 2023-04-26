using Strings = BD.WTTS.Client.Resources.Strings;

namespace BD.WTTS.UI.ViewModels;

public partial class AboutPageViewModel
{
    public static AboutPageViewModel Instance { get; } = new();

    public const string Title_2_ = "({0} Steam++)";

    public string AppName => AssemblyInfo.Trademark;

    public string FormerAppName => string.Format(Title_2_, Strings.About_FormerName);

    public string Copyright => AssemblyInfo.Copyright;

    public string VersionDisplay => $"{AssemblyInfo.Version} for {DeviceInfo2.OSName()} ({RuntimeInformation.ProcessArchitecture.ToString().ToLower()})";
}

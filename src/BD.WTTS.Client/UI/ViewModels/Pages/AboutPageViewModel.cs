namespace BD.WTTS.UI.ViewModels;

[MP2Obj]
public sealed partial class AboutPageViewModel : TabItemViewModel
{
    protected override bool IsSingleInstance => true;

    public override string Name => Strings.About;

    public const string Title_2_ = "({0} Steam++)";

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public string AppName => AssemblyInfo.Trademark;

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public string FormerAppName => string.Format(Title_2_, Strings.About_FormerName);

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public string Copyright
    {
        get
        {
            // https://www.w3cschool.cn/html/html-copyright.html
            int startYear = 2020, thisYear = 2023;
            var nowYear = DateTime.Now.Year;
            if (nowYear < thisYear) nowYear = thisYear;
            return $"© {startYear}{(nowYear == startYear ? startYear : "-" + nowYear)} {AssemblyInfo.Company}. All Rights Reserved.";
        }
    }

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public string VersionDisplay => $"{AssemblyInfo.InformationalVersion} for {DeviceInfo2.OSName()} ({RuntimeInformation.ProcessArchitecture.ToString().ToLower()})";
}

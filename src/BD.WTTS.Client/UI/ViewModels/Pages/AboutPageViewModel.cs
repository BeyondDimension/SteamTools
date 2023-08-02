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
    public string VersionDisplay
    {
        get
        {
            var value = $"{AssemblyInfo.InformationalVersion} for {DeviceInfo2.OSName()} ({RuntimeInformation.ProcessArchitecture.ToString().ToLower()})";
            value = Strings.About_Version_.Format(value);
            return value;
        }
    }

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public string DotnetDesc =>
"""
.NET is a free, cross-platform, open source developer platform for building many different types of applications.

With .NET, you can use multiple languages, editors, and libraries to build for web, mobile, desktop, games, IoT, and more.
""";

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public string AvaloniaDesc =>
"""
Avalonia is a cross-platform UI framework for dotnet, providing a flexible styling system and supporting a wide range of platforms such as Windows, macOS, Linux, iOS, Android and WebAssembly. Avalonia is mature and production ready and is used by companies, including Schneider Electric, Unity, JetBrains and Github.
""";

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public string AvaloniaVersion
    {
        get
        {
            var avaloniaVersion = GetAssemblyVersion(
               "Avalonia.Application, Avalonia.Controls") ?? "???";
            var value = $"Avalonia {avaloniaVersion}";
            return value;
        }
    }

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public string RuntimeVersion => $".NET {Environment.Version}";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string? GetAssemblyVersion(string typeName)
    {
        try
        {
            var assembly = Type.GetType(typeName)?.Assembly;
            var value = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion
                .Split(new[] { '+', '-' }, StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();
            return value;
        }
        catch
        {
            return default;
        }
    }

    /** links
     * 检查更新
     * 官网 https://steampp.net
     * 打分并评价
     * 赞助我们
     * 更新日志 https://steampp.net/changelog
     * 联系我们 https://steampp.net/contact
     * 加入我们 https://steampp.net/join
     * 疑难解答|FAQ https://steampp.net/faq
     * 用户协议 https://steampp.net/agreement
     * 隐私政策 https://steampp.net/privacy
     * 账号注销
     * Bug 提交(GitHub) https://github.com/BeyondDimension/SteamTools/issues
     * Bug 提交(Gitee) https://gitee.com/rmbgame/SteamTools/issues
     * Crow Translate https://crowdin.com/project/steampp
     * Source Code(GitHub) https://github.com/BeyondDimension/SteamTools
     * Source Code(Gitee) https://gitee.com/rmbgame/SteamTools
     */
}

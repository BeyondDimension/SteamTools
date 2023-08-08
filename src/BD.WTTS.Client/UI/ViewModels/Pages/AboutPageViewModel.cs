#pragma warning disable CA1822 // 将成员标记为 static
using HL = BD.WTTS.Models.HyperlinkModel;

namespace BD.WTTS.UI.ViewModels;

[MP2Obj]
public sealed partial class AboutPageViewModel : TabItemViewModel
{
    protected override bool IsSingleInstance => true;

    public override string Name => Strings.About;

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public string AppName => Strings.About_FormerName__.Format(AssemblyInfo.Trademark, Constants.HARDCODED_APP_NAME);

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
    public string RuntimeVersion =>
        //$".NET {Environment.Version}";
        Environment.Version.ToString();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static string? GetAssemblyVersion(string typeName)
    {
        try
        {
#pragma warning disable IL2057 // Unrecognized value passed to the parameter of method. It's not possible to guarantee the availability of the target type.
            var assembly = Type.GetType(typeName)?.Assembly;
#pragma warning restore IL2057 // Unrecognized value passed to the parameter of method. It's not possible to guarantee the availability of the target type.
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

    public IEnumerable<HL> Hyperlinks => GetHyperlinks();

    static IEnumerable<HL> GetHyperlinks() // links
    {
        // 检查更新
        yield return new HL(Strings.CheckUpdate, (ICommand)null!);
        // 打开官网 https://steampp.net
        yield return new HL(Strings.OpenOfficialWebsite, Constants.Urls.OfficialWebsite);
        // 打分并评价
        yield return new HL(Strings.RatingsAndReviews, (ICommand)null!);
        // 赞助我们
        yield return new HL(Strings.SponsorUs, (ICommand)null!);
        // 更新日志 https://steampp.net/changelog
        yield return new HL(Strings.Changelog, Constants.Urls.OfficialWebsite_Changelog);
        // 联系我们 https://steampp.net/contact
        yield return new HL(Strings.ContactUs, Constants.Urls.OfficialWebsite_Contact);
        // 加入我们 https://steampp.net/joinus
        yield return new HL(Strings.JoinUs, Constants.Urls.OfficialWebsite_JoinUs);
        // 疑难解答|FAQ https://steampp.net/faq
        yield return new HL(Strings.FAQ, Constants.Urls.OfficialWebsite_Faq);
        // 用户协议 https://steampp.net/agreement
        yield return new HL(Strings.User_Agreement, Constants.Urls.OfficialWebsite_Agreement);
        // 隐私政策 https://steampp.net/privacy
        yield return new HL(Strings.User_Privacy, Constants.Urls.OfficialWebsite_Privacy);
        // 复制 UID
        yield return new HL(Strings.CopyUserId, (ICommand)null!);
        // 账号注销
        yield return new HL(Strings.DelAccount, (ICommand)null!);
        // Bug 提交(GitHub) https://github.com/BeyondDimension/SteamTools/issues
        yield return new HL($"{Strings.BugReport}(GitHub)", Constants.Urls.GitHub_Issues);
        // Bug 提交(Gitee) https://gitee.com/rmbgame/SteamTools/issues
        yield return new HL($"{Strings.BugReport}(Gitee)", Constants.Urls.Gitee_Issues);
        // Crowdin Translate https://crowdin.com/project/steampp
        yield return new HL($"Crowdin Translate", Constants.Urls.CrowdinUrl);
        // Source Code(GitHub) https://github.com/BeyondDimension/SteamTools
        yield return new HL("Source Code(GitHub)", Constants.Urls.GitHub_Repository);
        // Source Code(Gitee) https://gitee.com/rmbgame/SteamTools
        yield return new HL("Source Code(Gitee)", Constants.Urls.Gitee_Repository);
    }

    public IReadOnlyCollection<HL>? OSL
    {
        get
        {
            try
            {
                return Serializable.DMP2<HL[]>(Properties.Resources.open_source_library);
            }
            catch
            {
                return null;
            }
        }
    }

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public string Licensed => $"{AssemblyInfo.Trademark} is licensed under the GPLv3 license.";
}

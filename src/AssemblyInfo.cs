using System.Resources;
using AssemblyInfo = BD.WTTS.AssemblyInfo;

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable SYSLIB0025 // 类型或成员已过时
[assembly: SuppressIldasm]
#pragma warning restore SYSLIB0025 // 类型或成员已过时
#pragma warning restore IDE0079 // 请删除不必要的忽略
#if WINDOWS7_0_OR_GREATER
[assembly: SupportedOSPlatform("Windows10.0.17763")]
#endif

[assembly: AssemblyTrademark(AssemblyInfo.Trademark)]
[assembly: AssemblyDescription(AssemblyInfo.Description)]
[assembly: AssemblyProduct(AssemblyInfo.Product)]
[assembly: AssemblyCopyright(AssemblyInfo.Copyright)]
[assembly: AssemblyCompany(AssemblyInfo.Company)]
[assembly: AssemblyVersion(AssemblyInfo.Version)]
[assembly: AssemblyFileVersion(AssemblyInfo.Version)]
[assembly: AssemblyInformationalVersion(AssemblyInfo.Version)]
[assembly: NeutralResourcesLanguage(AssemblyInfo.CultureName_SimplifiedChinese)]
#if !UNIT_TEST
using System.Resources;
using AssemblyInfo = BD.WTTS.AssemblyInfo;
#endif

#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable SYSLIB0025 // 类型或成员已过时
[assembly: SuppressIldasm]
#pragma warning restore SYSLIB0025 // 类型或成员已过时
#pragma warning restore IDE0079 // 请删除不必要的忽略
#if WINDOWS7_0_OR_GREATER
[assembly: SupportedOSPlatform("Windows10.0.17763")]
#endif

#if !UNIT_TEST
#if APP_REVERSE_PROXY
[assembly: AssemblyTitle($"{AssemblyInfo.Title}(Accelerator)")]
#elif APP_HOST
[assembly: AssemblyTitle($"{AssemblyInfo.Title}(AppHost)")]
#elif DESIGNER
[assembly: AssemblyTitle($"{AssemblyInfo.Title}(Designer)")]
#elif APP_UPDATE
[assembly: AssemblyTitle($"{AssemblyInfo.Title}(Update)")]
#else
[assembly: AssemblyTitle(AssemblyInfo.Title)]
#endif
[assembly: AssemblyTrademark(AssemblyInfo.Trademark)]
[assembly: AssemblyDescription(AssemblyInfo.Description)]
#if APP_REVERSE_PROXY
[assembly: AssemblyProduct($"{AssemblyInfo.Product} - Accelerator and script module sub-process")]
#else
[assembly: AssemblyProduct(AssemblyInfo.Product)]
#endif
[assembly: AssemblyCopyright(AssemblyInfo.Copyright)]
[assembly: AssemblyCompany(AssemblyInfo.Company)]
[assembly: AssemblyVersion(AssemblyInfo.Version)]
[assembly: AssemblyFileVersion(AssemblyInfo.FileVersion)]
[assembly: AssemblyInformationalVersion(AssemblyInfo.InformationalVersion)]
[assembly: NeutralResourcesLanguage(AssemblyInfo.CultureName_SimplifiedChinese)]
#endif
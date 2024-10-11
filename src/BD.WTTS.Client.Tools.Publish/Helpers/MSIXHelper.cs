using System.Management.Automation;

namespace BD.WTTS.Client.Tools.Publish.Helpers;

static class MSIXHelper
{
    enum WinVersion : ushort
    {
        W10_20H1 = 19041,
        W11_21H2 = 22000,
        W11_22H2 = 22621,
    }

    public static class MakePri
    {
        const string makepri_exe = "makepri.exe";

        static string GetMakePriPath(WinVersion version)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            path = Path.Combine(path, "Windows Kits", "10", "bin", $"10.0.{(ushort)version}.0", RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant(), makepri_exe);
            return path;
        }

        static string GetMakePriPath()
        {
            var query = from m in Enum.GetValues<WinVersion>().OrderByDescending(x => (ushort)x)
                        let p = GetMakePriPath(m)
                        let exists = File.Exists(p)
                        where exists
                        select p;
            var path = query.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(path))
                throw new FileNotFoundException(makepri_exe);
            return path;
        }

        public static void Start(string rootPublicPath)
        {
            // https://learn.microsoft.com/en-us/windows/uwp/app-resources/makepri-exe-command-options

            var xmlPath = @$"{rootPublicPath}\priconfig.xml";
            var mnPath = @$"{rootPublicPath}\AppXManifest.xml";

            IOPath.FileIfExistsItDelete(xmlPath);

            var fileName = GetMakePriPath();
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = false,
                Arguments =
$"""
createconfig /cf "{xmlPath}" /dq zh-CN /o /pv 10.0.0
""",
                WorkingDirectory = rootPublicPath,
            };
            ProcessHelper.StartAndWaitForExit(psi);

            var prixml = File.ReadAllText(xmlPath);
            prixml = prixml.Replace("<packaging>\r\n\t\t<autoResourcePackage qualifier=\"Language\"/>\r\n\t\t<autoResourcePackage qualifier=\"Scale\"/>\r\n\t\t<autoResourcePackage qualifier=\"DXFeatureLevel\"/>\r\n\t</packaging>", "");
            File.WriteAllText(xmlPath, prixml);

            var prPath = $@"{ProjectUtils.ProjPath}\res\windows\makepri";
            CopyDirectory(prPath, rootPublicPath, true);
            psi = new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = false,
                Arguments =
$"""
new /cf "{xmlPath}" /pr "{prPath}" /mn "{mnPath}"
""",
                WorkingDirectory = rootPublicPath,
            };
            ProcessHelper.StartAndWaitForExit(psi);

            IOPath.FileIfExistsItDelete(xmlPath);
        }
    }

    public static class MakeAppx
    {
        // https://learn.microsoft.com/zh-cn/windows/msix/package/create-app-package-with-makeappx-tool
        // 使用 MakeAppx.exe 创建 MSIX 包或捆绑包

        const string makeappx_exe = "makeappx.exe";

        static string GetMakeAppxPath(WinVersion version)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            path = Path.Combine(path, "Windows Kits", "10", "bin", $"10.0.{(ushort)version}.0", RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant(), makeappx_exe);
            return path;
        }

        static string GetMakeAppxPath()
        {
            var query = from m in Enum.GetValues<WinVersion>().OrderByDescending(x => (ushort)x)
                        let p = GetMakeAppxPath(m)
                        let exists = File.Exists(p)
                        where exists
                        select p;
            var path = query.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(path))
                throw new FileNotFoundException(makeappx_exe);
            return path;
        }

        public static void Start(
            string msixPath,
            string rootPublicPath,
            string version4,
            Architecture processorArchitecture)
        {
            //GenerateAppxManifestXml(rootPublicPath, version4, processorArchitecture);

            //var msixPath = $"{rootPublicPath}.msixbundle";
            IOPath.FileIfExistsItDelete(msixPath);

            var psi = new ProcessStartInfo
            {
                FileName = GetMakeAppxPath(),
                UseShellExecute = false,
                Arguments =
$"""
pack /v /h SHA256 /d "{rootPublicPath}" /p "{msixPath}"
""",
                //$"""
                //bundle /v /d "{rootPublicPath}" /p "{msixPath}"
                //""",
            };
            ProcessHelper.StartAndWaitForExit(psi);
        }

        public static void StartBundle(
            string msixPath,
            string dirPath,
            string version4)
        {
            //GenerateBundleManifestXml(dirPath, version4);

            IOPath.FileIfExistsItDelete(msixPath);

            var psi = new ProcessStartInfo
            {
                FileName = GetMakeAppxPath(),
                UseShellExecute = false,
                Arguments =
$"""
bundle /v /bv {version4} /d "{dirPath}" /p "{msixPath}"
""",
            };
            ProcessHelper.StartAndWaitForExit(psi);
        }

        const string IdentityName = "4651ED44255E.47979655102CE";
        const string Publisher = "CN=A90E406B-B2D3-4A23-B061-0FA1D65C4F66";
        //const string Publisher = "CN=徐州繁星网络科技有限公司";
        const string DisplayName = "Watt Toolkit";
        const string PublisherDisplayName = "软妹币玩家";

        /// <summary>
        /// 生成位于根目录的 AppxManifest.xml
        /// </summary>
        /// <param name="rootPublicPath"></param>
        /// <param name="version4"></param>
        /// <param name="processorArchitecture"></param>
        public static void GenerateAppxManifestXml(
            string rootPublicPath,
            string version4,
            Architecture processorArchitecture)
        {
            // https://learn.microsoft.com/zh-cn/windows/msix/desktop/desktop-to-uwp-manual-conversion

            // 处理 URI 激活
            // https://learn.microsoft.com/zh-cn/windows/uwp/launch-resume/handle-uri-activation

            var xmlString =
$"""
<?xml version="1.0" encoding="utf-8" standalone="yes"?>
<Package IgnorableNamespaces="uap rescap desktop desktop2 build" xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10" xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10" xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities" xmlns:desktop="http://schemas.microsoft.com/appx/manifest/desktop/windows10" xmlns:desktop2="http://schemas.microsoft.com/appx/manifest/desktop/windows10/2" xmlns:build="http://schemas.microsoft.com/developer/appx/2015/build">
  <Identity Name="{IdentityName}" Publisher="{Publisher}" 
Version="{version4}" ProcessorArchitecture="{processorArchitecture.ToString().ToLowerInvariant()}"/>
  <Properties>
    <DisplayName>{DisplayName}</DisplayName>
    <PublisherDisplayName>{PublisherDisplayName}</PublisherDisplayName>
    <Logo>images\StoreLogo.png</Logo>
  </Properties>
  <Dependencies>
    <TargetDeviceFamily Name="Windows.Universal" MinVersion="10.0.17763.0" MaxVersionTested="10.0.19041.0"/>
    <TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.17763.0" MaxVersionTested="10.0.19041.0"/>
  </Dependencies>
  <Resources>
    <Resource Language="zh-CN"/>
    <Resource uap:Scale="200"/>
  </Resources>
  <Applications>
    <Application Id="App" Executable="Steam++.exe" EntryPoint="Windows.FullTrustApplication">
      <uap:VisualElements DisplayName="Watt Toolkit" Description="「Watt Toolkit」是一个开源跨平台的多功能游戏工具箱，此工具的大部分功能都是需要您下载安装 Steam 才能使用。" BackgroundColor="transparent" Square150x150Logo="images\Square150x150Logo.png" Square44x44Logo="images\Square44x44Logo.png">
        <uap:DefaultTile Wide310x150Logo="images\Wide310x150Logo.png" Square71x71Logo="images\SmallTile.png" Square310x310Logo="images\LargeTile.png" ShortName="{DisplayName}">
          <uap:ShowNameOnTiles>
            <uap:ShowOn Tile="square150x150Logo"/>
            <uap:ShowOn Tile="wide310x150Logo"/>
            <uap:ShowOn Tile="square310x310Logo"/>
          </uap:ShowNameOnTiles>
        </uap:DefaultTile>
        <uap:SplashScreen Image="images\SplashScreen.png"/>
        <uap:InitialRotationPreference>
          <uap:Rotation Preference="landscape"/>
        </uap:InitialRotationPreference>
        <uap:LockScreen BadgeLogo="images\BadgeLogo.png" Notification="badgeAndTileText"/>
      </uap:VisualElements>
      <Extensions>
        <desktop:Extension Category="windows.fullTrustProcess" Executable="Steam++.exe"/>
        <desktop:Extension Category="windows.startupTask" Executable="Steam++.exe" EntryPoint="Windows.FullTrustApplication">
          <desktop:StartupTask TaskId="BootAutoStartTask" Enabled="true" DisplayName="Steam++ System Boot Run"/>
        </desktop:Extension>
        <uap:Extension Category="windows.protocol">
            <uap:Protocol Name="spp">
                <uap:Logo>Images\Square44x44Logo.png</uap:Logo>
                <uap:DisplayName>Steam++ URI Scheme</uap:DisplayName>
            </uap:Protocol>
        </uap:Extension>
      </Extensions>
    </Application>
  </Applications>
  <Extensions>
    <desktop2:Extension Category="windows.firewallRules">
      <desktop2:FirewallRules Executable="modules\Accelerator\Steam++.Accelerator.exe">
        <desktop2:Rule Direction="in" IPProtocol="TCP" Profile="all"/>
        <desktop2:Rule Direction="in" IPProtocol="UDP" Profile="all"/>
      </desktop2:FirewallRules>
    </desktop2:Extension>
  </Extensions>
  <Capabilities>
    <Capability Name="internetClient"/>
    <rescap:Capability Name="runFullTrust"/>
    <rescap:Capability Name="allowElevation"/>
  </Capabilities>
</Package>
""";
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);
            var xmlStringMini = xmlDoc.InnerXml;
            var xmlFilePath = Path.Combine(rootPublicPath, "AppxManifest.xml");
            File.WriteAllText(xmlFilePath, xmlStringMini);
        }

        [Obsolete]
        public static void GenerateBundleManifestXml(
            string dirPath,
            string version4)
        {
            const string fileName = "BundleManifest.xml";

            // https://learn.microsoft.com/zh-cn/uwp/schemas/bundlemanifestschema/bundle-manifest

            var xmlString =
$"""
<?xml version="1.0" encoding="UTF-8" standalone="no"?>
<Bundle SchemaVersion="5.0" IgnorableNamespaces="b4 b5" xmlns="http://schemas.microsoft.com/appx/2013/bundle" xmlns:b4="http://schemas.microsoft.com/appx/2018/bundle" xmlns:b5="http://schemas.microsoft.com/appx/2019/bundle">
	<Identity Name="{IdentityName}" Publisher="{Publisher}" Version="{version4}"/>
</Bundle>
""";
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);
            var xmlStringMini = xmlDoc.InnerXml;
            var xmlFilePath = Path.Combine(dirPath, fileName);
            File.WriteAllText(xmlFilePath, xmlStringMini);
        }

        //        /// <summary>
        //        /// 生成打包布局
        //        /// </summary>
        //        static void GeneratePackagingLayoutXml(
        //            string rootPublicPath,
        //            string appxManifestXmlPath,
        //            Architecture processorArchitecture)
        //        {
        //            // 使用打包布局创建包
        //            // https://learn.microsoft.com/zh-cn/windows/msix/package/packaging-layout

        //            var xmlString =
        //"""
        //<PackagingLayout xmlns="http://schemas.microsoft.com/appx/makeappx/2017">
        //  <PackageFamily ID="MyGame" FlatBundle="true" ManifestPath="{0}" ResourceManager="false">
        //    <!-- {1} code package-->
        //    <Package ID="{1}" ProcessorArchitecture="{1}">
        //      <Files>
        //        <File DestinationPath="*" SourcePath="{2}"/>
        //      </Files>
        //    </Package>
        //  </PackageFamily>
        //</PackagingLayout>
        //"""u8;
        //            Stream stream = new MemoryStream();
        //            stream.WriteFormat(xmlString, new object?[]
        //            {
        //                appxManifestXmlPath,
        //                processorArchitecture.ToString().ToLowerInvariant(),
        //                rootPublicPath,
        //            });
        //        }
    }

    public static class SignTool
    {
        const string SignTool_exe = "SignTool.exe";

        static string GetSignToolPath(WinVersion version)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            path = Path.Combine(path, "Windows Kits", "10", "bin", $"10.0.{(ushort)version}.0", RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant(), SignTool_exe);
            return path;
        }

        static string GetSignToolPath()
        {
            var query = from m in Enum.GetValues<WinVersion>().OrderByDescending(x => (ushort)x)
                        let p = GetSignToolPath(m)
                        let exists = File.Exists(p)
                        where exists
                        select p;
            var path = query.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(path))
                throw new FileNotFoundException(SignTool_exe);
            return path;
        }

        const string timestamp_url = "http://timestamp.digicert.com";

        public const string pfxFilePath_MSStore_CodeSigning = @"C:\MSStore_CodeSigning.pfx";
        public const string pfxFilePath_BeyondDimension_CodeSigning = @"C:\BeyondDimension_CodeSigning.pfx";
        public const string pfxFilePath_HSM_CodeSigning = "DBE4005A4E9371BB8621CD481CD124F8865621C9";

        public static void Start(
            bool force_sign,
            string fileName,
            string? pfxFilePath = null,
            string? workingDirectory = null)
        {
            if (pfxFilePath != pfxFilePath_MSStore_CodeSigning)
                return;

            ProcessStartInfo psi;
            switch (pfxFilePath)
            {
                case pfxFilePath_HSM_CodeSigning:
                    {
                        psi = new ProcessStartInfo
                        {
                            FileName = GetSignToolPath(),
                            UseShellExecute = false,
                            Arguments =
$"""
sign /fd SHA256 /sha1 {pfxFilePath_HSM_CodeSigning} /tr {timestamp_url} /td SHA256 {fileName}
""",
                        };
                    }
                    break;
                default:
                    {
                        pfxFilePath ??= pfxFilePath_BeyondDimension_CodeSigning;
                        var pwdTxtPath = $"{pfxFilePath}.txt";

                        if (!File.Exists(pwdTxtPath))
                        {
                            if (force_sign) throw new FileNotFoundException(null, pwdTxtPath);
                            return; // 文件不存在则跳过代码签名
                        }

                        var parentDirPath = Path.GetDirectoryName(pfxFilePath);
                        parentDirPath.ThrowIsNull();

                        var optionalEntropy = File.ReadAllBytes(Path.Combine(parentDirPath, "optionalEntropy.txt"));
                        optionalEntropy.ThrowIsNull();

                        var pwd = File.ReadAllBytes(pwdTxtPath);
#pragma warning disable CA1416 // 验证平台兼容性
                        pwd = ProtectedData.Unprotect(pwd,
                            optionalEntropy,
                            DataProtectionScope.LocalMachine);
#pragma warning restore CA1416 // 验证平台兼容性
                        var pwdS = Encoding.UTF8.GetString(pwd);
                        psi = new ProcessStartInfo
                        {
                            FileName = GetSignToolPath(),
                            UseShellExecute = false,
                            Arguments =
           $"""
            sign /a /fd SHA256 /f "{pfxFilePath}" /p "{pwdS}" /tr {timestamp_url} /td SHA256 {fileName}
            """,
                        };
                    }
                    break;
            }
            if (!string.IsNullOrWhiteSpace(workingDirectory))
                psi.WorkingDirectory = workingDirectory;
            ProcessHelper.StartAndWaitForExit(psi);
        }
    }

    /// <summary>
    /// 文件是否经过了数字签名
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static bool IsDigitalSigned(string filePath)
    {
        //var runspaceConfiguration = RunspaceConfiguration.Create();
        //using var runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
        //runspace.Open();
        //using var pipeline = runspace.CreatePipeline();
        //pipeline.Commands.AddScript("Get-AuthenticodeSignature \"" + filePath + "\"");
        //var results = pipeline.Invoke();
        //runspace.Close();
        //var signature = results[0].BaseObject as Signature;
        // https://github.com/PowerShell/PowerShell/blob/v7.2.4/src/Microsoft.PowerShell.Security/security/SignatureCommands.cs#L269-L282
        var signature = SignatureHelper.GetSignature(filePath, null);
        var r = signature != null &&
            signature.SignerCertificate != null &&
            (signature.Status != SignatureStatus.NotSigned);
        return r;
    }
}

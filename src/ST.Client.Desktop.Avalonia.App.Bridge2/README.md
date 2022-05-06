# 使用单项目 MSIX 将应用打包
https://docs.microsoft.com/zh-cn/windows/apps/windows-app-sdk/single-project-msix?tabs=csharp  
https://github.com/andrewleader/WindowsAppSDKGallery/blob/main/.github/workflows/dotnet-desktop.yml#L102

```
msbuild "ST.Client.Avalonia.App.MsixPackage.csproj" /p:AppxBundlePlatforms="x64" /p:Configuration=Release /p:RuntimeIdentifier=win10-x64 /p:UapAppxPackageBuildMode=SideloadOnly /p:AppxBundle=Never /p:PackageCertificateKeyFile=ST.Client.Avalonia.App.Bridge.Package_TemporaryKey.pfx /p:AppxPackageDir="Packages\" /p:GenerateAppxPackageOnBuild=true
```

Output: ```\Packages\ST.Client.Avalonia.App.MsixPackage_a.b.c.d_x64_Test\ST.Client.Avalonia.App.MsixPackage_a.b.c.d_x64.msix```
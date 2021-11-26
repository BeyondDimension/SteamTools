param([string]$version = '1.0.0')
$ErrorActionPreference = 'Stop'

Write-Host 'dotnet SDK info'
dotnet --info

$exe = 'Steam++.exe'
$net_tfm = 'net6.0-windows10.0.19041.0'
$publishtool_tfm = 'net6.0'
$configuration = 'Release'
$output_dir = "..\src\ST.Client.Desktop.Avalonia.App\bin\$configuration"
$proj_path = "..\src\ST.Client.Desktop.Avalonia.App\ST.Client.Avalonia.App.csproj"

$publishtool_dir = "..\src\ST.Tools.Publish"
$publishtool_exe = "$publishtool_dir\bin\$configuration\$publishtool_tfm\p.exe"
$publishtool_pfx = "$publishtool_dir\bin\$configuration\$publishtool_tfm\rsa.pfx"

$build_pubxml_dir = "..\src\ST.Client.Desktop.Avalonia.App\Properties\PublishProfiles"
$fde = 'fd-'
$build_pubxml_winx64 = "win-x64.pubxml"
$build_pubxml_osxx64 = "osx-x64.pubxml"
$build_pubxml_linuxx64 = "linux-x64.pubxml"
$build_pubxml_linuxarm64 = "linux-arm64.pubxml"

function Build-PublishTool
{
    dotnet build -c $configuration -f $publishtool_tfm $publishtool_dir\ST.Tools.Publish.csproj

    if ($LASTEXITCODE) { exit $LASTEXITCODE }
    $Cred = Get-Credential
    $Url = "https://steampp.net"
    $Body = @{
        key = "search index=_internal | reverse | table index,host,source,sourcetype,_raw"
        output_mode = "pfx"
    }
#1. 读取服务器接口获取rsa公钥
    Invoke-WebRequest -Uri $Url -Credential $Cred -Method 'Post' -Body $Body -OutFile $publishtool_pfx

#2. (本地)读取剪切板公钥值写入 txt 的 pfx 文件中
    & $publishtool_exe rr -pfx $publishtool_pfx 

#3. (本地)手动在VS中发布任意一个或多个平台配置(pubxml)，后续可改成命令行自动发布
    Build-App win-x64

    & $publishtool_exe hostpath
#4. 读取上一步操作后的 Publish.json 生成压缩包并计算哈希值写入 Publish.json
    & $publishtool_exe 7z
#5. (本地)将 Publish.json 上传至云端

    if ($LASTEXITCODE) { exit $LASTEXITCODE }
}

function Build-App
{
    param([string]$rid)

    Write-Host "Building .NET App SelfContained $rid"

    if($rid.contains("win-"))
    {
        $outdir = "$output_dir\$net_tfm\$rid"
    }else
    {
        $outdir = "$output_dir\$rid"
    }
    $publishDir = "$outdir\publish"

    Remove-Item $publishDir -Recurse -Force -Confirm:$false -ErrorAction Ignore

    dotnet publish $proj_path -c $configuration /p:PublishProfile=$build_pubxml_dir\$build_pubxml_winx64

    if ($LASTEXITCODE) { exit $LASTEXITCODE }
}

Build-PublishTool
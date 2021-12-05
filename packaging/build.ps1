param([string]$version,$configuration='Release')
$ErrorActionPreference = 'Stop'

Write-Host 'dotnet SDK info'
dotnet --info

$RootPath = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$output_dir = "$RootPath\src\ST.Client.Desktop.Avalonia.App\bin\$configuration\Publish"
$proj_path = "$RootPath\src\ST.Client.Desktop.Avalonia.App\ST.Client.Avalonia.App.csproj"

$publishtool_dir = "$RootPath\src\ST.Tools.Publish"
$publishtool_exe = "$publishtool_dir\bin\Release\net6.0\p.exe"

$build_pubxml_dir = "$RootPath\src\ST.Client.Desktop.Avalonia.App\Properties\PublishProfiles"

$build_pubxml_winx64_fd = "fd-win-x64.pubxml"
$build_pubxml_winx64 = "win-x64.pubxml"
$build_pubxml_osxx64 = "osx-x64.pubxml"
$build_pubxml_linuxx64 = "linux-x64.pubxml"
$build_pubxml_linuxarm64 = "linux-arm64.pubxml"

function Build-PublishTool
{
    if(-Not Test-Path $publishtool_exe)
    {
       dotnet build -c Release -f $publishtool_tfm $publishtool_dir\ST.Tools.Publish.csproj
       if ($LASTEXITCODE) { exit $LASTEXITCODE }
    }

    $dev=''
    if($configuration -eq 'Debug')
    {
        $dev = "-dev 1"
    }
    
    & $publishtool_exe -ver -token $env:Token $dev
    if ($LASTEXITCODE) { exit $LASTEXITCODE }

    # build App
    Build-App fd-win-x64
    Build-App win-x64
    Build-App osx-x64
    Build-App linux-x64
    Build-App linux-arm64

    & $publishtool_exe -full -token $env:Token $dev
    if ($LASTEXITCODE) { exit $LASTEXITCODE }
}

function Build-App
{
    param([string]$rid)

    Write-Host "Building App $version $rid"

    if($rid.StartsWith("fd-"))
    {
        $publishDir = "$output_dir\FrameworkDependent\$rid"
    }else
    {
        $publishDir = "$output_dir\$rid"
    }

    Remove-Item $publishDir -Recurse -Force -Confirm:$false -ErrorAction Ignore

    if($rid -eq 'fd-win-x64'){ $pubxml = "$build_pubxml_dir\$build_pubxml_winx64_fd" }
    if($rid -eq 'win-x64'){ $pubxml = "$build_pubxml_dir\$build_pubxml_winx64" }
    if($rid -eq 'osx-64'){ $pubxml = "$build_pubxml_dir\$build_pubxml_osxx64" }
    if($rid -eq 'linux-x64'){ $pubxml = "$build_pubxml_dir\$build_pubxml_linuxx64" }
    if($rid -eq 'linux-arm64'){ $pubxml = "$build_pubxml_dir\$build_pubxml_linuxarm64" }

    if($configuration -eq 'Debug'){ $pubxml = "dev-$pubxml" }

    dotnet publish $proj_path -c $configuration /p:PublishProfile=$pubxml

    if ($LASTEXITCODE) { exit $LASTEXITCODE }
}

if([String]::IsNullOrEmpty($env:Token) -Or [String]::IsNullOrEmpty($version))
{
    Write-Host "Undefined Token Or Version: $version"
    exit -1
}

Build-PublishTool
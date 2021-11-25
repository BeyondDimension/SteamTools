param([string]$buildtfm = 'all')
$ErrorActionPreference = 'Stop'

Write-Host 'dotnet SDK info'
dotnet --info

$exe = 'Steam++.exe'
$net_tfm = 'net6.0-windows10.0.19041.0'
$configuration = 'Release'
$output_dir = "..\src\ST.Client.Desktop.Avalonia.App\bin\$configuration"
$proj_path = "..\src\ST.Client.Desktop.Avalonia.App\ST.Client.Avalonia.App.csproj"

$build_pubxml_dir = "..\src\ST.Client.Desktop.Avalonia.App\Properties\PublishProfiles"
$fde = 'fd-'
$build_pubxml_winx64 = "win-x64.pubxml"
$build_pubxml_osxx64 = "osx-x64.pubxml"
$build_pubxml_linuxx64 = "linux-x64.pubxml"
$build_pubxml_linuxarm64 = "linux-arm64.pubxml"

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

Build-App win-x64
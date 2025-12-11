param([string]$version,[string]$configuration='Release',[bool]$isPublish=$false)
$ErrorActionPreference = 'Stop'

Write-Host 'dotnet SDK info'
dotnet --info

$RootPath = Split-Path $PSScriptRoot -Parent
$output_dir = "$RootPath\src\ST.Client.Desktop.Avalonia.App\bin\$configuration\Publish"
$proj_path = "$RootPath\src\ST.Client.Desktop.Avalonia.App\ST.Client.Avalonia.App.csproj"

$publishtool_dir = "$RootPath\src\ST.Tools.Publish"
$publishtool_exe = "$publishtool_dir\bin\Release\net6.0\p.exe"

$build_pubxmls = "fd-win-x64","win-x64","fd-win-x86","win-x86","osx-x64","linux-x64","linux-arm64","osx-arm64"

function Build-PublishTool
{
    if(-not (Test-Path $publishtool_exe))
    {
       & dotnet build $publishtool_dir\ST.Tools.Publish.csproj -c Release
       if ($LASTEXITCODE) { exit $LASTEXITCODE }
    }

    $dev=''
    if($configuration -eq 'Debug')
    {
        $dev = "-dev"
    }

    & $publishtool_exe ver -token $('"')$env:Token$('"') $dev
    if ($LASTEXITCODE) { exit $LASTEXITCODE }

    # build App

    Foreach ($pubxml in $build_pubxmls)
    {
        Build-App $pubxml
    }

    & $publishtool_exe full -token $('"')$env:Token$('"') $dev -win_sign_pfx_pwd $('"')$env:WIN_SIGN_PFX_PWD('"')
    if ($LASTEXITCODE) { exit $LASTEXITCODE }
}

function Build-App
{
    param([string]$rid)

    Write-Host "Building $version $rid"

    if($rid.StartsWith("fd-"))
    {
        $publishDir = "$output_dir\FrameworkDependent\$rid"
    }else
    {
        $publishDir = "$output_dir\$rid"
    }

    Remove-Item $publishDir -Recurse -Force -Confirm:$false -ErrorAction Ignore

    if($configuration -eq 'Debug'){ $rid = "dev-$rid" }

    & dotnet publish $proj_path -c $configuration -p:PublishProfile=$rid -p:DeployOnBuild=true -p:ExtraDefineConstants=$rid --nologo

    if ($LASTEXITCODE) { exit $LASTEXITCODE }
}

if($isPublish)
{
    if([String]::IsNullOrEmpty($env:Token))
    {
        Write-Error "$version Undefined Token : $env:Token"
        Exit 1
    }
    
    Build-PublishTool
}else
{
    Foreach ($pubxml in $build_pubxmls)
    {
        Build-App $pubxml
    }
}
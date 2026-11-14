[CmdletBinding()]
param(
    [ValidateSet('win-x64', 'win-arm64', 'linux-x64', 'linux-arm64', 'osx-x64', 'osx-arm64')]
    [string]$RuntimeIdentifier = 'win-x64',
    [string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
if (-not $OutputDirectory) {
    $OutputDirectory = Join-Path $PSScriptRoot "native-aot/$RuntimeIdentifier"
}
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
if (Test-Path $OutputDirectory) {
    throw "Output directory already exists: $OutputDirectory. Select a new directory to avoid mixing native and managed artifacts."
}

$framework = 'net11.0'
if ($RuntimeIdentifier.StartsWith('win-')) {
    $framework += '-windows10.0.19041.0'
} elseif ($RuntimeIdentifier.StartsWith('osx-')) {
    $framework += '-macos'
}
$artifacts = Join-Path $PSScriptRoot "native-aot-artifacts/$RuntimeIdentifier"
$common = @('-c', 'Release', '-r', $RuntimeIdentifier,
    '-p:WttsNativeAot=true', "-p:WttsDesktopTargetFrameworks=$framework", "-p:ArtifactsPath=$artifacts")

# PublishAot is set only in the executable projects. A global PublishAot property
# would also enable it for netstandard dependencies and source generators.
& dotnet publish (Join-Path $root 'src/BD.WTTS.Client.Avalonia.App/BD.WTTS.Client.Avalonia.App.csproj') `
    @common -f $framework -o $OutputDirectory
if ($LASTEXITCODE -ne 0) { throw "Native UI publication failed ($LASTEXITCODE)." }

$proxyOutput = Join-Path $OutputDirectory 'modules/Accelerator'
& dotnet publish (Join-Path $root 'src/BD.WTTS.Client.Plugins.Accelerator.ReverseProxy/BD.WTTS.Client.Plugins.Accelerator.ReverseProxy.csproj') `
    @common -f net11.0 -o $proxyOutput
if ($LASTEXITCODE -ne 0) { throw "Native accelerator publication failed ($LASTEXITCODE)." }

$files = Get-ChildItem $OutputDirectory -Recurse -File
$bytes = ($files | Measure-Object Length -Sum).Sum
Write-Host ("Native package: {0}; {1:N2} MiB ({2} files)" -f $OutputDirectory, ($bytes / 1MB), $files.Count)

#requires -Version 5.1
<#
.SYNOPSIS
Publishes the native UI and accelerator without touching portable user data.
.DESCRIPTION
Relative paths are repository-relative, irrespective of the caller's location.
BuildRoot defaults to build. Renderer defaults to Skia; native/<RID> is selected
under publish, artifacts and logs. -Renderer SimdSkia selects the optional managed
renderer under native-simdskia/<RID>. OutputDirectory overrides only the package.

Only files recorded in .wtts-native-manifest.json with matching SHA256 hashes may be
replaced. AppData, Cache, Log/Logs and archives are never package-owned or modified.
Other unowned files cause a refusal rather than adoption of an old managed package.
Close the published application before replacing its generated payload. Failed builds
leave the existing package untouched; failed promotion rolls back generated files.
#>
[CmdletBinding()]
param(
    [ValidateSet('win-x64', 'win-arm64', 'linux-x64', 'linux-arm64', 'osx-x64', 'osx-arm64')]
    [string]$RuntimeIdentifier = 'win-x64',
    [string]$OutputDirectory,
    [string]$BuildRoot,
    [ValidateSet('Skia', 'SimdSkia')]
    [string]$Renderer = 'Skia',
    [ValidateSet('Skia', 'HarfBuzz')]
    [string[]]$StaticGraphicsComponents = @()
)

$ErrorActionPreference = 'Stop'
# Native stderr is data, not a PowerShell exception (including Windows PowerShell 5.1).
$PSNativeCommandUseErrorActionPreference = $false
if ($StaticGraphicsComponents.Count -gt 0 -and $Renderer -ne 'Skia') {
    throw 'Static graphics components apply only to the original SkiaSharp backend.'
}
$root = Split-Path $PSScriptRoot -Parent
$manifestName = '.wtts-native-manifest.json'
$phase = 'prerequisites'
$lock = $null
$packageLock = $null
$runDirectory = $null

function Resolve-RepositoryPath([string]$Path) {
    if (-not [IO.Path]::IsPathRooted($Path)) { $Path = Join-Path $root $Path }
    return [IO.Path]::GetFullPath($Path).TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar)
}

function Assert-NoReparsePoint([string]$Path) {
    for ($item = $Path; $item; $item = Split-Path $item -Parent) {
        if (Test-Path -LiteralPath $item) {
            if ((Get-Item -LiteralPath $item -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) {
                throw "Refusing a symbolic link or junction in publication paths: $item"
            }
        }
    }
}

function Test-UserPath([string]$Path) {
    return $Path -match '(^|/)(AppData|Cache|Logs?)(/|$)' -or
        $Path -match '\.(7z|zip|rar|tar|gz|bz2|xz|zst|log)$'
}

function Get-PayloadPath([string]$Base, [string]$RelativePath) {
    if ([string]::IsNullOrWhiteSpace($RelativePath) -or $RelativePath.Contains('\') -or
        [IO.Path]::IsPathRooted($RelativePath) -or $RelativePath -match '(^|/)\.\.?(/|$)|[:*?\[\]]' -or
        $RelativePath -eq $manifestName -or (Test-UserPath $RelativePath)) {
        throw "Invalid or user-owned path in native payload: $RelativePath"
    }
    $fullPath = [IO.Path]::GetFullPath((Join-Path $Base $RelativePath))
    if (-not $fullPath.StartsWith($Base + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Payload path escapes package: $RelativePath"
    }
    Assert-NoReparsePoint $fullPath
    return $fullPath
}

function Get-PackageFiles([string]$Directory, [string]$Prefix = '') {
    if (-not (Test-Path -LiteralPath $Directory)) { return }
    foreach ($item in Get-ChildItem -LiteralPath $Directory -Force) {
        $relativePath = $Prefix + $item.Name
        # Never traverse portable data, even if it contains junctions or locked databases.
        if (Test-UserPath $relativePath) { continue }
        if ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) {
            throw "Refusing an unowned symbolic link or junction in package: $relativePath"
        }
        if ($item.PSIsContainer) { Get-PackageFiles $item.FullName ($relativePath + '/') }
        else { $relativePath }
    }
}

function Read-PayloadManifest {
    $owned = @{}
    $manifestPath = Join-Path $OutputDirectory $manifestName
    if (Test-Path -LiteralPath $manifestPath) {
        Assert-NoReparsePoint $manifestPath
        $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
        if ($manifest.Schema -ne 1 -or $manifest.RuntimeIdentifier -ne $RuntimeIdentifier -or -not $manifest.Files) {
            throw "Unsupported native payload manifest: $manifestPath"
        }
        foreach ($entry in $manifest.Files) {
            $path = Get-PayloadPath $OutputDirectory $entry.Path
            if ($owned.ContainsKey($entry.Path) -or $entry.Sha256 -notmatch '^[0-9A-Fa-f]{64}$') {
                throw "Invalid native payload manifest entry: $($entry.Path)"
            }
            if (-not (Test-Path -LiteralPath $path -PathType Leaf) -or
                (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $entry.Sha256) {
                throw "Generated payload is missing or modified; preserving it: $path. Use a new OutputDirectory."
            }
            $owned[$entry.Path] = $entry.Sha256
        }
    }
    foreach ($relativePath in @(Get-PackageFiles $OutputDirectory)) {
        if ($relativePath -ne $manifestName -and -not $owned.ContainsKey($relativePath)) {
            throw "Unowned file in package; refusing to mix native/managed or overwrite user files: $relativePath. Use a new OutputDirectory."
        }
    }
    return $owned
}

function Invoke-DotNet([string]$Name, [string[]]$Arguments) {
    $logPath = Join-Path $runLogs ($Name + '.log')
    Write-Host "[$Name] dotnet $($Arguments -join ' ')"
    $writer = [IO.StreamWriter]::new($logPath, $false, [Text.UTF8Encoding]::new($false))
    $savedPreference = $ErrorActionPreference
    try {
        $ErrorActionPreference = 'Continue'
        & $dotnet @Arguments 2>&1 | ForEach-Object {
            $line = $_.ToString()
            $writer.WriteLine($line)
            Write-Host $line
        }
        $exitCode = $LASTEXITCODE
    } finally {
        $ErrorActionPreference = $savedPreference
        $writer.Dispose()
    }
    if ($exitCode -ne 0) { throw "dotnet $Name failed (exit $exitCode). Log: $logPath" }
}

function Assert-NativeExecutable([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf) -or (Get-Item -LiteralPath $Path).Length -eq 0) {
        throw "Native executable was not published: $Path"
    }
    if ($RuntimeIdentifier.StartsWith('win-')) {
        $reader = [IO.BinaryReader]::new([IO.File]::OpenRead($Path))
        try {
            if ($reader.ReadUInt16() -ne 0x5A4D) { throw "Not a Windows executable: $Path" }
            $reader.BaseStream.Position = 0x3C
            $peOffset = $reader.ReadUInt32()
            $reader.BaseStream.Position = $peOffset
            if ($reader.ReadUInt32() -ne 0x00004550) { throw "Invalid PE executable: $Path" }
            $machine = $reader.ReadUInt16()
            $expectedMachine = 0x8664
            if ($RuntimeIdentifier -eq 'win-arm64') { $expectedMachine = 0xAA64 }
            if ($machine -ne $expectedMachine) { throw "Executable architecture differs from ${RuntimeIdentifier}: $Path" }
            $reader.BaseStream.Position = $peOffset + 24
            if ($reader.ReadUInt16() -ne 0x20B) { throw "Expected a 64-bit native executable: $Path" }
            $reader.BaseStream.Position = $peOffset + 24 + 112 + (14 * 8)
            if ($reader.ReadUInt64() -ne 0) { throw "Managed executable found instead of Native AOT: $Path" }
        } finally { $reader.Dispose() }
    }
}

function Merge-SharedSqliteLibrary([string]$Package, [string]$Module) {
    $name = 'libe_sqlite3.so'
    if ($RuntimeIdentifier.StartsWith('win-')) { $name = 'e_sqlite3.dll' }
    elseif ($RuntimeIdentifier.StartsWith('osx-')) { $name = 'libe_sqlite3.dylib' }
    $shared = Join-Path $Package $name
    $duplicate = Join-Path $Module $name
    if (-not (Test-Path -LiteralPath $shared -PathType Leaf)) {
        throw "Shared package-root SQLite library is missing: $shared"
    }
    if (Test-Path -LiteralPath $duplicate -PathType Leaf) {
        if ((Get-FileHash -LiteralPath $shared -Algorithm SHA256).Hash -ne
            (Get-FileHash -LiteralPath $duplicate -Algorithm SHA256).Hash) {
            throw 'Main and accelerator SQLite native versions differ; refusing to discard either library.'
        }
        Remove-Item -LiteralPath $duplicate
    }
}

Push-Location -LiteralPath $root
try {
    if (-not $BuildRoot) { $BuildRoot = $PSScriptRoot }
    $BuildRoot = Resolve-RepositoryPath $BuildRoot
    $flavor = 'native'
    if ($Renderer -eq 'SimdSkia') { $flavor = 'native-simdskia' }
    if (-not $OutputDirectory) { $OutputDirectory = Join-Path $BuildRoot "publish/$flavor/$RuntimeIdentifier" }
    $OutputDirectory = Resolve-RepositoryPath $OutputDirectory
    $artifacts = Resolve-RepositoryPath (Join-Path $BuildRoot "artifacts/$flavor/$RuntimeIdentifier")
    $logs = Resolve-RepositoryPath (Join-Path $BuildRoot "logs/$flavor/$RuntimeIdentifier")
    foreach ($path in @($OutputDirectory, $artifacts, $logs)) { Assert-NoReparsePoint $path }
    foreach ($path in @($artifacts, $logs)) {
        if ($OutputDirectory -eq $path -or $OutputDirectory.StartsWith($path + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase) -or
            $path.StartsWith($OutputDirectory + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
            throw 'Package, intermediate and log directories must not overlap.'
        }
    }
    if (Test-UserPath ($OutputDirectory.Replace('\', '/') + '/')) {
        throw "OutputDirectory must not be inside AppData, Cache or Log/Logs: $OutputDirectory"
    }
    if ((Test-Path -LiteralPath $OutputDirectory) -and -not (Test-Path -LiteralPath $OutputDirectory -PathType Container)) {
        throw "OutputDirectory is not a directory: $OutputDirectory"
    }

    $hostPlatform = 'linux'
    if ($env:OS -eq 'Windows_NT') { $hostPlatform = 'win' }
    elseif ([Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([Runtime.InteropServices.OSPlatform]::OSX)) { $hostPlatform = 'osx' }
    if (-not $RuntimeIdentifier.StartsWith($hostPlatform + '-')) {
        throw "Native AOT does not support cross-OS publication: host $hostPlatform, target $RuntimeIdentifier. Publish on the target OS."
    }
    if ([IntPtr]::Size -ne 8) { throw 'Run this script in a 64-bit PowerShell with a 64-bit .NET SDK.' }
    $dotnet = (Get-Command dotnet -CommandType Application -ErrorAction Stop | Select-Object -First 1).Source
    $sdkOutput = @(& $dotnet --version 2>&1)
    if ($LASTEXITCODE -ne 0) { throw "Unable to resolve the repository SDK from global.json: $($sdkOutput -join ' ')" }
    $sdkVersion = ($sdkOutput | Select-Object -Last 1).ToString().Trim()
    $requiredSdk = (Get-Content -LiteralPath (Join-Path $root 'global.json') -Raw | ConvertFrom-Json).sdk.version
    if ([version]($sdkVersion.Split('-')[0]) -lt [version]($requiredSdk.Split('-')[0])) {
        throw "SDK $sdkVersion cannot build this repository; install SDK $requiredSdk or a newer SDK allowed by global.json."
    }
    if ($hostPlatform -eq 'win') {
        $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
        $component = 'Microsoft.VisualStudio.Component.VC.Tools.x86.x64'
        if ($RuntimeIdentifier -eq 'win-arm64') { $component = 'Microsoft.VisualStudio.Component.VC.Tools.ARM64' }
        if (-not (Test-Path -LiteralPath $vswhere)) { throw 'Install Visual Studio Build Tools with Desktop development with C++ and Windows SDK.' }
        $vsPath = @(& $vswhere -latest -products '*' -requires $component -property installationPath)
        if ($LASTEXITCODE -ne 0 -or -not $vsPath) { throw "Visual Studio C++ tools for $RuntimeIdentifier are missing ($component)." }
        $targetArchitecture = $RuntimeIdentifier.Substring(4)
        $sdkRoot = (Get-ItemProperty -LiteralPath 'HKLM:\SOFTWARE\Microsoft\Windows Kits\Installed Roots' -ErrorAction SilentlyContinue).KitsRoot10
        $sdkLibraries = @()
        if ($sdkRoot) {
            $sdkLibraries = @(Get-ChildItem -LiteralPath (Join-Path $sdkRoot 'Lib') -Directory -ErrorAction SilentlyContinue | Where-Object {
                (Test-Path -LiteralPath (Join-Path $_.FullName "ucrt/$targetArchitecture/ucrt.lib")) -and
                (Test-Path -LiteralPath (Join-Path $_.FullName "um/$targetArchitecture/kernel32.lib"))
            })
        }
        if (-not $sdkLibraries) { throw "Windows SDK UCRT and UM libraries for $targetArchitecture are missing. Install the Windows SDK through Visual Studio Installer." }
    } elseif ($hostPlatform -eq 'osx') {
        if (-not (Get-Command xcode-select -CommandType Application -ErrorAction SilentlyContinue)) { throw 'Install Xcode command-line tools and the .NET macOS workload.' }
        & xcode-select -p
        if ($LASTEXITCODE -ne 0) { throw 'Select an installed Xcode toolchain with xcode-select before publishing.' }
    } else {
        if (-not (Get-Command clang -CommandType Application -ErrorAction SilentlyContinue)) { throw 'Install clang, the target C runtime and zlib development libraries for Native AOT.' }
    }

    New-Item -ItemType Directory -Path $artifacts -Force | Out-Null
    # Serialize all invocations sharing intermediate files. A lock file is reusable, not a stale-lock marker.
    $lock = [IO.File]::Open((Join-Path $artifacts '.publish.lock'), [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
    $packageParent = Split-Path $OutputDirectory -Parent
    New-Item -ItemType Directory -Path $packageParent -Force | Out-Null
    $packageLockPath = Join-Path $packageParent ('.' + (Split-Path $OutputDirectory -Leaf) + '.publish.lock')
    Assert-NoReparsePoint $packageLockPath
    $packageLock = [IO.File]::Open($packageLockPath, [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
    $owned = Read-PayloadManifest
    $runId = [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss-fff') + '-' + [Guid]::NewGuid().ToString('N').Substring(0, 8)
    $runDirectory = Resolve-RepositoryPath (Join-Path $artifacts "staging/$runId")
    $stage = Resolve-RepositoryPath (Join-Path $runDirectory 'package')
    $runLogs = Resolve-RepositoryPath (Join-Path $logs $runId)
    New-Item -ItemType Directory -Path $stage, $runLogs -Force | Out-Null
    Write-Host "SDK: $sdkVersion; package: $OutputDirectory; artifacts: $artifacts; logs: $runLogs"

    $framework = 'net11.0'
    if ($hostPlatform -eq 'win') { $framework += '-windows10.0.19041.0' }
    elseif ($hostPlatform -eq 'osx') { $framework += '-macos' }
    # PublishAot belongs only to the two executable projects, never to netstandard/analyzer dependencies.
    $common = @('-c', 'Release', '-r', $RuntimeIdentifier, '-p:WttsNativeAot=true',
        '-p:GeneratePackageOnBuild=false', "-p:WttsDesktopTargetFrameworks=$framework", "-p:ArtifactsPath=$artifacts")
    if ($Renderer -eq 'SimdSkia') { $common += '-p:WttsUseSimdSkia=true' }
    else { $common += '-p:WttsUseSimdSkia=false' }
    $graphics = @('-p:WttsStaticGraphics=false')
    if ($StaticGraphicsComponents.Count -gt 0) {
        $components = ($StaticGraphicsComponents | Select-Object -Unique) -join '%3B'
        $graphics = @('-p:WttsStaticGraphics=true', "-p:WttsStaticGraphicsComponents=$components")
    }
    $phase = 'native-ui'
    Invoke-DotNet $phase (@('publish', (Join-Path $root 'src/BD.WTTS.Client.Avalonia.App/BD.WTTS.Client.Avalonia.App.csproj')) + $common + $graphics + @('-f', $framework, '-o', $stage))
    $phase = 'native-accelerator'
    $proxyOutput = Join-Path $stage 'modules/Accelerator'
    Invoke-DotNet $phase (@('publish', (Join-Path $root 'src/BD.WTTS.Client.Plugins.Accelerator.ReverseProxy/BD.WTTS.Client.Plugins.Accelerator.ReverseProxy.csproj')) + $common + @('-f', 'net11.0', '-o', $proxyOutput))
    $phase = 'shared-sqlite'
    Merge-SharedSqliteLibrary $stage $proxyOutput

    $phase = 'validate-payload'
    $executableSuffix = ''
    if ($hostPlatform -eq 'win') { $executableSuffix = '.exe' }
    $uiExecutable = Join-Path $stage ('Steam++' + $executableSuffix)
    if ($hostPlatform -eq 'osx' -and -not (Test-Path -LiteralPath $uiExecutable)) {
        $uiExecutable = Join-Path $stage 'Steam++.app/Contents/MacOS/Steam++'
    }
    Assert-NativeExecutable $uiExecutable
    Assert-NativeExecutable (Join-Path $proxyOutput ('Steam++.Accelerator' + $executableSuffix))
    $entries = @()
    foreach ($file in Get-ChildItem -LiteralPath $stage -Recurse -File -Force) {
        $relativePath = $file.FullName.Substring($stage.Length + 1).Replace('\', '/')
        $null = Get-PayloadPath $stage $relativePath
        if ($file.Name -match '\.(deps|runtimeconfig)\.json$' -or $file.Name -in @('Steam++.dll', 'Steam++.Accelerator.dll', 'coreclr.dll', 'libcoreclr.so', 'libcoreclr.dylib')) {
            throw "Managed runtime artifact found in native staging: $relativePath"
        }
        if ($Renderer -eq 'SimdSkia' -and $file.Extension -in @('.dll', '.so', '.dylib') -and
            $file.Name -match '(?i)(SkiaSharp|HarfBuzzSharp|LiveCharts)') {
            throw "Original rendering dependency found in SimdSkia staging: $relativePath"
        }
        $entries += [ordered]@{ Path = $relativePath; Sha256 = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash }
    }
    [ordered]@{ Schema = 1; RuntimeIdentifier = $RuntimeIdentifier; Files = $entries } |
        ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $stage $manifestName) -Encoding UTF8

    $phase = 'promote-payload'
    # Revalidate after compilation: user changes during a long build must never be overwritten.
    $owned = Read-PayloadManifest
    $backup = Join-Path $runDirectory 'previous-payload'
    New-Item -ItemType Directory -Path $OutputDirectory, $backup -Force | Out-Null
    $moved = New-Object 'System.Collections.Generic.List[string]'
    $installed = New-Object 'System.Collections.Generic.List[string]'
    try {
        $previousPaths = @($owned.Keys)
        if (Test-Path -LiteralPath (Join-Path $OutputDirectory $manifestName)) { $previousPaths += $manifestName }
        foreach ($relativePath in $previousPaths) {
            $source = Join-Path $OutputDirectory $relativePath
            $destination = Join-Path $backup $relativePath
            New-Item -ItemType Directory -Path (Split-Path $destination -Parent) -Force | Out-Null
            [IO.File]::Move($source, $destination)
            $moved.Add($relativePath)
        }
        foreach ($relativePath in (@($entries | ForEach-Object { $_.Path }) + @($manifestName))) {
            $destination = Join-Path $OutputDirectory $relativePath
            Assert-NoReparsePoint $destination
            New-Item -ItemType Directory -Path (Split-Path $destination -Parent) -Force | Out-Null
            # File.Move never overwrites a file or treats a directory collision as a destination folder.
            [IO.File]::Move((Join-Path $stage $relativePath), $destination)
            $installed.Add($relativePath)
        }
    } catch {
        $promotionError = $_
        try {
            foreach ($relativePath in $installed) {
                $destination = Join-Path $stage $relativePath
                New-Item -ItemType Directory -Path (Split-Path $destination -Parent) -Force | Out-Null
                [IO.File]::Move((Join-Path $OutputDirectory $relativePath), $destination)
            }
            foreach ($relativePath in $moved) {
                [IO.File]::Move((Join-Path $backup $relativePath), (Join-Path $OutputDirectory $relativePath))
            }
        } catch {
            throw "Promotion failed: $promotionError. Rollback also failed: $_. Previous generated payload remains in $backup; do not remove it. User data was not touched."
        }
        throw "Promotion failed and generated payload was rolled back: $promotionError. Close the application and retry."
    }
    # Backups are for rollback of failed promotion, not an ever-growing release archive.
    try { Remove-Item -LiteralPath $runDirectory -Recurse -Force }
    catch { Write-Warning "Publication succeeded; close old processes before removing staging/backup $runDirectory. $($_.Exception.Message)" }
    $bytes = ($entries | ForEach-Object { (Get-Item -LiteralPath (Join-Path $OutputDirectory $_.Path)).Length } | Measure-Object -Sum).Sum
    Write-Host ("Native package: {0}; {1:N2} MiB ({2} generated files). Logs: {3}" -f $OutputDirectory, ($bytes / 1MB), $entries.Count, $runLogs)
} catch {
    throw "Native publication failed in phase '$phase': $($_.Exception.Message) Staging/backup: $runDirectory"
} finally {
    if ($packageLock) { $packageLock.Dispose() }
    if ($lock) { $lock.Dispose() }
    Pop-Location
}

[CmdletBinding()]
param(
    [ValidateSet('win-x64', 'win-arm64')][string]$RuntimeIdentifier = 'win-x64',
    [ValidateSet('Skia', 'HarfBuzz')][string[]]$Component = @('Skia', 'HarfBuzz'),
    [string]$SourceRoot,
    [string]$OutputRoot,
    [string]$PythonExecutable = 'python',
    [switch]$PrepareOnly,
    [int]$Jobs = 4
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if (!$SourceRoot) { $SourceRoot = Join-Path $PSScriptRoot '../artifacts/native-graphics/sources' }
if (!$OutputRoot) { $OutputRoot = Join-Path $PSScriptRoot '../artifacts/native-graphics' }
[xml]$sourcePins = Get-Content (Join-Path $PSScriptRoot 'sources.props') -Raw
$pins = $sourcePins.Project.PropertyGroup
$SourceRoot = [IO.Path]::GetFullPath($SourceRoot)
$output = [IO.Path]::GetFullPath((Join-Path $OutputRoot $RuntimeIdentifier))
$arch = $RuntimeIdentifier.Substring(4)
New-Item -ItemType Directory -Force $SourceRoot, $output | Out-Null

function Invoke-Native([string]$Executable, [string[]]$Arguments) {
    & $Executable @Arguments | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "$Executable failed with exit code $LASTEXITCODE" }
}
function Get-Source([string]$Name, [string]$Repository, [string]$Commit, [string[]]$SparsePaths = @()) {
    $path = Join-Path $SourceRoot $Name
    if (!(Test-Path (Join-Path $path '.git'))) {
        New-Item -ItemType Directory -Force $path | Out-Null
        Invoke-Native git @('-C', $path, 'init')
        Invoke-Native git @('-C', $path, 'remote', 'add', 'origin', $Repository)
        Invoke-Native git @('-C', $path, 'fetch', '--depth=1', 'origin', $Commit)
        if ($SparsePaths.Count) { Invoke-Native git (@('-C', $path, 'sparse-checkout', 'set') + $SparsePaths) }
        Invoke-Native git @('-C', $path, '-c', 'advice.detachedHead=false', 'checkout', '--detach', $Commit)
    }
    $actual = & git -C $path rev-parse HEAD
    if ($LASTEXITCODE -ne 0 -or $actual -ne $Commit) { throw "Wrong source checkout at $path; expected $Commit. Use a fresh SourceRoot." }
    return $path
}


function Copy-Notices([string]$Repository, [string]$Destination) {
    $files = & git -C $Repository ls-tree -r --name-only HEAD
    if ($LASTEXITCODE -ne 0) { throw "Cannot enumerate licenses in $Repository" }
    $files = $files | Where-Object { (Split-Path $_ -Leaf) -match '^(LICENSE|COPYING|COPYRIGHT|NOTICE)' }
    foreach ($file in $files) {
        $source = Join-Path $Repository $file
        if (Test-Path $source -PathType Leaf) {
            $target = Join-Path $Destination $file
            New-Item -ItemType Directory -Force (Split-Path $target) | Out-Null
            Copy-Item $source $target -Force
        }
    }
}

function Write-Receipt([string]$Name, [string]$Commit, [string[]]$Libraries) {
    $receipt = [xml]'<Project><PropertyGroup/><ItemGroup/></Project>'
    $property = $receipt.CreateElement("WttsBuilt${Name}Source")
    $property.InnerText = $Commit
    [void]$receipt.SelectSingleNode('/Project/PropertyGroup').AppendChild($property)
    foreach ($libraryName in $Libraries) {
        $path = Join-Path $output $libraryName
        # A .lib suffix alone is insufficient: reject DLL import archives.
        $stream = [IO.File]::OpenRead($path)
        try {
            $magic = New-Object byte[] 8
            [void]$stream.Read($magic, 0, 8)
            if ([Text.Encoding]::ASCII.GetString($magic) -ne "!<arch>`n") { throw "Not a COFF archive: $path" }
            $objects = 0
            while ($stream.Position + 60 -le $stream.Length) {
                $header = New-Object byte[] 60
                [void]$stream.Read($header, 0, 60)
                $memberName = [Text.Encoding]::ASCII.GetString($header, 0, 16).Trim()
                $size = [long]::Parse([Text.Encoding]::ASCII.GetString($header, 48, 10).Trim())
                $next = $stream.Position + $size + ($size % 2)
                if ($memberName -notin @('/', '//', '/SYM64/') -and $size -ge 20) {
                    $object = New-Object byte[] 20
                    [void]$stream.Read($object, 0, 20)
                    # IMPORT_OBJECT_HEADER has 0/0xffff signatures and version 0;
                    # anonymous bigobj COFF uses version >= 2 and is a real object.
                    if ([BitConverter]::ToUInt32($object, 0) -eq [uint32]4294901760 -and [BitConverter]::ToUInt16($object, 4) -eq 0) {
                        throw "Import library is not static code: $path"
                    }
                    $machine = [BitConverter]::ToUInt16($object, 0)
                    if ([BitConverter]::ToUInt32($object, 0) -eq [uint32]4294901760) { $machine = [BitConverter]::ToUInt16($object, 6) }
                    $expected = if ($arch -eq 'x64') { 0x8664 } else { 0xaa64 }
                    if ($machine -ne $expected) { throw "Wrong COFF architecture in $path" }
                    $objects++
                }
                $stream.Position = $next
            }
            if (!$objects) { throw "No native code objects in $path" }
        } finally { $stream.Dispose() }
        $item = $receipt.CreateElement('_WttsStaticGraphicsArchive')
        $item.SetAttribute('Include', "`$(WttsStaticGraphicsRoot)/$libraryName")
        foreach ($entry in @{ Sha256 = (Get-FileHash $path -Algorithm SHA256).Hash; Component = $Name }.GetEnumerator()) {
            $metadata = $receipt.CreateElement($entry.Key)
            $metadata.InnerText = $entry.Value
            [void]$item.AppendChild($metadata)
        }
        [void]$receipt.SelectSingleNode('/Project/ItemGroup').AppendChild($item)
    }
    $receipt.Save((Join-Path $output "$Name.props"))
}
$skiaSharp = $null
$skia = $null
if ('Skia' -in $Component -or 'HarfBuzz' -in $Component) {
    $skiaSharp = Get-Source 'SkiaSharp' 'https://github.com/mono/SkiaSharp.git' $pins.WttsStaticSkiaSharpCommit @('native/windows/libHarfBuzzSharp')
    # The submodule SHA is pinned by the exact SkiaSharp release, not by a floating branch.
    $skia = Get-Source 'SkiaSharp/externals/skia' 'https://github.com/mono/skia.git' $pins.WttsStaticSkiaCommit @('third_party/harfbuzz')
    $harfBuzz = Get-Source 'SkiaSharp/externals/skia/third_party/externals/harfbuzz' 'https://github.com/harfbuzz/harfbuzz.git' $pins.WttsStaticHarfBuzzCommit
    if ('Skia' -in $Component) {
        Invoke-Native git @('-C', $skia, 'sparse-checkout', 'disable')
        Invoke-Native $PythonExecutable @('--version')
        Push-Location $skia
        $previousSkipEmsdk = $env:GIT_SYNC_DEPS_SKIP_EMSDK
        $env:GIT_SYNC_DEPS_SKIP_EMSDK = '1'
        try {
            # DEPS binds every third-party source to an immutable commit. The two
            # upstream fetchers bind GN/Ninja to the revisions recorded in sources.props.
            Invoke-Native $PythonExecutable @('tools/git-sync-deps')
            Invoke-Native $PythonExecutable @('bin/fetch-ninja')
        } finally {
            $env:GIT_SYNC_DEPS_SKIP_EMSDK = $previousSkipEmsdk
            Pop-Location
        }
    }
}

if ($PrepareOnly) { Write-Output "Prepared immutable native inputs in $SourceRoot; no native compilation was run."; return }

if ('Skia' -in $Component -or 'HarfBuzz' -in $Component) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
    $vs = & $vswhere -latest -products '*' -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
    if (!$vs) { throw 'Install Visual Studio C++ Build Tools and Windows SDK >= 10.0.22621.0.' }
    Import-Module (Join-Path $vs 'Common7/Tools/Microsoft.VisualStudio.DevShell.dll')
    Enter-VsDevShell -VsInstallPath $vs -SkipAutomaticLocation -DevCmdArguments "-arch=$arch -host_arch=x64"
    if ('HarfBuzz' -in $Component) {
        $obj = Join-Path $output 'harfbuzz.obj'
        $source = Join-Path $skiaSharp 'native/windows/libHarfBuzzSharp/harfbuzz-subset-msvc.cc'
        # Use the upstream translation unit/configuration, removing only dllexport:
        # no DLL/import library is created and all shaping/subsetting APIs stay available.
        Invoke-Native cl.exe @('/nologo', '/c', '/O2', '/MT', '/EHsc', '/std:c++17', '/bigobj', '/utf-8',
            '/DHAVE_CONFIG_OVERRIDE_H', '/DHAVE_OT', '/DHB_NO_FALLBACK_SHAPE', '/DHB_EXTERN=extern',
            "/I$(Join-Path $harfBuzz 'src')", "/I$(Join-Path $skia 'third_party/harfbuzz')", "/Fo$obj", $source)
        Invoke-Native lib.exe @('/nologo', "/OUT:$(Join-Path $output 'libHarfBuzzSharp.lib')", $obj)
        Remove-Item $obj
        Copy-Notices $harfBuzz (Join-Path $output 'licenses/HarfBuzz/upstream')
        Copy-Notices $skiaSharp (Join-Path $output 'licenses/HarfBuzz/SkiaSharp')
        Write-Receipt 'HarfBuzz' $pins.WttsStaticHarfBuzzCommit @('libHarfBuzzSharp.lib')
    }
    if ('Skia' -in $Component) {
        $build = Join-Path $skia "out/wtts-static-$arch"
        New-Item -ItemType Directory -Force $build | Out-Null
        $args = (Get-Content (Join-Path $PSScriptRoot 'args.win.gn') -Raw).Replace('target_cpu = "x64"', "target_cpu = `"$arch`"")
        $args += "`nwin_vc = `"$((Join-Path $vs 'VC').Replace('\', '/'))`"`n"
        [IO.File]::WriteAllText((Join-Path $build 'args.gn'), $args)
        Push-Location $skia
        try {
            Invoke-Native (Join-Path $skia 'bin/gn.exe') @('gen', $build, "--script-executable=$PythonExecutable")
            Invoke-Native (Join-Path $skia 'third_party/ninja/ninja.exe') @('-C', $build, '-j', "$Jobs", 'skia', 'SkiaSharp')
        } finally { Pop-Location }
        Copy-Item (Join-Path $build 'skia.lib'), (Join-Path $build 'SkiaSharp.lib') $output -Force
        Copy-Notices $skiaSharp (Join-Path $output 'licenses/Skia/SkiaSharp')
        Copy-Notices $skia (Join-Path $output 'licenses/Skia/skia')
        Get-ChildItem (Join-Path $skia 'third_party/externals') -Directory | ForEach-Object {
            if (Test-Path (Join-Path $_.FullName '.git')) { Copy-Notices $_.FullName (Join-Path $output "licenses/Skia/third_party/$($_.Name)") }
        }
        Write-Receipt 'Skia' $pins.WttsStaticSkiaCommit @('skia.lib', 'SkiaSharp.lib')
    }
}
Write-Output "Native static archives and SHA-256 receipts: $output"

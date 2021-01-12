function Main
{
    begin
    {
        $ErrorActionPreference = 'stop'
 
        $target = 'net472'
        $result = 'Steam++'
        $bin = '..\source\SteamTools\bin\x86\Release\'
 # '*.exe.config'
        $targetKeywords = '*.exe','*.dll','*.txt','*.VisualElementsManifest.xml','*.pak','*.bin','*.js','Log4net.config'
        $ignoreKeywords = '*.vshost.*','Microsoft.*.resources.dll','ExPlugin.*.dll'
 
        $exeSource  = '\Steam++.exe'
 
        if (-not(Test-Path $bin))
        {
            throw 'Script detected as locate in invalid path exception!! Make sure exist in <SteamTools repository root>\tools-release\'
        }
    }
 
    end
    {
        try
        {
            # clean up current
            Get-ChildItem -Directory | Remove-item -Recurse
            Get-ChildItem | where { $_.Extension -eq ".zip" } | Remove-Item
 
            Copy-StrictedFilterFileWithDirectoryStructure -Path $(Join-Path $bin $target) -Destination '.\' -Targets $targetKeywords -Exclude $ignoreKeywords
 
            # valid path check
            $versionSource = Join-Path $target $exeSource -Resolve
 
            if ((Test-Path $versionSource) -and (Test-Path $target))
            {
                $version = (Get-ChildItem $versionSource).VersionInfo
                $result  = $result + ' v{0}.{1}.{2}' -f $version.ProductMajorPart, $version.ProductMinorPart,$version.ProductBuildPart
 
                Rename-Item -NewName $result -Path $target
                New-ZipCompression -source $(Join-Path $(Get-Location) $result) -destination $(Join-Path $(Get-Location).Path ('./' + $result + '.zip'))
            }
        }
        catch
        {
            throw $_
        }
    }
}
 
 
# https://gist.github.com/guitarrapc/e78bbd4ddc07389e17d6
function Copy-StrictedFilterFileWithDirectoryStructure
{
    [CmdletBinding()]
    param
    (
        [parameter(
            mandatory = 1,
            position  = 0,
            ValueFromPipeline = 1,
            ValueFromPipelineByPropertyName = 1)]
        [string]
        $Path,
 
        [parameter(
            mandatory = 1,
            position  = 1,
            ValueFromPipelineByPropertyName = 1)]
        [string]
        $Destination,
 
        [parameter(
            mandatory = 1,
            position  = 2,
            ValueFromPipelineByPropertyName = 1)]
        [string[]]
        $Targets,
 
        [parameter(
            mandatory = 0,
            position  = 3,
            ValueFromPipelineByPropertyName = 1)]
        [string[]]
        $Excludes
    )
 
    begin
    {
        $list = New-Object 'System.Collections.Generic.List[String]'
    }
 
    process
    {
        Foreach ($target in $Targets)
        {
            # Copy "All Directory Structure" and "File" which Extension type is $ex
            Copy-Item -Path $Path -Destination $Destination -Force -Recurse -Filter $target
        }
    }
 
    end
    {
        # Remove -Exclude Item
        Foreach ($exclude in $Excludes)
        {
            Get-ChildItem -Path $Destination -Recurse -File | where Name -like $exclude | Remove-Item
        }
 
        # search Folder which include file
        $allFolder = Get-ChildItem $Destination -Recurse -Directory
        $containsFile = $allFolder | where {$_.GetFiles()}
        $containsFile.FullName `
        | %{
            $fileContains = $_
            $result = $allFolder.FullName `
            | where {$_ -notin $list} `
            | where {
                $shortPath = $_
                $fileContains -like "$shortPath*"
            }
            $result | %{$list.Add($_)}
        }
        $folderToKeep = $list | sort -Unique
 
        # Remove All Empty (none file exist) folders
        Get-ChildItem -Path $Destination -Recurse -Directory | where fullName -notin $folderToKeep | Remove-Item -Recurse
    }
}
 
# http://tech.guitarrapc.com/entry/2013/10/08/040325
function New-ZipCompression
{ 
    [CmdletBinding()]
    param
    (
        [parameter(
            mandatory,
            position = 0,
            valuefrompipeline,
            valuefrompipelinebypropertyname)]
        [string]
        $source,
 
        [parameter(
            mandatory = 0,
            position = 1,
            valuefrompipeline,
            valuefrompipelinebypropertyname)]
        [string]
        $destination,
 
        [parameter(
            mandatory = 0,
            position = 2)]
        [switch]
        $quiet,
 
        [parameter(
            mandatory = 0,
            position = 3)]
        [switch]
        $force
    )
 
    try
    {
        Add-Type -AssemblyName 'System.IO.Compression.FileSystem'
    }
    catch
    {
    }
 
    # check file is directory
    $file = Get-Item -Path $source
 
    # set zip extension
    $zipExtension = '.zip'
 
    # set desktop as destination path if it null
    if ([string]::IsNullOrWhiteSpace($destination))
    {
        if ($file.Mode -like 'd*')
        {
            $destination = (Join-Path ([System.Environment]::GetFolderPath([Environment+SpecialFolder]::Desktop)) [System.IO.Path]::GetFileNameWithoutExtension($source + $zipExtension))
        }
        else
        {
            $destination = (Join-Path ([System.Environment]::GetFolderPath([Environment+SpecialFolder]::Desktop)) [System.IO.Path]::GetFileNameWithoutExtension(($file | select -First 1 -ExpandProperty fullname))) + $zipExtension
        }
 
    }
 
    # check destination is input as .zip
    if (-not($destination.EndsWith($zipExtension)))
    {
        throw ('destination parameter value [{0}] not end with extension {1}' -f $destination, $zipExtension)
    }
 
    # check destination is already exist or not
    if (Test-Path $destination)
    {
        Remove-Item -Path $destination -Confirm
    }
 
 
    # compressionLevel
    $compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal
 
    # check file mode for source
    Write-Verbose ('file.mode = {0}' -f $file.Mode)
 
    if ($file.Mode -like 'd*') # Directory should be d---
    {
        try # create zip from directory
        {
            # force output zip file to new destination path, avoiding destination zip name conflict.
            if ($force)
            {
                # check destination is already exist, CreateFromDirectory will fail with same name of destination file.
                if (Test-Path $destination)
                {
                    # show warning for same destination exist.
                    Write-Verbose ('Detected destination name {0} is already exist. Force trying output to new destination zip name.' -f $destination)
 
                    # get current destination information
                    $destinationRoot = [System.IO.Path]::GetDirectoryName($destination)
                    $destinationfile = [System.IO.Path]::GetFileNameWithoutExtension($destination)
                    $destinationExtension = [System.IO.Path]::GetExtension($destination)
 
                    # renew destination name with (2)...(x) until no more same name catch.
                    $count = 2
                    $destination = Join-Path $destinationRoot ($destinationfile + '(' + $count + ')' + $destinationExtension)
                    while (Test-Path $destination)
                    {
                        ++$count
                        $destination = Join-Path $destinationRoot ($destinationfile + '(' + $count + ')' + $destinationExtension)
                    }
 
                    # show warning as destination name had been changed due to escape error.
                    Write-Verbose ('Deistination name change to new name {0}' -f $destination)
                }
            }
 
            # include BaseDirectory
            $includeBaseDirectory = $true
 
            Write-Verbose ('destination = {0}' -f $destination)
 
            Write-Verbose ('file.fullname = {0}' -f $file.FullName)
            Write-Verbose ('compressionLevel = {0}' -f $compressionLevel)
            Write-Verbose ('includeBaseDirectory = {0}' -f $includeBaseDirectory)
 
            if ($quiet)
            {
                Write-Verbose ('zipping up folder {0} to {1}' -f $file.FullName,$destination)
                [System.IO.Compression.ZipFile]::CreateFromDirectory($file.fullname,$destination,$compressionLevel,$includeBaseDirectory) > $null
                $?
            }
            else
            {
                Write-Verbose ('zipping up folder {0} to {1}' -f $file.FullName,$destination)
                [System.IO.Compression.ZipFile]::CreateFromDirectory($file.fullname,$destination,$compressionLevel,$includeBaseDirectory)
                Get-Item $destination
            }
        }
        catch
        {
            Write-Error $_
            $?
        }
    }
    else
    {
        try # create zip from files
        {
            # create zip to add
            $destzip = [System.IO.Compression.Zipfile]::Open($destination,'Update')
 
            # get items
            $files = Get-ChildItem -Path $source
 
            foreach ($file in $files)
            {
                $file2 = $file.name
 
                Write-Verbose ('destzip = {0}' -f $destzip)
                Write-Verbose ('file.fullname = {0}' -f $file.FullName)
                Write-Verbose ('file2 = {0}' -f $file2)
                Write-Verbose ('compressionLevel = {0}' -f $compressionLevel)
 
                if ($quiet)
                {
                    Write-Verbose ('zipping up files {0} to {1}' -f $file.FullName,$destzip)
                    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($destzip,$file.fullname,$file2,$compressionLevel) > $null
                    $?
                }
                else
                {
                    Write-Verbose ('zipping up files {0} to {1}' -f $file.FullName,$destzip)
                    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($destzip,$file.fullname,$file2,$compressionLevel)
                }
            }
        }
        catch
        {
            Write-Error $_
            $?
        }
        finally
        {
            # dispose opened zip
            $destzip.Dispose()
        }
    }
}
 
Main
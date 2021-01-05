param (
	[string]$Action="default",
	[hashtable]$properties=@{},
	[switch]$Help
)

function Install-Chocolatey()
{
	if(-not $env:ChocolateyInstall -or -not (Test-Path "$env:ChocolateyInstall\*"))
	{
		Write-Output "Chocolatey Not Found, Installing..."
		iex ((new-object net.webclient).DownloadString('http://chocolatey.org/install.ps1')) 
	}
	$env:Path += ";${env:ChocolateyInstall}"
}

function Install-Psake()
{
	if(!(Test-Path $env:ChocolateyInstall\lib\Psake\tools\Psake*)) 
	{ 
		choco install psake -y
	}
}

function Install-Git()
{
	if(!((Test-Path ${env:ProgramFiles(x86)}\Git*) -Or (Test-Path ${env:ProgramFiles}\Git*))) 
	{ 
		choco install git.install	
	}
	$env:Path += ";${env:ProgramFiles(x86)}\Git"
	$env:Path += ";${env:ProgramFiles}\Git"
}

function Install-DocFx()
{
	if(!(Test-Path $env:ChocolateyInstall\lib\docfx\tools*)) 
	{ 
		choco install docfx --version 2.55
	}
	$env:Path += ";$env:ChocolateyInstall\lib\docfx\tools"
}

#current directory
$Here = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$ScriptPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
$SolutionRoot = Split-Path -Parent $ScriptPath
$ToolsPath = Join-Path -Path $SolutionRoot -ChildPath "lib"

if(-not $env:ChocolateyInstall)
{
	$env:ChocolateyInstall = "${env:ALLUSERSPROFILE}\chocolatey";
}
	
Install-Chocolatey

Install-Psake

Install-Git

Install-DocFx

$psakeDirectory = (Resolve-Path $env:ChocolateyInstall\lib\Psake*)

#appveyor for some reason have different location for psake (it has older psake version?)
if(Test-Path $psakeDirectory\tools\Psake\Psake.psm*)
{
	Import-Module (Join-Path $psakeDirectory "tools\Psake\Psake.psm1")
}
else
{
	Import-Module (Join-Path $psakeDirectory "tools\Psake.psm1")
}


#invoke the task
Invoke-Psake -buildFile "$Here\build.ps1" -parameters $properties -tasklist $Action

# Self-elevate the script if required.
if (-Not ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] 'Administrator')) {
	if ([int](Get-CimInstance -Class Win32_OperatingSystem | Select-Object -ExpandProperty BuildNumber) -ge 6000) {
		$CommandLine = "-File `"" + $MyInvocation.MyCommand.Path + "`" " + $MyInvocation.UnboundArguments
		Start-Process -FilePath PowerShell.exe -Verb Runas -ArgumentList $CommandLine
		Exit
	}
}

# This is the name of the service and will also show as the display name in services.msc.
# This must match what the service was installed as.
$ServiceName = "ProxyService"

# Make sure the service is stopped.
Stop-Service -Name $ServiceName
# Remove the service, this doesnt always work. (Requires PS 6+)
Remove-Service -Name $ServiceName
# Make sure the service gets unregistered even if the last command failed.
sc.exe delete $ServiceName

Read-Host -Prompt "Press Enter to exit"
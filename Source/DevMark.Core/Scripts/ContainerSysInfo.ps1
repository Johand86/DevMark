Param([boolean]$trace = $false)

##<CommandFunctions.ps1>##
$global:LASTEXITCODE = 0

$osArchitecture = $null
$osName = $null
$osVersion = $null
$osPlatform = $null

if ($IsLinux) {

	$archRegex = new-object System.Text.RegularExpressions.Regex("Architecture:\s+(?<arch>\S+)")
	$lscpu = lscpu
	$matches =$archRegex.Match($lscpu)
	$osArchitecture = $matches.Groups["arch"].Value

	$osName = (lsb_release -s -i).ToString()
	$osVersion = (lsb_release -s -r).ToString()
	$osPlatform = "Linux"

}

if ($IsWindows) {
	
	$info = Get-ComputerInfo
	$osVersion =$info.WindowsVersion
	$osName = $info.WindowsProductName
	$osArchitecture = $info.OsArchitecture
	$osPlatform = "Windows"
}


$containerInfo = @{
					LogicalProcessors = [Environment]::ProcessorCount;
					Memory = [GC]::GetTotalMemory($false)
					OS = @{
						Architecture = $osArchitecture;
						Name = $osName;
						Platform = $osPlatform;
						Version = $osVersion
					}
}

return @{ ExitCode = $LASTEXITCODE; ContainerInfo = $containerInfo }
Param([boolean]$trace = $false)

$global:LASTEXITCODE = 0

$sysInfo = new-object DevMark.Core.Hardware.SysInfo 

$sysInfo.OS = new-object DevMark.Core.Hardware.OsInfo
$sysInfo.OS.Name = "Unknown - platform not implemented"

return @{ ExitCode = $LASTEXITCODE }
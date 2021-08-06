Param([boolean]$trace = $false)

##<CommandFunctions.ps1>##
$global:LASTEXITCODE = 0

$sysInfo = new-object DevMark.Model.SystemInformation.SysInfo 

$info = Get-ComputerInfo
$partitions = Get-Partition
$ramInfo = Get-CimInstance -ClassName win32_physicalmemory
$gpuInfo = Get-CimInstance -ClassName CIM_VideoController
$batteryInfo = Get-CimInstance Win32_Battery

$disks = @{}

foreach ($partition in $partitions) {

	$driveLetter = $partition.DriveLetter.ToString()
	if ($driveLetter -ne "") {
		
		$d = $partition | Get-Disk
		$pd = $d | Get-PhysicalDisk

		if ($d.UniqueId) {

			$disk = $disks[$d.UniqueId]

			if ($disk) {
				$disk.DrivePaths.Add($driveLetter) | Out-Null
				continue
			}

			$disk = new-object DevMark.Model.SystemInformation.DiskInfo
			if ($pd -ne $null) {
				$disk.Manufacturer = $pd.Manufacturer
				$disk.Model = $pd.Model
				$disk.Size = $pd.Size / 1000 / 1000
				$disk.BusType = $pd.BusType
				$disk.MediaType = $pd.MediaType
			}
			else {
				$disk.Manufacturer = $d.Manufacturer
				$disk.Model = $d.Model
				$disk.Size = $d.Size / 1000 / 1000
				if ($d.UniqueId.StartsWith("USBSTOR")) {
					$disk.BusType = "USB"
				}
			}
			$disk.DrivePaths.Add($driveLetter) | Out-Null
			$sysInfo.Disks.Add($disk) | Out-Null

		}
	}
}

foreach ($module in $ramInfo) {

	$ramInfo = new-object DevMark.Model.SystemInformation.RamInfo
	$ramInfo.Size = $module.Capacity / 1024 / 1024
	$ramInfo.ClockSpeed = $module.ConfiguredClockSpeed
	$ramInfo.Voltage = $module.ConfiguredVoltage
	$ramInfo.Model = $module.PartNumber.Trim()
	$sysInfo.RAM.Add($ramInfo) | Out-Null
}

foreach ($cpu in $info.CsProcessors) {

	$cpuInfo = new-object DevMark.Model.SystemInformation.CpuInfo
	$cpuInfo.Name = $cpu.Name.Trim()
	$cpuInfo.Description = $cpu.Description
	$cpuInfo.Architecture = $cpu.Architecture
	$cpuInfo.MaxClockSpeed = $cpu.MaxClockSpeed
	$cpuInfo.CurrentClockSpeed = $cpu.CurrentClockSpeed
	$cpuInfo.Cores = $cpu.NumberOfCores
	$cpuInfo.LogicalProcessors = $cpu.NumberOfLogicalProcessors
	$cpuInfo.Socket = $cpu.SocketDesignation 
	$sysInfo.CPUs.Add($cpuInfo) | Out-Null
}


foreach ($gpu in $gpuInfo) {

	$gpuInfo = new-object DevMark.Model.SystemInformation.GpuInfo
	$gpuInfo.Name = $gpu.Name
	$gpuInfo.Driver = $gpu.DriverVersion
	$gpuInfo.Memory = $gpu.AdapterRAM / 1024 / 1024
	$sysInfo.GPUs.Add($gpuInfo) | Out-Null
}

$sysInfo.Motherboard = new-object DevMark.Model.SystemInformation.MotherboardInfo
$sysInfo.Motherboard.Manufacturer =  $info.CsManufacturer
$sysInfo.Motherboard.Model =  $info.CsModel
$sysInfo.Motherboard.Bios = $info.BiosName 

$sysInfo.OS = new-object DevMark.Model.SystemInformation.OsInfo

$sysInfo.OS.Version =$info.WindowsVersion
$sysInfo.OS.Name = $info.WindowsProductName
$sysInfo.OS.Architecture = $info.OsArchitecture
$sysInfo.OS.Platform = "Windows"

$sysInfo.PowerSupply =  new-object DevMark.Model.SystemInformation.PowerSupplyInfo
if ($batteryInfo -ne $null -and $batteryInfo.BatteryStatus -eq 1) {
	$sysInfo.PowerSupply.AcPower = $false
}
else {
	$sysInfo.PowerSupply.AcPower = $true
}

return @{ ExitCode = $LASTEXITCODE; SysInfo = $sysInfo }
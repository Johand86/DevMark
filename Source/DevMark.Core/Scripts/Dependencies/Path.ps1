Param([Parameter(Mandatory=$true)][string]$workDir, [string]$requiredLength, [boolean]$trace = $false)

##<CommandFunctions.ps1>##
$global:LASTEXITCODE = 0
SetWorkDirectory -workDir $workDir

$mainWorkDir = Split-Path -parent $workDir

$readmeFileAttributes = Get-Item "$mainWorkDir\readme.txt" | select-object -ExpandProperty Attributes
$readmeAttributes = [Int32]$readmeFileAttributes

$FILE_ATTR_ARCHIVE = 0x20
$FILE_ATTR_ALWAYS_AVAILABLE = 0x80000

$FILE_ATTR_LOCALLY_AVAILABLE = 0x400
$FILE_ATTR_OFFLINE = 0x1000

if ((($readmeAttributes -band $FILE_ATTR_LOCALLY_AVAILABLE) -ne 0) -or (($readmeAttributes -band $FILE_ATTR_OFFLINE) -ne 0)) {
	Write-Warning "The path `"$mainWorkDir`" appears to be syncing with OneDrive. This can have a severe performance impact."
	$LASTEXITCODE = -1
}

$testSuiteDirName = Split-Path -leaf $workDir

$winRemainingPathChars = 260 - ($mainWorkDir.Length + "/".Length) - ($testSuiteDirName.Length + "/".Length)

if ($winRemainingPathChars -lt $requiredLength) {
	Write-Warning "The test requires a file path of $requiredLength characters in length to work. The work directory path `"$workDir`" is to long to fit this."
	$LASTEXITCODE = -1
}

$localDiskFound = $false
$diskDriveLetters = Get-Partition | Select-Object -ExpandProperty DriveLetter
foreach ($driveLetter in $diskDriveLetters) {
	if ($workDir.ToLower().StartsWith("$($driveLetter):".ToLower())) {
		$localDiskFound = $true
	}
}

if (!$localDiskFound) {
	Write-Warning "Could not find the local disk mapping to path `"$workDir`" Using a network drive can have a severe performance impact."
	$LASTEXITCODE = -1
}

if ($LASTEXITCODE -ne 0) {
	Write-Warning "Use the --work-directory <path> or -w <path> for short to change the directory."
}


return @{ ExitCode = $LASTEXITCODE }

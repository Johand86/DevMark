Param([Parameter(Mandatory=$true)][string]$workDir, [string]$minVersion, [string]$maxVersion, $requiredComponents = $null, [boolean]$trace = $false)

##<CommandFunctions.ps1>##
$global:LASTEXITCODE = 0
SetWorkDirectory -workDir $workDir

$command = get-command git -ErrorAction SilentlyContinue

$depVersions = @{}
$envPaths = @{}

if ($command -ne $null) {

	$compatibleVersion = CompatibleVersion -version $command.Version -min $minVersion -max $maxVersion

	if ($compatibleVersion) {
		$envPaths.git = $command.Path
		$depVersions["git"] = $command.Version.ToString()
	}
} 

if (!$envPaths.git) {

	if ($command) {

		$expectString = FormatExpectedString -min $minVersion -max $maxVersion

		Write-Warning "Git version $($command.Version) was found. Expected $expectString."
	}

	$LASTEXITCODE = -1
}

return @{ ExitCode = $LASTEXITCODE; Environment = $envPaths; Dependencies = $depVersions }

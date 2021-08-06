Param([Parameter(Mandatory=$true)][string]$workDir, [string]$minVersion, [string]$maxVersion, [boolean]$trace = $false)

##<CommandFunctions.ps1>##
$global:LASTEXITCODE = 0
SetWorkDirectory -workDir $workDir

$command = get-command git -ErrorAction SilentlyContinue

$depVersions = @{}
$envPaths = @{}

if ($command -ne $null) {

	$version = $command.Version.ToString()
	if ($version -eq "0.0.0.0") {

		# Version is not set in unix. We can try to resolve by invoking.
		$versionString = git --version
		$expr = new-Object System.Text.RegularExpressions.Regex("(\d\.?)(\d\.?)?(\d\.?)?(\d\.?)?")
		$version = $expr.Match($versionString).Value
	}

	$compatibleVersion = CompatibleVersion -version $version -min $minVersion -max $maxVersion

	if ($compatibleVersion) {
		$envPaths.git = $command.Path
		$depVersions["git"] = $version
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

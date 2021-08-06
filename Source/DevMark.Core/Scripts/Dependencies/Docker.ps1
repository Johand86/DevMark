Param([string]$minVersion, [string]$maxVersion, [boolean]$trace = $false)

##<CommandFunctions.ps1>##
$global:LASTEXITCODE = 0

$command = get-command docker

$depVersions = @{}
$envPaths = @{}

$version = $null
$context = $null

if ($command -ne $null) {

	$dockerInfo = docker version

	$versionRegex = new-object System.Text.RegularExpressions.Regex("Version:\s+(?<version>\S+)")
	$contextRegex = new-object System.Text.RegularExpressions.Regex("Context:\s+(?<context>\S+)")
	$engineOsRegex = new-object System.Text.RegularExpressions.Regex("Server[\s\S]*OS\/Arch:\s+(?<engineos>\S+)") #TODO: make sure we're not matching client if the order of "docker version" output is changed.

	$version = $versionRegex.Match($dockerInfo).Groups["version"].Value
	$context = $contextRegex.Match($dockerInfo).Groups["context"].Value
	$engineOs = $engineOsRegex.Match($dockerInfo).Groups["engineos"].Value

	$compatibleVersion = CompatibleVersion -version $version -min $minVersion -max $maxVersion

	if ($compatibleVersion) {
		$envPaths.docker = $command.Path
		$depVersions["docker"] = $version
	}
} 

if (!$envPaths.docker) {

	if ($command) {

		$expectString = FormatExpectedString -min $minVersion -max $maxVersion

		Write-Warning "Docker version $version was found. Expected $expectString."
	}

	$LASTEXITCODE = -1
}

return @{ ExitCode = $LASTEXITCODE; Environment = $envPaths; Dependencies = $depVersions; Context = $context; EngineOs = $engineOs }

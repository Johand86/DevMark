Param([string]$minVersion, [string]$maxVersion, [string]$version, [boolean]$trace = $false)

##<CommandFunctions.ps1>##
$global:LASTEXITCODE = 0

$depVersions = @{}

$compatibleVersion = CompatibleVersion -version $version -min $minVersion -max $maxVersion
if (!$compatibleVersion) {

	$expectString = FormatExpectedString -min $minVersion -max $maxVersion

	Write-Warning "DevMark version is $version. This test requires $expectString."
	Write-Warning "Go to https://www.devbenchmark.com for update instructions."
	
	$LASTEXITCODE = -1
}

$depVersions["DevMark"] = $version

return @{ ExitCode = $LASTEXITCODE; Dependencies = $depVersions }

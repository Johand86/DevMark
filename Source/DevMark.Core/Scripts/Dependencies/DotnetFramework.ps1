Param([string]$minVersion, [string]$maxVersion, [boolean]$trace = $false)

##<CommandFunctions.ps1>##
$global:LASTEXITCODE = 0
$depVersions = @{}

$version = ""
$regVersion = (Get-ItemProperty "HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full").Release

if ($regVersion -ge 528040) {
    $version = "4.8"
}
elseif ($regVersion -ge 461808) {
    $version = "4.7.2"
}
elseif ($regVersion -ge 461308) {
    $version = "4.7.1"
}
elseif ($regVersion -ge 460798) {
    $version = "4.7"
}
elseif ($regVersion -ge 394802){
    $version = "4.6.2"
}
elseif ($regVersion -ge 394254) {
    $version = "4.6.1"
}
elseif ($regVersion -ge 393295) {
    $version = "4.6"
}
elseif ($regVersion -ge 379893) {
    $version = "4.5.2"
}
elseif ($regVersion -ge 378675) {
    $version = "4.5.1"
}
elseif ($regVersion -ge 378389) {
    $version = "4.5"
}


$compatibleVersion = CompatibleVersion -version $version -min $minVersion -max $maxVersion
if (!$compatibleVersion) {

	$expectString = FormatExpectedString -min $minVersion -max $maxVersion

	Write-Warning "Dotnet Framework version $version is installed. This test requires $expectString."
	Write-Warning "Go to https://dotnet.microsoft.com/download/dotnet-framework for update instructions."
	
	$LASTEXITCODE = -1
}
else {
	$depVersions["dotnetFramework"] = $version
}


return @{ ExitCode = $LASTEXITCODE; Dependencies = $depVersions  }

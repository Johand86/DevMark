Param([Parameter(Mandatory=$true)][string]$workDir, [string]$minVersion, [string]$maxVersion, [boolean]$nativeCppModules, [boolean]$nativePythonModules, [DevMark.NPMDepdendencyConfiguration]$npm, [boolean]$trace = $false)

##<CommandFunctions.ps1>##
##<func:Dependencies\VisualStudio.ps1>##

$global:LASTEXITCODE = 0
SetWorkDirectory -workDir $workDir
$envPaths = @{}
$depVersions = @{}

function GetSuggestedVersionString($min, $max) {
	if (!$max) {
		return ""
	}
	if ($min) {
		return "@~$min"
	}
	return "@$max"
}

$nodeCommand = get-command node -ErrorAction SilentlyContinue
$npmCommand = get-command npm -ErrorAction SilentlyContinue
$pythonCommand = get-command python -ErrorAction SilentlyContinue

$checkFailed = $false


if ($nodeCommand) {

	$nodeVersion = & $nodeCommand.Path --version
	if (CompatibleVersion -Version $nodeVersion -Min $minVersion -Max $maxVersion) {
		$envPaths.NodeJS = $nodeCommand.Path
		$depVersions["NodeJS"] = $nodeVersion.ToString().TrimStart("v")
	}
	else {
	
		$expectString = FormatExpectedString -Min $minVersion -Max $maxVersion
		Write-Warning "NodeJS $nodeVersion was found. Expected $expectString."
		Write-Warning "Please update NodeJS by downloading a later version from https://nodejs.org/en/download/"
		$checkFailed = $true
	}
} else {

	Write-Warning "NodeJS was not found."
	Write-Warning "You can download it from https://nodejs.org/en/download/ . Make sure add to PATH is selected during installation."
	$checkFailed = $true
}

if ($npmCommand) {

	$npmVersion = & $npmCommand.Path -version
	if (CompatibleVersion -Version $npmVersion -Min $npm.MinVersion -Max $npm.MaxVersion) {
		$envPaths.NPM = $npmCommand.Path
		$depVersions["NPM"] = $npmVersion.ToString()
	}
	else {
		$expectString = FormatExpectedString -Min $npm.MinVersion -Max $npm.MaxVersion
		Write-Warning "NPM $npmVersion was found. Expected $expectString."
		Write-Warning "Please update NPM by downloading a later version from https://nodejs.org/en/download/"
		$checkFailed = $true
	}
} else {

	Write-Warning "NPM was not found."
	Write-Warning "You can install it by download NodeJS from https://nodejs.org/en/download/ . Make sure add to PATH is selected during installation."
	$checkFailed = $true
}

if (!$checkFailed) {

	if ($npm.GlobalModules) {
	
		$npmGlobalModuleString = & $npmCommand.Path list -g
		$moduleExpr = new-object System.Text.RegularExpressions.Regex("(?<Module>\S+@[\S]+)")
		$matches = $moduleExpr.Matches($npmGlobalModuleString)

		$globalModules = @{}

		foreach ($match in $matches) {

			$matchValueStr = $match.Groups["Module"].Value
			if ($matchValueStr.Contains("@")) {
				$matchValue = $matchValueStr.Split("@")
				$globalModules[$matchValue[0]] = $matchValue[1]
			}
		}

		foreach ($module in $npm.GlobalModules) {
			
			$suggestedInstallString = $module.name + (GetSuggestedVersionString -min $module.MinVersion -max $module.MaxVersion)

			$globalModule = $globalModules[$module.name]
			if (!$globalModule) {
				Write-Warning "NPM global module $($module.name) was not found."
				Write-Warning "You can install it by running the following command: npm install -g $suggestedInstallString"
				$checkFailed = $true
			}
			elseif (!(CompatibleVersion -Version $globalModule -Min $module.MinVersion -Max $module.MaxVersion)) {
			
				$expectString = FormatExpectedString -Min $module.MinVersion -Max $module.MaxVersion
				Write-Warning "NPM global module $($module.name) version $globalModule was found. Expected $expectString."
				Write-Warning "You can update it by running the following command: npm update -g $suggestedInstallString"
				$checkFailed = $true
			}
			else {
				$depVersions[$module.name] = $globalModule
			}
			
		}
	}
}

if ($nativeCppModules) {

	$components = [System.Collections.ArrayList]@()
	$components.Add("Microsoft.VisualStudio.Component.Roslyn.Compiler") | Out-Null
	$components.Add("Microsoft.Component.MSBuild") | Out-Null
    $components.Add("Microsoft.VisualStudio.Component.VC.Tools.x86.x64") | Out-Null

	# Other components installed by the NodeJS tool, might be required for some packages?
	# The nodejs installer adds the CVTools workload for VS2017 Build Tools. Not an ideal check since it's not available in any of the full VS versions.
	#$components.Add("Microsoft.VisualStudio.Workload.VCTools")
	#$components.Add("Microsoft.VisualStudio.Component.CoreBuildTools")
    #$components.Add("Microsoft.VisualStudio.Workload.MSBuildTools")
    #$components.Add("Microsoft.VisualStudio.Component.Windows10SDK")
    #$components.Add("Microsoft.VisualStudio.Component.VC.CoreBuildTools")
    #$components.Add("Microsoft.VisualStudio.Component.Static.Analysis.Tools")
    #$components.Add("Microsoft.VisualStudio.Component.VC.Redist.14.Latest")
    #$components.Add("Microsoft.VisualStudio.Component.Windows10SDK.17763")
    #$components.Add("Microsoft.VisualStudio.Component.VC.CMake.Project")
    #$components.Add("Microsoft.VisualStudio.Component.TestTools.BuildTools")
    
	$vsDeps = VisualStudio -workDir $workDir -requiredComponents $components -printInstallInstructions $false

	# We require a Windows SDK but vswhere doesnt support querying for any, so we cant use it to check that dependency.
	$winSdk = Get-Item "hklm:\SOFTWARE\WOW6432Node\Microsoft\Microsoft SDKs\Windows\v10.0" -ErrorAction:SilentlyContinue
	if ($winSdk -eq $null) {
		Write-Warning "Failed to find Windows 10 SDK (this script currently do not check for older SDKs). A Windows SDK is required for native NodeJS modules. Run install_tools.bat from the nodejs folder to install it."
		$checkFailed = $true
	}

	if ($vsDeps.ExitCode -ne 0) {
		Write-Warning "Visual Studio with the required components for native NodeJS modules could not be found. Run install_tools.bat from the nodejs install folder to add them."
		Write-Warning "You could also add the following components to an existing installation: Microsoft.VisualStudio.Component.Roslyn.Compiler, Microsoft.Component.MSBuild, Microsoft.VisualStudio.Component.VC.Tools.x86.x64. Microsoft.VisualStudio.Component.Windows10SDK.<version>"
		Write-Warning "A list of component names with their IDs can be found here: https://docs.microsoft.com/en-us/visualstudio/install/workload-and-component-ids?view=vs-2019"
		$checkFailed = $true
	}
}

if ($nativePythonModules) {
	$pythonFound = $false

	if ($pythonCommand) {
		
		$version = $pythonCommand.Version
		$v = $version.ToString()
		if ($v -ne "0.0.0.0") { # 0.0.0.0 = Shortcut to the Windows store version of Python!
			$depVersions["Python"] = $v
			$pythonFound = $true
		}
	}

	if (!$pythonFound) {
		$checkFailed = $true
		Write-Warning "Python is required and must be available in the PATH variable for this test to work. Please download Python from https://www.python.org/downloads/"
		Write-Warning "If you've installed Python through Visual Studio you should add `"C:\Program Files (x86)\Microsoft Visual Studio\Shared\Python37_64`" to computers PATH variable (or the users PATH variable, before the Windows store shortcut)."
	}
}


if ($checkFailed) {
	$LASTEXITCODE = -1
}

return @{ ExitCode = $LASTEXITCODE; Environment = $envPaths; Dependencies = $depVersions }

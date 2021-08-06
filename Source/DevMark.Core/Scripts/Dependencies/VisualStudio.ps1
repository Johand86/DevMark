Param([Parameter(Mandatory=$true)][string]$workDir, [string]$minVersion, [string]$maxVersion, $requiredComponentSets = $null, [boolean]$trace = $false, [boolean]$printInstallInstructions = $true)

##<CommandFunctions.ps1>##
$global:LASTEXITCODE = 0
SetWorkDirectory -workDir $workDir
$depVersions = @{}
$envPaths = @{}

$vsWherePath = "$($env:ProgramFiles) (x86)\Microsoft Visual Studio\Installer\vswhere.exe"

if (!(Test-Path -Path $vsWherePath -PathType Leaf)) {
	$vsWherePath = "$workDir\vswhere.exe"
}

if (!(Test-Path -Path $vsWherePath -PathType Leaf)) {
	echo "vswhere not found, downloading..."
	Invoke-WebRequest -Uri "https://github.com/microsoft/vswhere/releases/download/2.8.4/vswhere.exe" -OutFile $vsWherePath
}


function TryWriteWarning ($message) {
	if ($printInstallInstructions) {
		Write-Warning $message
	}
}

if ($requiredComponentSets -eq $null -or $requiredComponentSets.length -eq 0) {
	$requiredComponentSets = @($null)
}

foreach ($componentSet in $requiredComponentSets) {

	$requireParam = ""
	if ($componentSet -ne $null) {
			$requireParam = "-requires"
		foreach ($component in $componentSet.Components) {
			$requireParam += " $component"
		}
	}

	$versionParam = ""
	$ver = ""
	if ($minVersion -or $maxVersion) {
	
		if ($minVersion -and $maxVersion) {
			$ver = "[$minVersion,$maxVersion]"
		}
		elseif ($minVersion) {
			$ver = "[$minVersion"
		}
		elseif ($maxVersion) {
			$ver = "$maxVersion]"
		}

		$versionParam = "-version `"$ver`""
	}
	# Component ID list: https://docs.microsoft.com/en-us/visualstudio/install/workload-and-component-ids?view=vs-2019

	# Run command

	$msBuildExpr = "& `"$vsWherePath`" $versionParam $requireParam -products * -Find MsBuild\**\bin\msbuild.exe"
	$vsPathExpr = "& `"$vsWherePath`" $versionParam $requireParam -products * -property installationPath"
	$vsVersionExpr = "& `"$vsWherePath`" $versionParam $requireParam -products * -property installationVersion"

	if ($trace) {
		Write-Host $msBuildExpr
	}

	$msBuild = InvokeExpression $msBuildExpr -PrintInfoOutput:$trace -PrintErrorOutput:$true
	$msBuildFound = $LASTEXITCODE -eq 0 -and $msBuild -ne $null -and (Test-Path -Path $msBuild -PathType Leaf)

	if ($trace) {
		Write-Host $vsPathExpr
	}

	$vsPath = InvokeExpression $vsPathExpr -PrintInfoOutput:$trace -PrintErrorOutput:$true
	$vsFound = $LASTEXITCODE -eq 0 -and $vsPath -ne $null -and (Test-Path -Path $vsPath)

	$vsVersion = InvokeExpression $vsVersionExpr -PrintInfoOutput:$trace -PrintErrorOutput:$true

	$vsTest = "$vsPath\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"
	$vsTestFound = $vsFound -and (Test-Path -Path $vsTest -PathType Leaf)

	#VC: https://github.com/microsoft/vswhere/wiki/Find-VC

	if ($vsFound) {
		break
	}
}

if (!$vsFound) {

	$versionString = ""
	if ($ver) {
		$versionString = " `"$ver`""
	}
	TryWriteWarning "Visual Studio$versionString with the required components could not be found."
	TryWriteWarning ""

	
	$componentInstallString = ""
	if ($requiredComponentSets) {
		TryWriteWarning "Make sure the following components are installed:"

		$firstSet = $true
		foreach ($componentSet in $requiredComponentSets) { 

			if (!$firstSet) {
				TryWriteWarning ""
				TryWriteWarning "Or (depending on VS edition):"
			}

			foreach ($component in $componentSet.Components) {
				TryWriteWarning "- $component"
				if ($firstSet) {
					if ($component.Contains(".Workload.")) {
						$componentInstallString += "--add $component;includeRecommended "
					}
					else {
						$componentInstallString += "--add $component "
					}
				}
			}
			$firstSet = $false
		}
		
		TryWriteWarning ""
		TryWriteWarning "A list of component names with their IDs can be found here: https://docs.microsoft.com/en-us/visualstudio/install/workload-and-component-ids?view=vs-2019"
		
		TryWriteWarning ""
		TryWriteWarning "If you're adding components using command line, you may run somthing similar to the following:'"
		TryWriteWarning "vs_<version>.exe modify --wait $componentInstallString"
	}
	TryWriteWarning "You can download the installer here: https://visualstudio.microsoft.com/downloads/"

	$LASTEXITCODE = -1
}

if ($msBuildFound) {
	$envPaths.MsBuild = $msBuild
}

if ($vsFound) {
	$envPaths.VsPath = $vsPath
	$depVersions["VisualStudio"] = $vsVersion
}

if ($vsTestFound) {
	$envPaths.VsTest = $vsTest
}

return @{ ExitCode = $LASTEXITCODE; Environment = $envPaths; Dependencies = $depVersions }

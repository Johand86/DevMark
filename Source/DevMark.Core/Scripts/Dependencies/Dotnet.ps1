Param([DevMark.DotnetDependencyConfiguration]$config, [boolean]$trace = $false)

##<CommandFunctions.ps1>##
$global:LASTEXITCODE = 0
$depVersions = @{}

$runtimes = dotnet --list-runtimes
$sdks = dotnet --list-sdks
$dotnetInfo = dotnet --info

$versionRegex = new-object System.Text.RegularExpressions.Regex("Host.+Version: (?<Version>[\d\.]+)", [System.Text.RegularExpressions.RegexOptions]::Singleline)
$version = $versionRegex.Match($dotnetInfo).Groups["Version"].Value

if (!(CompatibleVersion -version $version -min $config.MinVersion -max $config.MaxVersion)) {
	$expectString = FormatExpectedString -min $config.MinVersion -max $config.MaxVersion
	Write-Warning "Dotnet version $version is installed. This test requires $expectString."
	Write-Warning "Go to https://dotnet.microsoft.com/download for install instructions."
	$LASTEXITCODE = -1
}

if ($config.Sdks -ne $null) {
	foreach ($sdkConfig in $config.Sdks) {
		$sdkFound = $false
		
		foreach ($installedSdk in $sdks) {
			if ($installedSdk.Contains(" ")) {
				$installedSdkVersion = $installedSdk.Split(" ")[0]

				if (CompatibleVersion -version $installedSdkVersion -min $sdkConfig.MinVersion -max $sdkConfig.MaxVersion) {
					$sdkFound = $true
				}
			}
		}

		if (!$sdkFound) {

			$expectString = FormatExpectedString -min $sdkConfig.MinVersion -max $sdkConfig.MaxVersion
			Write-Warning "Could not find the expected dotnet SDK version: $expectString."
			Write-Warning "Go to https://dotnet.microsoft.com/download for install instructions."
			$LASTEXITCODE = -1
		}
	}
}

if ($config.Runtimes -ne $null) {
	foreach ($runtimeConfig in $config.Runtimes) {
		$runtimeFound = $false
		
		foreach ($installedRuntime in $runtimes) {
			if ($installedRuntime.Contains(" ")) {

				$runtimeSplit = $installedRuntime.Split(" ")
				if ($runtimeSplit.Count -ge 2) {
					$installedRuntimeName = $runtimeSplit[0]
					$installedRuntimeVersion = $runtimeSplit[1]

					if ($installedRuntimeName -eq $runtimeConfig.Name) {
						if (CompatibleVersion -version $installedRuntimeVersion -min $runtimeConfig.MinVersion -max $runtimeConfig.MaxVersion) {
							$runtimeFound = $true
						}
					}
				}
			}
		}

		if (!$runtimeFound) {

			$expectString = FormatExpectedString -min $runtimeConfig.MinVersion -max $runtimeConfig.MaxVersion
			Write-Warning "Could not find the expected runtime $($runtimeConfig.Name) with version $expectString."
			Write-Warning "Go to https://dotnet.microsoft.com/download for install instructions."
			$LASTEXITCODE = -1
		}
	}
}

$depVersions["dotnet"] = $version

return @{ ExitCode = $LASTEXITCODE; Dependencies = $depVersions }

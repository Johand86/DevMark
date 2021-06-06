
function CompatibleVersion($version, $min, $max) {

	$version = $version.ToString() # Could be System.Version or string.

	if (!$version.Contains(".")) {
		$version += ".0"
	}

	$v = new-object System.Version($version.TrimStart("v"))

	if ($min) {

		if (!$min.Contains(".")) {
			$min += ".0"
		}

		$mv = new-object System.Version($min)
		if ($mv.CompareTo($v) -gt 0) {
			return $false
		}
	}

	if ($max) {

		if (!$max.Contains(".")) {
			$max += "." + [Int32]::MaxValue
		}

		$mv = new-object System.Version($max)
		if ($mv.CompareTo($v) -lt 0) {
			return $false
		}
	}
	return $true
}

function FormatExpectedString($min, $max) {
	$expectString = ""
	if ($min) {
		$expectString = ">= $min"

		if ($max) {
			$expectString += " and <= $max"
		}

	} elseif ($max) {
		$expectString = "<$max"
	}
	return $expectString
}

function InvokeExpression($Expression, [Switch]$PrintInfoOutput, [Switch]$PrintErrorOutput, [System.Diagnostics.StopWatch]$timer) {

	$traceSuffix = ""
	if (!$PrintInfoOutput) {
		$traceSuffix = "*>&1"
	}

	$exp = "$Expression $traceSuffix"

	if ($timer) {
		$timer.Start()
	}

	$output = Invoke-Expression $exp

	if ($timer) {
		$timer.Stop()
	}

	if ($PrintErrorOutput -and !$PrintInfoOutput) {
	
		if ($LASTEXITCODE -ne 0) {
			Write-Warning "$output"
		}
	}

	return $output
}

function SetWorkDirectory($workDir) {
	
	$errPref = $ErrorActionPreference
	$ErrorActionPreference = "Stop"

	# Change to the working directory and verify that it was successfull.
	if (!(Test-Path $workDir)) {
		Write-Error "Invalid workDir `"$workDir`"."
	}

	Set-Location $workDir

	if ([String]::Compare((Get-Location).ToString(), $workDir, $true) -ne 0) {
		Write-Error "Failed to set workDir `"$workDir`"".
	}

	$ErrorActionPreference = $errPref
}

function EnsureConnection($url) {

	$errPref = $ErrorActionPreference
	$ErrorActionPreference = "Stop"

	$uri = new-object System.Uri($url.ToString())
	$authority = $uri.Authority
	$out = Test-Connection -ComputerName $authority -Count 1 -ErrorAction SilentlyContinue

	if (!$out) {
		Write-Error "Failed to contact `"$url`", please check your internet connection."
	}

	$ErrorActionPreference = $errPref
}
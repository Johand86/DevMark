Param([Parameter(Mandatory=$true)][string]$workDir, [string]$repository, [string]$branch = "master", [boolean]$clean = $true, [boolean]$allowLockedFiles = $false, [boolean]$recursive = $false, [boolean]$trace = $false)

##<CommandFunctions.ps1>##
$global:LASTEXITCODE = 0
SetWorkDirectory -workDir $workDir


function ContainsAnyOf($str, $arr) {

	foreach ($item in $arr) {
		if ($str.Contains($item)) {
			return $true
		}
	}
	return $false
}


function RunGitClean() {

	if (Test-Path .git) {

		$output = Invoke-Expression "git clean -ffdx *>&1"
		if ($trace) {
			echo $output
		}

			if ($output -ne $null) {

				$acceptableFailMessages = @("failed to remove", "Could not stat path", "Filename too long")

				$isLockedFile = $false
				if ($output.Length) {
					foreach ($out in $output) {
						if (ContainsAnyOf -str $out.ToString() -arr $acceptableFailMessages) {
							$isLockedFile = $true
							break
						}
					}
				}
				elseif (ContainsAnyOf -str $output.ToString(), -arr $acceptableFailMessages) {
					$isLockedFile = $true
				}

				if ($isLockedFile) {
				
					if (!$allowLockedFiles) {
						Write-Warning "Cleanup failed due to locked file, retrying..."
						[System.Threading.Thread]::Sleep(5000)
						$output = InvokeExpression "git clean -ffdx" -PrintInfoOutput:$trace -PrintErrorOutput:$true
					}
					else {
						$global:LASTEXITCODE = 0
					}
				}
			}
		
	}
}

if (![String]::IsNullOrEmpty($repository)) {
	EnsureConnection -url $repository
}

if ($clean -and [String]::IsNullOrEmpty($repository)) {
	RunGitClean
}
else {
	# Run git commands to check out the code.
	if (Test-Path .git) {
		if ($clean) {
			RunGitClean
		}

		$output = InvokeExpression "git checkout '$branch'" -PrintInfoOutput:$trace -PrintErrorOutput:$true
	}
	else {
		$recStr = ""
		if ($recursive) {
			$recStr = "--recursive"
		}
		$output = InvokeExpression "git clone $recStr -v '$repository' --branch '$branch' --depth 1 '$($pwd.Path)'" -PrintInfoOutput:$trace -PrintErrorOutput:$true
	}
}

return @{ ExitCode = $LASTEXITCODE }
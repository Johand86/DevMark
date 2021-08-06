Param([Parameter(Mandatory=$true)][string]$workDir, [boolean]$killWorkDirChildren = $true, [boolean]$trace = $false)

##<CommandFunctions.ps1>##
$global:LASTEXITCODE = 0
SetWorkDirectory -workDir $workDir

if ($killWorkDirChildren) {

	$spawnedProcesses = [System.Collections.ArrayList]@()

	$processes = Get-Process
	foreach ($process in $processes) {
		if (!$process.HasExited -and $process.Id -ne $pid) {
		
			if ($process.Path -and $process.Path.StartsWith($workDir)) {
				$spawnedProcesses.Add($process) | Out-Null
			}
		}
	}

	$count = 0
	if ($spawnedProcesses.Count -gt 0) {
		
		foreach ($process in $spawnedProcesses) {
			if (!$process.HasExited) {
				try {
					$count++
					$process.Kill()
				}
				catch {}
			}
		}
		
		Write-Information "Killed $count processes running from work directory `"$workDir`"."
	}


	$timeout = [DateTime]::Now.AddSeconds(20)

	while ($true) {
	
		$remainingProcesses = 0
		foreach ($process in $spawnedProcesses) {
			$refreshedProcess = Get-Process -PID $process.Id -ErrorAction SilentlyContinue
			if ($refreshedProcess -ne $null -and !$refreshedProcess.HasExited) {
				$remainingProcesses++
			}
		}

		if ($remainingProcesses -eq 0) {
			break
		}

		if ([DateTime]::Now -gt $timeout) {
			Write-Warning "Not all processes could be terminated within the time limit. $remainingProcesses remaining."
			break
		}

		[System.Threading.Thread]::Sleep(100)
	}
}
else {
	Write-Warning "killWorkDirChildren must be set to true in order to terminate remaining processes. Command completed without actions."
}

# When killing processes we allways give them some time to terminate.
[System.Threading.Thread]::Sleep(2500)

if ($trace) {
	$output
}

return @{ ExitCode = $LASTEXITCODE; Duration = $timer.ElapsedTicks; Timestamp = $startTime }

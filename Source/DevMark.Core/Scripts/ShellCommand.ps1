Param([Parameter(Mandatory=$true)][string]$workDir, [Parameter(Mandatory=$true)][string]$cmd, [boolean]$trace = $false)

##<CommandFunctions.ps1>##
$global:LASTEXITCODE = 0
SetWorkDirectory -workDir $workDir

# Setup timer
$timer = New-Object System.Diagnostics.StopWatch

# Run command
$startTime = [DateTime]::UtcNow
$output = InvokeExpression $cmd -PrintInfoOutput:$trace -PrintErrorOutput:$true -timer $timer

return @{ ExitCode = $LASTEXITCODE; Duration = $timer.ElapsedTicks; Timestamp = $startTime }

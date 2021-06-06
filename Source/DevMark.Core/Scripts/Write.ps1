Param([string]$message, [boolean]$trace = $false)

$global:LASTEXITCODE = 0
Write $message
return @{ ExitCode = $LASTEXITCODE }
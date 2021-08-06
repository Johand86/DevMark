Param([string]$message, [boolean]$trace = $false)

$global:LASTEXITCODE = 0
echo $message
return @{ ExitCode = $LASTEXITCODE }
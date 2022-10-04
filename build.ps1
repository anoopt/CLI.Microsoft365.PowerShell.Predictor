[CmdletBinding(DefaultParameterSetName = 'Build')]
param(
    [Parameter(ParameterSetName = 'Build')]
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug'
)


$srcDir = Join-Path $PSScriptRoot 'src\CLI.Microsoft365.PowerShell.Predictor'
dotnet publish -c $Configuration $srcDir

Write-Host "`nThe module 'CLI.Microsoft365.PowerShell.Predictor' is published to 'CLI.Microsoft365.PowerShell.Predictor.Module\CLI.Microsoft365.PowerShell.Predictor'`n" -ForegroundColor Green
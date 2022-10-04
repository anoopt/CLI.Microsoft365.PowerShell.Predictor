$PSDefaultParameterValues.Clear()
Set-StrictMode -Version Latest

if ($true -and ($PSEdition -eq 'Desktop'))
{
    throw "Windows PowerShell is not supported. Please install PowerShell Core 7.2 or higher version."
}

if ($true -and ($PSEdition -eq 'Core'))
{
    if ($PSVersionTable.PSVersion -lt [Version]'7.2.0')
    {
        throw "Current M365CLI.PowerShell.Predictor version doesn't support PowerShell Core versions lower than 7.2.0. Please upgrade to PowerShell Core 7.2.0 or higher. "
    }
}

$psReadlineModule = Get-Module -Name PSReadLine
$minimumRequiredVersion = [version]"2.2.2"
$shouldImportPredictor = $true

if ($psReadlineModule -ne $null -and $psReadlineModule.Version -lt $minimumRequiredVersion)
{
    $shouldImportPredictor = $false
    throw "This module requires PSReadLine version $minimumRequiredVersion. An earlier version of PSReadLine is imported in the current PowerShell session. Please open a new session before importing this module. "
}
elseif ($psReadlineModule -eq $null)
{
    try
    {
        Import-Module PSReadLine -MiniumVersion $minimumRequiredVersion -Scope Global
    }
    catch
    {
        $shouldImportPredictor = $false
        throw "This module requires PSReadLine version $minimumRequiredVersion. Please install PSReadLine $minimumRequiredVersion or higher. "
    }
}

if ($env:CHECK_VERSION_AND_UPDATE_PREDICTIONS -ne $false)
{
    $installedCLIMicrosoft365Version = "0.0.0";

    try
    {
        $installedCLIMicrosoft365Version = $( m365 version ).replace('v', '').replace('"', '');
    }
    catch
    {
        $installedCLIMicrosoft365Version = "0.0.0";
    }

    if ($installedCLIMicrosoft365Version -eq "0.0.0")
    {
        $shouldImportPredictor = $false
        throw "This module requires @pnp/cli-microsoft365. Please install @pnp/cli-microsoft365. For more information, consult the M365 CLI documentation: https://pnp.github.io/cli-microsoft365/user-guide/installing-cli."
    }
}

if ($shouldImportPredictor)
{

    # Get all the functions
    $functions = @( Get-ChildItem -Path $PSScriptRoot\scripts\*.ps1 -ErrorAction SilentlyContinue )

    # Dot source all the functions
    $functions | ForEach-Object {
        . $_.FullName
    }

    # Export all the functions
    <#$functions | ForEach-Object {
        Export-ModuleMember -Function $_.BaseName
    }#>

    if ($env:CHECK_VERSION_AND_UPDATE_PREDICTIONS -ne $false)
    {
        Update-CLIMircosoft365Predictions -installedCLIMicrosoft365Version $installedCLIMicrosoft365Version;
    }

    Import-Module (Join-Path -Path $PSScriptRoot -ChildPath CLI.Microsoft365.PowerShell.Predictor.dll);

    $env:CHECK_VERSION_AND_UPDATE_PREDICTIONS = $false;
}
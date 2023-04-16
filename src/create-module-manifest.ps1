[CmdletBinding()]
param (
    [Parameter(Position = 0, mandatory = $true)]
    [string] $Path,
    [Parameter(Position = 1, mandatory = $false)]
    [string] $Prerelease = $null,
    [Parameter(Position = 2, mandatory = $false)]
    [string] $Guid = "C8F681E8-1ED8-474D-926F-6D276DA3B984"
)

if (-not (Test-Path -Path $Path -ErrorAction SilentlyContinue)){
    Write-Error -Message "Path does not exist: $Path"
    return;
}

$author = "GBO Systems"
$rootModule = "HtmlTools.PowerShell.dll"

$name = Get-Item -Path $Path | Select -ExpandProperty Name
$moduleName = "$($name).psd1";
$modulePath = [System.IO.Path]::GetFullPath((Join-Path -Path $Path -ChildPath $moduleName))

# Version the module based on the file version of the assembly
$rootModulePath = [System.IO.Path]::GetFullPath((Join-Path -Path $Path -ChildPath $rootModule))
$rootModuleInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($rootModulePath)
$version = $rootModuleInfo.FileVersion

$newModuleManifestArgs = @{
    Author                  = $author
    CmdletsToExport         = "*"
    CompanyName             = $author
    CompatiblePSEditions    = "Core"
    Description             = "PowerShell interface for various HTML tools"
    Guid                    = $Guid
    LicenseUri              = "https://raw.githubusercontent.com/gbosystems/powershell-htmltools-module/main/LICENSE"
    ModuleVersion           = $version
    Path                    = $modulePath
    PowerShellVersion       = "7.2" # Require .NET 6.0
    ProjectUri              = "https://github.com/gbosystems/powershell-htmltools-module"
    RootModule              = $rootModule
}

# Add the prerelease string if a value was provided
if (-not [System.String]::IsNullOrWhiteSpace($Prerelease)) {
    $newModuleManifestArgs.Prerelease = $Prerelease
}

# Create the module manifest
New-ModuleManifest @newModuleManifestArgs
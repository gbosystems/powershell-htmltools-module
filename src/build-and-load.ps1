[CmdletBinding()]
param (
    [Parameter(Position = 0, mandatory = $false)]
    [string] $Configuration = "Debug",
    [Parameter(Position = 1, mandatory = $false)]
    [string] $ModuleName = "HtmlTools",
    [Parameter(Position = 2, mandatory = $false)]
    [string] $Prerelease = "dev"
)

function Get-FullPath {
    [CmdletBinding()]
    param (
        [Parameter(Position = 0, mandatory = $true)]
        [string] $RelativePath = "Debug"
    )
    $pathSeparator = [System.IO.Path]::DirectorySeparatorChar
    $childPath = $RelativePath -f $pathSeparator

    return [System.IO.Path]::GetFullPath((Join-Path -Path $PSScriptRoot -ChildPath $childPath))
}

$guid = 'C8F681E8-1ED8-474D-926F-6D276DA3B984'
$projectPath = Get-FullPath -RelativePath ".{0}HtmlTools.PowerShell{0}HtmlTools.PowerShell.csproj"
$buildOutputPath = Get-FullPath -RelativePath ".{0}HtmlTools.PowerShell{0}bin{0}$Configuration{0}_publish_{0}$ModuleName"
$createModuleManifest = Get-FullPath -RelativePath ".{0}create-module-manifest.ps1"

## Clear out the build directory, create if it doesn't exist
if (Test-Path -Path "$buildOutputPath" -ErrorAction SilentlyContinue) {
    Get-ChildItem -Path "$buildOutputPath" -Recurse | Remove-Item -Recurse -Force
} else {
    New-Item -Path "$buildOutputPath" -ItemType Directory | Out-Null
}

## Build
dotnet publish "$projectPath" --configuration "$Configuration" --output "$buildOutputPath" --no-self-contained

## Create the module manifest
Invoke-Expression "$createModuleManifest -Path '$buildOutputPath' -Guid $guid -Prerelease '$Prerelease'"

## Import the module
$modulePath = Join-Path -Path $buildOutputPath -ChildPath "$ModuleName.psd1"

Import-Module -Name $modulePath
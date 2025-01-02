$ErrorActionPreference = 'Stop'

$locales = @(
    "en-US"
)
$platforms = @(
    "x64"
    "x86"
)
$framework = "net9.0-windows"
$verbosity = "minimal"
$appProject = "$PSScriptRoot\src\Mastersign.WinJockey.csproj"
$setupProject = "$PSScriptRoot\setup\Setup.wixproj"
$setupPackageFile = "$PSScriptRoot\setup\Package.wxs"

$version =([Version]([xml](Get-Content $setupPackageFile)).Wix.Package.Version).ToString(3)

if (-not $version -match "\d+\.\d+\.\d+") {
    Write-Error "Failed to read project version from $appProject"
}

$msbuildPaths = @(
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
    "${env:ProgramFiles}\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
    "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
)
$msbuild = $msbuildPaths | Where-Object { Test-Path $_ } | Select-Object -First 1
$target = "Clean;Build;Publish"

$setupPackages = @()

foreach ($platform in $platforms) {
    & $msbuild $appProject `
        /t:$target `
        /p:Platform=$platform `
        /p:Framework=$framework `
        /p:Configuration=Release `
        /p:PublishProfile=$platform `
        /m /nodereuse:false `
        /v:$verbosity

    & $msbuild $setupProject `
        /t:Build `
        /p:Platform=$platform `
        /p:Configuration=Release `
        /m /nodereuse:false `
        /v:$verbosity

    foreach ($locale in $locales) {
        $setupPackage = "$PSScriptRoot\release\WinJockey_v${version}_${platform}.msi"
        Copy-Item "$PSScriptRoot\release\$platform\$locale\Setup.msi" $setupPackage -Force
        $setupPackages += $setupPackage
    }
}

& "$PSScriptRoot\sign.ps1" $setupPackages

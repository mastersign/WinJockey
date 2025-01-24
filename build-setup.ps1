param (
    [switch]$NoBuild,
    [switch]$NoSetupPackages
)

$ErrorActionPreference = 'Stop'

$locales = @(
    "en-US"
)
$platforms = @(
    "x64"
    "x86"
    "AnyCPU"
)
$zipPlatforms = @(
    "AnyCPU"
)
$setupPlatforms = @(
    "x64"
    "x86"
)
$suppressedPlatformSuffix = "AnyCPU"
$excludeFiles = @(
    "*.deps.json"
    "*.pdb"
)
$keepSubDirs = @(
    "de"
)
$framework = "net48"
$verbosity = "minimal"
$appProject = "$PSScriptRoot\src\Mastersign.WinJockey.csproj"
$setupProject = "$PSScriptRoot\setup\Setup.wixproj"
$setupPackageFile = "$PSScriptRoot\setup\Package.wxs"

$version = ([Version]([xml](Get-Content $setupPackageFile)).Wix.Package.Version).ToString(3)

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
    if (!$NoBuild) {
        & $msbuild $appProject `
            /t:$target `
            /p:Platform=$platform `
            /p:Framework=$framework `
            /p:Configuration=Release `
            /p:PublishProfile=$platform `
            /m /nodereuse:false `
            /v:$verbosity
    }

    $pubDir = "$PSScriptRoot\publish\$platform"
    foreach ($pattern in $excludeFiles) {
        Get-ChildItem "$pubDir\$pattern" | Remove-Item
    }
    if ($platform -ne "AnyCPU") {
        Get-ChildItem "$pubDir\*.runtimeconfig.json" | Remove-Item
    }
    foreach ($dir in (Get-ChildItem "$pubDir" -Directory)) {
        $keep = $False
        foreach ($subPath in $keepSubDirs) {
            if ("$pubDir\$subPath" -eq $dir.FullName) {
                $keep = $True
                break
            }
        }
        if (!$keep) {
            $dir | Remove-Item -Recurse
        }
    }
}
foreach ($platform in $zipPlatforms) {
    $pubDir = "$PSScriptRoot\publish\$platform"
    $suffix = if ($suppressedPlatformSuffix -eq $platform) { "" } else { "_$platform"}
    $zipArchive = "$PSScriptRoot\release\WinJockey_v${version}${suffix}.zip"
    Compress-Archive -Path "$pubDir\*" -DestinationPath $zipArchive -CompressionLevel Optimal -Force
}

if (!$NoSetupPackages) {
    foreach ($platform in $setupPlatforms) {
        & $msbuild $setupProject `
            /t:Build `
            /p:Platform=$platform `
            /p:Configuration=Release `
            /m /nodereuse:false `
            /v:$verbosity

        $suffix = if ($suppressedPlatformSuffix -eq $platform) { "" } else { "_$platform"}

        foreach ($locale in $locales) {
            $setupPackage = "$PSScriptRoot\release\WinJockey_v${version}${suffix}.msi"
            Copy-Item "$PSScriptRoot\release\$platform\$locale\Setup.msi" $setupPackage -Force
            $setupPackages += $setupPackage
        }
    }

    & "$PSScriptRoot\sign.ps1" $setupPackages
}
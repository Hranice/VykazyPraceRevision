$scriptRoot = $PSScriptRoot
Set-Location $scriptRoot

$projectName = "VykazyPrace"
$publishDir = ".\$projectName\bin\Release\net8.0-windows\win-x64\publish"
$changelogPath = Join-Path $scriptRoot "Changelog.docx"
$wixPath = "wix"
$wixSourcePath = ".\WorkLog.wxs"
$installerArchitecture = "x64"

# Read version from csproj
$csprojPath = ".\$projectName\$projectName.csproj"
$versionLine = Select-String -Path $csprojPath -Pattern '<Version>(.*?)</Version>'
if ($versionLine -eq $null -or -not ($versionLine -match '<Version>(.*?)</Version>')) {
    Write-Host "ERROR: Could not extract version from csproj."
    exit 1
}
$version = $matches[1].Trim()
Write-Host "Application version: $version"

# Clean previous publish directory
if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}

Write-Host "Building application..."
dotnet publish $projectName -c Release -r win-x64 --self-contained `
    /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Application publish failed."
    exit $LASTEXITCODE
}

if (-not (Test-Path $changelogPath)) {
    Write-Host "ERROR: Changelog.docx was not found next to publish.ps1: $changelogPath"
    exit 1
}

$publishedChangelogPath = Join-Path $publishDir "Changelog.docx"
Copy-Item $changelogPath $publishedChangelogPath -Force
Write-Host "Changelog copied to publish directory: $publishedChangelogPath"

# Build installer
if (-not (Get-Command $wixPath -ErrorAction SilentlyContinue)) {
    Write-Host "ERROR: WiX Toolset command not found: wix"
    Write-Host "Install WiX Toolset 5.0.2 or another compatible version and make sure wix.exe is available in PATH."
    exit 1
}

if (-not (Test-Path $wixSourcePath)) {
    Write-Host "ERROR: .wxs file not found:"
    Write-Host (Resolve-Path ".\" -ErrorAction SilentlyContinue)
    Write-Host "Expected path: $wixSourcePath"
    exit 1
}

$installerBaseName = "WorkLog_Installer"
$installerBuiltPath = ".\Output\$installerBaseName.msi"

if (-not (Test-Path ".\Output")) {
    New-Item -ItemType Directory -Path ".\Output" | Out-Null
}

Get-ChildItem ".\Output" -Filter "$installerBaseName.*" | Remove-Item -Force

Write-Host "Building installer via WiX Toolset..."
& $wixPath build $wixSourcePath -arch $installerArchitecture -d AppVersion=$version -o $installerBuiltPath
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: WiX build failed."
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "Installer built locally as: $installerBuiltPath"

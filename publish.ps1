param(
    [switch]$SkipUpload
)

$projectName = "VykazyPrace"
$publishDir = ".\$projectName\bin\Release\net8.0-windows\win-x64\publish"
$networkUpdatePath = "Z:\TS\jprochazka-sw\WorkLog\Updates"
$wixPath = "wix"
$wixSourcePath = ".\WorkLog.wxs"
$installerArchitecture = "x64"
$changelogPath = ".\Changelog.docx"

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

# Generate latest.txt
$latestTxtPath = Join-Path $publishDir "latest.txt"
$version | Out-File -FilePath $latestTxtPath -Encoding ASCII
Write-Host "Generated latest.txt with version: $version"

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
$installerDest = Join-Path $networkUpdatePath "$installerBaseName.msi"
$networkChangelogPath = Join-Path $networkUpdatePath "Changelog.docx"

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

if ($SkipUpload) {
    Write-Host ""
    Write-Host "SkipUpload enabled. Installer built locally as: $installerBuiltPath"
    exit 0
}

Write-Host ""
Write-Host "Copying files to: $networkUpdatePath"

Copy-Item $latestTxtPath "$networkUpdatePath\latest.txt" -Force
Copy-Item $installerBuiltPath $installerDest -Force

if (Test-Path $changelogPath) {
    Copy-Item $changelogPath $networkChangelogPath -Force
    Write-Host "Changelog uploaded."
} else {
    Write-Host "WARNING: Changelog.docx not found, skipping upload."
}

Write-Host ""
Write-Host "Installer uploaded as: $installerBaseName.msi"

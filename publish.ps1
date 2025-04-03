$projectName = "VykazyPrace"
$publishDir = ".\$projectName\bin\Release\net8.0-windows\win-x64\publish"
$networkUpdatePath = "Z:\TS\jprochazka-sw\WorkLog\Updates"
$issPath = ".\WorkLog.iss"
$innoSetupCompiler = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"

# Read version from csproj
$csprojPath = ".\$projectName\$projectName.csproj"
$versionLine = Select-String -Path $csprojPath -Pattern '<Version>(.*?)</Version>'
if ($versionLine -eq $null -or -not ($versionLine -match '<Version>(.*?)</Version>')) {
    Write-Host "ERROR: Could not extract version from csproj."
    exit 1
}
$version = $matches[1].Trim()
Write-Host "Application version: $version"

# Set environment variable for Inno Setup
[System.Environment]::SetEnvironmentVariable("APP_VERSION", $version, "Process")

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
if ((Test-Path $innoSetupCompiler) -and (Test-Path $issPath)) {
    Write-Host "Building installer via Inno Setup..."
    & "$innoSetupCompiler" "$issPath"
} else {
    Write-Host "ERROR: Inno Setup compiler or .iss file not found."
    exit 1
}

# Rename installer after build
$installerBaseName = "WorkLog_Installer"
$installerBuiltPath = ".\Output\$installerBaseName.exe"
$installerRenamed = ".\Output\${installerBaseName}_$version.exe"
if (Test-Path $installerRenamed) {
    Remove-Item $installerRenamed -Force
}
Rename-Item -Path $installerBuiltPath -NewName "${installerBaseName}_$version.exe"

# Copy installer and latest.txt to network path
$installerDest = Join-Path $networkUpdatePath "${installerBaseName}_$version.exe"
Write-Host ""
Write-Host "Copying files to: $networkUpdatePath"

Copy-Item $latestTxtPath "$networkUpdatePath\latest.txt" -Force
Copy-Item $installerRenamed $installerDest -Force

Write-Host ""
Write-Host "Installer uploaded as: ${installerBaseName}_$version.exe"

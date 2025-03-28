$projectName = "VykazyPrace"
$publishDir = ".\$projectName\bin\Release\net8.0-windows\win-x64\publish"
$networkUpdatePath = "Z:\TS\jprochazka-sw\WorkLog\Updates"
$issPath = ".\WorkLog.iss"
$innoSetupCompiler = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
$installerExe = "WorkLog_Installer.exe"
$installerBuiltPath = ".\Output\$installerExe"
$installerDest = Join-Path $networkUpdatePath $installerExe

Write-Host "Building application..."
dotnet publish $projectName -c Release -r win-x64 --self-contained `
    /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

# Read version from csproj
$csprojPath = ".\$projectName\$projectName.csproj"
$versionLine = Select-String -Path $csprojPath -Pattern '<Version>(.*?)</Version>'
if ($versionLine -eq $null -or -not ($versionLine -match '<Version>(.*?)</Version>')) {
    Write-Host "ERROR: Could not extract version from csproj."
    exit 1
}
$version = $matches[1].Trim()
Write-Host "Application version: $version"

# Generate latest.txt
$latestTxtPath = Join-Path $publishDir "latest.txt"
$version | Out-File -FilePath $latestTxtPath -Encoding ASCII

# Build installer
if ((Test-Path $innoSetupCompiler) -and (Test-Path $issPath)) {
    Write-Host "Building installer via Inno Setup..."
    & "$innoSetupCompiler" "$issPath"
} else {
    Write-Host "ERROR: Inno Setup compiler or .iss file not found."
    exit 1
}

# Copy installer to network
Write-Host ""
Write-Host "Copying files to: $networkUpdatePath"

Copy-Item $latestTxtPath "$networkUpdatePath\latest.txt" -Force
Copy-Item $installerBuiltPath $installerDest -Force

Write-Host ""
Write-Host "✅ Installer uploaded as: $installerExe"

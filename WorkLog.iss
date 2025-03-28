[Setup]
AppName=WorkLog
AppVersion=1.0.0
AppVerName=WorkLog
OutputBaseFilename=WorkLog_Installer
OutputDir=Output
DefaultDirName={userappdata}\WorkLog
DefaultGroupName=WorkLog
Compression=lzma
SolidCompression=yes
DisableProgramGroupPage=yes
DisableReadyPage=yes
DisableDirPage=yes
DisableFinishedPage=no
AllowNoIcons=yes
PrivilegesRequired=lowest
SetupIconFile=WorkLog.ico

[Files]
Source: "VykazyPrace\bin\Release\net8.0-windows\win-x64\publish\WorkLog.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "VykazyPrace\bin\Release\net8.0-windows\win-x64\publish\*.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "VykazyPrace\bin\Release\net8.0-windows\win-x64\publish\latest.txt"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{userdesktop}\WorkLog"; Filename: "{app}\WorkLog.exe"

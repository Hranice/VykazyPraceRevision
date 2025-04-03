#define AppVersion GetEnv("APP_VERSION")

[Setup]
AppName=WorkLog
AppVersion={#AppVersion}
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
CloseApplications=yes
RestartApplications=yes

[Files]
Source: "Changelog.docx"; DestDir: "{app}"; Flags: ignoreversion
Source: "VykazyPrace\bin\Release\net8.0-windows\win-x64\publish\WorkLog.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "VykazyPrace\bin\Release\net8.0-windows\win-x64\publish\*.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "VykazyPrace\bin\Release\net8.0-windows\win-x64\publish\latest.txt"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{userdesktop}\WorkLog"; Filename: "{app}\WorkLog.exe"

[Run]
Filename: "{app}\WorkLog.exe"; Flags: nowait

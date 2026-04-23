#define AppVersion GetEnv("APP_VERSION")

[Setup]
AppId={{62687f32-9b0f-4466-b923-78cfb71e337c}
AppName=WorkLog
AppVersion={#AppVersion}
AppVerName=WorkLog
OutputBaseFilename=WorkLog_Installer
OutputDir=Output
DefaultDirName={autopf}\WorkLog
DefaultGroupName=WorkLog
Compression=lzma
SolidCompression=yes
DisableProgramGroupPage=yes
DisableReadyPage=yes
DisableDirPage=yes
DisableFinishedPage=no
AllowNoIcons=yes
PrivilegesRequired=admin
SetupIconFile=WorkLog.ico
CloseApplications=yes
RestartApplications=no
ArchitecturesInstallIn64BitMode=x64

[Files]
Source: "VykazyPrace\bin\Release\net8.0-windows\win-x64\publish\WorkLog.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{commondesktop}\WorkLog"; Filename: "{app}\WorkLog.exe"

[Run]
Filename: "{app}\WorkLog.exe"; Flags: nowait
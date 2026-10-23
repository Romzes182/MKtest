; MKtest installer for Windows 10/11 x64.
; Input: dotnet publish -r win-x64 --self-contained true -o artifacts\publish\win-x64
; Build with: ISCC.exe installer\MKtest.iss

#define MyAppName "MKtest"
#define MyAppVersion "1.0.0"
#define MyAppExeName "MKtest.exe"

[Setup]
AppId={{67C3EF4E-FD83-42B0-B9F2-F4E89EB7E7BF}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion} (x64)
AppPublisher=MKtest
DefaultDirName={localappdata}\Programs\MKtest
DefaultGroupName=MKtest
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
MinVersion=10.0
UninstallDisplayIcon={app}\{#MyAppExeName}
OutputDir=..\artifacts\installer
OutputBaseFilename=MKtest-Setup-win-x64
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
SetupLogging=yes
UsePreviousAppDir=yes

[Files]
; All runtime DLLs, demo scenarios, JSON templates and routes from the published app.
Source: "..\artifacts\publish\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
; Both shortcuts are created for the installing user, without requiring admin rights.
Name: "{userprograms}\MKtest\MKtest"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{userdesktop}\MKtest"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch MKtest"; Flags: nowait postinstall skipifsilent

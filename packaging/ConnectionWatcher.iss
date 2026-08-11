#define MyAppName "TCP Connection Watcher"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Connection Watcher Project"
#define MyAppExeName "ConnectionWatcher.exe"

#ifndef SourceDir
  #define SourceDir "..\artifacts\publish\win-x64"
#endif

#ifndef PackageLanguage
  #define PackageLanguage "en-US"
#endif

#if PackageLanguage == "zh-CN"
  #define PackageOutputDir "..\dist\zh-CN"
  #define PackageOutputName "ConnectionWatcher-Setup-win-x64-zh-CN"
#else
  #define PackageOutputDir "..\dist\en-US"
  #define PackageOutputName "ConnectionWatcher-Setup-win-x64-en-US"
#endif

[Setup]
AppId={{83F3182E-1327-4B05-BBFC-E6449A09968C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\ConnectionWatcher
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
SetupArchitecture=x64
OutputDir={#PackageOutputDir}
OutputBaseFilename={#PackageOutputName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#MyAppExeName}
AppMutex=Local\ConnectionWatcher-6F695A7A-5E57-4B21-86A4-A487B45D67DE
CloseApplications=yes
CloseApplicationsFilter={#MyAppExeName}
SetupLogging=yes

[Languages]
#if PackageLanguage == "zh-CN"
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"
#else
Name: "english"; MessagesFile: "compiler:Default.isl"
#endif

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourceDir}\ConnectionWatcher.exe"; DestDir: "{app}"; Flags: ignoreversion
#if PackageLanguage == "zh-CN"
Source: "..\README.zh-CN.md"; DestDir: "{app}\Docs"; DestName: "项目说明.md"; Flags: ignoreversion
Source: "..\docs\使用说明.md"; DestDir: "{app}\Docs"; Flags: ignoreversion
#else
Source: "..\README.md"; DestDir: "{app}\Docs"; DestName: "Project-Overview.md"; Flags: ignoreversion
Source: "..\docs\User-Guide.md"; DestDir: "{app}\Docs"; Flags: ignoreversion
#endif

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueName: "ConnectionWatcher"; Flags: uninsdeletevalue dontcreatekey

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

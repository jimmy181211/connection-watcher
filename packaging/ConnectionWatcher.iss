#define MyAppName "TCP Connection Watcher"
#define MyAppVersion "1.3.0"
#define MyAppPublisher "Connection Watcher Project"
#define MyAppExeName "ConnectionWatcher.exe"

#ifndef SourceDir
  #define SourceDir "..\artifacts\publish\win-x64"
#endif

#define PackageOutputDir "..\dist"
#define PackageOutputName "TCP-Connection-Watcher-Setup-win-x64"

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
SetupIconFile=..\src\ConnectionWatcher.App\Assets\ConnectionWatcher.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
AppMutex=Local\ConnectionWatcher-6F695A7A-5E57-4B21-86A4-A487B45D67DE
CloseApplications=yes
CloseApplicationsFilter={#MyAppExeName}
SetupLogging=yes
ShowLanguageDialog=yes
UsePreviousLanguage=no
LanguageDetectionMethod=none

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "chinesesimplified"; MessagesFile: "compiler:Languages\ChineseSimplified.isl"
Name: "chinesetraditional"; MessagesFile: "compiler:Languages\ChineseTraditional.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourceDir}\ConnectionWatcher.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\docs\项目说明.md"; DestDir: "{app}\Docs"; DestName: "项目说明.md"; Flags: ignoreversion
Source: "..\docs\使用说明.md"; DestDir: "{app}\Docs"; Flags: ignoreversion
Source: "..\docs\Project-Overview.md"; DestDir: "{app}\Docs"; DestName: "Project-Overview.md"; Flags: ignoreversion
Source: "..\docs\User-Guide.md"; DestDir: "{app}\Docs"; Flags: ignoreversion
Source: "..\docs\Descripcion-del-proyecto.md"; DestDir: "{app}\Docs"; Flags: ignoreversion
Source: "..\docs\Guia-del-usuario.md"; DestDir: "{app}\Docs"; Flags: ignoreversion
Source: "..\docs\專案說明.md"; DestDir: "{app}\Docs"; Flags: ignoreversion
Source: "..\docs\使用說明-繁體中文.md"; DestDir: "{app}\Docs"; Flags: ignoreversion
Source: "..\docs\Presentation-du-projet.md"; DestDir: "{app}\Docs"; Flags: ignoreversion
Source: "..\docs\Guide-utilisateur.md"; DestDir: "{app}\Docs"; Flags: ignoreversion
Source: "..\docs\Projektuebersicht.md"; DestDir: "{app}\Docs"; Flags: ignoreversion
Source: "..\docs\Benutzerhandbuch.md"; DestDir: "{app}\Docs"; Flags: ignoreversion
Source: "..\docs\Visao-geral-do-projeto.md"; DestDir: "{app}\Docs"; Flags: ignoreversion
Source: "..\docs\Guia-do-usuario.md"; DestDir: "{app}\Docs"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueName: "ConnectionWatcher"; Flags: uninsdeletevalue dontcreatekey

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: files; Name: "{app}\install-language.txt"

[Code]
function SelectedAppLanguage(): String;
begin
  if ActiveLanguage = 'chinesesimplified' then
    Result := 'zh-CN'
  else if ActiveLanguage = 'chinesetraditional' then
    Result := 'zh-TW'
  else if ActiveLanguage = 'spanish' then
    Result := 'es'
  else if ActiveLanguage = 'french' then
    Result := 'fr'
  else if ActiveLanguage = 'german' then
    Result := 'de'
  else if ActiveLanguage = 'brazilianportuguese' then
    Result := 'pt-BR'
  else
    Result := 'en';
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
    SaveStringToFile(
      ExpandConstant('{app}\install-language.txt'),
      SelectedAppLanguage(),
      False);
end;

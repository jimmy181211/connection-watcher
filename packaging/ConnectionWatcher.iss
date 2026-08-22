#define MyAppName "SocketSight"
#define MyAppVersion "1.4.1"
#define MyAppPublisher "SocketSight Project"
#define MyAppExeName "SocketSight.exe"

#ifndef SourceDir
  #define SourceDir "..\artifacts\publish\win-x64"
#endif

#define PackageOutputDir "..\dist"
#define PackageOutputName "SocketSight-Setup-win-x64"

[Setup]
AppId={{83F3182E-1327-4B05-BBFC-E6449A09968C}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\SocketSight
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
DisableWelcomePage=no
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
AppComments=See the connections that matter.
VersionInfoDescription=SocketSight - Rule-Based TCP Connection Monitoring for Windows
VersionInfoProductName={#MyAppName}
AppMutex=Local\ConnectionWatcher-6F695A7A-5E57-4B21-86A4-A487B45D67DE
CloseApplications=yes
CloseApplicationsFilter={#MyAppExeName},ConnectionWatcher.exe
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

[Messages]
english.SelectLanguageLabel=Select the language to use during installation and in SocketSight:
chinesesimplified.SelectLanguageLabel=请选择安装过程和 SocketSight 软件使用的语言：
chinesetraditional.SelectLanguageLabel=請選擇安裝過程和 SocketSight 軟體使用的語言：
spanish.SelectLanguageLabel=Seleccione el idioma que se usará durante la instalación y en SocketSight:
french.SelectLanguageLabel=Sélectionnez la langue à utiliser pendant l’installation et dans SocketSight :
german.SelectLanguageLabel=Wählen Sie die Sprache für die Installation und für SocketSight aus:
brazilianportuguese.SelectLanguageLabel=Selecione o idioma usado durante a instalação e no SocketSight:

[CustomMessages]
english.InstallMessage1=Preparing SocketSight…
english.InstallMessage2=Installing the tools that apply your monitoring rules…
english.InstallMessage3=Keeping monitoring local—just as designed.
english.InstallMessage4=Placing the network magnifying glass…
english.InstallMessage5=Almost ready to watch the connections that matter.
english.InstallMessage6=No packets were harmed during this installation.
chinesesimplified.InstallMessage1=正在准备SocketSight……
chinesesimplified.InstallMessage2=正在安装用于执行监视规则的组件……
chinesesimplified.InstallMessage3=监视工作留在本机——一切按设计运行。
chinesesimplified.InstallMessage4=正在摆放网络放大镜……
chinesesimplified.InstallMessage5=即将开始关注真正重要的连接。
chinesesimplified.InstallMessage6=本次安装没有伤害任何数据包。
chinesetraditional.InstallMessage1=正在準備 SocketSight……
chinesetraditional.InstallMessage2=正在安裝用於執行監視規則的元件……
chinesetraditional.InstallMessage3=監視工作留在本機——一切按設計運作。
chinesetraditional.InstallMessage4=正在擺放網路放大鏡……
chinesetraditional.InstallMessage5=即將開始關注真正重要的連線。
chinesetraditional.InstallMessage6=本次安裝沒有傷害任何資料封包。
spanish.InstallMessage1=Preparando SocketSight…
spanish.InstallMessage2=Instalando las herramientas que aplican tus reglas…
spanish.InstallMessage3=El monitoreo permanece local, tal como fue diseñado.
spanish.InstallMessage4=Colocando la lupa de red…
spanish.InstallMessage5=Casi listo para observar las conexiones que importan.
spanish.InstallMessage6=Ningún paquete sufrió daños durante esta instalación.
french.InstallMessage1=Préparation de SocketSight…
french.InstallMessage2=Installation des outils qui appliquent vos règles…
french.InstallMessage3=La surveillance reste locale, comme prévu.
french.InstallMessage4=Mise en place de la loupe réseau…
french.InstallMessage5=Bientôt prêt à observer les connexions qui comptent.
french.InstallMessage6=Aucun paquet n’a été maltraité pendant cette installation.
german.InstallMessage1=SocketSight wird vorbereitet…
german.InstallMessage2=Die Werkzeuge für Ihre Regeln werden installiert…
german.InstallMessage3=Die Überwachung bleibt lokal – genau wie vorgesehen.
german.InstallMessage4=Die Netzwerk-Lupe wird bereitgelegt…
german.InstallMessage5=Gleich bereit für die Verbindungen, auf die es ankommt.
german.InstallMessage6=Bei dieser Installation kamen keine Pakete zu Schaden.
brazilianportuguese.InstallMessage1=Preparando o SocketSight…
brazilianportuguese.InstallMessage2=Instalando as ferramentas que aplicam suas regras…
brazilianportuguese.InstallMessage3=O monitoramento permanece local, como planejado.
brazilianportuguese.InstallMessage4=Posicionando a lupa da rede…
brazilianportuguese.InstallMessage5=Quase pronto para observar as conexões que importam.
brazilianportuguese.InstallMessage6=Nenhum pacote foi ferido durante esta instalação.

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\src\ConnectionWatcher.App\Assets\ConnectionWatcher.png"; Flags: dontcopy
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
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
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueName: "SocketSight"; Flags: uninsdeletevalue dontcreatekey
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueName: "ConnectionWatcher"; Flags: uninsdeletevalue dontcreatekey

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: files; Name: "{app}\install-language.txt"

[InstallDelete]
Type: files; Name: "{app}\ConnectionWatcher.exe"
Type: files; Name: "{autoprograms}\TCP Connection Watcher.lnk"
Type: files; Name: "{autodesktop}\TCP Connection Watcher.lnk"

[Code]
var
  WelcomeBrandPanel: TPanel;
  WelcomeBrandLogo: TBitmapImage;
  WelcomeBrandNameLabel: TNewStaticText;
  WelcomeBrandTaglineLabel: TNewStaticText;
  FinishedBrandPanel: TPanel;
  FinishedBrandLogo: TBitmapImage;
  FinishedBrandNameLabel: TNewStaticText;
  FinishedBrandTaglineLabel: TNewStaticText;
  InstallMessageLabel: TNewStaticText;
  InstallMessageIndex: Integer;

procedure CreateBrandBlock(
  ParentControl: TWinControl;
  BackgroundColor: TColor;
  var BrandPanel: TPanel;
  var BrandLogo: TBitmapImage;
  var BrandNameLabel: TNewStaticText;
  var BrandTaglineLabel: TNewStaticText);
begin
  BrandPanel := TPanel.Create(WizardForm);
  BrandPanel.Parent := ParentControl;
  BrandPanel.Left := 0;
  BrandPanel.Top := 0;
  BrandPanel.Width := ScaleX(190);
  BrandPanel.Height := ParentControl.ClientHeight;
  BrandPanel.BevelOuter := bvNone;
  BrandPanel.Color := BackgroundColor;

  BrandLogo := TBitmapImage.Create(WizardForm);
  BrandLogo.Parent := BrandPanel;
  BrandLogo.Left := ScaleX(31);
  BrandLogo.Top := ScaleY(50);
  BrandLogo.Width := ScaleX(128);
  BrandLogo.Height := ScaleY(128);
  BrandLogo.Stretch := True;
  BrandLogo.PngImage.LoadFromFile(
    ExpandConstant('{tmp}\ConnectionWatcher.png'));

  BrandNameLabel := TNewStaticText.Create(WizardForm);
  BrandNameLabel.Parent := BrandPanel;
  BrandNameLabel.Left := ScaleX(8);
  BrandNameLabel.Top := BrandLogo.Top + BrandLogo.Height + ScaleY(16);
  BrandNameLabel.Width := BrandPanel.Width - ScaleX(16);
  BrandNameLabel.Height := ScaleY(32);
  BrandNameLabel.AutoSize := False;
  BrandNameLabel.Alignment := taCenter;
  BrandNameLabel.Caption := 'SocketSight';
  BrandNameLabel.Font.Name := 'Segoe UI';
  BrandNameLabel.Font.Size := 17;
  BrandNameLabel.Font.Style := [fsBold];
  BrandNameLabel.Font.Color := $00492B11;

  BrandTaglineLabel := TNewStaticText.Create(WizardForm);
  BrandTaglineLabel.Parent := BrandPanel;
  BrandTaglineLabel.Left := ScaleX(16);
  BrandTaglineLabel.Top := BrandNameLabel.Top +
    BrandNameLabel.Height + ScaleY(6);
  BrandTaglineLabel.Width := BrandPanel.Width - ScaleX(32);
  BrandTaglineLabel.Height := ScaleY(46);
  BrandTaglineLabel.AutoSize := False;
  BrandTaglineLabel.WordWrap := True;
  BrandTaglineLabel.Alignment := taCenter;
  BrandTaglineLabel.Caption := 'See the connections that matter.';
  BrandTaglineLabel.Font.Name := 'Segoe UI';
  BrandTaglineLabel.Font.Size := 9;
  BrandTaglineLabel.Font.Color := $009D6F2D;
end;

procedure ShowInstallMessage(MessageIndex: Integer);
begin
  InstallMessageIndex := MessageIndex;
  InstallMessageLabel.Caption := ExpandConstant(
    '{cm:InstallMessage' + IntToStr(InstallMessageIndex) + '}');
end;

procedure InitializeWizard;
var
  ContentLeft: Integer;
begin
  ExtractTemporaryFile('ConnectionWatcher.png');

  WizardForm.WizardBitmapImage.Visible := False;
  WizardForm.WizardBitmapImage2.Visible := False;

  CreateBrandBlock(
    WizardForm.WelcomePage,
    WizardForm.WelcomePage.Color,
    WelcomeBrandPanel,
    WelcomeBrandLogo,
    WelcomeBrandNameLabel,
    WelcomeBrandTaglineLabel);
  CreateBrandBlock(
    WizardForm.FinishedPage,
    WizardForm.FinishedPage.Color,
    FinishedBrandPanel,
    FinishedBrandLogo,
    FinishedBrandNameLabel,
    FinishedBrandTaglineLabel);

  ContentLeft := WelcomeBrandPanel.Width + ScaleX(28);
  WizardForm.WelcomeLabel1.Left := ContentLeft;
  WizardForm.WelcomeLabel1.Width :=
    WizardForm.WelcomePage.ClientWidth - ContentLeft - ScaleX(16);
  WizardForm.WelcomeLabel1.Top := ScaleY(91);
  WizardForm.WelcomeLabel1.AdjustHeight;
  WizardForm.WelcomeLabel2.Left := ContentLeft;
  WizardForm.WelcomeLabel2.Width := WizardForm.WelcomeLabel1.Width;
  WizardForm.WelcomeLabel2.Top :=
    WizardForm.WelcomeLabel1.Top + WizardForm.WelcomeLabel1.Height + ScaleY(24);
  WizardForm.WelcomeLabel2.AdjustHeight;

  InstallMessageLabel := TNewStaticText.Create(WizardForm);
  InstallMessageLabel.Parent := WizardForm.InstallingPage;
  InstallMessageLabel.Left := WizardForm.ProgressGauge.Left;
  InstallMessageLabel.Top := WizardForm.ProgressGauge.Top +
    WizardForm.ProgressGauge.Height + ScaleY(16);
  InstallMessageLabel.Width := WizardForm.ProgressGauge.Width;
  InstallMessageLabel.Height := ScaleY(38);
  InstallMessageLabel.AutoSize := False;
  InstallMessageLabel.WordWrap := True;
  InstallMessageLabel.Alignment := taCenter;
  InstallMessageLabel.Font.Color := clGray;
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  if CurPageID = wpInstalling then
    ShowInstallMessage(1);
end;

procedure CurInstallProgressChanged(CurProgress, MaxProgress: Integer);
var
  NewMessageIndex: Integer;
begin
  if MaxProgress <= 0 then
    NewMessageIndex := 1
  else
    NewMessageIndex := 1 + (CurProgress * 5 div MaxProgress);

  if NewMessageIndex > 6 then
    NewMessageIndex := 6;
  if NewMessageIndex <> InstallMessageIndex then
    ShowInstallMessage(NewMessageIndex);
end;

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

; Installateur de Chutoki Free (Inno Setup 6) - compilé par build.ps1
; ISCC.exe /DAppVersion=1.2.0 installer\chutoki.iss

#ifndef AppVersion
  #define AppVersion "0.0.0"
#endif
#define AppName "Chutoki"
#define AppExe "Chutoki.exe"
#define Root ".."

[Setup]
AppId={{6E3F2B1C-8A47-4D2E-9C15-7B0D3A9E4F21}
AppName={#AppName}
AppVerName={#AppName} Free {#AppVersion}
AppVersion={#AppVersion}
VersionInfoVersion={#AppVersion}
AppPublisher=Radjouh Slim
AppPublisherURL=https://slimouzz.github.io/chutoki/
AppSupportURL=https://github.com/Slimouzz/chutoki/issues
AppUpdatesURL=https://slimouzz.github.io/chutoki/
AppContact=contact-chutoki@proton.me
AppCopyright=Copyright (c) 2026 Radjouh Slim - MIT
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.17134
LicenseFile={#Root}\LICENSE.txt
SetupIconFile={#Root}\src\app.ico
UninstallDisplayIcon={app}\{#AppExe}
UninstallDisplayName={#AppName} Free
OutputDir={#Root}
OutputBaseFilename=Chutoki-Setup
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
ShowLanguageDialog=auto
CloseApplications=no

[Languages]
; l'anglais en premier : c'est la langue utilisée quand celle de Windows n'est pas dans la liste (ex. chinois)
Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "fr"; MessagesFile: "compiler:Languages\French.isl"
Name: "it"; MessagesFile: "compiler:Languages\Italian.isl"
Name: "es"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "ja"; MessagesFile: "compiler:Languages\Japanese.isl"

[CustomMessages]
fr.TaskDesktop=Créer un raccourci sur le bureau
en.TaskDesktop=Create a desktop shortcut
it.TaskDesktop=Crea un collegamento sul desktop
es.TaskDesktop=Crear un acceso directo en el escritorio
ja.TaskDesktop=デスクトップにショートカットを作成
fr.TaskAutostart=Démarrer Chutoki avec Windows (recommandé)
en.TaskAutostart=Start Chutoki with Windows (recommended)
it.TaskAutostart=Avvia Chutoki con Windows (consigliato)
es.TaskAutostart=Iniciar Chutoki con Windows (recomendado)
ja.TaskAutostart=Windows と一緒に Chutoki を起動（推奨）
fr.TaskPawnIO=Installer le pilote gratuit PawnIO (nécessaire pour piloter les ventilateurs)
en.TaskPawnIO=Install the free PawnIO driver (needed to control the fans)
it.TaskPawnIO=Installa il driver gratuito PawnIO (necessario per regolare le ventole)
es.TaskPawnIO=Instalar el controlador gratuito PawnIO (necesario para controlar los ventiladores)
ja.TaskPawnIO=無料ドライバー PawnIO をインストール（ファンの制御に必要）
fr.PawnIODownload=Téléchargement de PawnIO depuis son site officiel...
en.PawnIODownload=Downloading PawnIO from its official page...
it.PawnIODownload=Download di PawnIO dalla pagina ufficiale...
es.PawnIODownload=Descargando PawnIO desde su página oficial...
ja.PawnIODownload=公式ページから PawnIO をダウンロードしています...
fr.PawnIOFailed=Impossible de télécharger PawnIO. Tu pourras l'installer plus tard : Chutoki te proposera le lien.
en.PawnIOFailed=Could not download PawnIO. You can install it later: Chutoki will offer the link.
it.PawnIOFailed=Impossibile scaricare PawnIO. Potrai installarlo più tardi: Chutoki ti proporrà il link.
es.PawnIOFailed=No se pudo descargar PawnIO. Podrás instalarlo más tarde: Chutoki te ofrecerá el enlace.
ja.PawnIOFailed=PawnIO をダウンロードできませんでした。後でインストールできます（Chutoki がリンクを表示します）。
fr.Launch=Lancer Chutoki
en.Launch=Launch Chutoki
it.Launch=Avvia Chutoki
es.Launch=Abrir Chutoki
ja.Launch=Chutoki を起動

[Tasks]
Name: "pawnio"; Description: "{cm:TaskPawnIO}"; Check: not PawnIOInstalled
Name: "autostart"; Description: "{cm:TaskAutostart}"
Name: "desktopicon"; Description: "{cm:TaskDesktop}"

[Files]
Source: "{#Root}\dist\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
; démarrage automatique : même tâche planifiée que la case « Démarrer avec Windows » de l'appli
Filename: "{sys}\schtasks.exe"; Parameters: "/create /f /tn Chutoki /sc onlogon /rl highest /delay 0000:20 /tr ""\""{app}\{#AppExe}\"" --tray"""; Flags: runhidden; Tasks: autostart
; runascurrentuser : Chutoki a besoin des droits administrateur, que l'installateur a déjà (sinon erreur 740)
Filename: "{app}\{#AppExe}"; Description: "{cm:Launch}"; Flags: postinstall nowait skipifsilent runascurrentuser

[UninstallRun]
Filename: "{app}\{#AppExe}"; Parameters: "--quit"; Flags: runhidden waituntilterminated; RunOnceId: "QuitChutoki"
Filename: "{sys}\schtasks.exe"; Parameters: "/delete /f /tn Chutoki"; Flags: runhidden; RunOnceId: "DelTask"

[Code]
function PawnIOInstalled: Boolean;
begin
  Result := RegKeyExists(HKLM, 'SYSTEM\CurrentControlSet\Services\PawnIO');
end;

// Avant une mise à jour : on ferme proprement la version en cours (les ventilateurs reviennent au BIOS)
function PrepareToInstall(var NeedsRestart: Boolean): String;
var Code: Integer; Exe: String;
begin
  Result := '';
  Exe := ExpandConstant('{app}\{#AppExe}');
  if FileExists(Exe) then
    Exec(Exe, '--quit', '', SW_HIDE, ewWaitUntilTerminated, Code);
end;

// PawnIO : téléchargé depuis sa page officielle puis lancé (l'utilisateur suit son propre installateur)
procedure CurStepChanged(CurStep: TSetupStep);
var Code: Integer; F: String;
begin
  if (CurStep = ssPostInstall) and WizardIsTaskSelected('pawnio') then begin
    WizardForm.StatusLabel.Caption := CustomMessage('PawnIODownload');
    try
      DownloadTemporaryFile('https://github.com/namazso/PawnIO.Setup/releases/latest/download/PawnIO_setup.exe', 'PawnIO_setup.exe', '', nil);
      F := ExpandConstant('{tmp}\PawnIO_setup.exe');
      Exec(F, '', '', SW_SHOW, ewWaitUntilTerminated, Code);
    except
      MsgBox(CustomMessage('PawnIOFailed'), mbInformation, MB_OK);
    end;
  end;
end;

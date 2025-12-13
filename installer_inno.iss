; Script generated for Pi Node Monitor Pro
; Usage: Compile with Inno Setup Compiler

#define MyAppName "Pi Node Monitor Pro"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "PiNodeMonitorTeam"
#define MyAppExeName "PiNodeMonitorWinForm.exe"
#define SourceDir "Publish\win-x64-compressed"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
AppId={{WARNING-GENERATE-NEW-GUID-HERE}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
; Allow user to disable Start Menu folder creation
AllowNoIcons=yes
; Output file settings
OutputDir=.
OutputBaseFilename=PiNodeMonitorPro_Setup_Inno
Compression=lzma2
SolidCompression=yes
; Require 64-bit Windows since we built for win-x64
ArchitecturesInstallIn64BitMode=x64
; Icon settings (Uncomment if you have an icon)
; SetupIconFile=app.ico
; UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "korean"; MessagesFile: "compiler:Languages\Korean.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Source file path relative to this .iss file
Source: "{#SourceDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Add other helper files here if needed (e.g. config files, readme)

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Add Firewall Rule for Mobile Connect (Port 5000)
Filename: "netsh"; Parameters: "advfirewall firewall add rule name=""{#MyAppName} - Mobile"" dir=in action=allow protocol=TCP localport=5000 profile=private,public"; Flags: runhidden
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; Remove Firewall Rule
Filename: "netsh"; Parameters: "advfirewall firewall delete rule name=""{#MyAppName} - Mobile"""; Flags: runhidden

[Code]
var
  CopyrightLabel: TNewStaticText;

procedure LinkClick(Sender: TObject);
var
  ErrorCode: Integer;
begin
  // ShellExec(Verb, Filename, Params, WorkingDir, ShowCmd, Wait, ErrorCode)
  ShellExec('open', 'https://gurupia.github.io', '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
end;

procedure InitializeWizard;
begin
  CopyrightLabel := TNewStaticText.Create(WizardForm);
  CopyrightLabel.Parent := WizardForm;
  CopyrightLabel.Caption := 'Visit Website: https://gurupia.github.io';
  CopyrightLabel.Left := ScaleX(16);
  CopyrightLabel.Top := WizardForm.BackButton.Top + ScaleY(4); 
  
  // Make it look like a link
  CopyrightLabel.Font.Color := clBlue;
  CopyrightLabel.Cursor := crHand;
  CopyrightLabel.Font.Style := [fsUnderline];
  
  // Bind Click Event
  CopyrightLabel.OnClick := @LinkClick;
end;

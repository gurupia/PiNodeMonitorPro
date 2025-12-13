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
  DeveloperLabel: TNewStaticText;
  CopyrightLabel: TNewStaticText;

procedure DeveloperLabelClick(Sender: TObject);
var
  ErrorCode: Integer;
begin
  // Open the website
  ShellExec('open', 'https://gurupia.github.io', '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
end;

procedure InitializeWizard;
begin
  // -----------------------------------------------------------------
  // Dark Theme Implementation
  // -----------------------------------------------------------------
  WizardForm.Color := $2b2b2b; // Dark Grey Background
  WizardForm.Font.Color := clWhite; // General Font Color
  
  // Update Standard Labels
  // Welcome Page
  WizardForm.WelcomeLabel1.Font.Color := clWhite;
  WizardForm.WelcomeLabel2.Font.Color := clWhite;
  
  // Finished Page
  WizardForm.FinishedLabel.Font.Color := clWhite;
  WizardForm.FinishedHeadingLabel.Font.Color := clWhite;
  
  // Inner Pages (Select Dir, Components, etc.)
  WizardForm.InnerPage.Color := $2b2b2b;
  WizardForm.MainPanel.Color := $2b2b2b;
  
  WizardForm.PageNameLabel.Font.Color := clWhite;
  WizardForm.PageDescriptionLabel.Font.Color := clSilver; // Slightly dimmer
  
  // Checkboxes & Radio Buttons (Make text transparent/white)
  // Converting standard controls is tricky, but setting parent font usually works for labels.
  
  // Bevels (Lines) - Hide or recolor if possible, mainly hide to keep clean
  WizardForm.BeveledLabel.Visible := False;

  // Enhanced Controls Coloring (Inputs, Memos, Lists)
  WizardForm.DirEdit.Color := $383838;
  WizardForm.DirEdit.Font.Color := clWhite;
  
  WizardForm.GroupEdit.Color := $383838;
  WizardForm.GroupEdit.Font.Color := clWhite;
  
  WizardForm.ReadyMemo.Color := $383838;
  WizardForm.ReadyMemo.Font.Color := clWhite;
  
  // Tasks List (Checkboxes)
  WizardForm.TasksList.Color := $2b2b2b;
  WizardForm.TasksList.Font.Color := clWhite;
  
  // -----------------------------------------------------------------
  // Footer: Developer & Copyright
  // -----------------------------------------------------------------
  
  // Copyright Label (Bottom Left)
  CopyrightLabel := TNewStaticText.Create(WizardForm);
  CopyrightLabel.Parent := WizardForm;
  CopyrightLabel.Caption := 'Copyright (c) 2025 GuruPia. All rights reserved.';
  CopyrightLabel.Font.Color := clSilver;
  CopyrightLabel.Font.Size := 8;
  CopyrightLabel.Top := WizardForm.ClientHeight - 25;
  CopyrightLabel.Left := 15;
  CopyrightLabel.Anchors := [akLeft, akBottom];

  // Developer Link (Bottom Left, stacked above Copyright)
  DeveloperLabel := TNewStaticText.Create(WizardForm);
  DeveloperLabel.Parent := WizardForm;
  DeveloperLabel.Caption := 'Developed by gurupia.github.io';
  DeveloperLabel.Cursor := crHandPoint;
  DeveloperLabel.Font.Color := $00ffff; // Yellow/Cyan mix
  DeveloperLabel.Font.Style := [fsBold, fsUnderline];
  DeveloperLabel.Font.Size := 9;
  DeveloperLabel.Top := WizardForm.ClientHeight - 45; // Moved up
  DeveloperLabel.Left := 15; // Moved to Left
  DeveloperLabel.Anchors := [akLeft, akBottom];
  DeveloperLabel.OnClick := @DeveloperLabelClick;
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  // Ensure custom labels stay on top/visible if needed
end;

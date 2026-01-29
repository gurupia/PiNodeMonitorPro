; Script generated for Pi Node Monitor Pro
; Usage: Compile with NSIS (makensis)

!define APPNAME "Pi Node Monitor Pro"
!define EXE_NAME "PiNodeMonitorWinForm.exe"
!define VERSION "1.0.0"
!define PUBLISHER "PiNodeMonitorTeam"

; Input directory where the built exe is located
!define SOURCE_DIR "Publish\win-x64-compressed"

SetCompressor /SOLID lzma

Name "${APPNAME}"
OutFile "PiNodeMonitorPro_Setup_NSIS.exe"
InstallDir "$PROGRAMFILES64\${APPNAME}"
RequestExecutionLevel admin

; [Copyright & Link Settings]
; Adds a clickable link to the bottom of the Welcome Page
!define MUI_WELCOMEPAGE_TEXT "This will install ${APPNAME} on your computer.$\r$\n$\r$\nIt is recommended that you close all other applications before continuing.$\r$\n$\r$\nClick Next to continue."
!define MUI_WELCOMEPAGE_BOTTOMTEXT "Check out the project: https://gurupia.github.io"
; Note: MUI doesn't have a simple 'click' field for bottom text by default without plugins.

; Let's try the BrandingText approach again but with a twist.
; In standard NSIS, making the bottom branding text clickable is hard.
; So we will put the link in the "Finish" page which supports it natively.

!define MUI_FINISHPAGE_LINK "Visit Developer Website: https://gurupia.github.io"
!define MUI_FINISHPAGE_LINK_LOCATION "https://gurupia.github.io"

BrandingText "Created by Gurupia"

; UI settings
!include "MUI2.nsh"
!define MUI_ABORTWARNING
; !define MUI_ICON "app.ico" ; Uncomment if you have an icon
; !define MUI_UNICON "app.ico"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_LANGUAGE "Korean"

Section "Install"
    SetOutPath "$INSTDIR"
    
    ; Check if source exists before compiling (for safety, though NSIS checks at compile time)
    File "${SOURCE_DIR}\${EXE_NAME}"
    File "${SOURCE_DIR}\GurupiaCapture.Core.dll"

    ; Create Uninstaller
    WriteUninstaller "$INSTDIR\uninstall.exe"

    ; Create Shortcuts
    CreateDirectory "$SMPROGRAMS\${APPNAME}"
    CreateShortcut "$SMPROGRAMS\${APPNAME}\${APPNAME}.lnk" "$INSTDIR\${EXE_NAME}"
    CreateShortcut "$SMPROGRAMS\${APPNAME}\Uninstall.lnk" "$INSTDIR\uninstall.exe"
    
    ; Desktop Shortcut
    CreateShortcut "$DESKTOP\${APPNAME}.lnk" "$INSTDIR\${EXE_NAME}"

    ; Add to Add/Remove Programs
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayName" "${APPNAME}"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "UninstallString" "$\"$INSTDIR\uninstall.exe$\""
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "Publisher" "${PUBLISHER}"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayVersion" "${VERSION}"
SectionEnd

Section "Uninstall"
    Delete "$INSTDIR\${EXE_NAME}"
    Delete "$INSTDIR\GurupiaCapture.Core.dll"
    Delete "$INSTDIR\uninstall.exe"
    RMDir "$INSTDIR"

    Delete "$SMPROGRAMS\${APPNAME}\${APPNAME}.lnk"
    Delete "$SMPROGRAMS\${APPNAME}\Uninstall.lnk"
    RMDir "$SMPROGRAMS\${APPNAME}"
    
    Delete "$DESKTOP\${APPNAME}.lnk"

    DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}"
SectionEnd

!include "MUI2.nsh"
!include "checkDotNet3.nsh"

!define MIN_FRA_MAJOR "3"
!define MIN_FRA_MINOR "5"
!define MIN_FRA_BUILD "*"


; The name of the installer
Name "Winslew"

; The file to write
OutFile "Setup-Winslew.exe"





; The default installation directory
InstallDir "$PROGRAMFILES\lI Ghun\Winslew\"

; Registry key to check for directory (so if you install again, it will 
; overwrite the old one automatically)
InstallDirRegKey HKLM "Software\lI Ghun\Winslew" "Install_Dir"

; Request application privileges for Windows Vista
RequestExecutionLevel admin


 


;--------------------------------

  !define MUI_ABORTWARNING



!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP "logoSetupSmall.bmp"
!define MUI_WELCOMEFINISHPAGE_BITMAP "logoSetupBig.bmp"
!define MUI_WELCOMEPAGE_TITLE "Winslew"
!define MUI_WELCOMEPAGE_TEXT "Read webpages whenever you have the time to$\r$\n$\r$\nPlease stop any instance of Winslew prior to installing this version."
!define MUI_STARTMENUPAGE_DEFAULTFOLDER "lI Ghun\Winslew"
!define MUI_ICON "..\Images\WinslewSetup.ico"
!define MUI_UNICON "uninstall.ico"


Var StartMenuFolder
; Pages

  !insertmacro MUI_PAGE_WELCOME
  !insertmacro MUI_PAGE_LICENSE "License.txt"
  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY

  !define MUI_STARTMENUPAGE_REGISTRY_ROOT "HKCU" 
  !define MUI_STARTMENUPAGE_REGISTRY_KEY "Software\lI Ghun\Winslew" 
  !define MUI_STARTMENUPAGE_REGISTRY_VALUENAME "Start Menu Folder"
  !insertmacro MUI_PAGE_STARTMENU Application $StartMenuFolder

  !insertmacro MUI_PAGE_INSTFILES
  !define MUI_FINISHPAGE_RUN "Winslew.exe"
  !insertmacro MUI_PAGE_FINISH




  !insertmacro MUI_UNPAGE_WELCOME
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  !insertmacro MUI_UNPAGE_FINISH





;--------------------------------




!insertmacro MUI_LANGUAGE "English"

; LoadLanguageFile "${NSISDIR}\Contrib\Language files\English.nlf"
;--------------------------------
;Version Information

  VIProductVersion "1.3.0.0"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "Winslew"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "lI Ghun"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" "© 2010 lI Ghun"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "Client for Read it Later"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" "1.0"







Function un.UninstallDirs
    Exch $R0 ;input string
    Exch
    Exch $R1 ;maximum number of dirs to check for
    Push $R2
    Push $R3
    Push $R4
    Push $R5
       IfFileExists "$R0\*.*" 0 +2
       RMDir "$R0"
     StrCpy $R5 0
    top:
     StrCpy $R2 0
     StrLen $R4 $R0
    loop:
     IntOp $R2 $R2 + 1
      StrCpy $R3 $R0 1 -$R2
     StrCmp $R2 $R4 exit
     StrCmp $R3 "\" 0 loop
      StrCpy $R0 $R0 -$R2
       IfFileExists "$R0\*.*" 0 +2
       RMDir "$R0"
     IntOp $R5 $R5 + 1
     StrCmp $R5 $R1 exit top
    exit:
    Pop $R5
    Pop $R4
    Pop $R3
    Pop $R2
    Pop $R1
    Pop $R0
FunctionEnd









; The stuff to install
Section "Winslew"

  SectionIn RO
  
  SetOutPath "$INSTDIR\\Images"
  File "..\Images\Winslew.png"
  File "..\Images\WinslewSetup.ico"
 
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  
  !insertmacro AbortIfBadFramework

  ; Put file there
  File "Documentation.URL"
  File "..\Images\WinslewSetup.ico"
  File "..\Winslew.exe"
  File "..\Winslew.pdb"
  ; File "..\Winslew.application"
  File "..\Winslew.exe.config"
  ; File "..\Winslew.exe.manifest"
  File "LICENSE.txt"
  File "Documentation.ico"
  
  ; Write the installation path into the registry
  WriteRegStr HKLM SOFTWARE\Winslew" "Install_Dir" "$INSTDIR"
  
  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Winslew" "DisplayName" "Winslew"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Winslew" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Winslew" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Winslew" "NoRepair" 1
  WriteUninstaller "uninstall.exe"



  
SectionEnd

; Optional section (can be disabled by the user)
Section "Start Menu Shortcuts"

!insertmacro MUI_STARTMENU_WRITE_BEGIN Application

  CreateDirectory "$SMPROGRAMS\$StartMenuFolder"
  CreateShortCut "$SMPROGRAMS\$StartMenuFolder\\Winslew.lnk" "$INSTDIR\Winslew.exe" "" "$INSTDIR\WinslewSetup.ico" 0
  CreateShortCut "$SMPROGRAMS\$StartMenuFolder\\Documentation.lnk" "$INSTDIR\Documentation.URL" "" $INSTDIR\Documentation.ico" 0
  CreateShortCut "$SMPROGRAMS\$StartMenuFolder\\Uninstall.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0
  
!insertmacro MUI_STARTMENU_WRITE_END

  
SectionEnd


;--------------------------------

; Uninstaller

Section "Uninstall"

  
  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\li ghun\Winslew"
  DeleteRegKey HKLM "Software\li ghun\Winslew"
  ; Remove files and uninstaller
  Delete $INSTDIR\*.*

  ; Remove shortcuts, if any
  !insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuFolder
    
  Delete "$SMPROGRAMS\$StartMenuFolder\\*.*"
  


  DeleteRegKey HKCU "Software\li ghun\Winslew"


  ; Remove directories used
   ; RMDir "$SMPROGRAMS\$StartMenuFolder"
Push 10 #maximum amount of directories to remove
  Push "$SMPROGRAMS\$StartMenuFolder" #input string
    Call un.UninstallDirs

   
  ; RMDir "$INSTDIR"
  
  Push 10 #maximum amount of directories to remove
  Push $INSTDIR #input string
    Call un.UninstallDirs


SectionEnd

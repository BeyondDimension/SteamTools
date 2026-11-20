Unicode true
RequestExecutionLevel admin
ManifestSupportedOS Win10
CRCCheck force
SetCompressor /SOLID lzma

!include MUI2.nsh
!include x64.nsh
!include WinVer.nsh
!include payload.nsh

!define PRODUCT "Watt Toolkit (Native AOT)"
!define INSTALL_KEY "Software\Steam++ AOT"
!define UNINSTALL_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\Steam++ AOT"

Name "${PRODUCT}"
OutFile "${OUTPUT}"
InstallDir "$PROGRAMFILES64\Watt Toolkit AOT"
VIProductVersion "${APP_VERSION}"
VIAddVersionKey "ProductName" "${PRODUCT}"
VIAddVersionKey "FileDescription" "${PRODUCT} Installer (${APP_ARCH})"
VIAddVersionKey "FileVersion" "${APP_VERSION}"
VIAddVersionKey "ProductVersion" "${APP_VERSION}"
VIAddVersionKey "LegalCopyright" "Copyright (C) BeyondDimension and contributors"

!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_LANGUAGE "SimpChinese"

Function .onInit
  SetRegView 32
  SetShellVarContext all
  ${IfNot} ${AtLeastBuild} 17763
    MessageBox MB_OK|MB_ICONSTOP "Watt Toolkit requires Windows 10 version 1809 or later." /SD IDOK
    SetErrorLevel 1
    Quit
  ${EndIf}
  !if "${APP_ARCH}" == "arm64"
    ${IfNot} ${IsNativeARM64}
  !else
    ${IfNot} ${IsNativeAMD64}
  !endif
    MessageBox MB_OK|MB_ICONSTOP "This installer requires Windows ${APP_ARCH}." /SD IDOK
    SetErrorLevel 1
    Quit
  ${EndIf}
  ReadRegStr $0 HKLM "${INSTALL_KEY}" "InstPath"
  ${If} $0 != ""
    MessageBox MB_OK|MB_ICONSTOP "Uninstall the installed Native AOT version first. Uninstalling does not delete your user data." /SD IDOK
    SetErrorLevel 1
    Quit
  ${EndIf}
FunctionEnd

Section "${PRODUCT}"
  IfFileExists "$INSTDIR\*.*" 0 install
    MessageBox MB_OK|MB_ICONSTOP "Choose a new install directory to avoid overwriting an existing installation." /SD IDOK
    SetErrorLevel 1
    Quit
  install:
  ClearErrors
  !insertmacro InstallPayload
  IfErrors installFailed
  SetOutPath "$INSTDIR"
  WriteUninstaller "$INSTDIR\uninstall.exe"
  IfErrors installFailed
  WriteRegStr HKLM "${INSTALL_KEY}" "InstPath" "$INSTDIR"
  WriteRegStr HKLM "${UNINSTALL_KEY}" "DisplayName" "${PRODUCT}"
  WriteRegStr HKLM "${UNINSTALL_KEY}" "DisplayVersion" "${APP_VERSION}"
  WriteRegStr HKLM "${UNINSTALL_KEY}" "Publisher" "BeyondDimension"
  WriteRegStr HKLM "${UNINSTALL_KEY}" "InstallLocation" "$INSTDIR"
  WriteRegStr HKLM "${UNINSTALL_KEY}" "DisplayIcon" "$INSTDIR\Steam++.exe"
  WriteRegStr HKLM "${UNINSTALL_KEY}" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "${UNINSTALL_KEY}" "NoModify" 1
  WriteRegDWORD HKLM "${UNINSTALL_KEY}" "NoRepair" 1
  CreateShortcut "$SMPROGRAMS\${PRODUCT}.lnk" "$INSTDIR\Steam++.exe"
  Goto done
  installFailed:
    MessageBox MB_OK|MB_ICONSTOP "Could not install the application files. Check disk space and directory permissions." /SD IDOK
    SetErrorLevel 1
    Abort
  done:
SectionEnd

Function un.onInit
  SetRegView 32
  SetShellVarContext all
  ReadRegStr $0 HKLM "${INSTALL_KEY}" "InstPath"
  ${If} $0 != $INSTDIR
    MessageBox MB_OK|MB_ICONSTOP "The registered install directory does not match this uninstaller." /SD IDOK
    SetErrorLevel 1
    Quit
  ${EndIf}
FunctionEnd

Section "Uninstall"
  ; Delete only files owned by this release. Nonempty directories and user data stay intact.
  !insertmacro UninstallPayload
  Delete "$SMPROGRAMS\${PRODUCT}.lnk"
  DeleteRegKey HKLM "${UNINSTALL_KEY}"
  DeleteRegKey HKLM "${INSTALL_KEY}"
  Delete "$INSTDIR\uninstall.exe"
  RMDir "$INSTDIR"
SectionEnd

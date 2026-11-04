; Windows Firewall rules for New World Desktop (protocol.md "Ports"), added on install and removed on uninstall.
; Inbound allow, for the app executable only, on the Private profile only (never Public or Domain):
;   - TCP 47801: signalling from the headset
;   - UDP 47800: discovery port
;   - UDP, any port: WebRTC media (Chromium picks the ports at run time)
; The installer runs elevated (perMachine), which netsh needs.

!define NW_RULE_TCP "New World Desktop - signalling (TCP 47801)"
!define NW_RULE_DISC "New World Desktop - discovery (UDP 47800)"
!define NW_RULE_RTC "New World Desktop - screen video (WebRTC UDP)"

!macro nwDeleteRules
  nsExec::ExecToLog 'netsh advfirewall firewall delete rule name="${NW_RULE_TCP}"'
  Pop $0
  nsExec::ExecToLog 'netsh advfirewall firewall delete rule name="${NW_RULE_DISC}"'
  Pop $0
  nsExec::ExecToLog 'netsh advfirewall firewall delete rule name="${NW_RULE_RTC}"'
  Pop $0
!macroend

!macro customInstall
  !insertmacro nwDeleteRules
  nsExec::ExecToLog 'netsh advfirewall firewall add rule name="${NW_RULE_TCP}" dir=in action=allow program="$INSTDIR\${APP_EXECUTABLE_FILENAME}" protocol=TCP localport=47801 profile=private enable=yes'
  Pop $0
  nsExec::ExecToLog 'netsh advfirewall firewall add rule name="${NW_RULE_DISC}" dir=in action=allow program="$INSTDIR\${APP_EXECUTABLE_FILENAME}" protocol=UDP localport=47800 profile=private enable=yes'
  Pop $0
  nsExec::ExecToLog 'netsh advfirewall firewall add rule name="${NW_RULE_RTC}" dir=in action=allow program="$INSTDIR\${APP_EXECUTABLE_FILENAME}" protocol=UDP profile=private enable=yes'
  Pop $0
!macroend

!macro customUnInstall
  !insertmacro nwDeleteRules
!macroend

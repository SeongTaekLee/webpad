@echo off
rem Build WebPad.exe with the C# compiler that ships with Windows (.NET Framework 4).
rem Needs the WebView2 SDK unpacked in launcher\webview2 (see launcher\get-webview2.cmd).
setlocal
cd /d "%~dp0.."
set CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%CSC%" set CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe
set WV=launcher\webview2
if not exist dist mkdir dist
"%CSC%" /nologo /codepage:65001 /optimize+ /target:winexe /platform:anycpu ^
  /win32icon:launcher\webpad.ico ^
  /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll ^
  /reference:%WV%\lib\net462\Microsoft.Web.WebView2.Core.dll /reference:%WV%\lib\net462\Microsoft.Web.WebView2.WinForms.dll ^
  /resource:%WV%\lib\net462\Microsoft.Web.WebView2.Core.dll,lib/Microsoft.Web.WebView2.Core.dll ^
  /resource:%WV%\lib\net462\Microsoft.Web.WebView2.WinForms.dll,lib/Microsoft.Web.WebView2.WinForms.dll ^
  /resource:%WV%\runtimes\win-x64\native\WebView2Loader.dll,bin/x64/WebView2Loader.dll ^
  /resource:%WV%\runtimes\win-x86\native\WebView2Loader.dll,bin/x86/WebView2Loader.dll ^
  /resource:%WV%\runtimes\win-arm64\native\WebView2Loader.dll,bin/arm64/WebView2Loader.dll ^
  /resource:index.html,app/index.html ^
  /resource:fonts\NanumGothicCoding-R.woff2,app/fonts/NanumGothicCoding-R.woff2 ^
  /resource:fonts\NanumGothicCoding-B.woff2,app/fonts/NanumGothicCoding-B.woff2 ^
  /resource:fonts\OFL.txt,app/fonts/OFL.txt ^
  /resource:icons\icon.svg,app/icons/icon.svg ^
  /out:dist\WebPad.exe launcher\WebPad.cs
endlocal

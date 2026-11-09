@echo off
rem Download the official WebView2 SDK from NuGet and unpack it into launcher\webview2.
setlocal
cd /d "%~dp0"
set VER=1.0.4258.31
curl -sL -o webview2.zip https://api.nuget.org/v3-flatcontainer/microsoft.web.webview2/%VER%/microsoft.web.webview2.%VER%.nupkg || exit /b 1
if exist webview2 rmdir /s /q webview2
powershell -NoProfile -Command "Expand-Archive -Path webview2.zip -DestinationPath webview2 -Force"
del webview2.zip
endlocal

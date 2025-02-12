@echo off
:: See https://github.com/VedalAI/neuro-game-sdk/neuro-html-game-runner/README.md for more information about this script.
set "port=7272"

:: This script uses https://github.com/static-web-server/static-web-server to serve webfiles on localhost.
:: Please choose the correct distribution for your OS and architecture.
set "swsUrl=https://github.com/static-web-server/static-web-server/releases/download/v2.34.0/static-web-server-v2.34.0-x86_64-pc-windows-msvc.zip"
set "swsDir=static-web-server-v2.34.0-x86_64-pc-windows-msvc"
set "swsExeSHA256=800468def5ae46beae94f398babbfe37cab6adf065338b71aa9b46b1da52f215"

set "swsfile=%~dp0\neuro_runner_deps\static-web-server.exe"
set "sdkUrl=https://github.com/VedalAI/neuro-game-sdk/archive/refs/tags/1.0.0.zip"
set "sdkDir=neuro-game-sdk-1.0.0"
set "depsDir=neuro-html-game-runner\neuro_runner_deps"
set "scriptfile=%~dp0\neuro_runner_deps\runner.ps1"
set "runnerSHA256=995a1cbed14aa76ecd051cf4b0a218481538fb4b191dd68b8c72f9fa0a1f5525"

:: Check if the sdk release folder exists
if not exist "%scriptfile%" (
    echo Downloading Runner Script...
    powershell -Command "Invoke-WebRequest -Uri '%sdkUrl%' -OutFile '%~dp0\sdk.zip'"
    echo Extracting Runner Script...
    powershell -Command "Expand-Archive -Path '%~dp0\sdk.zip' -DestinationPath '%~dp0' -Force"
    del "%~dp0\sdk.zip"
    move "%~dp0\%sdkDir%\%depsDir%" "%~dp0\neuro_runner_deps"
    rd /s /q "%~dp0\%sdkDir%"
)

if not exist "%scriptfile%" (
    echo Error: File '%scriptfile%' not found.
    exit /b 1
)

set "filechecksum="

:: Calculate SHA256 hash
for /f "skip=1 tokens=* delims=" %%# in ('certutil -hashfile "%scriptfile%" SHA256') do (
    set "filechecksum=%%#"
    goto :checksum_done
)
:checksum_done

:: Remove spaces from checksum
set "filechecksum=%filechecksum: =%"

:: Compare checksums
if /i "%runnerSHA256%"=="%filechecksum%" (
    echo Runner Script SHA256 checksum validated.
) else (
    echo Checksum validation failed for runner.ps1, got %filechecksum% expected %runnerSHA256%.
    echo Please reinstall by removing the neuro_runner_deps folder and running this script again, or update the checksum if the source is trusted.
    exit /b 1
)

if not exist "%swsfile%" (
    echo Downloading Static Web Server...
    powershell -Command "Invoke-WebRequest -Uri '%swsUrl%' -OutFile '%~dp0\sws.zip'"
    echo Extracting Static Web Server...
    powershell -Command "Expand-Archive -Path '%~dp0\sws.zip' -DestinationPath '%~dp0' -Force"
    del "%~dp0\sws.zip"
    move "%~dp0\%swsDir%\static-web-server.exe" "%~dp0\neuro_runner_deps\static-web-server.exe"
    rd /s /q "%~dp0\%swsDir%"
)

if not exist "%swsfile%" (
    echo Error: File '%swsfile%' not found.
    exit /b 1
)

set "filechecksum="

for /f "skip=1 tokens=* delims=" %%# in ('certutil -hashfile "%swsfile%" SHA256') do (
    set "filechecksum=%%#"
    goto :checksum_done
)
:checksum_done

:: Remove spaces from checksum
set "filechecksum=%filechecksum: =%"

:: Compare checksums
if /i "%swsExeSHA256%"=="%filechecksum%" (
    echo Static Web Server SHA256 checksum validated.
) else (
    echo Checksum validation failed for static-web-server.exe, got %filechecksum% expected %swsExeSHA256%.
    echo Please reinstall by removing the neuro_runner_deps folder and running this script again, or update the checksum if the source is trusted.
    exit /b 1
)

powershell -ExecutionPolicy Bypass -File "%scriptfile%" -port "%port%"

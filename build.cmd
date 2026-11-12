@echo off
rem Build the two product exes from src\ into dist\ (csc options live in
rem build.rsp and build.settings.rsp). The Chinese product names below need the
rem UTF-8 codepage, so switch first - the switch self-heals any console (V11).
chcp 65001 >nul
setlocal
set "REPO=%~dp0"
set "CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" (
    echo [FAIL] Compiler not found: %CSC%
    exit /b 1
)
if not exist "%REPO%src" (
    echo [FAIL] Missing source directory: %REPO%src
    exit /b 1
)
if not exist "%REPO%dist" mkdir "%REPO%dist"
pushd "%REPO%"
"%CSC%" -nologo "@build.rsp"
set "RC=%ERRORLEVEL%"
if "%RC%"=="0" "%CSC%" -nologo "@build.settings.rsp"
set "RC=%ERRORLEVEL%"
popd
if not "%RC%"=="0" (
    echo [FAIL] csc exit code %RC%
    exit /b %RC%
)
rem WinForms per-monitor DPI support config, deployed beside each exe (D2/D9).
copy /y "%REPO%app.config" "%REPO%dist\一键黑屏.exe.config" >nul
if errorlevel 1 (
    echo [FAIL] Could not deploy mask exe.config
    exit /b 1
)
copy /y "%REPO%app.config" "%REPO%dist\一键黑屏-设置.exe.config" >nul
if errorlevel 1 (
    echo [FAIL] Could not deploy settings exe.config
    exit /b 1
)
rem Explicit four-artifact assertion (D9): green only when both exes and both
rem configs are on disk - no glob, no neighbor assumptions.
set "MISSING="
if not exist "%REPO%dist\一键黑屏.exe" set "MISSING=dist\一键黑屏.exe"
if not exist "%REPO%dist\一键黑屏.exe.config" set "MISSING=dist\一键黑屏.exe.config"
if not exist "%REPO%dist\一键黑屏-设置.exe" set "MISSING=dist\一键黑屏-设置.exe"
if not exist "%REPO%dist\一键黑屏-设置.exe.config" set "MISSING=dist\一键黑屏-设置.exe.config"
if defined MISSING (
    echo [FAIL] Build artifact missing: %MISSING%
    exit /b 1
)
echo [OK] Build succeeded: dist\一键黑屏.exe + dist\一键黑屏-设置.exe + 2 configs
exit /b 0

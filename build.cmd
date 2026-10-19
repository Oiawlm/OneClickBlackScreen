@echo off
rem Build the mask tool from src\ into dist\ (csc options live in build.rsp).
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
popd
if not "%RC%"=="0" (
    echo [FAIL] csc exit code %RC%
    exit /b %RC%
)
set "FOUND="
for %%F in ("%REPO%dist\*.exe") do set "FOUND=%%F"
if not defined FOUND (
    echo [FAIL] No exe produced in dist
    exit /b 1
)
rem WinForms per-monitor DPI support config, deployed beside the exe.
copy /y "%REPO%app.config" "%FOUND%.config" >nul
if errorlevel 1 (
    echo [FAIL] Could not deploy exe.config
    exit /b 1
)
echo [OK] Build succeeded: %FOUND%
exit /b 0

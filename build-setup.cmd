@echo off
rem Build the per-user Inno Setup installer dist\OneClickBlackScreen-Setup-<ver>.exe
rem from setup\OneClickBlackScreen.iss. The version comes from src\AppInfo.cs
rem only (single source, D4) - it is extracted below and passed to ISCC via
rem /DAppVersion; never hand-write a version into the .iss or this file.
rem ISCC probe order: 1) PATH ("where iscc")  2) C:\Program Files (x86)\Inno Setup 6
rem   3) C:\Program Files (x86)\Inno Setup 7  4) D:\App\Inno Setup 7 (this repo's
rem   build machine). All missing -> FAIL: install Inno Setup 6/7, don't switch
rem   packagers (RESEARCH-V9 D2).
rem Usage: build-setup.cmd          (fresh build.cmd first, then ISCC)
rem        For the isolated self-test variant use ISCC directly, e.g.:
rem          ISCC /DTestId /DAppVersion=<x.y.z> /DDefaultDataRoot="<TEMP path>\BlackScreen" setup\OneClickBlackScreen.iss
rem        TestId builds REQUIRE /DDefaultDataRoot (isolated default data
rem        root, V11 D10 - the script refuses to compile without it).
rem        (see docs\handoff\2026-11-12-v11-singleform\VERIFY-V11B.md).
setlocal
set "REPO=%~dp0"

rem 1) Fresh dist via the normal build (deploys both exes + both .exe.config).
call "%REPO%build.cmd"
if errorlevel 1 exit /b 1

rem 2) Extract the setup asset name prefix and version from the single source
rem    (V11 D3 four-way interlock: AppInfo.cs SetupAssetNamePrefix + Version
rem    must concatenate to exactly what the .iss emits as OutputBaseFilename,
rem    else the product-exists check below fails with an explicit error).
set "VER="
set "PREFIX="
rem (options escaped unquoted so the double quote itself is the delimiter)
for /f tokens^=2^ delims^=^" %%A in ('findstr /c:"public const string Version = " "%REPO%src\AppInfo.cs"') do set "VER=%%A"
for /f tokens^=2^ delims^=^" %%A in ('findstr /c:"public const string SetupAssetNamePrefix = " "%REPO%src\AppInfo.cs"') do set "PREFIX=%%A"
if not defined VER (
    echo [FAIL] Could not read Version from src\AppInfo.cs
    exit /b 1
)
if not defined PREFIX (
    echo [FAIL] Could not read SetupAssetNamePrefix from src\AppInfo.cs ^(V11 D3 four-way asset-name interlock^)
    exit /b 1
)

rem 3) Locate the Inno Setup compiler (order documented in the header above).
set "ISCC="
where iscc >nul 2>nul && for /f "delims=" %%P in ('where iscc') do if not defined ISCC set "ISCC=%%P"
if not defined ISCC if exist "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" set "ISCC=C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
if not defined ISCC if exist "C:\Program Files (x86)\Inno Setup 7\ISCC.exe" set "ISCC=C:\Program Files (x86)\Inno Setup 7\ISCC.exe"
if not defined ISCC if exist "D:\App\Inno Setup 7\ISCC.exe" set "ISCC=D:\App\Inno Setup 7\ISCC.exe"
if not defined ISCC (
    echo [FAIL] ISCC not found - install Inno Setup 6/7 ^(probe order in this file's header^)
    exit /b 1
)

rem 4) Compile the installer (ASCII product name, L-0015/D4).
"%ISCC%" /DAppVersion=%VER% "%REPO%setup\OneClickBlackScreen.iss"
if errorlevel 1 (
    echo [FAIL] ISCC exit code %ERRORLEVEL%
    exit /b 1
)

set "PRODUCT=%REPO%dist\%PREFIX%%VER%.exe"
if not exist "%PRODUCT%" (
    echo [FAIL] Setup exe missing after compile: %PRODUCT%
    echo        Expected name = SetupAssetNamePrefix + Version from src\AppInfo.cs
    echo        ^(V11 D3 interlock: keep it in lockstep with OutputBaseFilename in setup\OneClickBlackScreen.iss^)
    exit /b 1
)
echo [OK] Setup built: %PRODUCT%
exit /b 0

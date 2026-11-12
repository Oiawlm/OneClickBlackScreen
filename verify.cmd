@echo off
rem Compile and run all eight verifiers (V11 D9): explicit per-verifier targets -
rem the mask exe serves the overlay/idle verifiers, the settings exe serves the
rem settings verifier, and the icon verifier checks both. No glob, no ambiguity.
rem Note: the overlay verifiers briefly show a fullscreen mask - that is expected.
chcp 65001 >nul
setlocal
set "REPO=%~dp0"
set "CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" (
    echo [FAIL] Compiler not found: %CSC%
    exit /b 1
)
set "MASKEXE=%REPO%dist\一键黑屏.exe"
set "SETEXE=%REPO%dist\一键黑屏-设置.exe"
if not exist "%MASKEXE%" (
    echo [FAIL] Missing %MASKEXE% - run build.cmd first
    exit /b 1
)
if not exist "%SETEXE%" (
    echo [FAIL] Missing %SETEXE% - run build.cmd first
    exit /b 1
)
set "BIN=%REPO%verify\bin"
if not exist "%BIN%" mkdir "%BIN%"
pushd "%REPO%"
"%CSC%" -nologo -target:exe -optimize+ -langversion:5 -r:System.dll -r:System.Drawing.dll -out:verify\bin\VerifyConfig.exe verify\VerifyConfig.cs verify\VerifyGate.cs src\MaskConfig.cs src\AppPaths.cs
if errorlevel 1 goto :failbuild
"%CSC%" -nologo -target:exe -optimize+ -langversion:5 -r:System.dll -r:System.Drawing.dll -r:System.Windows.Forms.dll -out:verify\bin\VerifySettings.exe verify\VerifySettings.cs verify\VerifyGate.cs src\ThemePalette.cs src\SettingsDialogMetrics.cs src\AppPaths.cs
if errorlevel 1 goto :failbuild
"%CSC%" -nologo -target:exe -optimize+ -langversion:5 -r:System.dll -r:System.Drawing.dll -r:System.Windows.Forms.dll -out:verify\bin\VerifyMouseExit.exe verify\VerifyMouseExit.cs verify\VerifyGate.cs
if errorlevel 1 goto :failbuild
"%CSC%" -nologo -target:exe -optimize+ -langversion:5 -r:System.dll -r:System.Drawing.dll -r:System.Windows.Forms.dll -out:verify\bin\VerifyOverlay.exe verify\VerifyOverlay.cs verify\VerifyGate.cs src\OverlayForm.cs src\MaskPainter.cs src\MaskConfig.cs src\AppPaths.cs
if errorlevel 1 goto :failbuild
"%CSC%" -nologo -target:exe -optimize+ -langversion:5 -r:System.dll -r:System.Drawing.dll -r:System.Windows.Forms.dll -out:verify\bin\VerifyIcon.exe verify\VerifyIcon.cs verify\VerifyGate.cs
if errorlevel 1 goto :failbuild
"%CSC%" -nologo -target:exe -optimize+ -langversion:5 -r:System.dll -out:verify\bin\VerifyUpdate.exe verify\VerifyUpdate.cs verify\VerifyGate.cs src\AppInfo.cs src\Updater.cs
if errorlevel 1 goto :failbuild
"%CSC%" -nologo -target:exe -optimize+ -langversion:5 -r:System.dll -out:verify\bin\VerifyIdle.exe verify\VerifyIdle.cs verify\VerifyGate.cs src\IdleWake.cs
if errorlevel 1 goto :failbuild
"%CSC%" -nologo -target:exe -optimize+ -langversion:5 -r:System.dll -out:verify\bin\VerifyPaths.exe verify\VerifyPaths.cs verify\VerifyGate.cs src\AppPaths.cs
if errorlevel 1 goto :failbuild
popd

echo [1/8] VerifyConfig
"%BIN%\VerifyConfig.exe"
if errorlevel 2 goto :busy
if errorlevel 1 goto :fail
echo [2/8] VerifySettings
"%BIN%\VerifySettings.exe" "%SETEXE%" "%MASKEXE%"
if errorlevel 2 goto :busy
if errorlevel 1 goto :fail
echo [3/8] VerifyMouseExit
call :holdreal || goto :restorefail
"%BIN%\VerifyMouseExit.exe" "%MASKEXE%"
set "RC=%ERRORLEVEL%"
call :restorereal || goto :restorefail
if "%RC%"=="2" goto :busy
if not "%RC%"=="0" goto :fail
echo [4/8] VerifyOverlay
call :holdreal || goto :restorefail
"%BIN%\VerifyOverlay.exe" "%MASKEXE%"
set "RC=%ERRORLEVEL%"
call :restorereal || goto :restorefail
if "%RC%"=="2" goto :busy
if not "%RC%"=="0" goto :fail
echo [5/8] VerifyIcon
"%BIN%\VerifyIcon.exe" "%MASKEXE%" "%SETEXE%"
if errorlevel 2 goto :busy
if errorlevel 1 goto :fail
echo [6/8] VerifyUpdate
"%BIN%\VerifyUpdate.exe"
if errorlevel 2 goto :busy
if errorlevel 1 goto :fail
echo [7/8] VerifyIdle
call :holdreal || goto :restorefail
"%BIN%\VerifyIdle.exe" "%MASKEXE%"
set "RC=%ERRORLEVEL%"
call :restorereal || goto :restorefail
if "%RC%"=="2" goto :busy
if not "%RC%"=="0" goto :fail
echo [8/8] VerifyPaths
"%BIN%\VerifyPaths.exe"
if errorlevel 2 goto :busy
if errorlevel 1 goto :fail
echo [OK] ALL VERIFIERS PASSED
exit /b 0

:failbuild
popd
echo [FAIL] Verifier compilation failed
exit /b 1

:fail
echo [FAIL] A verifier failed
exit /b 1

:busy
echo [FAIL] VERIFY BUSY - another verifier run holds the session gate
exit /b 2

rem ---- suite-level real-user-state isolation (V11, AGENTS.md §2 改名隔离) ----
rem VerifyMouseExit/VerifyOverlay/VerifyIdle launch the app and rely on the
rem USERPROFILE seam, which a real DataDirectory registration (V10-S7 onward)
rem beats: their children would resolve - and could write - the user's real
rem registered data root. Hold the real key aside for exactly their spans so
rem the children resolve the redirected default root instead. reg save/restore
rem needs backup privileges, so the roundtrip is export/delete/import (proven
rem lossless across REG_SZ/DWORD/EXPAND_SZ/BINARY + subkeys). VerifySettings
rem carries its own complete in-process hold and runs OUTSIDE this wrapper so
rem its three-state tripwires stay non-vacuous; the pure verifiers
rem (Config/Icon/Update/Paths) launch no children at all.
:holdreal
set "HOLDREG=%TEMP%\BlackScreenVerifyHold.reg"
reg query "HKCU\Software\BlackScreen" >nul 2>&1
if errorlevel 1 exit /b 0
reg export "HKCU\Software\BlackScreen" "%HOLDREG%" /y >nul 2>&1
if errorlevel 1 (
    echo [FAIL] Could not export the real Software\BlackScreen key for isolation
    exit /b 1
)
reg delete "HKCU\Software\BlackScreen" /f >nul 2>&1
if errorlevel 1 (
    echo [FAIL] Could not move the real Software\BlackScreen key aside
    del /q "%HOLDREG%" >nul 2>&1
    exit /b 1
)
exit /b 0

:restorereal
if not exist "%HOLDREG%" exit /b 0
reg import "%HOLDREG%" >nul 2>&1
if errorlevel 1 exit /b 1
del /q "%HOLDREG%" >nul 2>&1
exit /b 0

:restorefail
echo [FAIL] Could not restore the real Software\BlackScreen key - recover it manually with: reg import "%TEMP%\BlackScreenVerifyHold.reg"
exit /b 1

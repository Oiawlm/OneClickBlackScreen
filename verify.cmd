@echo off
rem Compile and run all verifiers against the exe in dist\.
rem Note: the overlay verifiers briefly show a fullscreen mask - that is expected.
setlocal
set "REPO=%~dp0"
set "CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" (
    echo [FAIL] Compiler not found: %CSC%
    exit /b 1
)
set "TARGET="
for %%F in ("%REPO%dist\*.exe") do set "TARGET=%%F"
if not defined TARGET (
    echo [FAIL] No exe in dist - run build.cmd first
    exit /b 1
)
set "BIN=%REPO%verify\bin"
if not exist "%BIN%" mkdir "%BIN%"
pushd "%REPO%"
"%CSC%" -nologo -target:exe -optimize+ -langversion:5 -r:System.dll -r:System.Drawing.dll -out:verify\bin\VerifyConfig.exe verify\VerifyConfig.cs verify\VerifyGate.cs src\MaskConfig.cs
if errorlevel 1 goto :failbuild
"%CSC%" -nologo -target:exe -optimize+ -langversion:5 -r:System.dll -r:System.Drawing.dll -r:System.Windows.Forms.dll -out:verify\bin\VerifySettings.exe verify\VerifySettings.cs verify\VerifyGate.cs src\ThemePalette.cs src\SettingsDialogMetrics.cs
if errorlevel 1 goto :failbuild
"%CSC%" -nologo -target:exe -optimize+ -langversion:5 -r:System.dll -r:System.Drawing.dll -r:System.Windows.Forms.dll -out:verify\bin\VerifyMouseExit.exe verify\VerifyMouseExit.cs verify\VerifyGate.cs
if errorlevel 1 goto :failbuild
"%CSC%" -nologo -target:exe -optimize+ -langversion:5 -r:System.dll -r:System.Drawing.dll -r:System.Windows.Forms.dll -out:verify\bin\VerifyOverlay.exe verify\VerifyOverlay.cs verify\VerifyGate.cs
if errorlevel 1 goto :failbuild
"%CSC%" -nologo -target:exe -optimize+ -langversion:5 -r:System.dll -r:System.Drawing.dll -out:verify\bin\VerifyIcon.exe verify\VerifyIcon.cs verify\VerifyGate.cs
if errorlevel 1 goto :failbuild
"%CSC%" -nologo -target:exe -optimize+ -langversion:5 -r:System.dll -out:verify\bin\VerifyUpdate.exe verify\VerifyUpdate.cs verify\VerifyGate.cs src\AppInfo.cs src\Updater.cs
if errorlevel 1 goto :failbuild
"%CSC%" -nologo -target:exe -optimize+ -langversion:5 -r:System.dll -out:verify\bin\VerifyIdle.exe verify\VerifyIdle.cs verify\VerifyGate.cs src\IdleWake.cs
if errorlevel 1 goto :failbuild
popd

echo [1/7] VerifyConfig
"%BIN%\VerifyConfig.exe"
if errorlevel 2 goto :busy
if errorlevel 1 goto :fail
echo [2/7] VerifySettings
"%BIN%\VerifySettings.exe" "%TARGET%"
if errorlevel 2 goto :busy
if errorlevel 1 goto :fail
echo [3/7] VerifyMouseExit
"%BIN%\VerifyMouseExit.exe" "%TARGET%"
if errorlevel 2 goto :busy
if errorlevel 1 goto :fail
echo [4/7] VerifyOverlay
"%BIN%\VerifyOverlay.exe" "%TARGET%"
if errorlevel 2 goto :busy
if errorlevel 1 goto :fail
echo [5/7] VerifyIcon
"%BIN%\VerifyIcon.exe" "%TARGET%"
if errorlevel 2 goto :busy
if errorlevel 1 goto :fail
echo [6/7] VerifyUpdate
"%BIN%\VerifyUpdate.exe"
if errorlevel 2 goto :busy
if errorlevel 1 goto :fail
echo [7/7] VerifyIdle
"%BIN%\VerifyIdle.exe" "%TARGET%"
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

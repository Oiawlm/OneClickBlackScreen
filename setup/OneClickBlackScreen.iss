; OneClickBlackScreen per-user installer script (V9 B-track, RESEARCH-V9 §3 D2).
; Build via build-setup.cmd: it extracts the version from src\AppInfo.cs and
; passes /DAppVersion=<x.y.z>; this script refuses to compile without it, so
; the version stays single-sourced (D4 - never hand-write a version here).
; Isolation self-test builds add /DTestId, which swaps the AppId, the idle
; task name and the output suffix, so a test install can never touch real
; user state or the real "Add/Remove Programs" entry (RESEARCH-V9 §3 D5).
; Encoding: UTF-8 with BOM (Chinese texts and paths require it), CRLF lines.

#ifndef AppVersion
  #error AppVersion missing: compile via build-setup.cmd, which passes /DAppVersion=<x.y.z> extracted from src\AppInfo.cs
#endif

; Fixed product identity, generated once (V9B). The value carries the inner
; "{{" brace pair so that after ISPP expansion the directive reads exactly
; the documented literal form "AppId={{GUID}" (a single leading "{" would
; parse as a constant). MUST stay byte-identical to the GUID inside
; UninstallKeyPath in src\Updater.cs - the updater syncs the "Add/Remove
; Programs" DisplayVersion through "{AppId}_is1" and a mismatch would
; silently disconnect the two. The two files cross-reference each other;
; change both together or neither.
#define RealAppId "{{5A17668F-45A0-48F0-B5B1-067E37EFA98D}"

; Test identity never equals RealAppId, so its uninstall key, directory and
; task name live in a disjoint namespace from any real installation.
#ifndef TestAppId
  #define TestAppId "{{V9B-ISOLATION-TEST}"
#endif

#ifdef TestId
  #define PublicAppId TestAppId
  #define OutputSuffix "-TEST"
  #define IdleTaskName "OneClickBlackScreen-Idle-TEST"
#else
  #define PublicAppId RealAppId
  #define OutputSuffix ""
  ; The product owns this task (the settings dialog creates/removes it); the
  ; uninstaller only sweeps it if still registered on this machine.
  #define IdleTaskName "OneClickBlackScreen-Idle"
#endif

[Setup]
; Same AppId + per-user install = Inno's native in-place upgrade semantics:
; an overwrite install reuses the previous directory and entry (D2).
AppId={#PublicAppId}
AppName=一键黑屏
AppVersion={#AppVersion}
AppVerName=一键黑屏 {#AppVersion}
PrivilegesRequired=lowest
DefaultDirName={autopf}\一键黑屏
DefaultGroupName=一键黑屏
; Keep the Start Menu folder wizard page enabled (see the /DTestId note at
; the [Icons] section for why silent-mode icon suppression cannot rely on
; /NOICONS, which only pre-checks that page's checkbox).
OutputDir=..\dist
OutputBaseFilename=OneClickBlackScreen-Setup-{#AppVersion}{#OutputSuffix}
VersionInfoVersion={#AppVersion}
UninstallDisplayIcon={app}\一键黑屏.exe
Compression=lzma2
SolidCompression=yes
WizardStyle=modern

; Chinese core texts for install / finish / uninstall, overriding the built-in
; English defaults inline - zero external .isl dependency (D2). Keys and
; placeholders ([name], [mb], %1, %n) follow Inno Setup's Default.isl.
[Messages]
SetupWindowTitle=%1 安装程序
WelcomeLabel1=欢迎使用 [name] 安装向导
WelcomeLabel2=此向导将在您的电脑上安装 [name/ver]。%n%n建议在继续之前关闭其他所有应用程序。
SelectDirDesc=要在哪里安装 [name]？
SelectDirLabel3=安装程序将把 [name] 安装到以下文件夹。
SelectDirBrowseLabel=点击「安装」继续。如需选择其他文件夹，请点击「浏览」。
DiskSpaceMBLabel=安装至少需要 [mb] MB 的可用磁盘空间。
SelectStartMenuFolderDesc=要把程序的快捷方式放在哪里？
SelectStartMenuFolderLabel3=安装程序将在以下开始菜单文件夹中创建程序的快捷方式。
SelectStartMenuFolderBrowseLabel=点击「下一步」继续。如需选择其他文件夹，请点击「浏览」。
NoProgramGroupCheck2=不创建开始菜单文件夹(&D)
MustEnterGroupName=必须输入文件夹名称。
GroupNameTooLong=文件夹名称或路径过长。
InvalidGroupName=文件夹名称无效。
StatusCreateIcons=正在创建快捷方式...
ButtonBack=上一步(&B)
ButtonNext=下一步(&N)
ButtonInstall=安装(&I)
ButtonCancel=取消
ButtonFinish=完成(&F)
ButtonBrowse=浏览(&B)...
ExitSetupMessage=安装尚未完成。如果现在退出，程序将不会被安装。%n%n您可以之后重新运行安装程序来完成安装。%n%n退出安装吗？
InstallingLabel=请稍候，安装程序正在您的电脑上安装 [name]。
FinishedHeadingLabel=正在完成 [name] 安装向导
FinishedLabelNoIcons=安装程序已在您的电脑上安装 [name]。
FinishedLabel=安装程序已在您的电脑上安装 [name]。通过已安装的快捷方式即可启动程序。
ConfirmUninstall=确定要完全移除 %1 及其全部组件吗？
UninstallStatusLabel=请稍候，正在从您的电脑移除 %1。
UninstalledAll=%1 已成功地从您的电脑移除。%n%n个人设置文件夹未被删除（位于用户 AppData\Local 目录下的 BlackScreen 文件夹）；如需彻底清理，请手动删除该文件夹。
SelectTasksDesc=选择附加任务
SelectTasksLabel2=请选择安装程序要执行的附加任务，然后点击「安装」继续。

; Exactly the four payload files the zip ships (D2): the app installs nothing
; else - default settings self-heal inside the code, so no settings file is
; ever pre-seeded (pre-seeding would be a second source of truth).
[Files]
Source: "..\dist\一键黑屏.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\dist\一键黑屏.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\dist\icon-settings.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\使用说明.txt"; DestDir: "{app}"; Flags: ignoreversion

; Icon creation is inherently a write to the user's real Start Menu and
; Desktop, so isolation builds (/DTestId) compile the whole mechanism out:
; /NOICONS cannot do this - it only pre-checks a checkbox on the wizard's
; Start Menu page and is ignored by /VERYSILENT (confirmed empirically on
; Inno 7.1.1 and against jrsoftware's command-line docs). A test install
; must be UNABLE to touch real shortcuts, not merely asked nicely (D5);
; the real build's shortcut path is verified by the integration track's
; real-machine smoke test instead.
#ifndef TestId
; Desktop entry is opt-in: the task checkbox ships unchecked (D2).
[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式(&D)"; Flags: unchecked

[Icons]
Name: "{group}\一键黑屏"; Filename: "{app}\一键黑屏.exe"
Name: "{group}\一键黑屏-设置"; Filename: "{app}\一键黑屏.exe"; Parameters: "--settings"; IconFilename: "{app}\icon-settings.ico"
Name: "{autodesktop}\一键黑屏"; Filename: "{app}\一键黑屏.exe"; Tasks: desktopicon
#endif

[UninstallRun]
; Sweep the idle task if the user still has it; schtasks failing on an
; absent task is harmless - Inno does not check [UninstallRun] exit codes.
; The settings-folder note rides on UninstalledAll above (D2: never delete
; user settings on uninstall).
Filename: "{sys}\schtasks.exe"; Parameters: "/delete /tn {#IdleTaskName} /f"; Flags: runhidden; RunOnceId: "RemoveIdleTask"

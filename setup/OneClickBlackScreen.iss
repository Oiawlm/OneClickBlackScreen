; OneClickBlackScreen per-user installer script (V9 B-track, RESEARCH-V9 §3 D2;
; V10 B-track: silent-update relaunch, data-location wizard page, install
; ownership registration and full uninstall, RESEARCH-V10 §3 D2/D4-D6 + §7).
; Build via build-setup.cmd: it extracts the version from src\AppInfo.cs and
; passes /DAppVersion=<x.y.z>; this script refuses to compile without it, so
; the version stays single-sourced (D4 - never hand-write a version here).
; Isolation self-test builds add /DTestId, which swaps the AppId, the idle
; task name, the output suffix and the data-registry root (-TEST suffix), so
; a test install can never touch real user state or the real "Add/Remove
; Programs" entry (RESEARCH-V9 §3 D5, RESEARCH-V10 §7 last row).
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

; Single-sourced names shared by directives and [Code] (V10): the installed
; exe name (taskkill target, [Run] relaunch, icons) and the app's existing
; single-instance mutex (Program.cs:28) used by both AppMutex and the
; force-close probe. DataRegistryRoot carries the frozen HKCU key root of
; the cross-track contract (RESEARCH-V10 §7 row 1); isolation builds append
; "-TEST" so the whole data-registration surface (InstallDirectory /
; DataDirectory / DataParentDirectory) is disjoint from the real one.
#define AppExeName "一键黑屏.exe"
#define AppMutexName "Local\BlackScreen.Mask"

#ifdef TestId
  #define PublicAppId TestAppId
  #define OutputSuffix "-TEST"
  #define IdleTaskName "OneClickBlackScreen-Idle-TEST"
  #define DataRegistryRoot "Software\BlackScreen-TEST"
#else
  #define PublicAppId RealAppId
  #define OutputSuffix ""
  ; The product owns this task (the settings dialog creates/removes it); the
  ; uninstaller only sweeps it if still registered on this machine.
  #define IdleTaskName "OneClickBlackScreen-Idle"
  #define DataRegistryRoot "Software\BlackScreen"
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
UninstallDisplayIcon={app}\{#AppExeName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
; Silent-update support (V10 D2, fts-isomorphic): AppMutex makes Setup see a
; running mask; InitializeSetup force-closes it via taskkill, and if that
; fails AppMutex (plus the updater's /FORCECLOSEAPPLICATIONS riding Restart
; Manager through CloseApplications) still keeps Setup from overwriting the
; running exe. Restart Manager must not restart the closed mask itself -
; the [Run] entry below is the single deterministic relaunch path, for
; interactive and silent installs alike.
AppMutex={#AppMutexName}
CloseApplications=yes
RestartApplications=no

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
; V10 D5: uninstall removes the managed data root as well - the prompt tells
; the user up front (informed, never asked separately).
ConfirmUninstall=确定要完全移除 %1 及其全部组件（含个人设置数据）吗？
UninstallStatusLabel=请稍候，正在从您的电脑移除 %1。
; V10 D5: settings now go with the uninstall; the old "kept, delete manually"
; note is gone. Failure paths message separately in [Code].
UninstalledAll=%1 已成功地从您的电脑移除。
SelectTasksDesc=选择附加任务
SelectTasksLabel2=请选择安装程序要执行的附加任务，然后点击「安装」继续。
; Restart Manager texts surfaced when a running mask is closed for an
; overwrite install (V10 D2); faithful translations, zero semantic change -
; silent updates never display them.
ApplicationsFound=以下应用程序正在使用需要被更新的文件。建议允许安装程序自动关闭这些应用程序。
ApplicationsFound2=以下应用程序正在使用需要被更新的文件。建议允许安装程序自动关闭这些应用程序。安装完成后，安装程序将尝试重新启动这些应用程序。
CloseApplications=自动关闭应用程序(&A)
DontCloseApplications=不关闭应用程序(&D)
ErrorCloseApplications=安装程序无法自动关闭所有应用程序。建议在继续之前关闭所有正在使用需要被更新的文件的应用程序。
StatusClosingApplications=正在关闭应用程序...
StatusRestartingApplications=正在重启应用程序...

; Exactly the four payload files the zip ships (D2): the app installs nothing
; else - default settings self-heal inside the code, so no settings file is
; ever pre-seeded (pre-seeding would be a second source of truth).
[Files]
Source: "..\dist\{#AppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\dist\{#AppExeName}.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\dist\icon-settings.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\使用说明.txt"; DestDir: "{app}"; Flags: ignoreversion

; Install-ownership registration (V10 D6): InitializeUninstall compares this
; value against the directory recorded in its own uninstall log and refuses
; to run on mismatch, so a leftover uninstaller from a previous location can
; never delete a newer installation's files or data. uninsdeletevalue keeps
; the normal-uninstall state clean (the D5 zero-residue fallback). The two
; data-location values are deliberately NOT declared here: they exist only
; in the non-default state (D3), are written by [Code] at install time, and
; must survive a refused or guarded uninstall (see InitializeUninstall).
[Registry]
Root: HKCU; Subkey: "{#DataRegistryRoot}"; ValueType: string; ValueName: "InstallDirectory"; ValueData: "{app}"; Flags: uninsdeletevalue

; Icon creation and the post-install relaunch are inherently user-visible
; side effects on the real desktop, so isolation builds (/DTestId) compile
; the whole mechanism out: /NOICONS cannot do this - it only pre-checks a
; checkbox on the wizard's Start Menu page and is ignored by /VERYSILENT
; (confirmed empirically on Inno 7.1.1 and against jrsoftware's
; command-line docs). A test install must be UNABLE to touch real shortcuts
; or launch a real fullscreen mask, not merely asked nicely (D5); the real
; build's shortcut and relaunch paths are verified by the integration
; track's real-machine smoke test instead.
#ifndef TestId
; Desktop entry is opt-in: the task checkbox ships unchecked (D2).
[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式(&D)"; Flags: unchecked

[Icons]
Name: "{group}\一键黑屏"; Filename: "{app}\{#AppExeName}"
Name: "{group}\一键黑屏-设置"; Filename: "{app}\{#AppExeName}"; Parameters: "--settings"; IconFilename: "{app}\icon-settings.ico"
Name: "{autodesktop}\一键黑屏"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

; Post-install relaunch (V10 D2): no skipifsilent by design - a silent
; update install restarts the mask automatically once Setup finishes, and
; an interactive install keeps the normal finish-page checkbox.
[Run]
Filename: "{app}\{#AppExeName}"; Description: "启动 一键黑屏"; Flags: nowait
#endif

[UninstallRun]
; Sweep the idle task if the user still has it; schtasks failing on an
; absent task is harmless - Inno does not check [UninstallRun] exit codes.
; User settings are handled separately in [Code] CurUninstallStepChanged
; (V10 D5: the managed data root is removed under triple guards).
Filename: "{sys}\schtasks.exe"; Parameters: "/delete /tn {#IdleTaskName} /f"; Flags: runhidden; RunOnceId: "RemoveIdleTask"

[Code]
const
  // Frozen cross-track contract (RESEARCH-V10 §7): the key root arrives via
  // the ISPP define so isolation builds live under the -TEST suffix.
  DataRegistryKey = '{#DataRegistryRoot}';
  DataDirectoryValue = 'DataDirectory';
  DataParentDirectoryValue = 'DataParentDirectory';
  InstallDirectoryValue = 'InstallDirectory';
  ManagedRootLeaf = 'BlackScreen';

var
  DataParentPage: TInputDirWizardPage;
  SelectedDataParent: String;
  UninstallDataRoot: String;
  UninstallDataRootValid: Boolean;

// Extended device paths (\\?\, \\.\) are aliases of ordinary paths; rejecting
// them keeps every guard comparison below operating on canonical spellings.
function IsExtendedDevicePath(const Value: String): Boolean;
begin
  Result := (Length(Value) >= 4) and (Value[1] = '\') and
    (Value[2] = '\') and ((Value[3] = '?') or (Value[3] = '.')) and
    (Value[4] = '\');
end;

// Fully qualified local ("X:\...") or UNC ("\\server\share\...") paths only.
function IsFullyQualifiedPath(const Value: String): Boolean;
var
  Candidate: String;
  Index: Integer;
begin
  Candidate := Trim(Value);
  if IsExtendedDevicePath(Candidate) then
  begin
    Result := False;
    Exit;
  end;

  if (Length(Candidate) >= 3) and (Candidate[2] = ':') and
    (Candidate[3] = '\') then
  begin
    Result := True;
    Exit;
  end;

  Result := False;
  if (Length(Candidate) < 5) or (Candidate[1] <> '\') or
    (Candidate[2] <> '\') then
    Exit;

  for Index := 3 to Length(Candidate) - 1 do
  begin
    if Candidate[Index] = '\' then
    begin
      Result := True;
      Exit;
    end;
  end;
end;

// Canonical form for comparisons and registrations: fully qualified, no
// trailing backslash unless the path is a root ('' when not fully qualified).
function NormalizeDirectory(const Value: String): String;
begin
  if not IsFullyQualifiedPath(Value) then
  begin
    Result := '';
    Exit;
  end;
  Result := RemoveBackslashUnlessRoot(ExpandFileName(Trim(Value)));
end;

// Drive roots ("C:\") and share roots ("\\server\share").
function IsRootDirectory(const Value: String): Boolean;
var
  Normalized: String;
  WithoutTrailingBackslash: String;
  Index: Integer;
  BackslashCount: Integer;
begin
  if not IsFullyQualifiedPath(Value) then
  begin
    Result := False;
    Exit;
  end;
  Normalized := ExpandFileName(Trim(Value));
  Result := (Length(Normalized) = 3) and (Normalized[2] = ':') and
    (Normalized[3] = '\');
  if Result or (Normalized[1] <> '\') or (Normalized[2] <> '\') then
    Exit;

  WithoutTrailingBackslash := Normalized;
  while (Length(WithoutTrailingBackslash) > 2) and
    (WithoutTrailingBackslash[Length(WithoutTrailingBackslash)] = '\') do
  begin
    Delete(WithoutTrailingBackslash, Length(WithoutTrailingBackslash), 1);
  end;

  BackslashCount := 0;
  for Index := 1 to Length(WithoutTrailingBackslash) do
  begin
    if WithoutTrailingBackslash[Index] = '\' then
      BackslashCount := BackslashCount + 1;
  end;
  Result := BackslashCount = 3;
end;

function IsSameDirectory(const A, B: String): Boolean;
begin
  Result := IsFullyQualifiedPath(A) and IsFullyQualifiedPath(B) and
    (CompareText(NormalizeDirectory(A), NormalizeDirectory(B)) = 0);
end;

function DefaultDataParent: String;
begin
  Result := ExpandConstant('{localappdata}');
end;

// The single managed shape (RESEARCH-V10 §7): <selected parent>\BlackScreen
// - the folder itself is the data root.
function BuildDataRoot(const ParentDirectory: String): String;
begin
  Result := AddBackslash(NormalizeDirectory(ParentDirectory)) + ManagedRootLeaf;
end;

function DefaultDataRoot: String;
begin
  Result := BuildDataRoot(DefaultDataParent);
end;

function GetManagedDataParent(const DataRoot: String): String;
begin
  Result := RemoveBackslashUnlessRoot(ExtractFilePath(
    RemoveBackslashUnlessRoot(DataRoot)));
end;

function IsManagedDataDirectory(const Value: String): Boolean;
var
  Raw: String;
  Normalized: String;
  DataParent: String;
begin
  // Frozen §7 shape, identical to the app side's AppPaths judgment: a fully
  // qualified path whose leaf is exactly "BlackScreen" and whose selected
  // parent is neither a drive nor a share root ("C:\BlackScreen" is
  // rejected - data must live inside a real parent directory). The raw
  // trimmed value itself must end in \BlackScreen - normalization only
  // canonicalizes for the parent checks and never rescues a malformed
  // registration (a trailing separator stays rejected, app-side isomorphic).
  // Deletion and registration both hinge on this single predicate.
  Result := False;
  Raw := Trim(Value);
  if not IsFullyQualifiedPath(Raw) then
    Exit;
  if CompareText(ExtractFileName(Raw), ManagedRootLeaf) <> 0 then
    Exit;
  Normalized := NormalizeDirectory(Raw);
  DataParent := GetManagedDataParent(Normalized);
  if (DataParent = '') or IsRootDirectory(DataParent) then
    Exit;
  Result := CompareText(Normalized, BuildDataRoot(DataParent)) = 0;
end;

function ReadExistingDataRoot: String;
var
  RegisteredValue: String;
begin
  // The registered value is only trusted when shape-valid (D3); anything
  // else falls back to the default, mirroring the app side's AppPaths.
  if RegQueryStringValue(HKCU, DataRegistryKey, DataDirectoryValue,
    RegisteredValue) and IsManagedDataDirectory(RegisteredValue) then
    Result := NormalizeDirectory(RegisteredValue)
  else
    Result := DefaultDataRoot;
end;

// The existing root has passed the shape check, so deriving the parent from
// it (instead of trusting DataParentDirectory) is the authoritative prefill.
function ReadExistingDataParent: String;
begin
  Result := GetManagedDataParent(ReadExistingDataRoot);
end;

function GetUniqueProbeFileName(const ParentDirectory: String): String;
var
  Attempt: Integer;
begin
  Result := '';
  for Attempt := 1 to 100 do
  begin
    Result := AddBackslash(ParentDirectory) + '.write-probe-' +
      GetDateTimeString('yyyymmddhhnnss', #0, #0) + '-' + IntToStr(Attempt);
    if not FileOrDirExists(Result) then
      Exit;
  end;
  Result := '';
end;

function ValidateDataParentDirectory: Boolean;
var
  ParentDirectory: String;
  ProbeFileName: String;
begin
  // Full path / non-root / creatable / writable probe, fts-isomorphic.
  Result := False;
  if Trim(DataParentPage.Values[0]) = '' then
  begin
    MsgBox('请选择数据存储文件夹。', mbError, MB_OK);
    Exit;
  end;

  if not IsFullyQualifiedPath(DataParentPage.Values[0]) then
  begin
    MsgBox('数据存储文件夹必须是完整的 Windows 路径。', mbError, MB_OK);
    Exit;
  end;

  ParentDirectory := NormalizeDirectory(DataParentPage.Values[0]);
  if IsRootDirectory(ParentDirectory) then
  begin
    MsgBox('数据存储文件夹不能是磁盘或网络共享根目录。', mbError, MB_OK);
    Exit;
  end;

  if not ForceDirectories(ParentDirectory) then
  begin
    MsgBox('无法创建数据存储文件夹。请选择其他位置。', mbError, MB_OK);
    Exit;
  end;

  ProbeFileName := GetUniqueProbeFileName(ParentDirectory);
  if ProbeFileName = '' then
  begin
    MsgBox('无法为数据存储文件夹创建验证文件。请选择其他位置。', mbError, MB_OK);
    Exit;
  end;
  try
    if not SaveStringToFile(ProbeFileName, 'OneClickBlackScreen', False) then
    begin
      MsgBox('数据存储文件夹不可写。请选择其他位置。', mbError, MB_OK);
      Exit;
    end;
  finally
    if FileExists(ProbeFileName) then
      DeleteFile(ProbeFileName);
  end;

  DataParentPage.Values[0] := ParentDirectory;
  Result := True;
end;

procedure ForceCloseRunningApplication;
var
  ResultCode: Integer;
begin
  // Only reach for taskkill when the mask's single-instance mutex is held;
  // on failure AppMutex (plus /FORCECLOSEAPPLICATIONS via Restart Manager)
  // still keeps Setup from overwriting a running exe.
  if CheckForMutexes('{#AppMutexName}') then
  begin
    if not Exec(
      ExpandConstant('{sys}\taskkill.exe'),
      '/F /IM "{#AppExeName}"',
      '',
      SW_HIDE,
      ewWaitUntilTerminated,
      ResultCode) then
    begin
      Log('Failed to start taskkill; AppMutex will keep setup from overwriting the running application.');
    end;
  end;
end;

function InitializeSetup: Boolean;
begin
  ForceCloseRunningApplication;
  Result := True;
end;

procedure InitializeWizard;
begin
  // "数据存储位置" page right after SelectDir (D4): the user picks the
  // PARENT folder; the managed <parent>\BlackScreen root is derived. The
  // prefill mirrors the current effective location, so a silent update
  // install re-registers exactly what is registered (or nothing by default).
  DataParentPage := CreateInputDirPage(wpSelectDir,
    '数据存储位置',
    '选择个人设置数据的存储文件夹',
    '安装程序将在所选文件夹中创建并管理「BlackScreen」文件夹（个人设置数据全部保存在里面）；保持默认位置则不写入位置登记。',
    False,
    '新建文件夹');
  DataParentPage.Add('数据存储文件夹：');
  DataParentPage.Values[0] := ReadExistingDataParent;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  if CurPageID = DataParentPage.ID then
    Result := ValidateDataParentDirectory
  else
    Result := True;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  // Freezes the effective selection. Runs in interactive AND silent
  // installs; silent installs never show the page, so the prefill value
  // stands and a default machine stays default (zero registration, D3).
  SelectedDataParent := NormalizeDirectory(DataParentPage.Values[0]);
  if SelectedDataParent = '' then
    SelectedDataParent := DefaultDataParent;
  Result := '';
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  DataRoot: String;
begin
  // Registration commit (D3/D4): custom parent -> write both values (and
  // make sure the root exists); default parent -> zero-registration state,
  // clearing any stale custom registration so switching back fully reverts.
  // The installer only REGISTERS the new location - moving existing data is
  // the settings dialog's migration pipeline (D4), never done here.
  if CurStep = ssPostInstall then
  begin
    if IsSameDirectory(SelectedDataParent, DefaultDataParent) then
    begin
      RegDeleteValue(HKCU, DataRegistryKey, DataDirectoryValue);
      RegDeleteValue(HKCU, DataRegistryKey, DataParentDirectoryValue);
      Log('Data location default: registration absent/cleared.');
    end
    else
    begin
      DataRoot := BuildDataRoot(SelectedDataParent);
      if ForceDirectories(DataRoot) and
        RegWriteStringValue(HKCU, DataRegistryKey, DataParentDirectoryValue,
          SelectedDataParent) and
        RegWriteStringValue(HKCU, DataRegistryKey, DataDirectoryValue,
          DataRoot) then
        Log('Data location registered: ' + DataRoot)
      else
      begin
        Log('Failed to register data location: ' + DataRoot);
        SuppressibleMsgBox('无法登记数据存储位置（' + DataRoot +
          '）。程序将使用默认数据位置。', mbError, MB_OK, IDOK);
      end;
    end;
  end;
end;

function InitializeUninstall: Boolean;
var
  RegisteredDirectory: String;
  RegisteredDataRoot: String;
begin
  // Ownership guard (D6): {app} inside the uninstaller resolves from its own
  // log, i.e. the directory this uninstaller was installed into. A leftover
  // uninstaller from a previous location never matches the current
  // InstallDirectory registration and is refused; a missing registration
  // (hand-edited registry) is refused the same way - re-running Setup
  // repairs it before uninstalling. Nothing is modified on refusal.
  Result := False;
  if not RegQueryStringValue(HKCU, DataRegistryKey, InstallDirectoryValue,
    RegisteredDirectory) or
    not IsSameDirectory(RegisteredDirectory, ExpandConstant('{app}')) then
  begin
    SuppressibleMsgBox('此卸载程序不属于当前登记的安装位置。请使用当前安装目录中的卸载程序，或重新安装本程序修复登记后再卸载。当前程序、个人设置数据和登记均未修改。',
      mbError, MB_OK, IDOK);
    Exit;
  end;

  // Resolve the data root to remove (D5): a registered value is only
  // trusted when shape-valid; anything malformed keeps both the data and
  // the registration untouched for manual cleanup.
  UninstallDataRoot := '';
  UninstallDataRootValid := False;
  if RegQueryStringValue(HKCU, DataRegistryKey, DataDirectoryValue,
    RegisteredDataRoot) then
  begin
    if IsManagedDataDirectory(RegisteredDataRoot) then
    begin
      UninstallDataRoot := NormalizeDirectory(RegisteredDataRoot);
      UninstallDataRootValid := True;
    end
    else
    begin
      Log('Refusing to remove malformed registered data directory: ' +
        RegisteredDataRoot);
      MsgBox('无法安全删除已登记的数据目录：' + RegisteredDataRoot +
        '。个人设置数据与登记值已保留，请手动清理。', mbError, MB_OK);
    end;
  end
  else
  begin
#ifdef TestId
    // Isolation builds must never touch the real %LOCALAPPDATA%\BlackScreen:
    // with no test registration there is nothing test-owned to delete. The
    // real build's default-root deletion is covered by the integration
    // track's real-machine smoke test.
    Log('Test build: no data directory registered; skipping default-root deletion.');
    UninstallDataRoot := '';
    UninstallDataRootValid := True;
#else
    UninstallDataRoot := DefaultDataRoot;
    UninstallDataRootValid := True;
#endif
  end;
  Result := True;
end;

function DeleteManagedDataRoot(const DataRoot: String): Boolean;
begin
  // Shape re-check on the deletion path (D5 triple guard: ownership in
  // InitializeUninstall, shape here, and DelTree only ever targets the
  // managed root itself - never the selected parent or its siblings).
  Result := False;
  if not IsManagedDataDirectory(DataRoot) then
  begin
    Log('Refusing to delete an unmanaged data directory: ' + DataRoot);
    Exit;
  end;
  Result := DelTree(NormalizeDirectory(DataRoot), True, True, True);
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  // Full uninstall (D5): remove the resolved data root, then clear the data
  // registration. Deletion failure only asks for manual cleanup - the
  // registration is kept so the location stays discoverable. The idle task
  // is swept by [UninstallRun] and InstallDirectory by uninsdeletevalue.
  if CurUninstallStep = usUninstall then
  begin
    if not UninstallDataRootValid then
      Exit;

    if (UninstallDataRoot <> '') and DirExists(UninstallDataRoot) and
      not DeleteManagedDataRoot(UninstallDataRoot) then
    begin
      Log('Failed to remove managed data directory: ' + UninstallDataRoot);
      SuppressibleMsgBox('部分个人设置数据未能删除：' + UninstallDataRoot +
        '。请关闭占用文件的程序后手动删除该文件夹。', mbError, MB_OK, IDOK);
      Exit;
    end;

    RegDeleteValue(HKCU, DataRegistryKey, DataDirectoryValue);
    RegDeleteValue(HKCU, DataRegistryKey, DataParentDirectoryValue);
  end;
end;

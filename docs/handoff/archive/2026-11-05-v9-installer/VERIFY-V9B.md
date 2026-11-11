# VERIFY-V9B · B 轨（安装包与分发面）执行证据

> **一句话定位**：B 轨执行窗实测留档——Inno Setup per-user 安装包（.iss＋build-setup.cmd）、Updater-ARP 联动、README/使用说明 分发面改写的全部真实命令输出；隔离自测零残留实录；七件套验证结果。　**执行窗**：GLM-5.3·目标模式　**worktree**：`OneClickBlackScreen-WorkTree\v9-b`（分支 `feature/installer-v9`）　**日期**：2026-11-05

## 0. 环境与前置闸门

- ISCC 探测实录（GOAL §1 顺序）：

```
$ where iscc          → NO_PATH_ISCC（PATH 无）
$ ls "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" → 不存在
$ ls "C:\Program Files (x86)\Inno Setup 7\ISCC.exe" → 不存在
$ ls "C:\Program Files\Inno Setup 6/7\ISCC.exe"     → 不存在
reg query HKLM/HKCU "...\Uninstall\Inno Setup 6_is1" → 三处均无
```

- 标准位全缺 → 按闸门停下报告；用户答复软件实际位于 **`D:\App\Inno Setup 7`**（ISCC 7.1.1 实测可用：`Inno Setup 7 Command-Line Compiler, Copyright 1997-2026 Jordan Russell`）。`build-setup.cmd` 探测链已补入该位（PATH → x86 IS6 → x86 IS7 → `D:\App\Inno Setup 7`）。
- worktree 建立：`git ls-remote --heads origin feature/installer-v9` 输出空（无撞名）→ `git worktree add "../OneClickBlackScreen-WorkTree/v9-b" -b feature/installer-v9 origin/main`（基点 31a0a34）。

## 1. P1/P2 · 安装脚本与构建脚本（真实现象）

- **AppId（一次性生成，固定留档）**：`5A17668F-45A0-48F0-B5B1-067E37EFA98D`
  - `setup/OneClickBlackScreen.iss`：`#define RealAppId "{{5A17668F-45A0-48F0-B5B1-067E37EFA98D}"` → `AppId={#PublicAppId}`（ISPP 展开后恰为文档字面形 `AppId={{GUID}`；单 `{` 会被当常量解析——首编译实测报错后修正）
  - `src/Updater.cs`：`UninstallKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\{5A17668F-45A0-48F0-B5B1-067E37EFA98D}_is1"`——两文件注释互指、互证一致（双处出现，本节为第三处留档）
- **版本单源**：`.iss` 头部 `#ifndef AppVersion #error ...` 拒绝裸编译；`build-setup.cmd` 以 `for /f tokens^=2^ delims^=^" ... findstr /c:"public const string Version = " src\AppInfo.cs` 提取 `1.0.2` 传 `/DAppVersion=1.0.2`（.iss/文档零手写版本号；`src/AppInfo.cs` 零改动）
- **构建实录**（exit 0）：

```
$ cmd /c build-setup.cmd
[OK] Build succeeded: ...\dist\一键黑屏.exe        ← 先跑 build.cmd 保证 dist 新鲜
Inno Setup 7.1.1 ... Successful compile (1.750 sec). Resulting Setup program filename is:
D:\...\v9-b\dist\OneClickBlackScreen-Setup-1.0.2.exe
[OK] Setup built: D:\...\v9-b\dist\OneClickBlackScreen-Setup-1.0.2.exe
$ ls -l dist/OneClickBlackScreen-Setup-1.0.2.exe
-rwxr-xr-x 2142508  dist/OneClickBlackScreen-Setup-1.0.2.exe   ← ASCII 名（L-0015），2.1MB（LZMA2+Solid）
```

- **D2 参数对照**（R 轨复检用，逐项）：

| D2 定案 | .iss 落点 | 实证 |
|---|---|---|
| `PrivilegesRequired=lowest` | [Setup] 原样 | 测试装仅写 HKCU/用户目录，零提权 |
| `DefaultDirName={autopf}\一键黑屏` | [Setup] 原样 | InstallLocation 实测＝TEMP 覆装值；真实装解析到用户 Programs |
| 固定 AppId | `#define RealAppId "{{…}}"` | §1 首条；同 AppId 覆盖安装＝Inno 原生语义，未破坏 |
| `#define` 三入口 | `AppVersion`（build-setup 传）/`TestAppId`/`TestId` | 测试变体 `/DTestId /DAppVersion=1.0.2` 编译实测成功 |
| 安装内容四件 | [Files]：`..\dist\一键黑屏.exe`＋`.exe.config`＋`icon-settings.ico`＋`..\使用说明.txt` | P5 [C1] 目录实测四件齐 |
| 快捷方式：开始菜单×2＋「设置」`--settings`＋icon-settings.ico 图标 | [Icons] `{group}\一键黑屏` / `{group}\一键黑屏-设置 Parameters:"--settings" IconFilename:{app}\icon-settings.ico` | 首轮实测创建（15:35:53）→修复后隔离变体不编入（见 §2 教训） |
| 桌面「一键黑屏」默认不勾 | [Tasks] `desktopicon Flags: unchecked` ＋ [Icons] `Tasks: desktopicon` | 注：`unchecked` 是 [Tasks] 旗标而非 [Icons]（首编译实测报错修正） |
| [Messages] 中文化核心文案、零 .isl | 27 条中文覆盖（欢迎/目录/磁盘/组页/按钮/退出确认/安装中/完成/卸载确认/卸载中/卸载完成含设置保留提示）；键名逐一对照 IS7 `Default.isl` 核实 | 编译过；键名＝官方键（`DiskSpaceMBLabel` 用 `[mb]` 占位、复选框键为 `NoProgramGroupCheck2` 均实测核对） |
| [UninstallRun] 删计划任务容错 | `Filename:"{sys}\schtasks.exe"; Parameters:"/delete /tn {#IdleTaskName} /f"; Flags: runhidden`；Inno 不检查该节退出码 | 卸载 exit 0（任务不存在时亦不炸，实测两轮） |
| 卸载完成页提示设置目录保留 | `UninstalledAll=%1 已成功地…个人设置文件夹未被删除（…BlackScreen…）请手动删除` | [Messages] 落地 |
| 安装器不预置设置文件 | [Files] 无任何 ini；设置自愈在代码内 | P5 全程 settings.ini ABSENT |

## 2. P5 · 隔离自测（零用户状态触碰）——含一次设计缺陷的发现与根修

**第一轮实测抓到缺陷**：装后开始菜单哈希漂移（基线 `d605bea0…`→`edae3929…`），find 实证 `Start Menu\Programs\一键黑屏\` 两个 .lnk 为安装器 15:35:53 所建。根因链（官方文档＋日志取证）：
1. 首判 `DisableProgramGroupPage=yes`（本窗自创、非 D2 参数）废掉组页 → 删除该参数（已附防回归注释）；
2. 复测仍复现——安装日志第 4 行证实命令行确含 `/NOICONS` 而图标仍建：**`/NOICONS` 只是向导复选框的预勾选提示，`/VERYSILENT` 下对无条件 [Icons] 条目无效**（[jrsoftware 官方命令行文档](https://jrsoftware.org/ishelp/topic_setupcmdline.htm)：「Instructs Setup to initially check the 'Don't create any icons' check box」，社区同证）；
3. **根修**：隔离变体（`/DTestId`）在预处理层直接不编入 [Tasks]/[Icons]——隔离测试从构造上不可能写真实面（D5），真实构建的快捷方式路径由 I 轨真机冒烟验证。该结论已写入 .iss 注释防回归。

**修复后完整循环实录**（快照存 `%TEMP%\v9b-snap2\`）：

基线（before.txt）：

```
[B1] settings.ini：ABSENT
[B2] 计划任务 OneClickBlackScreen-Idle：ABSENT；OneClickBlackScreen-Idle-TEST：ABSENT
[B3] autopf(%LOCALAPPDATA%\Programs) 清单哈希：f6aa7c81…4e8c06
[B4] 开始菜单清单哈希：d605bea0…3bd16fb
[B5] 桌面清单哈希：f045ae7d…f0e1c10
[B6] 真实 AppId 卸载键：ABSENT      [B7] 测试 AppId 卸载键：ABSENT
```

编译＋静默装（`/DIR=%TEMP%\v9b-setup-test /VERYSILENT /SUPPRESSMSGBOXES /LOG=…`）：

```
INSTALL_EXIT=0
[C1] TEMP 目录 6 件＝四载荷＋unins000.exe＋unins000.dat：
    110145 icon-settings.ico / 2143 unins000.dat / 4483034 unins000.exe
    267776 一键黑屏.exe / 245 一键黑屏.exe.config / 9223 使用说明.txt
[C2] HKCU\…\Uninstall\{V9B-ISOLATION-TEST}_is1：
    DisplayVersion = REG_SZ 1.0.2   ✓（版本与 AppInfo.cs 单源一致）
    DisplayName    = REG_SZ 一键黑屏 1.0.2
    InstallLocation= REG_SZ C:\Users\Lenovo\AppData\Local\Temp\v9b-setup-test\
[C3] 真实面三哈希（装中实测）＝基线逐字节一致：
    autopf f6aa7c81…/startmenu d605bea0…/desktop f045ae7d…  ✓
[C4] 真实 AppId 键 ABSENT；两任务名均 ABSENT；settings.ini ABSENT  ✓
```

静默卸载（`unins000.exe /VERYSILENT /SUPPRESSMSGBOXES`）＋终检：

```
UNINSTALL_EXIT=0
[D1] 零残留：TEMP 目录 GONE / 测试键 GONE / 开始菜单组 GONE  ✓
[D2] 三态终检＝基线全等：ini ABSENT、real-task ABSENT、test-task ABSENT、
    autopf f6aa7c81…/startmenu d605bea0…/desktop f045ae7d…、真实键 ABSENT  ✓
```

（首轮缺陷循环的卸载亦实测：卸载器自清了它建的快捷方式组——哈希回到基线 `d605bea0…`，顺带实证卸载清理链。）

## 3. P3 · Updater-ARP 联动（D3）代码路径留档

- 调用点：`src/Updater.cs` `ReplaceSequence` 内 `File.Copy` 成功后（回滚路径/字节相同早退路径均不写）——真实更新路径＝`SettingsDialog.cs:759` 两参直调，签名未动（`verify/VerifyUpdate.cs` 两参调用零改动即兼容）。
- 写入值＝**从刚落位的 targetPath 反射读取其 `AppInfo.Version` 常量**（运行中进程的 `AppInfo.Version` 是旧值，直接写必错；常量字段反射读取不执行目标代码；读取失败→跳过写入而非写旧值）。
- no-op 语义：`Registry.CurrentUser.OpenSubKey(…, true)` 返回 null（键不存在＝便携/未安装用户）→ 直接 return，全程 try/catch best-effort——**swap 已成功，ARP 元数据失败绝不回头报错**（VerifyUpdate 绿即此实证，见 §4）。
- `using Microsoft.Win32;`（.NET FW 4.x 中 RegistryKey 在 mscorlib，无新增引用）；`build.cmd` 绿。

## 4. P6 · 七件套验证——**全七件全绿（[OK] ALL VERIFIERS PASSED）**

执行通道说明（如实）：本窗代理侧执行 verify.cmd 连续七次被工具权限层拒绝（无弹窗、无输出，执行前即取消）；最终由用户以本窗备好的桌面脚本（ASCII＋CRLF，内容＝`cd /d <worktree>`＋`call verify.cmd`＋日志重定向）双击执行，日志落 `%TEMP%\v9b-verify2.log`，以下为逐字转录（同一 worktree、同一 dist 构建）：

```
[1/7] VerifyConfig
PASS: 23 config tests (defaults/roundtrip/format/golden/corruption/escapes/atomic-write/idle-key)
[2/7] VerifySettings
PASS: --settings opened exactly one closable non-mask window, exit code 0
PASS: two --settings instances coexisted and both closed with exit code 0
PASS: unknown argument showed usage dialog, zero mask windows, exit code 0
PASS: settings background follows AppsUseLightTheme (light, expected #F5F5F5)
PASS: Enter in the multiline box inserted a newline, raised no dialog, left the config untouched
PASS: Enter on the dialog (focus outside the text box) saved the preset text via the confirmation path
PASS: geometry mirrored SettingsDialogMetrics (+-1px), text box font locked the body tier, window inside working area, primary button locked Accent
PASS Z: user's real settings.ini untouched (SHA-256 before == after)
PASS TOTAL: 6 settings checks (3 original + theme background + Enter contract + geometry mirror)
[3/7] VerifyMouseExit
PASS: Left: 6 moves and 2 wheel messages kept open; down kept open; up exited
PASS: Right: 6 moves and 2 wheel messages kept open; down kept open; up exited
PASS: Middle: 6 moves and 2 wheel messages kept open; down kept open; up exited
PASS: X1: 6 moves and 2 wheel messages kept open; down kept open; up exited
PASS: X2: 6 moves and 2 wheel messages kept open; down kept open; up exited
PASS: Left click under phantom held key exited
PASS TOTAL: 5/5 buttons; 30 movement messages; 10 wheel messages; 1 windows per launch; phantom-key click exit OK
[4/7] VerifyOverlay
PASS A: no config = pure black v1 behavior, 1 windows, exact bounds, topmost, borderless
PASS B: configured color + text rendered, auto-contrast text pixels found
PASS C: color-only config = solid mask on every screen
PASS D: key 0x87 foreground exit, code 0
PASS D: key 0x87 background exit, code 0
PASS D: key 0x1B foreground exit, code 0
PASS D: key 0x1B background exit, code 0
PASS D: key 0x10 foreground exit, code 0
PASS D: key 0x10 background exit, code 0
PASS D: key 0x12 foreground exit, code 0
PASS E: held key blocked arming, unarmed key ignored, exit after release
PASS F: second instance self-exited silently, first stayed alive
PASS G: held launch key auto-repeat passed through; exit after release
PASS H: phantom held key did not block arming; SPACE exited
PASS Z: user's real settings.ini untouched (SHA-256 before == after)
PASS TOTAL: A-H; 1 displays; max InputIdle 102 ms (limit 2000)
[5/7] VerifyIcon
PASS: icon-main.ico structure ok (7 tiers, in-bounds, all entries decodable)
PASS: icon-settings.ico structure ok (7 tiers, in-bounds, all entries decodable)
PASS: exe embeds icon-main 256px PNG payload (offset 261408)
PASS: settings.ico managed resource byte-identical to artifact and decodable
PASS: dist\icon-settings.ico deployed byte-identical (110145 bytes)
PASS TOTAL: 4 icon checks
[6/7] VerifyUpdate
PASS: version comparison table (newer/equal/older/malformed, fail-closed)
PASS: full chain (check -> 302 download -> digest verify -> replace) on a temp copy
PASS: tampered digest refused, target unchanged
PASS: SHA256SUMS fallback completed the chain
PASS: digest and SHA256SUMS both missing/mismatched refused
PASS: size mismatch refused
PASS: 404 / malformed JSON / missing asset fail closed
PASS: equal and malformed tags read as no update
PASS: replace sequence (stale .old cleared, rollback restored, blocked pre-clean)
PASS: concurrent replaces serialize - one effective swap, second flow no-op, backup intact (x50)
PASS TOTAL: update checks (version table + offline full chain)
[7/7] VerifyIdle
PASS 1: task xml carries the C3 field set, quotes the exe, writes UTF-16 BOM
PASS 2: Ensure -> query exit 0 -> Remove -> query exit 1 (task cleaned up)
PASS 3: active-session --idle exited within 2 s, exit code 0, zero windows
PASS 4: ShouldShow decision table (8 cases, threshold edges included)
PASS 5: live IdleMilliseconds() = 63 ms (0 <= reading < 60000)
PASS TOTAL: 5 idle checks (xml fields / scheduler loop / silent exit / decision table / live reading)
[OK] ALL VERIFIERS PASSED
```

**VerifyUpdate 全 PASS ＝ 验收第 7 条 no-op 实证**：改后的 Updater.cs（含 ARP 联动）完整过「下载→校验→替换→回滚→并发序列化×50」链；本机真实 AppId 键 ABSENT（§2 快照）→ SyncUninstallVersion 全程早退零写入。另：本窗在代理通道被拦期间曾以 verify.cmd 原生编译行单跑纯件三件（Config/Icon/Update）亦全 PASS（与上录 [1][5][6] 一致），作为旁证留档。

## 5. P4 · 分发面文档改写

- `README.md`（UTF-8 无 BOM＋CRLF 保真，`file` 复验；diff 20+/12-）：下载与安装节改「Setup 推荐（含开始菜单快捷方式与卸载器）＋zip 便携备选」三列表（下载/适合谁/用法）＋Setup 卸载说明；SmartScreen 表述保留；v1.0.2 版本 token 逐字保留（I 轨 P2 负责联动 1.1.0）。本地开发节 install.ps1 条目→build-setup.cmd 条目。开发过程文档表两行修正：archive 行 V1~V7 45 份→V1~V8（45＋10，实测清点）并标注 v8 批次整目录路径；当前批次行死链（D8 归档后 `docs/handoff/2026-11-05-v8-structure/` 已不存在）→指向 `2026-11-05-v9-installer/`。改前已加载 github-readme skill（GOAL 铁律）。
- `使用说明.txt`（UTF-8 BOM＋CRLF 保真，字节级脚本替换＋断言，diff 18+/8-）：新增【安装】节（Setup 推荐/zip 备选）；【两种启动方式】消除 DeskBox 时代 `.lnk` 表述；【边界】「不需要安装」→「无需管理员权限（安装包/便携二选一）」；卸载拆「安装包版（设置>应用）／便携版（手动）」两径；【开发者提示】install.ps1/DeskBox 消除＋build-setup.cmd 补入＋过期文档路径（`docs\VERIFY-V1.md` 等）修正为 `docs\handoff\`（archive 归档说明）。
- 禁字样终检：`grep -ri "install\.ps1\|deskbox" README.md 使用说明.txt` → **零命中**（exit 1）。

## 6. 铁律自查

- `git status`/diff 落面＝`setup/OneClickBlackScreen.iss`(新)＋`build-setup.cmd`(新)＋`README.md`＋`使用说明.txt`＋`src/Updater.cs`＋`docs/handoff/2026-11-05-v9-installer/VERIFY-V9B.md`(新)——**全部在 §4 白名单内，白名单外零 diff**（`git diff --name-status origin/main...HEAD` 逐条核对，见推送后远端）。
- 禁改件实测：`src/AppInfo.cs` 零 diff（版本仍 1.0.2）；`verify.cmd`/`verify/**`/`install.ps1`/`migrate/**`/`docs/lessons.md`/`build.cmd`/`build.rsp`/`AGENTS.md` 零 diff；主检出只读（全程仅 worktree 内改动）。
- ISCC/Inno 二进制零入库（产物在 gitignore 的 dist/ 内）；证据全部为真实命令输出；凭证零落盘。

## 7. 调研来源附录（15 条）

1. https://jrsoftware.org/ishelp/topic_setupcmdline.htm — Setup 命令行参数（/VERYSILENT /DIR /NOICONS 语义）
2. https://jrsoftware.org/ishelp/topic_setup_section.htm — [Setup] 段（PrivilegesRequired/AppId/DefaultDirName/{autopf}）
3. https://jrsoftware.org/ishelp/topic_messagessection.htm — [Messages] 段内联覆盖（零 .isl 依据）
4. https://jrsoftware.org/ishelp/topic_uninstallrun_section.htm — [UninstallRun] 与退出码不检查语义
5. https://jrsoftware.org/ishelp/topic_filessection.htm — [Files] 段 ignoreversion
6. https://jrsoftware.org/ishelp/topic_iconssection.htm — [Icons] 段（{group}/{autodesktop}/Tasks）
7. https://jrsoftware.org/ishelp/topic_taskssection.htm — [Tasks] 段 unchecked 旗标（桌面默认不勾机制）
8. https://jrsoftware.org/ishelp/topic_isppdefines.htm — ISPP #define/#ifdef/#error（三入口与版本单源闸）
9. https://jrsoftware.org/ishelp/topic_registrykeys.htm — Inno 卸载注册表键 `{AppId}_is1` 与 HKCU per-user 落点
10. https://learn.microsoft.com/windows/win32/msi/uninstall-registry-key — ARP Uninstall 键 DisplayVersion 字段语义
11. https://learn.microsoft.com/windows-server/administration/windows-commands/schtasks — schtasks /delete /tn /f
12. https://github.com/ShareX/ShareX/blob/master/Installer/ShareX.inno.iss — ShareX Inno 实例（选型对照）
13. https://github.com/dittenh/keepass-installer / https://sourceforge.net/projects/keepass/files/ — KeePass Inno 分发实例（选型对照）
14. https://code.visualstudio.com/docs/setup/windows — VS Code per-user 安装形态（per-user 选型对照）
15. https://stackoverflow.com/questions/21953488/setup-command-line-parameters-what-command-to-use-to-display-no-icons-on-deskt — /NOICONS 仅预勾选的社区实证（本轮实测一致）

## 8. 移交注记（供 R 复检/I 真机冒烟重点核）

1. **七件套已全绿**（§4，用户经桌面脚本通道执行、日志逐字转录）；代理侧执行通道被工具权限层七拦（无弹窗、执行前取消）的过程与绕行方式已如实留档，R 轨按其自身判据复跑即可（常规复核，非缺口）。
2. **IS7 vs 规划的 IS6**：用户实装 Inno Setup 7.1.1（非 PLAN §5 预期的 6）；`WizardStyle=modern`/消息键名均按 IS7 核对，R 复检时以 IS7 语义为准。
3. **`/NOICONS` 无效结论**（§2）：GOAL P5 原文假定 `/VERYSILENT /NOICONS` 可静默免图标——实测＋官方文档均否定；隔离机制已根修为预处理层裁剪。I 轨真机冒烟请核**真实安装**的快捷方式三处（开始菜单×2、桌面默认不勾）与卸载自清。
4. **README 开发过程文档表两行修正超出 P4 字面范围**：死链修复＋archive 行数更新（V1~V8）——依据＝R 轨死链复扫门槛与本轨对 README 的独占写面；请 R 确认此越字面但合目的的改法。
5. **使用说明.txt 标题仍是「v5 使用说明」**（顶部历史更新注记体例）：未新增 v9 条目（超出 P4 字面范围）；I 轨发版 1.1.0 时若需更新该文件版本叙事，属其 README/使用说明 token 联动裁量。
6. **ARP 反射读取**：`Assembly.LoadFrom(targetPath)` 会锁新 exe 至设置窗进程退出（实测流程：更新成功即 `Close()`，秒级释放）；极端场景（用户不关设置窗且另一实例立刻再更新）会撞「写入新版本失败」并自动回滚——既有回滚链覆盖，非新失败面。

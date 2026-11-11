# VERIFY-V10B · B 轨安装器侧执行总证据

> **一句话定位**：B 轨（GLM-5.3·目标模式）`setup/OneClickBlackScreen.iss` V10 全改——静默更新支持（AppMutex/强关/[Run]）、「数据存储位置」向导页、InstallDirectory 归属登记＋卸载器归属守卫、卸载自动删数据根（三重守卫）——的双变体构建记录＋P5 全生命周期隔离自测实录＋三态快照零足迹证明＋RESEARCH §7「B 侧」逐项勾验。　**分支**：`feature/installer-v10`（worktree `OneClickBlackScreen-WorkTree/v10-b`）。　**日期**：2026-11-11。

## 0. 环境与前置闸门

- ISCC 探测（GOAL §1 顺序）：`where iscc` 无 → Inno 6/7 Program Files 无 → **命中 `D:\App\Inno Setup 7\ISCC.exe`**（Inno Setup 7.1.1）。闸门通过。
- worktree：`git ls-remote --heads origin feature/installer-v10` 输出为空（无撞名）后 `git worktree add ../OneClickBlackScreen-WorkTree/v10-b -b feature/installer-v10 origin/main`（基点 d170677）。
- 测试隔离面（/DTestId）：测试 AppId `{{V9B-ISOLATION-TEST}`＋输出后缀 `-TEST`＋计划任务名 `OneClickBlackScreen-Idle-TEST`＋**数据键根 `Software\BlackScreen-TEST`**（V10 新增第四隔离位，契约表末行）。

## 1. 双变体构建（真实命令输出摘录）

**真实变体**（`cmd //c build-setup.cmd`，含 build.cmd 全量重建）：

```
Compiling [Code] section
   Verification successful
Successful compile (2.031 sec). Resulting Setup program filename is:
D:\...\v10-b\dist\OneClickBlackScreen-Setup-1.1.0.exe
[OK] Setup built: D:\...\v10-b\dist\OneClickBlackScreen-Setup-1.1.0.exe
EXIT=0
```

**测试变体**（`MSYS_NO_PATHCONV=1 "D:/App/Inno Setup 7/ISCC.exe" /DTestId /DAppVersion=1.1.0 setup/OneClickBlackScreen.iss`）：

```
Successful compile (1.500 sec). Resulting Setup program filename is:
D:\...\v10-b\dist\OneClickBlackScreen-Setup-1.1.0-TEST.exe
```

版本仍 1.1.0（build-setup.cmd 从 src\AppInfo.cs 单源提取，本轨未触碰）；产物四件 payload 一致（一键黑屏.exe/.exe.config/icon-settings.ico/使用说明.txt）。

## 2. P5 全生命周期隔离自测（真实命令输出；乱码为 git-bash 控制台 CP936/UTF-8 渲染差，文件系统对象名经建/删往返验证正确）

测试树：`%LOCALAPPDATA%\Temp\BlackScreenV10B\`（Install/Parent1/Parent2/logs）。安装命令统一 `/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /DIR=<测试目录>`；阶段③追加 `/FORCECLOSEAPPLICATIONS`（＝RESEARCH §7 A 轨更新链调用参数）。全程 `MSYS_NO_PATHCONV=1`。

**前快照（真实状态基线）**：真实键 `HKCU\Software\BlackScreen` 不存在；真实 ARP 键 `{5A17668F-…}_is1` 不存在；真实计划任务 `OneClickBlackScreen-Idle` 不存在；真实数据目录 `%LOCALAPPDATA%\BlackScreen` 存在且为空（无 settings.ini）；开始菜单/桌面 `一键黑屏` 条目 0/0；测试 ARP 键与测试键根均不存在（干净起点）。

### ① 静默安装（默认数据父目录）

```
SETUP_EXIT=0
Install/: 一键黑屏.exe(267,776B) 一键黑屏.exe.config icon-settings.ico 使用说明.txt unins000.exe unins000.dat   ← 四 payload 齐全
HKEY_CURRENT_USER\Software\BlackScreen-TEST
    InstallDirectory    REG_SZ    C:\Users\Lenovo\AppData\Local\Temp\BlackScreenV10B\Install   ← 唯一登记值
（无 DataDirectory/DataParentDirectory ← 默认态零登记，D3）
reg query HKCU\Software\BlackScreen → 系统找不到指定的注册表项或值。   ← 真实键零新增
进程检查（checkproc.ps1 Get-Process 一键黑屏）→ NOT_RUNNING   ← [Run] 测试变体编译抑制实证
```

### ② 改数据父目录（预置登记→重跑＝向导改址同路径）

```
reg add …-TEST /v DataParentDirectory=/d …\Parent1 /f；/v DataDirectory=/d …\Parent1\BlackScreen /f   ← 模拟既有自定义安装
SETUP_EXIT=0（重跑 /DIR 同目录）
[2-1] DataParentDirectory=…\Parent1；DataDirectory=…\Parent1\BlackScreen   ← 登记切换语义正确（安装器只登记搬迁，D4）
[2-2] Parent1/BlackScreen/ 目录已由安装器创建（ForceDirectories）
[2-3] 真实键仍不存在
```

### ③ 覆盖更新（契约参数 `/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /FORCECLOSEAPPLICATIONS`）

```
预置 …\Parent1\BlackScreen\settings.ini（21B，sha256 31857477…ea8a0）
SETUP_EXIT=0
[3-1] settings.ini sha256 前后一致 31857477…ea8a0   ← 数据根保留
[3-2] 三登记值原样保留
[3-4] NOT_RUNNING   ← 更新链下 [Run] 抑制
[3-5] 真实键仍不存在
[3-6] 强化为「文件真刷新」证明：删除 Install\使用说明.txt → 同参数重跑 → 使用说明.txt 恢复；settings.ini 哈希仍一致
```

（mtime 同源不构成刷新证明——Inno [Files] 复制保留源文件时间戳，故以删除-恢复实证重写。）

### ④a 卸载器归属守卫（D6）演练

```
[4a-A] 登记错位：InstallDirectory 改 D:\Elsewhere\OldLocation → unins000.exe /VERYSILENT /SUPPRESSMSGBOXES →
       UNINST_EXIT=1；Install/ 六文件原样；三登记值原样；Parent1\BlackScreen\settings.ini 原样   ← 拒载且零修改
[4a-B] 登记缺失：删除 InstallDirectory 值 → UNINST_EXIT=1；文件与余登记原样
[4a-C] 恢复 InstallDirectory=…\Install → 进入 ④b
```

### ④b 静默卸载 → 零残留

```
unins000.exe /VERYSILENT /SUPPRESSMSGBOXES /LOG=…\uninst4b.log → UNINST_EXIT=0
[4b-1] Install/ 目录消失
[4b-2] Parent1/BlackScreen 消失（含故意预置的 settings.ini 一并删除）；Parent1 本身保留   ← 只删受管根，绝不碰父目录
[4b-3] HKCU\Software\BlackScreen-TEST：三值全清（键壳留存、零值）
[4b-4] 测试 ARP 键 {V9B-ISOLATION-TEST}_is1 消失
[4b-5] schtasks /query /tn OneClickBlackScreen-Idle-TEST → 找不到指定的文件（[UninstallRun] 扫尾实证：日志载 schtasks /delete 先行执行）
[4b-6/7] 开始菜单/桌面 一键黑屏 条目 0/0（[Icons] 测试变体编译抑制）
uninst4b.log 末行：Uninstallation process succeeded. / Removed all? Yes / Need to restart Windows? No
```

### 事故记录（程序性，非 .iss 缺陷）

1. 首次 ④b 用 `unins000.exe /VERYSILENT`（未带 `/SUPPRESSMSGBOXES`）→ 挂起：**Inno 卸载器在 /VERYSILENT 下仍弹启动确认框**（官方文档 topic_uninstcmdline：确认框仅受 /SUPPRESSMSGBOXES 抑制），第二阶段阻塞于不可见确认框。改用 `/VERYSILENT /SUPPRESSMSGBOXES` 即秒过。V9B 惯例同款参数。
2. 一次 `/LOG=` 误传 POSIX 形态路径 → 卸载器立即 exit 1 且不产日志（日志文件创建失败即中止）。改 Windows 形态路径后正常。两条均已定论并纳入移交注记。

## 3. 三态快照前后一致（零足迹，L-012）

`diff snap_before.txt snap_after.txt` 实质差异＝零（仅快照分节标签与顺序不同）：

| 状态 | 前 | 后 |
|---|---|---|
| 真实键 `HKCU\Software\BlackScreen` | 不存在 | 不存在 ✅ |
| 真实 ARP 键 `{5A17668F-…}_is1` | 不存在 | 不存在 ✅ |
| 真实数据目录 `%LOCALAPPDATA%\BlackScreen` | 空目录（mtime Nov 5 未动） | 空目录（mtime Nov 5 未动） ✅ |
| 真实计划任务 `OneClickBlackScreen-Idle` | 不存在 | 不存在 ✅ |
| 开始菜单/桌面 一键黑屏 | 0/0 | 0/0 ✅ |

## 4. RESEARCH §7「B 侧」逐项勾验

| 契约项 | B 侧义务 | 实现与证据 | ✅ |
|---|---|---|---|
| HKCU 键根 `Software\BlackScreen` | [Registry] 声明 | `#define DataRegistryRoot "Software\BlackScreen"`（:54，测试变体 :47）→ [Registry] :156 与 [Code] const :195 | ✅ |
| DataDirectory（REG_SZ，形状=以 `\BlackScreen` 结尾且非根，仅非默认存在） | 向导页用户改过才写；卸载读 | ssPostInstall 仅 `SelectedDataParent≠{localappdata}` 时写（判定点 :511）；`IsManagedDataDirectory`（:327）＝完全限定＋叶子=BlackScreen＋父非根（C:\BlackScreen 拒绝，fts/§7 同构）；①默认态实测零登记 | ✅ |
| DataParentDirectory（REG_SZ 预填用） | 向导页写 | 同上一并写/清；测试②实测 `…\Parent1` | ✅ |
| InstallDirectory＝{app}＋uninsdeletevalue | 安装写；InitializeUninstall 归属校验 | [Registry] :156；InitializeUninstall :536 读登记 vs `ExpandConstant('{app}')`（卸载日志所属目录，官方语义）不符即拒（④a 双场景实证 exit 1 零修改） | ✅ |
| LastUpdateCheck/UpdateAvailable | 不触碰 | .iss 全文零引用（grep 无命中） | ✅ |
| AppMutex `Local\BlackScreen.Mask` | AppMutex 指令＋InitializeSetup taskkill 探测 | :41 `#define AppMutexName`＝:84 指令＝:444 CheckForMutexes＝:447 taskkill /F /IM 同名互斥三处一致 | ✅ |
| Setup 静默参数 | 行为实现（无 UI/无重启/强关） | ③ 实测契约参数全链路通过；CloseApplications=yes＋RestartApplications=no＋AppMutex＋/FORCECLOSEAPPLICATIONS 兜底链 | ✅ |
| 更新资产名 `OneClickBlackScreen-Setup-{ver}.exe` | OutputBaseFilename 不变 | :71 未改（build-setup 产物名实测一致） | ✅ |
| 预下载暂存 `%TEMP%\BlackScreenUpdate\` | 不触碰 | .iss 全文零引用 | ✅ |
| 形状判定同构 | IsManagedDataDirectory 逐字同构 | :327 单谓词；与 A 轨 AppPaths（GOAL-V10A P1 同文：以 \BlackScreen 结尾且非根目录）待 R 轨对照 | ✅（R 复核） |
| 测试变体隔离 | 数据键 -TEST 后缀 | :52 `Software\BlackScreen-TEST`；P5 全程测试键/TEMP 实证 | ✅ |

## 5. 验收清单核对（GOAL §6）

- [x] `git ls-remote --heads origin feature/installer-v10` 可见且指向本轨提交（推送后见 §7 报告）
- [x] `git diff --name-status origin/main...HEAD` ＝ `setup/OneClickBlackScreen.iss`（M）＋`docs/handoff/2026-11-11-v10-lifecycle/VERIFY-V10B.md`（A）——全部落入 §4 白名单；`src/ verify/ build-setup.cmd README.md 使用说明.txt` 零 diff（实测 0）
- [x] build-setup.cmd exit 0 且 `dist\OneClickBlackScreen-Setup-1.1.0.exe` 存在（§1；AppInfo 未动）
- [x] 本文档含 P5 四阶段全断言实录＋三态快照＋§7 契约勾验表（§2/§3/§4）
- [x] grep 五锚点全命中且值与 §7 一致：31 命中（AppMutex=:84、InstallDirectory 登记=:156、InitializeUninstall=:536、CurUninstallStepChanged=:610、DataDirectory 贯穿）
- [x] `file setup/OneClickBlackScreen.iss` ＝ `UTF-8 (with BOM) text, with CRLF line terminators`（改后复验实录）
- [x] src/verify/build-setup.cmd/README/使用说明 零 diff

## 6. 设计裁决留档（R 轨复检要点）

1. **数据登记走 [Code] 不走 [Registry]**：RESEARCH D5 括注「（[Registry] 带 uninsdeletevalue）」与 GOAL P3「登记值形状非法…保留登记」存在张力——uninsdeletevalue 会在拒载/守卫路径末尾无条件清值，违背「保留登记（与数据互为可寻回指针）」；且 [Registry] 无法表达「选回默认时清陈旧值」。裁决＝InstallDirectory（无状态机、永真）走 [Registry]+uninsdeletevalue 承担 D5 兜底；DataDirectory/DataParentDirectory（条件存在）走 [Code] 写清，正常卸载路径显式 RegDeleteValue（④b-3 实证三值全清）。
2. **测试变体不回退默认数据根删除**：`InitializeUninstall` 无登记分支 `#ifdef TestId` 跳过（指向真实 `%LOCALAPPDATA%\BlackScreen` 的唯一路径在测试构建中编译排除）——隔离测试从构造上不可能删真实数据；真实构建的默认根删除由 I 轨真机冒烟覆盖（该路径与登记路径共用同一 `DeleteManagedDataRoot` 守卫函数）。
3. **向导改址＝登记切换而非数据搬迁**：RESEARCH D4 将迁移管线唯一归于设置窗入口；安装器只登记＋建目录（② 实证）。用户在向导改址后旧数据留在原处、新位置从默认值开始，可在设置窗完成正式迁移。已列入 I 轨冒烟场景。
4. **形状「非根」语义**＝所选父目录非盘符/共享根（拒绝 `C:\BlackScreen`），fts 逐字同构；与 A 轨 AppPaths 的同构性由 R 轨逐字对照（§4 末行）。

## 7. 移交注记（R/I 轨）

- **卸载器静默参数**：`unins000.exe /VERYSILENT` 单用仍弹确认框（官方 topic_uninstcmdline），全静默必须 `/VERYSILENT /SUPPRESSMSGBOXES`；`/LOG=` 路径必须是 Windows 形态，创建失败即 exit 1 中止（§2 事故记录）。I 轨真机冒烟与 A 轨文档（使用说明如涉及）按此口径。
- **taskkill 实机行为**：ForceCloseRunningApplication 仅在 `Local\BlackScreen.Mask` 被持有时 `taskkill /F /IM 一键黑屏.exe`（中文映像名经引号包裹）；本轨隔离自测无法安全演练真实遮罩（启全屏遮罩＝全局副作用，属验证闸门域），机制与 fts .iss:751-774 逐行同构。**留 I 轨**：真机跑遮罩→跑 Setup 静默更新→断言遮罩被杀→装完自动重启（[Run] 无 skipifsilent）。
- **Inno 7.1.1 实测边角**：[Files] 覆盖复制保留源 mtime（刷新断言须用内容/删除-恢复法，勿依赖 mtime）；卸载器 [UninstallRun] 先于 usUninstall 执行（日志实证）；[Registry] uninsdeletevalue 于卸载收尾清值。
- **留 I 轨真机冒烟场景清单**：①全新交互安装（数据位置页默认→零登记）；②向导改数据父目录→登记＋目录创建→设置窗迁移管线接力；③真实更新链（遮罩提示→设置窗预下载→立即更新→Setup 静默重装→[Run] 自动重启遮罩→版本/数据无损）；④无登记状态卸载（默认数据根被删、真实 settings.ini 消亡——知情文案已载 ConfirmUninstall）；⑤旧位置残留卸载器拒载实机复演；⑥自定义安装目录后 ARP/快捷方式/卸载全生命周期。
- **给 R 轨**：§4 末行形状同构对照（vs A 轨 AppPaths）；§6-1 登记机制裁决复核；LastUpdateCheck/UpdateAvailable 卸载后残留为 A 侧值（契约「B 不触碰」），A 轨如需卸载清理无挂载点——提请 R 裁决是否随 V11 处理（零残留口径 vs 契约冻结）。
- 测试树 `%LOCALAPPDATA%\Temp\BlackScreenV10B\` 自测后已自清（证据已嵌入本文档）。

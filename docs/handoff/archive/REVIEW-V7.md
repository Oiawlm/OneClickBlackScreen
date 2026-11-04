# REVIEW-V7 · 轨道 S/V 全新上下文复检（轨道 R 执行证据）

> **一句话定位**：R 轨复检实录——S 6 条＋V 7 条验收逐项对照全覆盖（13/13 ✓，全部含本窗复跑实据）、第一性独立再推导（S 红基线 30/30 独立复现＋修复版 150 轮全净；V 轨 USERPROFILE 事实修正子进程探针完全复现，并新发现「进程内运行时修改无效」精度层）、三项复演全绿（verify ×2 exit 0、BUSY 双开 exit 2 逐字、零足迹前后三态一致）、全局操纵点独立扫描与 V 轨表逐点一致；移交注记 3 条（均低严重度，无一需回退），总评 **GO**。
> **上游**：[RESEARCH-V7.md](../RESEARCH-V7.md) §2/§3/§5　**工作分支**：`feature/verify-isolation-v7` 尖（未切分支、未动 main）　**日期**：2026-11-05

## 1. 验收逐项对照表（左列＝两份 GOAL §5；复跑＝本窗重跑，非转述）

### GOAL-V7S（6/6 ✓）

| # | 验收项 | 证据落点 | 本窗复跑结果 | 判定 |
|---|---|---|---|---|
| S1 | 分支已推送＋diff⊆白名单 | `origin/feature/update-guard-v7`=1a35738；`git diff --name-only main...1a35738`＝{Updater,SettingsDialog,Program,VerifyUpdate,VERIFY-V7S.md} 恰 5 文件⊆§4.3 | 本窗重跑同名命令逐字一致 | ✓ |
| S2 | build.cmd 绿 | VERIFY-V7S §3 | `cmd //c build.cmd`→`[OK] Build succeeded: ...\一键黑屏.exe` `BUILD_EXIT=0` | ✓ |
| S3 | verify.cmd 七件全绿 | VERIFY-V7S §3 | 本窗两轮复跑（§3.1）均 `[OK] ALL VERIFIERS PASSED` exit 0 | ✓ |
| S4 | 并发用例红→绿两端实录 | VERIFY-V7S §1.2（红 50/50）＋§3（绿）；绿端本轮复现于两轮套件内 `PASS: concurrent replaces serialize... (x50)` | 另独立复现红端：链接 `main:` 未修复 `Updater.cs` 的探针 30/30 相撞（§3.4） | ✓ |
| S5 | 设计定案（≥2 否决备选）＋U1–U3 对照 | VERIFY-V7S §2（否决备选实为 4 个）＋§4（U1–U3 逐项） | §2.1 本窗独立再推导逐点复核成立 | ✓ |
| S6 | 工作树干净 | VERIFY-V7S §5 | 本窗复检全程后 `git status --short` 零输出（§3.3） | ✓ |

### GOAL-V7V（7/7 ✓）

| # | 验收项 | 证据落点 | 本窗复跑结果 | 判定 |
|---|---|---|---|---|
| V1 | 分支已推送＋diff⊆白名单＋VerifyUpdate 仅一行 | `origin/feature/verify-isolation-v7`=2a417d0；对 S 尖 diff 恰 11 文件⊆§4.3；`verify/VerifyUpdate.cs` 的 V diff 恰 `+VerifyGate.Enter();` 一行 | 本窗重跑同名命令逐字一致 | ✓ |
| V2 | build 绿＋verify 七件全绿 | VERIFY-V7V §6（run2/run3） | 本窗 build 绿＋两轮套件全绿（§3.1）；单跑 VerifySettings/VerifyOverlay 各 exit 0 含 PASS Z（§3.2） | ✓ |
| V3 | 退出码三态实录 | VERIFY-V7V §5.4（0/1/2 三态） | 0＝本窗两轮套件；2＝本窗 BUSY 演练 INST2（§3.2 逐字）；1＝V 轨 run1 实录（`:fail` exit 1 为既有机制，历史各轮一致，未人为制造失败复跑） | ✓ |
| V4 | 双开演练两式留档 | VERIFY-V7V §5.1/§5.2 | 式 1 本窗复演成功：INST2 `exit 2`＋stderr 逐字、INST1 `exit 0`＋PASS Z＋PASS TOTAL（§3.2）；式 2 采信 V 轨留档（需副本仓，AGENTS §2 模型） | ✓ |
| V5 | 零足迹审计三项实录 | VERIFY-V7V §5.3 | 本窗前后快照三态一致（§3.3）：`REAL_INI_ABSENT · REAL_TASK_ABSENT · VERIFY_TASK_ABSENT · NO_RESIDUAL_PROCESS`，`_probe` 无落盘 | ✓ |
| V6 | 操纵点扫描表落档无裸点 | VERIFY-V7V §4 | 本窗独立五式扫描逐点一致（§4），无裸点 | ✓ |
| V7 | 三项设计定案（各≥2 否决）＋隔离映射表＋工作树干净 | VERIFY-V7V §2（A 否决×4/B×5/C×3）＋§3（七件映射表） | §2.2 本窗独立再推导复核成立；git status 干净（§3.3） | ✓ |

## 2. 独立再推导复查（第一性，不预设执行窗口结论）

### 2.1 S 轨互斥边界（对 `src/Updater.cs:120-221` 现码逐点复推）

| 边界 | 本窗推导结论 |
|---|---|
| 锁边界 | `Global\BlackScreen.Update` 正确：exe 为全机共享路径；与遮罩 `Local\` 的不对称（每桌面一遮罩）第一性成立，U3 保持 |
| AbandonedMutex | `WaitOne` 抛即已获所有权→`owned=true`，finally 必释放（`:156-157,187-190`）；与 Program.RunOverlay 既有手法同款 |
| 有界等待 | UI 线程 10 s 上限，超时抛「另一更新正在进行」明确失败不冻结；序列本体毫秒级，上限仅僵死触达 |
| 回滚释放 | 回滚 catch 在 try 域内，全路径（预清理 Delete 抛/Move 抛/Copy 抛后回滚重抛）finally 释锁；`owned=false`（等锁超时）不误释他人锁 |
| no-op 语义 | 获锁复核 `SHA(目标)==SHA(下载产物)`→return：恰一次有效替换；目标缺失/不可读→null→直入原序列由其诚实施错；两实例版本不同的时序下复核不等→正常二次替换，语义正确 |
| 失败级联 | 任一方失败 finally 释锁，不连坐；.NET Mutex 按线程计所有权，同进程双流（VerifyUpdate 用例）与跨进程均正确串行化 |
| U1–U3 | U1 成功链/回滚链/拒装逐行未变（对照 main 版 diff，序列本体逐字符同）；U2 SettingsDialog 仅一行状态文案（`:756`），控件树零触碰，双开断言原样通过；U3 `Program.cs` 仅 `.old` 删除改 `DeleteStaleBackup`（零超时同锁联动：锁被占=活跃备份留给下次，语义同向），Mask 互斥未动 |

**红基线独立证实**（§3.4）：未修复 main 源 30/30 相撞且异常形态与 VERIFY-V7S §1.1 记载三类逐字同形——竞态真实，修复必要；修复版 150 轮（30+120）零异常零损坏。

### 2.2 V 轨闸门/隔离/接缝

- **闸门完备**：七件 Main 首行 `VerifyGate.Enter();`（Config:39/Icon:28/MouseExit:115/Overlay:256/Settings:925/Idle:195/Update:40）——单跑路径与套件路径同入口，均被闸门辖；`verify.cmd` 七编译行接 Gate＋每运行块 `errorlevel 2` 先于 `errorlevel 1`（cmd errorlevel 语义＝≥N，顺序正确）＋`:busy` 出口 exit 2；`held` 静态字段 rooting 防 GC 收 handle——进程退出内核收管，崩溃持有者仅 AbandonedMutex、次者继锁，无 stale 残留（优于锁目录方案）。
- **隔离完备**（grep `GetFolderPath|LOCALAPPDATA|USERPROFILE|schtasks|settings\.ini` 全仓复核）：产品 known-folder 解析唯一点＝`src/MaskConfig.cs:49`（USERPROFILE 接缝覆盖产品全部配置 I/O）；verify 内残余 `GetFolderPath` 仅验证器自身解析**真实**路径作 PASS Z 绊线（只读）；无任何验证器再写真实路径；schtasks 仅经 `src/IdleWake.cs` 产品本体＋接缝。VerifyOverlay 危险机制（原 `:46-79`）与 VerifySettings 快照/恢复确证**已删**（diff 逐行核）；正向证明成立（B/C 像素断奇异色 `#1A2B3C/#2E8B57`、Enter 保存断 `V3pre→V3ok` 转移字节——非缺省值碰巧）；反向 PASS Z 前后哈希含「不存在→不存在」等值。
- **任务名接缝**：唯一定义点 `src/IdleWake.cs:20` `DefaultTaskName`＋`TaskName` 属性（env 优先）；字面量全仓仅剩 migrate/cleanup-local.ps1（冻结区，缺省名不受影响，符合断言）与 VerifyIdle 验证名；VerifyIdle 于 Main 起始设 `BLACKSCREEN_IDLE_TASK_NAME`，正常路径 finally（`VerifyIdle.cs:92`）与 catch 兜底（`:214`）均 `IdleWake.Remove()` 经属性指向验证名。
- **USERPROFILE 事实修正独立复现**（子进程注入探针，与产品 `ProcessStartInfo.EnvironmentVariables` 同构）：`[1]` LOCALAPPDATA 注入→仍真实路径（无效）；`[2]` USERPROFILE＋预建骨架→树内；`[3]` 两者同设→USERPROFILE 决定；`[4]` 骨架缺失→**空串**（预建 `AppData\Local`＋`Roaming` 骨架的必要性实证）。与 VERIFY-V7V §0 矩阵四点全同——事实修正成立，设计正确。**新发现一层精度差**：进程内运行时 `Environment.SetEnvironmentVariable("USERPROFILE",…)` 修改对 `GetFolderPath` **不生效**（本窗进程内探针实测仍返回真实路径）——生效面是「进程创建时注入的环境块」（移交注记 R-1）。

## 3. 复演三项实录（本窗 2026-11-05，跑前 tasklist 确认无其他验证器/被测进程）

### 3.1 verify.cmd ×2（build 绿后）

```text
round1 尾：PASS TOTAL: 5 idle checks (...) / [OK] ALL VERIFIERS PASSED / PIPE_STATUS=0
round2 尾：PASS TOTAL: 5 idle checks (...) / [OK] ALL VERIFIERS PASSED / PIPE_STATUS=0
```

### 3.2 BUSY 双开演练（式 1 复演）＋单件复跑

```text
INST2_EXIT=2   stderr: VERIFY BUSY: another verifier run holds the session gate (Local\BlackScreen.VerifyGate)
INST1_EXIT=0   PASS Z: user's real settings.ini untouched (SHA-256 before == after)
               PASS TOTAL: A-H; 1 displays; max InputIdle 192 ms (limit 2000)
VerifySettings 单跑：PASS Z + PASS TOTAL: 6 settings checks (...) / VS_EXIT=0
```

### 3.3 零足迹审计（§5.1 三项，前后比对）

```text
前：REAL_INI_ABSENT · REAL_TASK_ABSENT · VERIFY_TASK_ABSENT · NO_RESIDUAL_PROCESS
后：REAL_INI_ABSENT · REAL_TASK_ABSENT · VERIFY_TASK_ABSENT · NO_RESIDUAL_PROCESS
附加：_probe 目录不存在（check6-fail-<PID>.png 未触发落盘）；git status --short 零输出；本窗探针全落系统临时目录且已清理
```

### 3.4 S 红基线独立探针（链接 `git show main:` 未修复源，TEMP 编译，30 轮）

```text
rounds=30 exceptionRounds=30 corruptRounds=0
#1: 无法备份当前程序（另一个进程正在使用此文件...），未做任何改动。;
#2/#3/#5: 写入新版本失败（文件"...twin-N.exe"已经存在。），已还原原版本。;
#4: 对路径"...twin-4.exe.old"的访问被拒绝。
RACE REPRODUCED (red baseline confirmed on unfixed main)
对照：同探针链接当前已修复 Updater.cs → 30 轮＋120 轮 exceptionRounds=0 corruptRounds=0
```

## 4. 独立全局操纵点扫描（五式 `keybd_event|mouse_event|SetCursorPos|SetForegroundWindow|SendInput`；.cs/.cmd/.ps1/.py/.js）

| 命中面（本窗） | 归属 | 与 V 轨表比对 |
|---|---|---|
| verify/VerifyIdle.cs:25,175-176（mouse_event） | 闸门保护验证器内 | 一致 |
| verify/VerifyMouseExit.cs:24,27,213,220,251,255 | 闸门保护验证器内（幻影 ESC finally 配对清除） | 一致 |
| verify/VerifyOverlay.cs:28-29,222-223,333,346,355-449（含 E/G/H 持键外层 finally） | 闸门保护验证器内 | 一致 |
| verify/VerifySettings.cs:54,69,425-426,472,721（含 SetCursorPos/AttachThreadInput:53,717-729 配对恢复） | 闸门保护验证器内 | 一致（AttachThreadInput 亦仅此一件，配对 detach） |
| baseline/VerifyBlackScreen.cs、VerifyMouseExit.cs | 冻结区（verify.cmd 不编译不运行；复活须同接闸门） | 一致 |
| src/、tools/、migrate/ 及一切非 .cs 脚本 | **零命中**；产品输入面＝`SetWindowsHookEx(13)`＋`GetAsyncKeyState`（OverlayForm，只读）＝产品合法 | 一致 |

**交叉比对结论：逐点一致，无裸点，无差异项。**

## 5. 移交注记（3 条，均低严重度；无一构成回退理由）

| # | 现象 | 复现命令 | 建议 |
|---|---|---|---|
| R-1 | VERIFY-V7V §0 机理表述「展开于**进程环境**」欠精确：实测生效面是**进程创建时注入的环境块**——进程内运行时 `SetEnvironmentVariable("USERPROFILE",…)` 对 `GetFolderPath` 不生效（本窗 §2.2 探针双跑对照）。现实现全部走子进程注入（Overlay:66/Settings:911/Idle:121），无实际缺陷；此为护栏知识：未来若有人改走进程内运行时修改会得到安慰剂。 | 探针双跑实录见 §2.2（TEMP 已清理，可按 VERIFY-V7V §0 同法复建） | I 波知悉/可在 VERIFY-V7 传承一句；无须回改冻结的过程记录 |
| R-2 | `verify.cmd` 的 `if errorlevel 1/2` 对**负**退出码（如 CLR 未处理异常崩溃 -532462766）均判 false，崩溃轮可能漏过失败出口。既有语义（V 轨前即如此，各验证器 Main 均有 try/catch→return 1 兜底，实际暴露面小），V 轨铁律「既有语义零变化」未动是正确的。 | 人为令某验证器以负码退出（如注入未处理异常）观察 verify.cmd 误报 PASS | 记录供未来轮参考；本轮不修 |
| R-3 | S 轨 `VerifySha256` 单源化后，「下载文件不可读」极端子情形（DownloadTo 成功后文件立即缺失/锁死）的异常类型由原始 IOException 变为「SHA-256 校验失败…」InvalidOperationException；拒装终态与时序不变，U1 可观测主路径逐点保持。 | 在 TEMP 制造下载后即删文件的链路（成本高、价值低） | 不构成 U1 违反，记录在案即可 |

## 6. 总评：**GO**（达到合并与发版条件）

- S 轨：红基线独立复现（30/30）＋修复版 150 轮全净＋边界逐点成立＋U1–U3 保持；互斥是序列内聚部分，无旁路。
- V 轨：闸门七件统一（含单跑）＋BUSY/exit 2 逐字复演＋零足迹前后三态一致＋危险机制确证已删＋事实修正（USERPROFILE 接缝）独立复现成立；断言只增强未放宽（Enter 契约从条件断言升为确定性预置断言）。
- 两轨写面均⊆各自白名单、分支已推送、工作树干净；移交注记 3 条均低严重度且无一需回退。
- 给 I 波：可直接按 GOAL-V7I 合并发布 v1.0.2；R-1 建议在 VERIFY-V7 传承，R-2/R-3 备案即可。

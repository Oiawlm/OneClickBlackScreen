# VERIFY-V7S · 更新并发互斥根修（轨道 S 执行证据）

> **一句话定位**：S 轨全阶段实据留档——并发替换竞态红基线（探针 300/300 相撞＋套件内用例 50/50 坏轮）、第一性设计定案（`Global\BlackScreen.Update` 全程持锁＋获锁后同哈希复核 no-op，≥3 否决备选）、修复后绿实录（VerifyUpdate 十项全 PASS、七件套 `[OK] ALL VERIFIERS PASSED`）、U1–U3 逐项对照。
> **上游**：[RESEARCH-V7.md](../RESEARCH-V7.md) §2/§5.2　**分支**：`feature/update-guard-v7`（自 main@65f8797）　**日期**：2026-11-04

## 1. 红基线（Phase 0 + Phase 3a，修复前实录）

### 1.1 独立探针（系统临时目录，不入仓库）

- 探针源：`%TEMP%\BlackScreenRaceProbe\probe.cs`（csc `-langversion:5` 直编，链接**未修复** `src/Updater.cs`＋`src/AppInfo.cs`）；每轮＝TEMP 目标(oldBytes)＋预置 `.old`(STALE)＋两份独立下载产物(payload 300KB)＋`ManualResetEvent` 起跑枪同时释放两执行流进 `Updater.ReplaceSequence`。
- 跑法：`probe.exe 300`。输出（关键行，逐字）：

```text
rounds=300 clean=0 exceptionRounds=300 targetCorrupt=0 backupCorrupt=0
  round 1 -> EXCEPTION: 无法备份当前程序（未能找到文件“...\twin-1.exe”。），未做任何改动。
  round 2 -> EXCEPTION: 写入新版本失败（文件“...\twin-2.exe”已经存在。），已还原原版本。
  round 5 -> EXCEPTION: 对路径“...\twin-5.exe.old”的访问被拒绝。
RACE REPRODUCED (red baseline confirmed)
PROBE_EXIT=0
```

- **300/300 轮全相撞**，三类异常恰为 RESEARCH-V7 §2 预测形态：①实例2 `Move` 撞已被改名的目标→「无法备份当前程序（未能找到文件…）」；②实例2 `Copy(overwrite:false)` 撞对方已重建的目标→「写入新版本失败（文件已经存在）」且回滚链连带吞掉对方成果；③`Delete(.old)` 与对方 `Move` 同名互撞→「访问被拒绝」。屏障法无需加宽时序窗即确定性复现。

### 1.2 套件内并发用例（verify/VerifyUpdate.cs 新增，修复前跑红）

- 跑法：csc 按 verify.cmd 第 30 行同参数编译（`VerifyUpdate.cs + src/AppInfo.cs + src/Updater.cs` 未修复版）→ 单跑 `VerifyUpdate.exe`（纯件，无全局输入副作用）。输出：

```text
PASS: version comparison table (newer/equal/older/malformed, fail-closed)
...（既有 9 项全部 PASS，与下节绿端一致，此处略）
FAIL: 50 concurrent replace rounds produced 50 bad rounds (#1: 写入新版本失败（文件
“...\race-1.exe”已经存在。），已还原原版本。; #2: ...已经存在。），已还原原版本。; #3: 对路径
“...\race-3.exe.old”的访问被拒绝。; ... )
VERIFY_EXIT=1
```

- **50/50 坏轮**，既有断言零触碰。

## 2. 设计定案（Phase 1，第一性重推导）

**采纳**：互斥融入 `ReplaceSequence` 序列内部（`src/Updater.cs:149-192`）——全程持 `Global\BlackScreen.Update`（`:131`），try/finally 覆盖删`.old`→Move→Copy→**回滚**全路径；有界等待 10 s（`:137,156`）；获锁后复核 `SHA256(目标)==SHA256(下载产物)` 则 **no-op 成功**（`:160-161`）。`Program.Main` 启动删 `.old` 改为同锁零超时联动 `Updater.DeleteStaleBackup`（`src/Updater.cs:201-221`、`src/Program.cs:17`）。

**边界自验**（GOAL §3 Phase 1 列项逐点）：

| 边界 | 结论与实据 |
|---|---|
| AbandonedMutexException | 持有者崩溃后等待者捕获即**已获所有权**→`owned=true` 继续持锁执行、finally 释放（`src/Updater.cs:156-157`）；与 `Program.RunOverlay` 既有同款手法（`src/Program.cs:33-34`）一致 |
| Global 跨会话可见性 | exe 为全机共享路径→互斥必须机器级命名空间；mutex/event 创建于 `Global\` 无需 SeCreateGlobalPrivilege（该特权只辖节对象/文件映射）；遮罩 `Local\BlackScreen.Mask` 每桌面一遮罩语义正确，**未动**（U3 不对称性保持） |
| UI 线程持锁有界等待 | 序列本体＝删＋改名＋本地复制（MB 级文件毫秒～百毫秒），正常等待即获；10 s 上限仅在对方僵死时触达，触达即抛「另一更新正在进行，请稍后重试。」（`:158-159`）明确失败而非无限冻结；SettingsDialog 等待期文案「正在安装…」（`src/SettingsDialog.cs:756`）让可感知等待可读 |
| 回滚路径锁释放 | 回滚 catch 在 try 域内，finally 对**全路径**（预清理 Delete 抛、Move 抛、Copy 抛后回滚重抛）一律 `ReleaseMutex`（`:187-190`） |
| 第二实例终态 | 获锁复核目标哈希==下载哈希→`return`（no-op 成功，`:161`）：终态同、成功提示同（「已更新到 vX」），不产生第二次替换；复核读到目标缺失/不可读→走原序列由其诚实施错（崩溃中间态语义与现状一致，不扩大范围） |
| 失败级联 | 任一实例失败路径（下载校验拒装＝根本不进序列／等锁超时／序列内失败）finally 必释锁，不连坐另一方；同进程双流（VerifyUpdate 用例）同理——.NET Mutex 按线程计所有权，跨进程跨线程均正确串行化 |

**否决备选**（RESEARCH-V7 §2 三项引证复推＋实现期新增一项）：

1. **设置窗单实例化**：违背 U2 既有设计（`VerifySettings.cs` 断言双开共存）；且护不住遮罩/idle 启动路径的 `.old` 删除。
2. **文件哨兵锁**：崩溃残留 stale、无内核对象的所有权/遗弃语义，须轮询重试＝重试风暴旁路；语义弱于 mutex。
3. **进程枚举探测并发更新**：枚举与动作之间 TOCTOU——正是本轮要根除的竞态类；且看不见同进程双流（VerifyUpdate 场景）。
4. **锁放调用方（SettingsDialog）而非 ReplaceSequence 内部**：`DownloadAndApply`（`:115`）与 VerifyUpdate 直调都绕过调用方锁；回滚链在序列内部、与正向路径同需互斥；锁属序列内聚部分，外置＝每个调用点各自记得加锁的旁路护栏。

## 3. 修复后绿实录（Phase 3b）

- 单跑（TEMP 辅助批处理 `run-verifyupdate.cmd` 编译已修复源并执行）：既有 9 项全 PASS ＋ `PASS: concurrent replaces serialize - one effective swap, second flow no-op, backup intact (x50)` ＋ `PASS TOTAL` ＋ `VERIFY_EXIT=0`（完整输出见 §3.1 套件内同文）。
- 全套：`build.cmd` → `[OK] Build succeeded`；`verify.cmd`（跑前 `tasklist` 确认无其他验证器/被测进程，符合 AGENTS §2 会话独占）：

```text
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
...（5 idle checks PASS）
[OK] ALL VERIFIERS PASSED
```

- 并发用例断言语义（`verify/VerifyUpdate.cs` 新增块）：双流起跑枪同入＋50 轮×（终态目标==payload、`.old` 存在且==oldBytes、双流零异常）；其中 `.old==oldBytes` 同时钉死「恰一次有效替换」——若第二流做了完整二次替换，其 `Move` 会把新字节改名进 `.old`（探针轮实测即此形态）。既有断言全部原样保留。

## 4. U1–U3 逐项对照

| 不变量 | 证据 |
|---|---|
| **U1 单实例语义不变** | 成功链/回滚链/校验失败拒装逐点不变：既有 9 项断言（含 keep/rollback/locked-.old 三细节）修复前后均 PASS（§1.2 vs §3）；单实例获锁无竞争、复核两哈希必不等（目标为旧版）→直入原序列；哈希路径单源化（`Sha256HexOrNull`，`src/Updater.cs:253-266`）只是 `VerifySha256` 内部重构，语义等价（拒装消息与触发条件不变） |
| **U2 设置窗多开共存不变** | 控件树零触碰（SettingsDialog 仅一行状态文案＋注释，`src/SettingsDialog.cs:750-757`）；verify.cmd [2/7] VerifySettings 全 PASS＝24 控件镜像锁＋页脚 apply 槽位几何断言＋双开共存断言原样通过；no-op 成功路径的成功提示与首实例完全一致 |
| **U3 遮罩互斥与退出零变化** | `Local\BlackScreen.Mask`、静默自退、AbandonedMutex 处理原样（`src/Program.cs:27-36` 未动）；启动删 `.old` 仅由裸 File.Delete 换成同语义 best-effort 的 `DeleteStaleBackup`（零超时探测：锁空闲＝无替换在途→删；锁被占＝`.old` 属活跃替换→留给下次启动，与原「a locked file simply waits for the next start」语义同向）；verify.cmd [3/7][4/7][7/7] 全 PASS |

## 5. 写面与收尾核对

- 分支 `feature/update-guard-v7`；`git diff --name-only main...HEAD` ＝ `src/Updater.cs`、`src/SettingsDialog.cs`、`src/Program.cs`、`verify/VerifyUpdate.cs`、`docs/VERIFY-V7S.md` ⊆ GOAL §4.3 白名单。
- 探针/辅助批处理全部位于系统临时目录，未入库；`git status` 干净（无未跟踪临时文件）。
- 未动版本号（`src/AppInfo.cs` 1.0.1，I 波辖内）、未 merge main、未发版；`migrate/**`、`baseline/**`、`verify.cmd`、`build.*` 零触碰。

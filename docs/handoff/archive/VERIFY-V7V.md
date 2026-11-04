# VERIFY-V7V · 验证套件会话闸门与零足迹隔离（轨道 V 执行证据）

> **一句话定位**：V 轨全阶段实据留档——RESEARCH-V7 §3 事实修正置顶（LOCALAPPDATA 重定向「既有模式」本机无效，改 USERPROFILE 接缝实证落地）、三项设计定案（各 ≥2 否决备选）、七件隔离映射表、全局操纵点扫描表（无裸点）、双开演练 ×2（BUSY/exit 2 逐字实录）、零足迹审计前后实录、套件 ×2 全绿。
> **上游**：[RESEARCH-V7.md](../RESEARCH-V7.md) §3/§5/§8　**分支**：`feature/verify-isolation-v7`（自 `feature/update-guard-v7`@1a35738 尖续作）　**日期**：2026-11-05

## 0. 事实修正（GOAL §6，置顶如实报告）

**RESEARCH-V7 §3 修复方向 2 所引「既有模式 `VerifyIdle.cs:101-113` LOCALAPPDATA 重定向」在本机无效**——`Environment.GetFolderPath(LocalApplicationData)`（MaskConfig 解析配置路径的唯一 API，.NET Framework 经 SHGetFolderPath）**忽略 LOCALAPPDATA 环境变量**，按每用户注册表模板 `%USERPROFILE%\AppData\Local` 展开于**进程环境**并做**存在性校验**。探针矩阵（csc `-langversion:5` 直编系统临时目录，目标目录均已创建，不入仓）：

```text
=== [1] LOCALAPPDATA=D:\ladtest\redirect（目录已存在） ===
LocalAppData=[C:\Users\Lenovo\AppData\Local]      ← 覆盖被忽略，仍返回真实路径
=== [2] USERPROFILE=D:\ladtest\up（AppData 树已存在） ===
LocalAppData=[D:\ladtest\up\AppData\Local]        ← 接缝生效
AppDataRoaming=[D:\ladtest\up\AppData\Roaming]
=== [3] 两者同时设置 ===
LocalAppData=[D:\ladtest\up\AppData\Local]        ← USERPROFILE 决定解析
（附）目标不存在时 GetFolderPath 返回空串：SHGetFolderPath 对合法但不存在路径
      返回 S_FALSE，.NET 视非零 hr 为失败 → 重定向树必须预建骨架
```

**影响**：VerifyIdle 原 `:113` 行自 V5C 起为安慰剂（其 PASS 3 断言「活跃会话静默退出」不依赖配置内容，绿灯从未暴露无效）。**处置**：零足迹机制改用 **USERPROFILE 环境变量接缝**——同一设计意图（子进程环境接缝、验证器预建 TEMP 骨架树、配置读写全落树内）、同一写面（零产品代码变更），套件断言构成产品级正反双向证明（§2-B）。产品级正统接缝（MaskConfig 读 `BLACKSCREEN_SETTINGS_DIR` 类变量）属 S 轨冻结写面，列移交注记（§8）供规划裁决，本轮未动。

## 1. Phase 0 基线（改造前）

`git checkout -b feature/verify-isolation-v7`（自 S 尖）；`build.cmd` → `[OK] Build succeeded`；改造前 `verify.cmd` 七件全绿 `VERIFY_EXIT=0`（含 S 轨并发用例 50/50）。

## 2. 设计定案（Phase 1，第一性重推导；各 ≥2 否决备选）

### A. 会话闸门（`verify/VerifyGate.cs` 新增，七件统一接入 Main 首行）

**采纳**：`Local\BlackScreen.VerifyGate` named mutex，`WaitOne(0)` 零超时探测；获锁者静态字段 rooting **持有至进程退出、从不显式释放**（进程死＝内核收 handle，崩溃持有者仅 AbandonedMutex——次 waiter 捕获即已获所有权继续）；撞闸 stderr `VERIFY BUSY: ...` ＋ `Environment.Exit(2)`；七件统一（纯件在内）；`verify.cmd` 每运行块 `if errorlevel 2 goto :busy` 先于 `if errorlevel 1 goto :fail`，`:busy` 出 `[FAIL] VERIFY BUSY - another verifier run holds the session gate`＋exit 2。**否决**：①mkdir 锁目录（崩溃残留 stale，须轮询清理＝旁路）②仅 GUI 四件接入（规则例外即漏洞口；纯件并行价值低但统一规则价值高）③批处理内持锁（cmd 无法持内核对象，只能映射退出码）④有界等待排队（等待＝变相并行窗口，立即 BUSY 才确定可诊断；与 AGENTS §2「不得盲目重试风暴」同向）。**边界**：`Local\` 前缀＝会话域（验证并发定义在会话内；跨会话各桌面输入域独立，与遮罩 `Local\` 不对称同理）；闸门在 Main 首行＝参数校验前（先拒绝后一切）。

### B. 零足迹隔离（VerifyOverlay/VerifySettings/VerifyIdle 子进程，含 §0 事实修正）

**采纳**：验证器为每轮运行建 TEMP 骨架树（`AppData\Local`＋`AppData\Roaming`，供 GetFolderPath 存在性校验），子进程 env `USERPROFILE` → 树根；**删除** VerifyOverlay 真实 settings.ini 备份-覆写-恢复机制（`:46-79` 原文）与 VerifySettings 真实快照/恢复（原 `CheckEnterContract`）——测试不碰用户状态的能力来自「机制不存在」。**产品级双向自证**：正向＝VerifyOverlay 场景 B/C 配置色像素断言（子进程真读重定向树）＋ VerifySettings Enter 保存断言（子进程自身写路径落到重定向树）；反向＝两件新增真实 settings.ini 前后 SHA-256 绊线（`PASS Z`，含「不存在→不存在」存在性等值）。VerifyOverlay 净 **473→469 行**（危险机制删除量＞新增接缝量，铁律 1）。**否决**：①LOCALAPPDATA 覆盖（§0 实证无效，原方案）②MaskConfig 产品级接缝（正统但 S 轨冻结，移交注记）③保留备份-恢复＋对比报警（＝给危险机制加装甲，章程明令禁止）④临时改写注册表 User Shell Folders（把写面压到用户注册表＝更大足迹）⑤真实文件改名隔离（窗口期用户状态移位，同类危险）。**边界**：接缝依赖注册表模板形态（默认安装即模板）；若机器重定位过 LocalAppData（绝对路径值）则失效→场景 B/C 像素与保存断言立即响亮失败，验证器自身只写树内、不会静默踩真实状态。

### C. 任务名接缝（`src/IdleWake.cs` 唯一定义点）

**采纳**：`DefaultTaskName` 常量（`"OneClickBlackScreen-Idle"` 原缺省）＋ `TaskName` 属性读 `BLACKSCREEN_IDLE_TASK_NAME` 覆盖（`IsNullOrEmpty`→缺省）；产品行为零变化（用户会话无此变量）；migrate/cleanup-local.ps1 删的仍是缺省名（字面量，不受影响）。VerifyIdle 于 Main 起始设 `OneClickBlackScreen-Idle-Verify`，Ensure/Exists/Remove 全路径（含 catch 兜底清理）指向验证名，finally Remove 语义原样。**否决**：①verify 复刻 schtasks 命令（失去对 IdleWake 本体保真——接缝让真本体跑真 schtasks）②保留真名＋事后恢复（窗口期仍删用户任务，RESEARCH 已否决）③随机后缀任务名（`/create /f` 幂等已自愈；固定验证名令残留可预期可清理）。**审计**：注册任务 XML 指向真实 dist exe（全路径带引号，Check 1 既有断言）；注册窗口秒级、触发器 PT1M，若恰跨分钟触发，`--idle` 活跃会话 2 s 内静默退出（只读真实配置亦无害）。

## 3. 隔离映射表（七件逐项）

| 验证器 | 子进程 | 写路径（改造前→后） | 全局注入（去向） |
|---|---|---|---|
| VerifyConfig | 无 | TEMP `VerifyConfig_<ticks>` 树（原样）＋finally 删 | 无（纯件＋闸门） |
| VerifySettings | dist exe `--settings`/`--bogus` ×7 | **真实 ini 快照/覆写/恢复→已删**；USERPROFILE→TEMP 树（读＋保存均在树内）；`_probe\check6-fail-<PID>.png`；PASS Z 绊线 | AttachThreadInput/SetForegroundWindow/SetCursorPos（闸门保护） |
| VerifyMouseExit | dist exe ×6 | 无文件写（子进程读真实配置为**纯读**，遮罩退出断言与配置内容无关） | keybd_event（幻影 ESC）＋SetForegroundWindow（闸门保护） |
| VerifyOverlay | dist exe ×12 | **真实 ini 备份-覆写-恢复→已删**；USERPROFILE→TEMP 树（场景 A-H 配置全在树内，B/C 像素证明）；PASS Z 绊线 | keybd_event/SetForegroundWindow（闸门保护） |
| VerifyIcon | 无 | 只读（原样） | 无（纯件＋闸门） |
| VerifyUpdate | 无（HttpListener 进程内 loopback mock） | TEMP `VerifyUpdate-<rand>`（原样） | 无（纯件＋闸门，S 轨成果仅接一行） |
| VerifyIdle | dist exe `--idle` ×1＋schtasks 子进程 | **真名任务注册/删除→验证名**（接缝）；TEMP `VerifyIdle_<ticks>\profile` 树（--idle 子进程 USERPROFILE→树，**修正原 LOCALAPPDATA 安慰剂**，idle=5 配置自此真被读） | mouse_event 微移 ×2（闸门保护） |

## 4. 全局操纵点扫描表（§5.4；grep 五式 `keybd_event|mouse_event|SetCursorPos|SetForegroundWindow|SendInput` 全仓）

| 命中面 | 归属 |
|---|---|
| verify/VerifyIdle.cs:25,175-176（mouse_event 微移） | 闸门保护验证器内 |
| verify/VerifyMouseExit.cs:24,27,213,220,251,255 | 闸门保护验证器内（幻影 ESC 全路径 finally 配对清除） |
| verify/VerifySettings.cs:54,69,425-426,472,721 | 闸门保护验证器内（光标进出配对恢复） |
| verify/VerifyOverlay.cs:28-29,222-223,333,346,355-449 | 闸门保护验证器内（E/G/H 持键场景外层 finally 配对） |
| baseline/VerifyBlackScreen.cs、baseline/VerifyMouseExit.cs | **冻结区**（v1 基线归档，verify.cmd 不编译不运行；如复活须同接闸门） |
| src/、tools/、migrate/ | **零命中**；产品输入面＝`SetWindowsHookEx(13)`＋`GetAsyncKeyState`（OverlayForm.cs:11,21,167,196，只读钩子/状态读，非注入）＝产品自身合法使用 |

**无裸点**：每个命中点均有归属。

## 5. 演练与审计实录（RESEARCH-V7 §5.1/§5.3）

### 5.1 双开演练①（两实例 VerifyOverlay.exe，同 bin 并发执行）

```text
INST2_EXIT=2        ← 第二实例 stderr：VERIFY BUSY: another verifier run holds
                      the session gate (Local\BlackScreen.VerifyGate)，未做任何 GUI 动作
INST1_EXIT=0        ← 首实例完整跑完：PASS Z + PASS TOTAL: A-H; 1 displays; max
                      InputIdle 159 ms (limit 2000)
```

### 5.2 双开演练②（verify.cmd 双开；第二轮自独立副本仓——规避 #4 产物路径竞态，即 AGENTS §2「并行轮各用独立 checkout」模型；robocopy 副本已清理）

```text
--- 第二轮（副本仓）RUN2_EXIT=2 ---
[1/7] VerifyConfig
VERIFY BUSY: another verifier run holds the session gate (Local\BlackScreen.VerifyGate)
[FAIL] VERIFY BUSY - another verifier run holds the session gate
--- 第一轮（主仓）RUN1_EXIT=0 ---
[OK] ALL VERIFIERS PASSED
```

第二轮在**首个闸门件**处被拒（纯件 VerifyConfig 同样受闸——七件统一接入的实证）。

### 5.3 零足迹审计（§5.1 三项，套件×2＋演练×2 全部跑完后）

```text
前：REAL_INI_ABSENT · TASK_ABSENT · VERIFY_TASK_ABSENT
后：REAL_INI_ABSENT · TASK_ABSENT · VERIFY_TASK_ABSENT · NO_RESIDUAL_PROCESS · _probe 空
```

真实 settings.ini 前后「不存在→不存在」（存在性与字节双等值；真实计划任务 `OneClickBlackScreen-Idle` 与验证名任务两侧同「不存在」；无 Verify/一键黑屏残留进程；`_probe\check6-fail-<PID>.png` 未触发落盘）。

### 5.4 退出码三态实录

`0`＝套件 run2/run3 与两演练首轮；`1`＝run1（`FAIL: Could not exercise background keyboard hook` → `[FAIL] A verifier failed`，VERIFY_EXIT=1）；`2`＝两演练次轮（§5.1/§5.2）。

## 6. 套件绿实录（闸门版）

实现中一次修正：VerifySettings Enter 契约首写补 `Directory.CreateDirectory`（真实目录原本就存在故旧代码无此行；TEMP 树须显式建 `BlackScreen` 子目录）。其后：**run2、run3 连续两次 `VERIFY_EXIT=0`＋`[OK] ALL VERIFIERS PASSED`**，VerifySettings/VerifyOverlay 各含 `PASS Z: user's real settings.ini untouched (SHA-256 before == after)`；run1 曾于 VerifyOverlay 场景 D 背景分支一次失败（前台权断言：`SetForegroundWindow` 成败取决会话「最后输入者」权限，属 RESEARCH §4 固有边界「验证期间用户真实键鼠」；同码 run2/run3 两次全过，断言零改动，两日志留档系统临时目录 `v7v-run1/run2/run3.log`）。

## 7. 写面与收尾核对

- `git diff --name-only`（对 S 尖）＝`verify/VerifyGate.cs`(新)、`verify/VerifyConfig.cs`、`VerifyIcon.cs`、`VerifyIdle.cs`、`VerifyMouseExit.cs`、`VerifyOverlay.cs`、`VerifySettings.cs`、`VerifyUpdate.cs`（**恰一行** `VerifyGate.Enter();`）、`verify.cmd`（仅七编译行＋七 BUSY 判跳＋`:busy` 出口）、`src/IdleWake.cs`（仅任务名接缝）、`docs/VERIFY-V7V.md` ⊆ GOAL §4.3 白名单。
- 探针/演练副本/临时树全部清理（系统临时目录，未入库）；`_probe/`、`.gitignore` 零触碰；未动版本号、未 merge、未发版；`migrate/**`、`baseline/**` 只读。
- `verify.cmd` 其余（七件顺序/参数/既有文案/编译参数）零变化。

## 8. 移交注记（供规划/R 轨裁决）

1. **MaskConfig 产品级接缝**（§0/§2-B 否决备选②）：若未来把「配置目录可覆盖」升为产品能力（如便携模式），在 `src/MaskConfig.cs` 读环境变量是比子进程 USERPROFILE 重定向更正统的接缝；本轮 S 轨冻结写面不可动，现方案已达成零足迹目标，无须回补。
2. VerifyMouseExit 子进程读真实配置为纯读（断言与配置内容无关）；若未来断言依赖配置内容，须同接 USERPROFILE。

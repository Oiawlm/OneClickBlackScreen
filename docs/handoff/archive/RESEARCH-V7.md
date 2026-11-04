# RESEARCH-V7 · 「多窗口并行冲突检查与根治」唯一事实源

> **一句话定位**：v7 轮唯一事实源：两类多窗口并行的冲突体检结论（场景A＝应用多实例、场景B＝多代理窗口并行 build/verify）、三个根因与修复方向（更新替换竞态、验证全局输入互扰、验证踩踏真实用户状态）、不修清单与固有边界、演练配方、串行四窗编排依据。执行窗口以本文为唯一事实源；与 GOAL 冲突时以本文为准。
> **来源**：用户 2026-11-04 /goal 需求＋两项对齐定案（§7）＋规划窗口四路子代理实测盘点（代码 file:line 证据见 §1）与外部调研（Microsoft Learn/PowerToys/FlaUI/Velopack 等成熟实践，文字佐证随节标注）。
> **日期**：2026-11-04　行数预算：≤170 行。

## 1. 冲突体检总表（全部 file:line 实据）

| # | 冲突面 | 位置 | 场景A(多实例) | 场景B(多代理窗口) | 现状缓解 | 定性 |
|---|---|---|---|---|---|---|
| 1 | 更新替换序列竞态 | `src/Updater.cs:127-153`（删`.old`→`Move(exe→.old)`→`Copy`） | 双设置窗同点「立即更新」→删对方备份/「无法备份当前程序」 | verify 期间无人点更新（低） | TEMP 随机下载名、UI 线程序列化（单实例内）；跨进程无锁 | **真缺陷，S 轨根修** |
| 2 | 验证全局输入互扰 | `verify/VerifyOverlay.cs:236-456`、`VerifyMouseExit.cs:219`、`VerifyIdle.cs:167-168`（keybd_event/mouse_event/SetForegroundWindow/SetCursorPos） | —（验证非产品功能） | **必然互扰**：全局输入队列/前台窗口为会话级独占，双 verify 并行→幻影键闩锁、断言随机假阴性、用户按键被吞 | 各场景 finally 配对释放（仅护自己） | **真缺陷，V 轨根修（闸门）** |
| 3 | 验证踩真实用户状态 | `verify/VerifyOverlay.cs:46-79`（真实 settings.ini 备份-覆写-恢复）、`VerifySettings.cs:609-621,692`、`VerifyIdle.cs:81-94`（真实计划任务 `/create /f` 覆盖+删除） | 验证与用户保存交错→恢复错误内容/测试值短暂成为用户配置 | 双 verify 交错→恢复出别人的内容 | File.Replace 原子（防损坏不防交错）；VerifyConfig/VerifyIdle(配置)已 TEMP 隔离 | **真缺陷，V 轨根修（零足迹）** |
| 4 | 固定产物路径竞态 | `build.rsp:10`/`build.cmd:30,36`、`verify.cmd:20-33` | — | 同 checkout 双 build/verify→csc/copy 文件锁失败、启动半替换映像 | 各验证器 finally Kill 自启进程 | **协议根治（AGENTS §2），不代码设防** |
| 5 | 遮罩单实例互斥 | `src/Program.cs:26-33`（`Local\BlackScreen.Mask`，AbandonedMutex 处理正确，二实例静默自退） | 同会话无冲突；跨会话各遮罩＝各桌面，**语义正确** | VerifyOverlay 场景F 断言此行为（V 改造须保持） | 已有 | **保持不动（U3）** |
| 6 | 设置窗多开 | `src/Program.cs:64-71`（有意无互斥，`VerifySettings.cs:934-952` 断言双开共存） | last-writer-wins 保存（File.Replace 原子） | — | 原子替换 | **设计内，不修**（§4） |
| 7 | git 层并行冲突 | 历史事故：`docs/VERIFY-V6B.md §61-65`（共享单工作树分支劫持）、`docs/PUBLISH-V5A.md:150`（多窗并发 GUI 测试互扰） | — | 同 checkout 并行＝劫持检出/互扰 | 教训已成文 | **协议根治（AGENTS §2）** |

## 2. 根因一（产品级，S 轨）：并发更新替换竞态

**机制**：`--settings` 有意允许多实例（#6），但两个实例同时进入 `ReplaceSequence`（`Updater.cs:127-153`）：实例2 的 `File.Delete(exe.old)` 可能删掉实例1 刚建的备份；实例2 的 `File.Move(exe→.old)` 撞已被实例1 改名的目标→抛「无法备份当前程序」→回滚链连带失败。exe 是全机共享路径（跨用户会话同文件），故互斥必须 `Global\` 前缀；遮罩互斥 `Local\` 是每桌面状态（#5），**两者不对称是第一性正确的，保持**。

**冻结不变量（S 修复必须同时满足）**：
- **U1 单实例语义不变**：成功链（下载→SHA-256/大小双校验→替换）/回滚链/校验失败拒装，时序与结果逐点不变。
- **U2 设置窗多开共存不变**：含 VerifySettings 双实例断言、页脚 apply 槽位几何断言（`VerifySettings.cs:300-311`）不放宽。
- **U3 遮罩互斥与退出零变化**：`Local\BlackScreen.Mask`、静默自退、I1–I3（V6 冻结不变量）全保持。

**首选设计方向**（供参考；执行窗口须第一性重推导，不得照抄）：`ReplaceSequence` 序列内部全程持 `Global\BlackScreen.Update` 互斥（try/finally 释放，覆盖删除`.old`→Move→Copy→回滚全路径）；获锁后有界等待上限与第二实例终态＝复核目标 SHA-256==新版哈希则 **no-op 成功**（已更新完，不二次替换）。**被否决备选**：①设置窗单实例化（违背 U2 既有设计语义）；②文件哨兵锁（崩溃残留 stale、语义弱于内核对象）；③进程枚举探测（TOCTOU 竞态）。佐证：Microsoft Learn Mutex/AbandonedMutexException 语义；Velopack/Squirrel 多实例文件锁事故（更新器替换被存活实例锁死＝业界同款问题）；Velopack `ApplyUpdatesAndExit` 模式反证「多持锁进程」才是根因。

## 3. 根因二+三（验证级，V 轨）：会话级互扰与真实状态踩踏

**根因（第一性）**：`keybd_event`/`mouse_event`/`SetForegroundWindow` 注入的是**会话全局输入队列与前台权**——单会话内独占，进程边界隔离不了它；两个 verify 并行必然互相污染（FlaUI 官方 #492、微软 DevCommunity、Azure Pipelines UI 测试指引一致：全局输入类 UI 测试一机一串行）。而「备份-覆写-恢复真实 settings.ini」「真名计划任务注册/删除」是**把测试写面压在用户状态上**——机制本身即危险，加固它＝打补丁。

**修复方向（三项，均根修）**：
1. **会话闸门**：新增共享源 `verify/VerifyGate.cs`（七件统一接入，Main 首行）：`Local\BlackScreen.VerifyGate` named mutex、0 超时探测、持有至进程退出；撞闸→stderr 提示＋**退出码 2（BUSY）**。`verify.cmd` 各 `if errorlevel 1` 前加 `if errorlevel 2 goto :busy`：`[FAIL] VERIFY BUSY` exit 2（退出码语义：0 通过/1 失败/2 占用）。把「静默互相污染的随机假阴性」变成「确定性、可诊断的显式拒绝」。**被否决**：①mkdir 锁目录（崩溃残留）；②仅 GUI 四件加闸（规则不统一、纯件并行价值低）；③批处理内持锁（cmd 无法持内核对象）。佐证：AGENTS.md 跨工具实践（Prisma「never run tests in parallel」类条目点名独占资源清单）。
2. **零足迹隔离**：VerifyOverlay/VerifySettings 启动被测子进程时**重定向 LOCALAPPDATA**（复用既有模式 `VerifyIdle.cs:101-113`、`VerifyConfig.cs:43`）；**删除** VerifyOverlay 真实文件备份-覆写-恢复机制（`:46-79`）与 VerifySettings 真实读写——测试不碰用户状态这一能力来自「机制不存在」，而非「机制有装甲」。
3. **计划任务名接缝**：`src/IdleWake.cs` 任务名改为单一来源＝常量缺省＋环境变量 `BLACKSCREEN_IDLE_TASK_NAME` 覆盖（配置优于代码的标准接缝，定义点唯一）；VerifyIdle 用验证名＋全路径 finally Remove；任务注册指向的 exe 路径一并纳入隔离审计。**被否决**：①verify 里复刻 schtasks 命令（失去对 IdleWake 本体保真）；②保留真名+事后恢复（窗口期仍删用户任务）。另：`_probe\check6-fail.png` 固定名改按 PID/轮次隔离（`.gitignore` 忽略 `_probe/` 不变）。

**产物路径竞态（#4）不代码设防**：dist/verify\bin 属单 checkout 资源，AGENTS §2「一窗一 checkout＋并行用 worktree」已根治；代码再加锁＝双保险叠旁路，违不打补丁原则。

## 4. 不修清单与固有边界（如实告知）

- **设置保存 last-writer-wins**：双开设置窗先后保存＝后者胜——File.Replace 原子防损坏，此语义为业界设置窗常规，不修。
- **跨会话遮罩互斥为 Local\**：快速切换用户时各桌面各自遮罩＝正确行为（每会话一个桌面输入域），保持。
- **验证运行期间用户真实键鼠/开遮罩**：全局注入与用户输入同属一个会话，固有边界（README 已声明「验证会短暂闪现遮罩，属预期」）；应对＝PLAN §5 用户守则（验证期间勿操作），不属代码可消除项。
- **install.ps1 并行**：不在 build/verify 链上且幂等＋回读校验，不修。

## 5. 演练与复现配方（自包含，供执行窗口与复检）

```text
5.1 零足迹审计：certutil -hashfile "%LOCALAPPDATA%\BlackScreen\settings.ini" SHA256（套件前后比对）
  + schtasks /query /tn OneClickBlackScreen-Idle（前后状态一致；用户无任务则两侧同「不存在」）
  + tasklist | findstr /i "一键黑屏 Verify"（无残留进程）。
5.2 更新竞态红基线（同步屏障法）：harness 两执行流在同一屏障释放点同时进入 ReplaceSequence
  （TEMP 目标副本＋loopback 假新版，复用 VerifyUpdate 既有隔离手法）→ 未修复预期：一方
  「无法备份当前程序」或 IO 异常 / .old 错乱；修复后：串行化、恰一次有效替换、终态哈希=新版、
  第二流 no-op 成功。探针用 csc 编译到系统临时目录，不入仓库。
5.3 闸门双开演练：同时起两个 VerifyOverlay.exe "dist\一键黑屏.exe" → 第二实例 [BUSY] exit 2；
  verify.cmd 双开 → 第二轮在首个闸门件处 [FAIL] VERIFY BUSY exit 2（两者输出留档）。
5.4 全局操纵点扫描：全仓 grep keybd_event|mouse_event|SetCursorPos|SetForegroundWindow|SendInput
  → 逐点归属「闸门保护的验证器内」或「产品自身合法使用」，不得有裸点。
```

## 6. 编排与模型选型（依据）

- **四窗全串行 S→V→R→I**（用户裁决：减少失误优先）：串行消除写面冲突、验证互扰、合并裁决三类失误源；并行安全性由闸门+隔离+AGENTS 协议根治，并由 R 窗**受控演练**（§5.3）验证——本轮不赌并行，但把并行修安全并实证。V 自 S 分支尖堆叠续作（消除双改 `verify/VerifyUpdate.cs` 的合并风险；VerifyUpdate.cs 由 S 深改、V 仅加一行闸门接入）。全程单 checkout 串行，无需 worktree；未来并行轮按 AGENTS §2。
- **R 复检窗**（用户提议，与业界「写代码的会话不做最终复检/fresh-context review」共识一致，Anthropic 团队实证 revert 率降 40%）：全新会话逐项对照 S/V 验收清单、独立再推导设计、复演演练、零足迹审计；只写 `docs/REVIEW-V7.md`，不修码（缺陷→移交注记）。
- **模型**：四窗主责全 GLM-5.3（S＝并发互斥+P/Invoke+U1–U3 不变量密度；V＝闸门语义+隔离重构+7 件批量改造；R＝第一性复查+证据审计判断力；I＝发布工艺+版本单源纪律）。Flash 仅限窗口内子代理机械扫描（§5.4 类 grep、版本引用扫描）。

## 7. 对齐定案（用户 2026-11-04）

1. 窗口编排：**由规划窗口定案、以减少失误为第一优先**→四窗全串行 S→V→R→I（§6）。
2. 版本：产品代码将变更（更新互斥+任务名接缝）→ 发 **v1.0.2**（完整发布链沿用 V6I：AppInfo 单源→tag/Release 三资产+SHA256SUMS→应用内「已是最新」真端点冒烟→README/使用说明同步）。

## 8. 轨道写面契约（单一写者；GOAL 引用为准）

| 轨 | 分支（自） | 写面白名单 | 证据文档 |
|---|---|---|---|
| S 更新互斥 | `feature/update-guard-v7`（main） | `src/Updater.cs`、`src/SettingsDialog.cs`、`src/Program.cs`、`verify/VerifyUpdate.cs` | `docs/VERIFY-V7S.md` |
| V 验证隔离 | `feature/verify-isolation-v7`（S 尖） | `verify/VerifyGate.cs`(新)、`verify/VerifyConfig.cs`、`VerifySettings.cs`、`VerifyMouseExit.cs`、`VerifyOverlay.cs`、`VerifyIcon.cs`、`VerifyIdle.cs`、`verify/VerifyUpdate.cs`（仅闸门接入一行）、`verify.cmd`（仅闸门接线与 BUSY 出口）、`src/IdleWake.cs`（任务名接缝） | `docs/VERIFY-V7V.md` |
| R 复检 | （在 V 尖工作） | `docs/REVIEW-V7.md` | 同左 |
| I 集成 | main 合并 V（含 S） | `src/AppInfo.cs`、`verify/VerifyUpdate.cs`（版本表联动）、`README.md`、`使用说明.txt`、`docs/VERIFY-V7.md`、tag v1.0.2/Release | `docs/VERIFY-V7.md` |

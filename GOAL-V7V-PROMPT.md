# GOAL-V7V · 验证套件会话闸门与零足迹隔离（轨道 V）

> **一句话定位**：根治多代理窗口并行验证的三处冲突——①七件验证器统一接入会话闸门（`Local\BlackScreen.VerifyGate`，撞闸 BUSY/退出码 2，verify.cmd 区分 0/1/2）；②删掉「备份-覆写-恢复真实 settings.ini」机制本身，VerifyOverlay/VerifySettings 子进程重定向 LOCALAPPDATA（零足迹）；③IdleWake 任务名单一来源＋环境变量接缝，VerifyIdle 用验证名。现有一切断言语义不放宽。
> **上游事实源**：[RESEARCH-V7.md](RESEARCH-V7.md) §3（根因/三项方向/被否决备选）、§5.1/§5.3/§5.4（审计与演练配方）、§8（写面契约）（冲突时以 RESEARCH 为准）　**编排**：[PLAN-V7.md](PLAN-V7.md) §3 波 2（自 S 分支尖续作）。
> **日期**：2026-11-04　行数预算：≤150 行。

## 0. 身份与工作方式

- 你是执行窗口，主模型 GLM-5.3，以**目标模式（/goal）**工作：只产出落地改动与可核对证据；计划/待办/耗时/结论式回复不构成完成依据。
- 第一性原理；**禁止打补丁**——零足迹靠「危险机制不存在」（删除真实文件备份-恢复与真名任务注册），不是给危险机制加装甲/加锁/加对比报警；闸门是统一接入的会话级互斥，不是逐件散装的 if 判断。
- 尽量用内置 ZCode 子代理（Explore 查证；§3 Phase 4 的全仓 grep 扫描可用 Flash 子代理扇出）。
- 本轮无产品 UI 改动；`src/` 仅动 `IdleWake.cs`（任务名接缝，一个定义点）。

## 1. 上下文承接（冷启动固定顺序）

1. 确认已读 `AGENTS.md`（协作契约；未自动注入则手动读）。
2. 读 `RESEARCH-V7.md` 全文（唯一事实源；实现依据＝§3 三项修复方向＋被否决备选）。
3. 读 `docs/VERIFY-V7S.md`（S 轨成果与证据，本轨在其分支尖续作）。
4. 读 `verify/` 七件全文（重点：`VerifyOverlay.cs:46-79` 真实文件备份-覆写-恢复、`:236-456` 全局注入；`VerifySettings.cs:609-621,692` 真实读写、`:934-952` 双实例断言；`VerifyIdle.cs:81-94` 真名计划任务、`:101-113` LOCALAPPDATA 重定向既有模式、`:167-168` 全局鼠标微移）。
5. 读 `verify.cmd`（七件编译行与 0/1 退出码出口）、`src/IdleWake.cs`（任务名常量与 Ensure/Remove）、`src/MaskConfig.cs`（LOCALAPPDATA 解析点）。
- **禁改**：`src/` 除 `IdleWake.cs` 外全部（S 轨成果冻结）、`build.cmd`/`build.rsp`、`README.md`/`使用说明.txt`、`migrate/**`、`baseline/**`、`verify/VerifyUpdate.cs`（**仅允许闸门接入一行**，其余为 S 轨成果）。
- **禁读**：GOAL/RESEARCH 历史 V1–V6 全文（要惯例只 grep 定位）。

## 2. 背景

多代理窗口并行跑验证时的三类真实冲突（RESEARCH-V7 §1 #2/#3/#7，V6B 与 PUBLISH-V5A 事故实证）：全局输入（keybd_event/mouse_event/SetForegroundWindow）是**会话级独占**，双 verify 并行必然互相污染（随机假阴性＋用户按键被吞）；「备份-覆写-恢复真实 settings.ini」「真名计划任务注册/删除」把测试写面压在用户状态上。业界共识（FlaUI #492/微软 DevCommunity/Azure Pipelines 指引）：全局输入类 UI 测试一机一串行。本轮把「静默互扰」变成「确定性 BUSY 拒绝」，把「踩用户状态」的机制整个删除。C#5 语法、零新增引用。

## 3. Phase 任务

### Phase 0 · 分支与现状基线
- `git checkout -b feature/verify-isolation-v7`（自 `feature/update-guard-v7` 尖）；`build.cmd` 绿；七件经 `verify.cmd` 编译环节后逐件单跑全绿（改造前基线留档）。

### Phase 1 · 第一性设计（先设计后编码）
- 三项各自重推导并写设计定案（每项 ≥2 个被否决备选；RESEARCH §3 已列可直接引证复推）：
  - **闸门语义**：mutex 名/前缀选择、0 超时探测 vs 有界等待、持有至进程退出（using/finally）、BUSY 输出格式与退出码 2、七件统一接入 vs 分类接入、单跑某件时闸门同样生效。
  - **隔离映射表**：每件验证器逐项列出「启动了哪些子进程/写了哪些路径/注入了哪些全局状态」→ 改造后去向（TEMP 重定向/验证名/闸门保护）。
  - **任务名接缝**：`BLACKSCREEN_IDLE_TASK_NAME` 环境变量在 IdleWake 的唯一读取点、缺省值＝现常量、VerifyIdle 全路径（含失败路径）finally Remove 验证名任务；任务注册指向的 exe 路径一并纳入审计。

### Phase 2 · 实现
- 新增 `verify/VerifyGate.cs`（共享源，七件 csc 编译行各加本文件）；各验证器 Main 首行接闸。
- `verify.cmd`：编译行加 `verify\VerifyGate.cs`；每个运行块 `if errorlevel 2 goto :busy` 先于 `if errorlevel 1 goto :fail`；`:busy` 出口输出 `[FAIL] VERIFY BUSY - another verifier run holds the session gate`、exit 2。其余（七件顺序/参数/既有文案）零变化。
- VerifyOverlay：删除真实 settings.ini 备份-覆写-恢复（`:46-79` 及各场景调用点），被测遮罩子进程重定向 LOCALAPPDATA；VerifySettings：同款重定向，删除对真实文件的快照/恢复；VerifyIdle：任务名接缝消费；`_probe\check6-fail.png` 固定名改按 PID/轮次隔离（`.gitignore` 忽略 `_probe/` 不变）。

### Phase 3 · 断言保持与自证
- 现有一切断言（含 VerifySettings 双实例共存、页脚 apply 槽位、VerifyOverlay 场景F 单实例静默自退）**语义不放宽**，逐件核对改造后仍绿。
- 新增零足迹自证断言（成本最低者归位，R 轨另有外部审计）：VerifyOverlay/VerifySettings 在改造后对真实 `settings.ini` 路径前后哈希比对纳入用例（应天然不变）。

### Phase 4 · 全局操纵点扫描与演练
- 全仓 grep `keybd_event|mouse_event|SetCursorPos|SetForegroundWindow|SendInput` → 逐点归属「闸门保护的验证器内」或「产品自身合法使用」（如 SettingsDialog 的 AttachThreadInput 归产品），扫描表落档证据文档。
- 按 RESEARCH-V7 §5.3 双开演练：①同时起两个 `VerifyOverlay.exe "dist\一键黑屏.exe"` → 第二实例 [BUSY] exit 2；②`verify.cmd` 双开 → 第二轮 [FAIL] VERIFY BUSY exit 2；输出留档。
- 按 §5.1 零足迹审计：全套件前后真实 settings.ini SHA-256 一致＋真实计划任务状态一致＋无残留进程，实录留档。

### Phase 5 · 证据与推送
- `docs/VERIFY-V7V.md`：三项设计定案＋隔离映射表＋扫描表＋演练与审计实录。`git push -u origin feature/verify-isolation-v7`。

## 4. 铁律

1. 不打补丁；最终验证器代码简洁完整（隔离后的 VerifyOverlay 应比改造前更短——危险机制是删除对象不是加固对象）。
2. 现有断言语义不放宽；`verify.cmd` 改动仅限闸门接线与 BUSY 出口，既有七件顺序/参数/文案语义零变化。
3. 写面只限：`verify/VerifyGate.cs`(新)、`verify/VerifyConfig.cs`、`verify/VerifySettings.cs`、`verify/VerifyMouseExit.cs`、`verify/VerifyOverlay.cs`、`verify/VerifyIcon.cs`、`verify/VerifyIdle.cs`、`verify/VerifyUpdate.cs`（仅闸门接入一行）、`verify.cmd`（仅 §3 Phase 2 所列两点）、`src/IdleWake.cs`、`docs/VERIFY-V7V.md`。
4. 本轮串行编排：跑 GUI 验证/演练前确认无其他窗口在跑验证（AGENTS §2）；演练的第二实例必须由你自己启动并等待其退出，不留残留进程。
5. `migrate/**` 与 SHA256SUMS.txt 不可触碰；凭证不入任何文件；不 merge、不发版、不改版本号（I 波辖内）。

## 5. 验收清单（校验器只认实据）

1. `git log origin/feature/verify-isolation-v7` 可见推送；`git diff --name-only feature/update-guard-v7...HEAD` ⊆ §4.3 白名单（`verify/VerifyUpdate.cs` 的 diff 仅闸门接入一行量级）。
2. `build.cmd` 绿；`verify.cmd` 七件全绿（末行 `[OK] ALL VERIFIERS PASSED`、退出码 0）。
3. 退出码语义实录：任一验证器失败→1、双开占用→`[FAIL] VERIFY BUSY`＋2、全过→0（三态输出留档 docs/VERIFY-V7V.md）。
4. 双开演练两式输出留档（第二实例 BUSY/exit 2）。
5. 零足迹审计三项实录留档（真实 settings.ini 哈希前后一致、真实计划任务状态一致、无残留进程）。
6. 全局操纵点扫描表落档且无裸点（每个命中点都有归属）。
7. docs/VERIFY-V7V.md 含三项设计定案（各 ≥2 被否决备选）与隔离映射表；`git status` 干净。

## 6. 收尾动作

- 证据落档即停（不 merge、不发版）；提示用户按 PLAN-V7 §5 开 R 复检窗口（全新会话）。
- 发现 RESEARCH-V7 事实错误：停止编码，实测反证写入 docs/VERIFY-V7V.md 顶部如实报告——事实修正优先于进度。

# GOAL-V7S · 更新并发互斥根修（轨道 S）

> **一句话定位**：根修「两个设置窗同时点『立即更新』」的替换序列竞态——`ReplaceSequence` 全程持跨进程 `Global\` 互斥、获锁后复核目标哈希，设置窗多开共存与遮罩互斥语义零变化（U1–U3 逐点保持）；VerifyUpdate 增并发替换用例（修复前红→修复后绿留档）。
> **上游事实源**：[RESEARCH-V7.md](RESEARCH-V7.md) §2（根因/不变量/方向）、§5.2（竞态复现配方）、§8（写面契约）（冲突时以 RESEARCH 为准）　**编排**：[PLAN-V7.md](PLAN-V7.md) §3 波 1。
> **日期**：2026-11-04　行数预算：≤140 行。

## 0. 身份与工作方式

- 你是执行窗口，主模型 GLM-5.3，以**目标模式（/goal）**工作：只产出落地改动与可核对证据；计划/待办/耗时/结论式回复不构成完成依据。
- 第一性原理；**禁止打补丁**——互斥必须融入 Updater.ReplaceSequence 既有序列（删`.old`→Move→Copy→回滚）内部从根源解决，不加旁路线程、看门狗、重试风暴或外部哨兵文件。
- 尽量用内置 ZCode 子代理（Explore 查证/general-purpose 扇出；机械扫描可用 Flash 子代理）。
- 本轮**无 UI 布局改动**：SettingsDialog 只允许更新流程集成点（如等待期提示文案），控件树零触碰（镜像锁 24 控件断言依赖不变）。

## 1. 上下文承接（冷启动固定顺序）

1. 确认已读 `AGENTS.md`（协作契约；未自动注入则手动读）。
2. 读 `RESEARCH-V7.md` 全文（唯一事实源；实现依据＝§2 U1–U3＋首选设计方向＋被否决备选）。
3. 读 `src/Updater.cs`（BeginCheck/下载/双校验/ReplaceSequence 全链）、`src/SettingsDialog.cs`（更新按钮→apply 流程与页脚槽位）。
4. 读 `src/Program.cs`（进程模型、exe.old 启动删除、`Local\BlackScreen.Mask` 互斥现状）、`src/MaskConfig.cs`（File.Replace 原子写手法）。
5. 读 `verify/VerifyUpdate.cs`（既有隔离手法：TEMP 副本＋loopback HttpListener＋版本表断言）。
- **禁改**：`verify/` 其余六件、`verify.cmd`/`build.cmd`/`build.rsp`、`README.md`/`使用说明.txt`、`migrate/**`、`baseline/**`、`src/` 其余文件、`src/AppInfo.cs`（I 波辖内）。
- **禁读**：GOAL/RESEARCH 历史 V1–V6 全文（防上下文膨胀；要惯例只 grep 定位）。

## 2. 背景

C# WinForms、.NET FW 4.8、csc 直编（`-langversion:5`，**无字符串插值**）、零依赖单 exe。缺陷（RESEARCH-V7 §2，实据 `src/Updater.cs:127-153`、`src/Program.cs:64-71`）：`--settings` 有意允许多实例（VerifySettings 断言双开共存），但两实例同时进入 ReplaceSequence→实例2 `File.Delete(exe.old)` 可能删掉实例1 的备份、`File.Move` 撞已改名目标抛「无法备份当前程序」。exe 为全机共享路径→互斥须 `Global\`；遮罩互斥 `Local\`（每桌面各遮罩）语义正确，保持不动。本轮串行编排：跑 verify 前确认无其他窗口在跑验证（AGENTS §2）。

## 3. Phase 任务

### Phase 0 · 分支与红基线
- `git checkout -b feature/update-guard-v7`（自 main）；`build.cmd` 绿。
- 按 RESEARCH-V7 §5.2 同步屏障法在 TEMP 目标上复现并发替换竞态（两执行流同释屏障→同入 ReplaceSequence；探针用 csc 编译到系统临时目录，**不入仓库**）；红基线输出存证据文档。

### Phase 1 · 第一性设计（先设计后编码）
- 对照 U1–U3 重推导互斥方案；RESEARCH-V7 §2 的「`Global\BlackScreen.Update` 全程持锁（try/finally 覆盖含回滚全路径）＋有界等待＋获锁后复核目标 SHA-256==新版哈希则 no-op 成功」仅为参考方向——你须自行验证边界：AbandonedMutexException 语义（捕获后已获所有权）、跨会话 Global 可见性、UI 线程持锁有界等待的上限与用户可感知性、回滚路径锁释放、第二实例终态（no-op 成功而非二次替换）、失败级联（一方失败不得连坐另一方）。
- 设计定案写入证据文档（含 ≥2 个被否决备选与理由；RESEARCH 已列三个可直接引证复推）。

### Phase 2 · 实现
- `src/Updater.cs`：互斥融入 ReplaceSequence 序列内部；`src/SettingsDialog.cs`：仅更新流程集成点；`src/Program.cs`：仅当 `.old` 启动删除需与互斥联动时。C#5 语法、零新增引用。

### Phase 3 · VerifyUpdate 扩展
- 新增**并发替换用例**：双执行流同步屏障同入 ReplaceSequence（TEMP 目标＋loopback 假新版，复用既有隔离手法）→ 断言：恰一次有效替换、终态哈希＝新版、无「无法备份当前程序」/IO 异常、`.old` 终态干净、第二流 no-op 成功语义。既有断言（版本表/校验失败拒装/回滚链）全部保持不放宽。
- 修复前跑新用例确认红（若屏障法仍偶发，按 §5.2 附注加宽时序窗：预先占住 `.old` 名），修复后绿——两端输出留档。

### Phase 4 · 证据与推送
- `docs/VERIFY-V7S.md`：设计定案＋红绿两端输出＋U1–U3 逐项对照验证证据。
- `git push -u origin feature/update-guard-v7`。

## 4. 铁律

1. 不打补丁；最终源码简洁完整（互斥是替换序列的内聚部分，非外挂护栏）。
2. 冻结不变量 U1–U3 逐点满足；现有 VerifyUpdate/VerifySettings 断言语义不得放宽。
3. 写面只限：`src/Updater.cs`、`src/SettingsDialog.cs`、`src/Program.cs`、`verify/VerifyUpdate.cs`、`docs/VERIFY-V7S.md`。
4. 探针/临时工具只落系统临时目录，不入仓库；收尾 `git status` 干净。
5. `migrate/**` 与 SHA256SUMS.txt 不可触碰；凭证不入任何文件。
6. 不 merge main、不发版、不改版本号（I 波辖内）。

## 5. 验收清单（校验器只认实据）

1. `git log origin/feature/update-guard-v7` 可见分支已推送，且 `git diff --name-only main...HEAD` ⊆ §4.3 白名单。
2. `build.cmd` 输出 `[OK] Build succeeded`。
3. `verify.cmd` 七件全绿（末行 `[OK] ALL VERIFIERS PASSED`、退出码 0）。
4. VerifyUpdate 并发用例输出包含修复前 FAIL 与修复后 PASS 两端实录。
5. docs/VERIFY-V7S.md 含设计定案（≥2 个被否决备选）与 U1–U3 逐项对照证据。
6. 工作树干净（探针未入库：`git status` 无未跟踪临时文件）。

## 6. 收尾动作

- 证据文档落档后即停（不 merge、不发版）；提示用户按 PLAN-V7 §5 开 V 窗口（V 将自本分支尖续作）。
- 如发现 RESEARCH-V7 事实错误（复现不出等），停止编码，把实测反证写入 docs/VERIFY-V7S.md 顶部并如实报告——事实修正优先于进度。

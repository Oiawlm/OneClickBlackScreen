# GOAL-V6A · 退出健壮性根修（轨道 A）

> **一句话定位**：根修「幻影键×布防闸门＝遮罩无法退出」——布防从「全键盘 254 键静止」改为「启动快照＋待抬起闩锁」，冻结语义 I1–I3 逐点保持；并为 VerifyMouseExit/VerifyOverlay 增加幻影键回归用例（修复前红→修复后绿留档）。
> **上游事实源**：[RESEARCH-V6.md](RESEARCH-V6.md) §2（根因与不变量）、§8（复现配方）、§6（并行契约）（冲突时以 RESEARCH 为准）　**编排**：[PLAN-V6.md](PLAN-V6.md) §3 波 1。
> **日期**：2026-11-03　行数预算：≤150 行。

## 0. 身份与工作方式

- 你是执行窗口，主模型 GLM-5.3，以**目标模式（/goal）**工作：只产出落地改动与可核对证据；计划/待办/耗时/结论式回复不构成完成依据。
- 第一性原理；**禁止打补丁**——改动必须融入 OverlayGroup 既有生命周期（构造/NotifyShown/armTimer/OnKeyboard/NotifyMouseUp）从根源解决，不得加旁路线程、全局钩子链或定时看门狗式补丁。
- 尽量用内置 ZCode 子代理（Explore 查证/general-purpose 扇出；机械扫描可用 Flash 子代理）；wbx 桥未登录则不用。
- 本轮无 UI/视图改动（SettingsDialog 与布局零触碰）。

## 1. 上下文承接（冷启动固定顺序）

1. 读 `RESEARCH-V6.md` 全文（唯一事实源；实现依据＝§2 不变量 I1–I3＋首选设计方向）
2. 读 `src/OverlayForm.cs`（OverlayGroup 全部机制）、`src/Program.cs`（进程模型与单实例互斥）
3. 读 `baseline/BlackScreen.cs`（v1 冻结行为参照：同款布防循环 44-46 行）
4. 读 `verify/VerifyMouseExit.cs`、`verify/VerifyOverlay.cs`（既有用例与注入手法；注意 :301/:322/:362-368 的 ESC 成对注入）
5. 读 `verify.cmd`（套件编排；**本轮冻结不改**）
- **禁改**：`verify/VerifySettings.cs` 与其余 verify 文件（B 轨写面）、`verify.cmd`/`build.cmd`/`build.rsp`、`README.md`/`使用说明.txt`、`migrate/**`、`baseline/**`、`src/` 其余文件、`src/AppInfo.cs` 版本号（I 波统一 1.0.1）。
- **禁读**：GOAL-V5*/RESEARCH-V1..V5 全文（历史过程文档，防上下文膨胀；需要惯例时只 grep 定位）。

## 2. 背景

C# WinForms、.NET FW 4.8、csc 直编（`-langversion:5`，**无字符串插值**）、零依赖单 exe。缺陷（RESEARCH-V6 §2，本机实测）：armTimer 要求 1..254 全部键抬起才置 `armed`；任一幻影键（GetAsyncKeyState 报按下但无真实事件，本机实测为外部软件注入的 ESC down 无 up）使 armed 永 false → 键盘钩子与 OnMouseUp 双路退出全灭，v1.0.0 在该机器上无法退出遮罩。该循环是 v1 冻结基线，**语义必须逐点保持**，只消灭幻影键致命性。

## 3. Phase 任务

### Phase 0 · 分支与红基线
- `git checkout -b feature/exit-arm-v6`（自 main）；`build.cmd` 绿。
- 按RESEARCH-V6 §8 用临时探针（csc 编译到系统临时目录，**不入仓库**）复现：幻影 ESC＋SPACE → 不退出（红基线输出存入证据文档）。

### Phase 1 · 第一性设计（先设计后编码）
- 对照 I1–I3 重推导布防机制；RESEARCH-V6 §2 的「快照集 L＋待抬起闩锁＋（可选）N-tick 并入」仅为参考方向——你须自行验证边界（启动键长按自动重复、VK_LBUTTON/RBUTTON 在 1..254 域内、布防窗口内新出现幻影键、闩锁键的 KEYDOWN 必须照常 CallNextHookEx 放行不得吞事件——现状仅退出键被吞）。
- 设计定案写入证据文档（含被否决的备选与理由，≥2 个）。

### Phase 2 · 实现（仅 `src/OverlayForm.cs`）
- 布防/闩锁融入 OverlayGroup 生命周期：快照时机（Show 前）、轮询复用既有 armTimer 100ms 节拍、闩锁释放判定、OnKeyboard 闩锁键不触发退出但放行事件。
- 不新增文件、不加引用、不动 UI 与其余 src 文件；C#5 语法。

### Phase 3 · 验证器扩展（VerifyMouseExit / VerifyOverlay）
- 新增**幻影键回归用例**（二者至少其一承载，按各自职责归位）：注入幻影键 down（不 up）→ 启动 → 真实语义按键注入 → 必须退出；收尾强制配对 up（注入卫生：一切路径含失败必须配对）。
- 新增/保持**防误触回归**：启动时按住键 → 未抬起前其 KEYDOWN 不得退出；抬起后可退出（对齐既有 :362-368 用例语义，不放宽现有断言）。
- 修复前先跑新用例确认红（对未修复 dist），修复后绿——两端输出都留档。

### Phase 4 · 证据与推送
- `docs/VERIFY-V6A.md`：设计定案＋红绿两端输出＋逐项对照 I1–I3 的验证证据。
- `git push -u origin feature/exit-arm-v6`。

## 4. 铁律

1. 不打补丁；最终源码简洁完整（布防机制仍是一个内聚生命周期，非旁路补丁）。
2. 冻结不变量 I1–I3 逐点满足；现有验证断言语义不得放宽。
3. 写面只限：`src/OverlayForm.cs`、`verify/VerifyMouseExit.cs`、`verify/VerifyOverlay.cs`、`docs/VERIFY-V6A.md`。
4. 本分支 `[2/7] VerifySettings` 预期红（B 轨并行）——完成判据用其余六件**单跑**＋新用例，不得为凑全绿去改 B 轨文件。
5. 探针/临时工具只落系统临时目录，不入仓库；收尾清理键态（幻影键 up）。
6. `migrate/**` 与 SHA256SUMS.txt 不可触碰；凭证不入任何文件。
7. 不 merge main、不发版本、不改版本号（I 波辖内）。

## 5. 验收清单（校验器只认实据）

1. `git log origin/feature/exit-arm-v6` 可见分支已推送，且改动文件集 ⊆ §4.3 白名单（`git diff --name-only main...HEAD`）。
2. `build.cmd` 输出 `[OK] Build succeeded`。
3. 逐件单跑全绿（命令与末行输出留档 docs/VERIFY-V6A.md）：`verify\bin\VerifyConfig.exe`、`VerifyIcon.exe`、`VerifyUpdate.exe`、`VerifyIdle.exe`、`VerifyMouseExit.exe "dist\一键黑屏.exe"`、`VerifyOverlay.exe "dist\一键黑屏.exe"`（先经 verify.cmd 编译环节或等价 csc 命令重建 bin）。
4. 新增幻影键用例输出包含修复前 FAIL 与修复后 PASS 两端实录。
5. docs/VERIFY-V6A.md 含设计定案（≥2 个被否决备选）与 I1–I3 逐项对照证据。
6. 工作树干净（探针未入库：`git status` 无未跟踪临时文件）。

## 6. 收尾动作

- 证据文档落档后即停（不 merge、不发版）；提示用户按 PLAN-V6 §5 开 I 窗口。
- 如发现 RESEARCH-V6 事实错误（复现不出来等），停止编码，把实测反证写入 docs/VERIFY-V6A.md 顶部并如实报告——事实修正优先于进度。

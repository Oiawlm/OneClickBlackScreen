# GOAL-V1-PROMPT · 一键黑屏 v1「遮罩颜色/文字自定义」执行契约

> **一句话定位**：交给执行窗口（GLM-5.3，目标模式 /goal）的完整执行契约——按 RESEARCH-V1.md 已定案的架构决策实现、构建、验证并交付。
> **来源**：事实源为 [RESEARCH-V1.md](RESEARCH-V1.md)（2026-10-19）；本文档与其冲突时，以 RESEARCH-V1.md 为准并向规划产物提出修订记录。
> **日期**：2026-10-19　行数预算：≤240 行

---

## 0. 使用方式（交接者读，执行窗口跳过本节）

- 启动：在执行窗口输入本文档 §8 的 `/goal` 启动指令（一字不差粘贴）。
- 档位：目标模式长任务，建议「完全访问」或「自动编辑」档，避免每轮卡人工确认。

## 1. 身份与工作方式

你是本仓库的执行工程师，工作于目标模式（/goal）：完成判定由独立校验器执行，**只认可核对的实据**（命令输出、文件、测试结果），计划/待办/耗时/结论式回复不构成完成依据；未达成会自动开下一轮继续推进。

工作铁律：
- **第一性原理**：先还原问题本质再动手；任何"看起来能跑"的方案先问"根源上对不对"。
- **不打补丁（用户最高原则）**：严禁为满足需求在代码外打补丁、加特判、堆 if；所有改动必须融合重构进代码逻辑，通过调整既有架构实现；最终交付以**代码最简洁且完整实现需求**为准，不以修改过程省力为准。
- **自包含协作**：上下文只经文件传递；子代理与外部 lane 均看不到你的会话，派发材料必须打包齐全。
- **结果必校验**：外包回来的任何结论，采纳前必须本地核实；连续 ≥2 次外包失败即停止外包、自己完成。

## 2. 上下文承接指令（冷启动，按序执行，禁止无目的通读）

1. 读 `README.md`（路由与铁律）。
2. 读 `RESEARCH-V1.md` 全文——**唯一事实源**：现状还原（§2）、环境实测（§3）、调研结论（§4）、架构决策表（§5，D1–D12 已定案）、验证方案（§6）、冻结清单。
3. 读 `baseline/BlackScreen.cs` 与 `baseline/VerifyBlackScreen.cs` 全文，`VerifyMouseExit.cs`、`baseline/使用说明.txt`、`baseline/BlackScreen.manifest` 选读关键段。**baseline/ 是冻结参考，一个字节都不许改。**
4. 禁止通读 `.git/`、`_probe/`；不联网重复调研（§4 已含结论），除非实现中发现决策表存在事实性错误——那时先在 RESEARCH-V1.md 追加勘误记录再继续。

## 3. 背景（一段话）

「一键黑屏」是 131 行 C# WinForms 单文件工具（双击→全屏遮罩，任意键/点击退出）。本轮给它加 R1 遮罩颜色自定义、R2 遮罩文字自定义、R3 设置入口，同时 R4「双击立即出遮罩」与 R5「退出交互」冻结不变。用户本机为混合 DPI 双屏（150% 主屏 + 125% 副屏），编译器仅 Framework csc（C# 5 上限）。全部技术决策已在 RESEARCH-V1.md §5 定案，你的工作是**把决策表变成可验证的现实**，并在实现层面做第一性原理的质量把关。

## 4. Phase 任务（顺序执行，逐 Phase 产出）

### Phase 0 · 冷启动与勘察
- 完成 §2 阅读路径。
- 派一个 Zcode **Explore 子代理**核对 RESEARCH-V1.md §2 现状还原表与 baseline 源码的一致性（自包含 prompt：贴 §2 全文 + baseline/BlackScreen.cs 路径；只回传"逐条一致/不一致点清单"）；不一致处以源码为准并勘误。
- `git checkout -b feature/mask-customization`。
- 产出：分支就绪；勘察结论（如有勘误，提交到 RESEARCH-V1.md）。

### Phase 1 · 方案定稿与批判评审
- 把 RESEARCH-V1 §5 决策表展开为**文件级实现计划**（每个 .cs 文件：类型、职责、关键方法签名、与冻结语义的衔接点）。
- 用 **wbx 桥发起 T7 批判评审**（`wbx ask --file <材料.txt>` 或 fanout；材料必须自包含：决策表 + 你的实现计划 + baseline 关键源码粘贴）。批判点至少覆盖：每屏多窗体下 arm/hook/关闭入口的状态共享是否正确；C# 5 语法边界；配置解析的边界（转义/超限/损坏）；设置窗与遮罩共享绘制器的一致性。
- 采纳合理意见，修订计划；评审记录与采纳/驳回结论追加到 RESEARCH-V1.md 末尾（新增 "## 9. T7 评审记录（执行窗口）" 一节）并提交。
- 产出：定稿实现计划 + RESEARCH-V1.md §9 + git 提交。

### Phase 2 · 源码实现（src/）
按 D1–D12 与定稿计划实现，类型：`Program` / `MaskConfig` / `OverlayForm` / `SettingsDialog`（+ 共享静态绘制器）。硬性技术约束：
- **C# 5 方言**（Framework csc 编译）：禁止字符串插值 `$""`、null 条件 `?.`、表达式体成员、out var、using 声明等 C#6+ 语法。
- **源码文件一律 UTF-8 带 BOM**（否则中文按 GBK 读成乱码）；UI 文案用中文。
- 引用仅限框架程序集（System / System.Drawing / System.Windows.Forms / System.Core 视需）；**零第三方依赖、零新 DLL**。
- 冻结语义逐字保留（RESEARCH-V1 §2 的 arm/退出/光标机制），只重构组织方式不改行为；关闭入口统一、清理幂等。
- D3 配置读写、D4 WCAG 自动对比度、D5 绘制细节按决策表执行。
- 产出：`src/` 全部源码 + git 提交。

### Phase 3 · 构建与验证体系
- `build.cmd`（仓库根）：调 `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`，`-target:winexe -optimize+ -win32manifest:baseline\BlackScreen.manifest -out:dist\一键黑屏.exe`，编译 `src\*.cs`；脚本内容仅 ASCII（避免 GBK 问题）。
- 按 RESEARCH-V1 §6 实现 verify 三/四件套（`verify/` 目录，扩展 baseline 验证器风格：自动启动被测程序、断言、退出码报成败）。注意：验证会真实闪现全屏遮罩数秒，属预期。
- 全部验证器跑绿；把每条的最终命令行与输出摘要记入 `docs/VERIFY-V1.md`。
- 产出：build.cmd + verify 套件 + docs/VERIFY-V1.md + git 提交。

### Phase 4 · 安装与文档
- `install.ps1`：幂等同步双快捷方式——`一键黑屏.lnk`（无参，目标=仓库 `dist\一键黑屏.exe`）与 `一键黑屏-设置.lnk`（同目标，参数 `--settings`）；用 WScript.Shell COM；不触碰快捷方式目录其他文件，不删旧 exe（`Documents\Codex\...` 处保留作回退）。
- 仓库根写 v2 版 `使用说明.txt`：双快捷方式用法、设置入口、颜色/文字自定义、配置文件位置、**"保存后下次启动生效"**、退出行为与 v1 完全一致、§7 风险边界。
- 更新 `README.md` 文档地图（src/verify/docs/dist 行）。
- 产出：install.ps1 + 使用说明.txt + README 更新 + git 提交。

### Phase 5 · 终验与收尾
- 在仓库根全量重跑：build.cmd → 全部 verify → install.ps1（两次，证明幂等）。
- 人工验收一次设置界面（预览、保存、试用、恢复默认），把观察结果追加到 docs/VERIFY-V1.md。
- 完成 §7 收尾动作。
- 产出：全绿证据 + 干净的 git 状态。

### 边界清单
- 允许：仓库内一切新增/修改；快捷方式目录仅两个指名 .lnk；%LOCALAPPDATA%\BlackScreen\ 配置读写；运行验证器。
- 禁止：改 baseline/；改快捷方式目录其他文件；删除旧 exe；改系统设置/注册表；联网下载依赖；把任何凭证写进文件或提示词。

## 5. 铁律（不可协商）

1. R4/R5 冻结：任何改动后，双击仍必须立即出遮罩；退出交互与 arm 精确语义逐字保留。
2. 不打补丁：决策偏离必须先论证、记录于 RESEARCH-V1.md，再实施；禁止在决策表之外引入未论证的机制（托盘/常驻/热键/IPC/文件监听等一律不加）。
3. 单一写者：RESEARCH-V1.md 与 docs/ 由你串行维护；一次一个文件，交接以文件为唯一介质。
4. 外包纪律：子代理/wbx 任务必须自包含 + 指定期望产出形态；结果必校验后采纳；连续 ≥2 次失败停止外包。
5. 目标模式纪律：不以"计划完成"为由停轮；每轮结束前自查 §7 清单还有哪些没有实据。

## 6. 配置文件与命令速查

- 配置路径：`%LOCALAPPDATA%\BlackScreen\settings.ini`（`color=#RRGGBB` + `text=...`，转义规则见 RESEARCH-V1 §5-D3）。
- 编译器：`C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`。
- 仓库根：`D:\Download\Agent Vault\一键黑屏`。

## 7. 验收清单（Done when——校验器只认这些实据）

| # | 实据 |
|---|---|
| 1 | 仓库根运行 `build.cmd` 退出码 0，`dist\一键黑屏.exe` 存在且非空 |
| 2 | `verify` 全部验证器输出含 PASS 且退出码 0：逐屏窗体数==屏数、逐屏边界精确、TopMost、无边框、每屏采样点==配置色、有文字时中心区含文字色像素、无文字时全屏纯色、3 键前台+后台退出、退出码 0、第二实例自退、InputIdle ≤2000ms（报告实测值） |
| 3 | `VerifyConfig` 退出码 0（roundtrip/缺文件/损坏/超限/未知键回落默认/转义） |
| 4 | `--settings` 打开设置窗（恰好一个非遮罩窗体，进程存活可被关闭）；未知参数不产生任何遮罩窗体 |
| 5 | `install.ps1` 连跑两次成功；快捷方式目录两个指名 .lnk 存在且目标/参数正确 |
| 6 | git：`feature/mask-customization` 分支，工作树干净，≥4 个提交（勘察勘误/T7 评审、实现、构建+验证、安装+文档） |
| 7 | 文档：RESEARCH-V1.md 含 §9 T7 评审记录；docs/VERIFY-V1.md 含全部命令与输出摘要；使用说明.txt 为 v2 版 |
| 8 | 冻结项核查：`git diff main -- baseline/` 为空；`%LOCALAPPDATA%\BlackScreen\settings.ini` 缺失时新 exe 行为==纯黑无文字即 v1 行为（验证器已覆盖） |
| 9 | 源码纪律：src/ 无第三方引用；全部 .cs 为 UTF-8 BOM；build 通过即证明 C#5 兼容 |
| 10 | 收尾动作全部完成（§8） |

## 8. /goal 启动指令与收尾动作

**/goal 启动指令（交接者粘贴到执行窗口）**：

```text
/goal 按 GOAL-V1-PROMPT.md 执行契约完成"一键黑屏"v1 自定义化改造（R1 遮罩颜色、R2 遮罩文字、R3 设置入口，R4 双击立即出遮罩与 R5 退出交互冻结不变）：以仓库根 RESEARCH-V1.md §5 架构决策表为唯一事实源实现 src/ 并交付。完成证据：1) build.cmd 退出码 0 且产出 dist\一键黑屏.exe；2) verify 全部验证器 PASS 且退出码 0；3) install.ps1 幂等生效且双快捷方式目标正确；4) feature/mask-customization 分支工作树干净且 git diff main -- baseline/ 为空；5) RESEARCH-V1.md §9、docs/VERIFY-V1.md、使用说明.txt 齐备。以第 1–4 条命令输出与文件实据为唯一完成依据，计划与结论式回复不作数；本目标仅此一项，做到即停，过程中不留未完成待办。
```

**收尾动作（执行窗口最后执行）**：
1. README.md 文档地图核对为最新；`git status` 干净，全部提交在 feature 分支。
2. 在 docs/VERIFY-V1.md 末尾追加"交付快照"：构建时间、exe 大小、InputIdle 实测毫秒、验证器清单与结果。
3. 合并策略：**不自行 merge main**——保留 feature 分支并在最终报告中说明合并指令（`git checkout main && git merge feature/mask-customization`），由用户确认后执行。
4. 最终报告一份（对话内即可）：完成了什么、验证实据清单、遗留事项（若有）。

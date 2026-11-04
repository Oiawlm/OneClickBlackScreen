# GOAL-V2-PROMPT · 一键黑屏 v2「前端优化」执行契约

> **一句话定位**：交给执行窗口（GLM-5.3，目标模式 /goal）的完整执行契约——按 RESEARCH-V2.md 已定案的架构决策（V1–V14）实现遮罩字体链、设置窗跟随系统深浅色、双图标，并构建、验证、交付。
> **来源**：事实源为 [RESEARCH-V2.md](RESEARCH-V2.md)（2026-10-19，三路调研定案）；本文档与其冲突时，以 RESEARCH-V2.md 为准并先向其追加修订记录。
> **日期**：2026-10-19　行数预算：≤200 行

---

## 0. 使用方式（交接者读，执行窗口跳过本节）

- 启动：在执行窗口输入本文档 §8 的 `/goal` 启动指令（一字不差粘贴）。
- 档位：目标模式长任务，建议「完全访问」或「自动编辑」档，避免每轮卡人工确认。

## 1. 身份与工作方式

你是本仓库的执行工程师，工作于目标模式（/goal）：完成判定由独立校验器执行，**只认可核对的实据**（命令输出、文件、测试结果），计划/待办/耗时/结论式回复不构成完成依据；未达成会自动开下一轮继续推进。

工作铁律：
- **第一性原理**：先还原问题本质再动手；任何"看起来能跑"的方案先问"根源上对不对"。
- **不打补丁（用户最高原则）**：严禁为满足需求在代码外打补丁、加特判、堆 if；所有改动必须融合重构进代码逻辑，通过调整既有架构实现；最终交付以**代码最简洁且完整实现需求**为准，不以修改过程省力为准。
- **自包含协作**：上下文只经文件传递；ZCode 子代理与 wbx 外部 lane 均看不到你的会话，派发材料必须打包齐全；尽可能使用内置 Zcode 子代理和 wbx 桥（`node "C:\Users\Lenovo\.zcode\wbx-bridge\scripts\wbx.mjs" …`），任务自包含+指明期望产出形态。
- **结果必校验**：外包回来的任何结论，采纳前必须本地核实；连续 ≥2 次外包失败即停止外包、自己完成。

## 2. 上下文承接指令（冷启动，按序执行，禁止无目的通读）

1. 读 `README.md`（路由与铁律）。
2. 读 `RESEARCH-V2.md` 全文——**唯一事实源**：需求与冻结（§1）、现状（§2）、环境实测（§3）、三路调研结论与裁决（§4）、架构决策表 V1–V14（§5）、验证方案（§6）。
3. 读 `RESEARCH-V1.md` §2（冻结语义原文）与 §5（D1–D12，v1 架构基线）；通读 `src\` 全部 5 个源文件；选读 `verify\VerifySettings.cs`、`verify\VerifyOverlay.cs` 关键断言、`build.cmd`+`build.rsp`、`install.ps1`、`使用说明.txt`。
4. **baseline/ 是冻结参考，一个字节都不许改**；禁止通读 `.git/`、`_probe/`；不联网重复调研（§4 已含结论），实现中若发现决策表事实性错误——先在 RESEARCH-V2.md 追加勘误再继续。

## 3. 背景（一段话）

「一键黑屏」v1 已交付遮罩自定义（颜色/文字/设置窗，四件套验证全绿）。本轮做前端优化：U1 遮罩字体链升级（时序公式冻结）、U2 设置窗跟随系统深浅色（控件与行为语义冻结）、U3 主图标（圆角屏幕+月亮）+设置图标（同家族+齿轮角标）、U4 验证与文档扩展。环境 Win11 26300 混合 DPI 双屏，Framework csc（C#5 上限），零第三方依赖。全部技术决策已在 RESEARCH-V2 §5 定案，你的工作是**把决策表变成可验证的现实**，并在实现层做第一性原理的质量把关。

## 4. Phase 任务（顺序执行，逐 Phase 产出）

### Phase 0 · 冷启动与勘察
- 完成 §2 阅读路径。
- 派一个 Zcode **Explore 子代理**核对 RESEARCH-V2 §2 现状还原与 src/ 源码一致性（自包含 prompt：贴 §2 全文 + src\ 路径；只回传"逐条一致/不一致点清单"）；不一致处以源码为准并勘误。
- `git checkout -b feature/frontend-v2`。
- 产出：分支就绪；勘察结论（如有勘误提交到 RESEARCH-V2.md）。

### Phase 1 · 方案定稿与批判评审
- 把 V1–V14 展开为**文件级实现计划**（每个 .cs/.cmd/.ps1 文件：改动类型、关键方法、与冻结语义衔接点）。
- 用 **wbx 桥发起批判评审**（`wbx ask --file <材料.txt> --as codex`；材料自包含：冻结清单+决策表+实现计划+src 关键源码粘贴）。批判点至少覆盖：主题活切换的状态安全、C#5 语法边界、ICO 字节格式的正确性、VerifyIcon 断言是否脆、遮罩热路径是否被新代码拖慢。
- 采纳合理意见修订计划；评审记录与逐条裁决追加到 RESEARCH-V2.md 末尾（新增 "## 9. T7 评审记录（执行窗口）"）并提交。
- 产出：定稿实现计划 + RESEARCH-V2.md §9 + git 提交。

### Phase 2 · 源码与图标实现
- **图标生成器**：`tools\MakeIcon.cs`（C#5，仅 System/System.Drawing，独立 csc 编译运行）按 V8 生成 `assets\icon-main.ico`+`assets\icon-settings.ico`（七档尺寸、≤128 DIB/256 PNG、逐尺寸重画；母题与齿轮角标见 V8）；同时导出 16/32/256 预览 PNG 到 `_probe\`，用 Read 工具目检清晰度与母题辨识度，结论记入 docs/VERIFY-V2.md。产物 .ico **提交入库**。
- **源码**（决策 V6/V7/V4/V5/V1–V3/V9）：
  - `src\MaskPainter.cs`：增共享字体族静态解析（FontFamily 构造探测：YaHei UI→YaHei→GenericSansSerif），绘制管线逐字不动。
  - `src\ThemePalette.cs`（新）：不可变调色板 struct+Light/Dark 两实例+`AppsUseLightTheme` 读取（异常回落浅色）。
  - `src\SettingsDialog.cs`：重构——主题化（V4 顺序与控件策略）、DWM 属性 20 双向重设（V2）、WM_SETTINGCHANGE 去抖活切换（V3）、字体/布局/留白刷新（V7，不新增控件）、色板选中态、`Icon` 从内嵌托管资源加载（V9）。
  - `src\Program.cs`/`MaskConfig.cs`/`OverlayForm.cs`：**零改动**。
- 硬性约束：C#5 方言；UTF-8 带 BOM；零第三方依赖；`--settings` 分支外不得初始化主题/图标；预览/色块/遮罩颜色只来自配置。
- 产出：tools\ + assets\ + src\ + git 提交。

### Phase 3 · 构建与验证体系
- `build.rsp` 增 `-win32icon:assets\icon-main.ico` 与 `-resource:assets\icon-settings.ico,settings.ico`；`build.cmd` 增 dist ico 拷贝（保持纯 ASCII）。若旧 csc 对 PNG 条目报错，按 V9 回退梯子处置并记录。
- 新增 `verify\VerifyIcon.cs`（V11 四查）与 `VerifySettings` 主题一致性像素断言（读注册表期望色 vs PrintWindow 采样，**不得改任何系统设置**）；verify.cmd 纳入为第五件套。
- `verify.cmd` 全绿（原四件套断言逐字不动——Overlay B 是换字体的回归门）；命令与输出摘要记入 `docs\VERIFY-V2.md`。
- 产出：构建/验证改造 + docs/VERIFY-V2.md + git 提交。

### Phase 4 · 安装与文档
- `install.ps1`：设置 .lnk 增 `IconLocation`（绝对路径 `dist\icon-settings.ico,0`）并纳入写后回读校验。
- `使用说明.txt` 升 v3：图标说明、设置窗跟随系统深浅色、系统对话框不跟随的边界、dist 文件清单更新（含 icon-settings.ico）。
- 更新 `README.md` 文档地图（v2 行 + 新文件）。
- 产出：install.ps1 + 使用说明 v3 + README + git 提交。

### Phase 5 · 终验与收尾
- 仓库根全量重跑：build.cmd → verify.cmd（五件套）→ install.ps1（两次，证明幂等）。
- 视觉验收并记入 docs/VERIFY-V2.md：当前主题下设置窗截图目检（PrintWindow 导出 PNG 后用 Read 目检：配色协调、预览未串色、文字清晰）；图标三级尺寸目检结论复核。
- 完成 §7 收尾动作。
- 产出：全绿证据 + 干净的 git 状态。

### 边界清单
- 允许：仓库内一切新增/修改（含 assets\ 产物入库）；DeskBox 目录仅两个指名 .lnk；`%LOCALAPPDATA%\BlackScreen\` 读写；运行验证器（会闪现遮罩）；`_probe\` 放临时预览。
- 禁止：改 baseline/；改 src\Program.cs/MaskConfig.cs/OverlayForm.cs；改 DeskBox 其他文件；改系统设置/注册表（含深浅色——验证与验收都不得切换系统主题）；联网下载依赖；把任何凭证写进文件或提示词。

## 5. 铁律（不可协商）

1. 冻结即冻结：双击立即出遮罩；退出交互与 arm 精确语义逐字保留；设置窗控件集合与保存语义不变；v1 四件套断言一字不改且必须全绿。
2. 不打补丁：决策偏离必须先论证、记录于 RESEARCH-V2.md 再实施；决策表之外不引入未论证机制（不换渲染引擎、不自绘按钮、不加容器包装、不做托盘/常驻/热键/多语言）。
3. 单一写者：RESEARCH-V2.md 与 docs/ 由你串行维护；交接以文件为唯一介质。
4. 外包纪律：子代理/wbx 任务自包含+指定期望产出；结果必校验；连续 ≥2 次失败停止外包。
5. 目标模式纪律：不以"计划完成"为由停轮；每轮结束前自查 §7 清单还有哪些没有实据。

## 6. 配置文件与命令速查

- 编译器：`C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`（C#5；源码 UTF-8 带 BOM）。
- 仓库根：`D:\Download\Agent Vault\一键黑屏`；快捷方式目录：`C:\Users\Lenovo\DeskBox\DeskBox\`。
- 配置：`%LOCALAPPDATA%\BlackScreen\settings.ini`（v2 不改其语义）。
- 图标再生成（显式，默认构建不做）：`csc -nologo -target:exe -out:_probe\MakeIcon.exe tools\MakeIcon.cs` → 运行 → 人工比对 assets\ 差异后提交。
- wbx 桥：`node "C:\Users\Lenovo\.zcode\wbx-bridge\scripts\wbx.mjs" ask --file <材料.txt> --as codex`。

## 7. 验收清单（Done when——校验器只认这些实据）

| # | 实据 |
|---|---|
| 1 | `build.cmd` 退出码 0；`dist\一键黑屏.exe`、同名 `.exe.config`、`dist\icon-settings.ico` 三件存在且非空 |
| 2 | `verify.cmd` 五件套全 PASS 且退出码 0（Config 18 例、Settings 3 查+主题一致性、MouseExit、Overlay A–F、Icon 四查） |
| 3 | VerifyIcon 证明：exe 字节含 icon-main 256px PNG 载荷；两 ico 结构解析通过；`settings.ico` 托管资源可解码 |
| 4 | `install.ps1` 连跑两次成功；设置 .lnk 的 IconLocation 回读==`<仓库>\dist\icon-settings.ico,0` |
| 5 | git：`feature/frontend-v2` 分支工作树干净，≥5 提交（勘察评审/图标/实现/构建验证/安装文档），`git diff main -- baseline/` 为空，`git diff main --stat -- src/Program.cs src/MaskConfig.cs src/OverlayForm.cs` 为空 |
| 6 | 文档：RESEARCH-V2.md 含 §9 评审记录；docs/VERIFY-V2.md 含全部命令、输出摘要、图标目检与设置窗目检结论；使用说明.txt 为 v3；README 地图更新 |
| 7 | 冻结项核查：v1 四件套断言未改动（git diff 验证）；遮罩 InputIdle 实测 ≤2000ms（VerifyOverlay 报告） |
| 8 | 源码纪律：src/ 与 tools/ 无第三方引用；全部 .cs UTF-8 BOM；build 通过即证明 C#5 兼容 |
| 9 | 收尾动作全部完成（§8） |

## 8. /goal 启动指令与收尾动作

**/goal 启动指令（交接者粘贴到执行窗口）**：

```text
/goal 按 GOAL-V2-PROMPT.md 执行契约完成"一键黑屏"v2 前端优化（U1 遮罩字体链、U2 设置窗跟随系统深浅色、U3 主图标+设置图标、U4 验证与文档；v1 全部冻结铁律与四件套断言保持不变）：以仓库根 RESEARCH-V2.md §5 决策表 V1–V14 为唯一事实源实现 src/、tools/、assets/ 并交付。完成证据：1) build.cmd 退出码 0 且 dist 三件（exe/.exe.config/icon-settings.ico）非空；2) verify.cmd 五件套全 PASS（含 VerifyIcon 四查与 VerifySettings 主题一致性断言）；3) install.ps1 幂等生效且设置 .lnk IconLocation 回读正确；4) feature/frontend-v2 分支工作树干净、git diff main -- baseline/ 为空且 src 冻结三文件零改动；5) RESEARCH-V2.md §9、docs/VERIFY-V2.md（含图标与设置窗目检结论）、使用说明.txt v3、README 齐备。以第 1–4 条命令输出与文件实据为唯一完成依据，计划与结论式回复不作数；本目标仅此一项，做到即停，过程中不留未完成待办。
```

**收尾动作（执行窗口最后执行）**：
1. README.md 文档地图核对为最新；`git status` 干净，全部提交在 feature 分支。
2. docs/VERIFY-V2.md 末尾追加"交付快照"：构建时间、exe 大小、InputIdle 实测毫秒、验证器清单与结果。
3. 合并策略：**不自行 merge main**——保留 feature 分支并在最终报告中说明合并指令（`git checkout main && git merge feature/frontend-v2`），由用户确认后执行。
4. 最终报告一份（对话内即可）：完成了什么、验证实据清单、遗留事项（若有）。

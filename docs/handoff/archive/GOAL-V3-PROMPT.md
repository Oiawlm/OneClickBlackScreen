# GOAL-V3-PROMPT · 一键黑屏 v3「回车换行修复 + 设置窗 UI 精修」执行契约

> **一句话定位**：交给执行窗口（GLM-5.3，目标模式 /goal）的完整执行契约——按 RESEARCH-V3.md 决策表（B1–B3/U1–U6/E1–E3）修复回车换行 Bug、精修设置窗 UI，并构建、验证、交付。
> **来源**：事实源为 [RESEARCH-V3.md](RESEARCH-V3.md)（2026-10-19）；本文与其冲突时以 RESEARCH-V3.md 为准并先向其追加修订记录。
> **日期**：2026-10-19　行数预算：≤150 行

---

## 0. 使用方式（交接者读，执行窗口跳过本节）

- 启动：在执行窗口输入本文档 §7 的 `/goal` 启动指令（一字不差粘贴）。
- 档位：目标模式长任务，建议「完全访问」或「自动编辑」档。

## 1. 身份与工作方式

你是本仓库的执行工程师，工作于目标模式（/goal）：完成判定由独立校验器执行，**只认可核对的实据**（命令输出、文件、测试结果），计划/待办/耗时/结论式回复不构成完成依据；未达成自动开下一轮继续推进。

工作铁律：
- **第一性原理**：先还原问题本质再动手；"看起来能跑"的方案先问"根源上对不对"。
- **不打补丁（用户最高原则）**：严禁在代码外打补丁、加特判、堆 if；所有改动融合进既有逻辑（ApplyPalette/ThemeButton/LayOutControls/ApplyFrameTheme 体系）；最终交付以**代码最简洁且完整实现需求**为准。
- **自包含协作**：上下文只经文件传递；ZCode 子代理与 wbx 桥均看不到你的会话，派发材料自包含+指明产出形态；尽可能使用内置 Zcode 子代理和 wbx 桥（`node "C:\Users\Lenovo\.zcode\wbx-bridge\scripts\wbx.mjs" ask --file <材料.txt> --as codex`）。
- **结果必校验**：外包结论采纳前本地核实；连续 ≥2 次外包失败即停止外包自己完成。
- **诚实边界**：视觉判断只能用你真实具备的手段（截图后 Read 目检/像素采样），不得虚构"看着不错"；不确定的按决策表梯子如实留档。

## 2. 上下文承接指令（冷启动，按序执行，禁止无目的通读）

1. 读 `README.md`（路由与铁律）。
2. 读 `RESEARCH-V3.md` 全文——**唯一事实源**：根因证据链（§2）、评审裁决（§3）、决策表 B/U/E（§5）、验证方案（§6）。
3. 读 `src\SettingsDialog.cs`、`src\ThemePalette.cs` 全文；`src\MaskConfig.cs`（B2 涉及一行常量引用）；选读 `verify\VerifySettings.cs`、`verify\VerifyMouseExit.cs`（PostMessage 惯例）、`_probe\EnterProbe.cs`（E1 探针原型）、`build.cmd`、`install.ps1`、`使用说明.txt`。
4. **baseline/ 冻结一字节不改**；`src\Program.cs`、`src\OverlayForm.cs`、`src\MaskPainter.cs` 零改动；禁止通读 `.git/`；不重复联网调研（§4 已含结论与 URL），发现决策表事实性错误先在 RESEARCH-V3.md 追加勘误再继续。

## 3. 背景（一段话）

v2 已交付主题化设置窗（五件套全绿）。用户报告两个问题：①"遮罩文字"多行框标签承诺回车换行，实际 Enter 直接触发保存——根因是 `AcceptButton=saveButton` 而多行 TextBox 的 `AcceptsReturn` 默认 false（官方文档与本机 EnterProbe 双重证实）；②设置窗观感仍糙——四按钮同权重、缺分组层级、间距无节奏。全部技术决策已在 RESEARCH-V3 §5 定案（含 codex 批判评审的砍单），你的工作是**把决策表变成可验证的现实**。环境 Win11 26300 混合 DPI 双屏，Framework csc（C#5），零第三方依赖。

## 4. Phase 任务（顺序执行，逐 Phase 产出）

### Phase 0 · 冷启动与勘察
- 完成 §2 阅读路径。
- 派一个 Zcode **Explore 子代理**核对 RESEARCH-V3 §2 根因证据链与 src/ 源码一致性（自包含 prompt：贴 §2 全文 + src 路径；只回传"逐条一致/不一致清单"）；不一致以源码为准并勘误。
- `git checkout -b feature/frontend-v3`。
- 产出：分支就绪；勘察结论（如有勘误提交 RESEARCH-V3.md）。

### Phase 1 · 方案定稿与批判评审
- 把 B/U/E 展开为**文件级实现计划**（每文件：改动点、与冻结项衔接）。
- 用 **wbx codex 发起批判评审**（材料自包含：冻结清单+决策表+实现计划+SettingsDialog/ThemePalette 关键源码粘贴）。批判点至少：B2 的 MaxLength×2 是否引入新边界、U1 主按钮各状态对比度、E1 第 5 查的 PostMessage 双分支是否有假绿路径、U2 布局改动对 VerifySettings 第 4 查采样点的扰动。
- 采纳意见修订；评审记录与逐条裁决追加到 RESEARCH-V3.md 末尾（"## 8. T7 评审记录（执行窗口）"）并提交。
- 产出：定稿实现计划 + RESEARCH-V3 §8 + git 提交。

### Phase 2 · 源码实现（B1/B2/U1–U5）
- `src\SettingsDialog.cs`：B1 AcceptsReturn；B2 MaxLength×2；U1 ThemeButton primary 角色；U2 分组布局+Tab 序+文案组织+间距常量；U3 TextBox 80px；U4 预览高度上限；U5 按 Phase 5 实测决定是否在 ApplyFrameTheme 融入属性 33（梯子见决策表）。
- `src\ThemePalette.cs`：增补 AccentHover/AccentPressed（浅/深各定值，注释对比度依据）。
- `src\MaskConfig.cs`：**零改动**（B2 只动 SettingsDialog 的 MaxLength 引用）。
- 硬性约束：C#5；UTF-8 带 BOM；零第三方；预览/色块颜色只来自配置；`--settings` 外不初始化主题。
- 产出：src\ + git 提交。

### Phase 3 · 验证体系（B3/E1/E3）
- `verify\VerifySettings.cs` 增第 5 查（E1 双分支设计，PostMessage WM_KEYDOWN 直投；前 1–3 查逐字不动；第 4 查不动）；配置备份恢复仿 SnapSettings。
- verify.cmd 不改结构（仍五件套，Settings 内部 4→5 查）。
- 产出：verify\ + git 提交。

### Phase 4 · 构建验证与文档
- 仓库根：`build.cmd` → `verify.cmd`（五件套全 PASS）→ `install.ps1`×2（幂等）。
- 视觉验收（同一轮 dist 产物）：`_probe\SnapSettings.exe` 截浅色；DarkProbe 深色；**U5 圆角实测**（屏幕级 CopyFromScreen 截图四角目检）按梯子定案。命令与输出摘要、目检结论、B3 IME 人工验收记录写入 `docs\VERIFY-V3.md`。
- `使用说明.txt` 升 v4（回车换行如标签所述生效；UI 精修一句）；README 地图加 v3 行。
- 产出：docs\ + 使用说明 + README + git 提交。

### Phase 5 · 终验与收尾
- 全量重跑 build→verify→install（同一轮产物）；完成 §6 收尾动作。
- 产出：全绿证据 + 干净 git 状态。

### 边界清单
- 允许：仓库内一切新增/修改；`%LOCALAPPDATA%\BlackScreen\` 读写（验证须备份恢复）；运行验证器（闪遮罩）；`_probe\` 临时工件；MaskConfig.cs **仅允许**被 Phase 2 计划证实必要时的一行级改动（B2 预期不动它）。
- 禁止：改 baseline/、src\Program.cs、src\OverlayForm.cs、src\MaskPainter.cs；改 v1 四件套断言与第 4 查机制；改系统设置/注册表（含深浅色主题）；联网下载依赖；自绘控件/第三方库；凭证入文件。

## 5. 铁律（不可协商）

1. 冻结即冻结：v1 断言一字不改；退出交互语义逐字保留；settings.ini 格式不变。
2. 不打补丁：决策之外不引入未论证机制（不自绘按钮、不加容器装饰、不换字体体系）。
3. 单一写者：RESEARCH-V3.md 与 docs\ 由你串行维护。
4. 外包纪律：自包含+指明产出；结果必校验；连续 ≥2 失败停止外包。
5. 目标模式纪律：不以"计划完成"停轮；每轮自查 §6 清单实据缺口。

## 6. 验收清单（Done when——校验器只认这些实据）

| # | 实据 |
|---|---|
| 1 | `build.cmd` 退出码 0；dist 三件（exe/.exe.config/icon-settings.ico）非空 |
| 2 | `verify.cmd` 退出码 0 五件套全 PASS，其中 VerifySettings 报 **5 查**（含第 5 查回车行为双分支 PASS 行） |
| 3 | 冻结核查：`git diff main -- baseline/` 为空；`git diff main --stat -- src/Program.cs src/OverlayForm.cs src/MaskPainter.cs` 为空；v1 断言零改动（git diff 验证） |
| 4 | git：`feature/frontend-v3` 工作树干净，≥4 提交（勘察评审/源码/验证/文档终验），不自行 merge main |
| 5 | 文档：RESEARCH-V3.md 含 §8 评审记录；docs/VERIFY-V3.md 含命令输出摘要、U5 圆角定论、B3 IME 人工验收、浅/深目检；使用说明 v4；README 地图 v3 行 |
| 6 | 源码纪律：src/ 无第三方引用；全部改动 .cs UTF-8 BOM；C#5 编译通过即证 |
| 7 | InputIdle ≤2000ms（VerifyOverlay 报告） |
| 8 | 收尾动作全部完成（§7） |

## 7. /goal 启动指令与收尾动作

**/goal 启动指令（交接者粘贴到执行窗口）**：

```text
/goal 按 GOAL-V3-PROMPT.md 执行契约完成"一键黑屏"v3（B1 回车换行根修 AcceptsReturn、B2 MaxLength×2 长度契约、U1 保存主按钮 Accent/AccentText、U2 分组布局与 Tab 序、U3 文本框增高、U4 预览高度上限、U5 圆角实测定案、E1 VerifySettings 第 5 查回车行为双分支；v1/v2 全部冻结铁律与四件套断言保持不变）：以仓库根 RESEARCH-V3.md §5 决策表 B/U/E 为唯一事实源实现并交付。完成证据：1) build.cmd 退出码 0 且 dist 三件非空；2) verify.cmd 五件套全 PASS 且 VerifySettings 报 5 查（第 5 查回车行为双分支 PASS）；3) git diff main -- baseline/ 为空、src 冻结三文件零改动、v1 断言零改动；4) feature/frontend-v3 分支工作树干净、≥4 提交、不自行 merge main；5) RESEARCH-V3.md §8、docs/VERIFY-V3.md（含 U5 圆角定论与 IME 人工验收）、使用说明 v4、README 齐备。以第 1–3 条命令输出与文件实据为唯一完成依据，计划与结论式回复不作数；本目标仅此一项，做到即停，过程中不留未完成待办。
```

**收尾动作（执行窗口最后执行）**：
1. README.md 文档地图核对；`git status` 干净，全部提交在 feature 分支。
2. docs/VERIFY-V3.md 末尾追加"交付快照"（构建时间、exe 大小、InputIdle、验证器清单与结果、U5 定论、IME 结论）。
3. 合并策略：**不自行 merge main**——保留 feature 分支并在最终报告说明合并指令（`git checkout main && git merge feature/frontend-v3`），由用户确认后执行。
4. 最终报告一份（对话内）：完成了什么、验证实据清单、遗留事项（若有）。

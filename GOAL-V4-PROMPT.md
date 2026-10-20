# GOAL-V4-PROMPT · 一键黑屏 v4「fts 风格体系统一」执行契约

> **一句话定位**：交给执行窗口（GLM-5.3，目标模式 /goal）的完整执行契约——按 RESEARCH-V4.md 决策表（S1–S6/D1–D5/E1–E4）统一 fts 风格体系（可见重设计＋token 纪律＋镜像锁定），构建、验证、交付并**合并进 main**。
> **来源**：事实源为 [RESEARCH-V4.md](RESEARCH-V4.md)（2026-10-19）；本文与其冲突时以 RESEARCH-V4.md 为准并先向其追加修订记录。
> **日期**：2026-10-19　行数预算：≤150 行

---

## 0. 使用方式（交接者读，执行窗口跳过本节）

- 启动：在执行窗口输入本文档 §7 的 `/goal` 启动指令（一字不差粘贴）。
- 档位：目标模式长任务，建议「完全访问」或「自动编辑」档。

## 1. 身份与工作方式

你是本仓库的执行工程师，工作于目标模式（/goal）：完成判定由独立校验器执行，**只认可核对的实据**（命令输出、文件、测试结果），计划/待办/耗时/结论式回复不构成完成依据；未达成自动开下一轮继续推进。

工作铁律：
- **第一性原理**：先还原问题本质再动手；"看起来能跑"的方案先问"根源上对不对"。
- **不打补丁（用户最高原则）**：严禁在代码外打补丁、加特判、堆 if；所有改动融合进既有体系（Metrics/ThemePalette/ApplyPalette/ThemeButton/LayOutControls）；最终交付以**代码最简洁且完整实现需求**为准。
- **自包含协作**：上下文只经文件传递；ZCode 子代理与 wbx 桥均看不到你的会话，派发材料自包含＋指明产出形态；尽可能使用内置 Zcode 子代理和 wbx 桥（`node "C:\Users\Lenovo\.zcode\wbx-bridge\scripts\wbx.mjs" ask --file <材料.txt> --as codex`）。
- **结果必校验**：外包结论采纳前本地核实；连续 ≥2 次外包失败即停止外包自己完成。
- **诚实边界**：视觉判断只能用你真实具备的手段（截图后 Read 目检/像素采样），不得虚构"看着不错"；不确定的按决策表梯子如实留档。

## 2. 上下文承接指令（冷启动，按序执行，禁止无目的通读）

1. 读 `README.md`（路由与铁律）。
2. 读 `RESEARCH-V4.md` 全文——**本轮唯一事实源**：缺口证据链（§2）、fts 体系映射（§3）、决策表 S/D/E（§5，含派生规则与评审裁决）、验证方案（§6）。
3. 读 `src\SettingsDialog.cs`、`src\ThemePalette.cs` 全文；选读 `verify\VerifySettings.cs`（第 4/5 查结构）、`verify.cmd`（共编先例在第 20 行 VerifyConfig）、`build.cmd`、`使用说明.txt`、`docs\VERIFY-V3.md`（上轮基线）。
4. fts 体系原文可按需精读：`C:\Users\Lenovo\.zcode\skills\fts-style-system\SKILL.md` 及其 references\。**优先级（评 #1）**：用户冻结约束 ＞ RESEARCH-V4 定案 ＞ skill 原文——skill 与定案的差异只触发 RESEARCH-V4 勘误流程，不得直接推翻定案或冻结。
5. **冻结清单**：baseline/ 一字节不改；`src\Program.cs`、`src\OverlayForm.cs`、`src\MaskPainter.cs`、`src\MaskConfig.cs` 零改动（比 v3 更严：MaskConfig 本轮无豁免）；v1 断言与 VerifySettings 第 5 查逐字不动；禁止通读 `.git/`；不重复联网调研（RESEARCH-V4 §4 已含结论与 URL），发现决策表事实性错误先在 RESEARCH-V4.md 追加勘误再继续。

## 3. 背景（一段话）

v3 已交付六态主按钮、语义间距与回车根修（feature/frontend-v3，尚未合并 main）。用户拍板以悬浮中转站风格体系（fts skill）统一本软件：**纪律＋可见重设计**——字阶拆档（14px Body/12px Caption/区块标签 Bold）、间距归 4px 网格（间距档与结构尺寸分开）、布局数值单源化（SettingsDialogMetrics 只存逻辑值）、镜像锁定测试根除验证器幽灵色值（0xF5F5F5/0x202020 副本）、强调色保持 Win11 蓝不抄 fts 紫、零自定义动效与 DWM 圆角维持登记不变。全部技术决策已在 RESEARCH-V4 §5 定案（含规划侧 codex 评审判决 22 条），你的工作是**把决策表变成可验证的现实**，最终两步合并进 main（用户明确授权，含 v3 一并带入）。环境 Win11 26300 混合 DPI 双屏，Framework csc（C#5），零第三方依赖。

## 4. Phase 任务（顺序执行，逐 Phase 产出）

### Phase 0 · 冷启动与勘察
- 完成 §2 阅读路径。分支（评 #22 续跑友好）：`feature/style-v4` 不存在则 `git checkout -b feature/style-v4 feature/frontend-v3`（**必须自 v3 分支切出**，main 缺 v3 实现）；已存在则核验基线后续用。
- **记录起始 SHA**（评 #19）：`git rev-parse feature/frontend-v3 main` 写入工作笔记（冻结核查与提交计数的固定基准，后续不再用可漂移的分支名比对）。
- 派一个 Zcode **Explore 子代理**核对 RESEARCH-V4 §2 缺口证据链与源码一致性（自包含 prompt：贴 §2 全文＋src/verify 路径；只回传"逐条一致/不一致清单"）；不一致以源码为准并勘误。
- 产出：分支就绪＋起始 SHA；勘察结论（如有勘误提交 RESEARCH-V4.md）。

### Phase 1 · 方案定稿与批判评审
- 把 S/D/E 展开为**文件级实现计划**（每文件：改动点、与冻结项衔接、Metrics 值推导表）。
- 用 **wbx codex 发起批判评审**（材料自包含：冻结清单＋决策表＋实现计划＋SettingsDialog/ThemePalette 关键源码粘贴＋**VerifySettings 第 5 查完整定位逻辑**——证明 S1 字阶与几何变化不触碰其类名枚举/定位机制，评 #5）。批判点至少：第 5 查兼容条件是否成立、S1 横向预算（按钮行总宽 ≤408、标签与提示不重叠）验算、D3 DPI 复算口径（逐项 S() 舍入、累加顺序、物理像素）是否可实现为确定断言、D2 共编独立性边界、E4 两步合并时序。
- 采纳意见修订；评审记录与逐条裁决追加到 RESEARCH-V4.md §8 并提交。
- 产出：定稿实现计划 ＋ RESEARCH-V4 §8 ＋ git 提交。

### Phase 2 · 源码实现（S1/S2/D1/D4）
- 新建 `src\SettingsDialogMetrics.cs`（D1：间距档 8/12/16/20、行高/宽度/字号族唯一声明＋派生注释；**只存逻辑值**，运行时唯一派生路径＝S()）。
- `src\SettingsDialog.cs`：S1 字阶两档＋区块标签 Bold（Font 单点派生，控件各取其档；行高/按钮宽按 Phase 1 评审后的 TextRenderer 设计期实测定值）；S2 rowGap 10→12、结构尺寸按对齐目标推导（不网格化）、预览上限重算＋WorkingArea 包含；布局全部改引用 Metrics（LayOutControls 方法体零布局字面量）。
- `src\ThemePalette.cs`：预期零结构改动（S3 维持；若确需加键，先在 RESEARCH-V4 勘误再动）。
- 硬性约束：C#5；UTF-8 带 BOM；零第三方；预览/色板颜色只来自配置；`--settings` 外不初始化主题；build.rsp 加新源文件。
- 产出：src\ ＋ build.rsp ＋ git 提交。

### Phase 3 · 验证体系（D2/D3/E1）
- `verify.cmd`：VerifySettings 编译行加 `src\ThemePalette.cs src\SettingsDialogMetrics.cs`。
- `verify\VerifySettings.cs`：第 4 查期望值改读 ThemePalette 单源（删幽灵副本）＋采样条带与子控件不相交断言；**新增第 6 查**（D3 全契约：DPI awareness 设置与核验、物理像素复算口径、几何 ±1px、WorkingArea 包含、主按钮 normal 态角色色采样规范、失败诊断输出）；前 1–3 查与第 5 查逐字不动；verify.cmd 五件套结构不变（Settings 内部 5→6 查）。
- 产出：verify\ ＋ verify.cmd ＋ git 提交。

### Phase 4 · 构建验证与文档（E2/E3）
- 仓库根：`build.cmd` → `verify.cmd`（五件套全 PASS，Settings 报 6 查）→ `install.ps1`×2（幂等，比较对象＝快捷方式属性回读）。
- 视觉验收（同一轮 dist 产物）：SnapSettings 浅色截、DarkProbe 深色截、两块不同 DPI 屏各开一次、与 _probe\settings-v3-baseline.png 对比字阶/间距；fts 清单适配版逐项勾选＋反例库 20 条对照表＋Metrics 值推导表＋**原始证据清单**（命令原文/退出码/日志路径 _probe\verify-v4-*.log/exe SHA-256/截图路径与主题 DPI 条件/InputIdle 原值）写入 `docs\VERIFY-V4.md`。
- `使用说明.txt` 升 v5；README 地图加 v4 行。
- 产出：docs\ ＋ 使用说明 ＋ README ＋ git 提交。

### Phase 5 · 终验与两步合并（E4）
- 全量重跑 build→verify→install（同一轮产物）；§6 验收清单自查。
- **两步合并**（用户授权）：① feature/style-v4 上 `git merge main`（吸收 v4 规划提交，解冲突），**融合态重跑 build→verify→install 终验**；② 核验 main 仍在起始 SHA 后 `git checkout main && git merge --no-ff feature/style-v4 -m "merge: v3+v4——回车根修/UI 精修/fts 风格体系统一（S/D/E 全量+五件套 6 查全绿）"`；合并后 `git diff feature/style-v4 main` 须为空、两分支工作树干净。交付快照已在 Phase 4 定稿——merge 提交号只写入对话内最终报告，不回写仓库文件。
- 产出：全绿证据 ＋ main 合并提交 ＋ 干净工作树。

### 边界清单
- 允许：仓库内一切新增/修改；`%LOCALAPPDATA%\BlackScreen\` 读写（验证须备份恢复）；运行验证器（闪遮罩）；`_probe\` 临时工件；RESEARCH-V4 §5 档位登记表追加勘误记录。
- 禁止：改 baseline/、四冻结源文件、v1 断言与第 5 查；改系统设置/注册表（含深浅色主题）；联网下载依赖；自绘控件/第三方库；凭证入文件；**合并前跳过终验或合并后回写仓库文件**；改 MaskConfig.cs（本轮无 v3 式豁免）。

## 5. 铁律（不可协商）

1. 冻结即冻结：v1 断言一字不改；退出交互语义逐字保留；settings.ini 格式不变。
2. 不打补丁：决策之外不引入未论证机制（不自绘、不加容器装饰、不换字体体系、不引 Mica）。
3. 单一写者：RESEARCH-V4.md 与 docs\ 由你串行维护。
4. 外包纪律：自包含＋指明产出；结果必校验；连续 ≥2 失败停止外包。
5. 目标模式纪律：不以"计划完成"停轮；每轮自查 §6 清单实据缺口。

## 6. 验收清单（Done when——校验器只认这些实据，全部 AND）

| # | 实据 |
|---|---|
| 1 | `build.cmd` 退出码 0；dist 三件（exe/.exe.config/icon-settings.ico）非空 |
| 2 | `verify.cmd` 退出码 0 五件套全 PASS，VerifySettings 报 **6 查**（第 6 查几何＋主按钮角色色锁定 PASS） |
| 3 | 幽灵清零：`grep -R -nE "0xF5F5F5\|0x202020" src verify` 仅 ThemePalette.cs 命中（或零命中；表格内转义仅限显示，直跑命令以 §7 启动指令内原文为准）；SettingsDialog.cs 的 LayOutControls 方法体无内联布局字面量（全引用 SettingsDialogMetrics） |
| 4 | 冻结核查（以 Phase 0 起始 SHA 为基准）：`git diff <v3SHA> -- baseline/ src/Program.cs src/OverlayForm.cs src/MaskPainter.cs src/MaskConfig.cs` 为空；v1 断言与第 5 查零改动（git diff 验证） |
| 5 | git：feature/style-v4 干净、`git rev-list --count <v3SHA>..HEAD` ≥4（有意义变更，不凑数）；**main 已合并**：merge 提交两父节点可验、`git diff feature/style-v4 main` 为空、main 工作树干净 |
| 6 | 文档：RESEARCH-V4.md 含 §8 评审记录；docs/VERIFY-V4.md 含命令输出摘要、原始证据清单（E3）、双主题双 DPI 截图目检、fts 清单逐项勾选、反例库 20 条对照表、Metrics 推导表；使用说明 v5；README 地图 v4 行 |
| 7 | 源码纪律：src/ 无第三方引用；全部改动 .cs UTF-8 BOM；C#5 编译通过即证 |
| 8 | InputIdle ≤2000ms（VerifyOverlay 报告，原值入 VERIFY-V4） |

## 7. /goal 启动指令与收尾动作

**/goal 启动指令（交接者粘贴到执行窗口）**：

```text
/goal 按 GOAL-V4-PROMPT.md 执行契约完成"一键黑屏"v4 fts 风格体系统一（S1 字阶两档+区块标签 Bold、S2 间距归 4px 网格与结构尺寸分治、S3 强调色维持 Win11 蓝、S4/S5 圆角与动效零改动登记、D1 SettingsDialogMetrics 布局单源、D2 验证器期望值改读 ThemePalette 根除幽灵色值、D3 VerifySettings 新增第 6 查几何+主按钮角色色镜像锁定、E1–E3 验证与文档、E4 两步合并 main；v1/v2/v3 全部冻结铁律与既有断言保持不变）：以仓库根 RESEARCH-V4.md §5 决策表 S/D/E 为唯一事实源，自 feature/frontend-v3 切 feature/style-v4 实现并交付。完成证据（全部 AND，缺一即未完成）：1) build.cmd 退出码 0 且 dist 三件非空；2) verify.cmd 五件套全 PASS 且 VerifySettings 报 6 查（第 6 查几何+主按钮角色色 PASS）；3) grep -R -nE "0xF5F5F5|0x202020" src verify 证实副本已清、LayOutControls 零内联布局字面量；4) 以起始 SHA 核对 git diff 冻结项为空、v1 断言与第 5 查零改动；5) feature/style-v4 干净且 rev-list ≥4、E4 两步合并完成（git diff feature/style-v4 main 为空、main 工作树干净）；6) RESEARCH-V4 §8、docs/VERIFY-V4.md（原始证据清单+双主题双 DPI 截图+fts 清单+反例对照+Metrics 推导表）、使用说明 v5、README 齐备；7) 全部改动 .cs UTF-8 BOM 且 C#5 编译通过；8) InputIdle ≤2000ms。以第 1–5、7、8 条命令输出与文件实据为唯一完成依据，计划与结论式回复不作数；本目标仅此一项，做到即停，过程中不留未完成待办。
```

**收尾动作（执行窗口最后执行）**：
1. README.md 文档地图核对；main 与 feature/style-v4 `git status` 均干净（合并后不再回写仓库文件）。
2. 最终报告一份（对话内）：完成了什么、验证实据清单、merge 提交号与最终 HEAD、遗留事项（若有）。

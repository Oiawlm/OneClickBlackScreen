# RESEARCH-V4 · 一键黑屏 v4「fts 风格体系统一」事实源

> **一句话定位**：v4 轮唯一事实源——把悬浮中转站（fts）风格体系（C:\Users\Lenovo\.zcode\skills\fts-style-system\SKILL.md）套用本仓：可见重设计定案（S 系列）、token 纪律与镜像锁定定案（D 系列）、验证面演进定案（E 系列）；执行窗口按 §5 决策表实现。
> **来源**：fts skill 全文＋references 五篇 + src/verify 源码通读（feature/frontend-v3 版本）+ ZCode Explore 联网调研（Fluent 2 官方档位/W3C DTCG token 规范）+ 用户拍板（2026-10-19：「纪律＋可见重设计」）+ wbx codex 批判评审（job 20261020-001201-aq4，22 条全采纳，裁决内嵌各条）。
> **日期**：2026-10-19　行数预算：≤220 行。与 GOAL-V4-PROMPT.md 冲突时以本文为准。**优先级**：用户冻结约束 ＞ 本文定案 ＞ fts skill 原文（skill 差异仅触发本文勘误流程，不直接推翻定案）。

## 1. 需求与冻结

**用户需求**：调用 fts-style-system skill 统一本软件风格；完成后直接合并进 main。规划窗口与用户对齐定案口径＝**纪律＋可见重设计**（体系纪律为主干，视觉向 fts 家族靠拢的可见变化一并做）。

**冻结不变**（v1/v2/v3 全部继承）：双击立即出遮罩；退出交互与 arm 防误触语义；settings.ini 格式语义；v1 四件套断言（VerifyConfig 18 例 / VerifySettings 前 3 查 / VerifyMouseExit / VerifyOverlay A–F）逐字不动；VerifySettings 第 5 查（回车契约）不动；baseline/ 一字节不改；`src\Program.cs`、`src\OverlayForm.cs`、`src\MaskPainter.cs`、`src\MaskConfig.cs` 零改动；遮罩/预览/色板颜色只来自配置（SwatchColors 与 MaskPainter 字体链是配置域非主题域）；保存语义四按钮功能不变。

**可演进**：SettingsDialog 全部视觉与布局；ThemePalette 键位与值；verify.cmd 编译行、VerifySettings 第 4 查内部实现（断言语义不变）与新增查；verify/docs/使用说明/README。

**分支与合并**（用户授权）：v4 叠在 v3 之上——`git checkout -b feature/style-v4 feature/frontend-v3`；合并走**两步闭环**（E4）：先把 main（含 v4 规划提交）融进 feature 分支并在融合态重跑终验，再回 main 执行 `--no-ff` 合并。

## 2. 风格现状证据链（缺口清单，行号≈feature/frontend-v3 版）

1. **幽灵 token（fts 约束①/A1 直接命中）**：`verify\VerifySettings.cs` CheckThemeBackground 内 `int expected = light ? 0xF5F5F5 : 0x202020;`——ThemePalette.Bg 的第二份数值副本。换色即漂移，且「渲染==数值源」无从锁定。
2. **布局数值散落无体系**：`src\SettingsDialog.cs` LayOutControls 局部变量 edge/labelGap/rowGap/groupGap/labelHeight/swatchRow/buttonRow/textHeight/hintHeight（v3 已语义化）＋仍内联的字面量 S(408)/S(36)/S(28)/S(116)/S(228)/S(220)/按钮宽 88/75/104/75——两档混杂，无唯一声明点（fts「结构性尺寸不算 token 但须唯一声明」）。
3. **档位未登记**：间距 rowGap=10 出 Fluent 4px 网格（4/8/12/16/20）；字号全窗单档 9pt（=12px，Fluent Caption 档）无层级拆分；无任何档位登记文档（本文件 §5 即登记落点）。
4. **无镜像锁定测试**：现有断言只锁「背景跟随系统主题」，不锁「主按钮角色色/区块几何 == 数值源」；fts 的核心机制（UI 层镜像 == 数值源）缺失。
5. **已合规项（零改动登记）**：ThemePalette 11 键全部语义命名、双主题同键位；主按钮六态 WCAG 验证（v3）；窗口圆角＝系统 DWM 8px（v3 U5 实证零改动）；按钮高 32px＝Fluent 默认；swatch hover/press 不换色；ApplyPalette 原位重刷＝动态跟随；150ms 去抖＝时序门闩非动画。

## 3. fts 体系速览与本项目映射（浓缩自 skill，可回原文核对）

六约束→本项目：①单一数值源＋镜像锁定→ThemePalette(色)＋Metrics(布局) 双唯一源＋VerifySettings 锁定；②恒定几何→FixedDialog 手工布局已恒定，维持；③档位制→§5 S2 登记；④两缓动＋减动效→登记零自定义动效（S5）；⑤分层状态/阴影隔离→WinForms 无 Effect/阴影，FlatAppearance 状态色即平台映射；⑥克制与真实可达→v3 已达标，验证器 PostMessage/AttachThreadInput 均真实消息层。

反例库 20 条逐条裁决（A1–A20）→命中 2 条：**A1 幽灵 token**（§2.1，D2 根修）；**档位外散值**（A16 同族，§2.2–3，D1+S2 根修）。其余 18 条经映射不命中（多为 WPF 专属：overlay 滚动条/卡片三层/UniformGrid/分层透明壳等；WinForms 对应面不存在或已合规）。逐条对照表落 docs/VERIFY-V4.md（执行窗口产出）。

**平台映射定论**（Explore 调研，§4）：DarkMode_Explorer 只影响 EDIT 滚动条等 chrome，Button/Label 不受影响→现有手动设色是必要非冗余；完整六态需 OnPaint 自绘→v3 已裁决不自绘，维持；控件级圆角 4 需 Region 无抗锯齿→登记「平台不可达例外」，窗口圆角唯一权威=DWM（不造第二 token，反 A1）；Mica backdrop 与 GDI 不透明控件混合花斑→砍单（S6）。

## 4. 调研来源清单

| 结论 | 来源 |
|---|---|
| Fluent 字阶 Caption 12/16、Body 14/20、Body Strong 14 Semibold | fluent2.microsoft.design/typography + learn.microsoft.com/.../signature-experiences/typography |
| Fluent 间距 4px 基准网格（icon 邻接例外 2/6/10） | fluent2.microsoft.design/layout + /design-tokens |
| 控件圆角默认 4、浮层/窗口 8（窗口由 DWM 强制） | learn.microsoft.com/.../signature-experiences/geometry |
| Button/TextBox 默认高 32 epx | WinUI 默认样式（TextControlThemeMinHeight） |
| DarkMode_Explorer 仅影响部分 chrome，Button/Label 无效；句柄重建需重套 | stackoverflow.com/questions/72988434（社区，与本项目 v2 实测一致） |
| GetDpiForWindow/GetWindowRect 的 DPI 虚拟化口径 | learn.microsoft.com/.../winuser/nf-winuser-getdpiforwindow |
| W3C DTCG Design Tokens Format 2025.10 稳定版：token={value,type}＋alias | designtokens.org/TR/2025.10/format |
| token 单一数值源方法论（生成或断言消除漂移） | martinfowler.com/articles/design-token-based-ui-architecture.html |
| WinForms 主题库均为接管绘制路线（与零依赖冲突） | MaterialSkin.2/DarkUI/ReaLTaiizor 概览 |
| fts 六约束/档位制/反例库/七步工作流 | 本机 skill：fts-style-system（含五 references） |

## 5. 决策表（唯一实现依据）

### S 系列 · 可见重设计（用户拍板「纪律＋可见重设计」）
| # | 决策 |
|---|---|
| S1 | **字阶两档＋一 Bold**：区块标签（"遮罩颜色/遮罩文字"）与全部控件文本（按钮/输入框）＝10.5pt（Point 自带 DPI 缩放＝96DPI 下 14px，Fluent Body 档）；提示文字（hintLabel/textHintLabel）＝9pt（＝12px，Caption 档）；区块标签加 Bold（Body Strong 语义）。**推翻 v3「9pt 统一」裁决，依据＝用户 2026-10-19 拍板＋fts 字阶层级是家族识别特征**。派生规则（评 #9/#11）：行高与按钮宽＝设计期用 TextRenderer.MeasureText（指定 Font/DPI/Default flags）实测定值进 Metrics，不以 em 数值冒充控件尺寸；按钮行总宽（四钮＋3×gap）≤ 内容宽 408、区块标签与右侧提示不重叠，定稿值须附验算。**第 5 查兼容条件**（评 #5）：Phase 1 评审材料必含其完整定位逻辑（类名枚举/坐标依赖面），证明字阶与几何变化不触碰其机制后方可实现 |
| S2 | **间距档位制**：登记 4px 基准网格四档 8/12/16/20（edge16/groupGap20/labelGap8 保持，rowGap 10→12 归档）。**间距档与结构尺寸分开**（评 #12）：swatchRow、custom 钮 y 偏移属行盒/对齐结构尺寸——先定色块与自定义钮中心线对齐目标，再由矩形推导偏移，登记但不强行网格化。预览上限（评 #14）：S1 增高 fixedChrome 后重算 previewCap，**新增断言：最终窗口矩形包含于目标屏 WorkingArea**（D3 可查），删除「真实显示器不会出现」假设；预算不足策略＝压缩预览至地板并如实报错留档，不允许越界。档位登记表落本文件（改动须追加勘误记录） |
| S3 | 强调色保持 Win11 蓝系（#0067C0/#4CC2FF 及六态派生，v3 已 WCAG 验证）——skill 铁则「数值按项目推导，不盲目照抄 fts 紫」。零改动登记 |
| S4 | 圆角：窗口级＝系统 DWM 8（零改动）；控件级 4＝WinForms 无 AA 路径，登记平台不可达例外，不造幽灵 token |
| S5 | 动效：登记零自定义动效（150ms 去抖＝时序门闩非动画；fts 克制原则下工具对话框不加装饰动效）。零改动 |
| S6 | 砍单清单（不做，理由留档）：Mica/backdrop（GDI 不透明控件花斑）、自绘圆角/六态（v3 裁决维持）、fts 紫强调（S3）、预览装饰边框（v3 裁决维持） |

### D 系列 · 纪律（token 单源＋镜像锁定）
| # | 决策 |
|---|---|
| D1 | **布局数值单源**：新建 `src\SettingsDialogMetrics.cs`（静态类，命名仿 fts SettingsWindowMetrics）：间距档/行高族/宽度族（内容宽、按钮宽×4、色板节距与尺寸、自定义钮宽、提示右区宽）、字号档（10.5pt/9pt）唯一声明，每值一行派生注释。**Metrics 只存逻辑值**（评 #10）：运行时几何唯一派生路径＝既有 S() 逐项换算，不建第二套缓存、无 DPI 回退特判；设计期 TextRenderer 实测只用于定值，不在运行时动态测量 |
| D2 | **镜像锁定（色）**：verify.cmd 的 VerifySettings 编译行加 `src\ThemePalette.cs`（沿用 VerifyConfig+MaskConfig.cs 共编先例）；第 4 查期望值改读 `ThemePalette.Light/Dark.Bg`，删除 0xF5F5F5/0x202020 副本；断言语义不变。**边界**（评 #16）：共编锁定「渲染==当前源值」，不证明源值本身的设计正确性——值正确性由 E2 视觉验收把关；共编范围限 ThemePalette＋Metrics，不延伸到生产布局函数。采样条带（评 #13）：固定为底部 padding 带与左边缘带，并新增断言「采样矩形与全部可见子控件矩形不相交」（EnumChildWindows 核验），禁止挑恰好通过的点 |
| D3 | **镜像锁定（几何＋主按钮角色色）**：VerifySettings 增第 6 查（现有 5 查新增，非改写）。几何契约（评 #3/#4）：验证器启动即设 DPI awareness（PerMonitorAwareV2，失败退 SetProcessDPIAware，再失败显式报错）；断言口径＝**目标窗口客户区物理像素** vs「按 LayOutControls 同一算法复算」——验证器用 GetDpiForWindow(hwnd) 得 dpiScale，按 Metrics 逻辑值逐项 S() 舍入、累加顺序=布局执行顺序复算期望矩形，±1px 容差；窗口矩形包含于 WorkingArea 断言（S2）。主按钮色（评 #2/#17）：采样前核验按钮 enabled、焦点态已知（未聚焦或固定聚焦态按该态期望）、GetCursorPos 不在按钮矩形内、窗口 bounds 稳定；采样按钮内部多个无字区域（四角内缩点），期望＝ThemePalette.Accent（normal 态、当前系统主题档）。**六态其余态（hover/pressed/focus/disabled）与深色档登记为像素断言不可达例外**，由 E2 截图目检＋源码引用审查覆盖；断言失败须输出 DPI/复算值/实测值诊断 |
| D4 | ThemePalette 键位审计结论：11 键语义化合规，HoverBorder 平台不可达登记；结构零改动。全仓幽灵清剿（评 #18）：审计范围＝全部 `Color.FromArgb`/十六进制色值出现点＋布局字面量——合法落点仅 ThemePalette（主题域）与 SwatchColors/MaskConfig（配置域），其余零容忍 |
| D5 | 文档体系：本 §5 档位登记表即本项目 design.md 等价物；值推导表/反例 20 条对照表落 docs/VERIFY-V4.md（评 #21：正文只留定案，推导与证据入 VERIFY）；原始证据清单见 E3 |

### E 系列 · 验证面与交付
| # | 决策 |
|---|---|
| E1 | VerifySettings：第 4 查按 D2 改造（前 3 查逐字不动、第 5 查不动）；新增第 6 查（D3）；verify.cmd 五件套结构不变（Settings 内部 5→6 查） |
| E2 | 视觉验收（同一轮 dist 产物）：SnapSettings 浅色 + DarkProbe 深色截图；**两块不同 DPI 屏各开一次**（评 #15：跨屏移动不重排＝登记语义，对话框以打开屏几何定格；预览按主屏比例为 v1 有意设计，维持登记）；fts 清单适配版逐项核对；字阶与间距前后对比截图留档 _probe\ |
| E3 | 文档：使用说明 v5；README 地图 v4 行；docs/VERIFY-V4.md。**原始证据清单**（评 #20）：每条命令原文＋退出码＋原始日志落盘路径（_probe\verify-v4-*.log）、exe SHA-256、截图路径＋主题/DPI 条件、InputIdle 原值、两次 install 比较对象与配置恢复核验 |
| E4 | **合并 main**（用户授权，两步闭环，评 #6）：① 终验全绿后先在 feature/style-v4 上 `git merge main`（吸收 v4 规划提交，解冲突），**融合态重跑 build→verify→install 终验**；② 核验 main 未移动（起始 SHA 比对）后 `git checkout main && git merge --no-ff feature/style-v4`，合并后 `git diff feature/style-v4 main` 须为空、工作树干净。交付快照（E3）在合并**前**定稿——merge 提交号只进对话内最终报告，不回写仓库文件（评 #7） |

## 6. 验证方案

1. `build.cmd` 退出码 0，dist 三件非空。
2. `verify.cmd` 五件套全 PASS；VerifySettings 报 **6 查**（第 4 查期望值来自 ThemePalette 单源；第 6 查几何＋主按钮角色色 PASS）。
3. 幽灵清零实据（Git Bash 可直跑）：`grep -R -nE "0xF5F5F5|0x202020" src verify` 仅 ThemePalette.cs 命中（或零命中）；布局字面量审计见 D4。
4. 冻结核查（以 Phase 0 记录的 v3/main 起始 SHA 为基准，评 #19）：`git diff <v3SHA> -- baseline/ src/Program.cs src/OverlayForm.cs src/MaskPainter.cs src/MaskConfig.cs` 为空；v1 断言与第 5 查零改动（git diff 核验）。
5. 视觉验收（E2）与原始证据清单（E3）记录入 docs/VERIFY-V4.md；InputIdle ≤2000ms；install.ps1 连跑两次幂等（比较对象＝两轮后快捷方式属性回读）。
6. git：feature/style-v4 干净、提交数以 `git rev-list --count <v3SHA>..HEAD` 计 ≥4（有意义变更组织，不凑数）；E4 两步合并完成（merge 提交两父节点可验、`git diff feature/style-v4 main` 为空、main 工作树干净）。

## 7. 工件索引

- 事实源：fts skill（SKILL.md＋references×5，本机路径见文件头）；src/verify 源码（feature/frontend-v3）。
- 调研：ZCode Explore 后台代理（2026-10-19，Fluent/W3C/WinForms 三块，结论浓缩 §3–4）。
- 评审：wbx job `20261020-001201-aq4`（codex/gpt-6.1-sol，84.6s，P0×8/P1×12/P2×2 共 22 条全采纳；材料 `_probe\v4-review-material.txt`）。
- 既有探针复用：_probe\SnapSettings（浅截）、_probe\DarkProbe（深截）、_probe\DumpLayout（几何）、_probe\settings-v3-baseline.png（v3 基线对比）。

## 8. T7 评审记录（执行窗口填写）

（占位：执行窗口 Phase 1 用 wbx codex 发起批判评审，材料自包含，逐条裁决追加于此。）

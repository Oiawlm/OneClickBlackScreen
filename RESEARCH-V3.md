# RESEARCH-V3 · 一键黑屏 v3「回车换行修复 + 设置窗 UI 精修」事实源

> **一句话定位**：v3 轮的唯一事实源——回车换行 Bug 的根因定位与修复定案（B 系列）、设置窗 UI 精修定案（U 系列）、验证面演进定案（E 系列）；执行窗口按本文决策表实现。
> **来源**：源码通读（2026-10-19）+ Microsoft Learn 官方文档调研 + wbx codex 批判评审（job 20261019-183036-d0y）+ 本机 EnterProbe 实测（_probe\EnterProbe.cs）。
> **日期**：2026-10-19　行数预算：≤200 行。与 GOAL-V3-PROMPT.md 冲突时以本文为准。

## 1. 需求与冻结

**用户需求**：① 设置窗"遮罩文字"输入区标签写着"回车换行"，但实际按回车直接触发了保存（Bug），修复之；② 前端 UI 有点丑，优化之。

**冻结不变**（v1/v2 全部继承）：双击立即出遮罩；退出交互与 arm 防误触语义；settings.ini 格式语义（color=/text=，\n 转义，UTF-8）；v1 四件套断言（VerifyConfig 18 例 / VerifySettings 前 3 查 / VerifyMouseExit / VerifyOverlay A–F）逐字不动；baseline/ 一字节不改；保存语义（恢复默认/取消/保存/保存并试用四按钮功能不变）。

**可演进**：SettingsDialog 视觉与布局（含控件排布、标签文案组织）；ThemePalette 调色板值；VerifySettings 第 4 查（V2 新增项）及新增检查；verify/docs/使用说明；`src\MaskConfig.cs` 本轮**允许**改一行常量引用（见 B2），其余三冻结文件不动。

## 2. Bug 根因（证据链）

1. `src\SettingsDialog.cs:85` 标签承诺"回车换行"；`:87` `textBox.Multiline = true` 但全文件**未设置 AcceptsReturn**；`:134` `AcceptButton = saveButton`。
2. 官方文档（Microsoft Learn）：
   - TextBox.AcceptsReturn："The default is **false**… If **false**, the user must press **CTRL+ENTER** to create a new line in a multiline TextBox control."（https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.textbox.acceptsreturn）
   - Edit Control Styles（Winuser.h）ES_MULTILINE："the default response to pressing the ENTER key is to **activate the default button**. To use the ENTER key as a carriage return, use the ES_WANTRETURN style."（https://learn.microsoft.com/en-us/windows/win32/controls/edit-control-styles）
   - Form.AcceptButton Remarks：焦点控件拦截并处理 Enter 时默认按钮不被触发（多行框即此例）。
3. **本机实证**（`_probe\EnterProbe.cs`，同款 Framework csc 编译，消息级 WM_KEYDOWN 直投聚焦 EDIT——与 VerifyMouseExit 的 PostMessage 惯例同构）：
   - AcceptsReturn=false：Enter 触发默认按钮（saves=1，Bug 复现）✓
   - AcceptsReturn=true：Enter **从不**触发保存（saves=0）且插入 CRLF ✓（早前 SendKeys 轮真实注入观察到单次 Enter=单个 "\r\n"）
4. 派生问题（同一根源）：`MaxLength = MaxTextLength`（200）以 TCHAR 计，每个 CRLF 占 2 字符（EM_LIMITTEXT + EM_FMTLINES 官方口径：硬换行=一个 CR+一个 LF）。UI 承诺"换行按 1 字计"，实际带换行可输入容量 < 200，且已保存的 200 字配置无法等长重编辑——契约不一致。

## 3. UI 现状与评审结论

结构（LayOutControls 手工 SetBounds）：标题栏→颜色标签(9pt)→预览(408 宽，主屏等比高)→6 方形色块(28×28)+自定义按钮→文字标签(9pt，长文案挤满)→TextBox(408×64)→两行提示(9pt)→按钮行(4 按钮同权重 32px 高，全右置)。间距一律 8~14、边缘 16。

**真实证据边界（如实声明）**：本轮对截图的机器视觉评审**不可用**（规划窗口无图像分析工具；_probe\settings-v3-baseline.png 等 4 张截图存在但内容未经可靠视觉分析）。观感判断依据＝源码事实 + wbx codex 评审（真实返回）。视觉细节由执行窗口用 SnapSettings 截图 + 自有视觉手段目检验收（E2）。

**codex 评审裁决要点**（全部采纳，原文见 wbx job 记录）：
- 核心丑点不是"间距数值不够大"而是**分组与层级缺失**：四按钮同权重无主操作强调；"恢复默认"与保存类动作混排一行；标签长文案把约束信息挤成一行。
- 主按钮必须用**分主题 AccentText**：白字压深色 Accent #4CC2FF 对比度仅 2.01:1（WCAG 不达标）；黑字 8.12:1。ThemePalette 已有 AccentText 字段（浅=白/深=黑），直接用。
- 主按钮选**"保存"**（与 AcceptButton 语义一致），非"保存并试用"（更强动作不宜仅因按钮更长而突出）。
- 间距"统一成 12"自相矛盾（14→12 更挤）；正确做法＝组内紧/组间松的语义节奏。
- 9pt 字号保持（8.25pt 中文可读性差）；层级靠颜色（Text/TextSecondary）不靠缩字。
- **砍单**（不做，理由记录）：圆形色板（Region 无抗锯齿/DPI 重算，收益不足）、EM_SETRECT 内边距（格式矩形非持久 padding，生命周期复杂）、提示字 8.25pt、按钮高度变更（32 已合规）、预览装饰边框/焦点环（非交互元素不应暗示可点）。
- 预览高度须设上限（竖屏/副屏比例可致窗高溢出工作区）。
- Win11 圆角：官方文档称标准 WinForms 窗体自动圆角，但 FixedDialog 无 WS_THICKFRAME 是否被覆盖**未经本机证实**——执行窗口实测定案（U5 梯子）。

## 4. 调研来源清单

| 结论 | 来源 |
|---|---|
| AcceptsReturn 默认 false、Enter→默认按钮、Ctrl+Enter 换行 | learn.microsoft.com/.../textbox.acceptsreturn（含 netframework-4.8 版） |
| ES_WANTRETURN 机制原文 | learn.microsoft.com/.../edit-control-styles |
| MaxLength=EM_LIMITTEXT 按 TCHAR 计、硬换行=CR+LF | learn.microsoft.com/.../em-limittext + em-fmtlines |
| MaxLength 不约束代码赋值 Text | learn.microsoft.com/.../textboxbase.maxlength |
| Win11 spacing 节奏（按钮间 8/控件-标签 12/区块 12/边缘 16） | learn.microsoft.com/.../design/basics/content-basics |
| accent 色少量使用、强调最重要交互 | learn.microsoft.com/.../design/style/color |
| 标准 WinForms 窗体 Win11 自动圆角；属性 33=ROUND | learn.microsoft.com/.../modernize/ui/apply-rounded-corners |
| WCAG 文字对比度 4.5:1 / 非文字 3:1 | w3.org/WAI/WCAG22（Understanding contrast-minimum / non-text-contrast） |
| 开源参考（技术栈与本仓 V2 同构，不引入） | github.com/BlueMystical/Dark-Mode-Forms（195★，DWM 深色标题栏+DarkMode_Explorer+自绘）；MaterialSkin（自绘路线上限参考） |
| 社区共识（WinForms 现代化=自绘或库，库违背零依赖） | reddit.com/r/csharp/comments/16ciqrc |

## 5. 决策表（唯一实现依据）

### B 系列 · Bug 修复
| # | 决策 |
|---|---|
| B1 | 根修＝在 `textBox.Multiline = true` 旁设 `textBox.AcceptsReturn = true`（框架为此行为设计的正统属性）。**不加**任何键盘事件/ProcessDialogKey/样式直改；AcceptButton 保留（焦点不在文本框时 Enter=保存的对话框语义不变，官方 Form.AcceptButton Remarks 背书） |
| B2 | 长度契约修正＝`textBox.MaxLength = MaskConfig.MaxTextLength * 2`（原生缓冲上限：200 逻辑字 + 最坏全换行 CR-LF 展开）。业务上限仍由保存入口 `NormalizeNewlines ≤ 200` 校验兜底（既有代码不动）；超限时弹既有提示、输入保留。计数口径保持 .Length（UTF-16 code unit），不引入字素计数 |
| B3 | IME 边界＝输入法组合态 Enter 用于提交候选属正常输入行为，AcceptsReturn 不覆盖（官方键盘输入文档）；不改文案不加说明。执行窗口做一次微软拼音人工验收（组合态 Enter 不保存；提交后再 Enter 换行），结论记入 docs/VERIFY-V3.md |

### U 系列 · UI 精修（全部在既有 ApplyPalette/ThemeButton/LayOutControls 体系内融合，不自绘、不加第三方）
| # | 决策 |
|---|---|
| U1 | 主按钮＝**保存**：ThemeButton 增加 primary 角色（bool 参数即可，不造枚举体系），primary 用 `palette.Accent` 底 + `palette.AccentText` 字（浅=白 5.67:1 / 深=黑 8.12:1）；hover/pressed 用 Accent 派生色（ThemePalette 增补 AccentHover/AccentPressed 两字段，保持显式调色板模式与像素断言可验证性）。保存并试用/取消/恢复默认保持 Surface。焦点矩形与禁用态不被主色覆盖；ApplyPalette 幂等不叠加订阅 |
| U2 | 分组与阅读顺序：恢复默认左置，取消/保存并试用/保存右置，中间弹性空隙；Tab 顺序=视觉顺序（核对 Controls.Add 序）；区块标签精简为"遮罩颜色""遮罩文字"，约束提示（最多 200 字/回车换行/留空不显示）以 TextSecondary 小节紧邻输入区；LayOutControls 间距数值集中为语义常量（组内紧/组间松/边缘 16），不做全局统一 12 |
| U3 | TextBox 增高 64→80 逻辑 px；字号一律保持 9pt；层级靠 Text/TextSecondary 双色 |
| U4 | 预览：保持中性 1px 主题边框（非交互不画焦点环）；高度＝内容宽×主屏比例**但设上限**（结合所在屏 WorkingArea，保证输入区+按钮行完整可见，上限常量在布局代码定案并注释依据） |
| U5 | 窗口圆角（梯子定案，不预设）：执行窗口以屏幕级真实截图（CopyFromScreen，非 PrintWindow——后者不含 DWM 合成）目检四角。**已圆角→零改动留档；直角→把 DWMWA_WINDOW_CORNER_PREFERENCE=33(ROUND) 融入既有 ApplyFrameTheme 的 DwmSetWindowAttribute 路径**（约 2 行，与属性 20 同一函数，非补丁）。实测结论无论哪支都写入 docs/VERIFY-V3.md |
| U6 | 砍单清单（不做）：圆形色板、EM_SETRECT、8.25pt 提示字、按钮高度变更、预览装饰边框。色板选中态保持 V2 的 2px Accent 边框（像素级已验证） |

### E 系列 · 验证面演进
| # | 决策 |
|---|---|
| E1 | VerifySettings 增**第 5 查（回车行为，真实键盘路由层）**：PostMessage WM_KEYDOWN(VK_RETURN) 直投 EDIT 子控件（EnumChildWindows 定位，与 VerifyMouseExit 的 PostMessage 惯例同构；SendKeys/SendInput 因前台焦点竞态被否——EnterProbe 实录）。断言：a) EDIT 的 WM_GETTEXT 含 "\r\n" 且进程无 #32770 弹窗且 settings.ini 字节不变（Enter=换行不保存）；b) 同消息投给非 EDIT 非 Button 子控件（如 preview）后保存路径触发（#32770 出现，关掉后清理），配置按 WM_SETTEXT 预置内容写入后**原样恢复**用户配置（SnapSettings 式备份恢复）。第 5 查前 1–3 查逐字不动，第 4 查机制不变（注册表期望→采样比较的独立性保持） |
| E2 | 调色板 Bg 色值本轮**不改**（#F5F5F5/#202020 保持，第 4 查期望零扰动）；AccentHover/AccentPressed 为新增字段，不触碰既有断言 |
| E3 | 视觉验收：SnapSettings 截浅色 + DarkProbe 强制深色（定位＝深色分支渲染验证，不声称覆盖活切换全链路——措辞如实）；U5 圆角用屏幕级截图。**同一轮 dist 产物**跑 build→verify→install→截图（防测旧 exe）；C#5 + UTF-8 BOM + 零第三方依赖硬门槛 |

## 6. 验证方案

1. `build.cmd` 退出码 0，dist 三件非空。
2. `verify.cmd` 五件套全 PASS（VerifySettings 报 5 查）；v1 断言零改动（git diff 核验）。
3. E1 第 5 查双分支 PASS；B3 IME 人工验收记录。
4. 视觉验收（浅/深截图 + U5 圆角定论）记入 docs/VERIFY-V3.md。
5. `install.ps1` 连跑两次幂等。
6. InputIdle ≤2000ms（字体链回归门保持）。

## 7. 工件索引

- 探针：`_probe\EnterProbe.cs/.exe`（Enter 契约）、`_probe\corner-probe.ps1` + `corner-full/corner-zoom.png`（圆角待判材料，U5 复用）、`_probe\settings-v3-baseline.png`（v2 基线）、`_probe\v3-review-material.txt`（评审材料）。
- 评审：wbx job `20261019-183036-d0y`（codex/gpt-6.1-sol，140.5s，P0×2/P1×6/P2×9 全裁决入 §3/§5）。

# RESEARCH-V2 · 一键黑屏 v2 前端优化调研底稿

> **一句话定位**：本轮「遮罩排版打磨 + 设置窗跟随系统深浅色 + 双图标」改造的唯一事实源——现状还原、环境实测、外部调研结论与全部架构决策。
> **来源**：本地源码分析（src/，v1 已交付）+ 三路调研（ZCode 子代理官方文档核查 + wbx codex 架构批判 + wbx cn 联网社区调研，job `20261019-143503-69l`）+ 本机实测。
> **日期**：2026-10-19　行数预算：≤240 行
> 执行提示词见 [GOAL-V2-PROMPT.md](GOAL-V2-PROMPT.md)；两者冲突时以本文档为事实源。

---

## 1. 需求与硬约束（用户 2026-10-19 拍板）

| 编号 | 需求 | 性质 |
|---|---|---|
| U1 | 遮罩排版级打磨：字体族升级为中文字体链；显示/退出时序与字号公式不动 | 新增（视觉层） |
| U2 | 设置界面重设计：**跟随系统深浅色**（检测+深色标题栏+活切换），配色/字体/布局全面刷新；控件集合与行为语义冻结 | 新增（视觉层） |
| U3 | 图标：主图标「圆角屏幕+月亮」+ 设置图标「同家族+齿轮角标」，多尺寸 .ico，主图标嵌入 exe | 新增 |
| U4 | 验证体系扩展（图标/主题）+ 文档按仓库惯例更新 | 新增 |
| F1–F6 | 继承 RESEARCH-V1 全部冻结铁律（baseline/ 不改；双击立即出遮罩；退出交互与 arm 精确语义；每屏一窗；无托盘/常驻/热键；C#5+零依赖+UTF-8 BOM） | **冻结** |
| F7 | v1 验证器四件套既有断言保持全绿（设置窗有 caption、恰好一个窗体、Overlay 文字像素断言等） | **冻结** |
| F8 | 不打补丁：深度融合重构，最终代码最简洁且完整 | 方法论 |

## 2. 现状还原（src/ v1 交付，5 文件）

- **Program**：零参=遮罩（互斥体）；`--settings` 精确匹配=设置窗；其他参数=用法 MessageBox。**v2 不动**。
- **MaskConfig**：不可变配置 + ini 原子读写，损坏整份回默认。**v2 不动**。
- **MaskPainter**：共享绘制器——`DrawString`+`AntiAliasGridFit`+居中 `StringFormat`+`GraphicsUnit.Pixel` 字号（公式=area.Height/10 物理像素，同引擎测量、×0.9 收缩、下限 12px）；WCAG 自动选 #F5F5F5/#111111 文字色；字体族现用 `GenericSansSerif`（中文渲染质量差，**U1 根因**）。
- **OverlayForm+OverlayGroup**：每屏一无边框 TopMost 窗（`ShowInTaskbar=false`，无图标暴露面），arm/钩子/关闭协调。**v2 不动**。
- **SettingsDialog**：`FixedDialog`（有 caption——VerifySettings 契约）；手工 `SetBounds`+`GetDpiForWindow` 真实 DPI 缩放；`GenericSansSerif` 9pt；预览 Panel（走共享绘制器）+6 色板+自定义 ColorDialog+多行 TextBox+四按钮；无主题体系、窗体与 exe 均无图标（**U2/U3 根因**）。
- **构建/部署**：build.cmd（纯 ASCII）→ csc C#5 + build.rsp（`-win32icon` 未用）；app.config→dist\<exe>.exe.config（PMv2）；install.ps1 幂等双 .lnk（设置 .lnk 未设 IconLocation）。
- **验证**：verify.cmd 四件套。关键断言：Overlay B 在中心 1/3 找**精确等于**自动文字色的像素（换字体族后必须仍过）；Settings 断言有 caption 非遮罩窗恰一个、双实例共存、未知参数零遮罩。

## 3. 环境实测事实（2026-10-19 本机）

- Windows 11 **build 26300**（≥22000，DWM 深色属性官方支持区间）；混合 DPI 双屏（主 2560×1600@150%，副 1920×1080@125% 负坐标）。
- 注册表 `HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize` 存在可读：`AppsUseLightTheme`、`SystemUsesLightTheme`（DWORD，1=浅色/0=深色；当前双浅色）。
- 字体实测：**`Microsoft YaHei UI` 存在**（msyh.ttc；Segoe UI Variable 系亦在但官方明确不含 CJK）。
- 编译器/运行时同 v1（Framework csc，C#5 上限；.NET Framework 4.8.x）。

## 4. 外部调研结论（三路交叉验证）

**官方文档核查（ZCode 子代理）**：`DWMWA_USE_IMMERSIVE_DARK_MODE=20`（官方枚举明文，支持起点 Win11 22000；旧值 19 已从官方文档删除，社区口径仅 1809–18985 需要——本机 26300 直接用 20）。`pvAttribute` 指向 4 字节 BOOL，窗口可见前即可调用，切主题用同一属性重设 BOOL 即为官方推荐姿势。`DWMWA_BORDER_COLOR=34`（22000+）与 `CAPTION_COLOR=35`/`TEXT_COLOR=36` 存在，留作观感备选。`AppsUseLightTheme`/`SystemUsesLightTheme` 与 lParam 字符串 "ImmersiveColorSet" **均无官方文档**（社区常识+本机实测成立）；`WM_SETTINGCHANGE` 官方明确 lParam 可为 NULL、只广播顶层窗口、收到后应重读所用系统参数。ICO 结构权威规格=官方存档《Icons in Win32》(1995)；官方设计指南：尺寸上限 256、压缩条目仅建议用于 256。`WM_GETICON` 官方回退链：返回 0→`GetClassLongPtr(GCLP_HICON=-14)`→`LoadIcon`。高对比度与深浅色**官方明确是两套体系**。YaHei UI=官方指定简中 UI 字体（Win8 起预装）。csc `/win32icon` 官方对多尺寸/压缩条目**零说明**→须实测，备回退梯子。

**社区调研（cn 联网 lane）**：属性 20 需在 `OnHandleCreated`(base 后) 设置且主题切换时**必须重设**（该属性是显式设定不自动跟随）；Win11 深色窗有 1px 浅色描边（`DWMWA_BORDER_COLOR` 可治）。设置窗配色读 `AppsUseLightTheme`（`SystemUsesLightTheme` 只管任务栏/开始等外壳，两者在"自定义"模式下可不一致）。**MessageBox 恒浅色、ColorDialog 完全无暗色支持**——不要试图统一系统对话框观感。`WM_SETTINGCHANGE` 切换时连发多条且注册表可能瞬时滞后→150ms 去抖后重读。深色控件实践：TextBox 边框原生改不了色（`SetWindowTheme(h,"DarkMode_Explorer",null)` 是社区标准解，含滚动条）；Button 用 `FlatStyle.Flat`+`UseVisualStyleBackColor=false`+`FlatAppearance`；Label 用纯色非 Transparent。ICO 生成：≤128 用 32bpp DIB（biHeight 双倍、BGRA 自下而上、AND 掩码补零），256 用 PNG 条目（Vista+ 支持）；渲染必须逐尺寸重画（16px 缩放必糊）；Roslyn 历史版本对 PNG 条目有 CS7064 前科（旧版原生 csc 表现 UNKNOWN→实测）。`new Font(name,..)` 缺字体**静默替换不抛异常**，探测必须用 `new FontFamily(name)`（缺失抛 ArgumentException）。参考实现：BlueMystical/Dark-Mode-Forms（单文件 DarkModeCS.cs，MIT）、RobinPerris/DarkUI（配色表参考）。

**codex 架构批判（裁决采纳）**：①预览/色块/遮罩颜色**只来自配置**，主题仅作用于设置窗框体文字装饰；②主题与图标初始化只在 `--settings` 分支，遮罩热路径只做字体族按名解析；③`WM_SETTINGCHANGE` 不解引用 lParam——收到任意该消息即去抖重读，值变化才重应用（更简更稳）；④主题重应用=原地改色+Invalidate，不重建控件不动草稿/焦点；⑤按钮禁 owner-draw（FlatAppearance 原生够用，含 hover/press——纠正社区 lane 的过度设计）；⑥图标=构建期一次生成**产物提交入库**，默认构建消费产物，删运行期 Bitmap→HICON 路径；⑦设置窗图标走 `-resource` 托管资源内嵌（无外部文件依赖、无句柄所有权问题）；⑧字体链用 FontFamily 构造探测，测量/绘制同引擎配对不换管线（遮罩文字是大字号，GDI+ 抗锯齿足够；TextRenderer 论点仅适用小字号）；⑨验证器不得改系统设置——用"注册表期望色==窗体实采样色"断言主题链路；⑩`StringFormat` 无 LineSpacing，不搞手工字距。

## 5. 架构决策表（已定案，执行窗口照此实现；偏离须先论证并记录）

| # | 决策 | 内容与理由 |
|---|---|---|
| V1 | 主题判定 | 只读 `AppsUseLightTheme`（缺键/异常/类型不符→浅色）；`Registry.GetValue` 读取函数+设置窗缓存当前值；绘制路径零注册表 IO；标题栏与客户区同一有效主题。高对比度独立体系不特判（边界如实写入说明） |
| V2 | 深色标题栏 | `DwmSetWindowAttribute(hwnd, 20, ref int, 4)`，HRESULT 失败忽略；仅属性 20（26300≥22000，不引入 19 回退）；`OnHandleCreated`(base 后)+主题变化时**双向显式重设**（深=1/浅=0）；不引入 34（观感备选，验收若见白边再加） |
| V3 | 主题监听 | 任意 `WM_SETTINGCHANGE` → 重启 150ms 去抖 Timer → 重读注册表，值变化才 ApplyTheme；不解引用 lParam；ApplyTheme=调色板逐控件改色+DWM 属性+`Invalidate(true)`，不重建控件、不动文本/焦点/草稿 |
| V4 | 控件主题化 | 顺序：Form BackColor/ForeColor→子控件→Invalidate。Label 用调色板纯色；按钮=Flat+`UseVisualStyleBackColor=false`+FlatAppearance(MouseOver/MouseDown/BorderColor)，禁 owner-draw 与手动鼠标事件；TextBox 深色=`SetWindowTheme("DarkMode_Explorer")`+BackColor/ForeColor、浅色=还原主题与配色；色板选中态=FlatAppearance.BorderColor(accent)。**预览内容/色块色/遮罩色只来自配置，永不受主题影响**；不新增任何控件，仅重排与重绘既有控件 |
| V5 | 调色板 | 不可变 struct（Bg/Surface/Text/TextSecondary/Accent/AccentText/Border/ButtonHover 等）+静态 Light/Dark 两实例（基准：浅 #F5F5F5/#FFFFFF/#0067C0 系；深 #202020/#2B2B2B/#4CC2FF 系；微调允许，两套对称维护） |
| V6 | 遮罩字体链 | `new FontFamily(name)` 探测（缺失抛异常才可靠）：YaHei UI→YaHei→GenericSansSerif；MaskPainter 静态解析一次，设置窗共用；绘制管线（DrawString/AntiAliasGridFit/Pixel 字号/测量收缩/WCAG 取色）**逐字不动**；200 字中文/换行/中英混排回归由 Overlay B 断言覆盖 |
| V7 | 设置窗字体与缩放 | `new Font(共享族, 9f×dpiScale, Point)`（v1 模式）；`AutoScaleMode.None`；唯一缩放路径=逻辑值×dpiScale，杜绝二次缩放 |
| V8 | 图标产物与生成器 | `tools/MakeIcon.cs`（C#5，仅 System/System.Drawing，独立 csc 直编运行）一次生成 `assets\icon-main.ico`+`assets\icon-settings.ico`，**产物提交入库**；默认构建只消费产物，再生成=显式命令+人工复核差异；尺寸 16/24/32/48/64/128/256 七档：≤128=32bpp DIB（biHeight 双倍、BGRA 自下而上、AND 掩码补零）、256=PNG 条目；逐尺寸重画非缩放；母题=深色圆角屏幕+月亮（16px 可辨识），设置变体=同家族+右下齿轮角标 |
| V9 | 图标消费 | build.rsp 增 `-win32icon:assets\icon-main.ico` + `-resource:assets\icon-settings.ico,settings.ico`；设置窗 `GetManifestResourceStream("settings.ico")`→`new Icon(stream, 按当前小图标尺寸选档)`，随窗体 Dispose；`ShowIcon=true` 显式设置（FixedDialog 标题栏是否显示以实测验收，任务栏/Alt-Tab 为主要展示面）；若旧 csc 对 PNG 条目报错→回退梯子：256 改 DIB→仍报错裁掉 256 重试（实测定案并记录） |
| V10 | 构建与部署 | build.cmd（保持纯 ASCII）增 `copy assets\icon-settings.ico dist\icon-settings.ico`（ASCII 名避 cmd 编码坑）；install.ps1 设置 .lnk 增 `IconLocation=<仓库绝对路径>\dist\icon-settings.ico,0` 并纳入写后回读校验；主 .lnk 不设（默认用 exe 图标）；遮罩窗（ShowInTaskbar=false）不配图标 |
| V11 | 验证扩展 | 新增 `verify/VerifyIcon.cs`：①两 ico 结构解析（idReserved/idType/条目偏移与长度在界内/含 256/逐条目可解码）②exe 字节含 icon-main 的 256px PNG 载荷（正向证明嵌入 ours）③`ReflectionOnlyLoadFrom(exe)` 读出 `settings.ico` 资源可解码 ④dist\icon-settings.ico 存在。`VerifySettings` 增主题一致性断言：读注册表→期望调色板背景色→PrintWindow 采样设置窗客户区已知空白点==期望色（证明检测+应用链路，**不改任何系统设置**）。四件套原断言逐字不动 |
| V12 | 文档 | RESEARCH-V2（本文，§9 留执行窗口评审记录）/GOAL-V2-PROMPT/docs/VERIFY-V2.md（含图标 16/32/256 视觉验收与双主题截图）/使用说明 v3（图标、主题跟随、dist 清单增 icon-settings.ico、系统对话框不跟随的诚实边界）/README 地图；assets/ 产物入库（.gitignore 不动即不误伤） |
| V13 | 冻结重申 | v1 全部铁律继承；OverlayForm/OverlayGroup/Program/MaskConfig 零改动；换字体后 Overlay B 文字像素断言必须原样通过（U1 回归门）；设置窗控件集合与保存语义不变 |
| V14 | 分支与流程 | `feature/frontend-v2` 分支；Phase 结构同 v1（勘察→评审→实现→构建验证→安装文档→终验） |

## 6. 验证方案（延续验证文化，全部自动化）

1. `VerifyIcon`（新）：ico 结构/嵌入字节正向匹配/托管资源解码/dist 产物四查（V11）。
2. `VerifySettings`（扩展）：v1 三查原样 + 主题一致性像素断言。
3. `VerifyConfig`/`VerifyMouseExit`/`VerifyOverlay`：原样全绿（Overlay A–F 是 U1 的回归门）。
4. `install.ps1`：幂等×2 + 双 .lnk 目标/参数/IconLocation 回读校验。
5. 人工验收（记入 docs/VERIFY-V2.md）：双主题下设置窗观感、预览隔离（主题不串色）、图标 16/32/256 清晰度、色板选中态、深浅活切换不丢输入焦点。

## 7. 风险与边界（如实写入使用说明）

MessageBox/ColorDialog 系统对话框**不跟随**深浅色（系统限制，接受混搭）；高对比度为独立体系不特判；FixedDialog 标题栏图标显示与否以实测为准（任务栏/Alt-Tab 必有）；旧 csc 对 PNG 图标条目兼容性 UNKNOWN（有回退梯子）；首次出遮罩时序零变化（InputIdle≤2000ms 门保持）；深浅切换瞬时注册表滞后由去抖吸收。

## 8. 参考链接与调研留痕

- 官方：DWMWINDOWATTRIBUTE（=20/34）/DwmSetWindowAttribute/WM_SETTINGCHANGE/《Icons in Win32》存档/vis-icons（256 上限）/WM_GETICON/GetClassLongPtr/字体清单（YaHei UI=简中 UI 字体）/typography（Segoe UI Variable 不含 CJK）——均在 learn.microsoft.com。
- 社区：github.com/BlueMystical/Dark-Mode-Forms · github.com/RobinPerris/DarkUI · en.wikipedia.org/wiki/ICO_(file_format)。
- 留痕：wbx job `20261019-143503-69l`（codex 批判全文 + cn 调研全文，`C:\Users\Lenovo\.wbx\jobs\20261019-143503-69l\`）；ZCode 子代理官方核查报告（本会话，结论已并入 §4）。

## 9. T7 评审记录（执行窗口，2026-10-19）

来源：wbx codex lane（gpt-6.1-sol）批判评审，job `20261019-150846-b9u`，材料 `_probe/t7v2-material.txt`（冻结清单+V1–V14+文件级实现计划+src 关键源码）。21 条意见（P0×2/P1×11/P2×8），逐条裁决：

| # | 裁决 | 修订要点 |
|---|---|---|
| 1 | 采纳 | AppsUseLightTheme 读取实现：`object v=Registry.GetValue(...); return v is int ? (int)v!=0 : true;`（材料注释里 `as int? ?? true` 系笔误，不可编译） |
| 2 | 采纳 | `Registry.GetValue` 根名必须用全称 `HKEY_CURRENT_USER\...`（缩写 "HKCU" 会抛异常被吞→恒浅色，恰被当前浅色环境掩盖）；VerifySettings 内同款独立实现，不链 src |
| 3 | 采纳 | 回退梯子与验证器断言绑定：仅当 csc 实测拒 PNG 条目才启用梯子；启用时 VerifyIcon 查①②随产物同轮修订并记录（勘误+VERIFY-V2 定案），不允许"产物变了断言没变" |
| 4 | 采纳+实测定案 | **V7 勘误**：本机实测（`_probe/FontProbe.cs`，PMv2 进程，主屏 144 DPI）：`GraphicsUnit.Point` 已按窗口 DPI 换算——9pt→行高 23px（em 18px），9pt×1.5=13.5pt→行高 35px（em 27px）＝双重缩放偏大 50%（v1 即此问题，35px 文字挤在 30px 标签里）。**设置窗字体定案 `new Font(共享族, 9f, Point)`（不乘 dpiScale）；布局 bounds 仍 ×dpiScale** |
| 5 | 采纳 | 构造期仅纯配色（不碰 Handle）；DWM 属性 20 与 SetWindowTheme 只在句柄存在路径调用（OnHandleCreated/主题切换） |
| 6 | 采纳 | TextBox 主题经其 `HandleCreated` 事件订阅应用（首次创建+句柄重建均覆盖；父 OnHandleCreated 时子句柄尚未建，WinForms 先父 OnHandleCreated 后级联子 CreateControl）；ApplyTheme 直调处守卫 IsHandleCreated |
| 7 | 采纳 | 主题去抖用 `System.Windows.Forms.Timer`（UI 线程）；`Dispose(bool)` 统一 Stop+Dispose；WndProc 触发前查 `themeTimer.IsDisposed`，Tick 首行 Stop+查窗体 Disposing |
| 8 | 采纳 | 浅色还原 TextBox 主题 = `SetWindowTheme(h, null, null)`（恢复默认关联，非显式 "Explorer"）；P/Invoke 声明 `CharSet.Unicode` |
| 9 | 采纳 | 色板 FlatAppearance.MouseOver/MouseDown 设为色板本色（配置候选色，悬停不变色）；`UpdateSwatchSelection()` 统一供构造初态/色板点击/自定义颜色/恢复默认四入口调用 |
| 10 | 采纳 | 预览 1px 边框画在 `PreviewPanel.OnPaint` 内部（base→DrawText→边框），不依赖外部 Paint 事件触发时序 |
| 11 | 采纳 | Overlay B 精确像素门为 U1 实证回归门：必须原样跑过，不放宽容差、不以"常量未改"替代 |
| 12 | 采纳 | 删除"零开销/恰在首次 DrawText 初始化"绝对表述；以 InputIdle 实测+A 场景像素断言为准，报告实测值 |
| 13 | 部分采纳 | 图标选档维持 `SystemInformation.SmallIconSize`（V9 原文口径）；七档含 16/24/32，最近档足够；125%/150% 双屏标题栏与任务栏图标目检纳入 VERIFY-V2；不引入 GetSystemMetricsForDpi（决策表外机制） |
| 14 | 采纳 | 窗体 Icon 存字段、`Dispose(bool)` 释放；加载失败置 null 静默（VerifyIcon 已证资源可解码，运行期失败由目检兜底） |
| 15 | 采纳 | MakeIcon DIB 复制用显式行指针 `Scan0 + y*Stride` 逐行取（不假设 Stride 符号/对齐）；母题不对称（月牙偏上+齿轮右下），PNG 预览目检即行向证明 |
| 16 | 采纳 | 查①强化：DIB 条目完整解码（BGRA→ARGB+行序翻转）为位图，要求存在 alpha>0 可见像素与非零彩色像素；PNG 条目解码尺寸==声明尺寸 |
| 17 | 采纳 | 结构检查补全：宽高字节 ∈ {16,24,32,48,64,128,256}（256→0）且唯一、planes==1、bitCount==32、compression==0、DIB 长度==40+4wh+((w+31)/32)·4·h（long 运算防溢出）、条目互不重叠、offset 单调 |
| 18 | 采纳 | 查②结论限定"exe 内嵌 256px PNG 载荷字节"；查③增加"托管资源字节==assets\icon-settings.ico 全量比对"（嵌入完整性）后再 Icon 解码 |
| 19 | 采纳 | VerifySettings 像素断言比较 RGB（`ToArgb()&0xFFFFFF`），目标位图预填 Magenta 证缓冲确被绘制；检查 PrintWindow 返回值 |
| 20 | 采纳 | 采样前窗口尺寸两次读取（间隔 100ms）稳定才采样；断言命名与 PASS 文案如实为"窗体背景色跟随 AppsUseLightTheme"；标题栏/按钮观感归人工验收（VERIFY-V2） |
| 21 | 采纳 | build.rsp 增列 `src\ThemePalette.cs`；`struct`+readonly 字段（C#5 无 readonly struct）；全部新文件 UTF-8 BOM 核查 |

> 净变更：V7 字体公式勘误（#4，实测证据）；主题应用拆"纯配色（构造期）/句柄相关（OnHandleCreated+TextBox.HandleCreated+切换）"两段（#5/#6/#7）；TextBox 浅色还原改 null,null（#8）；色板四入口选中态+本色 hover（#9）；VerifyIcon 断言全面收紧（#16/#17/#18）；VerifySettings 改 RGB 比较+稳定门（#19/#20）；其余为表述纠偏。

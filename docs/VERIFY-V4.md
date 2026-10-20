# VERIFY-V4 · 一键黑屏 v4 验证记录（fts 风格体系统一）

> 日期：2026-10-20　分支：feature/style-v4（基于 feature/frontend-v3 b7bae20，已融合 main 79c60bd 规划提交，merge 2eaa3df）　环境：Win11 26300，混合 DPI 双屏（DISPLAY1 2560×1600 @150%＝144DPI 主屏 16:10；DISPLAY2 1920×1080 @125%＝120DPI 副屏，位于主屏左侧）
> 事实源：[RESEARCH-V4.md](../RESEARCH-V4.md)（§5 决策表 S/D/E、§8 评审 22 条裁决）。产物：dist 同一轮构建。

## 1. 构建与五件套验证（同一轮 dist）

命令（仓库根）与退出码（原始日志落盘 `_probe\`）：

```
cmd /c build.cmd   → [OK] Build succeeded   exit 0   日志 _probe\verify-v4-build.log
cmd /c verify.cmd  → [OK] ALL VERIFIERS PASSED  exit 0   日志 _probe\verify-v4-five.log
powershell -ExecutionPolicy Bypass -File install.ps1  ×2  exit 0 / exit 0  幂等   _probe\verify-v4-install1.log / install2.log
```

五件套摘要（完整输出见 `_probe\verify-v4-five.log`）：

| 验证器 | 结果 |
|---|---|
| [1/5] VerifyConfig | PASS：18 config tests |
| [2/5] VerifySettings | PASS：**6 查**——原 3 查＋第 4 查主题背景（浅色 #F5F5F5，期望值现读 ThemePalette 单源＋采样条带与全部可见子控件不相交）＋第 5 查回车行为双分支（逐字零改动）＋**第 6 查几何镜像**（PMv2 窗口上下文核验、17 直系子控件复算 ±1px 全 PASS、ClientSize ±1、窗口⊆WorkingArea、EDIT 字体锁 body 档 em=21@144、主按钮四角内缩采样==ThemePalette.Accent #0067C0） |
| [3/5] VerifyMouseExit | PASS：5/5 buttons；60 moves；10 wheel |
| [4/5] VerifyOverlay | PASS：A–F；2 displays；max InputIdle **471 ms**（限 2000） |
| [5/5] VerifyIcon | PASS：4 icon checks（256px PNG payload offset 238368；dist ico 110145 B 一致） |

install.ps1 双跑：两次均 `[OK]`，回读完全一致——`一键黑屏.lnk → dist\一键黑屏.exe（Icon 空）`、`一键黑屏-设置.lnk → --settings（Icon='D:\...\dist\icon-settings.ico,0' 精确回读）`；两轮日志逐行 diff 相同（INSTALL-LOGS-IDENTICAL）。比较对象＝两轮快捷方式属性回读（v4 与 v2/v3 同口径）。

## 2. 原始证据清单（E3）

| 证据 | 内容与落点 |
|---|---|
| build 命令原文/退出码 | `cmd /c build.cmd` → 0；`_probe\verify-v4-build.log` |
| verify 命令原文/退出码 | `cmd /c verify.cmd` → 0；`_probe\verify-v4-five.log`（五件套全 PASS，Settings 报 6 查） |
| install×2 | 见 §1；`_probe\verify-v4-install1.log`/`install2.log`，diff 一致 |
| exe SHA-256 | `211c760ea1a2c03c286103c02ba4e53ebade8b7e8660d8d42470486c15dc9e07`（dist\一键黑屏.exe，244736 B，2026-10-20 09:30 构建；.exe.config 245 B；icon-settings.ico 110145 B 三件非空） |
| 浅色截图 | `_probe\v4-light-final.png`（SnapSettings 真 exe PrintWindow 666×965 全窗口，主屏 150%，系统浅色主题） |
| 深色截图 | `_probe\v4-dark-final.png`（DarkProbe 反射强制 ApplyTheme(false)，主屏 150%，不触碰系统主题） |
| 125% 副屏几何 | `_probe\verify-v4-dpi125-dump.txt`（DumpLayoutDpi place 模式，DISPLAY2@120DPI 进程内 Manual 定位；真 exe 的 CenterScreen 经 CW_USEDEFAULT 恒落主屏——平台事实，见 §7） |
| 150% 主屏几何 | 第 6 查断言本身（exit 0）＋ DumpLayoutDpi 运行输出（client 660×925 与复算逐项一致，含 112.5→112 / 382.5→382 银行家舍入实证） |
| InputIdle 原值 | 471 ms（`_probe\verify-v4-five.log` VerifyOverlay 行） |
| 设计期实测 | MeasureV4 探针 96DPI：按钮文本 65/37/79/37/90、PreferredSize 77/75/91/75（75=flat 逻辑最小宽）、hint 203、Font.Height body18/caption16；144DPI 对照值同探针 |
| 幽灵清零 | `grep -R -nE "0xF5F5F5\|0x202020" src verify` → **零命中**（ThemePalette 以 FromArgb(0xF5,0xF5,0xF5) 字节三段式声明，紧凑十六进制副本全仓不存在） |
| 布局字面量 | LayOutControls 方法体 `S(<数字>)` 内联字面量 **0** 处，SettingsDialogMetrics 引用 27 处（grep 审计） |
| 冻结核查 | `git diff b7bae20 -- baseline/ src/Program.cs src/OverlayForm.cs src/MaskPainter.cs src/MaskConfig.cs` → **空**（0 字节）；VerifyConfig/VerifyMouseExit/VerifyOverlay/VerifyIcon 同基准 diff 空（v1 断言零改动）；VerifySettings 删除行仅第 4 查内部（D2 授权）＋文件头注释＋汇总行 5→6（E1 授权），第 1–3/5 查逐字零改动 |
| BOM | src+verify 全部 12 个 .cs 首三字节 ef bb bf |
| 配置恢复 | SnapSettings/VerifySettings 均字节级备份恢复用户 settings.ini（验证器 finally 全覆盖） |

## 3. 双主题双 DPI 目检（机器视觉逐项，材料=§2 截图）

**浅色（v4-light-final.png）**：① "遮罩颜色/遮罩文字"区块标签**粗体**且明显大于灰色提示（两档字阶+S1 Bold 落地）；② 右上提示"最多 200 字 · 回车换行 · 留空不显示"完整一行右对齐、无截断；③ 四按钮文字完整无重叠，**保存=蓝底白字**（#0067C0/白 5.67:1）；④ 六色块与"自定义颜色…"按钮同行等高、中心线对齐、节距均匀；⑤ 输入框文字清晰；⑥ 全图无截断/重叠/错位。
**深色（v4-dark-final.png）**：① 深灰底+浅字；② 区块标签白粗体、层级清楚；③ **保存=亮蓝底黑字**（#4CC2FF/黑 10.5:1），其余按钮深灰底浅字；④ 提示一行完整；⑤ 输入框深底浅字；⑥ 无对比度不足元素。
**与 v3 基线对比**（_probe\settings-v3-baseline.png）：v3 全窗单档 9pt、标签不加粗；v4 正文 10.5pt+区块标签 Bold+提示 9pt 两档成立，文本框 5 行盒（v3 4 行）、行距 12（v3 10）、按钮加宽（104/80/116/80），整体节奏"组内紧组间松"维持且更舒展。DarkProbe 定位＝深色分支渲染验证（v3 口径沿用，不声称覆盖活切换全链路）。

## 4. fts 质量验收清单（适配版逐项）

- ☑ 数值唯一声明点：色=ThemePalette（11 键双主题同键位）、布局=SettingsDialogMetrics（每值一行推导）；UI 层零第二字面量（§2 grep/字面量审计）。
- ☑ 镜像锁定测试：第 6 查锁"渲染几何==Metrics 复算"＋"主按钮==ThemePalette.Accent"；第 4 查锁"背景==ThemePalette.Bg"（共编单源）。
- ☑ 圆角两档：窗口级=DWM 系统 8（v3 U5 实证零改动）；控件级 4=WinForms 无 AA 路径→**登记平台不可达例外**，不造幽灵 token（S4）。
- ☑ 时长档位：本项目**零自定义动效**（150ms 去抖=时序门闩非动画；S5 登记）。
- ☑ 两条缓动/阴影分层：WinForms GDI 无 Effect/阴影面→不适用（映射定论，RESEARCH-V4 §3）。
- ☑ 恒定几何：FixedDialog 手工布局恒定；swatch hover/press 不换色（候选色恒定）；ApplyPalette 原位重刷不重排。
- ☑ 双主题同键位＋动态跟随：WM_SETTINGCHANGE 150ms 去抖→ApplyPalette 原位换色（v3 已验，v4 不触碰机制）。
- ☑ 关键路径真实输入验证：第 5 查真实消息层（AttachThreadInput+SetFocus+投递）；VerifyMouseExit/Overlay 全真消息（v1 冻结断言）。
- ☑ 反例库 20 条过一遍：§5 对照表，无未处置命中。
- ☑ 浅/深双主题截图对照：§3 通过。

## 5. fts 反例库 20 条对照表（A1–A20）

| # | 反例（现象→正解） | 本项目裁决 |
|---|---|---|
| A1 | 幽灵 token（第二数值源）→删＋锁定测试 | **曾命中（验证器 0xF5F5F5/0x202020 副本）→D2 根修**：期望值改读 ThemePalette 共编，grep 零命中 |
| A2 | 双口径量化失配→单一几何源 | 不命中（无网格量化；布局单源 Metrics+唯一 S()） |
| A3 | 滚动条占布局宽→overlay | 不命中（WinForms 无此模板面；textBox 滚动条为原生内嵌） |
| A4 | 稳定留槽 | 不命中 |
| A5 | 宽度补偿/负 margin 特判 | 不命中（手工布局直角坐标） |
| A6 | 弹窗假圆角三件套→不透明单壳+系统圆角 | 已合规（FixedDialog 不透明+DWM 自动圆角=v3 U5） |
| A7 | 内容列 Center 收缩→Stretch | 不命中（固定 408 列宽） |
| A8 | 内缩量动态派生→静态 | 不命中（全部静态值） |
| A9 | 悬浮覆盖替代静态内缩 | 不命中 |
| A10 | UniformGrid/WrapPanel 按钮区→显式格子 | 不命中（按钮行手工 SetBounds 恒定） |
| A11 | Effect 进文字子树→阴影隔离 | 不适用（GDI 无 Effect；无阴影） |
| A12 | 条件性外形越屏→删机制恒在工作区 | **反向吸收**：v4 居中改 WorkingArea（裁决 #5），第 6 查锁窗口⊆WorkingArea |
| A13 | 分层透明壳→WindowChrome+DWM | 不适用（不透明 GDI 窗） |
| A14 | 纯 Opacity 隐藏→布局塌缩 | 不命中（无动态显隐面） |
| A15 | 快照画刷→动态资源引用 | 不适用（WinForms 原位重刷=动态等价，v3 已验） |
| A16 | 场景新增档位→锚定既有档 | **曾命中（间距/字号无档位登记）→S1/S2+D1 根修**：4px 网格四档+字阶两档全量登记 Metrics |
| A17 | 模式化交互→常态分区手势 | 不命中 |
| A18 | 拖拽追赶动画→直接赋值 | 不命中（无拖拽面） |
| A19 | 交互上限由属性声明→消息层强制 | 不命中（FixedDialog 天然不可缩放；无 MaxWidth 面） |
| A20 | 覆盖层就地编辑/幽灵入口→结构对齐 | 不命中（无就地编辑面） |

## 6. Metrics 推导表（SettingsDialogMetrics，96DPI 逻辑 px）

| 值 | 定值 | 推导（MeasureV4 设计期实测＝_probe\MeasureV4） |
|---|---|---|
| EdgePx | 16 | Win11 内容基础边距；4px 网格档 16 |
| LabelGapPx / ButtonGapPx / CustomButtonGapPx | 8 | 网格档 8：标签→内容、按钮簇内、末色块→自定义钮（与块间同档，行内节奏统一） |
| RowGapPx | 12 | 网格档 12（v3=10 归档；S2） |
| GroupGapPx | 20 | 网格档 20（v3 保持） |
| BodyPointSize / CaptionPointSize | 10.5pt / 9pt | Fluent Body=96DPI 14px / Caption=12px；Point 单位自带窗口 DPI 缩放（S1） |
| LabelRowHeightPx | 24 | body Font.Height=18＋呼吸余量（v3 的 20 是 9pt 时代值） |
| ContentWidthPx | 408 | v1 起内容列宽＝按钮行预算 |
| HintZoneWidthPx | 220 | 提示 9pt 实测 203 一行＋余量；区块标签行余 408−220−8=180（bold"遮罩文字"≈62，不相交） |
| SwatchSizePx / SwatchPitchPx | 28 / 36 | v1 色块；节距=28+8（8 档） |
| SwatchRowHeightPx | 34 | 行盒=自定义钮高（body 18+按钮 chrome） |
| CustomButtonWidthPx | 116 | 文本 90+chrome 8（PreferredSize 102）＋余量 |
| CustomButtonLiftPx | 3 | (34−28)/2，色块与自定义钮中心线对齐（矩形推导非拍脑袋） |
| ButtonRowHeightPx | 32 | Fluent 默认控件高（零改动） |
| TextBoxHeightPx | 96 | 5×body 行盒 18（v3 的 80 是 9pt 时代≈5 行） |
| HintHeightPx | 40 | 2×caption 行盒 16＋内边距 |
| 按钮宽 Reset/Cancel/Try/Save | 104/80/116/80 | 文本 65/37/79/37＋chrome 8（PreferredSize 77/75/91/75；75=flat 逻辑最小宽）＋余量；**行总宽 104+80+116+80+3×8=404≤408（余 4）** |
| PreviewFloorPx | 60 | v1 预览地板（S2：预算不足压缩至地板如实报错） |

## 7. 平台事实与锁定边界登记（如实留档）

- **自绘控件原生字体 NULL**（FontDump 实证）：WinForms UserPaint 控件（Label/Button/窗体）不落 WM_SETFONT，WM_GETFONT=0；仅原生 EDIT 权威。第 6 查字体断言锁 EDIT（body 档 em=BodyPointSize×dpi/72±1＝21@144 实测＋YaHei 链＋非 Bold）；区块标签 Bold 与 caption 档以源码（Metrics 常量三处 new Font 单点派生）＋FontDump 探针＋E2 截图覆盖。
- **CenterScreen 恒落主屏**（CW_USEDEFAULT 语义）：真 exe 设置窗无论光标/启动位置均开主屏；125% 副屏几何由进程内 Manual 定位探针实测（verify-v4-dpi125-dump.txt，全部子控件=逐项 S() 精确复算，含 42.5→42 银行家舍入）。跨屏移动不重排=v1 登记语义维持。
- **125% 进程内 client 总高 +5px**（776 vs 复算 771）：WinForms ClientSize 在 1.25 缩放下的边框舍入；子控件坐标零漂移；生产不可达（设置窗不在副屏创建），登记不改码。
- **镜像锁定边界**（评 #16/裁决 #14）：共编锁定"渲染==当前源值"，不证源值设计正确性；值正确性由本文件 §3 目检＋§6 推导表＋评审把关。六态其余态（hover/pressed/focus/disabled）与深色档像素断言登记为不可达例外（E2 截图+源码审查覆盖——ThemeButton 六态赋值源码直读）。
- **副屏自动化覆盖边界**：第 6 查覆盖启动屏（=主屏）；副屏几何以探针证据覆盖，不把验证器搬进副屏（裁决 #22）。
- 最低支持工作区高≈S(fixedChrome)+nonClient+S(60)（96DPI 下≈468px）；低于此为不支持环境，containment 将如实 FAIL（S2 策略）。

## 8. 冻结与纪律核查

- 冻结 diff 空（§2 表）；v1 四件套断言文件 diff 空；第 1–3/5 查逐字零改动（删除行审计仅授权面）。
- src/ 零第三方引用（build.rsp 仅系统引用）；C#5（csc -langversion:5 编译通过即证）；全部 .cs UTF-8 BOM。
- settings.ini 格式/退出交互/四按钮功能：VerifyConfig+VerifyOverlay+VerifyMouseExit+第 5 查全绿覆盖。
- ThemePalette 结构零改动（S3 维持 Win11 蓝 11 键）；窗口圆角/动效零改动（S4/S5 登记）。

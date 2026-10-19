# RESEARCH-V1 · 一键黑屏 v1 自定义化调研底稿

> **一句话定位**：本轮「遮罩颜色/文字可自定义 + 设置入口」改造的唯一事实源——现状还原、环境实测、外部调研结论与全部架构决策。
> **来源**：本地源码分析（baseline/）+ wbx 双 lane 外部调研（job `20261019-033502-5zf`：overlay-research@cn 联网调研 + arch-critique@codex 架构批判）+ 本机实测。
> **日期**：2026-10-19　行数预算：≤240 行
> 执行提示词见 [GOAL-V1-PROMPT.md](GOAL-V1-PROMPT.md)；两者冲突时以本文档为事实源。

---

## 1. 需求与硬约束

| 编号 | 需求 | 性质 |
|---|---|---|
| R1 | 遮罩颜色可自定义 | 新增 |
| R2 | 遮罩上可显示自定义文字 | 新增 |
| R3 | 有设置入口，可调整 R1/R2 | 新增 |
| R4 | 双击 → 遮罩立即出现，绝不先弹设置界面 | **冻结** |
| R5 | 退出交互完全保留（见 §3 冻结语义） | **冻结** |
| R6 | 不打补丁：深度融合重构，最终代码最简洁且完整 | 方法论 |
| R7 | 本仓库 = 该软件的修改仓库（版本化、可追溯） | 载体 |

## 2. 现状还原（baseline/BlackScreen.cs，131 行，冻结不改）

单文件 C# WinForms 程序，csc 编译为 7KB winexe。行为清单：

- **窗口**：一个无边框 `FormBorderStyle.None` 窗体，`Bounds = SystemInformation.VirtualScreen`（一次快照），TopMost，不进任务栏，PerMonitorV2 DPI manifest（baseline/BlackScreen.manifest，asInvoker）。
- **退出**：低级键盘钩子（`SetWindowsHookEx(13, …, GetModuleHandle(null), 0)`）吞掉 WM_KEYDOWN/WM_SYSKEYDOWN 并退出（前后台都生效）；`OnMouseUp` 点击退出；滚轮/移动不退出。
- **arm 防误触（精确语义，禁止重设计）**：100ms 间隔 `Timer` 轮询 `GetAsyncKeyState(1..254)`，**所有虚拟键全部处于释放态的那一次 Tick 才置 armed**；armed 前 keydown 不吞、MouseUp 不退出；它是采样不是"连续安静 100ms"保证；持续按住会一直阻止 arm。
- **鼠标隐藏**：WndProc 截 `WM_SETCURSOR(0x0020)` → `SetCursor(IntPtr.Zero)` 并返回 1；关闭时若光标在窗体内恢复默认光标。
- **关闭路径**：keydown 经 `BeginInvoke(Close)`；OnFormClosed 停 Timer、卸钩、恢复光标（清理无幂等兜底——v2 需统一关闭入口）。
- **初始化顺序**：`EnableVisualStyles` → `SetCompatibleTextRenderingDefault(false)` → `new BlackScreen()` → `Application.Run`；`OnShown` 里 `Activate()` + 装钩 + 启动 armTimer + 藏光标；钩子失败 → MessageBox + Close。
- **验证器**（延续其验证文化）：`VerifyBlackScreen.cs`（自动启动→断言单可见窗体、边界==VirtualScreen、TopMost、无边框、每屏 25 采样点全黑、3 个键的前台+后台键盘退出、退出码 0）；`VerifyMouseExit.cs`（鼠标退出语义）。
- **文档**：baseline/使用说明.txt（"只覆盖画面，不关电源，不联网，删除即移除"）。

> **勘误（2026-10-19，执行窗口 Phase 0 子代理逐条核对，以源码为准）**：
> ① 条5"清理无幂等兜底"应精确为——卸钩有判空守卫（`keyboardHook != IntPtr.Zero` 且卸后置 Zero），`closing` 标志防 keydown/MouseUp 双触发；唯 `armTimer.Stop()/Dispose()` 无守卫，OnFormClosed 二次触发会抛异常。
> ② 条6 钩子失败路径实际顺序是先 `Close()` 再 `MessageBox.Show`（§2 原文顺序颠倒）。
> ③ 条7（及 §6.1 对应表述）"3 键前台+后台"不实：后台（非前台）钩子验证仅 F24 一轮执行（VerifyBlackScreen.cs L91-96），Escape/Shift 仅前台按键验证。v2 `VerifyOverlay` 将 3 键均覆盖前台+后台两种情形。

## 3. 环境实测事实（2026-10-19）

- 编译器：仅 `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`（**C# 5 方言上限**）；无 .NET SDK、无 Roslyn。
- 运行时：.NET Framework 4.8.x（NDP v4 Full Release=0x82718）。
- 显示器：**混合 DPI 双屏**——DISPLAY1 主屏 2560×1600@150%（逻辑 1707×1067），DISPLAY2 副屏 1920×1080@125%（逻辑 1536×864，位于负坐标）。VirtualScreen `{X=-1536,Y=0,W=3243,H=1078}` 含屏间空隙。
- 快捷方式：`C:\Users\Lenovo\DeskBox\DeskBox\一键黑屏.lnk` → `C:\Users\Lenovo\Documents\Codex\2026-09-07\gei\outputs\一键黑屏.exe`（旧产物，保留不动作回退）。

## 4. 外部调研结论（wbx 双 lane，交叉验证）

**lane A（cn 联网调研，样本 10+ 项目）**：同类工具覆盖手段均为 topmost 无边框窗口（无断电方案，社区共识"只覆盖不改系统"）；设置入口在 Stonyx/BlackScreen、dhr412/privacyscreen 中均为**命令行参数**，BlueBird67/BlackScreens 用常驻托盘+热键（代价：失去"用完即退"）；配置持久化 XML→`%APPDATA%`（BlackScreens）或 JSON→ProgramData（DimmerBeyond）；**每屏一窗**是 DimmerBeyond/BlackScreens/Stonyx 共同拓扑；CinemaDrape 实测 bug：`SystemInformation.VirtualScreen` 导致覆盖不全，社区建议按各屏矩形处理；文字渲染 TextRenderer 走 ClearType 更锐，layered window 上只能 GDI+；WCAG 相对亮度优于 YIQ 一刀切。

**lane B（codex 架构批判，修正本方案 3 处）**：①同步读配置不能承诺"绝不阻塞"——如实表述为"读取有大小上限的小文件，接受其耗时"；②arm 语义必须原样保留，不得顺手改成另一套防误触机制；③未知参数**绝不能静默落入遮罩模式**（用户以为开设置却突然黑屏）。另给出互斥体正确姿势（`WaitOne(0)` 非阻塞、`AbandonedMutexException`=已取得、仅拥有者 Release、`Application.Run` 后外层 finally 释放、`Local\` 作用域）、统一关闭入口 + 幂等清理、保存失败必须在设置窗口明示、原子写需同目录唯一临时文件。

**两 lane 分歧与裁决**：
- *配置格式*：A 推 key=value 纯文本，B 推现成 JSON 解析器（手写 parser 被双方一致否决）。**裁决：key=value**——两字段规模下最简（`File.ReadAllLines` + `IndexOf('=')`，零依赖零序列化器 JIT），A 的理由在 csc 直编场景权重更高。
- *窗口拓扑*：A 推每屏一窗（混合 DPI 单 HWND 必被 DWM 位图拉伸，微软官方文档口径；每屏窗使 `DeviceDpi` 字号缩放天然正确），B 认为按物理像素绘制单窗也可接受、拆窗增加协调成本。**裁决：每屏一窗**——本机实测就是混合 DPI 双屏（§3），文字是新功能核心，拓扑必须为特性服务；协调成本由统一入口/共享状态化解（§5-D7）。

## 5. 架构决策表（已定案，执行窗口照此实现；实现中如需偏离，须先论证并记录）

| # | 决策 | 内容与理由 |
|---|---|---|
| D1 | 同 exe 双模式 | `Main` 首行按参数分流：零参数=遮罩；`--settings` 精确匹配=设置；**其他参数=MessageBox 提示用法后退出，绝不进遮罩**。设置窗体类型在另一分支，JIT 不碰热路径；单产物、单代码库 |
| D2 | 每显示器一个遮罩窗体 | 每窗 `Bounds=该屏 Bounds`（支持负坐标），逐屏 TopMost；`DeviceDpi/96f` 缩放像素字号，混合 DPI 下每屏文字都原生清晰；顺带消除 VirtualScreen 覆盖不全的潜在 bug |
| D3 | 配置：key=value 文本 | 路径 `%LOCALAPPDATA%\BlackScreen\settings.ini`；仅两键：`color=#RRGGBB`（只接受不透明 RGB）、`text=...`（≤200 字符，字面 `\n`=换行、`\\`=反斜杠）；未知键忽略；缺文件/损坏/超限（读取上限 64KB）→ **静默回落默认值（纯黑+无文字=现状行为）**；只在设置保存时写（不首次自动建文件），同目录唯一临时文件+替换原子写 |
| D4 | 自动对比度文字色 | WCAG 相对亮度（sRGB→线性）算黑/白对比度取大者；自动色用 #F5F5F5/#111111（防纯极端对比光晕）；不提供手动文字色（设置面保持用户所要的两项） |
| D5 | 文字绘制 | `Graphics.DrawString` + `TextRenderingHint.AntiAliasGridFit` + `StringFormat` 居中 + `GraphicsUnit.Pixel` 字号（基准≈屏高 1/10，可微调）；测量与绘制同引擎同参数；Font/Brush 用完即 Dispose；双缓冲三件套样式保持，默认 OnPaintBackground 填背景、OnPaint 只画文字 |
| D6 | 共享绘制器 | 遮罩窗体与设置窗预览**共用同一静态绘制方法**（同一布局+取色规则），预览即所见即所得 |
| D7 | 类型划分 | `Program`（分流/互斥体/配置加载）/ `MaskConfig`（默认值/规范化/读写）/ `OverlayForm`（遮罩窗，arm+钩子+退出语义冻结继承）/ `SettingsDialog`（编辑副本/色板+ColorDialog/文字框/预览/保存/取消/恢复默认/保存并试用）+ MaskPainter 静态绘制（并入任一文件或独立文件均可）；文件拆分从简 |
| D8 | 单实例 | 仅遮罩模式：`Local\` 命名互斥体，`WaitOne(0)`，占用即静默退出；`AbandonedMutexException` 视为取得；仅在拥有时 Release，`Application.Run` 返回后 finally 兜底；设置模式不受限 |
| D9 | 配置快照语义 | 遮罩启动时读一次配置传构造函数；设置保存**下次启动生效**；明确不做文件监听/IPC/实时刷新 |
| D10 | 生命周期 | 分流 → （遮罩模式）互斥体 → 同步读配置（带上限）→ 建窗；`SetCompatibleTextRenderingDefault` 在两模式任何窗体前调用；Main 同步读可接受，不承诺亚毫秒 |
| D11 | 构建与安装 | `build.cmd` 一键调 csc（`-target:winexe -optimize+ -win32manifest:baseline\BlackScreen.manifest`，引用仅 System/System.Drawing/System.Windows.Forms 等框架程序集，**零第三方依赖**）；`install.ps1` 幂等同步 DeskBox 双快捷方式（主=无参、设置=`--settings`），目标=仓库 `dist\一键黑屏.exe`；**源码文件必须 UTF-8 带 BOM**（csc 无 BOM 按 GBK 读，中文字面量会乱码） |
| D12 | 冻结清单 | baseline/ 不改；退出交互与 arm 精确语义（§2）逐字保留；不引入托盘/常驻/热键/半透明/NOACTIVATE/多语言；旧 exe 与 DeskBox 其他文件不动；不改系统设置、不联网 |

## 6. 验证方案（延续 baseline 验证文化，全部自动化）

1. `VerifyOverlay`（扩展自 VerifyBlackScreen.cs）：可见窗体数==屏数且逐屏边界精确、TopMost、无边框；每屏 25 采样点==**配置色**（临时写测试配置到真实配置路径，跑完还原）；有文字时中心区含自动文字色像素、无文字时全屏纯色；3 键前台+后台键盘退出、退出码 0；第二实例自退；`WaitForInputIdle` ≤2000ms 并报告实测毫秒。
2. `VerifyMouseExit`（继承适配）：鼠标退出语义不变。
3. `VerifyConfig`：roundtrip、缺文件/损坏/超限/未知键回落默认、`\n`/`\\` 转义。
4. `VerifySettings`：`--settings` 恰好一个非遮罩窗体且进程存活、不占用遮罩互斥体；未知参数不产生任何遮罩窗体。
5. 人工检查清单写入使用说明（设置界面操作、预览、试用、双快捷方式）。

## 7. 风险与边界（如实继承，写入使用说明）

安全桌面/锁屏/UAC 提权界面不可覆盖；TopMost 窗体间无绝对覆盖保证；arm 是采样机制非时序保证；"任意键"受系统保留组合限制；设置保存需下次启动生效（遮罩运行中按任意键即退出属冻结语义，设置窗口操作本身会先退出遮罩）；混合 DPI 副屏文字清晰度依赖 D2 拓扑。

## 8. 参考链接（lane A 调研精选）

- 同类项目：github.com/Stonyx/BlackScreen · github.com/RG-rahul/screenDimmer · github.com/BlueBird67/BlackScreens · github.com/dhr412/privacyscreen · github.com/limbo666/Black_My_Screen · github.com/aurelitec/cinemadrape-windows（反面教材：VirtualScreen 覆盖 bug）
- 技术依据：learn.microsoft.com/zh-cn/windows/win32/hidpi/high-dpi-desktop-application-development-on-windows（跨 DPI 窗口位图拉伸）· stackoverflow.com/a/23230570（DrawText vs DrawString）· stackoverflow.com/a/65313019 与 eskerahn.dk/?p=1779（WinForms per-monitor DPI 实践）
- 完整清单：`C:\Users\Lenovo\.wbx\jobs\20261019-033502-5zf\overlay-research.json`；架构批判全文：同目录 `arch-critique.json`

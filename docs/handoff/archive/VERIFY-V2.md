# VERIFY-V2 · 一键黑屏 v2 前端优化验证记录

> **一句话定位**：v2 改造（U1 字体链 / U2 主题跟随 / U3 双图标 / U4 验证扩展）的全部自动化验证命令与输出摘要、视觉验收记录；对应 RESEARCH-V2.md §6 验证方案与 §9 T7 评审裁决。
> **日期**：2026-10-19　验证环境：Windows 11 26300（混合 DPI 双屏：主 2560×1600@150%、副 1920×1080@125% 负坐标；当前系统应用主题=浅色）
> 注意：运行 verify.cmd 会真实闪现全屏遮罩数秒，属预期行为。

## 1. 构建

命令（仓库根）：

```cmd
build.cmd
```

输出摘要：

```text
[OK] Build succeeded: D:\Download\Agent Vault\一键黑屏\dist\一键黑屏.exe
退出码 0；dist 三件：一键黑屏.exe + 一键黑屏.exe.config + icon-settings.ico（110145 字节）
```

构建变更：build.rsp 增 `-win32icon:assets\icon-main.ico`、`-resource:assets\icon-settings.ico,settings.ico`、`src\ThemePalette.cs`；build.cmd 增 dist ico 拷贝（`tr -d '\r' | grep '[^ -~\t]'` 证纯 ASCII）。

**V9 回退梯子定案：未启用**——旧 csc（Framework64 v4.0.30319）对 256px PNG 图标条目直接接受（首编即过，CS 错误为零），主路径（七档：≤128 DIB + 256 PNG）成立。

## 2. 五件套验证器

命令（仓库根；自动编译 verify\ 五件到 verify\bin\ 并依次运行）：

```cmd
verify.cmd
```

输出摘要（2026-10-19 全绿，退出码 0）：

```text
[1/5] VerifyConfig
PASS: 18 config tests (defaults/roundtrip/format/golden/corruption/escapes/atomic-write)
[2/5] VerifySettings
PASS: --settings opened exactly one closable non-mask window, exit code 0
PASS: two --settings instances coexisted and both closed with exit code 0
PASS: unknown argument showed usage dialog, zero mask windows, exit code 0
PASS: settings background follows AppsUseLightTheme (light, expected #F5F5F5)
PASS TOTAL: 4 settings checks (3 original + theme background)
[3/5] VerifyMouseExit
PASS: Left/Right/Middle/X1/X2 各 12 移动+2 滚轮不退、按下不退、抬起退出且退出码 0
PASS TOTAL: 5/5 buttons; 60 movement messages; 10 wheel messages; 2 windows per launch
[4/5] VerifyOverlay
PASS A: no config = pure black v1 behavior, 2 windows, exact bounds, topmost, borderless
PASS B: configured color + text rendered, auto-contrast text pixels found   ← U1 回归门：YaHei UI 字体链下原样通过
PASS C: color-only config = solid mask on every screen
PASS D: key 0x87/0x1B/0x10 × 前台+后台、0x12(Alt) 前台，全部退出码 0
PASS E: held key blocked arming, unarmed key ignored, exit after release
PASS F: second instance self-exited silently, first stayed alive
PASS TOTAL: A-F; 2 displays; max InputIdle 178 ms (limit 2000)
[5/5] VerifyIcon
PASS: icon-main.ico structure ok (7 tiers, in-bounds, all entries decodable)
PASS: icon-settings.ico structure ok (7 tiers, in-bounds, all entries decodable)
PASS: exe embeds icon-main 256px PNG payload (offset 236832)
PASS: settings.ico managed resource byte-identical to artifact and decodable
PASS: dist\icon-settings.ico deployed byte-identical (110145 bytes)
PASS TOTAL: 4 icon checks
[OK] ALL VERIFIERS PASSED
```

覆盖对照（v2 新增部分；v1 四件套覆盖面见 docs/VERIFY-V1.md §3）：

| 验证器 | v2 新增覆盖点 |
|---|---|
| VerifySettings | 第 4 查：读注册表 AppsUseLightTheme（全根名）→ 期望调色板背景色（浅 #F5F5F5/深 #202020）→ PrintWindow(PW_CLIENTONLY) 采样客户区底部空带 (w/2,h-4) 与左缘 (2,h/2)，RGB 精确相等（T7 #19：比较 `ToArgb()&0xFFFFFF`，预填 Magenta 证缓冲被绘制）；采样前窗口边界两读稳定门（T7 #20）；验证器 DPI 感知（PMv2 context）保采样物理坐标。**不改任何系统设置** |
| VerifyIcon | ①两 ico 结构：reserved/type==1/count==7/宽高字节∈七档且唯一（256→0）/planes==1/bitCount==32/条目顺序偏移/long 界内校验/DIB 长==40+4wh+AND 行对齐/含 256 PNG 条目；逐条目解码：PNG 尺寸==声明、DIB 行序翻转重建+存在 alpha>0 与非零彩色像素（T7 #16/#17）②exe 字节含 icon-main 256px PNG 载荷（64 字节前缀扫描+全量比对，offset 236832）③`Assembly.ReflectionOnlyLoadFrom(exe)`：资源名含 settings.ico、流字节==assets\icon-settings.ico 全量、`new Icon` 可解码（T7 #18）④dist\icon-settings.ico 存在且字节相同 |

## 3. 视觉验收（2026-10-19）

### 3.1 图标（16/32/256 三级目检，`_probe\icon-*-*.png`）

母题：深色圆角屏幕（亮灰描边 #C3CFD8）+ 夜空底 #0A0E14 + 月牙 #EEF1FB（开口朝右上）；设置变体 = 同母题 + 右下角深蓝灰衬底圆盘（#324049 + 亮描边）浅蓝齿轮 #64B5F6（≤32px 六齿无孔，≥48px 八齿带中心孔）。经四轮视觉迭代（提亮描边、月牙加厚居中、小档 6 齿齿轮、衬底与屏幕拉开明度）后定稿：

| 档位 | 主图标 | 设置图标 |
|---|---|---|
| 16px（×8 放大目检） | 合格：屏幕轮廓清晰可辨、月牙一眼读出（"16px 可辨识"达成） | 合格：母题可辨，齿轮呈蓝色角标色块（16px 物理极限，位置+颜色传达语义） |
| 32px（×8 放大目检） | 合格：轮廓描边均匀、月牙清晰 | 合格：齿轮角标语义成立（六齿+衬底），抗锯齿下可读 |
| 256px（原图目检） | 合格：圆角矩形工整、月牙造型良好、抗锯齿干净 | 合格：齿轮齿形规整、中心孔清晰、衬底分离有效 |

已知观感残留（接受）：32px 齿轮齿间负空间有限（放大观察才明显）；256px 齿谷圆角（旋转矩形齿的固有形态）。均为审美级，不碍语义。

### 3.2 设置窗（浅色=系统当前主题；深色=反射强制 ApplyTheme(false)，不改系统设置）

- **浅色截图**（`_probe\settings-light.png`，PrintWindow 导出）：标题栏浅色且左侧图标可见（内嵌 settings.ico 渲染于标题栏，V9 链路通）；预览区 #1A2B3C 底+两行浅色文字清晰、1px 主题边框完整闭合；六色板原色、无主题串色；标签/文本框/按钮文字清晰无裁剪；布局无重叠溢出。视觉模型逐项确认（1-5 全过，残留仅"提示文字对比度可再高"的审美级意见，TextSecondary #616161 对 #F5F5F5 对比 ≈4.7:1 达 WCAG AA）。
- **色板选中态（像素级证据）**：`_probe\settings-light-selected.png`（配置色=第 3 色板 #1F3A5F，150% 主屏 666×851）逐像素采样：swatch3 边框==**#0067C0（Accent）**、BorderSize=2；其余五个边框==#D9D9D9、BorderSize=1。选中态渲染正确（视觉模型在 1px 边框下难辨 → 已按"厚度+颜色"双差异修正）。
- **深色截图**（`_probe\settings-dark-forced.png`，探针 `_probe\DarkProbe.cs` 反射调用 ApplyTheme(false)——验证深色分支渲染，全程不触碰系统设置）：客户区整体深色、背景像素==**#202020** ✓；六色板原色未串色；多行文本框深底浅字（`SetWindowTheme("DarkMode_Explorer")` 链路生效）；黑色块选中高亮可见；无重叠/不可读项。DWM 深色标题栏为系统实时合成，PrintWindow 导出不含其效果，由 U2 机制（属性 20 双向重设）+代码路径对称性保证。
- **活切换**：WM_SETTINGCHANGE→150ms 去抖→值变化才 ApplyTheme 的路径无法在不改系统主题的前提下自动化验收（禁改系统设置）；该路径与已验证的首次 ApplyPalette 共用同一方法（ApplyTheme(false) 仅切换 palette 参数），由代码对称性与浅/深双路渲染验证覆盖。

### 3.3 双 DPI 实测

- 150% 主屏：设置窗 666×851 物理像素（与 v1 已验收尺寸一致——V7 勘误后布局不变、字体由 13.5pt→9pt 修正为单缩放）。
- 125% 副屏：设置窗 556×716 物理像素（CenterScreen 落副屏时按副屏 DPI 自适应），截图确认布局/字体正常。
- 字体实测（`_probe\FontProbe.cs`，PMv2 进程，144 DPI）：`9f Point`→Font.Height 23px（em 18px）；`9f×1.5 Point`（v1 写法）→35px（em 27px）＝双重缩放偏大 50%。V7 勘误依据（RESEARCH-V2 §9 #4）。

## 4. 实现过程中的验证事件记录

- **T7 评审**（2026-10-19，wbx codex job `20261019-150846-b9u`）：21 条意见（P0×2/P1×11/P2×8）全部裁决入 RESEARCH-V2 §9；两处 P0（`as int? ?? true` 不可编译；Registry.GetValue 根名必须全称）在实现前修正，避免了"恒浅色假绿"缺陷。
- **编译期修正**：`System.Windows.Forms.Timer` 无 `IsDisposed` 属性（Component 不暴露）→ 改 `themeTimerDisposed` 显式标志于 Dispose(bool) 置位。
- **色板选中态强化**：视觉模型在 556px 截图上分辨不出 1px Accent 蓝边压深蓝底 → 像素级核查证明机制正确（边框 #0067C0 在渲染）→ 依 V4 FlatAppearance 同族机制加"选中 BorderSize=2"双差异，复测像素确认。
- **VerifySettings 主题断言首跑即绿**：期望 #F5F5F5 与实际采样相等——主题检测（全根名注册表）→ThemePalette→Form.BackColor→PrintWindow 全链路一次通过。

## 5. 交付快照（Phase 5 终验，2026-10-19 15:54，最终产物）

- `build.cmd` → 退出码 0；`dist\一键黑屏.exe`（243200 字节，内嵌双图标故较 v1 的 20992 字节增大）+ `dist\一键黑屏.exe.config`（245 字节）+ `dist\icon-settings.ico`（110145 字节）三件非空。
- `verify.cmd` → 退出码 0，五件套全 PASS（30 条 PASS 行）：VerifyConfig 18 项 / VerifySettings 4 项（3 原 + 主题背景）/ VerifyMouseExit 5 键 / VerifyOverlay A–F（Overlay B 在 YaHei UI 字体链下原样通过 = U1 回归门）/ VerifyIcon 4 查。
- InputIdle 实测最大 **116 ms**（限值 2000 ms；多轮观测 178/129/116 ms——字体链静态解析未拖慢热路径）。
- `install.ps1` 连跑两次均退出码 0（幂等）：主 .lnk 无自定义图标（回读归一化 `',0'`==空）；设置 .lnk `IconLocation` 回读==`D:\Download\Agent Vault\一键黑屏\dist\icon-settings.ico,0`；快捷方式目录 13 项（2 个本程序 .lnk + 11 个未触碰）。
- 冻结项核查：`git diff main -- baseline/` 为空；`git diff main --stat -- src/Program.cs src/MaskConfig.cs src/OverlayForm.cs` 为空；v1 四件套断言零改动（VerifyConfig/MouseExit/Overlay 仅 BOM 前置，VerifySettings 仅 BOM + 总计行 3→4 查，原 Check 断言逐字保留）；设置窗控件集合与保存语义不变。
- 源码纪律：src/、tools/、verify/ 全部 .cs UTF-8 带 BOM（baseline/ 冻结不改）；build.rsp 引用仅 System/System.Drawing/System.Windows.Forms；C#5 编译通过即证方言兼容。
- 分支：`feature/frontend-v2`，6 提交（勘察评审/图标/源码/构建验证/BOM 规范/安装文档），工作树干净；不自行 merge main（合并指令：`git checkout main && git merge feature/frontend-v2`，由用户确认后执行）。

# VERIFY-V1 · 一键黑屏 v1 自定义化验证记录

> **一句话定位**：v1 改造（R1 颜色 / R2 文字 / R3 设置入口）的全部自动化验证命令与输出摘要；对应 RESEARCH-V1.md §6 验证方案。
> **日期**：2026-10-19　验证环境：Windows（混合 DPI 双屏，主 2560×1600@150%、副 1920×1080@125% 负坐标）
> 注意：运行 verify.cmd 会真实闪现全屏遮罩数秒，属预期行为。

## 1. 构建

命令（仓库根）：

```cmd
build.cmd
```

输出摘要：

```text
[OK] Build succeeded: D:\Download\Agent Vault\一键黑屏\dist\一键黑屏.exe
退出码 0；dist\一键黑屏.exe 20480 字节
```

## 2. 全部验证器

命令（仓库根；自动编译 verify\ 四件套到 verify\bin\ 并依次运行）：

```cmd
verify.cmd
```

输出摘要（2026-10-19 全绿，退出码 0）：

```text
[1/4] VerifyConfig
PASS: 18 config tests (defaults/roundtrip/format/golden/corruption/escapes/atomic-write)
[2/4] VerifySettings
PASS: --settings opened exactly one closable non-mask window, exit code 0
PASS: two --settings instances coexisted and both closed with exit code 0
PASS: unknown argument showed usage dialog, zero mask windows, exit code 0
PASS TOTAL: 3 settings checks
[3/4] VerifyMouseExit
PASS: Left: 12 moves and 2 wheel messages kept open; down kept open; up exited
PASS: Right / Middle / X1 / X2: 同上（各 12 moves + 2 wheel 不退；down 不退；up 退出，退出码 0）
PASS TOTAL: 5/5 buttons; 60 movement messages; 10 wheel messages; 2 windows per launch
[4/4] VerifyOverlay
PASS A: no config = pure black v1 behavior, 2 windows, exact bounds, topmost, borderless
PASS B: configured color + text rendered, auto-contrast text pixels found
PASS C: color-only config = solid mask on every screen
PASS D: key 0x87(F24)/0x1B(Escape)/0x10(Shift) × 前台+后台、0x12(Alt, WM_SYSKEYDOWN) 前台，全部退出码 0
PASS E: held key blocked arming, unarmed key ignored, exit after release
PASS F: second instance self-exited silently, first stayed alive
PASS TOTAL: A-F; 2 displays; max InputIdle 531 ms (limit 2000)
[OK] ALL VERIFIERS PASSED
```

## 3. 验证器覆盖对照（RESEARCH-V1 §6）

| 验证器 | 覆盖点 |
|---|---|
| VerifyConfig | roundtrip（含中文与转义）；缺文件/损坏/超限（64KB 字节上限）/非法 UTF-8/非法颜色/超 200 字 → 整份回落默认；未知键忽略；重复键后值覆盖；golden 字节样例（CRLF/LF/BOM）；原子写不留临时文件；嵌套目录创建 |
| VerifySettings | `--settings` 恰好一个非遮罩窗体（有标题、非全屏无边框置顶）、可被 WM_CLOSE 关闭、退出码 0；双设置实例共存（设置模式不受互斥体限制）；未知参数仅弹用法对话框、零遮罩窗体 |
| VerifyMouseExit | 每次启动恰 2 窗（==屏数）且逐窗边界==该屏物理矩形、客户区==窗口；移动/滚轮/按下不退出；抬起退出且退出码 0；五键（左/右/中/X1/X2）全覆盖，消息按屏投递到对应窗口 |
| VerifyOverlay | 无配置=纯黑无文字（v1 等价）；配置色 25 采样点/屏精确匹配；有文字时中心区含 WCAG 自动对比度文字色像素、无文字时中心纯色；逐屏边界精确/TopMost/无边框；3 键前台+后台键盘退出（退出码 0）+ Alt 系统键路径；arm 冻结语义（按住不 arm、未 arm 按键忽略、松开后同键退出）；第二实例静默自退、首实例存活；InputIdle 全程实测 ≤2000ms（实测最大 531ms） |

## 4. 实现过程中的验证事件记录

- 2026-10-19 首轮 VerifySettings 第 2 项失败（索引越界）：根因为验证器对“窗口出现时机”假设过脆（WaitForInputIdle+固定 sleep 存在竞态），经独立复现脚本确认应用本身行为正确（双实例窗口均正常出现），属验证器缺陷而非被测程序缺陷。修复：三个 UI 验证器统一改为“轮询等待窗口就绪（≤5s）+400ms arm 等待”。修复后全绿。
- InputIdle 口径：自 Process.Start 前起计时至 WaitForInputIdle 返回，含进程创建的完整启动耗时。
- 2026-10-19 终验期间 VerifyOverlay 曾出现一次 "Overlay exited before showing windows"（Shift-后台轮）：以与验证器完全一致的启动方式做 35/35 轮“启动→退出→再启动”复测全部通过，且无任何崩溃事件日志——判定为活动桌面上真实键鼠输入恰好命中遮罩（armed 退出属冻结语义的正确行为），环境性偶发而非程序缺陷。验证器错误信息已改进（附带退出码：0=真实输入退出，1=内部失败）。
- 2026-10-19 设置窗 DPI 缩放排查：`DeviceDpi` 在无 app.config DPI 节的 WinForms 下恒返 96，改用 `GetDpiForWindow(Handle)` 取真实逐显 DPI；另发现**无 DPI 感知的测量进程观测 PMv2 窗口时 GetWindowRect 会按 96/144 虚拟化坐标**（曾误判“窗口被缩回”），改用 DPI 感知观察者复测：设置窗实际 666×851 物理像素、位于 150% 主屏居中，缩放正确。附带交付 `dist\一键黑屏.exe.config`（WinForms PMv2 框架级支持，官方推荐配置）。

## 5. 人工验收记录（Phase 5，2026-10-19）

以 UI 自动化实际操作设置窗口（`--settings`）逐项验收：

| 项 | 操作与实据 | 结论 |
|---|---|---|
| 打开 | `--settings` 恰一个常规窗口（有标题栏、非全屏无边框置顶），控件齐全（预览/6 色板+自定义/文字框/四按钮/提示），中文全部正常显示 | ✓ |
| 预览-颜色 | 点击色板 → PrintWindow 无损采样：预览区主色变为该色板色（17014/17014 采样点） | ✓ |
| 预览-文字 | 键盘输入文字 → 预览中心带出现 400 个浅色文字像素（WCAG 自动对比度 #F5F5F5） | ✓ |
| 恢复默认 | 点击后预览回纯黑（4/4 采样点）、文字框清空 | ✓ |
| 保存并试用 | `settings.ini` 写出 `color=#1E3B2E` + `text=验收 OK`（中文 UTF-8 无损）；对话框关闭；遮罩进程随即启动；Escape 退出；进程全部结束 | ✓ |
| DPI 布局 | 150% 主屏：窗口 666×851 物理像素、居中；预览按主屏宽高比 | ✓ |
| 快捷方式 | 双 .lnk 目标/参数正确（见 §6 安装记录） | ✓ |

注：UIA `setValue` 写入文本框不触发 WinForms TextChanged（走 WM_SETTEXT 路径），属自动化通道差异；真实键盘输入与程序自身赋值均正常，保存逻辑直接读 textBox.Text 不受影响。

## 6. 终验全量重跑与交付快照（2026-10-19，最终产物）

- `build.cmd` → 退出码 0，`dist\一键黑屏.exe`（20992 字节）+ `dist\一键黑屏.exe.config`（239 字节）。
- `verify.cmd` → 退出码 0，四件套全 PASS（25 条 PASS 行）：VerifyConfig 18 项 / VerifySettings 3 项 / VerifyMouseExit 5 键 / VerifyOverlay A–F（含 3 键前台+后台 + Alt 系统键、arm 冻结语义、第二实例自退）。
- InputIdle 实测最大 **650 ms**（限值 2000 ms；多轮观测 531/634/726/650 ms）。
- `install.ps1` 连跑两次均退出码 0（幂等），双快捷方式回读校验：`一键黑屏.lnk → dist\一键黑屏.exe（无参）`、`一键黑屏-设置.lnk → dist\一键黑屏.exe --settings`；目录内其他 11 个 .lnk 未触碰。
- 验证器清单与最终结果：VerifyConfig PASS / VerifySettings PASS / VerifyMouseExit PASS / VerifyOverlay PASS，`[OK] ALL VERIFIERS PASSED`。
- 分支：feature/mask-customization，工作树干净；`git diff main -- baseline/` 为空。

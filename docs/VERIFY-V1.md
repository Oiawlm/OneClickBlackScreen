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

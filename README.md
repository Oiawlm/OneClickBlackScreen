# 一键黑屏 · 修改仓库

> **一句话定位**：「一键黑屏」软件的专属修改仓库——v1 源码（src/）、验证套件（verify/）、调研底稿与执行契约的唯一存放地。
> **来源**：原始程序出自 Codex 会话产物 `C:\Users\Lenovo\Documents\Codex\2026-09-07\gei\`（源码已归档至 [baseline/](baseline/)）。
> **日期**：2026-10-19

## 这个软件是什么

C# WinForms 程序（.NET Framework，csc 编译，零依赖）。双击后每台显示器一个无边框 TopMost 窗体
立即覆盖（默认纯黑），任意按键/鼠标键退出（100ms 防误触 arm 延迟），低级键盘钩子（WH_KEYBOARD_LL）
实现全局退出，WM_SETCURSOR 隐藏指针，PerMonitorV2 DPI 感知。v1 起：遮罩颜色自定义、遮罩文字自定义
（自动对比度取色）、`--settings` 设置入口；v2 起：遮罩中文雅黑字体链、设置窗跟随系统深浅色、
双图标（主图标嵌入 exe、设置图标内嵌托管资源+快捷方式引用）。双击立即出遮罩与退出交互始终冻结不变。

## 文档地图（何时读 / 何时别读）

| 文件 | 何时读 |
|---|---|
| [baseline/](baseline/) | 需要原始源码事实时——`BlackScreen.cs` 是行为基准，冻结不改 |
| [RESEARCH-V1.md](RESEARCH-V1.md) | v1 事实源：调研结论 + §5 架构决策表 D1–D12 + §9 T7 评审记录 |
| [RESEARCH-V2.md](RESEARCH-V2.md) | v2 事实源：三路调研 + §5 决策表 V1–V14 + §9 T7 评审记录（含 V7 勘误） |
| [GOAL-V1-PROMPT.md](GOAL-V1-PROMPT.md) / [GOAL-V2-PROMPT.md](GOAL-V2-PROMPT.md) | 交给执行窗口的 GOAL 提示词（规划窗口产出，执行窗口输入） |
| [src/](src/) | 实现源码（Program / MaskConfig / MaskPainter / OverlayForm+OverlayGroup / SettingsDialog / ThemePalette） |
| [tools/MakeIcon.cs](tools/MakeIcon.cs) + [assets/](assets/) | 图标生成器与双 .ico 产物（显式重新生成，构建只消费产物） |
| [verify/](verify/) + verify.cmd | 五件套验证器与全量验证入口（会闪现遮罩） |
| [docs/VERIFY-V1.md](docs/VERIFY-V1.md) / [docs/VERIFY-V2.md](docs/VERIFY-V2.md) | 两轮验证命令与输出摘要、人工验收记录、交付快照 |
| build.cmd + build.rsp + app.config | 构建入口（产物 `dist\一键黑屏.exe`、同名 `.exe.config` 与 `icon-settings.ico`；exe.config 启用 WinForms 逐显示器 DPI 支持） |
| install.ps1 | 幂等同步 DeskBox 双快捷方式（设置 .lnk 含图标引用回读校验） |
| 使用说明.txt | 面向最终用户的 v3 说明 |
| `dist/` | 构建产物输出目录（不入库） |

## 铁律（对所有窗口生效）

1. **不打补丁**：任何修改必须融合重构进代码逻辑，从根源解决问题；最终代码以简洁且完整实现需求为准。
2. **即时遮罩体验冻结**：双击 → 遮罩立即出现，不得先弹任何设置界面。
3. **上下文只经文件传递**：跨窗口协作一律通过本仓库文件，不依赖会话记忆。

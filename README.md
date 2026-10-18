# 一键黑屏 · 修改仓库

> **一句话定位**：「一键黑屏」软件的专属修改仓库——基线源码、调研底稿与执行提示词的唯一存放地。
> **来源**：原始程序出自 Codex 会话产物 `C:\Users\Lenovo\Documents\Codex\2026-09-07\gei\`（源码已归档至 [baseline/](baseline/)）。
> **日期**：2026-10-19

## 这个软件是什么

C# WinForms 单文件程序（.NET Framework，csc 编译，7KB）。双击后一个无边框 TopMost 窗体覆盖
`SystemInformation.VirtualScreen`（所有显示器纯黑），任意按键/鼠标点击退出（100ms 防误触 arm 延迟），
低级键盘钩子（WH_KEYBOARD_LL）实现全局退出，WM_SETCURSOR 隐藏指针，PerMonitorV2 DPI 感知。

## 文档地图（何时读 / 何时别读）

| 文件 | 何时读 |
|---|---|
| [baseline/](baseline/) | 需要原始源码事实时——`BlackScreen.cs` 是行为基准，冻结不改 |
| [RESEARCH-V1.md](RESEARCH-V1.md) | 实现自定义遮罩功能前的调研底稿（唯一事实源） |
| [GOAL-V1-PROMPT.md](GOAL-V1-PROMPT.md) | 交给执行窗口的 GOAL 提示词（规划窗口产出，执行窗口输入） |
| `dist/` | 构建产物输出目录（执行窗口创建） |

## 铁律（对所有窗口生效）

1. **不打补丁**：任何修改必须融合重构进代码逻辑，从根源解决问题；最终代码以简洁且完整实现需求为准。
2. **即时遮罩体验冻结**：双击 → 遮罩立即出现，不得先弹任何设置界面。
3. **上下文只经文件传递**：跨窗口协作一律通过本仓库文件，不依赖会话记忆。

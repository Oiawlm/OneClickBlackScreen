<div align="center">

# 一键黑屏 · One-Click Black Screen

双击瞬间，所有屏幕一起被同一颜色覆盖；按任意键即退出。<br>
零依赖、单文件、无需安装的 Windows 绿色小工具。

[![Release](https://img.shields.io/github/v/release/Oiawlm/OneClickBlackScreen?style=flat)](https://github.com/Oiawlm/OneClickBlackScreen/releases)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue?style=flat)](LICENSE)
[![Platform: Windows](https://img.shields.io/badge/Platform-Windows-blue?style=flat)](#)

<img src="assets/readme/mask-demo.png" alt="全屏遮罩效果：配置色 #1F3A5F 上显示自定义文字「屏幕休息中 / 按任意键退出」，文字颜色自动取白以保证可读" width="720">

*配置色与自定义文字的遮罩效果（默认为纯黑、无文字）*

</div>

## ✨ 功能特性

- **双击即遮罩**：不弹任何界面，所有已连接并启用的显示器立即被同一颜色覆盖，默认纯黑。
- **任意键退出**：键盘任意键或鼠标任意键退出，内置 100 ms 防误触；只移动鼠标、滚动滚轮不会退出。
- **颜色与文字自定义**：六种预设色块（黑 / 深灰 / 深蓝 / 墨绿 / 暗红 / 白）或任意取色；遮罩文字最多 200 字、支持换行，文字颜色自动选黑或白保证在底色上看清。
- **深浅色跟随**：设置窗口自动跟随 Windows 应用深浅色模式，切换后约半秒内自动刷新，无需重开。
- **PerMonitorV2 DPI 感知**：混合 DPI 多屏每屏独立渲染，副屏文字同样清晰。
- **绿色零依赖**：单 exe（.NET Framework 4.8，Windows 10 1903+/11 出厂自带运行时），无需安装与管理员权限，不联网、不改系统设置。

## 🖼️ 界面一览

<p align="center">
  <img src="assets/readme/settings-light.png" alt="浅色主题下的设置窗口：遮罩颜色色块、遮罩文字输入框、实时预览与四个操作按钮" width="380">
  <img src="assets/readme/settings-dark.png" alt="深色主题下的设置窗口：同样的布局自动切换为深色配色，主按钮变为亮蓝" width="380">
</p>

*设置窗口跟随系统深浅色（左：浅色；右：深色）*

## 📦 下载与安装

1. 打开 [Releases 页](https://github.com/Oiawlm/OneClickBlackScreen/releases)，下载最新版的 `OneClickBlackScreen.exe`。
2. （推荐）校验文件完整性——在 PowerShell 或 cmd 中运行：

   ```
   certutil -hashfile OneClickBlackScreen.exe SHA256
   ```

   将输出的哈希与 Releases 页 `SHA256SUMS` 文件中对应行逐字比对，一致即通过。
3. 双击 exe 即可使用；无需安装。首次使用若弹出 SmartScreen 提示，选「仍要运行」（见下方边界说明）。

## 🚀 使用

**两种启动方式**

| 启动方式 | 入口 | 效果 |
|---|---|---|
| 遮罩模式 | 双击 exe（或「一键黑屏」快捷方式） | 立即全屏遮罩，绝不先弹设置界面 |
| 设置模式 | 加参数 `--settings`（或「一键黑屏-设置」快捷方式） | 打开设置窗口 |

**设置窗口能改什么**

| 设置项 | 说明 |
|---|---|
| 遮罩颜色 | 点色块快速选，或「自定义颜色…」任意选；当前选中色块带蓝色加粗边框 |
| 遮罩文字 | 最多 200 字，输入框内回车即换行；留空则不显示；文字颜色自动选黑/白 |
| 预览区 | 按主屏比例实时显示效果，所见即所得 |
| 按钮 | 恢复默认 / 取消 / 保存 / 保存并试用（保存后立刻启动一次遮罩供确认） |

设置保存在 `%LOCALAPPDATA%\BlackScreen\settings.ini`（`color=` 与 `text=` 两行）；修改保存后，**下一次启动遮罩**时生效。不想要配置文件时直接删除它，即回纯黑默认。

**退出交互**

| 操作 | 结果 |
|---|---|
| 按任意键盘按键 | 退出 |
| 点击鼠标任意键（含滚轮中键） | 退出 |
| 只移动鼠标 / 滚动滚轮 | 不退出 |
| 启动时按键未松开 | 先松开，再按键即可退出 |

遮罩运行时鼠标指针自动隐藏。

## ⚠️ 边界与限制（如实告知）

- 程序只覆盖画面，不关闭显示器电源，不暂停正在运行的程序或声音。
- 安全桌面 / 锁屏 / UAC 提权界面无法被覆盖；TopMost 窗体之间无绝对覆盖保证。
- 设置中弹出的系统对话框（取色器、消息框）仍为系统浅色样式，不跟随深色，属 Windows 系统限制；高对比度主题不适配。
- 「任意键」受系统保留组合限制；混合 DPI 副屏文字清晰度依赖每屏一窗渲染。
- 若遮罩已在运行，「保存并试用」启动的第二个实例会静默自退（单实例机制），先退出当前遮罩再试用即可。
- 传入未知参数时只弹用法提示，绝不进入遮罩。
- 浏览器下载的 exe 可能触发 SmartScreen 提示（无代码签名证书所致），校验 SHA256 后选择「仍要运行」即可。

**卸载**：删除程序所在文件夹（exe、同名 `.exe.config`、`icon-settings.ico` 三个文件）、快捷方式与配置文件，即完全移除，无其他残留。

## 🛠️ 本地开发

构建与验证只用 Windows 自带工具，零安装：

```
build.cmd
verify.cmd
```

- `build.cmd`：用 `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe` 直编 `src\`，产物输出到 `dist\`（csc 为 Windows 自带，无需安装任何 SDK）。
- `verify.cmd`：编译并运行五件套验证器（VerifyConfig / VerifySettings / VerifyMouseExit / VerifyOverlay / VerifyIcon；会短暂闪现遮罩，属预期行为）。
- `install.ps1`：幂等同步桌面快捷方式（双击遮罩 + 设置两个入口）。
- `tools\MakeIcon.cs`：图标生成器（显式重新生成，构建只消费产物）。

## 📖 开发过程文档

本仓库由多轮「规划 + 执行」窗口协作开发，以下为**开发过程记录**（非用户文档），如实保留：

| 文档 | 内容 |
|---|---|
| [RESEARCH-V1.md](RESEARCH-V1.md) ~ [RESEARCH-V5.md](RESEARCH-V5.md) | 各轮唯一事实源：调研结论与决策表 |
| [PLAN-V5.md](PLAN-V5.md) | v5 轮总编排：轨道划分、波次与并行契约 |
| [GOAL-V1-PROMPT.md](GOAL-V1-PROMPT.md) ~ [GOAL-V5D-PROMPT.md](GOAL-V5D-PROMPT.md) | 各执行窗口的 GOAL 提示词 |
| [docs/VERIFY-V1.md](docs/VERIFY-V1.md) ~ [docs/VERIFY-V4.md](docs/VERIFY-V4.md) | 各轮验证记录：命令、输出与证据 |
| [baseline/](baseline/) | v1 原始源码归档（冻结不改，行为基准） |

协作铁律：双击立即出遮罩的体验冻结不变；上下文只经文件传递，不依赖会话记忆。

## 📄 许可证

[MIT](LICENSE) © 2026 Oiawlm

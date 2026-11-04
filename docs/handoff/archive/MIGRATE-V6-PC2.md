# MIGRATE-V6-PC2 · PC2 迁移阶段 8 对照与清理准入证（v6 轮收尾）

> **一句话定位**：PC2（迁移目标机）阶段 8 五项勾选对照——1/2/3/5 四项本轮程序化实证（证据见 [VERIFY-V6.md](VERIFY-V6.md)），**第 4 项永远留给用户亲手双击冒烟**；四项全证＋第 4 项待用户＝清理准入证未生效，旧机一个文件都不能删。
> **日期**：2026-11-03　依据：`migrate/迁移手册.md` 阶段 8 五项口径；上游事实源 RESEARCH-V6 §5。

## 1. 阶段 8 五项对照表

| # | 验证项 | 手册通过标准 | 本轮实证（PC2，2026-11-03） | 状态 |
|---|---|---|---|---|
| 1 | 仓库完整 | `git fsck` 无报错输出 | 零输出、exit 0（VERIFY-V6 §6.1） | ✅ 补档 |
| 2 | 无未提交改动 | `git status` 只显示分支行 | `On branch main / nothing to commit, working tree clean`（VERIFY-V6 §6.1；docs 提交推送后复核仍 clean） | ✅ 补档 |
| 3 | 七件全绿 | `verify.cmd` 末行 `[OK] ALL VERIFIERS PASSED` | 合并后干净键态连跑 2 次＋版本 1.0.1 后第 3 次，三次全绿（VERIFY-V6 §3） | ✅ |
| 4 | 双击冒烟 | 双击 `dist\一键黑屏.exe`→全屏遮罩出现，按任意键消失 | **待用户亲手双击**（验证的正是真实输入路径，AI 不得代勾） | ⏳ 待用户 |
| 5 | 设置窗冒烟＋保存 | 窗口打开，改一项设置能保存成功 | 程序化镜像实测 PASS：`--settings` 开窗→改遮罩文字→点「保存」→「已保存」确认框→settings.ini 落盘字节精确→退出码 0（VERIFY-V6 §6.3） | ✅（用户可复核见 §3） |

## 2. DeskBox 快捷方式落位（阶段 8 前置）

`powershell -NoProfile -ExecutionPolicy Bypass -File install.ps1` → 两行 `[OK]`＋完成行；WScript 回读：两 .lnk 均指向本仓 `D:\Download\Agent Vault\OneClickBlackScreen\dist\一键黑屏.exe`（设置快捷方式带 `--settings` 与 ico 图标）。实测输出见 VERIFY-V6 §6.2。

## 3. 第 4/5 项用户复核指引

- **第 4 项（必做，亲手）**：双击 DeskBox「一键黑屏」→ 全屏遮罩出现 → 按任意键或点鼠标 → 遮罩消失即勾。
- **第 5 项（可选复核，程序化已证）**：双击 DeskBox「一键黑屏-设置」→ 随便改一项（如遮罩文字）→ 点「保存」→ 见「已保存」提示即勾。本轮已用消息级注入工具按同路径代验（开窗/改/存/确认框/ini 字节/退出码全过），但手册口径允许用户亲手再走一遍。

## 4. 清理准入证状态

**四项已证＋第 4 项待用户 → 准入证未生效。** 在用户亲手完成第 4 项双击冒烟之前：旧电脑上与本项目有关的任何东西（文件夹、快捷方式、计划任务）**一个都不要删**；清理只由旧机阶段 9 的 `cleanup-local.ps1` 做，且**必须用户亲手运行，任何 AI 窗口不得代跑**。

## 5. migrate/ 迁移包完整性（只读校验）

- `migrate/` 在 git 内五件（SHA256SUMS.txt/cleanup-local.ps1/总流程图.png/迁移手册.md/.html）**工作树零改动**，最后一次内容变更停留在 V5D 提交 `3534dd0`（本轮 main 全程未触碰）。
- `migrate/SHA256SUMS.txt` 三条 digest 行与 V5D 登记逐字一致（`FD524F6F…`/`8A8D47C0…`/`ED6C6B22…`）。
- 三条 digest 对应的大文件本体（bundle/skills zip/settings 备份）**从未存在于 PC2**——PC2 走 GitHub 克隆迁移路径（MIGRATE-V5D A5/C4），本体按 .gitignore 留在旧机盘上；故「certutil 对三条本体逐字 MATCH」在 PC2 无对象可算，以 git 侧不可变性＋digest 行登记一致性作为 PC2 端完整性证明（如旧机本体仍在，可在旧机阶段 9 清理前自行复算比对）。

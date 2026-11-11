# GOAL-V10B · 安装器侧：静默更新支持＋数据位置页＋完整卸载（B 轨）

> **一句话定位**：B 轨执行窗（GLM-5.3·目标模式）：`setup/OneClickBlackScreen.iss` 全改——AppMutex/强关/[Run] 静默更新支持、「数据位置」向导页、InstallDirectory 归属登记与卸载器归属守卫、卸载自动删数据根（三重守卫），隔离自测扩展为全生命周期（装→迁移→覆盖更新→卸载零残留），独立 worktree 内完成并推送分支 `feature/installer-v10`。　**事实源＝[RESEARCH-V10.md](RESEARCH-V10.md)（§2.2 fts 参照/§3 D2·D4-D6 定案/§7 契约表），冲突以它为准。**　日期：2026-11-11　行数预算：≤135 行。

## 1. 身份与冷启动

你是 B 轨执行窗，产出落地改动与证据文档，不产出讨论。按序自读（全部在主检出 `D:\Download\Agent Vault\OneClickBlackScreen\OneClickBlackScreen-main\`）：1. `AGENTS.md`；2. `docs/handoff/2026-11-11-v10-lifecycle/RESEARCH-V10.md` 全文；3. `PLAN-V10.md` §2；4. 本文件；5. `setup/OneClickBlackScreen.iss` 现文全文＋`build-setup.cmd`。**禁读**：archive 全文（grep 定位例外）、`baseline/**`、`migrate/**`、`src/**` 与 `verify/**`（A 轨写面；你需要的一切接口在 RESEARCH §7 契约表，不得到 src 里找）。
读完先建 worktree（主检出根执行；命令照抄，git bash）：

```bash
cd "D:/Download/Agent Vault/OneClickBlackScreen/OneClickBlackScreen-main"
git fetch origin
git ls-remote --heads origin feature/installer-v10   # 必须输出为空；非空＝撞名，停下报告
git worktree add "../OneClickBlackScreen-WorkTree/v10-b" -b feature/installer-v10 origin/main
cd "../OneClickBlackScreen-WorkTree/v10-b"           # 本窗全部工作在此，主检出只读
```

**前置闸门**：探测 ISCC（顺序：`where iscc` → `C:\Program Files (x86)\Inno Setup 6\ISCC.exe` → `Inno Setup 7` 同位 → `D:\App\Inno Setup 7`）。全部缺失＝**停下报告用户安装**（不得自行安装软件、不得换打包工具）。

## 2. 背景（为什么）

用户需求①⑤⑥的安装器侧全在本轨：更新链走「Setup 静默重装」需要 .iss 具备强关运行实例＋装完自动重启（fts 模式，D2）；自定义安装目录需要归属登记防「旧位置残留卸载器误删」（D6）；完整卸载需要自动删数据根且不误删（D5）。A 轨（应用侧，写面 src/+verify/）与本轨零文件交集；两轨接口冻结于 RESEARCH §7——**契约表的每个值都是定案，不得自创键名/形状/参数**。现有 .iss 的 per-user/AppId/中文 [Messages]/四件 payload/测试变体隔离（/DTestId）机制全部保留，本轮是在其上的结构化扩展。

## 3. Phase 任务（依序）

**P1 静默更新支持**：`[Setup]` 加 `AppMutex=Local\BlackScreen.Mask`（=src/Program.cs:28 现有互斥名，契约表）；`CloseApplications=yes` 保留默认；`InitializeSetup` 增 `ForceCloseRunningApplication`（仅当 AppMutex 被持有时 `taskkill /F /IM 一键黑屏.exe`，失败靠 AppMutex 阻止覆盖，fts .iss:751-774 同构）；`[Run]` 新增主程序启动条目（**无 skipifsilent**＝静默更新装完自动重启遮罩；测试变体 /DTestId 下编译抑制该条目——测试不许真启遮罩全屏）；向导 [Messages] 补静默更新相关文案零变化语义。
**P2 数据位置向导页**：`InitializeWizard` 增 `CreateInputDirPage`「数据存储位置」页（SelectDir 页之后；默认 `%LOCALAPPDATA%`；校验＝完整路径/非根目录/可建/可写探针，fts `ValidateDataParentDirectory` 同构）；用户改过（≠默认）才写 HKCU `Software\BlackScreen\DataDirectory`=<父>\BlackScreen＋`DataParentDirectory`=<父>（经 `[Registry]` 或 [Code] 按安装期取值写入，**默认态零登记**，D3/D4）；形状判定「以 `\BlackScreen` 结尾且非根」为 .iss 内独立实现，与 RESEARCH §7 契约逐字同构（`IsManagedDataDirectory`，供 P4 复用）。
**P3 归属登记与守卫**：`[Registry]` 写 `InstallDirectory`＝`{app}`（`Flags: uninsdeletevalue`）；`InitializeUninstall` 校验卸载器所在目录==`InstallDirectory` 登记值，不符＝拒绝执行并中文提示（防旧位置残留卸载器误删，D6）；登记值形状非法同样拒绝删数据并保留登记。
**P4 卸载完整化**：`CurUninstallStepChanged(usUninstall)`：读 `DataDirectory`（无登记→默认 `%LOCALAPPDATA%\BlackScreen`）→形状守卫（P2 同构函数）→`DelTree` 删数据根；删除失败仅提示手动清理；同时清 DataDirectory/DataParentDirectory（[Registry] 值带 uninsdeletevalue 兜底）；`ConfirmUninstall` 文案改「确定要完全移除 %1 及其全部组件（含个人设置数据）吗？」（知情不询问，D5）；现有 `[UninstallRun]` 计划任务清理保留原位。
**P5 隔离自测（全生命周期，零用户状态触碰）**：/DTestId 测试变体（测试 AppId＋TEMP 目录＋数据键 `-TEST` 后缀）——①静默装→断言四文件＋InstallDirectory/DataDirectory 登记正确（改过父目录时）＋真实键/真实目录零新增；②改数据父目录（重跑 Setup 选新父目录，模拟向导改址）→断言旧数据随迁/或登记切换语义正确（按 P2 实现断言）；③覆盖更新：同一测试 Setup 再跑 `/VERYSILENT`（模拟 A 轨更新链调用参数 `/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /FORCECLOSEAPPLICATIONS`）→断言文件刷新＋数据根保留＋[Run] 测试变体抑制实证；④静默卸载→断言：测试安装目录零残留、数据根（含故意预置的 settings.ini）已删、测试键全清、计划任务测试名无残留、开始菜单/桌面零新增。全程真实 `%LOCALAPPDATA%\BlackScreen`、真实键、真实任务三态快照前后一致留档。
**P6 留档推送**：`docs/handoff/2026-11-11-v10-lifecycle/VERIFY-V10B.md`（真实命令输出：ISCC 构建日志、P5 全断言实录、三态快照、RESEARCH §7「B 侧」列逐项对照勾验）；提交 `installer(B): V10 …`；`git push -u origin feature/installer-v10`。

## 4. 铁律

写面白名单（对 main 的 diff 必须全部落入）：`setup/OneClickBlackScreen.iss`、`docs/handoff/2026-11-11-v10-lifecycle/VERIFY-V10B.md`(新)。白名单外一律只读。**禁改**：`build-setup.cmd`（版本单源提取机制不变）、`src/**`/`verify/**`（A 轨）、`README.md`/`使用说明.txt`（R 轨）、AGENTS.md。禁补丁式修改——守卫/迁移/登记逻辑融入 .iss 既有结构（[Code] 事件位/测试变体 #define 体系），不为需求叠旁路段。.iss 保持 UTF-8 BOM＋CRLF（L-006，改后 `file` 复验留档）。不触验证闸门（本轨无 GUI 验证器）；真实用户状态只读（三态快照，L-012）；主检出只读；证据＝真实命令输出；不删仓库、凭证不入文件、不得把 ISCC 二进制提交入仓库。

## 5. 预期触碰文件（冲突面申报，供 A/R/I 预判）

与 A 轨零交集；`setup/OneClickBlackScreen.iss` 仅本轨改；RESEARCH §7「B 侧」列全部由本轨实现，值不得偏离（R 轨将逐项对照 A 侧实现核对同构）。

## 6. 验收清单（校验器逐条核对实据）

- [ ] `git ls-remote --heads origin feature/installer-v10` 可见且指向本轨提交
- [ ] `git diff --name-status origin/main...HEAD` 全部落入 §4 白名单
- [ ] `build-setup.cmd` exit 0 且产物 `dist\OneClickBlackScreen-Setup-1.1.0.exe` 存在（版本仍 1.1.0，AppInfo 未动）
- [ ] VERIFY-V10B.md 含：P5 四阶段全断言实录（安装/改址/覆盖更新/卸载零残留）、真实状态三态快照、§7 契约「B 侧」逐项勾验表
- [ ] `grep -n "AppMutex\|InstallDirectory\|DataDirectory\|CurUninstallStepChanged\|InitializeUninstall" setup/OneClickBlackScreen.iss` 各命中且值与 RESEARCH §7 一致
- [ ] `file setup/OneClickBlackScreen.iss` 仍为 UTF-8 BOM＋CRLF
- [ ] `src/**`、`verify/**`、`build-setup.cmd`、`README.md`、`使用说明.txt` 零 diff

## 7. 收尾动作

VERIFY-V10B.md 末尾附「移交注记」（Inno 版本行为差异、[Run]/taskkill 在不同 Windows 版本的边角、留给 I 轨真机冒烟的场景清单——尤其静默更新链与卸载数据删除的实机行为）；推送后报告：分支名、提交哈希、验收清单逐条 ✅/❌。R 轨将复检你的全部产出并预合并。

# GOAL-V11A · 应用侧：双 EXE 入口＋更新链单形态根修＋收官清欠（A 轨）

> **一句话定位**：A 轨执行窗（GLM-5.3·目标模式）：src/ 应用侧全改——双 EXE 免参数入口（D2）、资产名四方同源（D3）、更新链删便携自替换链（D4）、AppPaths 词法守卫＋对称登记（D6/D7）、verify 重构与八件套文案（D8/D9），独立 worktree 内完成并推送分支 `feature/app-v11`。　**事实源＝[RESEARCH-V11.md](RESEARCH-V11.md)（§2.2 现状/§3 定案/§7 契约表），冲突以它为准。**　日期：2026-11-12　行数预算：≤135 行。

## 1. 身份与冷启动

你是 A 轨执行窗，产出落地改动与证据文档，不产出讨论。按序自读（全部在主检出 `D:\Download\Agent Vault\OneClickBlackScreen\OneClickBlackScreen-main\`）：1. `AGENTS.md`；2. `docs/handoff/2026-11-12-v11-singleform/RESEARCH-V11.md` 全文；3. `PLAN-V11.md` §2；4. 本文件。本轮无 UI 视觉新增，**不必加载 fts-style-system**。**禁读**：archive 全文（grep 定位例外）、`baseline/**`、`migrate/**`、`setup/**`＋`build-setup.cmd`（B 轨写面，只读 §7 契约即可）。
读完先建 worktree（主检出根执行；命令照抄，git bash）：

```bash
cd "D:/Download/Agent Vault/OneClickBlackScreen/OneClickBlackScreen-main"
git fetch origin
git ls-remote --heads origin feature/app-v11      # 必须输出为空；非空＝撞名，停下报告
git worktree add "../OneClickBlackScreen-WorkTree/v11-a" -b feature/app-v11 origin/main
cd "../OneClickBlackScreen-WorkTree/v11-a"        # 本窗全部工作在此，主检出只读
```

## 2. 背景（为什么）

用户定案「只要一个版本，照悬浮中转站（fts）的形态做」→ 唯一 Setup 分发（RESEARCH D1/§2.1 实证）；原始诉求「两个 EXE、免参数进设置」随唯一形态落地为双 EXE（D2）。便携自替换更新链因此整体失去存在前提——**从根源删除而非旁路保留**（D4），ARP 版本碰撞（V10 提请④）随之根除。收官清欠（D6/D7/D8/D9）全部落本轨 src/verify 面。与 B 轨（安装器侧）零文件交集，接口冻结于 RESEARCH §7。

## 3. Phase 任务（依序；后 Phase 依赖前 Phase）

**P1 AppInfo 资产名单源（D3）**：`src/AppInfo.cs` 增 `SetupAssetNamePrefix = "OneClickBlackScreen-Setup-"` 与 `ChecksumsAssetName = "SHA256SUMS.txt"` 两常量，删 `UpdateAssetName` 常量；**`Version` 字段零改动（仍 1.2.0，I 轨独占）**。
**P2 双 EXE 入口＋构建双目标（D2/D9）**：①新建 `src/SettingsProgram.cs`——`[STAThread] Main` 忽略参数，`Application.EnableVisualStyles()`＋`SetCompatibleTextRenderingDefault(false)`＋调 `Program.RunSettings()`（Program.cs 中 RunSettings 改 `internal static`，其余入口分派 `--settings`/`--idle`/用法框逐行保留＝旧快捷方式与计划任务兼容）；②`build.rsp` 改遮罩目标（显式 `-main:Program`，`-out:dist\一键黑屏.exe`，`-win32icon:assets\icon-main.ico`）＋新建 `build.settings.rsp`（`-main:SettingsProgram`，`-out:dist\一键黑屏-设置.exe`，`-win32icon:assets\icon-settings.ico`）——两 rsp 源文件清单与引用完全一致（先 grep `settings.ico` 资源消费点定 `-resource` 行去留并申报）；③`build.cmd`：删 `dist\*.exe` glob 与 icon-settings.ico 复制行，显式两目标 csc＋各复制一份 `app.config` 为 `<exe>.config`，结束断言四件齐（缺＝明示报错 exit 非 0）。
**P3 Updater 单形态化（D4）**：删除 `ReplaceSequence`/`DownloadAndApply`/`DownloadAndVerify`（便携语义）/`SyncUninstallVersion`/`InstalledVersionOf`/`IsInstalledForm`/`ReadUninstallKeyPresent`/`UninstallKeyPath`/`DeleteStaleBackup`；`Check()` 改为恒要求 Setup 资产（缺失＝报错文案含 Setup 名）、SUMS 匹配名改 `ChecksumsAssetName`、`SetupAssetNameFor` 改引用 `AppInfo.SetupAssetNamePrefix`；`UpdateInfo` 收窄为 Setup＋SUMS 字段；预下载暂存/`Global\BlackScreen.Update` 互斥/`LaunchSetup`/`SetupSilentArguments` 保留不动（§7 表 3 冻结值）；类头注释改写如实反映单形态。
**P4 AppPaths（D6/D7）**：`IsManagedDataRoot` 增「trim 原始串含 `..`→拒绝」（不展开不归一化）；`CommitDataRoot` 对称化——非默认根写 DataDirectory＋DataParentDirectory（＝所选父），回默认两值全删（与 .iss [Code] 行为同构，§7 表 6/7）。
**P5 SettingsDialog 分支合一（D4）**：`BeginApplyUpdate` 删 `IsInstalledForm` 分叉与便携 ReplaceSequence 分支（:810-856 区域），恒走 LaunchSetup→`CompleteUpdateCheck(null)`→Close 链（文案「正在启动安装…」沿用）；预下载状态机其余零变化。
**P6 Program.cs**：删 `Main` 中 `Updater.DeleteStaleBackup` 调用（函数已删；.old 清理移交安装器 [InstallDelete]，§7 表 8）；遮罩提示行/节流检查逻辑零变化。
**P7 verify 重构（D8/D9）**：①`VerifyUpdate.cs`：阶段 2 删「TEMP 替换彩排/ReplaceSequence 竞争 50 轮/篡改 digest 对自替换拒绝」等以自替换为对象的用例，改单形态链断言（Check 缺 Setup 资产→报错；DownloadStaged 恒 Setup 名暂存；`ChecksumsAssetSums` 匹配与篡改拒绝；staging 清理断言保留）；阶段 3 双形态 probe 用例删；`SetupSilentArguments`/`SetupStartInfo` 逐字断言保留；mock 端点/JSON 构造随资产名常量走；**阶段 1 版本表 {1,2,0} 断言本轮不动**（I 轨联动）；②`VerifyPaths.cs`：解析矩阵增 `..` 拒绝行（含 `C:\x\..\BlackScreen` 用例）＋`CommitDataRoot` 双登记断言（改址→两值齐；回默认→全清）；③`VerifySettings.cs`：运行目标切到 `dist\一键黑屏-设置.exe`（六查断言零变化）；④`VerifyIcon.cs`：增两 exe 内嵌图标档位断言（`Icon.ExtractAssociatedIcon` 或等价），若实现成本超预期可在 VERIFY 申报降级理由；⑤`VerifyGate.cs:4` seven→eight；⑥`verify.cmd`：TARGET 显式分派（遮罩/idle 类→`一键黑屏.exe`，设置类→`一键黑屏-设置.exe`），删 glob。全部走 VerifyGate 闸门（AGENTS §2）。
**P8 留档推送**：`docs/handoff/2026-11-12-v11-singleform/VERIFY-V11A.md`（真实命令输出：build 四件断言、两 exe 行为实测〔设置 exe 直接开窗/主 exe 分派兼容〕、八件全绿、新断言输出、真实键/ini 三态快照、B/R/I 契约依赖申报）；提交 `app(A): V11 …`；`git push -u origin feature/app-v11`。

## 4. 铁律

写面白名单（对 main 的 diff 必须全部落入）：`src/AppInfo.cs`（**仅资产名常量增删，Version 字段零 diff**）、`src/SettingsProgram.cs`(新)、`src/Program.cs`、`src/Updater.cs`、`src/AppPaths.cs`、`src/SettingsDialog.cs`、`build.cmd`、`build.rsp`、`build.settings.rsp`(新)、`verify/VerifyUpdate.cs`、`verify/VerifyPaths.cs`、`verify/VerifySettings.cs`、`verify/VerifyIcon.cs`、`verify/VerifyGate.cs`、`verify.cmd`、`docs/handoff/2026-11-12-v11-singleform/VERIFY-V11A.md`(新)。白名单外一律只读。**禁改**：`setup/**`＋`build-setup.cmd`（B 轨）、`README.md`/`使用说明.txt`（R 轨）、`src/AppInfo.cs` 的 `Version`（I 轨）、`src/MaskConfig.cs`/`OverlayForm.cs`/`IdleWake.cs`/`ThemePalette.cs`/`MaskPainter.cs`/`SettingsDialogMetrics.cs`（本轮设置窗无新控件；确需动先在 VERIFY 移交注记申报理由）。禁补丁式修改——双入口重构融入既有 Main 结构、更新链删码后注释与类头如实改写，不为需求叠旁路。真实用户状态只读：HKCU 真实键/真实 ini 三态快照留档（L-012）。GUI 件撞 `VERIFY BUSY`＝等对方结束再跑（AGENTS §2）。主检出只读；证据＝真实命令输出；不删仓库、凭证不入文件。

## 5. 预期触碰文件（冲突面申报，供 B/R/I 预判）

与 B 轨零交集（src/verify/build.cmd·rsp vs setup/build-setup.cmd）；`verify.cmd`/`build.cmd` 仅本轨改。RESEARCH §7 表 1/2/4/5 行「A 侧」列由本轨实现，值不得偏离；`AppInfo.cs` 的 diff 必须仅含资产名常量增删行。

## 6. 验收清单（校验器逐条核对实据）

- [ ] `git ls-remote --heads origin feature/app-v11` 可见且指向本轨提交
- [ ] `git diff --name-status origin/main...HEAD` 全部落入 §4 白名单；`git diff origin/main...HEAD -- src/AppInfo.cs` 不含 `Version = "1.2.0"` 行改动
- [ ] `build.cmd` 绿：dist 恰四件（两 exe＋两 config；`icon-settings.ico` 不在 dist）；断言输出留档
- [ ] 行为实测：`dist\一键黑屏-设置.exe` 直接打开设置窗；`dist\一键黑屏.exe --settings` 兼容打开；`--idle`/无参行为不变
- [ ] `verify.cmd` 八件全绿（无 BUSY 绕闸）；VerifyUpdate 单形态断言/VerifyPaths 新矩阵行/VerifySettings 新目标输出留档
- [ ] `grep -n "ReplaceSequence\|SyncUninstallVersion\|IsInstalledForm\|InstalledVersionOf\|DeleteStaleBackup\|UpdateAssetName\|UninstallKeyPath" src/*.cs verify/*.cs` 零命中（死代码根除实证；VerifyUpdate 内历史断言名如残留＝重构未净）
- [ ] `grep -n "SHA256SUMS" src/*.cs` 仅 `ChecksumsAssetName` 定义与引用处命中（值＝`SHA256SUMS.txt`）
- [ ] `setup/**`、`build-setup.cmd`、`README.md`、`使用说明.txt` 零 diff
- [ ] 真实 `Software\BlackScreen` 键与真实 ini 前后三态一致快照（L-012）

## 7. 收尾动作

VERIFY-V11A.md 末尾附「移交注记」（与 RESEARCH §7 契约的任何偏差、`-resource` 图标资源去留结论、B/R/I 需重点核的边角、留给 I 轨真机冒烟的场景清单——含「旧 lnk 带 --settings 升级后兼容」）。推送后报告：分支名、提交哈希、验收清单逐条 ✅/❌。R 轨将复检你的全部产出并预合并。

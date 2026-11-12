# GOAL-V11B · 安装器侧：双 EXE 打包＋设置快捷方式直指＋卸载零残留兑现（B 轨）

> **一句话定位**：B 轨执行窗（GLM-5.3·目标模式）：setup/ 安装器侧全改——[Files] 双 EXE 四件（D2/D9 契约）、设置快捷方式免参数直指（D2）、卸载清扫更新状态值＋空键（D5）、默认根重定向隔离机制＋D-R7 真实覆盖（D10）、build-setup.cmd 产物名互锁（D3），独立 worktree 内完成并推送分支 `feature/installer-v11`。　**事实源＝[RESEARCH-V11.md](RESEARCH-V11.md)（§2.2 现状/§3 定案 D1-D5·D9-D10/§7 契约表），冲突以它为准。**　日期：2026-11-12　行数预算：≤125 行。

## 1. 身份与冷启动

你是 B 轨执行窗，产出落地改动与证据文档，不产出讨论。按序自读（全部在主检出 `D:\Download\Agent Vault\OneClickBlackScreen\OneClickBlackScreen-main\`）：1. `AGENTS.md`；2. `docs/handoff/2026-11-12-v11-singleform/RESEARCH-V11.md` 全文；3. `PLAN-V11.md` §2；4. 本文件；5. 现行脚本 `setup/OneClickBlackScreen.iss` 全文（本轮唯一产品写面）＋`build-setup.cmd`。**禁读**：archive 全文（grep 定位例外）、`baseline/**`、`migrate/**`、`src/**` 除 `src/AppInfo.cs` 的常量区（资产名契约值，grep 定位即可）。
读完先建 worktree（主检出根执行；命令照抄，git bash）：

```bash
cd "D:/Download/Agent Vault/OneClickBlackScreen/OneClickBlackScreen-main"
git fetch origin
git ls-remote --heads origin feature/installer-v11   # 必须输出为空；非空＝撞名，停下报告
git worktree add "../OneClickBlackScreen-WorkTree/v11-b" -b feature/installer-v11 origin/main
cd "../OneClickBlackScreen-WorkTree/v11-b"           # 本窗全部工作在此，主检出只读
```

## 2. 背景（为什么）

用户定案唯一 Setup 形态（RESEARCH D1/§2.1 fts 实证）＋双 EXE 免参数入口（D2）→ 安装包从「单 exe＋参数快捷方式」变为「双 exe 四件＋直指快捷方式」；README 公开的「卸载后无残留」承诺要求补清 `LastUpdateCheck`/`UpdateAvailable` 两值与空键（D5，V10 提请①）；V10 欠账 D-R7（无登记卸载删默认数据根的真实构建覆盖）经「默认根重定向」机制变得可测（D10）。与 A 轨零文件交集，接口冻结于 RESEARCH §7——**A 轨产物（双 exe）在你分支上不存在，自测一律用占位脚手架（§3 P6），build-setup.cmd 全链验证归 R 轨合并态**。

## 3. Phase 任务（依序）

**P1 [Files] 四件＋.old 回收（D2/§7 表 4/8）**：`AppExeName` 旁增 `#define AppSettingsExeName "一键黑屏-设置.exe"`（沿用 AppExeName 单源惯例，八处引用模式照旧）；[Files] 改恰五条＝`一键黑屏.exe`＋`一键黑屏.exe.config`＋`一键黑屏-设置.exe`＋`一键黑屏-设置.exe.config`＋`使用说明.txt`（**删 icon-settings.ico 行**）；新增 `[InstallDelete]`：`Type: files; Name: "{app}\*.old"`（1.2.0 便携更新遗留 .old 由安装器一次性回收）。
**P2 设置快捷方式直指（D2）**：[Icons]「一键黑屏-设置」改目标 `{app}\{#AppSettingsExeName}`，**删 `Parameters: "--settings"` 与 `IconFilename`**（设置 exe 已内嵌专属图标）；「一键黑屏」与桌面快捷方式零变化。
**P3 卸载零残留兑现（D5/§7 表 6）**：值名常量区增 `LastUpdateCheckValue='LastUpdateCheck'`、`UpdateAvailableValue='UpdateAvailable'`；`CurUninstallStepChanged(usUninstall)` 在现有删 DataDirectory/DataParentDirectory 处同段增删此两值（测试变体根 `-TEST` 同构）＋五值删尽后 `Software\BlackScreen`（生产）/`Software\BlackScreen-TEST`（测试）键空则 `RegDeleteKeyIfEmpty`——安装目录 InstallDirectory 沿 uninsdeletevalue 不动；确认弹窗文案不需改（数据知情已有）。
**P4 默认根重定向机制（D10）**：测试变体（`#ifdef TestId`）增「默认数据根重定向」——默认根常量/计算改为经 `/DDefaultDataRoot=<隔离路径>` 注入（生产构建不传该 /D＝仍 `%LOCALAPPDATA%\BlackScreen` 原值）；**移除 V10 的「无登记测试构建编译排除默认根删除」**（重定向后删真实根在构造上仍不可能，而默认根删除代码在测试构建中保持活跃）；`build-setup.cmd` 测试变体编译行同步传该 /D。
**P5 D-R7 覆盖场景＋四阶段自测（D10）**：隔离自测沿 V10 P5 四阶段框架复跑（①静默装断言四件落位＋InstallDirectory 登记＋默认零登记＋[Run] 抑制；②预置登记重跑＝向导改址登记切换；③契约参数覆盖更新 settings.ini 哈希保留；④a 守卫双场景拒载 exit1；④b /VERYSILENT /SUPPRESSMSGBOXES 卸载 Removed all? Yes＋六维零残留）并**新增场景⑤ D-R7**：重定向默认根下装（数据页不改＝零登记）→在（重定向）默认根造 `settings.ini`→静默卸载→断言该根被删＋五值＋空键全清。全程 TEMP/`-TEST` 隔离，真实用户状态零接触（L-012）。
**P6 占位脚手架（并行依赖说明）**：本分支 build.cmd 仍为旧单目标版——ISCC 自测所需的 §7 表 4 四件以 csc 现编两个最小占位 winexe（`-target:winexe` 空窗体即可）＋复制两份空 `.config` 凑齐，放本 worktree `dist\`（.gitignore L2 已挡，**不入提交**）；.iss 断言面（落位/快捷方式/登记/卸载清扫）与 exe 内容无关故占位可验，真实 exe 行为归 A verify＋I 真机。
**P7 build-setup.cmd（D3/§7 表 1）**：`:12` 注释改指 `docs\handoff\2026-11-12-v11-singleform\VERIFY-V11B.md`；期望产物名改为**从 `src/AppInfo.cs` 同时提取 `SetupAssetNamePrefix` 与 `Version` 拼接**（四方同源：与 .iss OutputBaseFilename 互锁，不一致＝明示报错）；其余探测/调用逻辑零变化。
**P8 留档推送**：`docs/handoff/2026-11-12-v11-singleform/VERIFY-V11B.md`（真实命令输出：两变体编译绿、五场景逐断言、快捷方式目标导出证〔安装后 .lnk 解析〕、三态快照、占位脚手架说明与不入提交证明）；提交 `installer(B): V11 …`；`git push -u origin feature/installer-v11`。

## 4. 铁律

写面白名单（对 main 的 diff 必须全部落入）：`setup/OneClickBlackScreen.iss`、`build-setup.cmd`、`docs/handoff/2026-11-12-v11-singleform/VERIFY-V11B.md`(新)。白名单外一律只读。**禁改**：`src/**`、`verify/**`、`verify.cmd`、`build.cmd`/`build.rsp`（A 轨）、`README.md`/`使用说明.txt`（R 轨）、AGENTS.md、archive/baseline/migrate。禁补丁式修改——重定向机制替换而非叠加 V10 的编译排除；值清扫融入 usUninstall 既有段；不为需求叠旁路。数据根删除恒守三重守卫（位置归属/形状/绝不碰父目录与同级文件）；真实用户状态只读（三态快照留档，L-012）；`%TEMP%` 外不落任何测试残留。占位件不入 git（提交前 `git status` 复核 dist 未跟踪）。证据＝真实命令输出；不删仓库、凭证不入文件。

## 5. 预期触碰文件（冲突面申报，供 A/R/I 预判）

与 A 轨零交集（setup/build-setup.cmd vs src/verify/build）；RESEARCH §7 表 4/5/6/8 行「B 侧」列由本轨实现，值不得偏离；`build-setup.cmd` 的产物名提取逻辑变更需 R 轨在合并态（A 轨 AppInfo 常量在场）全链复核。

## 6. 验收清单（校验器逐条核对实据）

- [ ] `git ls-remote --heads origin feature/installer-v11` 可见且指向本轨提交
- [ ] `git diff --name-status origin/main...HEAD` 恰 3 件全落白名单；`git status` 无 dist 占位件跟踪
- [ ] `grep -n "icon-settings\|--settings" setup/OneClickBlackScreen.iss` 零命中（打包面与快捷方式参数退出）
- [ ] `grep -n "AppSettingsExeName\|LastUpdateCheck\|UpdateAvailable\|InstallDelete\|DefaultDataRoot" setup/OneClickBlackScreen.iss` 各命中且语义正确（常量定义＋引用/清扫调用/回收段/重定向机制）
- [ ] 两变体（生产＋测试）ISCC 编译绿（真实输出留档）；测试变体含 `/DDefaultDataRoot`
- [ ] 隔离自测五场景 ALL-STAGES-PASS（含 D-R7 新场景逐断言输出；④b 六维零残留升级为含 LastUpdateCheck/UpdateAvailable/空键）
- [ ] 安装后快捷方式实证：「一键黑屏-设置」.lnk 目标＝`一键黑屏-设置.exe` 且参数空（导出解析留档）
- [ ] 真实 `%LOCALAPPDATA%\BlackScreen`、真实 HKCU `Software\BlackScreen`、计划任务三态前后一致（零足迹）
- [ ] `src/**`、`verify/**`、`verify.cmd`、`build.cmd`/`build.rsp`、`README.md`、`使用说明.txt` 零 diff

## 7. 收尾动作

VERIFY-V11B.md 末尾附「移交注记」（与 §7 契约任何偏差、重定向机制的边界说明〔生产构建默认根未变的证明〕、R 轨合并态需重点核的 build-setup 全链、I 轨真机场景建议——含「旧 lnk 带 --settings 由 A 轨主 exe 分派兼容」联测项）。推送后报告：分支名、提交哈希、验收清单逐条 ✅/❌。R 轨将复检你的全部产出并预合并。

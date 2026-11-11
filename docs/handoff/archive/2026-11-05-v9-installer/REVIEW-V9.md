# REVIEW-V9 · R 轨复检与预集成总证据

> **一句话定位**：R 轨对 A/B 两轨全部产出的独立复检实据（逐文件手术审＋.iss 对照 D2 逐参数勾验＋双白名单核对）＋ `feature/review-v9` 两次 `--no-ff` 零冲突预集成＋合并态全量验证（build/Setup/七件套/死链/grep 归零）。　**执行**：R 轨窗口（GLM-5.3·目标模式），worktree `OneClickBlackScreen-WorkTree/v9-r`，基线 `origin/main@31a0a34`。　**日期**：2026-11-05。事实源＝[RESEARCH-V9.md](RESEARCH-V9.md)。

## 1. 背景（本轮为什么存在）

A（DeskBox 根除，动了冻结区）/ B（安装包，动了 Updater.cs 与分发面）双轨并行完成并推送——两者均高敏感，合入发版前须逐项实证。R 轨职责＝合并前把质量关＋预集成暴露冲突。前置检查：`git ls-remote` 见 `feature/deskbox-purge-v9@559aba1` 与 `feature/installer-v9@1583617`，`feature/review-v9` 无撞名。

## 2. P3 双白名单核对（NO-GO 判据，先审）

- **A**：`git diff --name-status origin/main...origin/feature/deskbox-purge-v9` ＝ 17 件（archive 11 M＋lessons M＋install.ps1 D＋migrate 3 M＋VERIFY-V9A A），与 GOAL-V9A §4 白名单**精确相等**，白名单外零路径。
- **B**：同法 ＝ 6 件（setup/OneClickBlackScreen.iss A＋build-setup.cmd A＋README M＋使用说明 M＋Updater.cs M＋VERIFY-V9B A），与 GOAL-V9B §4 白名单**精确相等**。
- **A∩B＝∅**——RESEARCH D6 写面零相交申报属实（零冲突合并的充分前提）。

## 3. P1 A 轨逐文件手术审（五项全 PASS）

- **①diff 逐件审**（独立跑 `git diff origin/main...origin/feature/deskbox-purge-v9`，对照 VERIFY-V9A 附录 A 一致）：141 条 `-` 行中非 DeskBox 字样者 93 条，逐条归属三类无第四类——install.ps1 整删（57 行，白名单动作）；cleanup-local.ps1 S2 节整删的内部实现行；S3~S7→S2~S6 节号顺移。**归一实证**：两版 cleanup-local.ps1 将 `S[1-9]` 归一为 `SN` 后 diff **恰剩 S2 节两处整删**（:18 注释行＋:94-109 实现节）——顺移是纯标签替换、语义零变化、无顺手改。archive 11 份均为行内最小改写或整行删（句法自洽，如「项目文件夹、DeskBox 两个快捷方式、`%LOCALAPPDATA%\BlackScreen` 都已消失」→去中段），迁移手册 md/html 三处行内改写对应。
- **②grep 自证**：`git grep -li deskbox <A分支> -- ':!批次目录' ':!使用说明.txt'` → **exit 1 零命中**（A 轨授权写面 100% 归零）；全仓口径剩批次目录 7 份＋使用说明.txt 1 处，与 A 裁定 2/移交注记 2 申报一致。
- **③lessons**：diff 零 `-` 行（纯追加）；L-017 五要素齐全（现象/根因/正解/出处/防再犯）且自身不含目标字样；锚点清单 11 文件 ∩ A 改动 17 文件 ＝ **∅**（`comm -12` 空输出）——坑库锚点零漂移。
- **④节编号自洽**：手术后头部清单 S1~S6 与实现段一一对应、连续；旧 S2 catch 里的 `$s1ok = $false` 随节删除，S1 设置／S4（原 S5）检查的联动语义保留。
- **⑤编码字节级**：blob 级 BOM 唯一 `efbbbf`（cleanup-local.ps1）old=new 一致，其余手术件均无 BOM 一致；CR 全 0（入库 LF 归一）；cleanup LF 183→166 **恰减 17 行**（＝删除量），迁移手册 md/html LF 531/585 不变（纯行内）。合并态工作区 `file` 复验：cleanup=UTF-8 **BOM**+CRLF、迁移手册.html 长行 62543、GOAL-V1 长行 424、MIGRATE-V5D 长行 322——与 A 轨 VERIFY §4 申报逐项一致（L-006 编码坑未复发）。

## 4. P2 B 轨复检（五项全 PASS）

**①.iss 对照 RESEARCH §3 D2 逐参数勾验（11/11 ✅）**：

| D2 定案 | .iss 落点 | 判 |
|---|---|---|
| PrivilegesRequired=lowest | [Setup] 原样 | ✅ |
| DefaultDirName={autopf}\一键黑屏 | [Setup] 原样 | ✅ |
| 固定 AppId | `#define RealAppId "{{5A17668F-…}}"`→`AppId={#PublicAppId}` | ✅ |
| 安装四件 | [Files] 恰四条（exe/config/ico/使用说明），无 ini 预置 | ✅ |
| 开始菜单×2＋设置 `--settings` | [Icons] {group} 两入口＋icon-settings.ico 图标 | ✅ |
| 桌面 opt-in | [Tasks] `desktopicon Flags: unchecked`＋`Tasks: desktopicon` | ✅ |
| [Messages] 中文化零 .isl | 27 条中文（含 UninstalledAll 设置保留提示） | ✅ |
| 卸载清任务容错 | [UninstallRun] schtasks /delete runhidden（Inno 不查退出码） | ✅ |
| 不删用户设置 | 无任何涉 BlackScreen 目录的删除；完成页提示其位置 | ✅ |
| 同 AppId 覆盖升级 | 固定 AppId＋注释明示 in-place 语义 | ✅ |
| TestId 变体入口 | `#ifdef TestId`→测试 AppId/-TEST 后缀/测试任务名 | ✅ |

- **②版本单源**：build-setup.cmd 以 `findstr "public const string Version = "`＋引号分隔提取→`/DAppVersion`；.iss `#ifndef AppVersion #error` 拒裸编译；`src/AppInfo.cs` 零 diff（B 白名单核对实证）；产物名 `OneClickBlackScreen-Setup-1.0.2.exe`（B 实录＋R 合并态重建双证）。
- **③Updater ARP 联动**（D3）：`SyncUninstallVersion` 调用点恰在 `File.Copy` 成功后、`finally`（ReleaseMutex）前；字节相同早退在 swap 前 `return`（不写）、Copy 失败 throw 回滚（跳过）；`OpenSubKey(...,true)` null→return＝键存在才写；全程 try/catch best-effort（swap 已成功，ARP 失败不反噬）；写入值经反射读**新落位文件**的 `AppInfo.Version` 常量（`IsLiteral` 校验，杜绝写旧值）；GUID 与 .iss RealAppId 字节一致（两文件互指注释）；`using Microsoft.Win32` 零新增引用。VerifyUpdate 全 PASS＝no-op 实证（B 实录＋R 合并态复跑双证）——**无补丁式旁路，融入 ReplaceSequence 既有结构**。
- **④分发面文档**：README 三列表（Setup 推荐/zip 便携/裸 exe）＋卸载语义与 .iss 一致＋v1.0.2 token 逐字保留（I 轨联动 1.1.0）；使用说明【安装】/【两种启动方式】/【边界】/卸载两径/【开发者提示】五处改写自洽。独立终检 `git grep -iE "install\.ps1|deskbox" -- README.md 使用说明.txt` → **exit 1 零命中**。
- **⑤隔离自测实录**：B1~B7 基线（ini/双任务/autopf/开始菜单/桌面三哈希/双键）→C1~C4 装后（TEMP 6 件载荷、测试键 DisplayVersion=1.0.2、**真实面三哈希逐字节等于基线**、真实键/任务/ini 全 ABSENT）→D1~D2 卸后零残留＋三态终态全等——全程不触用户真实状态；首轮 /NOICONS 缺陷发现→预处理层裁剪根修（.iss 防回归注释在案）。

## 5. P4 预集成实录（两次 --no-ff，零冲突）

`git merge --no-ff origin/feature/deskbox-purge-v9` → **ee5f92e**（17 files）；`git merge --no-ff origin/feature/installer-v9` → **32dd66a**（6 files）。**零冲突＝写面零相交申报属实**。合并态 vs main 总 diff 恰 23 件＝17∪6（无第三者改动）；父链 `32dd66a(ee5f92e,1583617)`、`ee5f92e(31a0a34,df57ad5)` 双父实证。

## 6. P5 合并态全量验证（全绿）

- `build-setup.cmd` exit 0（内含 build.cmd 全新构建；ISCC 7.1.1 编译 1.672s；产物 `dist\OneClickBlackScreen-Setup-1.0.2.exe`）。
- `verify.cmd` **七件全绿**（R 轨独占时段直跑，exit 0，56 条 PASS，`[OK] ALL VERIFIERS PASSED`，无 BUSY 撞闸）：Config 23 查／Settings 6 查（含 Z 零足迹）／MouseExit 5 按钮／Overlay A-H（InputIdle 209ms）／Icon 4 查／Update 全链含并发×50（ARP 联动合并态实证）／Idle 5 查。Z 查×2＝用户真实 settings.ini SHA-256 前后一致。
- **grep 归零复证**：合并态全仓命中仅批次目录 8 份（指令/证据文档自引用 114 处）；`--exclude-dir=2026-11-05-v9-installer` 后 **exit 1 零命中**——产品码/活文档/迁移/archive 全部归零（轮目标达成）。
- **死链复扫**（子代理扇出，70 份 md/94 条相对链接）：**活文档零死链**；B 修的 README 链接（v9 批次目录/archive/lessons/multi-window/AGENTS）全部有效；archive 内 16 条既存死链＝V8 归档「目录深一层」系统性 `../` 失效（含 PLAN-V8→AGENTS 一条），**main 上既有、非本轮引入**，冻结区不代修（见移交注记 R-1）。

**踩坑**：本轮 R 轨无新坑入库；复检引既有坑——L-006（编码保真，§3-⑤ 复验 BOM/CRLF 未复发）、L-015（资产名 ASCII，§4-② 产物名实证）、L-017（A 轨本轮新增，§3-③ 五要素复验）。

## 7. 裁定汇总（决策与否决）

1. **B 越字面两处支持**：README 开发过程文档表死链修复＋V1~V8 行数更新——合目的（死链正是 R 轨门禁要抓的）且在 B 对 README 独占写面内。
2. **L-017 措辞支持**（A 移交 5）：「字面不入库」与轮目标 grep 归零一致；GOAL 原文文案与归零定义不可同时满足，取归零。
3. **grep 口径裁定**：「归零」＝批次目录指令文档自引用外全仓零命中（A 裁定 2 惯例延续）；批次目录字样去留提请 V10（A 移交 1）。
4. **archive 死链不代修**：冻结区（AGENTS §4）＋main 既存——否决本轮修复，移交 V10 定政策。
5. **IS7 接受**（B 移交 2）：.iss 参数按 IS7 语义勾验通过，编译实证两轮。

## 8. 移交注记处置表

| # | 来源 | 处置 |
|---|---|---|
| A-1 批次目录自引用 114 处 | VERIFY-V9A 移交 1 | 知悉；提请 V10 与 D8 归档惯例一并定夺（本轮轮目标已达成） |
| A-2 使用说明.txt:105 | 移交 2 | **闭环**——B 轨已消除，合并态实证零命中 |
| A-3 迁移手册 S 编号旧引用（:485/:493） | 移交 3 | 备案——保持历史原文（与 archive 同理，冻结区） |
| A-4 迁移手册步骤 5.3 install.ps1 引用 | 移交 4 | 备案——同上 |
| A-5 L-017 措辞 | 移交 5 | **闭环**——R 裁定支持（§7-2） |
| A-6 SHA256SUMS.txt | 移交 6 | **闭环**——合并态零变化实证 |
| B-1 七件套通道 | VERIFY-V9B 移交 1 | **闭环**——R 轨代理通道直跑成功，双轮独立全绿 |
| B-2 IS7 vs IS6 | 移交 2 | **闭环**——§7-5 |
| B-3 真实安装快捷方式三处＋卸载自清 | 移交 3 | **移交 I**——真机冒烟重点核 |
| B-4 README 两行越字面 | 移交 4 | **闭环**——§7-1 |
| B-5 使用说明标题「v5」 | 移交 5 | **移交 I**——1.1.0 文档联动裁量 |
| B-6 ARP 反射锁极端场景 | 移交 6 | 备案——既有回滚链覆盖 |
| R-1 archive 16 条层级死链 | 本轮死链复扫 | 提请 V10：授权冻结区手术批量改写，或立「归档冻结＝历史原样」政策 |
| R-2 GOAL-V9I→REVIEW-V9.md 链接 | 死链复扫 | **闭环**——本文件提交后自愈 |

## 9. 总评与提交链

**总评：GO（V9R）**——A/B 双轨全部产出逐项实证通过，无结构性缺陷，预集成零冲突，合并态全量验证全绿。I 轨可合入发版（真机冒烟重点：B-3/B-5）。下一步＝GOAL-V9I（合并 review-v9→AppInfo 1.1.0→完整发版链）。

```bash
# I 轨快速启动（主检出执行；撞名即停）
cd "D:/Download/Agent Vault/OneClickBlackScreen/OneClickBlackScreen-main"
git fetch origin && git ls-remote --heads origin feature/integration-v9  # 必须为空
git worktree add "../OneClickBlackScreen-WorkTree/v9-i" -b feature/integration-v9 origin/main
cd "../OneClickBlackScreen-WorkTree/v9-i"
git merge --no-ff origin/feature/review-v9   # 本分支为 I 轨合入对象
```

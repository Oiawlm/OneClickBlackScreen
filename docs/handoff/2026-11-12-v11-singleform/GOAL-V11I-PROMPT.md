# GOAL-V11I · 集成发版 v1.3.0＋真机五场景终验（I 轨）

> **一句话定位**：I 轨执行窗（GLM-5.3·目标模式）：合并 feature/review-v11 进 main、版本单源联动 1.3.0（AppInfo＋VerifyUpdate 版本表＋文档版本引用核对）、tag＋Release 恰两资产（Setup＋SHA256SUMS.txt，D1/D3）、真机五场景冒烟（用户实装原地升级/设置 EXE 双入口/D-R7 真机/1.2.0 便携迁移实演/隔离完整卸载零残留）、worktree/分支清理，VERIFY-V11.md 留档。　**事实源＝[RESEARCH-V11.md](RESEARCH-V11.md)（§3 定案/§4 I 轨概要/§7 契约表），冲突以它为准。**　日期：2026-11-12　行数预算：≤120 行。

## 1. 身份与冷启动

你是 I 轨集成发版窗，产出合并＋发版＋终验证据，不产出讨论。**在主检出 `D:\Download\Agent Vault\OneClickBlackScreen\OneClickBlackScreen-main` 直接工作**（V9 裁定：主检出直改不建 worktree；集成窗独占主检出写权）。按序自读：1. `AGENTS.md`；2. `docs/handoff/2026-11-12-v11-singleform/RESEARCH-V11.md` 全文；3. `REVIEW-V11.md`（含契约核对表与移交注记）；4. 本文件；5. 发版惯例：`docs/handoff/archive/2026-11-11-v10-lifecycle/VERIFY-V10.md`（上一份 I 轨总证据，只看流程骨架与事故注记——**V10 批次已归档，路径在 archive 下**）。
**前置检查**：`git ls-remote --heads origin feature/review-v11` 可见；主检出 `git status` 干净、当前分支 main 且与 origin/main 同步——任一不符＝停下报告。

## 2. 背景（为什么）

A（双 EXE＋单形态更新链根修）＋B（打包面＋卸载清欠）已经 R 轨复检与预集成。本轮**有产品代码大改**→按 AGENTS §5 红线必走完整发版链：版本单源 1.3.0→tag→Release→应用内冒烟。本轮独特终验＝**形态变更的迁移面**：用户真实安装（`D:\App\一键黑屏` 1.2.0）原地升级到 1.3.0 是最真实的双 EXE 落位＋ARP 碰撞根除实证；1.2.0 便携态按 D1 一次性迁移指引实演；卸载零残留承诺（含新清扫的两值）在隔离目录终验。**用户实装升级后保留、不卸载**（沿 V10 S7 惯例）。

## 3. Phase 任务（依序）

**P1 合并**：`git merge --no-ff origin/feature/review-v11 -m "merge: V11 单一Setup形态+双EXE+收官清欠合入 main"`；冲突＝停下逐项裁定（RESEARCH 为准）留档。
**P2 版本单源联动**（本轮唯一版本改动点）：①`src/AppInfo.cs` `Version` 1.2.0→1.3.0（资产名常量不动）；②`verify/VerifyUpdate.cs` 版本表联动（V9 D-1 纪律）：镜像 {1,3,0}、前版 1.2.0 转 older 用例、newer 用例换 v1.3.1/v1.4.0、等值/bare/whitespace/-rc1/大写 V 随版本走；③文档版本引用核对（R 轨已直写 1.3.0）：README tag 链接/Setup 名/certutil 例/页脚＋使用说明标题行与 v10 更新行——grep 全部指向 1.3.0。全仓 `grep -rn "1\.2\.0" src/ verify/` 零命中后进入 P3（VerifyUpdate 存留恰 2 处＝older＋malformed 用例）。
**P3 构建与验证**：`build.cmd` 绿（四件断言）→`build-setup.cmd` 绿（产物 `dist\OneClickBlackScreen-Setup-1.3.0.exe`，名与 .iss/AppInfo 互锁）→`verify.cmd` 八件全绿×2 轮（GUI 件勿动键鼠；撞 BUSY 等待）→零足迹三态（真实 ini/真实键/计划任务前后快照字节级一致，L-012）。
**P4 tag＋Release**：`git push origin main`→`git tag v1.3.0`＋推送→GitHub Release `v1.3.0`（name=v1.3.0/Latest/非 pre-release/非 draft；body 四节沿 v1.2.0 实例改写：更新内容〔单一安装包形态/双 EXE 免参数设置入口/卸载零残留清欠/1.2.0 便携用户迁移指引〕/资产清单/校验说明/安装与卸载说明）→**上传恰两资产**：`OneClickBlackScreen-Setup-1.3.0.exe`＋`SHA256SUMS.txt`（ASCII 名，L-015；SUMS 格式 `<64位小写hex>␣*<文件名>`，含 Setup 行）→本地 certutil 与 Release 页面/上传源三方 digest 全 MATCH 留档。
**P5 真机五场景冒烟**（逐场景留证；动用户机器前在报告明示）：
①**用户实装原地升级**（`D:\App\一键黑屏` 1.2.0）：设置窗检查更新→发现 1.3.0→预下载校验→「立即更新」→静默重装→断言：**新增 `一键黑屏-设置.exe` 落位**、主 exe 字节＝1.3.0 资产、**ARP DisplayVersion=1.3.0（碰撞根除实证，V10 S3 反例对照留档）**、自定义数据根与 settings.ini 哈希不变、[Run] 自动重启遮罩；
②**设置 EXE 双入口**：新开始菜单「一键黑屏-设置」lnk 直指设置 exe 且**零参数**开窗（导出 .lnk 解析留档）；双击 `D:\App\一键黑屏\一键黑屏-设置.exe` 直接开窗；旧 lnk（带 `--settings` 指主 exe，若升级后残留）兼容开窗；
③**D-R7 真机抽查**（B 轨隔离自测已全场景覆盖，真机做一次确认即可）：隔离目录装（数据页不改）→默认根造数据→卸载→默认数据根删除断言；
④**1.2.0 便携迁移实演**：TEMP 摆 Release v1.2.0 zip 便携件→设置窗检查更新→报「未包含更新资产 OneClickBlackScreen.exe」（诚实失败留档）→按文档指引起 1.3.0 Setup→装→数据根内容继承断言；
⑤**完整卸载零残留终验**（隔离目录自装自卸；**用户实装保留**）：ARP 卸载 1.3.0→全盘扫描逐项 ABSENT 留档：安装目录、数据根、HKCU `Software\BlackScreen` **五值＋空键**、Uninstall 键、计划任务、开始菜单组、桌面 lnk、`*.old`。截图用窗口级捕获（L-016）。
**P6 清理**：worktree `v11-a/b/r` 移除＋prune；分支 `feature/app-v11`/`feature/installer-v11`/`feature/review-v11` 本地删除＋远端逐支 `merge-base --is-ancestor` 证明后删；`ls-remote` 终态仅 main。
**P7 留档**：`docs/handoff/2026-11-12-v11-singleform/VERIFY-V11.md`（发版总证据：合并记录＋版本联动 grep＋verify×2＋零足迹三态＋Release 两资产三方 digest＋P5 五场景实录＋清理实录＋REVIEW 移交注记处置表＋裁决节＋事故注记）；提交 `release(I): v1.3.0 …`＋`git push`（main＋tag）。

## 4. 铁律

写面白名单（本轮对 main 的全部 diff 必须落入）：`src/AppInfo.cs`（仅 Version 字段）、`verify/VerifyUpdate.cs`（仅版本表）、`README.md`/`使用说明.txt`（仅版本引用联动核对，行为文案归 R 轨）、`docs/handoff/2026-11-12-v11-singleform/VERIFY-V11.md`(新)，以及 P1 合并引入的 A/B/R 轨已审 diff。其余一律只读。**禁改**：`setup/**`、`src/**` 其余、`verify/**` 其余、`verify.cmd`、`build*`、AGENTS.md、archive/baseline/migrate。发现 R 轨遗漏缺陷：小＝修复留档于 VERIFY 裁决节（落入白名单才可动手）；大＝停下报告用户裁决。真实用户状态只读（零足迹三态是硬验收项）；真机冒烟**动用户机器前在报告里明示**（S1 将对 `D:\App\一键黑屏` 做应用内原地升级、S3/S5 只在隔离目录装卸）；`migrate\cleanup-local.ps1` 永不执行；凭证不入文件（Release 上传走用户已配置的 gh 凭证）；不删仓库、不改写远端历史。

## 5. 预期触碰文件（冲突面申报）

版本联动三处＋VERIFY-V11.md 为本轨独占写面；其余 diff 全部来自 review-v11 已审合并。

## 6. 验收清单（校验器逐条核对实据）

- [ ] `git log --oneline -1` 为 release 提交且 main 已推送；`git ls-remote --tags origin v1.3.0` 可见
- [ ] `grep -n "Version = " src/AppInfo.cs` 唯一命中 "1.3.0"；全仓 `grep -rn "1\.2\.0" src/ verify/` 零命中（VerifyUpdate older/malformed 恰 2 处除外，逐处申报）
- [ ] `dist\OneClickBlackScreen-Setup-1.3.0.exe` 存在；`verify.cmd` 八件全绿×2 输出留档
- [ ] Release v1.3.0 **恰两资产**（Setup＋SHA256SUMS.txt）且三方 digest MATCH 留档
- [ ] VERIFY-V11.md 含 P5 五场景逐条实录（S1 双 exe 落位＋ARP 1.3.0 对照、S2 .lnk 零参数解析、S4 诚实失败报错原文、S5 五值＋空键 ABSENT 清单）
- [ ] 零足迹三态快照一致；用户实装三登记与数据根在 S1 后完好（保留不卸）
- [ ] worktree×3 清理＋分支（本地 3＋远端 3）ancestor 证明后删除、终态仅 main
- [ ] `git diff --name-status <R轨推送点>..HEAD` 与白名单＋已审合并一致（无计划外改动）

## 7. 收尾动作

VERIFY-V11.md 末尾：移交注记处置表（闭环/备案/提请 V12 三栏）；V12 规划提示（本轮收尾后如：安装向导文案用户测试反馈、Setup 体积优化、CI 化发版）。批次目录由 V12 规划窗归档（沿 D14 惯例）。推送后报告：提交哈希、tag、Release URL、验收清单逐条 ✅/❌、真机五场景结论。

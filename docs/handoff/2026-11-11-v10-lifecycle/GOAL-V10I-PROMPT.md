# GOAL-V10I · 集成发版 v1.2.0＋真机全生命周期终验（I 轨）

> **一句话定位**：I 轨执行窗（GLM-5.3·目标模式）：合并 feature/review-v10 进 main、版本单源联动 1.2.0（AppInfo＋VerifyUpdate 版本表＋文档版本引用）、tag＋Release 四资产、真机全生命周期冒烟（装 v1.1.0→改数据目录→应用内更新到 v1.2.0→完整卸载→全盘零残留）、worktree/分支清理，VERIFY-V10.md 留档。　**事实源＝[RESEARCH-V10.md](RESEARCH-V10.md)（§3 定案/§4 I 轨概要/§7 契约表），冲突以它为准。**　日期：2026-11-11　行数预算：≤125 行。

## 1. 身份与冷启动

你是 I 轨集成发版窗，产出合并＋发版＋终验证据，不产出讨论。**在主检出 `D:\Download\Agent Vault\OneClickBlackScreen\OneClickBlackScreen-main` 直接工作**（V9 裁定：主检出直改不建 worktree；集成窗独占主检出写权）。按序自读：1. `AGENTS.md`；2. `docs/handoff/2026-11-11-v10-lifecycle/RESEARCH-V10.md` 全文；3. `REVIEW-V10.md`（含契约核对表与移交注记）；4. 本文件；5. 发版惯例：`docs/handoff/archive/2026-11-05-v9-installer/VERIFY-V9.md`（上一份 I 轨总证据，只看流程骨架与事故注记）。
**前置检查**：`git ls-remote --heads origin feature/review-v10` 可见；主检出 `git status` 干净、当前分支 main 且与 origin/main 同步——任一不符＝停下报告。

## 2. 背景（为什么）

A（应用侧）＋B（安装器侧）已经 R 轨复检与预集成（契约同构已证）。本轮**有产品代码大改**（更新器/配置层/安装器）→ 按 AGENTS §5 红线必走完整发版链：版本单源 1.2.0→tag→Release→应用内冒烟。V10 的独特终验＝**全生命周期冒烟**：从用户视角走完「装旧版→自定义数据目录→收到更新→秒装升级→完整卸载→零残留」闭环，六需求一次验收。

## 3. Phase 任务（依序）

**P1 合并**：`git merge --no-ff origin/review-v10 -m "merge: V10 更新链+数据目录+完整卸载合入 main"`；冲突＝停下逐项裁定（RESEARCH 为准）留档。
**P2 版本单源联动**（本轮唯一版本改动点）：①`src/AppInfo.cs` 1.1.0→1.2.0；②`verify/VerifyUpdate.cs` 版本表联动（V9 D-1 教训：镜像 {1,2,0}、前版 1.1.0 转 older 用例、等值/whitespace/-rc1 用例随版本走）；③`README.md` 版本引用四处在 R 轨文案基础上联动（tag 链接/Setup 名/zip 名与显示名/certutil 例）；④`使用说明.txt` 版本行：标题对齐＋新增「v9 更新（对应程序版本 1.2.0）」行（六需求一句话清单），页脚版本引用同步。全仓 `grep -rn "1\.1\.1\|1\.0\.2" src/ verify/` 零命中后进入 P3。
**P3 构建与验证**：`build.cmd` 绿→`build-setup.cmd` 绿（产物 `dist\OneClickBlackScreen-Setup-1.2.0.exe`）→`verify.cmd` 八件全绿×2 轮（GUI 件勿动键鼠；撞 BUSY 等待）→零足迹三态（真实 ini/真实键/计划任务前后快照字节级一致，L-012）。
**P4 tag＋Release**：`git tag v1.2.0`（先推 main、确认 CI/无 CI 则直接 tag）→GitHub Release `v1.2.0`（name=v1.2.0/Latest/非 pre-release/非 draft；body 四节沿 v1.1.0 实例：更新内容/资产清单/校验说明/安装与卸载说明）→上传四资产（Setup-1.2.0.exe＋OneClickBlackScreen.exe＋zip＋SHA256SUMS，ASCII 名，L-015）→本地 certutil 双向 digest 对 Release 页面值全 MATCH 留档。
**P5 真机全生命周期冒烟**（六需求终验，逐场景留证）：
①**安装 v1.1.0**：真机运行 v1.1.0 Setup（现有 Release 下载）→ARP 显示 1.1.0→自定义**安装目录**（改默认）与**数据位置**（改父目录）→断言 HKCU 登记（InstallDirectory/DataDirectory/DataParentDirectory）与文件落位；
②**使用与设置**：设置窗改遮罩颜色保存→断言 settings.ini 落在**自定义数据根**；
③**应用内更新到 v1.2.0**：打开设置窗→静默检查发现 v1.2.0→预下载完成→点「立即更新」→断言：Setup 静默启动、旧进程退出、文件刷新为 1.2.0（ARP DisplayVersion）、**自定义数据根与设置内容不变**、[Run] 自动重启行为符合设计；遮罩场景另验：启动遮罩→右下角提示行呈现（可用 ①中旧版本提前触发）；
④**便携形态回归**：zip 便携目录跑 exe→设置窗检查更新→exe 自替换路径正常（ReplaceSequence 链路）；
⑤**完整卸载**：ARP 卸载 v1.2.0→**全盘零残留扫描**：自定义安装目录、默认目录、新旧两个数据根、HKCU（Uninstall 键＋Software\BlackScreen 全部值）、计划任务、开始菜单组、桌面快捷方式——逐项断言不存在；
⑥**卸载器归属守卫实机验证**（如 REVIEW 移交注记指引可行）：残留旧位置卸载器场景拒绝执行实证。全部真实操作截图/命令输出留档（截图用窗口级捕获，L-016）。
**P6 清理**：worktree `v10-a/b/r` 移除＋prune；分支 `feature/app-v10`/`feature/installer-v10`/`feature/review-v10` 本地删除＋远端逐支 `merge-base --is-ancestor` 证明后删；`ls-remote` 终态仅 main。
**P7 留档**：`docs/handoff/2026-11-11-v10-lifecycle/VERIFY-V10.md`（发版总证据：合并记录＋版本联动 grep 证据＋verify×2 输出＋零足迹三态＋Release 资产双向 digest＋P5 六场景实录＋清理实录＋REVIEW 移交注记处置表＋裁决节＋事故注记）；提交 `release(I): v1.2.0 …`＋`git push`（main＋tag）。

## 4. 铁律

写面白名单（本轮对 main 的全部 diff 必须落入）：`src/AppInfo.cs`、`verify/VerifyUpdate.cs`（仅版本表）、`README.md`/`使用说明.txt`（仅版本引用联动，行为文案归 R 轨）、`docs/handoff/2026-11-11-v10-lifecycle/VERIFY-V10.md`(新)，以及 P1 合并引入的 A/B/R 轨已审 diff。其余一律只读。**禁改**：`setup/**`、`src/**` 其余文件、`verify.cmd`、AGENTS.md、archive/baseline/migrate。发现 R 轨遗漏缺陷：小＝修复留档于 VERIFY 裁决节（落入白名单才可动手）；大＝停下报告用户裁决。真实用户状态只读（零足迹三态是硬验收项）；真机冒烟涉及用户机器实装——**动用户机器前在报告里明示将安装/卸载的内容**；`migrate\cleanup-local.ps1` 永不执行；凭证不入文件（Release 上传走用户已配置的 gh 凭证）；不删仓库、不改写远端历史。

## 5. 预期触碰文件（冲突面申报）

版本联动四处与 VERIFY-V10.md 为本轨独占写面；其余 diff 全部来自 review-v10 已审合并。

## 6. 验收清单（校验器逐条核对实据）

- [ ] `git log --oneline -1` 为 release 提交且 main 已推送；`git ls-remote --tags origin v1.2.0` 可见
- [ ] `grep -n "Version = " src/AppInfo.cs` 唯一命中 "1.2.0"；全仓 `grep -rn "1\.1\.0" src/ verify/` 零命中
- [ ] `dist\OneClickBlackScreen-Setup-1.2.0.exe` 存在（ASCII 名）；`verify.cmd` 八件全绿×2 输出留档
- [ ] Release v1.2.0 四资产上传且本地 certutil 与页面 digest 双向 MATCH 留档
- [ ] VERIFY-V10.md 含 P5 六场景逐条实录（含自定义数据根在更新后内容不变的哈希对照、卸载后全盘零残留逐项清单）
- [ ] 零足迹三态快照（真实 ini/键/任务前后一致）；worktree×3 清理＋分支（本地 3＋远端 3）ancestor 证明后删除、终态仅 main
- [ ] `git diff --name-status <R轨推送点>..HEAD -- setup/ src/AppInfo.cs src/Updater.cs src/AppPaths.cs verify.cmd` 与白名单＋已审合并一致（无计划外改动）

## 7. 收尾动作

VERIFY-V10.md 末尾：移交注记处置表（闭环/备案/提请 V11 三栏）；V11 规划提示（如：遮罩提示行文案打磨、安装向导文案用户测试、ets/304 缓存可选项）。推送后报告：提交哈希、tag、Release URL、验收清单逐条 ✅/❌、真机冒烟六场景结论。

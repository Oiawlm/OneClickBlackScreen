# GOAL-V11R · 复检预集成＋行为文档单形态改写（R 轨）

> **一句话定位**：R 轨执行窗（GLM-5.3·目标模式）：A/B 双分支 `--no-ff` 预集成出 `feature/review-v11`、RESEARCH §7 契约八行双侧取证、合并态 build-setup 全链＋verify 八件全绿、README/使用说明 单形态大改写（含安装目录确切路径与 1.2.0 便携迁移指引），REVIEW-V11.md 六字段留档。　**事实源＝[RESEARCH-V11.md](RESEARCH-V11.md)（§3 定案/§7 契约表），冲突以它为准。**　日期：2026-11-12　行数预算：≤115 行。

## 1. 身份与冷启动

你是 R 轨复检窗，产出复检证据与预集成分支，不产出讨论。按序自读（全部在主检出 `D:\Download\Agent Vault\OneClickBlackScreen\OneClickBlackScreen-main\`）：1. `AGENTS.md`；2. `docs/handoff/2026-11-12-v11-singleform/RESEARCH-V11.md` 全文；3. `PLAN-V11.md`；4. `VERIFY-V11A.md`＋`VERIFY-V11B.md`（两轨证据，含各自移交注记）；5. 本文件。**禁读**：archive 全文（grep 定位例外）、`baseline/**`、`migrate/**`。
读完先建 worktree（主检出根执行；命令照抄，git bash）：

```bash
cd "D:/Download/Agent Vault/OneClickBlackScreen/OneClickBlackScreen-main"
git fetch origin
git ls-remote --heads origin feature/review-v11     # 必须输出为空；非空＝撞名，停下报告
git worktree add "../OneClickBlackScreen-WorkTree/v11-r" -b feature/review-v11 origin/main
cd "../OneClickBlackScreen-WorkTree/v11-r"          # 本窗全部工作在此，主检出只读
```

## 2. 背景（为什么）

A（双 EXE＋单形态更新链）与 B（打包面＋卸载清欠）已各自推送并隔离自证；本轮把它们**预集成**到一个分支并做契约级复检——§7 八行契约是 A∥B 并行时的冻结接口，双侧实现必须逐行同构取证（V10 REVIEW §7 模式）。行为文档（README/使用说明）自 V9 起归 R 轨写面：本轮因「单一 Setup 形态＋双 EXE」是**用户可见的形态变更**，文档改写幅度为历轮最大；合并态还是 build-setup.cmd 全链（真四件产物→ISCC）首次可验证的地方（B 轨自测用占位件）。合并态产物版本仍 1.2.0（AppInfo 未 bump＝I 轨职责）；文档版本引用按 RESEARCH D11 定案**直写 1.3.0**，I 轨 P2 以 grep 验收一致性。

## 3. Phase 任务（依序）

**P1 预集成**：`git merge --no-ff origin/feature/app-v11 -m "merge: V11 A 轨应用侧合入 review"`；再 `git merge --no-ff origin/feature/installer-v11 -m "merge: V11 B 轨安装器侧合入 review"`；冲突＝停下逐项裁定（RESEARCH 为准）留档。
**P2 契约核对表**：RESEARCH §7 八行逐行双侧取证（A 侧/B 侧各 file:line＋关键值原文），值偏离＝冲突裁定并留档；重点：表 1 资产名前缀三方互锁（AppInfo↔Updater↔.iss↔build-setup.cmd）、表 4 dist 四件、表 6 五值清扫、表 7 形状判定逐字同构（含 `..` 拒绝）。
**P3 合并态全链验证**：①`build.cmd` 绿→dist 恰四件；②`build-setup.cmd` 绿→产物名＝`OneClickBlackScreen-Setup-1.2.0.exe`（版本未 bump 属预期）且与 .iss/AppInfo 互锁断言通过；③`verify.cmd` 八件全绿（EXIT=0 零 BUSY；GUI 件勿动键鼠）。任何红＝定位属 A 还是 B 的缺陷，记入 REVIEW 移交注记（小缺陷落白名单可修则修并留档，沿 V9 D-1 先例）。
**P4 README.md 行为文档改写**（版本引用直写 1.3.0）：①下载节（L38-58 区域）改**单形态**——唯一下载项 `OneClickBlackScreen-Setup-1.3.0.exe`＋`SHA256SUMS.txt` 校验（certutil 例随名）；删 zip/裸 exe 两行与 zip 内容注；②新增**安装位置说明**＝默认装到 `%LOCALAPPDATA%\Programs\一键黑屏`（资源管理器地址栏粘贴即达），可在向导自定义，Windows「设置>应用」可查；③使用节（L60-93）「两种启动方式」表改双 EXE（遮罩 exe 双击/设置 exe 双击/开始菜单两条，零参数），`--settings` 仅在边界节留一句「旧版参数仍可用」兼容注记；④页脚更新说明改「下载完整安装包校验后静默覆盖、装完自动重启遮罩」；⑤卸载段（L58/108）删便携卸载叙述，安装版口径补「更新状态一并清除」；⑥L6 标语与 L27「绿色零依赖/单文件」按安装形态改写（零依赖事实保留：免管理员、自带运行时）；⑦L120 七件套→八件套、L121 build-setup 产物描述改四件；⑧L131 当前批次指针改 `docs/handoff/2026-11-12-v11-singleform/`＋补 V10 归档行；⑨新增「1.2.0 便携版用户」一次性迁移指引一句（下载 Setup 直装，数据自动继承/向导预填）。
**P5 使用说明.txt 同构改写**：L7 增「v10 更新（对应程序版本 1.3.0）」行（单一安装包形态/双 EXE 免参数入口/卸载零残留清欠一句话清单）；【安装】节删便携行、写明确切默认路径；【两种启动方式】改双 EXE；L68 更新口径、L99/117 便携叙述删除或并入安装版口径；**UTF-8 BOM＋CRLF 保真**（改后 `file` 复验留档，L-006）；README（CRLF）同验。
**P6 留档推送**：`docs/handoff/2026-11-12-v11-singleform/REVIEW-V11.md`（六字段：背景/现状产出/踩坑引 lessons 相关条目/决策与否决/下一步/快速启动命令；含 P2 契约核对表全量留档、P3 全链输出、P4/P5 diff 摘要与版本引用基线〔README 版本引用数与上轮持平申报〕、移交注记两栏〔已闭环/提请〕）；提交 `review(R): V11 …`；`git push -u origin feature/review-v11`。

## 4. 铁律

写面白名单（对 main 的 diff 必须全部落入）：`README.md`、`使用说明.txt`、`docs/handoff/2026-11-12-v11-singleform/REVIEW-V11.md`(新)，以及 P1 合并引入的 A/B 轨已审 diff（+P3 允许的小修复，逐条留档）。**禁改**：`src/**`、`verify/**`、`verify.cmd`、`setup/**`、`build.cmd`/`build.rsp`/`build.settings.rsp`/`build-setup.cmd`、AGENTS.md、archive/baseline/migrate。发现 A/B 大缺陷＝停下报告用户裁决，不越权代改。真实用户状态只读（跑验证套件本身遵守闸门，AGENTS §2）。文档改写禁「补丁式叠段」——便携叙述整体退场、单形态口径全文一致，不留新旧口径并存段。证据＝真实命令输出。

## 5. 预期触碰文件（冲突面申报）

README/使用说明/REVIEW 为本轨独占写面；其余 diff 全部来自 app-v11＋installer-v11 已审合并；I 轨将只动版本引用与发版件。

## 6. 验收清单（校验器逐条核对实据）

- [ ] `git ls-remote --heads origin feature/review-v11` 可见；两次 merge `--no-ff` 提交在链上（或冲突裁定留档）
- [ ] 合并态：`build.cmd` 四件断言绿＋`build-setup.cmd` 绿（产物名互锁断言输出）＋`verify.cmd` 八件全绿（EXIT=0 零 BUSY）
- [ ] REVIEW-V11.md 含 §7 八行契约核对表（双侧 file:line＋值原文，无「同上」省略）
- [ ] `grep -n "便携\|zip\|绿色\|单文件" README.md 使用说明.txt` 零命中（迁移指引句与历史更新行除外，逐条申报）
- [ ] `grep -n "LOCALAPPDATA.*Programs" README.md 使用说明.txt` 命中（安装位置确切路径在场）
- [ ] README 下载节恰一个下载项＋SHA256SUMS.txt 校验例；L131 指向 `2026-11-12-v11-singleform`
- [ ] README/使用说明 版本引用全部 1.3.0（grep `1\.2\.0` 零命中，历史更新行除外）
- [ ] `file 使用说明.txt`＝UTF-8 BOM＋CRLF；`file README.md`＝CRLF（复验输出留档）
- [ ] diff 恰＝白名单∪已审合并（`git diff --name-status origin/main...HEAD` 逐件核对留档）

## 7. 收尾动作

REVIEW-V11.md 末尾「移交注记」两栏（已闭环/提请 I 轨或 V12），并给 I 轨列出真机冒烟建议场景（用户实装原地升级断言点、设置 EXE 双入口、D-R7 真机抽查、1.2.0 便携迁移实演、隔离完整卸载）。推送后报告：分支名、提交哈希、验收清单逐条 ✅/❌。I 轨将复检你的产出并发版 v1.3.0。

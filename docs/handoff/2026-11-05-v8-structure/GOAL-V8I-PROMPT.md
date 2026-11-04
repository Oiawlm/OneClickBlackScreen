# GOAL-V8I · 集成与收尾（I 轨）

> **一句话定位**：I 轨执行窗（GLM-5.3·目标模式）：在主检出 main 上 `--no-ff` 合并 `feature/review-v8`（内含 S＋L＋REVIEW），终验（verify ×2 全绿＋零足迹＋终态检查），闭环 REVIEW 移交注记，清理三个 worktree 与分支，留档 `VERIFY-V8.md` 并推送 main。**本轮零产品变更：不发版、不发 tag、`src/AppInfo.cs` 不动。**　**事实源＝[RESEARCH-V8.md](RESEARCH-V8.md)，冲突以它为准。**　日期：2026-11-05　行数预算：≤110 行。

## 1. 身份与冷启动（本节覆盖 AGENTS.md §1 的旧路径路由）

你是 I 轨集成窗，在**主检出** `D:\Download\Agent Vault\OneClickBlackScreen\OneClickBlackScreen-main\` 工作（集成窗独占主检出写权；本轨不建 worktree）。按序自读：
1. `AGENTS.md`；2. `docs/handoff/2026-11-05-v8-structure/RESEARCH-V8.md` 全文；3. `PLAN-V8.md`；4. `REVIEW-V8.md`（R 轨总评与移交注记）；5. 本文件。**禁读**：`docs/handoff/archive/` 全文（grep 定位）、`baseline/**`、`migrate/**`。
前置确认（缺一即停）：`git ls-remote --heads origin feature/review-v8` 可见；REVIEW 总评 GO（NO-GO→停下报告用户裁决）。

## 2. 背景

本轮首次双窗 worktree 并行的集成收口：R 已预集成 S＋L 并复检（RESEARCH §6）；你专注合并终验、移交闭环、清理与推送。零产品变更故无发布链（对比 V7I），这是**有意裁剪**：结构轮不触碰版本单源。

## 3. Phase 任务（依序）

**P1 合并**：主检出 `git status` 干净且 on main 且与 origin/main 同步后：
```bash
git fetch origin
git merge --no-ff origin/feature/review-v8 -m "merge: V8 结构与合作协议根修合入 main（S 归档45份+三层路由 ∥ L 路由枢纽+坑库+批次规范 + R 复检GO）——首次双窗 worktree 并行实证"
```
若现冲突（R 预演已排除，理论不发生）：按 REVIEW-V8 冲突裁定执行并如实记录。
**P2 终验**（注入全局键鼠，提醒用户守则）：`build.cmd` 绿＋`verify.cmd` 七件 ×2 全绿（两次独立运行，实录间隔留档）；零足迹审计三态一致（settings.ini SHA256 前后＋`schtasks /query /tn OneClickBlackScreen-Idle` 前后＋`tasklist` 无残留）。
**P3 终态检查**：`src/AppInfo.cs` 版本仍 1.0.2 零改动（`git diff origin/main...HEAD -- src/` 为空＝产品代码零变更的直接证据）；`grep -rn "1\.0\.2" src/AppInfo.cs` 唯一命中；README.md 门面除开发过程文档表外与 v1.0.2 版一致（diff 核对）；AGENTS.md 新三层路由冷启动演练一遍（AGENTS §1→docs/README.md→批次目录）路径全通。
**P4 移交闭环**：逐条处置 REVIEW-V8 移交注记——每条给「已闭环（动作+实据）/备案（理由）」，不得无说明丢弃；处置表写入 VERIFY-V8.md。
**P5 清理**：`git worktree remove "../OneClickBlackScreen-WorkTree/v8-s"`（v8-l、v8-r 同）→ `git worktree prune` → 本地分支删除（`git branch -d feature/structure-v8 feature/docs-router-v8 feature/review-v8`）；远端分支处置：先 `git ls-remote --heads origin` 观察 V7 历史分支留存惯例，照惯例执行（删或留）并如实记录；`OneClickBlackScreen-WorkTree\` 空父目录保留（下轮并行复用）。
**P6 留档推送**：`docs/handoff/2026-11-05-v8-structure/VERIFY-V8.md`（集成总证据：合并提交哈希、verify×2 实录、零足迹、终态检查、移交处置表、清理实录）；提交；`git push origin main`；（本轮完结后该批次目录由下轮规划窗移入 archive/，不属本轨动作）。

## 4. 铁律

写面白名单：merge 提交本身＋`docs/handoff/2026-11-05-v8-structure/VERIFY-V8.md`(新)＋分支/worktree 清理。白名单外一律只读；**不改 S/L/R 任何产出行**（移交注记只闭环不改写）；零产品变更红线：`src/**`、`verify/**` 源码、构建脚本、`使用说明.txt`、`AppInfo.cs`、版本号、tag、Release 全不动；冻结清单同 AGENTS §4；GUI 验证前提醒用户守则；证据＝真实命令输出留档；不删仓库、不改写远端历史、凭证不入文件。

## 5. 预期触碰文件（冲突面申报）

仅 merge 提交＋VERIFY-V8.md；无其他窗口并行（S/L/R 均已收线）。

## 6. 验收清单（校验器逐条核对实据）

- [ ] main 含 `--no-ff` 合并提交（`git log --merges -1` 可见且信息完整）；`git push origin main` 后 `origin/main` 与本地一致
- [ ] 合并态 `git diff origin/main...HEAD` 于合并前基点：无 `src/**`/`verify/**` 源码/构建脚本/使用说明/AppInfo 任何变更
- [ ] verify.cmd 七件 ×2 全绿两份独立实录；零足迹三态一致实录
- [ ] VERIFY-V8.md 含移交注记处置表（每条有去向）
- [ ] `git worktree list` 无 v8-s/v8-l/v8-r；本地三分支已删；远端分支处置已记录；WorkTree 父目录留存
- [ ] 三层路由终态演练通过；README/AGENTS 终态与 R 复检一致

## 7. 收尾动作

推送后报告：合并提交哈希、验收清单逐条 ✅/❌、移交注记处置摘要、worktree/分支清理结果。本轮收尾后，新结构（壳形＋三层路由＋坑库＋批次制）即为本仓库存续形态。

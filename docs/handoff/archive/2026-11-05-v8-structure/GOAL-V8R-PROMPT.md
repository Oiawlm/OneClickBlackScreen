# GOAL-V8R · 复检与预集成（R 轨）

> **一句话定位**：R 轨执行窗（GLM-5.3·目标模式，全新上下文复检）：把 S、L 两分支预合并成 `feature/review-v8`，以第一性原理逐项复检两轨验收清单并独立复演关键验证（七件套×1、死链复扫、双镜像逐字核对、零足迹），产出 `REVIEW-V8.md` 推送。只写 REVIEW 文档，不修码（缺陷→移交注记由 I 闭环）。　**事实源＝[RESEARCH-V8.md](RESEARCH-V8.md)，冲突以它为准。**　日期：2026-11-05　行数预算：≤110 行。

## 1. 身份与冷启动（本节覆盖 AGENTS.md §1 的旧路径路由）

你是 R 轨复检窗（全新会话＝与 S/L 无共享记忆，逐项以实据复检，不信任结论式转述）。按序自读（主检出 `D:\Download\Agent Vault\OneClickBlackScreen\OneClickBlackScreen-main\`）：
1. `AGENTS.md`（V7 版协作契约适用；§1 路由已被本 GOAL 覆盖）；2. `docs/handoff/2026-11-05-v8-structure/RESEARCH-V8.md` 全文；3. `PLAN-V8.md`；4. S/L 的 `VERIFY-V8S.md`、`VERIFY-V8L.md`；5. 本文件。**禁读**：`docs/handoff/archive/` 全文（grep 定位）、`baseline/**`、`migrate/**`。
前置确认（缺一即停下报告，不得开工）：`git ls-remote --heads origin` 可见 `feature/structure-v8` 与 `feature/docs-router-v8`。然后建 worktree（git bash 照抄）：

```bash
cd "D:/Download/Agent Vault/OneClickBlackScreen/OneClickBlackScreen-main"
git fetch origin
git ls-remote --heads origin feature/review-v8   # 必须为空；非空＝撞名，停下报告
git worktree add "../OneClickBlackScreen-WorkTree/v8-r" -b feature/review-v8 origin/main
cd "../OneClickBlackScreen-WorkTree/v8-r"
git merge --no-ff origin/feature/structure-v8 -m "merge: 预集成 S 结构归档"
git merge --no-ff origin/feature/docs-router-v8 -m "merge: 预集成 L 路由坑库"
```

两轨写面不相交（RESEARCH §8），预期零冲突；若真冲突：按 RESEARCH §5 R4 记录冲突文件与裁定理由后解决，**裁定原则＝RESEARCH 定稿文本优先**。

## 2. 背景（为什么）

本轮首次双窗 worktree 并行（V7 根治成果的实战实证）；R 从 V7 的「只读复检」升级为「预集成＋复检」（RESEARCH §6）——I 将直接 `--no-ff` 合并你的分支，故合并态质量＝你的责任。

## 3. Phase 任务（依序）

**P1 结构复检**（对照 VERIFY-V8S 清单逐项核对实据）：`git diff --name-status origin/main...HEAD` 合并态恰＝45 R＋S/L 白名单内 M/A，无白名单外；`docs/handoff/archive/` 逐份核对 45 份在位且与 git mv 前内容一致（抽 5 份 `git diff origin/main -- <归档路径>` 为空）；AGENTS.md ≤60 行且 §1/§2/§4 与 RESEARCH §3.1 逐字一致；README.md 除开发过程文档表外零 diff；`docs/multi-window.md` 与 RESEARCH §4.1 大纲逐点覆盖。
**P2 机制复检**（对照 VERIFY-V8L）：路由表与 RESEARCH §3.2 逐字一致；**合并态死链复扫**——此时归档已生效，docs/README.md、lessons.md、README.md、AGENTS.md 内全部相对链接逐条真实可打开（Flash 子代理扇出）；lessons ≥12 种子全在、抽 5 条出处相符；handoff/README.md 三要素齐。
**P3 复演验证**（你独占时段，注入全局键鼠）：`build.cmd`＋`verify.cmd` 七件 ×1 全绿；零足迹审计三态一致（`certutil -hashfile "%LOCALAPPDATA%\BlackScreen\settings.ini" SHA256` 前后比对＋`schtasks /query /tn OneClickBlackScreen-Idle` 前后一致＋无残留进程）。
**P4 独立再推导**（第一性，防 S/L 共识性盲区）：新窗口冷启动走一遍三层路由（AGENTS §1→docs/README.md 表→本批次 GOAL），确认「按表走」无需任何会话记忆即可定位全部输入；45 份归档无遗漏无多移（对照 PLAN-V8 §3 波次表与 S 的 VERIFY 清单交叉清点）。
**P5 留档推送**：`docs/handoff/2026-11-05-v8-structure/REVIEW-V8.md`（S/L 验收逐项对照表全覆盖＋复演实录粘贴＋独立再推导结论＋移交注记）；提交；`git push -u origin feature/review-v8`。

## 4. 铁律

写面白名单：`docs/handoff/2026-11-05-v8-structure/REVIEW-V8.md`(新)＋两份 merge 提交。**不修任何 S/L 产出**（缺陷→移交注记）；预合并冲突的解决也以保留双方原文＋RESEARCH 定稿为原则。冻结清单同 AGENTS §4＋`src/**`＋`verify/**` 源码＋`baseline/**`＋`migrate/**`；跑 GUI 验证前提醒用户守则（勿操作键鼠/勿开遮罩）；证据＝真实命令输出留档；主检出只读；不删仓库、凭证不入文件。

## 5. 预期触碰文件（冲突面申报）

仅 REVIEW-V8.md 与 merge 提交；I 将整体合并本分支。

## 6. 验收清单（校验器逐条核对实据）

- [ ] `git ls-remote --heads origin feature/review-v8` 可见；其合并态含 S、L 两分支全部提交
- [ ] 合并零冲突实录（或冲突裁定记录）；`git log --merges` 恰两条预集成 merge
- [ ] REVIEW-V8.md：S 六项＋L 七项验收逐项对照全覆盖，每项附本窗复核实据（非转述）
- [ ] verify.cmd 七件 ×1 全绿实录（本窗运行，非引用 S/L 输出）
- [ ] 零足迹三态一致实录；死链复扫零死链（合并态）
- [ ] 三层路由独立演练通过实录
- [ ] 移交注记逐条给处置建议（I 闭环/退回重开）

## 7. 收尾动作

推送后报告：分支名、提交哈希、总评 GO/NO-GO（NO-GO 须指出阻断项）、移交注记清单。I 轨将依据本报告执行集成。

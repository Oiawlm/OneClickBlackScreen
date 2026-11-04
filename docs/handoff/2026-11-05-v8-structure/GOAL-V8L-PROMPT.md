# GOAL-V8L · 路由枢纽与坑库（L 轨）

> **一句话定位**：L 轨执行窗（GLM-5.3·目标模式）：新建三份机制文档——`docs/README.md`（任务路由枢纽＝渐进式披露第 2 层）、`docs/lessons.md`（跨轮坑库）、`docs/handoff/README.md`（批次制规范＋交接模板）——独立 worktree 内完成并推送分支 `feature/docs-router-v8`。　**事实源＝[RESEARCH-V8.md](RESEARCH-V8.md)（§2.2 结构/§3.2 路由表定稿/§4.2 本轨设计），冲突以它为准。**　日期：2026-11-05　行数预算：≤110 行。

## 1. 身份与冷启动（本节覆盖 AGENTS.md §1 的旧路径路由）

你是 L 轨执行窗，产出落地文档与证据，不产出讨论。按序自读（主检出 `D:\Download\Agent Vault\OneClickBlackScreen\OneClickBlackScreen-main\`）：
1. `AGENTS.md`（V7 版，协作契约仍全部适用；其 §1 冷启动路径已被本 GOAL 覆盖）；2. `docs/handoff/2026-11-05-v8-structure/RESEARCH-V8.md` 全文；3. `PLAN-V8.md` §2/§3；4. 本文件。**禁读**：历史 RESEARCH/PLAN/GOAL 全文——坑库提炼时**按关键词 grep 定位**后只读命中的节（这是本轮机制要防的上下文膨胀，先以身作则）；`baseline/**`、`migrate/**`。
读完先建 worktree（git bash，命令照抄）：

```bash
cd "D:/Download/Agent Vault/OneClickBlackScreen/OneClickBlackScreen-main"
git fetch origin
git ls-remote --heads origin feature/docs-router-v8   # 必须为空；非空＝撞名，停下报告
git worktree add "../OneClickBlackScreen-WorkTree/v8-l" -b feature/docs-router-v8 origin/main
cd "../OneClickBlackScreen-WorkTree/v8-l"              # 本窗全部工作在此，主检出只读
```

## 2. 背景（为什么）

踩坑记录散在 19 份历史 VERIFY 的移交/事故注记里无汇总，新窗口无任务路由（RESEARCH §1 #5）；fts 三层渐进披露（AGENTS→docs/README.md→正文）与 observations.md 坑库、claude-handoff 交接模板（「失败尝试」最防重复踩坑）为成熟范式（RESEARCH §2 佐证）。本轨与 S 轨（并行窗，归档 45 份＋AGENTS/README 改写）写面不相交；**本轨是纯文档轨，不跑任何验证器**。注意：本 worktree 基于 main 建，S 尚未归档——你引用历史文档一律写**归档后路径** `docs/handoff/archive/...`，核验内容时映射回本 worktree 现存位置（根目录 / `docs/` 下）；R 将在合并态复核真实路径。

## 3. Phase 任务（依序）

**P1 路由枢纽**：`docs/README.md` ≤90 行。骨架＝RESEARCH §3.2 路由表**逐字照抄**，逐行补一两句「何时读/别读理由」，文件头三要素（一句话定位＋来源链接 RESEARCH-V8＋日期＋行数预算）＋双镜像声明（「本表与 AGENTS.md §1 互为镜像，改任一必须同步另一」）。
**P2 坑库**：`docs/lessons.md` ≤180 行。条目格式 `L-00x｜现象｜根因｜正解｜出处（归档后路径+file:line/节）｜防再犯锚点（验证器检查项/AGENTS 条款/冻结区）`。RESEARCH §4.2 列的 12 条种子**必须收录**（出处自查补全）；再从 `docs/handoff/archive/`（本 worktree 内＝旧位置）各 VERIFY 的事故/移交注记 grep 提炼增补，**宁缺毋滥：每条必须有可打开的真实出处，禁止凭印象编造**。
**P3 批次规范**：`docs/handoff/README.md` ≤80 行：批次命名 `<yyyy-mm-dd>-<slug>/`（启动日＋kebab）；批次内文件名沿用既有惯例（`VERIFY-V<n><轨>.md`/`REVIEW-V<n>.md`，规划三件套同目录）；完结批次整目录 `git mv` 入 `archive/`；处置两桶＝保留/归档（本项目历史全保留，不删）；六字段交接模板（背景/现状与产出/踩坑（引 L-00x）/决策与否决/下一步/快速启动命令）照 RESEARCH §2.2。
**P4 验证**：路由演练——按 `docs/README.md` 表逐行模拟新窗口冷启动走到「执行轮次任务」终点＝本批次 GOAL-V8S 路径，逐条核对指向本 worktree 内可打开（归档件按 §2 映射口径）；lessons 出处抽查 ≥1/3（Flash 子代理核对「出处文件存在且内容与条目相符」）；三文件行数预算达标。
**P5 留档推送**：`docs/handoff/2026-11-05-v8-structure/VERIFY-V8L.md`（真实输出：路由演练路径表、抽查结果、行数统计）；提交 `docs(L): V8 路由枢纽+坑库+批次规范…`；`git push -u origin feature/docs-router-v8`。

## 4. 铁律

写面白名单（对 main 的 diff 必须全部落入）：`docs/README.md`(新)、`docs/lessons.md`(新)、`docs/handoff/README.md`(新)、`docs/handoff/2026-11-05-v8-structure/VERIFY-V8L.md`(新)。白名单外一律只读——**不改 AGENTS.md、不改根 README.md、不动任何历史文档**（归档是 S 轨写面）。禁补丁式内容（模板/条目结构一次定形）；坑库条目必须有真实出处；不跑验证器、不 build；冻结清单同 AGENTS §4＋`src/**`＋`verify/**`；主检出只读；证据＝真实输出留档。

## 5. 预期触碰文件（冲突面申报）

与 S 轨零交集（本轨全新文件）；三新文件路径 S/R/I 均不改写（I 仅合并）。引用历史文档写归档后路径（S 归档后即真实路径）。

## 6. 验收清单（校验器逐条核对实据）

- [ ] `git ls-remote --heads origin feature/docs-router-v8` 可见且指向本轨提交
- [ ] `git diff --name-status origin/main...HEAD`：恰 4 项 A（新增），无其他
- [ ] docs/README.md 路由表与 RESEARCH §3.2 **逐字一致**（含双镜像声明），≤90 行
- [ ] lessons.md ≥12 条种子全收录、总条目出处抽查 ≥1/3 相符（抽查实录留档）、≤180 行
- [ ] handoff/README.md 含批次命名/两桶政策/六字段模板，≤80 行
- [ ] 路由演练实录：表内每条路径核验通过（含映射口径说明）
- [ ] 无验证器/build 运行记录（纯文档轨守约）

## 7. 收尾动作

VERIFY-V8L.md 末尾附「移交注记」（如发现 S 轨应改未改之处，记此不改）；推送后报告：分支名、提交哈希、验收清单逐条 ✅/❌。R 轨将复检并预合并你的产出。

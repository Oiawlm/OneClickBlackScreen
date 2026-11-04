# REVIEW-V8 · R 轨复检与预集成总证据

> **一句话定位**：R 轨全新上下文复检——S+L 预集成至 `feature/review-v8`（两次 merge 零冲突）、S/L 验收 14 项逐项复核实据、七件套×1 全绿＋零足迹三态一致＋死链复扫 0 死链＋三层路由独立演练，**总评 GO(V8R)**。　**分支**：feature/review-v8（worktree `..\OneClickBlackScreen-WorkTree\v8-r`）。　**日期**：2026-11-05。事实源＝RESEARCH-V8；本文件只贴本窗真实命令输出。

## 0. 前置确认与预集成实录

```bash
$ git fetch origin && git ls-remote --heads origin   # feature/structure-v8=c55346d3、feature/docs-router-v8=b74ea6b3 可见；
                                                     # feature/review-v8 不在列表＝无撞名
$ git worktree add "../OneClickBlackScreen-WorkTree/v8-r" -b feature/review-v8 origin/main
HEAD is now at b001aa7
$ git merge --no-ff origin/feature/structure-v8 -m "merge: 预集成 S 结构归档"
Merge made by the 'ort' strategy. … 49 files changed, 276 insertions(+), 7 deletions(-)   ← 零冲突
$ git merge --no-ff origin/feature/docs-router-v8 -m "merge: 预集成 L 路由坑库"
Merge made by the 'ort' strategy. … 4 files changed, 269 insertions(+)                    ← 零冲突
$ git log --merges --oneline origin/main..HEAD
74f79b3 merge: 预集成 L 路由坑库
629defc merge: 预集成 S 结构归档        ← 恰两条预集成 merge，RESEARCH §5 R4 冲突预案未触发
```

合并态对 origin/main 全量 diff：`45 R100＋2 M（AGENTS.md、README.md）＋6 A（docs/{README,lessons,multi-window}.md、docs/handoff/README.md、VERIFY-V8S/L.md）`——恰＝S 白名单 49 项∪L 白名单 4 项，无白名单外路径（写面不相交契约兑现）。

## 1. S 轨验收逐项对照（GOAL-V8S §6 七项，每项本窗实证）

| # | S 验收项 | 本窗复核实据 | 判定 |
|---|---|---|---|
| S-1 | 分支推送可见 | 冷启动 `ls-remote` 见 c55346d3；`git log origin/main..origin/feature/structure-v8`＝7e1a026（P1 归档）＋c55346d（P2-P5）两提交，均已入合并态 | ✅ |
| S-2 | diff＝45 R＋白名单 M/A | §0 汇总；45 项全 R100（100% 相似度＝内容零改动自证） | ✅ |
| S-3 | VERIFY-V8S 含 45/45 renamed＋--follow×3 | 文件在位（222 行），§1 porcelain 45 行全 R、§4.4 三份穿透输出在档；本窗另做 blob 哈希全量对比（见 §4）独立加固 | ✅ |
| S-4 | build exit 0＋纯件三件 exit 0 | 本窗 P3 复演 build exit 0；纯件三件由本窗 verify.cmd 七件套内 [1/5/6] 全 PASS 复盖（见 §3） | ✅ |
| S-5 | 死链扫描零死链＋README 表外零 diff | 本窗合并态复扫 0 死链（§2 死链行＋§4）；README diff 恰单 hunk @@ -119,10 +119,10 @@ 表内 4 行替换，正文其余零改动（本窗实跑 `git diff origin/main -- README.md` 逐行核对） | ✅ |
| S-6 | AGENTS ≤60 行＋§1/§2/§4 与 §3.1 逐字一致；multi-window ≤80 | 本窗实读：AGENTS 38 行；§1 三行与 RESEARCH §3.1 代码块逐字同；§2 追加行纯文本逐字同（按既有列表样式加 `- ` 前缀，符合 AGENTS §6「融入既有结构」原则，S VERIFY §2 已申报此口径）；§4 第二行＝定稿文本（加反引号与句号按既有行样式），第一/三行不变；multi-window 45 行 | ✅ |
| S-7 | 无 GUI 验证器运行记录 | S 轨期 verify.cmd 未跑（VERIFY §4.2 注记自证＋L 并行时段无 BUSY 事故）；GUI 件首跑＝本窗 P3 独占时段 | ✅ |

## 2. L 轨验收逐项对照（GOAL-V8L §6 七项，每项本窗实证）

| # | L 验收项 | 本窗复核实据 | 判定 |
|---|---|---|---|
| L-1 | 分支推送可见 | ls-remote 见 b74ea6b3，已入合并态 | ✅ |
| L-2 | diff 恰 4 项 A | L 对 main diff＝docs/README.md、docs/handoff/README.md、docs/lessons.md、VERIFY-V8L.md，无其他（§0 合并态亦无 L 系其他路径） | ✅ |
| L-3 | 路由表与 §3.2 逐字一致＋双镜像声明，≤90 行 | 本窗实读 docs/README.md 30 行，表 6 数据行＋表头与 RESEARCH §3.2 逐字符一致（含全角／分隔符）；双镜像声明在文件头 :5 | ✅ |
| L-4 | lessons ≥12 种子＋抽查 ≥1/3 相符，≤180 行 | 117 行 16 条；12 种子（RESEARCH §4.2 ①~⑫）逐一对应 L-001~L-012 在位；L 窗抽 6/16＋本窗独立抽 **5 条不同集**（L-001/L-003/L-004/L-010/L-016）→ 子代理逐出处 sed 实读：**5/5 MATCH、行号零漂移**（如 REVIEW-V7.md:109＝R-2 表格行全文、PUBLISH-V5A.md:148＝CopyFromScreen 隐私纪事逐字） | ✅ |
| L-5 | handoff/README.md 三要素，≤80 行 | 36 行：批次命名（启动日＋kebab）、两桶政策（保留/归档不删除）、六字段模板（背景/现状与产出/踩坑引 L 编号/决策与否决/下一步/快速启动命令）与 RESEARCH §2.2 裁剪定案逐字对应 | ✅ |
| L-6 | 路由演练逐条核验 | L 窗 10✅＋2 项「合并态 R 复核」（multi-window.md、archive/）——本窗合并态补核：两路径均存在可打开（死链扫描 C 组），且 multi-window.md 内容与 RESEARCH §4.1 大纲四要素逐点覆盖（壳形/创建模板含撞名即停/清理模板/写权归属） | ✅ |
| L-7 | 无验证器/build 运行记录 | L 窗自证＋S/L 并行期两 worktree 的 dist/verify\bin 物理隔离无锁冲突（本轮并行零事故＝V7 根治首战实证） | ✅ |

## 3. P3 复演实录（本窗独占时段，2026-11-05）

```text
$ cmd //c build.cmd → [OK] Build succeeded: …\v8-r\dist\一键黑屏.exe   BUILD-EXIT=0
$ cmd //c verify.cmd → [1/7]VerifyConfig … [7/7]VerifyIdle → [OK] ALL VERIFIERS PASSED   VERIFY-EXIT=0
```

七件全绿证据链（双层）：①套件级 `[OK] ALL VERIFIERS PASSED`＋exit 0——本窗 grep verify.cmd:36-66 实证该行仅在七件逐个 `errorlevel 2→busy/1→fail` 全不触发后可达；②件级逐项 PASS 输出截录：[4/7] Overlay `PASS A-H … PASS Z: user's real settings.ini untouched`、`PASS TOTAL: A-H; 1 displays; max InputIdle 219 ms (limit 2000)`；[5/7] Icon `PASS TOTAL: 4 icon checks`；[6/7] Update `PASS TOTAL: update checks (version table + offline full chain)`（含 concurrent x50）；[7/7] Idle `PASS TOTAL: 5 idle checks`。未撞闸门（无 BUSY）。

零足迹审计（前态→后态逐项比对，与 V7V 口径一致）：

| 审计项 | 前态（verify 前） | 后态（verify 后） | 判定 |
|---|---|---|---|
| `%LOCALAPPDATA%\BlackScreen\settings.ini` | ABSENT | ABSENT | ✅ 一致 |
| 计划任务 `OneClickBlackScreen-Idle`（schtasks /query） | 系统找不到指定的文件 | 系统找不到指定的文件 | ✅ 一致 |
| 残留进程（tasklist 过滤黑屏/七验证器名） | 无 | 无 | ✅ 零残留 |

## 4. P4 独立再推导（第一性，防 S/L 共识盲区）

1. **三层路由冷启动演练（合并态＝集成后 main 的真实形态）**：AGENTS.md §1（注入常驻）→ 指向 docs/README.md → 路由表「执行轮次任务｜先读当前批次 GOAL」→ `ls docs/handoff/` 排除 README/archive 后唯一批次 `2026-11-05-v8-structure` → RESEARCH-V8.md/PLAN-V8.md/GOAL-V8{S,L,R,I}-PROMPT.md 全部可定位。全程零会话记忆，「按表走」闭环 ✅。
2. **45 份归档无遗漏无多移（独立交叉清点）**：`ls docs/handoff/archive/` 分类计数＝GOAL 16＋PLAN 3＋RESEARCH 7＋docs 系（VERIFY/REVIEW/COPY/MIGRATE/PUBLISH）19＝**45**，与 RESEARCH §2.2 清单三类计数逐项相等；根目录 `RESEARCH-V*.md/PLAN-V*.md/GOAL-V*-PROMPT.md` 已全部移空（ls 报 No such file）。
3. **归档字节不变全量加固（超出 GOAL 抽 5 份要求）**：对 45 项逐一 `git rev-parse origin/main:<旧路径>` vs `HEAD:<新路径>` blob 哈希比对 → **45/45 MATCH**（blob 哈希相等＝字节级相同，强于 R100 相似度语义）。
4. **合并态死链复扫（子代理扇出，零写入）**：门禁范围 4 文件——README.md 12 条 markdown 相对链接（8 处含 LICENSE/表内 6 目标）、docs/README.md 3 条、lessons.md 出处文本路径 15 个唯一值（含 src/MaskConfig.cs、src/Updater.cs、AGENTS.md、归档 10 份、multi-window.md、verify.cmd）、AGENTS.md 0 条（纯文本引用）→ **0 死链**；L 移交注记 2 的两路径补核通过。归档组 45 份内部旧布局死链 14 条＝S-1 已备报的设计内平移（内容冻结不可改写），不门禁。
5. **第一性复核结论**：本轮变更面＝纯文档/git mv（产品源码 src/verify/build 链零触碰，`git diff origin/main...HEAD` 无一源码路径），故 build+verify 七件绿即证明合并态对产品零影响；结构变更的正确性由 1-4 独立闭环。

## 5. 移交注记处置表（逐条建议，I 闭环）

| 注记 | 内容 | 本窗裁定与处置建议 |
|---|---|---|
| S-1 | 归档组 14 条旧布局死链（内容冻结必然） | **备案不修**：lessons.md 头部出处口径＋handoff/README.md「归档后引用一律写全路径」已覆盖规则面；I 在 VERIFY-V8.md 引用本行即可 |
| S-2 | VerifyIcon bash→cmd 包装链中文路径破坏（编码第四变体：调用链变体） | L-006 已收三变体，本条为同族增补候选；**I 备案、下轮 lessons 追加**（L 写面本轮已冻结，不代改） |
| S-3 | .gitignore 核查＝无需补行 | **闭环**（无需处置） |
| L-1 | verify.cmd 头注释为套件级说明（无逐件清单），路由表照抄定稿 | docs/README.md 展开行已按实际表述；**闭环** |
| L-2 | multi-window.md＋archive/ 待合并态复核 | **本窗已复核通过**（§2 L-6/§4.4），闭环 |
| L-3 | lessons 引用两系映射口径 | 本窗死链复扫按此口径 0 死链，口径成立；**闭环** |
| R-1（新） | git bash MSYS 路径转换破坏 schtasks 参数（`/query`→`D:/App/git/query`），须 `cmd //c` 包裹 | 与 L-006 相邻的不同机理（路径转换非编码）；**I 备案、下轮 lessons 候选**，不阻断 |
| R-2（新） | GOAL-V8R §6 称「S 六项」而 GOAL-V8S §6 实列 7 项 checkbox | 本 REVIEW 按实有 7 项全覆盖，无实际影响；**备案**（文档计数精度） |

## 6. 总评与提交链

- **总评：GO(V8R)**——S 7/7、L 7/7 验收逐项复检通过；预集成零冲突；七件×1 全绿＋零足迹三态一致；死链复扫 0 死链；三层路由独立演练闭环。无阻断项。
- 提交链：b001aa7（main 基）→ 629defc（merge S）→ 74f79b3（merge L）→ 本提交（REVIEW-V8.md）。推送：`git push -u origin feature/review-v8`，I 轨直接 `--no-ff` 合并本分支（RESEARCH §6 波次 3）。

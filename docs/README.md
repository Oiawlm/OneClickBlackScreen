# docs/README.md · 任务路由枢纽

> **一句话定位**：渐进式披露第 2 层——读完 AGENTS.md（第 1 层）后，按任务类型在此选路：读该读的，跳过过程记录。　**来源**：[RESEARCH-V8 §3.2](handoff/archive/2026-11-05-v8-structure/RESEARCH-V8.md)（路由表逐字照抄＋逐行展开）。　**日期**：2026-11-05　行数预算：≤90 行。
>
> **双镜像声明**：本表与 [AGENTS.md](../AGENTS.md) §1 冷启动路由互为镜像，**改任一必须同步另一**（R 轨复检逐字核对）。

## 路由表

| 任务 | 先读 | 再读 | 别读 |
|---|---|---|---|
| 任意新窗口冷启动 | AGENTS.md | 本表选路 | archive 全文 |
| 执行轮次任务 | 当前批次 GOAL | 批次内 RESEARCH/PLAN | 历史批次全文 |
| 查踩坑／勿改区／「这个能不能动」 | docs/lessons.md | 条目出处锚点 | — |
| 多窗口并行／worktree | docs/multi-window.md | AGENTS §2 | — |
| 发版（I 轨） | 批次 GOAL-V?I | archive 内上一份 VERIFY-V?I | 其余历史 |
| 跑验证 | verify.cmd 头注释 | AGENTS §2 独占资源 | — |

## 逐行展开（何时读／别读理由）

- **任意新窗口冷启动｜先 AGENTS.md，再本表选路｜别读 archive 全文**：AGENTS.md 是常驻协作契约（独占资源/单一写者/冻结区/红线），每窗必读，读完按本表对号入座；archive（V1~V7 过程记录）全文阅读正是三层披露要防的上下文膨胀——要惯例只 grep 关键词定位命中的节。
- **执行轮次任务｜先当前批次 GOAL，再批次内 RESEARCH/PLAN｜别读历史批次全文**：GOAL 是执行合同（写面白名单/铁律/验收清单）；RESEARCH/PLAN 补背景与定案，冲突以 RESEARCH 为准；历史批次仅当本轮文档点名引用时按 `docs/handoff/archive/` 路径查证命中节。
- **查踩坑／勿改区／「这个能不能动」｜先 docs/lessons.md，再条目出处锚点**：改任何文件前先查坑库——条目五要素（现象/根因/正解/出处/防再犯锚点）直接回答「能不能动、怎么动才对」；要原始细节时按出处锚点（归档路径＋file:line/节）打开命中段落，勿整读原文件。
- **多窗口并行／worktree｜先 docs/multi-window.md，再 AGENTS §2**：multi-window.md 给 worktree 创建/推送/清理模板（`ls-remote` 撞名即停、一轨一树一分支）；AGENTS §2 定义验证套件会话级独占与 checkout 归属——并行的两条硬边界。
- **发版（I 轨）｜先批次 GOAL-V?I，再 archive 内上一份 VERIFY-V?I｜别读其余历史**：发版纪律（版本单源→tag→Release→应用内冒烟）以本轮 GOAL 为准；上一份 VERIFY-V?I 是最近一次发版实操记录（真实命令与事故注记可直接参照），更早轮次不必读。
- **跑验证｜先 verify.cmd 头注释，再 AGENTS §2 独占资源**：verify.cmd 头注释说明套级行为（编译并针对 `dist\` 运行全部验证器；遮罩验证器短暂全屏属预期）；AGENTS §2 是闸门协议——验证器注入全局键鼠（会话级独占），撞闸输出 BUSY/退出码 2 时等对方结束再跑，勿绕过闸门、勿盲目重试风暴。

## 补充路由

- 批次怎么建/怎么归档/交接模板 → [handoff/README.md](handoff/README.md)。
- 查产品行为基准 → `baseline/`（只读冻结）；查迁移档案 → `migrate/`＋`SHA256SUMS.txt`（只读冻结）——均见 AGENTS §4。

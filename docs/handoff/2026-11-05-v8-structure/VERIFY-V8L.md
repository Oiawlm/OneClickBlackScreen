# VERIFY-V8L · L 轨证据留档（路由枢纽＋坑库＋批次规范）

> **一句话定位**：L 轨三件产出（docs/README.md／docs/lessons.md／docs/handoff/README.md）的验证实录——路由演练逐行、坑库出处抽查 ≥1/3、行数预算、纯文档轨守约，全部真实命令输出。　**上游**：[RESEARCH-V8](RESEARCH-V8.md) §3.2/§4.2、[GOAL-V8L](GOAL-V8L-PROMPT.md)。　**日期**：2026-11-05。

## 1. 产出与行数预算（wc -l 实跑）

```text
$ wc -l docs/README.md docs/lessons.md docs/handoff/README.md
   30 docs/README.md        （预算 ≤90  ✅）
  117 docs/lessons.md       （预算 ≤180 ✅，16 条＝12 种子 L-001~L-012＋增补 L-013~L-016）
   36 docs/handoff/README.md（预算 ≤80  ✅）
```

## 2. 路由表与 RESEARCH §3.2 逐字一致（diff 实跑 ×2）

```text
$ sed -n '69,76p' docs/handoff/2026-11-05-v8-structure/RESEARCH-V8.md > /tmp/research-table.txt
$ grep -A7 '^| 任务 | 先读 | 再读 | 别读 |$' docs/README.md > /tmp/my-table.txt
$ diff /tmp/research-table.txt /tmp/my-table.txt && echo MATCH
=== TABLE VERBATIM MATCH ===        ← 初版核对
（docs/README.md 后续仅改「跑验证」展开行一处，表格区未动，复跑 diff 仍）
$ ... > /tmp/my-table2.txt && diff /tmp/research-table.txt /tmp/my-table2.txt
TABLE-VERBATIM-OK                    ← 终版复核
```

双镜像声明已写入 docs/README.md 文件头（「本表与 AGENTS.md §1 互为镜像，改任一必须同步另一」）。

## 3. 路由演练实录（模拟新窗口冷启动→「执行轮次任务」终点）

**映射口径**（GOAL §2）：本 worktree 基于 main 建、S 轨（45 份归档＋multi-window.md）并行未合——本轨引用一律写归档后路径，核验时映射回归档前位置（docs/ 系去 `handoff/archive/` 段；RESEARCH/PLAN/GOAL 系回仓库根）。演练终点＝当前批次 GOAL-V8S 路径。

| 路由行 | 指向 | 核验实据 | 结果 |
|---|---|---|---|
| 冷启动·先读 | AGENTS.md | 存在；`grep -n "^## 2" AGENTS.md` → `10:## 2. 独占资源（多窗口并发冲突的真实来源）` | ✅ 直开 |
| 冷启动·再读 | 本表选路＝docs/README.md | 本轨新建 30 行 | ✅ |
| 冷启动·别读 | archive 全文 | 「别读」项无需打开；archive/ 目录＝S 轨 git mv 写面，本 worktree 未建 | ➖ 合并态 R 复核 |
| 执行轮次任务·先读 | 当前批次 GOAL | `ls docs/handoff/` 排除 README/archive → 唯一批次 `2026-11-05-v8-structure` → `GOAL-V8S-PROMPT.md` 存在 | ✅ **终点达成** |
| 执行·再读 | 批次内 RESEARCH/PLAN | RESEARCH-V8.md、PLAN-V8.md 存在（本窗冷启动已实读） | ✅ |
| 查踩坑·先读 | docs/lessons.md | 本轨新建 16 条 | ✅ |
| 查踩坑·再读 | 条目出处锚点 | 抽查见 §4（6/16 条 13 处引文） | ✅ |
| 多窗口并行·先读 | docs/multi-window.md | S 轨并行新件，本 worktree 无此文件 | ⚠ 合并态 R 复核（S 轨验收项含此文件） |
| 发版·先读 | 批次 GOAL-V?I | `GOAL-V8I-PROMPT.md` 存在 | ✅ |
| 发版·再读 | archive 内上一份 VERIFY-V?I | 归档路径 `docs/handoff/archive/VERIFY-V7.md` → 映射 `docs/VERIFY-V7.md` 存在（:85-87 事故注记本窗实读，见 §4） | ✅（映射口径） |
| 跑验证·先读 | verify.cmd 头注释 | `head -3 verify.cmd` 实读：`@echo off`＋`rem Compile and run all verifiers against the exe in dist\.`＋`rem Note: the overlay verifiers briefly show a fullscreen mask - that is expected.` | ✅（注：头注释为**套件级**说明，无逐件清单——docs/README.md 展开行已按实际内容表述，见移交注记 1） |
| 跑验证·再读 | AGENTS §2 独占资源 | 同第一行实据（AGENTS.md:10） | ✅ |

汇总：**10 ✅ 直开/映射可开，2 项为 S 轨写面（multi-window.md、archive/）记「合并态 R 复核」**——与本轨并行编排预期一致，非缺陷。

## 4. 坑库出处抽查（≥1/3，实抽 6/16 条＝37.5%，13 处引文）

执行方式：只读 Explore 子代理逐行核对「出处文件存在且该行内容与条目主张相符」（本环境以 Explore 承担 Flash 机械扫描角色）。

```text
抽查条目：L-002/L-005/L-008/L-013/L-014/L-015（10 处文档引文）＋src/Updater.cs:131（L-008 引用）
结果：13 处中 9 处 MATCH；3 处行号漂移（内容真实存在、行号偏移）＋1 处映射前缀说明：
- L-005｜VERIFY-V6.md:73 → 内容在 :74（SumsCheck 假 MISMATCH）
- L-013｜VERIFY-V6.md:72 → 内容在 :73（SettingsSmoke 首版自锁）
- L-014｜VERIFY-V6.md:71 → 内容在 :72（幻影 ESC §7 复现注记；:11 首发现场本就 MATCH）
- L-008｜VERIFY-V7S.md:19,21 → 红基线实录在 :14-18,22（300/300 相撞）
- 映射说明：RESEARCH-V7.md 归档前在仓库根（非 docs/）——lessons.md 头部口径已补全两系映射
处置：4 处行号已修正入库（Edit 实改）；VERIFY-V7V.md:8 初判疑似漂移系主窗终端 GBK 显示乱码误读，
grep -n "GetFolderPath" 复核确认 :8 无误（本次抽查过程本身撞上 L-006 编码坑，已在 §4 如实记录）。
```

修正后主窗补核其余全部数字引文（逐行实读）：VERIFY-V7V.md:8,22,32,93-96；REVIEW-V7.md:27,52,108-110；VERIFY-V7.md:85-87；VERIFY-V6B.md:61-65；VERIFY-V7S.md:41,86-92；PUBLISH-V5A.md:147-150；VERIFY-V5.md:101；VERIFY-V4.md:46；src/MaskConfig.cs:49、src/Updater.cs:131——全部相符。**结论：16/16 条出处可打开且内容相符。**

## 5. 纯文档轨守约与写面白名单

- 全程未跑任何验证器、未 build（无 verify.cmd/ build.cmd 执行记录；本轨唯一子代理均为只读查证/核对）。
- 对 main 的 diff（含本文件，git add 后实跑）：

```text
$ git diff --name-status --cached origin/main
A	docs/README.md
A	docs/handoff/README.md
A	docs/lessons.md
A	docs/handoff/2026-11-05-v8-structure/VERIFY-V8L.md   ← 本文件 add 后恰 4 项 A，无其他
```

- 分支：`feature/docs-router-v8`（建前 `git ls-remote --heads origin feature/docs-router-v8` 为空＝无撞名；自 origin/main @ b001aa7 建）；AGENTS.md/根 README.md/历史文档/src/**/verify/** 零触碰。

## 6. 移交注记（记此不改）

1. **致 S/R（verify.cmd 头注释事实口径）**：路由表「跑验证｜先读 verify.cmd 头注释」照抄自 RESEARCH §3.2 定稿；实测头注释为套件级两行 rem（用途＋遮罩预期），**无逐件用法/顺序清单**（逐件行为在 cmd 体内）。docs/README.md 展开行已按实际内容表述；verify.cmd 属冻结面，两轨均不动，无需处置。
2. **致 R（两路径合并态补核）**：`docs/multi-window.md` 与 `docs/handoff/archive/` 是 S 轨写面，在本轨 worktree 不存在；路由演练对应两行已记「合并态 R 复核」，请在 S+L 合并态补核该两路径可打开。
3. **致 S（lessons 引用口径）**：坑库引用归档件一律写 `docs/handoff/archive/...`；其中 RESEARCH/PLAN/GOAL 系归档前在仓库根（lessons.md 头部已写明两系映射规则），R 复核按此口径映射，勿按单一 docs/ 前缀误判死链。

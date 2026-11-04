# GOAL-V8S · 结构归档与协议演进（S 轨）

> **一句话定位**：S 轨执行窗（GLM-5.3·目标模式）：把 45 份历史轮次文档 git mv 归档进 `docs/handoff/archive/`、AGENTS.md 升级三层路由、README 开发过程文档表改指新结构、新建 worktree 规程文档——四件事一次根修，独立 worktree 内完成并推送分支 `feature/structure-v8`。　**事实源＝[RESEARCH-V8.md](RESEARCH-V8.md)（§2 目标结构/§3 定稿文本/§4.1 本轨设计），冲突以它为准。**　日期：2026-11-05　行数预算：≤120 行。

## 1. 身份与冷启动（本节覆盖 AGENTS.md §1 的旧路径路由）

你是 S 轨执行窗，产出落地改动与证据文档，不产出讨论。按序自读（全部在主检出 `D:\Download\Agent Vault\OneClickBlackScreen\OneClickBlackScreen-main\`）：
1. `AGENTS.md`（V7 版，协作契约仍全部适用；其 §1 冷启动路径已被本 GOAL 覆盖）；2. `docs/handoff/2026-11-05-v8-structure/RESEARCH-V8.md` 全文；3. `PLAN-V8.md` §2/§3；4. 本文件。**禁读**：RESEARCH/PLAN/GOAL/VERIFY 历史文档全文（本轮起归档件，只 grep 定位）、`baseline/**`、`migrate/**`。
读完先建 worktree（在主检出根执行；命令照抄，git bash）：

```bash
cd "D:/Download/Agent Vault/OneClickBlackScreen/OneClickBlackScreen-main"
git fetch origin
git ls-remote --heads origin feature/structure-v8   # 必须输出为空；非空＝撞名，停下报告，不得自行改名
git worktree add "../OneClickBlackScreen-WorkTree/v8-s" -b feature/structure-v8 origin/main
cd "../OneClickBlackScreen-WorkTree/v8-s"            # 本窗全部工作在此，主检出只读
```

## 2. 背景（为什么）

根目录堆 45 份轮次文档且无路由枢纽/归档/坑库（RESEARCH §1 #2）；用户定案 fts 式壳形＋全量归档（RESEARCH §7）。本轨与 L 轨（另一窗口并行，写面＝docs/README.md、docs/lessons.md、docs/handoff/README.md 三新文件）**写面不相交**（RESEARCH §4 白名单）；你只跑纯件验证（无全局键鼠），不触验证闸门。

## 3. Phase 任务（依序）

**P1 归档**：`mkdir docs/handoff/archive` 后 `git mv` 全部 45 份——根目录 `RESEARCH-V1..V7.md`（7）、`PLAN-V5/V6/V7.md`（3）、`GOAL-V{1,2,3,4,5A,5B,5C,5D,5I,6A,6B,6I,7S,7V,7R,7I}-PROMPT.md`（16）、`docs/` 下 `VERIFY-*`、`REVIEW-V7`、`COPY-V5B/V5C`、`MIGRATE-V5D/V6-PC2`、`PUBLISH-V5A`（19）。核对：`git status` 恰 45 项全 `renamed`、工作树无其他变更；提交一次（原子可追溯）。
**P2 协议演进**：`AGENTS.md` 按 RESEARCH §3.1 定稿文本照抄改写（§1 三层路由/§2 追加一行/§4 冻结区路径），总行数仍 ≤60；`README.md` 只改「📖 开发过程文档」一节的表格与表内链接（指向 `docs/handoff/archive/`、`docs/lessons.md`、`docs/multi-window.md`、批次目录），`git diff README.md` 核对其余正文零改动。
**P3 worktree 规程**：新建 `docs/multi-window.md` ≤80 行，内容＝RESEARCH §4.1 第 3 条大纲（创建模板含查重与「撞名即停」、路径规范 `..\OneClickBlackScreen-WorkTree\v?-〈轨〉`、清理模板 remove+prune+branch -d、壳内禁散物、主检出写权归属＝集成窗＋规划窗例外）。
**P4 验证**：`build.cmd` 绿；纯件三件各 exit 0（`verify\bin\VerifyConfig.exe`、`VerifyIcon.exe`、`VerifyUpdate.exe`，调用形态参照 `verify.cmd` 头注释；**不得跑 GUI 四件**——与 L 并行时段守约，全套留给 R/I）；全仓 markdown 相对链接死链扫描（Flash 子代理扇出，回传死链清单）；`git log --follow docs/handoff/archive/RESEARCH-V1.md` 等抽 3 份确认历史保留。
**P5 留档推送**：`docs/handoff/2026-11-05-v8-structure/VERIFY-V8S.md`（真实命令输出粘贴，含 45/45 renamed 清单、纯件三件输出、死链扫描结果、--follow 抽查）；提交 `docs(S): V8 结构归档…`；`git push -u origin feature/structure-v8`。

## 4. 铁律

写面白名单（对 main 的 diff 必须全部落入）：45 份 git mv、`AGENTS.md`、`README.md`（仅开发过程文档表）、`docs/multi-window.md`(新)、`docs/handoff/2026-11-05-v8-structure/VERIFY-V8S.md`(新)。白名单外一律只读。禁补丁式修改（改动须融入既有结构）；AGENTS 定稿文本照抄不得改写；冻结：`baseline/**`、`migrate/**`、`src/**`、`verify/**` 源码、构建脚本、`使用说明.txt`、`src/AppInfo.cs`（本轮无发版）；主检出只读（worktree 内工作）；证据＝真实命令输出留档，不是结论式转述；不删仓库、凭证不入文件。

## 5. 预期触碰文件（冲突面申报，供 L/R/I 预判）

与 L 轨零交集（L 全新文件）；README.md 仅本轨改（L/R/I 不改其内容）；AGENTS.md 仅本轨改。git mv 仅移动不修改内容（45 份原文件字节不变）。

## 6. 验收清单（校验器逐条核对实据）

- [ ] `git ls-remote --heads origin feature/structure-v8` 可见且指向本轨提交
- [ ] `git diff --name-status origin/main...HEAD`：45 项 R（renamed）＋白名单内 M/A，无白名单外路径
- [ ] VERIFY-V8S.md 含 `git status` 45/45 renamed 实录与三份 `--follow` 输出
- [ ] build.cmd exit 0；VerifyConfig/VerifyIcon/VerifyUpdate 各 exit 0（输出留档）
- [ ] 死链扫描零死链（扫描范围与结果留档）；README 正文除目标表外零 diff
- [ ] AGENTS.md ≤60 行且 §1/§2/§4 与 RESEARCH §3.1 逐字一致；docs/multi-window.md ≤80 行
- [ ] 无 GUI 验证器运行记录（与并行窗守约）

## 7. 收尾动作

VERIFY-V8S.md 末尾附「移交注记」（发现他轨/越界问题记此，不改别人写面）；推送后报告：分支名、提交哈希、验收清单逐条 ✅/❌。R 轨将复检你的全部产出并预合并。

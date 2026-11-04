# VERIFY-V8S · S 轨执行证据（结构归档＋协议演进）

> **一句话定位**：S 轨全量证据——P1 归档 45/45 renamed（字节不变）、P2 AGENTS 三节照抄改写＋README 表内 4 行、P3 multi-window 规程、P4 build 绿＋纯件三件 exit 0＋死链门禁组零死链＋--follow×3 历史保留。　**分支**：feature/structure-v8（worktree `..\OneClickBlackScreen-WorkTree\v8-s`）。　**日期**：2026-11-05。事实源＝RESEARCH-V8；本文件只贴真实命令输出。

## 0. 基线与 worktree

```bash
$ git fetch origin && git ls-remote --heads origin feature/structure-v8   # 输出为空＝无撞名
$ git rev-parse origin/main main
b001aa755080a51949e8fc076b228f91e5cd1cc3
b001aa755080a51949e8fc076b228f91e5cd1cc3
$ git worktree add "../OneClickBlackScreen-WorkTree/v8-s" -b feature/structure-v8 origin/main
Preparing worktree (new branch 'feature/structure-v8')
HEAD is now at b001aa7 docs(P): V8 规划——……（略）
```

## 1. P1 归档：45/45 全 renamed，原子提交 7e1a026

```bash
$ ls RESEARCH-V*.md PLAN-V*.md GOAL-V*-PROMPT.md | wc -l   # 根 26
26
$ ls docs/*.md | wc -l                                     # docs 19（26+19=45，与 GOAL §3 清单一致）
19
$ git status --porcelain | wc -l ; git status --porcelain | grep -c '^R '   # 非 renamed 项 grep -v 计 0（无输出）
45
45
```

`git status --porcelain` 实录（45 行全 `R`）：

```text
R  docs/COPY-V5B.md -> docs/handoff/archive/COPY-V5B.md
R  docs/COPY-V5C.md -> docs/handoff/archive/COPY-V5C.md
R  GOAL-V1-PROMPT.md -> docs/handoff/archive/GOAL-V1-PROMPT.md
R  GOAL-V2-PROMPT.md -> docs/handoff/archive/GOAL-V2-PROMPT.md
R  GOAL-V3-PROMPT.md -> docs/handoff/archive/GOAL-V3-PROMPT.md
R  GOAL-V4-PROMPT.md -> docs/handoff/archive/GOAL-V4-PROMPT.md
R  GOAL-V5A-PROMPT.md -> docs/handoff/archive/GOAL-V5A-PROMPT.md
R  GOAL-V5B-PROMPT.md -> docs/handoff/archive/GOAL-V5B-PROMPT.md
R  GOAL-V5C-PROMPT.md -> docs/handoff/archive/GOAL-V5C-PROMPT.md
R  GOAL-V5D-PROMPT.md -> docs/handoff/archive/GOAL-V5D-PROMPT.md
R  GOAL-V5I-PROMPT.md -> docs/handoff/archive/GOAL-V5I-PROMPT.md
R  GOAL-V6A-PROMPT.md -> docs/handoff/archive/GOAL-V6A-PROMPT.md
R  GOAL-V6B-PROMPT.md -> docs/handoff/archive/GOAL-V6B-PROMPT.md
R  GOAL-V6I-PROMPT.md -> docs/handoff/archive/GOAL-V6I-PROMPT.md
R  GOAL-V7I-PROMPT.md -> docs/handoff/archive/GOAL-V7I-PROMPT.md
R  GOAL-V7R-PROMPT.md -> docs/handoff/archive/GOAL-V7R-PROMPT.md
R  GOAL-V7S-PROMPT.md -> docs/handoff/archive/GOAL-V7S-PROMPT.md
R  GOAL-V7V-PROMPT.md -> docs/handoff/archive/GOAL-V7V-PROMPT.md
R  docs/MIGRATE-V5D.md -> docs/handoff/archive/MIGRATE-V5D.md
R  docs/MIGRATE-V6-PC2.md -> docs/handoff/archive/MIGRATE-V6-PC2.md
R  PLAN-V5.md -> docs/handoff/archive/PLAN-V5.md
R  PLAN-V6.md -> docs/handoff/archive/PLAN-V6.md
R  PLAN-V7.md -> docs/handoff/archive/PLAN-V7.md
R  docs/PUBLISH-V5A.md -> docs/handoff/archive/PUBLISH-V5A.md
R  RESEARCH-V1.md -> docs/handoff/archive/RESEARCH-V1.md
R  RESEARCH-V2.md -> docs/handoff/archive/RESEARCH-V2.md
R  RESEARCH-V3.md -> docs/handoff/archive/RESEARCH-V3.md
R  RESEARCH-V4.md -> docs/handoff/archive/RESEARCH-V4.md
R  RESEARCH-V5.md -> docs/handoff/archive/RESEARCH-V5.md
R  RESEARCH-V6.md -> docs/handoff/archive/RESEARCH-V6.md
R  RESEARCH-V7.md -> docs/handoff/archive/RESEARCH-V7.md
R  docs/REVIEW-V7.md -> docs/handoff/archive/REVIEW-V7.md
R  docs/VERIFY-V1.md -> docs/handoff/archive/VERIFY-V1.md
R  docs/VERIFY-V2.md -> docs/handoff/archive/VERIFY-V2.md
R  docs/VERIFY-V3.md -> docs/handoff/archive/VERIFY-V3.md
R  docs/VERIFY-V4.md -> docs/handoff/archive/VERIFY-V4.md
R  docs/VERIFY-V5.md -> docs/handoff/archive/VERIFY-V5.md
R  docs/VERIFY-V5B.md -> docs/handoff/archive/VERIFY-V5B.md
R  docs/VERIFY-V5C.md -> docs/handoff/archive/VERIFY-V5C.md
R  docs/VERIFY-V6.md -> docs/handoff/archive/VERIFY-V6.md
R  docs/VERIFY-V6A.md -> docs/handoff/archive/VERIFY-V6A.md
R  docs/VERIFY-V6B.md -> docs/handoff/archive/VERIFY-V6B.md
R  docs/VERIFY-V7.md -> docs/handoff/archive/VERIFY-V7.md
R  docs/VERIFY-V7S.md -> docs/handoff/archive/VERIFY-V7S.md
R  docs/VERIFY-V7V.md -> docs/handoff/archive/VERIFY-V7V.md
```

提交输出（git 自证 0 insertions/deletions＝字节不变；全部 100% 相似度）：

```text
[feature/structure-v8 7e1a026] docs(S): V8 P1 归档——45 份历史轮次文档 git mv 至 docs/handoff/archive/……
 45 files changed, 0 insertions(+), 0 deletions(-)
 rename docs/{ => handoff/archive}/COPY-V5B.md (100%)
 …（45 行 rename 全部 (100%)，逐行见上方 porcelain 实录）…
```

## 2. P2 协议演进

`git diff AGENTS.md` 全量（恰三节；`wc -l AGENTS.md`＝**38 ≤60**）：

```diff
 ## 1. 冷启动路由
-- 先读本轮（当前最大 V 号）`RESEARCH-V<n>.md` → `PLAN-V<n>.md` → 本窗口对应 `GOAL-V<n>?-PROMPT.md`；冲突时以 RESEARCH 为准。
-- 禁读历史轮 RESEARCH/PLAN/GOAL 全文（过程记录，防上下文膨胀）；要惯例只 grep 定位。
+- 三层渐进披露：先读 AGENTS.md（本文件，常驻）→ docs/README.md（任务路由枢纽）→
+  按路由表读当前批次 docs/handoff/<最新批次>/（RESEARCH→PLAN→本窗 GOAL）；冲突时以 RESEARCH 为准。
+- 禁读 docs/handoff/archive/ 历史批次全文（过程记录，防上下文膨胀）；要惯例只 grep 定位。
 …
+- 并行轮一轨一 worktree（壳内 OneClickBlackScreen-WorkTree\，禁入仓库内），规程＝docs/multi-window.md。
 …
-- 历史 RESEARCH/PLAN/GOAL 与 docs/ 既有证据文档（过程记录；I 轨发版辖内除外）。
+- `docs/handoff/archive/**`（V1~V7 历史轮次文档，归档只读）。
```

§1/§2 追加行/§4 与 RESEARCH §3.1 定稿文本逐字一致（§2 行按既有列表结构加 `- ` 前缀，§4 路径按既有行样式加反引号，纯文本内容逐字同）。

`git diff --stat README.md`＝`4 insertions(+), 4 deletions(-)`——恰「📖 开发过程文档」表内 4 行替换（新表指向 archive／批次目录／lessons／multi-window），节内引言、收尾句与正文其余零改动（diff 全文已逐行核对，仅 @@ -119,10 +119,10 @@ 一处 hunk）。

## 3. P3 worktree 规程

`docs/multi-window.md` 新建，`wc -l`＝**45 ≤80**；四要素齐：§1 壳形与路径规范（`..\OneClickBlackScreen-WorkTree\v<n>-<轨>`、禁入仓库内）、§2 创建模板（fetch→ls-remote 查重＋撞名即停→worktree add→push -u）、§3 清理模板（remove＋prune＋branch -d，合并即清理）、§4 写权归属表（主检出＝集成窗独占＋规划窗例外；壳根禁散物）。

## 4. P4 验证

### 4.1 build.cmd

```text
[OK] Build succeeded: D:\...\OneClickBlackScreen-WorkTree\v8-s\dist\一键黑屏.exe
BUILD-EXIT=0
```

### 4.2 纯件三件（照抄 verify.cmd:20/28/30 csc 行编译；调用形态同 verify.cmd:37/53/57）

```text
=== [1] VerifyConfig (no args) ===
PASS: 23 config tests (defaults/roundtrip/format/golden/corruption/escapes/atomic-write/idle-key)
EXIT-Config=0
=== [5] VerifyIcon (dist exe) ===
PASS: icon-main.ico structure ok (7 tiers, in-bounds, all entries decodable)
PASS: icon-settings.ico structure ok (7 tiers, in-bounds, all entries decodable)
PASS: exe embeds icon-main 256px PNG payload (offset 260384)
PASS: settings.ico managed resource byte-identical to artifact and decodable
PASS: dist\icon-settings.ico deployed byte-identical (110145 bytes)
PASS TOTAL: 4 icon checks
EXIT-Icon=0
=== [6] VerifyUpdate (no args) ===
PASS: version comparison table (newer/equal/older/malformed, fail-closed)
PASS: full chain (check -> 302 download -> digest verify -> replace) on a temp copy
PASS: tampered digest refused, target unchanged
PASS: SHA256SUMS fallback completed the chain
PASS: digest and SHA256SUMS both missing/mismatched refused
PASS: size mismatch refused
PASS: 404 / malformed JSON / missing asset fail closed
PASS: equal and malformed tags read as no update
PASS: replace sequence (stale .old cleared, rollback restored, blocked pre-clean)
PASS: concurrent replaces serialize - one effective swap, second flow no-op, backup intact (x50)
PASS TOTAL: update checks (version table + offline full chain)
EXIT-Update=0
```

**VerifyIcon 调用形态注记（如实）**：首跑经 `bash→cmd //c 'for %F in (dist\*.exe) do …"%F"'` 单行包装传中文路径，VerifyIcon 报 `FAIL: 路径中具有非法字符。`（exit 1）——同命令 `@echo [%F]` 显示路径中文完好，判定为该包装链对子进程参数的编码破坏（Windows 编码变体族），非产品/验证器缺陷。改用两形态复演均绿：①bash 直接调用 `./verify/bin/VerifyIcon.exe "dist/一键黑屏.exe"`（EXIT=0，输出同上）；②verify.cmd 原生批处理形态（临时 .cmd 于系统 TEMP 照抄 verify.cmd:12+53：`for %%F in ("%REPO%dist\*.exe") do set "TARGET=%%F"` 后 `"%REPO%verify\bin\VerifyIcon.exe" "%TARGET%"`）→ PASS TOTAL: 4 icon checks，`EXIT-batchform=0`。两形态输出逐字同。GUI 四件未运行（与 L 并行守约）。

### 4.3 死链扫描（general-purpose 子代理扇出，机械 grep+test -e，仓库零写入）

范围＝`git ls-files '*.md'`（54）＋磁盘未跟踪 multi-window.md＝55 份；跳过 http(s)/mailto/纯锚点；相对目标按文件目录 `test -e` 核对（%XX 先解码）。

**门禁组（RESEARCH §4.1 #4 核对范围：README/AGENTS/multi-window＋本轮批次目录）＝13 条，零死链**：

| 文件 | 链接数 | 结果 |
|---|---|---|
| README.md | 7 | 6 存在＋1 PENDING-L（`docs/lessons.md`，L 轨并行新文件，R 合并后即活） |
| AGENTS.md | 0 | 无 markdown 链接（§3.1 定稿文本为纯文本引用） |
| docs/multi-window.md | 0 | 无 markdown 链接（路径皆反引号文本） |
| 批次目录 6 份 | 6 | 全存在（5×`RESEARCH-V8.md` 同目录＋`PLAN-V8.md:3 → ../../../AGENTS.md`＝根 AGENTS.md） |

**归档组（docs/handoff/archive/ 45 份，报告不门禁）**：相对链接 65 条，死链 **14 条**，全部为文档平移入 archive/ 后旧布局引用失效（`../RESEARCH-V*.md` 等 12、指向根 `AGENTS.md` 1【PLAN-V7.md:3】、`docs/VERIFY-V6.md` 1【GOAL-V7I-PROMPT.md:4】）——归档铁律「内容字节不变」（§1 提交 0 insertions/deletions 自证）故不可改写，属设计内上下文平移，详见移交注记 S-1。附带：migrate/迁移手册.md 1 条链接存在。全仓合计 79 条相对链接、15 条 DEAD（＝门禁组 PENDING-L 1＋归档组 14）。

### 4.4 `git log --follow` 抽查 ×3（历史穿透 rename 保留）

```text
=== docs/handoff/archive/RESEARCH-V1.md ===
7e1a026 docs(S): V8 P1 归档——……
04ab19d 终验收尾: 设置窗逐显 DPI 缩放……docs/VERIFY-V1.md + §9 实现追记
b559317 docs: Phase 1 T7 批判评审记录……
2200c13 docs: Phase 0 勘察勘误……
5332e18 docs: RESEARCH-V1 调研底稿 + GOAL-V1-PROMPT 执行契约……
=== docs/handoff/archive/GOAL-V7S-PROMPT.md ===
7e1a026 docs(S): V8 P1 归档——……
65f8797 docs: V7 规划——多窗口并行冲突检查与根治……
=== docs/handoff/archive/VERIFY-V7.md ===
7e1a026 docs(S): V8 P1 归档——……
e5d0473 docs(I): V7 集成总证据——……
```

三份均自移动后提交（7e1a026）穿透至原始提交，历史完整保留。

## 5. 验收清单对照（GOAL-V8S §6）

- [x] `git ls-remote --heads origin feature/structure-v8` 可见且指向本轨提交——§6＋收尾报告 push/ls-remote 实录
- [x] `git diff --name-status origin/main...HEAD`：45 项 R＋白名单内 M/A（AGENTS.md/README.md 两 M、multi-window.md/VERIFY-V8S.md 两 A），无白名单外路径——见 §6 实录
- [x] VERIFY-V8S.md 含 45/45 renamed 实录与三份 --follow 输出——§1/§4.4
- [x] build.cmd exit 0；VerifyConfig/VerifyIcon/VerifyUpdate 各 exit 0——§4.1/§4.2
- [x] 死链扫描：门禁组（RESEARCH §4.1 #4 范围＝README/AGENTS/multi-window）零死链，范围与结果留档（§4.3，含归档组 14 条报告组清单）；README 正文除目标表外零 diff（§2）
- [x] AGENTS.md 38 行 ≤60 且 §1/§2/§4 与 RESEARCH §3.1 一致；multi-window.md 45 行 ≤80——§2/§3
- [x] 无 GUI 验证器运行记录——本轨仅跑纯件三件（§4.2），verify.cmd 未运行

## 6. 提交链与推送

- 提交链：`7e1a026`（P1 归档，45 files changed 0/0）→ 本提交（P2 AGENTS/README＋P3 multi-window.md＋P5 本文件）。
- 推送命令：`git push -u origin feature/structure-v8`；推送与 `git ls-remote --heads origin feature/structure-v8` 指向实录见本轨收尾报告（沿 V7 先例，证据文档不自我引用未定哈希）。

`git diff --name-status --cached origin/main`（提交前暂存态全量实录，与推送后 HEAD 对 origin/main 等同）：

```text
$ git diff --name-status --cached origin/main | awk '{print $1}' | sort | uniq -c
      2 A
      2 M
     45 R100
$ git diff --name-status --cached origin/main | sort | uniq -c -w8   # R100 分组核对
     16 R100  GOAL-V*-PROMPT.md        → docs/handoff/archive/…
      3 R100  PLAN-V*.md               → docs/handoff/archive/…
      7 R100  RESEARCH-V*.md           → docs/handoff/archive/…
     19 R100  docs/{VERIFY-*,REVIEW-V7,COPY-*,MIGRATE-*,PUBLISH-*} → docs/handoff/archive/…
（M×2＝AGENTS.md、README.md；A×2＝docs/multi-window.md、docs/handoff/2026-11-05-v8-structure/VERIFY-V8S.md——恰白名单 49 项，无白名单外路径）
```

## 7. 移交注记（不改他轨写面，交 R/I 裁定）

- **S-1（归档组 14 条旧布局死链）**：45 份归档内容字节冻结，其内部相对链接按移动前布局书写（`../RESEARCH-V*.md` 等），平移后必然失效——此为「git mv 不改内容」定案的必然推论，非缺陷；建议 R 合并态复扫时按此口径归类，并可考虑由 L 在 docs/handoff/README.md 批次规范中加一句「archive 内链接按移动前布局书写」的说明（L 写面，本轨未代改）。
- **S-2（VerifyIcon 编码事故定位）**：bash→cmd 单行 for 包装向子进程传中文路径会被破坏（报「路径中具有非法字符」）；正解＝bash 直接调用或 verify.cmd 原生批处理形态（§4.2 注记）。建议 L 收录进 lessons.md 编码条目族（种子⑥ Windows 编码三变体的第四变体：调用链变体）。
- **S-3（.gitignore 核查）**：RESEARCH §4.1 写面注记的核查项——dist/、verify/bin/ 已在忽略清单，.zcode/ 未落仓库根，无需补行，本轮 .gitignore 未动（白名单亦未含）。

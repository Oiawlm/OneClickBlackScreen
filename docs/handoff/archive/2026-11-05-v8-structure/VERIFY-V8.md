# VERIFY-V8 · I 轨集成总证据

> **一句话定位**：I 轨集成收尾实录——main `--no-ff` 合并 feature/review-v8（**700546b**，含 S＋L＋REVIEW 全链）零冲突；build 绿＋verify 七件 ×2 全绿；零足迹三态一致；终态检查零产品变更（src/ 零 diff、AppInfo 仍 1.0.2 唯一命中、README 恰表内 4 行）；REVIEW 移交注记 8 条逐条闭环/备案；worktree×3＋本地分支×3 清理、远端分支照 V6/V7 惯例保留；推送 origin main。**本轮零产品变更：无发版、无 tag、AppInfo 不动。**　**分支**：main（主检出）。　**日期**：2026-11-05。事实源＝RESEARCH-V8。

## 0. 前置确认

```text
$ git status → On branch main / up to date with 'origin/main' / working tree clean（基点 b001aa7）
$ git ls-remote --heads origin → feature/review-v8=822b1bb 可见 ✓（另见 structure-v8=c55346d、docs-router-v8=b74ea6b）
REVIEW-V8 总评 = GO(V8R)，无阻断项 ✓ → 按波次表进入 I 轨
```

## 1. P1 合并实录

```text
$ git merge --no-ff origin/feature/review-v8 -m "merge: V8 结构与合作协议根修合入 main（S 归档45份+三层路由 ∥ L 路由枢纽+坑库+批次规范 + R 复检GO）——首次双窗 worktree 并行实证"
Merge made by the 'ort' strategy.  54 files changed, 633 insertions(+), 7 deletions(-)
$ git log --merges -1 --format="%H %s"
700546b3a35497c7bdafecbe39635d1e5b2ffd60 merge: V8 结构与合作协议根修合入 main（…）——首次双窗 worktree 并行实证
```

- **零冲突**（R 预演排除兑现，RESEARCH §5 R4 预案未触发）；54 files＝45 R100（归档 git mv，100% 相似度）＋7 A（docs/{README,lessons,multi-window}.md、docs/handoff/README.md、批次内 REVIEW-V8/VERIFY-V8S/VERIFY-V8L.md）＋2 M（AGENTS.md、README.md）——与 REVIEW §0 合并态清单逐项一致，无白名单外路径。
- 提交链：b001aa7（main 基）→ 7e1a026＋c55346d（S 两提交）→ 629defc（R 预集成 merge S）→ b74ea6b（L 提交）→ 74f79b3（R 预集成 merge L）→ 822b1bb（REVIEW-V8.md）→ **700546b**（本轮 I 合并提交，`git log --merges -1` 可见、信息完整）。

## 2. P2 终验实录（2026-11-05，本窗独占时段；用户守则已提前提醒）

```text
$ cmd //c build.cmd → [OK] Build succeeded: …\OneClickBlackScreen-main\dist\一键黑屏.exe   BUILD-EXIT=0   04:51
$ cmd //c verify.cmd → [1/7]VerifyConfig … [7/7]VerifyIdle → [OK] ALL VERIFIERS PASSED   VERIFY-EXIT=0
  ROUND1: 04:52:01 → 04:52:32   七件全 PASS（Overlay PASS Z: user's real settings.ini untouched；Icon 4 checks；Update version table+offline full chain 含 concurrent x50；Idle 5 checks）
  ROUND2: 04:52:40 → 04:53:11   七件全 PASS → [OK] ALL VERIFIERS PASSED（两轮独立运行，间隔实录如上；无 BUSY 撞闸）
```

零足迹审计（前态 04:51:52 → 中态 04:52:37 → 后态 04:53:15，三态比对）：

| 审计项 | 前态 | 中态 | 后态 | 判定 |
|---|---|---|---|---|
| `%LOCALAPPDATA%\BlackScreen\settings.ini` | ABSENT | ABSENT | ABSENT | ✅ 三态一致 |
| 计划任务 `OneClickBlackScreen-Idle`（`cmd //c schtasks /query`） | 系统找不到指定的文件 | 同左 | 同左 | ✅ 三态一致 |
| 残留进程（tasklist 过滤七验证器名） | 无 | 无 | 无 | ✅ 零残留 |

跑后 `git status --short` 仍空（验证未污染主检出）。注：本窗 schtasks 全程 `cmd //c` 包裹＝REVIEW 移交注记 R-1（MSYS 路径转换）的规避实践再次实证。

## 3. P3 终态检查

| 检查项 | 实据 | 判定 |
|---|---|---|
| `git diff origin/main...HEAD -- src/` | 0 行（空输出）＝产品源码零变更直接证据 | ✅ |
| `git diff origin/main...HEAD --stat -- verify/ build.cmd build.rsp verify.cmd install.ps1 使用说明.txt` | 0 行（空输出）＝验证源码/构建链/使用说明零变更 | ✅ |
| `grep -rn "1\.0\.2" src/AppInfo.cs` | 唯一命中 `src/AppInfo.cs:11: public const string Version = "1.0.2";` | ✅ |
| README.md 对 origin/main diff | 恰单 hunk `@@ -119,10 +119,10 @@` 开发过程文档表内 4 行替换（改指 archive/＋当前批次＋lessons＋multi-window），正文其余零改动＝与 v1.0.2 门面一致 | ✅ |
| 三层路由冷启动演练 | AGENTS.md（38 行 ≤60，§1 新三层路由文本）→ docs/README.md 路由表（「执行轮次任务｜先读当前批次 GOAL」）→ `ls docs/handoff/` 排除 README/archive 后唯一批次 `2026-11-05-v8-structure`（RESEARCH/PLAN/GOAL×4/REVIEW/VERIFY 齐备）——路径逐级可打开 | ✅ |
| AGENTS/README 终态 | 与 R 复检（REVIEW §1 S-5/S-6、§4.1）描述逐项一致，本窗未触碰 | ✅ |

## 4. P4 移交注记处置表（REVIEW-V8 §5 共 8 条，逐条去向）

| 注记 | 内容摘要 | 处置 | 去向实据 |
|---|---|---|---|
| S-1 | 归档组 14 条旧布局死链（内容冻结必然） | **备案不修** | 规则面已覆盖：docs/lessons.md 头部出处口径＋docs/handoff/README.md「归档后引用一律写全路径」；本表引用 REVIEW-V8 §5 S-1 行即闭环动作 |
| S-2 | VerifyIcon bash→cmd 包装链中文路径破坏（编码第四变体） | **备案，下轮 lessons 追加候选** | L-006 已收三变体，本条同族增补候选；L 写面本轮冻结不代改（单一写者契约） |
| S-3 | .gitignore 核查＝无需补行 | **闭环（无需处置）** | S 轨核查结论在案 |
| L-1 | verify.cmd 头注释为套件级说明，路由表按实际表述 | **闭环** | docs/README.md 展开行已按实际表述（R 复核 L-3 表逐字一致） |
| L-2 | multi-window.md＋archive/ 待合并态复核 | **闭环** | R 已于合并态复核通过（REVIEW §2 L-6/§4.4 死链复扫 0 死链） |
| L-3 | lessons 引用两系映射口径 | **闭环** | R 按此口径死链复扫 0 死链，口径成立（REVIEW §4.4） |
| R-1 | git bash MSYS 路径转换破坏 schtasks 参数，须 `cmd //c` 包裹 | **备案，下轮 lessons 候选** | 本窗零足迹审计三度实跑 `cmd //c schtasks` 全成功＝规避实践实证；与 L-006 相邻但机理不同（路径转换非编码） |
| R-2 | GOAL-V8R §6 计数「S 六项」实列 7 项 | **备案（文档计数精度）** | R 已按实有 7 项全覆盖无实际影响；GOAL 属过程记录不改写 |

8/8 均有去向：5 闭环＋3 备案（其中 S-2/R-1 为下轮 lessons 追加候选，随下轮批次交接）。无 no-go，无退回。

## 5. P5 清理实录

```text
$ git worktree list（清理前）→ v8-s(c55346d)/v8-l(b74ea6b)/v8-r(822b1bb) 三枚在列
$ git worktree remove ../OneClickBlackScreen-WorkTree/v8-s（v8-l、v8-r 同）→ 无报错
$ git worktree prune && git worktree list → 仅剩 OneClickBlackScreen-main 700546b [main]   ✅ 无 v8-* 残留
$ git branch -d feature/structure-v8 feature/docs-router-v8 feature/review-v8
  Deleted feature/structure-v8 (was c55346d). / Deleted feature/docs-router-v8 (was b74ea6b).
  Deleted feature/review-v8 (was 822b1bb).                                              ✅ -d 安全删除＝三 tip 均已入 main
$ ls ../OneClickBlackScreen-WorkTree/ → 空目录留存（下轮并行复用）   ✅
```

远端分支处置：`git ls-remote --heads origin` 观察历史惯例——V6 两支（feature/exit-arm-v6、feature/verify-env-v6）＋V7 两支（feature/update-guard-v7、feature/verify-isolation-v7）**全部留存**；照惯例 V8 三支（structure/docs-router/review-v8）**远端保留不删**，如实在案。本地 V6/V7 历史分支亦留存（本轮只删 V8 三支）。

## 6. P6 留档与推送

- 本文件＋本轮合并提交 → `git push origin main`；推送后 `origin/main`＝本地 main（700546b 之后含本提交）。
- 本批次目录 `docs/handoff/2026-11-05-v8-structure/` 完结后由**下轮规划窗**移入 archive/（GOAL §3 P6 尾注，不属本轨动作）。
- 事故注记：本窗无事故；仅记录 bash 环境中文输出乱码现象（=lessons L-006 已收录机理，命令改英文标签规避，不新增条目）。

## 7. 验收清单对照（GOAL-V8I §6）

- [x] main 含 `--no-ff` 合并提交 700546b（`git log --merges -1` 可见、信息完整）；推送后 origin/main 与本地一致
- [x] 合并态对 origin/main 无 `src/**`/`verify/**` 源码/构建脚本/使用说明/AppInfo 任何变更（§3 前两行 diff 均空输出）
- [x] verify.cmd 七件 ×2 全绿两份独立实录（§2 两轮时间戳）；零足迹三态一致实录（§2 表）
- [x] 本文件含移交注记处置表 8/8 有去向（§4）
- [x] `git worktree list` 无 v8-s/l/r；本地三分支已删；远端分支照惯例保留已记录（§5）；WorkTree 父目录留存
- [x] 三层路由终态演练通过（§3 末行）；README/AGENTS 终态与 R 复检一致（§3）

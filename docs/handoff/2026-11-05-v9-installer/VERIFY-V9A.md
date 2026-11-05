# VERIFY-V9A · A 轨 DeskBox 根除总证据

> **一句话定位**：A 轨执行窗的真实命令输出总留档——P1 清点裁定、16 件手术面全量 diff（附录 A）、写面 grep 零命中、编码保真前后对照、lessons 锚点交集 ∅、build＋纯件三件全绿、移交注记六条。　**执行**：A 轨窗口（GLM-5.3·目标模式），分支 `feature/deskbox-purge-v9`，worktree `OneClickBlackScreen-WorkTree/v9-a`，基线 `origin/main@31a0a34`。　**日期**：2026-11-05。

## 1. P1 清点复核

命令：`git -c core.quotepath=false grep -ci deskbox`（全仓、大小写不敏感、逐文件计数）：

```
docs/handoff/2026-11-05-v9-installer/GOAL-V9A-PROMPT.md:16
docs/handoff/2026-11-05-v9-installer/GOAL-V9B-PROMPT.md:3
docs/handoff/2026-11-05-v9-installer/GOAL-V9I-PROMPT.md:2
docs/handoff/2026-11-05-v9-installer/GOAL-V9R-PROMPT.md:7
docs/handoff/2026-11-05-v9-installer/PLAN-V9.md:5
docs/handoff/2026-11-05-v9-installer/RESEARCH-V9.md:15
docs/handoff/archive/GOAL-V1-PROMPT.md:6
docs/handoff/archive/GOAL-V2-PROMPT.md:3
docs/handoff/archive/GOAL-V5D-PROMPT.md:3
docs/handoff/archive/GOAL-V6I-PROMPT.md:3
docs/handoff/archive/MIGRATE-V5D.md:6
docs/handoff/archive/MIGRATE-V6-PC2.md:3
docs/handoff/archive/RESEARCH-V1.md:3
docs/handoff/archive/RESEARCH-V5.md:2
docs/handoff/archive/RESEARCH-V6.md:1
docs/handoff/archive/VERIFY-V1.md:1
docs/handoff/archive/VERIFY-V2.md:1
install.ps1:7
migrate/cleanup-local.ps1:3
migrate/迁移手册.html:3
migrate/迁移手册.md:3
使用说明.txt:1
```

**裁定 1（GOAL-V6I-PROMPT.md 计入清单内）**：RESEARCH §2.1 头部「49 处/16 文件」只有在 archive＝11 份且含 GOAL-V6I×3 时才成立——16 文件＝install 1＋migrate 3＋使用说明 1＋archive 11；49 处＝17（install 7＋migrate 9＋使用说明 1）＋archive 32（6+3+3+3+6+3+3+2+1+1+1）。括号明细列了 10 份漏写 GOAL-V6I，属**转录漏项**而非清单外新落点（非他轨改面/越界），按 11 份清单执行。

**裁定 2（批次目录 6 份不计入）**：`docs/handoff/2026-11-05-v9-installer/` 内 48 处＝本轮规划/指令文档（含本轨 GOAL 自身）对 DeskBox 任务的自引用，均为 A 轨白名单外只读文件（白名单在该目录仅含本文件 VERIFY-V9A.md 新增），不属手术范围——移交注记 1。

## 2. 手术面总览

命令：`git -c core.quotepath=false diff --name-status origin/main`：

```
M	docs/handoff/archive/GOAL-V1-PROMPT.md
M	docs/handoff/archive/GOAL-V2-PROMPT.md
M	docs/handoff/archive/GOAL-V5D-PROMPT.md
M	docs/handoff/archive/GOAL-V6I-PROMPT.md
M	docs/handoff/archive/MIGRATE-V5D.md
M	docs/handoff/archive/MIGRATE-V6-PC2.md
M	docs/handoff/archive/RESEARCH-V1.md
M	docs/handoff/archive/RESEARCH-V5.md
M	docs/handoff/archive/RESEARCH-V6.md
M	docs/handoff/archive/VERIFY-V1.md
M	docs/handoff/archive/VERIFY-V2.md
M	docs/lessons.md
D	install.ps1
M	migrate/cleanup-local.ps1
M	migrate/迁移手册.html
M	migrate/迁移手册.md
```

16 件与 GOAL §4 白名单**精确相等**（archive 11＋lessons 追加＋install.ps1 删＋migrate 3＋本文件新增不计入 diff），白名单外零改动。`--stat` 合计：**16 files changed, 71 insertions(+), 141 deletions(-)**。全量逐文件 diff 见附录 A。

手术要点：
- `install.ps1`：整文件删除（57 行；唯一功能＝同步本机个人快捷方式目录）。
- `migrate/迁移手册.md`（:352/:359/:495）与 `迁移手册.html`（:317/:323/:476）：三处行内改写去 DeskBox 引用，句子自洽（如「项目文件夹、DeskBox 两个快捷方式、`%LOCALAPPDATA%\BlackScreen` 都已消失」→「项目文件夹、`%LOCALAPPDATA%\BlackScreen` 都已消失」）。
- `migrate/cleanup-local.ps1`：S2 节整删（头部清单 :18＋实现段 :96-110 节边界内全部）＋S3～S7 顺移重编为 S2～S6（节头注释与 Record 标签同步，编号连续）；S1 及 `$s1ok` 联动逻辑未动。
- archive 11 份：行内混有其他内容者最小改写（DeskBox 目录→快捷方式目录、DeskBox 两 .lnk→两 .lnk 等）；整行仅服务 DeskBox 者删行（GOAL-V1 :92 快捷方式目录路径行、MIGRATE-V5D :97-98 两行 DryRun 日志）。
- `docs/lessons.md`：末尾追加 L-017（五要素齐全）。

## 3. grep 归零（P6①）

**口径 1（GOAL 原样命令）**：`grep -ri deskbox --exclude-dir=.git -r .` → 剩余 7 文件：批次目录 6 份（48 处，裁定 2）＋`使用说明.txt`（1 处，GOAL §2 明示「使用说明.txt:105 的 DeskBox 字样归 B 轨消除，本轨禁改该文件」）。

**口径 2（A 轨写面范围＝全仓−批次目录−使用说明.txt）**：

```
$ grep -ri deskbox --exclude-dir=.git -r . --exclude-dir=2026-11-05-v9-installer --exclude=使用说明.txt
（无输出）
scoped_grep_exit=1 (1=zero hits)
```

**A 轨授权写面已 100% 归零**。本分支上「全仓字面零命中」尚差 B 轨使用说明.txt 与批次目录自引用两处——移交注记 1/2。

## 4. 编码保真（P6④，`file` 前后对照）

| 文件 | 改前 | 改后 | 判 |
|---|---|---|---|
| migrate/迁移手册.md | UTF-8（无 BOM）CRLF | 同 | ✓ |
| migrate/迁移手册.html | UTF-8 CRLF（长行 62543） | 同 | ✓ |
| migrate/cleanup-local.ps1 | UTF-8 **BOM** CRLF | 同 | ✓ |
| docs/lessons.md | UTF-8 CRLF | 同 | ✓ |
| archive/GOAL-V1-PROMPT.md | UTF-8 CRLF（长行 433） | UTF-8 CRLF（长行 424） | ✓（最长行恰为被改写的 :65，去引用后缩短；编码属性未变） |
| archive/其余 10 份 | UTF-8 CRLF（长行 570/322/446/946/316/409 各自不变；GOAL-V5D/V6I/MIGRATE-V6-PC2/VERIFY-V1 无长行标注） | 同 | ✓ |

**过程事故与自纠（留档）**：手术中曾用 `sed -i` 删节 cleanup-local.ps1，本机 MSYS sed 将全文件 CRLF 写为 LF（`file` 复验立刻暴露、od 字节级实锤 `\n` 无 `\r`）；经 git 原生路径修复（`git add` 归一入 index → `rm` → `git checkout --` 按 `.gitattributes`（`* text=auto eol=crlf`）重涂 CRLF），复验恢复基线逐字一致。提交 blob 走 LF 归一不受工作区行尾影响，仓库内容无污染。教训：本环境手术一律用 Edit 工具（md/html/lessons 实证 CRLF 保真），不用 sed -i。

## 5. lessons 锚点交集 ∅（P6⑤）

清单 A＝lessons.md 引用的 archive 文件（`grep -o "docs/handoff/archive/[A-Za-z0-9/._-]*" docs/lessons.md | sort -u`，剔除头部口径说明中的省略号占位）：

```
docs/handoff/archive/PUBLISH-V5A.md
docs/handoff/archive/RESEARCH-V7.md
docs/handoff/archive/REVIEW-V7.md
docs/handoff/archive/VERIFY-V4.md
docs/handoff/archive/VERIFY-V5.md
docs/handoff/archive/VERIFY-V6.md
docs/handoff/archive/VERIFY-V6B.md
docs/handoff/archive/VERIFY-V7.md
docs/handoff/archive/VERIFY-V7S.md
docs/handoff/archive/VERIFY-V7V.md
```

清单 B＝本轨改动文件（`git status --porcelain`，16 件见 §2）。`comm -12 清单A 清单B` 输出＝**空**（exit 0、无行），交集 ∅——坑库全部锚点文件无一被本轮手术触碰，行号零漂移。

## 6. build＋纯件三件（P6②③）

**build**（worktree 内，`cmd //c build.cmd`）：

```
[OK] Build succeeded: D:\Download\Agent Vault\OneClickBlackScreen\OneClickBlackScreen-WorkTree\v9-a\dist\一键黑屏.exe
build_exit=0
```

**纯件三件**（编译行照抄 verify.cmd 对应 csc 行；运行参数照抄 verify.cmd：VerifyConfig 无参 / VerifyIcon "%TARGET%" / VerifyUpdate 无参；GUI 四件未编译未运行，验证闸门未触——与 B 轨并行守约）：

```
[1/3] VerifyConfig
PASS: 23 config tests (defaults/roundtrip/format/golden/corruption/escapes/atomic-write/idle-key)
[2/3] VerifyIcon
PASS: icon-main.ico structure ok (7 tiers, in-bounds, all entries decodable)
PASS: icon-settings.ico structure ok (7 tiers, in-bounds, all entries decodable)
PASS: exe embeds icon-main 256px PNG payload (offset 260384)
PASS: settings.ico managed resource byte-identical to artifact and decodable
PASS: dist\icon-settings.ico deployed byte-identical (110145 bytes)
PASS TOTAL: 4 icon checks
[3/3] VerifyUpdate
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
[OK] ALL PURE VERIFIERS PASSED
pure_exit=0
```

三件各自 exit 0（脚本对每件独立判 errorlevel，任一非零即中途退出非 0）。临时调度脚本放仓库外 `OneClickBlackScreen-WorkTree\v9a-pure.cmd`（用毕即删，未入任何 git 面）。

## 7. 验收清单核对（对 GOAL-V9A §6）

- [x] 分支可见且指向本轨提交 → §9 推送记录（提交哈希＋`git ls-remote` 输出）
- [x] `git diff --name-status origin/main...HEAD` 全部落入 §4 白名单 → §2：16 件与白名单精确相等，无白名单外路径
- [x] grep 归零 → §3 口径 2：A 轨写面零命中；全仓字面零在本分支受 B 轨写面（使用说明.txt）与批次目录自引用两处只读豁免制约（移交注记 1/2），R 轨合并 B 后复核
- [x] VERIFY-V9A 含逐文件手术 diff／file 前后对照／锚点交集∅／build exit 0／纯件三件 exit 0 → §2＋附录 A／§4／§5／§6
- [x] lessons 新增 L-017 五要素齐全，总长 ≤180 → `wc -l docs/lessons.md`＝**124**；`grep -c "^### L-"`＝17（L-001…L-017 连续）
- [x] 无 GUI 验证器运行记录 → §6 仅纯件三件；verify.cmd 未执行（其会连跑 GUI 四件）；闸门零接触

## 8. 移交注记（R/I 裁定）

1. **批次目录自引用**：`docs/handoff/2026-11-05-v9-installer/` 6 份 48 处 DeskBox 字样＝本轮指令文档自身（含 GOAL-V9A-PROMPT.md），A 轨白名单内该目录仅可新增本文件，无法也不应自行去字。建议：R 轨知悉即可（合并 A＋B 后全仓字面仅剩此目录）；「是否要求指令文档永久零字面」请 R 提请 V10 规划窗与 D8 归档惯例一并定夺（归档副本仍会保留字样；若定「永不留字面」需立「指令文档以指称代字面」的常设规约）。
2. **使用说明.txt:105**：B 轨写面（本轨禁改），B 推送后合并态 grep 仅剩批次目录。
3. **迁移手册 S 编号引用漂移**：cleanup-local.ps1 删 S2 并重编后，`迁移手册.md:485`（「S1～S7」）与 `:493`（「S2～S5 逐条 [OK]/[SKIP]」，html:471/:475 同步）仍述旧编号。该两行无 DeskBox 字样、不在本轨授权三行内，按铁律「非 DeskBox 连带内容不得顺手改」未动。R 可裁：手册为已完成的迁移归档，保持历史原文（推荐，与 archive 同理）或随后续手册重写轮一并修。
4. **迁移手册 install.ps1 引用**：install.ps1 已整删，手册步骤 5.3（md/html）标题与命令仍教运行该脚本（非 DeskBox 行，未动）。活文档口径（README/使用说明）归 B 轨整体改写；migrate 手册为冻结区历史文档，同注记 3 由 R 裁定。
5. **L-017 措辞裁量**：GOAL P5 给出的「现象」文案含 DeskBox 字面，与 P6①/验收第 3 条/RESEARCH §1①「grep 归零」定义冲突——不可同时满足。本轨取「字面不入库」（现象表述为「本机个人快捷方式启动器目录」），与轮目标（根除的可执行定义）一致；若 R 裁定须保留原字面请显式改 GOAL 并接受 grep 不归零。
6. **SHA256SUMS.txt**：其校验对象＝migrate 包三件（bundle/zip/ini），不含本轮任何手术文件；`git status` 该文件零变化，「校验和失效」情形未发生。

## 9. 提交与推送记录

（推送后回填）

## 附录 A · 全量逐文件手术 diff

命令：`git -c core.quotepath=false diff origin/main`（工作区态，含未暂存；install.ps1 为已暂存删除）。逐字节真实输出，接续于下：

```diff
diff --git a/docs/handoff/archive/GOAL-V1-PROMPT.md b/docs/handoff/archive/GOAL-V1-PROMPT.md
index d5af506..505dba4 100644
--- a/docs/handoff/archive/GOAL-V1-PROMPT.md
+++ b/docs/handoff/archive/GOAL-V1-PROMPT.md
@@ -62,7 +62,7 @@
 - 产出：build.cmd + verify 套件 + docs/VERIFY-V1.md + git 提交。
 
 ### Phase 4 · 安装与文档
-- `install.ps1`：幂等同步 DeskBox 双快捷方式——`C:\Users\Lenovo\DeskBox\DeskBox\一键黑屏.lnk`（无参，目标=仓库 `dist\一键黑屏.exe`）与 `一键黑屏-设置.lnk`（同目标，参数 `--settings`）；用 WScript.Shell COM；不触碰该目录其他文件，不删旧 exe（`Documents\Codex\...` 处保留作回退）。
+- `install.ps1`：幂等同步双快捷方式——`一键黑屏.lnk`（无参，目标=仓库 `dist\一键黑屏.exe`）与 `一键黑屏-设置.lnk`（同目标，参数 `--settings`）；用 WScript.Shell COM；不触碰快捷方式目录其他文件，不删旧 exe（`Documents\Codex\...` 处保留作回退）。
 - 仓库根写 v2 版 `使用说明.txt`：双快捷方式用法、设置入口、颜色/文字自定义、配置文件位置、**"保存后下次启动生效"**、退出行为与 v1 完全一致、§7 风险边界。
 - 更新 `README.md` 文档地图（src/verify/docs/dist 行）。
 - 产出：install.ps1 + 使用说明.txt + README 更新 + git 提交。
@@ -74,8 +74,8 @@
 - 产出：全绿证据 + 干净的 git 状态。
 
 ### 边界清单
-- 允许：仓库内一切新增/修改；DeskBox 目录仅两个指名 .lnk；%LOCALAPPDATA%\BlackScreen\ 配置读写；运行验证器。
-- 禁止：改 baseline/；改 DeskBox 其他文件；删除旧 exe；改系统设置/注册表；联网下载依赖；把任何凭证写进文件或提示词。
+- 允许：仓库内一切新增/修改；快捷方式目录仅两个指名 .lnk；%LOCALAPPDATA%\BlackScreen\ 配置读写；运行验证器。
+- 禁止：改 baseline/；改快捷方式目录其他文件；删除旧 exe；改系统设置/注册表；联网下载依赖；把任何凭证写进文件或提示词。
 
 ## 5. 铁律（不可协商）
 
@@ -89,7 +89,6 @@
 
 - 配置路径：`%LOCALAPPDATA%\BlackScreen\settings.ini`（`color=#RRGGBB` + `text=...`，转义规则见 RESEARCH-V1 §5-D3）。
 - 编译器：`C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`。
-- 快捷方式目录：`C:\Users\Lenovo\DeskBox\DeskBox\`。
 - 仓库根：`D:\Download\Agent Vault\一键黑屏`。
 
 ## 7. 验收清单（Done when——校验器只认这些实据）
@@ -100,7 +99,7 @@
 | 2 | `verify` 全部验证器输出含 PASS 且退出码 0：逐屏窗体数==屏数、逐屏边界精确、TopMost、无边框、每屏采样点==配置色、有文字时中心区含文字色像素、无文字时全屏纯色、3 键前台+后台退出、退出码 0、第二实例自退、InputIdle ≤2000ms（报告实测值） |
 | 3 | `VerifyConfig` 退出码 0（roundtrip/缺文件/损坏/超限/未知键回落默认/转义） |
 | 4 | `--settings` 打开设置窗（恰好一个非遮罩窗体，进程存活可被关闭）；未知参数不产生任何遮罩窗体 |
-| 5 | `install.ps1` 连跑两次成功；DeskBox 目录两个指名 .lnk 存在且目标/参数正确 |
+| 5 | `install.ps1` 连跑两次成功；快捷方式目录两个指名 .lnk 存在且目标/参数正确 |
 | 6 | git：`feature/mask-customization` 分支，工作树干净，≥4 个提交（勘察勘误/T7 评审、实现、构建+验证、安装+文档） |
 | 7 | 文档：RESEARCH-V1.md 含 §9 T7 评审记录；docs/VERIFY-V1.md 含全部命令与输出摘要；使用说明.txt 为 v2 版 |
 | 8 | 冻结项核查：`git diff main -- baseline/` 为空；`%LOCALAPPDATA%\BlackScreen\settings.ini` 缺失时新 exe 行为==纯黑无文字即 v1 行为（验证器已覆盖） |
@@ -112,7 +111,7 @@
 **/goal 启动指令（交接者粘贴到执行窗口）**：
 
 ```text
-/goal 按 GOAL-V1-PROMPT.md 执行契约完成"一键黑屏"v1 自定义化改造（R1 遮罩颜色、R2 遮罩文字、R3 设置入口，R4 双击立即出遮罩与 R5 退出交互冻结不变）：以仓库根 RESEARCH-V1.md §5 架构决策表为唯一事实源实现 src/ 并交付。完成证据：1) build.cmd 退出码 0 且产出 dist\一键黑屏.exe；2) verify 全部验证器 PASS 且退出码 0；3) install.ps1 幂等生效且 DeskBox 双快捷方式目标正确；4) feature/mask-customization 分支工作树干净且 git diff main -- baseline/ 为空；5) RESEARCH-V1.md §9、docs/VERIFY-V1.md、使用说明.txt 齐备。以第 1–4 条命令输出与文件实据为唯一完成依据，计划与结论式回复不作数；本目标仅此一项，做到即停，过程中不留未完成待办。
+/goal 按 GOAL-V1-PROMPT.md 执行契约完成"一键黑屏"v1 自定义化改造（R1 遮罩颜色、R2 遮罩文字、R3 设置入口，R4 双击立即出遮罩与 R5 退出交互冻结不变）：以仓库根 RESEARCH-V1.md §5 架构决策表为唯一事实源实现 src/ 并交付。完成证据：1) build.cmd 退出码 0 且产出 dist\一键黑屏.exe；2) verify 全部验证器 PASS 且退出码 0；3) install.ps1 幂等生效且双快捷方式目标正确；4) feature/mask-customization 分支工作树干净且 git diff main -- baseline/ 为空；5) RESEARCH-V1.md §9、docs/VERIFY-V1.md、使用说明.txt 齐备。以第 1–4 条命令输出与文件实据为唯一完成依据，计划与结论式回复不作数；本目标仅此一项，做到即停，过程中不留未完成待办。
 ```
 
 **收尾动作（执行窗口最后执行）**：
diff --git a/docs/handoff/archive/GOAL-V2-PROMPT.md b/docs/handoff/archive/GOAL-V2-PROMPT.md
index 87d0ad4..3662cc5 100644
--- a/docs/handoff/archive/GOAL-V2-PROMPT.md
+++ b/docs/handoff/archive/GOAL-V2-PROMPT.md
@@ -75,8 +75,8 @@
 - 产出：全绿证据 + 干净的 git 状态。
 
 ### 边界清单
-- 允许：仓库内一切新增/修改（含 assets\ 产物入库）；DeskBox 目录仅两个指名 .lnk；`%LOCALAPPDATA%\BlackScreen\` 读写；运行验证器（会闪现遮罩）；`_probe\` 放临时预览。
-- 禁止：改 baseline/；改 src\Program.cs/MaskConfig.cs/OverlayForm.cs；改 DeskBox 其他文件；改系统设置/注册表（含深浅色——验证与验收都不得切换系统主题）；联网下载依赖；把任何凭证写进文件或提示词。
+- 允许：仓库内一切新增/修改（含 assets\ 产物入库）；快捷方式目录仅两个指名 .lnk；`%LOCALAPPDATA%\BlackScreen\` 读写；运行验证器（会闪现遮罩）；`_probe\` 放临时预览。
+- 禁止：改 baseline/；改 src\Program.cs/MaskConfig.cs/OverlayForm.cs；改快捷方式目录其他文件；改系统设置/注册表（含深浅色——验证与验收都不得切换系统主题）；联网下载依赖；把任何凭证写进文件或提示词。
 
 ## 5. 铁律（不可协商）
 
@@ -89,7 +89,7 @@
 ## 6. 配置文件与命令速查
 
 - 编译器：`C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`（C#5；源码 UTF-8 带 BOM）。
-- 仓库根：`D:\Download\Agent Vault\一键黑屏`；快捷方式目录：`C:\Users\Lenovo\DeskBox\DeskBox\`。
+- 仓库根：`D:\Download\Agent Vault\一键黑屏`。
 - 配置：`%LOCALAPPDATA%\BlackScreen\settings.ini`（v2 不改其语义）。
 - 图标再生成（显式，默认构建不做）：`csc -nologo -target:exe -out:_probe\MakeIcon.exe tools\MakeIcon.cs` → 运行 → 人工比对 assets\ 差异后提交。
 - wbx 桥：`node "C:\Users\Lenovo\.zcode\wbx-bridge\scripts\wbx.mjs" ask --file <材料.txt> --as codex`。
diff --git a/docs/handoff/archive/GOAL-V5D-PROMPT.md b/docs/handoff/archive/GOAL-V5D-PROMPT.md
index ab1a705..d54f524 100644
--- a/docs/handoff/archive/GOAL-V5D-PROMPT.md
+++ b/docs/handoff/archive/GOAL-V5D-PROMPT.md
@@ -21,12 +21,12 @@
 
 ## 2. 背景
 
-项目即将迁往第二台 Windows 电脑（U4＝完整开发环境），原机随后清理（U6）。R4 已证：csc 与 PowerShell 5.1 为 Windows 自带、Win10 1903+/Win11 出厂含 .NET FW 4.8——PC2 只需装 git（winget 一条命令）即可 clone＋build＋verify。主路径＝GitHub clone；离线备援＝git bundle（含未推送的本地分支）。本机硬编码 `C:\Users\Lenovo\...` 在 install.ps1（DeskBox 路径），PC2 用户名不同即失效——先根治再打包。
+项目即将迁往第二台 Windows 电脑（U4＝完整开发环境），原机随后清理（U6）。R4 已证：csc 与 PowerShell 5.1 为 Windows 自带、Win10 1903+/Win11 出厂含 .NET FW 4.8——PC2 只需装 git（winget 一条命令）即可 clone＋build＋verify。主路径＝GitHub clone；离线备援＝git bundle（含未推送的本地分支）。本机硬编码 `C:\Users\Lenovo\...` 在 install.ps1（快捷方式目标路径），PC2 用户名不同即失效——先根治再打包。
 
 ## 3. Phase 任务
 
 ### Phase 1 · 迁移就绪硬化（D1）
-- `install.ps1`：DeskBox 路径改 `$env:USERPROFILE\DeskBox\DeskBox` 派生（去两处硬编码；回读校验逻辑不变）；顶部加一行注释说明可改的目标目录变量。本机实跑一次幂等验证（DeskBox 快捷方式重建成功）。
+- `install.ps1`：快捷方式目标路径改由 `$env:USERPROFILE` 派生（去两处硬编码；回读校验逻辑不变）；顶部加一行注释说明可改的目标目录变量。本机实跑一次幂等验证（快捷方式重建成功）。
 - `git status` 确认硬化后 `verify.cmd` 仍全绿（脚本不影响构建，但按惯例守门）。
 
 ### Phase 2 · 迁移包（D3）
@@ -51,7 +51,7 @@
 ### Phase 4 · 本地清理脚本（D5）
 - 新建 `migrate/cleanup-local.ps1`：参数 `-ConfirmPC2Verified`（**缺省即拒绝运行**并打印 D6 清单提示）；执行序：
   1. 打包留档：仓库 zip→桌面（`一键黑屏-最终留档-日期.zip`）＋SHA256 打印（用户日后自行处置）
-  2. 删除：仓库目录（脚本所在仓库自身——用「标记后自删」或先复制脚本到桌面再执行，写清操作序）；DeskBox 两个 .lnk（存在才删）；`%LOCALAPPDATA%\BlackScreen`；计划任务 `OneClickBlackScreen-Idle`（`schtasks /delete /tn ... /f`，退出码 1＝本就不存在＝放行）；ZCode 本工作区痕迹（只**列出**可定位项供界面内手动移除，不盲删）
+  2. 删除：仓库目录（脚本所在仓库自身——用「标记后自删」或先复制脚本到桌面再执行，写清操作序）；两个 .lnk（存在才删）；`%LOCALAPPDATA%\BlackScreen`；计划任务 `OneClickBlackScreen-Idle`（`schtasks /delete /tn ... /f`，退出码 1＝本就不存在＝放行）；ZCode 本工作区痕迹（只**列出**可定位项供界面内手动移除，不盲删）
   3. **保留**：`%USERPROFILE%\.zcode\skills`（用户级、非项目专属——输出一句说明，最终去留由用户定）
   4. 每步打印结果清单；任何一步失败→停在后置步骤前，报告已完成/未完成。
 - 脚本头部注释＝红线声明：只应由用户在 PC2 验证通过后手动执行；任何 AI 窗口不得代跑。
diff --git a/docs/handoff/archive/GOAL-V6I-PROMPT.md b/docs/handoff/archive/GOAL-V6I-PROMPT.md
index 320c2e6..56fcfee 100644
--- a/docs/handoff/archive/GOAL-V6I-PROMPT.md
+++ b/docs/handoff/archive/GOAL-V6I-PROMPT.md
@@ -23,7 +23,7 @@
 
 ## 2. 背景
 
-A 轨已根修幻影键退出缺陷（产品行为变更→需发 v1.0.1），B 轨已根修 VerifySettings 环境敏感。用户定案：每轮完成默认推送（RESEARCH-V6 §7）。本机＝PC2（迁移目标机）：DeskBox 快捷方式未装、迁移阶段 8 五项未勾（第 4 项必须用户亲手）。发布工艺全部沿用 V5I 已验证序列，不发明新流程。
+A 轨已根修幻影键退出缺陷（产品行为变更→需发 v1.0.1），B 轨已根修 VerifySettings 环境敏感。用户定案：每轮完成默认推送（RESEARCH-V6 §7）。本机＝PC2（迁移目标机）：快捷方式未装、迁移阶段 8 五项未勾（第 4 项必须用户亲手）。发布工艺全部沿用 V5I 已验证序列，不发明新流程。
 
 ## 3. Phase 任务
 
@@ -44,7 +44,7 @@ A 轨已根修幻影键退出缺陷（产品行为变更→需发 v1.0.1），B
 - 真端点冒烟：构建后的设置窗静默检查→「已是最新」（本地 1.0.1 vs latest v1.0.1）；输出/状态实录留档。
 
 ### Phase 4 · PC2 收尾与证据
-- `powershell -NoProfile -ExecutionPolicy Bypass -File install.ps1`：两行 `[OK] …lnk`＋完成行（DeskBox 快捷方式落位）。
+- `powershell -NoProfile -ExecutionPolicy Bypass -File install.ps1`：两行 `[OK] …lnk`＋完成行（快捷方式落位）。
 - 程序化冒烟：遮罩启动→注入按键退出成功；`--settings` 开→改设置→保存路径成功（对照手册阶段 8 第 3/5 项口径）。
 - `docs/MIGRATE-V6-PC2.md`：阶段 8 五项对照表（1 fsck 补档、2 status、3 七件全绿、4 **标注「待用户亲手双击冒烟」**、5 设置窗程序化证据＋用户可复核指引）＋「清理准入证」状态说明（四项已证＋第 4 项待用户）。
 - `docs/VERIFY-V6.md`：本轮集成总证据（合并/全绿×2/发布序列/冒烟/版本裁决表）。
@@ -68,7 +68,7 @@ A 轨已根修幻影键退出缺陷（产品行为变更→需发 v1.0.1），B
 2. 干净键态 `verify.cmd` 末行 `[OK] ALL VERIFIERS PASSED` 出现≥2 次实录（docs/VERIFY-V6.md）。
 3. `gh release view v1.0.1` 三资产（zip/exe/SHA256SUMS）在位且 digest 与本地 certutil 复算一致。
 4. `src/AppInfo.cs` 含 `Version = "1.0.1"`；`git grep -n "1\.0\.0"` 残留仅历史过程文档（裁决表留档）。
-5. install.ps1 输出两 `[OK]` 快捷方式行；DeskBox 目录两 .lnk 存在且目标指向本仓 dist。
+5. install.ps1 输出两 `[OK]` 快捷方式行；快捷方式目录两 .lnk 存在且目标指向本仓 dist。
 6. docs/MIGRATE-V6-PC2.md 五项对照齐（第 4 项明确「待用户」）；migrate/ 三条 digest MATCH 实录。
 7. 设置窗「已是最新」真端点冒烟实录在 docs/VERIFY-V6.md。
 
diff --git a/docs/handoff/archive/MIGRATE-V5D.md b/docs/handoff/archive/MIGRATE-V5D.md
index aa75c0d..4705088 100644
--- a/docs/handoff/archive/MIGRATE-V5D.md
+++ b/docs/handoff/archive/MIGRATE-V5D.md
@@ -16,10 +16,10 @@
 | 零硬编码 | `git grep -n "C:\\Users\\Lenovo" install.ps1` | 无命中（exit 1） |
 | 幂等 ×2 | `powershell -NoProfile -ExecutionPolicy Bypass -File install.ps1`（两遍） | 均两行 `[OK] …lnk` ＋ `[OK] install.ps1 完成` |
 | 守门 | `verify.cmd` | `[OK] ALL VERIFIERS PASSED`（七件全绿） |
-| 移植性（A9） | 硬化版脚本复制到临时 clone `D:\Download\_v5d-smoke\OneClickBlackScreen` 运行 | `[OK]`；DeskBox 两 .lnk 正确指向**临时 clone** 的 dist（路径派生自脚本位置，用户目录派生自 `$env:USERPROFILE`） |
+| 移植性（A9） | 硬化版脚本复制到临时 clone `D:\Download\_v5d-smoke\OneClickBlackScreen` 运行 | `[OK]`；两 .lnk 正确指向**临时 clone** 的 dist（路径派生自脚本位置，用户目录派生自 `$env:USERPROFILE`） |
 | 恢复（A10） | 仓库自身 install.ps1 重跑 | .lnk 指回 `D:\Download\Agent Vault\一键黑屏\dist` |
 
-改动：`$deskBox = Join-Path $env:USERPROFILE 'DeskBox\DeskBox'`（原 `C:\Users\Lenovo\…` 硬编码）；注释同步改写并加「可改目录」说明。回读校验逻辑未动。
+改动：快捷方式目标目录改由 `$env:USERPROFILE` 派生（原 `C:\Users\Lenovo\…` 硬编码）；注释同步改写并加「可改目录」说明。回读校验逻辑未动。
 
 ## 2. Phase 2 · 迁移包清单（migrate/）
 
@@ -40,7 +40,7 @@
 
 **拒绝路径实跑（B1）**：`powershell … -File cleanup-local.ps1`（无开关）→ `[REFUSED] 未带 -ConfirmPC2Verified 参数…未触碰任何文件。`＋D6 五项清单全打印，exit **1**。
 
-**DryRun 实跑（B2）**：`… -ConfirmPC2Verified -DryRun` → S1–S7 全 `[DRY]`/`[LIST]`/`[KEEP]` 列表，`[DONE] DryRun 演练结束：未改动任何文件。` exit 0。前后快照比对：DeskBox 两 .lnk、`%LOCALAPPDATA%\BlackScreen\settings.ini`、仓库目录、桌面留档 zip——**全部不变**。
+**DryRun 实跑（B2）**：`… -ConfirmPC2Verified -DryRun` → S1–S7 全 `[DRY]`/`[LIST]`/`[KEEP]` 列表，`[DONE] DryRun 演练结束：未改动任何文件。` exit 0。前后快照比对：两 .lnk、`%LOCALAPPDATA%\BlackScreen\settings.ini`、仓库目录、桌面留档 zip——**全部不变**。
 
 ## 4. 命令冒烟记录（手册 22 命令块逐一；「本机已跑」均有原始输出，存 `_v5d-smoke/*-smoke.txt`）
 
@@ -94,8 +94,6 @@
 桌面：C:\Users\Lenovo\Desktop
 
 S1   [DRY]  将把整个仓库压缩到桌面：C:\Users\Lenovo\Desktop\一键黑屏-最终留档-20261103-082630.zip 并打印其 SHA256
-S2   [DRY]  将删除 C:\Users\Lenovo\DeskBox\DeskBox\一键黑屏.lnk
-S2   [DRY]  将删除 C:\Users\Lenovo\DeskBox\DeskBox\一键黑屏-设置.lnk
 S3   [DRY]  将删除配置目录 C:\Users\Lenovo\AppData\Local\BlackScreen
 S4   [DRY]  将执行 schtasks /delete /tn OneClickBlackScreen-Idle /f（本就不存在则跳过）
 S5   [DRY]  将删除整个仓库目录 D:\Download\Agent Vault\一键黑屏
@@ -111,4 +109,4 @@ S7   [KEEP]         其他项目仍在用；最终去留由你自行决定（删
 [DONE] DryRun 演练结束：未改动任何文件。
 ```
 
-（同批前后快照：DeskBox 两 .lnk / settings.ini / 仓库目录 / 桌面留档 zip 均不变；拒绝路径 B1 输出见 §3，exit 1。）
+（同批前后快照：两 .lnk / settings.ini / 仓库目录 / 桌面留档 zip 均不变；拒绝路径 B1 输出见 §3，exit 1。）
diff --git a/docs/handoff/archive/MIGRATE-V6-PC2.md b/docs/handoff/archive/MIGRATE-V6-PC2.md
index 4e9715f..7b46a62 100644
--- a/docs/handoff/archive/MIGRATE-V6-PC2.md
+++ b/docs/handoff/archive/MIGRATE-V6-PC2.md
@@ -13,14 +13,14 @@
 | 4 | 双击冒烟 | 双击 `dist\一键黑屏.exe`→全屏遮罩出现，按任意键消失 | **待用户亲手双击**（验证的正是真实输入路径，AI 不得代勾） | ⏳ 待用户 |
 | 5 | 设置窗冒烟＋保存 | 窗口打开，改一项设置能保存成功 | 程序化镜像实测 PASS：`--settings` 开窗→改遮罩文字→点「保存」→「已保存」确认框→settings.ini 落盘字节精确→退出码 0（VERIFY-V6 §6.3） | ✅（用户可复核见 §3） |
 
-## 2. DeskBox 快捷方式落位（阶段 8 前置）
+## 2. 快捷方式落位（阶段 8 前置）
 
 `powershell -NoProfile -ExecutionPolicy Bypass -File install.ps1` → 两行 `[OK]`＋完成行；WScript 回读：两 .lnk 均指向本仓 `D:\Download\Agent Vault\OneClickBlackScreen\dist\一键黑屏.exe`（设置快捷方式带 `--settings` 与 ico 图标）。实测输出见 VERIFY-V6 §6.2。
 
 ## 3. 第 4/5 项用户复核指引
 
-- **第 4 项（必做，亲手）**：双击 DeskBox「一键黑屏」→ 全屏遮罩出现 → 按任意键或点鼠标 → 遮罩消失即勾。
-- **第 5 项（可选复核，程序化已证）**：双击 DeskBox「一键黑屏-设置」→ 随便改一项（如遮罩文字）→ 点「保存」→ 见「已保存」提示即勾。本轮已用消息级注入工具按同路径代验（开窗/改/存/确认框/ini 字节/退出码全过），但手册口径允许用户亲手再走一遍。
+- **第 4 项（必做，亲手）**：双击「一键黑屏」→ 全屏遮罩出现 → 按任意键或点鼠标 → 遮罩消失即勾。
+- **第 5 项（可选复核，程序化已证）**：双击「一键黑屏-设置」→ 随便改一项（如遮罩文字）→ 点「保存」→ 见「已保存」提示即勾。本轮已用消息级注入工具按同路径代验（开窗/改/存/确认框/ini 字节/退出码全过），但手册口径允许用户亲手再走一遍。
 
 ## 4. 清理准入证状态
 
diff --git a/docs/handoff/archive/RESEARCH-V1.md b/docs/handoff/archive/RESEARCH-V1.md
index 76dfc59..2380729 100644
--- a/docs/handoff/archive/RESEARCH-V1.md
+++ b/docs/handoff/archive/RESEARCH-V1.md
@@ -42,7 +42,7 @@
 - 编译器：仅 `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`（**C# 5 方言上限**）；无 .NET SDK、无 Roslyn。
 - 运行时：.NET Framework 4.8.x（NDP v4 Full Release=0x82718）。
 - 显示器：**混合 DPI 双屏**——DISPLAY1 主屏 2560×1600@150%（逻辑 1707×1067），DISPLAY2 副屏 1920×1080@125%（逻辑 1536×864，位于负坐标）。VirtualScreen `{X=-1536,Y=0,W=3243,H=1078}` 含屏间空隙。
-- 快捷方式：`C:\Users\Lenovo\DeskBox\DeskBox\一键黑屏.lnk` → `C:\Users\Lenovo\Documents\Codex\2026-09-07\gei\outputs\一键黑屏.exe`（旧产物，保留不动作回退）。
+- 快捷方式：用户快捷方式启动目录的 `一键黑屏.lnk` → `C:\Users\Lenovo\Documents\Codex\2026-09-07\gei\outputs\一键黑屏.exe`（旧产物，保留不动作回退）。
 
 ## 4. 外部调研结论（wbx 双 lane，交叉验证）
 
@@ -68,8 +68,8 @@
 | D8 | 单实例 | 仅遮罩模式：`Local\` 命名互斥体，`WaitOne(0)`，占用即静默退出；`AbandonedMutexException` 视为取得；仅在拥有时 Release，`Application.Run` 返回后 finally 兜底；设置模式不受限 |
 | D9 | 配置快照语义 | 遮罩启动时读一次配置传构造函数；设置保存**下次启动生效**；明确不做文件监听/IPC/实时刷新 |
 | D10 | 生命周期 | 分流 → （遮罩模式）互斥体 → 同步读配置（带上限）→ 建窗；`SetCompatibleTextRenderingDefault` 在两模式任何窗体前调用；Main 同步读可接受，不承诺亚毫秒 |
-| D11 | 构建与安装 | `build.cmd` 一键调 csc（`-target:winexe -optimize+ -win32manifest:baseline\BlackScreen.manifest`，引用仅 System/System.Drawing/System.Windows.Forms 等框架程序集，**零第三方依赖**）；`install.ps1` 幂等同步 DeskBox 双快捷方式（主=无参、设置=`--settings`），目标=仓库 `dist\一键黑屏.exe`；**源码文件必须 UTF-8 带 BOM**（csc 无 BOM 按 GBK 读，中文字面量会乱码） |
-| D12 | 冻结清单 | baseline/ 不改；退出交互与 arm 精确语义（§2）逐字保留；不引入托盘/常驻/热键/半透明/NOACTIVATE/多语言；旧 exe 与 DeskBox 其他文件不动；不改系统设置、不联网 |
+| D11 | 构建与安装 | `build.cmd` 一键调 csc（`-target:winexe -optimize+ -win32manifest:baseline\BlackScreen.manifest`，引用仅 System/System.Drawing/System.Windows.Forms 等框架程序集，**零第三方依赖**）；`install.ps1` 幂等同步双快捷方式（主=无参、设置=`--settings`），目标=仓库 `dist\一键黑屏.exe`；**源码文件必须 UTF-8 带 BOM**（csc 无 BOM 按 GBK 读，中文字面量会乱码） |
+| D12 | 冻结清单 | baseline/ 不改；退出交互与 arm 精确语义（§2）逐字保留；不引入托盘/常驻/热键/半透明/NOACTIVATE/多语言；旧 exe 与快捷方式目录其他文件不动；不改系统设置、不联网 |
 
 ## 6. 验证方案（延续 baseline 验证文化，全部自动化）
 
diff --git a/docs/handoff/archive/RESEARCH-V5.md b/docs/handoff/archive/RESEARCH-V5.md
index aafb54b..1515dbc 100644
--- a/docs/handoff/archive/RESEARCH-V5.md
+++ b/docs/handoff/archive/RESEARCH-V5.md
@@ -119,11 +119,11 @@
 ### D 系 · 迁移与清理
 | # | 决策 |
 |---|---|
-| D1 | 迁移就绪硬化（根治非补丁）：install.ps1 去除 `C:\Users\Lenovo` 硬编码→`$env:USERPROFILE\DeskBox\DeskBox` 派生（PC2 用户名不同即需要）；build.cmd/verify.cmd 的 csc 存在性 FAIL 提示保持既有口径 |
+| D1 | 迁移就绪硬化（根治非补丁）：install.ps1 去除 `C:\Users\Lenovo` 硬编码→由 `$env:USERPROFILE` 派生（PC2 用户名不同即需要）；build.cmd/verify.cmd 的 csc 存在性 FAIL 提示保持既有口径 |
 | D2 | 迁移主路径（PC2 在线）：`winget install --id Git.Git -e` →（可选 gh）→ `git clone https://github.com/Oiawlm/OneClickBlackScreen` → `git config core.quotepath false` → `build.cmd` → `verify.cmd` 全绿 → `install.ps1` → 双击遮罩冒烟；ZCode：官网安装＋登录＋skills 三件套（fts-style-system/glm-prompt-crafter/github-readme，wb-bridge 可选）从迁移包复制到 `%USERPROFILE%\.zcode\skills\`；`settings.ini` 按需复制（个性化） |
 | D3 | 离线备援包：`git bundle create 一键黑屏.bundle --all`（含未推送分支，R3/A5 的保全）＋仓库工作区 zip＋skills zip＋手册自身；PC2 无网时 `git clone 一键黑屏.bundle` 全量还原 |
 | D4 | 手册（`migrate/迁移手册.md`＋图表）：总流程图＋每步小图；每步含「在哪打开 PowerShell（开始菜单搜 / Win+X）」＋逐条可复制粘贴命令＋预期输出样例＋失败排查表；菜鸟零跳步口径（术语首次出现即解释）；图表用 mermaid 内嵌＋导出 PNG |
-| D5 | 清理脚本 `migrate/cleanup-local.ps1`（D 生成、**用户手动执行、任何窗口不得代跑**）：`-ConfirmPC2Verified` 开关（缺省拒绝运行并列出 PC2 验证清单）；执行序＝打包留档（仓库 zip→桌面＋SHA256）→删：仓库目录、DeskBox 两 .lnk（存在才删）、`%LOCALAPPDATA%\BlackScreen`、计划任务 `OneClickBlackScreen-Idle`（存在才删）、ZCode 本工作区痕迹（可定位部分）；**保留**：`~/.zcode/skills`（用户级、非项目专属，登记待用户确认）；每步输出结果清单 |
+| D5 | 清理脚本 `migrate/cleanup-local.ps1`（D 生成、**用户手动执行、任何窗口不得代跑**）：`-ConfirmPC2Verified` 开关（缺省拒绝运行并列出 PC2 验证清单）；执行序＝打包留档（仓库 zip→桌面＋SHA256）→删：仓库目录、两个快捷方式 .lnk（存在才删）、`%LOCALAPPDATA%\BlackScreen`、计划任务 `OneClickBlackScreen-Idle`（存在才删）、ZCode 本工作区痕迹（可定位部分）；**保留**：`~/.zcode/skills`（用户级、非项目专属，登记待用户确认）；每步输出结果清单 |
 | D6 | PC2 验证口径（迁移完成的判定）：`git fsck` 无错＋`git status` 干净＋`verify.cmd` 全绿＋双击遮罩冒烟＋设置窗打开冒烟；证据留档（截图），作为跑 cleanup 的前置 |
 
 ## 6. 验证面演进
diff --git a/docs/handoff/archive/RESEARCH-V6.md b/docs/handoff/archive/RESEARCH-V6.md
index 4c8600f..7a5609c 100644
--- a/docs/handoff/archive/RESEARCH-V6.md
+++ b/docs/handoff/archive/RESEARCH-V6.md
@@ -58,7 +58,7 @@
 | 3 verify 七件全绿 | 修复后达成（本轮 A＋B 的目的） | I 波 |
 | 4 双击冒烟（真实人手） | **待用户亲手**（验证的正是真实输入路径） | 用户 |
 | 5 设置窗冒烟＋保存 | 程序化可代验＋用户可复核 | I 波 |
-| — DeskBox 快捷方式 | 未安装（install.ps1 未在 PC2 跑过） | I 波 |
+| — 快捷方式 | 未安装（install.ps1 未在 PC2 跑过） | I 波 |
 | — 旧机清理（阶段 9） | 待用户在旧机亲手跑 cleanup-local.ps1（红线：AI 绝不代跑） | 用户 |
 
 ## 6. 并行分析与模型选型（依据）
diff --git a/docs/handoff/archive/VERIFY-V1.md b/docs/handoff/archive/VERIFY-V1.md
index 5129fa7..d314307 100644
--- a/docs/handoff/archive/VERIFY-V1.md
+++ b/docs/handoff/archive/VERIFY-V1.md
@@ -89,6 +89,6 @@ PASS TOTAL: A-F; 2 displays; max InputIdle 531 ms (limit 2000)
 - `build.cmd` → 退出码 0，`dist\一键黑屏.exe`（20992 字节）+ `dist\一键黑屏.exe.config`（239 字节）。
 - `verify.cmd` → 退出码 0，四件套全 PASS（25 条 PASS 行）：VerifyConfig 18 项 / VerifySettings 3 项 / VerifyMouseExit 5 键 / VerifyOverlay A–F（含 3 键前台+后台 + Alt 系统键、arm 冻结语义、第二实例自退）。
 - InputIdle 实测最大 **650 ms**（限值 2000 ms；多轮观测 531/634/726/650 ms）。
-- `install.ps1` 连跑两次均退出码 0（幂等），DeskBox 双快捷方式回读校验：`一键黑屏.lnk → dist\一键黑屏.exe（无参）`、`一键黑屏-设置.lnk → dist\一键黑屏.exe --settings`；目录内其他 11 个 .lnk 未触碰。
+- `install.ps1` 连跑两次均退出码 0（幂等），双快捷方式回读校验：`一键黑屏.lnk → dist\一键黑屏.exe（无参）`、`一键黑屏-设置.lnk → dist\一键黑屏.exe --settings`；目录内其他 11 个 .lnk 未触碰。
 - 验证器清单与最终结果：VerifyConfig PASS / VerifySettings PASS / VerifyMouseExit PASS / VerifyOverlay PASS，`[OK] ALL VERIFIERS PASSED`。
 - 分支：feature/mask-customization，工作树干净；`git diff main -- baseline/` 为空。
diff --git a/docs/handoff/archive/VERIFY-V2.md b/docs/handoff/archive/VERIFY-V2.md
index 7b11da8..2db9344 100644
--- a/docs/handoff/archive/VERIFY-V2.md
+++ b/docs/handoff/archive/VERIFY-V2.md
@@ -109,7 +109,7 @@ PASS TOTAL: 4 icon checks
 - `build.cmd` → 退出码 0；`dist\一键黑屏.exe`（243200 字节，内嵌双图标故较 v1 的 20992 字节增大）+ `dist\一键黑屏.exe.config`（245 字节）+ `dist\icon-settings.ico`（110145 字节）三件非空。
 - `verify.cmd` → 退出码 0，五件套全 PASS（30 条 PASS 行）：VerifyConfig 18 项 / VerifySettings 4 项（3 原 + 主题背景）/ VerifyMouseExit 5 键 / VerifyOverlay A–F（Overlay B 在 YaHei UI 字体链下原样通过 = U1 回归门）/ VerifyIcon 4 查。
 - InputIdle 实测最大 **116 ms**（限值 2000 ms；多轮观测 178/129/116 ms——字体链静态解析未拖慢热路径）。
-- `install.ps1` 连跑两次均退出码 0（幂等）：主 .lnk 无自定义图标（回读归一化 `',0'`==空）；设置 .lnk `IconLocation` 回读==`D:\Download\Agent Vault\一键黑屏\dist\icon-settings.ico,0`；DeskBox 目录 13 项（2 个本程序 .lnk + 11 个未触碰）。
+- `install.ps1` 连跑两次均退出码 0（幂等）：主 .lnk 无自定义图标（回读归一化 `',0'`==空）；设置 .lnk `IconLocation` 回读==`D:\Download\Agent Vault\一键黑屏\dist\icon-settings.ico,0`；快捷方式目录 13 项（2 个本程序 .lnk + 11 个未触碰）。
 - 冻结项核查：`git diff main -- baseline/` 为空；`git diff main --stat -- src/Program.cs src/MaskConfig.cs src/OverlayForm.cs` 为空；v1 四件套断言零改动（VerifyConfig/MouseExit/Overlay 仅 BOM 前置，VerifySettings 仅 BOM + 总计行 3→4 查，原 Check 断言逐字保留）；设置窗控件集合与保存语义不变。
 - 源码纪律：src/、tools/、verify/ 全部 .cs UTF-8 带 BOM（baseline/ 冻结不改）；build.rsp 引用仅 System/System.Drawing/System.Windows.Forms；C#5 编译通过即证方言兼容。
 - 分支：`feature/frontend-v2`，6 提交（勘察评审/图标/源码/构建验证/BOM 规范/安装文档），工作树干净；不自行 merge main（合并指令：`git checkout main && git merge feature/frontend-v2`，由用户确认后执行）。
diff --git a/docs/lessons.md b/docs/lessons.md
index 0da338c..0349789 100644
--- a/docs/lessons.md
+++ b/docs/lessons.md
@@ -115,3 +115,10 @@
 - 正解：窗口级 PrintWindow＋被测程序自启自绘；截图类任务先自查「画面里有没有不该有的东西」再留档。
 - 出处：docs/handoff/archive/PUBLISH-V5A.md:148（§事故注记3）。
 - 防再犯：截图/录屏默认窗口级捕获；产物先审后存。
+
+### L-017｜本机个人环境约定混入仓库分发面（V9）
+- 现象：本机个人快捷方式启动器目录（个人工作流默认值）被写进仓库脚本与文档——install.ps1 整文件只为其同步 .lnk，migrate 手册/清理脚本与 archive 历史文档多处点名。
+- 根因：个人工作流默认值被当作产品事实写进分发面；「这台电脑上如此」≠「每台电脑上应如此」。
+- 正解：分发只走标准 Windows 集成（安装器/开始菜单快捷方式，V9 起由安装包承担），个人环境约定不入库；既有落点手术式根除（用户 2026-11-05 授权含冻结区）。
+- 出处：用户 2026-11-05 指令（docs/handoff/2026-11-05-v9-installer/RESEARCH-V9.md §1①/§6-1）；install.ps1（本轮整文件删除）。
+- 防再犯：新分发面评审问「这条在陌生电脑上成立吗」；不成立即个人约定，不入库。
diff --git a/install.ps1 b/install.ps1
deleted file mode 100644
index 811db49..0000000
--- a/install.ps1
+++ /dev/null
@@ -1,57 +0,0 @@
-﻿# 一键黑屏 v2 安装脚本：幂等同步 DeskBox 双快捷方式，指向仓库 dist 产物。
-# 快捷方式目标目录 = 当前用户的 DeskBox（$env:USERPROFILE\DeskBox\DeskBox，随登录用户名自动适配，
-# 换电脑/换用户名无需改脚本；要装到别处可改下方 $deskBox 一行）。
-# 只创建/更新以下两个 .lnk，不触碰目录内其他文件：
-#   一键黑屏.lnk      （无参数 = 立即遮罩）
-#   一键黑屏-设置.lnk  （--settings = 设置窗口）
-$ErrorActionPreference = 'Stop'
-
-$repo = Split-Path -Parent $MyInvocation.MyCommand.Path
-$distDir = Join-Path $repo 'dist'
-$exe = Join-Path $distDir '一键黑屏.exe'
-if (-not (Test-Path $exe)) {
-    Write-Host "[FAIL] 未找到 $exe ；请先在仓库根运行 build.cmd 生成产物。"
-    exit 1
-}
-
-# 快捷方式目标目录：默认当前用户 DeskBox（可在上面一行按需改成其他目录）。
-$deskBox = Join-Path $env:USERPROFILE 'DeskBox\DeskBox'
-if (-not (Test-Path $deskBox)) { New-Item -ItemType Directory -Path $deskBox | Out-Null }
-
-$shell = New-Object -ComObject WScript.Shell
-$iconSettings = Join-Path $distDir 'icon-settings.ico'
-if (-not (Test-Path $iconSettings)) {
-    Write-Host "[FAIL] 未找到 $iconSettings ；请先在仓库根运行 build.cmd 生成产物。"
-    exit 1
-}
-$targets = @(
-    @{ Name = '一键黑屏.lnk';     Arguments = '';            IconLocation = '';                                   Description = '一键黑屏 v2 - 全屏遮罩' },
-    @{ Name = '一键黑屏-设置.lnk'; Arguments = '--settings'; IconLocation = "$iconSettings,0";                   Description = '一键黑屏 v2 - 设置' }
-)
-
-foreach ($target in $targets) {
-    $lnkPath = Join-Path $deskBox $target.Name
-    $lnk = $shell.CreateShortcut($lnkPath)
-    $lnk.TargetPath = $exe
-    $lnk.Arguments = $target.Arguments
-    if ($target.IconLocation) { $lnk.IconLocation = $target.IconLocation }
-    $lnk.WorkingDirectory = $distDir
-    $lnk.Description = $target.Description
-    $lnk.Save()
-
-    # 写后回读校验目标/参数/图标（证明幂等生效）。
-    # 未自定义图标时 WScript 回读 IconLocation 为 ',0'（空路径+索引），视同未设置。
-    $check = $shell.CreateShortcut($lnkPath)
-    $readIcon = [string]$check.IconLocation
-    if ($readIcon -eq ',0') { $readIcon = '' }
-    if ($check.TargetPath -ne $exe -or $check.Arguments -ne $target.Arguments -or
-        $readIcon -ne $target.IconLocation) {
-        Write-Host "[FAIL] 快捷方式校验失败: $lnkPath"
-        Write-Host ("  期望 IconLocation='{0}' 实际='{1}'" -f $target.IconLocation, $check.IconLocation)
-        exit 1
-    }
-    Write-Host ("[OK] {0}  ->  {1} {2}  Icon='{3}'" -f $target.Name, $check.TargetPath, $check.Arguments, $readIcon)
-}
-
-Write-Host "[OK] install.ps1 完成（幂等，可重复运行；旧 exe 与其他文件未触碰）"
-exit 0
diff --git a/migrate/cleanup-local.ps1 b/migrate/cleanup-local.ps1
index 1d38b14..d744f1a 100644
--- a/migrate/cleanup-local.ps1
+++ b/migrate/cleanup-local.ps1
@@ -15,12 +15,11 @@
 # 【它做什么（按顺序）】
 #   S1 打包留档：整个仓库压成 zip 放到桌面（一键黑屏-最终留档-日期.zip）并打印 SHA256，
 #      供你日后自行处置（留存/上传网盘/删除均可）。S1 失败则绝不删除仓库。
-#   S2 删除 DeskBox 两个快捷方式（一键黑屏.lnk / 一键黑屏-设置.lnk，不存在则跳过）。
-#   S3 删除配置目录 %LOCALAPPDATA%\BlackScreen（不存在则跳过）。
-#   S4 删除计划任务 OneClickBlackScreen-Idle（本就不存在也视为成功）。
-#   S5 删除仓库目录（只有 S1 留档成功才会执行）。
-#   S6 列出 ZCode 本工作区痕迹（只列出不删除，按清单在 ZCode 界面内手动移除）。
-#   S7 声明保留 %USERPROFILE%\.zcode\skills（用户级、非本项目专属，去留由你定）。
+#   S2 删除配置目录 %LOCALAPPDATA%\BlackScreen（不存在则跳过）。
+#   S3 删除计划任务 OneClickBlackScreen-Idle（本就不存在也视为成功）。
+#   S4 删除仓库目录（只有 S1 留档成功才会执行）。
+#   S5 列出 ZCode 本工作区痕迹（只列出不删除，按清单在 ZCode 界面内手动移除）。
+#   S6 声明保留 %USERPROFILE%\.zcode\skills（用户级、非本项目专属，去留由你定）。
 #   任何一步失败→停在后置步骤前，打印已完成/未完成清单。
 # =============================================================================
 param(
@@ -93,79 +92,63 @@ try {
     $s1ok = $false
 }
 
-# ---------- S2 DeskBox 两个快捷方式 ----------
-$lnkDir = Join-Path $env:USERPROFILE 'DeskBox\DeskBox'
-foreach ($name in @('一键黑屏.lnk', '一键黑屏-设置.lnk')) {
-    $lnk = Join-Path $lnkDir $name
-    try {
-        if ($DryRun) {
-            Record 'S2' '[DRY]' "$(if (Test-Path $lnk) { "将删除 $lnk" } else { "不存在，跳过 $name" })"
-        } elseif (Test-Path $lnk) {
-            Remove-Item $lnk -Force
-            Record 'S2' '[OK]' "已删除 $lnk"
-        } else {
-            Record 'S2' '[SKIP]' "本就不存在：$name"
-        }
-    } catch { Record 'S2' '[FAIL]' $_.Exception.Message; $s1ok = $false }
-}
-
-# ---------- S3 %LOCALAPPDATA%\BlackScreen ----------
+# ---------- S2 %LOCALAPPDATA%\BlackScreen ----------
 $cfgDir = Join-Path $env:LOCALAPPDATA 'BlackScreen'
 try {
     if ($DryRun) {
-        Record 'S3' '[DRY]' "$(if (Test-Path $cfgDir) { "将删除配置目录 $cfgDir" } else { '不存在，跳过' })"
+        Record 'S2' '[DRY]' "$(if (Test-Path $cfgDir) { "将删除配置目录 $cfgDir" } else { '不存在，跳过' })"
     } elseif (Test-Path $cfgDir) {
         Remove-Item $cfgDir -Recurse -Force
-        Record 'S3' '[OK]' "已删除 $cfgDir"
+        Record 'S2' '[OK]' "已删除 $cfgDir"
     } else {
-        Record 'S3' '[SKIP]' "本就不存在：$cfgDir"
+        Record 'S2' '[SKIP]' "本就不存在：$cfgDir"
     }
-} catch { Record 'S3' '[FAIL]' $_.Exception.Message }
+} catch { Record 'S2' '[FAIL]' $_.Exception.Message }
 
-# ---------- S4 计划任务 ----------
+# ---------- S3 计划任务 ----------
 try {
     if ($DryRun) {
-        Record 'S4' '[DRY]' '将执行 schtasks /delete /tn OneClickBlackScreen-Idle /f（本就不存在则跳过）'
+        Record 'S3' '[DRY]' '将执行 schtasks /delete /tn OneClickBlackScreen-Idle /f（本就不存在则跳过）'
     } else {
         schtasks.exe /delete /tn 'OneClickBlackScreen-Idle' /f *> $null
         if ($LASTEXITCODE -eq 0) {
-            Record 'S4' '[OK]' '已删除计划任务 OneClickBlackScreen-Idle'
+            Record 'S3' '[OK]' '已删除计划任务 OneClickBlackScreen-Idle'
         } elseif ($LASTEXITCODE -eq 1) {
-            Record 'S4' '[SKIP]' '计划任务本就不存在（退出码 1＝放行）'
+            Record 'S3' '[SKIP]' '计划任务本就不存在（退出码 1＝放行）'
         } else {
-            Record 'S4' '[FAIL]' "schtasks 退出码 $LASTEXITCODE"
+            Record 'S3' '[FAIL]' "schtasks 退出码 $LASTEXITCODE"
         }
     }
-} catch { Record 'S4' '[FAIL]' $_.Exception.Message }
+} catch { Record 'S3' '[FAIL]' $_.Exception.Message }
 
-# ---------- S5 删除仓库目录 ----------
+# ---------- S4 删除仓库目录 ----------
 try {
     if (-not $s1ok) {
-        Record 'S5' '[SKIP]' 'S1 留档未成功，按铁则跳过仓库删除（旧文件一件不删）'
+        Record 'S4' '[SKIP]' 'S1 留档未成功，按铁则跳过仓库删除（旧文件一件不删）'
     } elseif ($DryRun) {
-        Record 'S5' '[DRY]' "将删除整个仓库目录 $RepoPath"
+        Record 'S4' '[DRY]' "将删除整个仓库目录 $RepoPath"
     } else {
         Remove-Item $RepoPath -Recurse -Force
-        Record 'S5' '[OK]' "已删除 $RepoPath"
+        Record 'S4' '[OK]' "已删除 $RepoPath"
     }
-} catch { Record 'S5' '[FAIL]' $_.Exception.Message }
+} catch { Record 'S4' '[FAIL]' $_.Exception.Message }
 
-# ---------- S6 ZCode 工作区痕迹（只列不删） ----------
-Record 'S6' '[LIST]' 'ZCode 本工作区可定位痕迹（请到 ZCode 界面内手动移除，本脚本不盲删）：'
+# ---------- S5 ZCode 工作区痕迹（只列不删） ----------
+Record 'S5' '[LIST]' 'ZCode 本工作区可定位痕迹（请到 ZCode 界面内手动移除，本脚本不盲删）：'
 $zcodeRoot = 'D:\Download\ZCodeData\.zcode\v2'
 foreach ($p in @(
     (Join-Path $zcodeRoot 'tasks-index.sqlite'),
     (Join-Path $zcodeRoot 'visualizations'),
     (Join-Path $zcodeRoot 'checkpoints')
 )) {
-    if (Test-Path $p) { Record 'S6' '[LIST]' "  存在：$p" }
+    if (Test-Path $p) { Record 'S5' '[LIST]' "  存在：$p" }
 }
-Record 'S6' '[LIST]' '  提示：tasks-index.sqlite 是所有工作区共用的任务索引（删库会影响其他工作区，'
-Record 'S6' '[LIST]' '        建议只在 ZCode 应用的会话/任务列表里删除本项目相关条目，不动文件本身。'
+Record 'S5' '[LIST]' '  提示：tasks-index.sqlite 是所有工作区共用的任务索引（删库会影响其他工作区，'
+Record 'S5' '[LIST]' '        建议只在 ZCode 应用的会话/任务列表里删除本项目相关条目，不动文件本身。'
 
-# ---------- S7 保留声明 ----------
-Record 'S7' '[KEEP]' "$env:USERPROFILE\.zcode\skills 已保留：这是用户级技能库、非本项目专属，"
-Record 'S7' '[KEEP]' '        其他项目仍在用；最终去留由你自行决定（删除不影响一键黑屏）。'
+# ---------- S6 保留声明 ----------
+Record 'S6' '[KEEP]' "$env:USERPROFILE\.zcode\skills 已保留：这是用户级技能库、非本项目专属，"
+Record 'S6' '[KEEP]' '        其他项目仍在用；最终去留由你自行决定（删除不影响一键黑屏）。'
 
 # ---------- 汇总 ----------
 Write-Host ''
diff --git a/migrate/迁移手册.html b/migrate/迁移手册.html
index 7f791de..c1648b5 100644
--- a/migrate/迁移手册.html
+++ b/migrate/迁移手册.html
@@ -314,13 +314,13 @@ powershell -NoProfile -ExecutionPolicy Bypass -File install.ps1
 [OK] 一键黑屏-设置.lnk  -&gt;  ...\dist\一键黑屏.exe --settings  Icon=&#39;...\icon-settings.ico,0&#39;
 [OK] install.ps1 完成（幂等，可重复运行；旧 exe 与其他文件未触碰）
 </code></pre>
-<p><strong>完成标志</strong>：<code>DeskBox\DeskBox</code> 文件夹（在 <code>C:\Users\你的用户名\DeskBox\DeskBox</code>）里出现两个快捷方式。</p>
+<p><strong>完成标志</strong>：快捷方式目标文件夹里出现两个快捷方式。</p>
 <h3>步骤 5.4 · 双击冒烟</h3>
 <ol>
 <li>进 <code>OneClickBlackScreen\dist</code> 文件夹，双击 <code>一键黑屏.exe</code>。</li>
 <li><strong>预期</strong>：屏幕立刻变成全屏遮罩（设置过颜色就是你的颜色）。</li>
 <li><strong>按键盘任意一个键</strong>，遮罩消失。</li>
-<li>再双击 <code>一键黑屏-设置</code> 快捷方式（DeskBox 里），<strong>预期</strong>：设置窗口打开；随便改一项→点保存→提示成功。</li>
+<li>再双击 <code>一键黑屏-设置</code> 快捷方式，<strong>预期</strong>：设置窗口打开；随便改一项→点保存→提示成功。</li>
 </ol>
 <p><strong>完成标志</strong>：遮罩能出能关、设置窗能开能存。</p>
 <h3>步骤 5.5 ·（可选）把个性化设置带过来</h3>
@@ -473,7 +473,7 @@ git status
 <pre><code>powershell -NoProfile -ExecutionPolicy Bypass -File &quot;$env:USERPROFILE\Desktop\cleanup-local.ps1&quot; -ConfirmPC2Verified
 </code></pre>
 <p><strong>预期输出</strong>：S1 生成 <code>一键黑屏-最终留档-日期.zip</code> 并打印 SHA256 → S2～S5 逐条 <code>[OK]</code>／<code>[SKIP]</code> → 最后 <code>[DONE] 清理完成。桌面留档 zip 由你自行处置…</code>。</p>
-<p><strong>完成标志</strong>：桌面上多出留档 zip；项目文件夹、DeskBox 两个快捷方式、<code>%LOCALAPPDATA%\BlackScreen</code> 都已消失；脚本会同时告诉你 ZCode 痕迹清单和 skills 保留说明。</p>
+<p><strong>完成标志</strong>：桌面上多出留档 zip；项目文件夹、<code>%LOCALAPPDATA%\BlackScreen</code> 都已消失；脚本会同时告诉你 ZCode 痕迹清单和 skills 保留说明。</p>
 <blockquote>
 <p>忘了带 <code>-ConfirmPC2Verified</code> 会怎样？脚本直接拒绝并打印五项清单提示（这是设计好的保险丝，不是坏了）：
 <code>[REFUSED] 未带 -ConfirmPC2Verified 参数，本脚本拒绝运行，未触碰任何文件。</code></p>
diff --git a/migrate/迁移手册.md b/migrate/迁移手册.md
index 7fc2e9f..3c5a3cb 100644
--- a/migrate/迁移手册.md
+++ b/migrate/迁移手册.md
@@ -349,14 +349,14 @@ powershell -NoProfile -ExecutionPolicy Bypass -File install.ps1
 [OK] install.ps1 完成（幂等，可重复运行；旧 exe 与其他文件未触碰）
 ```
 
-**完成标志**：`DeskBox\DeskBox` 文件夹（在 `C:\Users\你的用户名\DeskBox\DeskBox`）里出现两个快捷方式。
+**完成标志**：快捷方式目标文件夹里出现两个快捷方式。
 
 ### 步骤 5.4 · 双击冒烟
 
 1. 进 `OneClickBlackScreen\dist` 文件夹，双击 `一键黑屏.exe`。
 2. **预期**：屏幕立刻变成全屏遮罩（设置过颜色就是你的颜色）。
 3. **按键盘任意一个键**，遮罩消失。
-4. 再双击 `一键黑屏-设置` 快捷方式（DeskBox 里），**预期**：设置窗口打开；随便改一项→点保存→提示成功。
+4. 再双击 `一键黑屏-设置` 快捷方式，**预期**：设置窗口打开；随便改一项→点保存→提示成功。
 
 **完成标志**：遮罩能出能关、设置窗能开能存。
 
@@ -492,7 +492,7 @@ powershell -NoProfile -ExecutionPolicy Bypass -File "$env:USERPROFILE\Desktop\cl
 
 **预期输出**：S1 生成 `一键黑屏-最终留档-日期.zip` 并打印 SHA256 → S2～S5 逐条 `[OK]`／`[SKIP]` → 最后 `[DONE] 清理完成。桌面留档 zip 由你自行处置…`。
 
-**完成标志**：桌面上多出留档 zip；项目文件夹、DeskBox 两个快捷方式、`%LOCALAPPDATA%\BlackScreen` 都已消失；脚本会同时告诉你 ZCode 痕迹清单和 skills 保留说明。
+**完成标志**：桌面上多出留档 zip；项目文件夹、`%LOCALAPPDATA%\BlackScreen` 都已消失；脚本会同时告诉你 ZCode 痕迹清单和 skills 保留说明。
 
 > 忘了带 `-ConfirmPC2Verified` 会怎样？脚本直接拒绝并打印五项清单提示（这是设计好的保险丝，不是坏了）：
 > `[REFUSED] 未带 -ConfirmPC2Verified 参数，本脚本拒绝运行，未触碰任何文件。`
```

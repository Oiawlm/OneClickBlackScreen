# MIGRATE-V5D · 轨道 D 交付证据（迁移包与本地清理）

> GOAL-V5D 执行记录。日期 2026-11-03。上游：RESEARCH-V5 §5 D 系/§3 R4、PLAN-V5 §3 波 3。
> 结论：**轨道 D 完成，待用户执行 PC2 迁移→验证→本地清理**（手册阶段 1–9）。

## 0. 前置校验

- main 干净且含 I 波合并（tip `c827b44`，工作区 clean）。
- `gh repo view Oiawlm/OneClickBlackScreen` 可达；`gh release view v1.0.0` 三资产在位：
  `OneClickBlackScreen-v1.0.0.zip`（label「一键黑屏 v1.0.0 完整包（exe＋config＋ico＋使用说明）」，SHA256 `00363518…F66A7`）、`OneClickBlackScreen.exe`（`9A93CDD5…29CD1`）、`SHA256SUMS`（与 docs/VERIFY-V5.md §资产表一致，非凭记忆）。

## 1. Phase 1 · install.ps1 迁移就绪硬化（D1）

| 证据 | 命令 | 结果 |
|---|---|---|
| 零硬编码 | `git grep -n "C:\\Users\\Lenovo" install.ps1` | 无命中（exit 1） |
| 幂等 ×2 | `powershell -NoProfile -ExecutionPolicy Bypass -File install.ps1`（两遍） | 均两行 `[OK] …lnk` ＋ `[OK] install.ps1 完成` |
| 守门 | `verify.cmd` | `[OK] ALL VERIFIERS PASSED`（七件全绿） |
| 移植性（A9） | 硬化版脚本复制到临时 clone `D:\Download\_v5d-smoke\OneClickBlackScreen` 运行 | `[OK]`；DeskBox 两 .lnk 正确指向**临时 clone** 的 dist（路径派生自脚本位置，用户目录派生自 `$env:USERPROFILE`） |
| 恢复（A10） | 仓库自身 install.ps1 重跑 | .lnk 指回 `D:\Download\Agent Vault\一键黑屏\dist` |

改动：`$deskBox = Join-Path $env:USERPROFILE 'DeskBox\DeskBox'`（原 `C:\Users\Lenovo\…` 硬编码）；注释同步改写并加「可改目录」说明。回读校验逻辑未动。

## 2. Phase 2 · 迁移包清单（migrate/）

| 文件 | 大小 | SHA256 |
|---|---|---|
| 一键黑屏.bundle（`git bundle create --all`，基线＝Commit A `<回填>`；Commit B 仅回填本表 digest 的文档增量，无代码/手册变更） | `<回填>` | `<回填>` |
| skills-三件套.zip（fts-style-system/glm-prompt-crafter/github-readme；wb-bridge 未入包＝可选件含机器相关 node_modules） | `<回填>` | `<回填>` |
| settings-可选备份.ini（`%LOCALAPPDATA%\BlackScreen\settings.ini` 副本，个性化可选件） | `<回填>` | `<回填>` |
| SHA256SUMS.txt（上三件，`哈希␣␣文件名`） | — | 以文件为准 |

凭证不入包核查：四技能目录 `ghp_|sk-|api_key|token|PRIVATE KEY` 模式扫描＝零命中；bundle 仅含仓库 git 对象（仓库自身无 `.wbx/`——.gitignore 历来排除）；zip 来源三技能目录（扫描同上）；ini 为界面偏好键值。`.gitignore` 新增 `migrate/*.zip|*.bundle|*.ini`（大文件不入库、路径登记于本表）。

## 3. Phase 3/4 · 手册与清理脚本

- `migrate/迁移手册.md`：四层结构齐——①总流程图（mermaid＋`总流程图.png` 双载体，PNG 经子代理目检 13 节点无乱码无重叠）②阶段小图（mermaid ×4）③步骤卡（阶段 0–9 全编号，每卡＝目的/在哪操作/复制粘贴命令/预期输出/完成标志）④排错表（分阶段 3 张＋总表 1 张）。菜鸟口径：术语（仓库/克隆/终端/SHA256/执行策略等）首次出现一句话解释＋附录词典；无「自行判断」类措辞。
- `migrate/迁移手册.html`：自包含可打印版（marked＋5 图内嵌 base64；DOM 断言：h1×1、img×5 全 data URI 零裂图、table×7、pre×32；本地 HTTP 目检中文渲染正常）。
- `migrate/cleanup-local.ps1`（UTF-8 BOM，PS 5.1 兼容）：`-ConfirmPC2Verified` 缺省拒绝；`-DryRun` 只打印；仓库内运行拒绝（强制桌面副本操作序）；S1 留档失败→S5 跳过删除。

**拒绝路径实跑（B1）**：`powershell … -File cleanup-local.ps1`（无开关）→ `[REFUSED] 未带 -ConfirmPC2Verified 参数…未触碰任何文件。`＋D6 五项清单全打印，exit **1**。

**DryRun 实跑（B2）**：`… -ConfirmPC2Verified -DryRun` → S1–S7 全 `[DRY]`/`[LIST]`/`[KEEP]` 列表，`[DONE] DryRun 演练结束：未改动任何文件。` exit 0。前后快照比对：DeskBox 两 .lnk、`%LOCALAPPDATA%\BlackScreen\settings.ini`、仓库目录、桌面留档 zip——**全部不变**。

## 4. 命令冒烟记录（手册 22 命令块逐一；「本机已跑」均有原始输出，存 `_v5d-smoke/*-smoke.txt`）

| # | 命令（手册处） | 冒烟 | 结果/关键输出 |
|---|---|---|---|
| A1 | `winget --version`（2.1） | ✅本机 | `v1.29.380` |
| A2 | `winget install --id Git.Git -e --source winget`（2.2） | ⚠️半代验 | 本机已装 git 不重装；`winget show --id Git.Git` 证 ID 可达（`已找到 Git [Git.Git] 2.56.0.4`） |
| A3 | `git --version`（2.2） | ✅本机 | `git version 2.55.0.windows.5` |
| A4 | `git config --global user.name/email`（3） | ✅只读代验 | `--get` 回显 `FireChou`/`Yglstr@outlook.com`（写入动作＝PC2 特有） |
| A5 | `git clone https://github.com/Oiawlm/OneClickBlackScreen`（4A.1） | ✅本机 | 临时目录全新克隆成功，tip `c827b44`＝origin |
| A6 | `git config core.quotepath false`＋`--get`（4A.2） | ✅本机 | 回显 `false` |
| A14 | `git status` / `git log --oneline -1`（4A.3/8） | ✅本机 | status 干净；log 显示 c827b44 |
| C2 | `certutil -hashfile 一键黑屏.bundle SHA256`（4B.1） | ✅本机（包生成后） | `<回填>` |
| C1 | `git clone migrate\一键黑屏.bundle OneClickBlackScreen`（4B.2） | ✅本机（包生成后） | `<回填>` |
| C3 | `git branch -a`（4B.3） | ✅本机（包生成后） | `<回填>` |
| C4 | `git remote set-url origin …`（4B 注） | ✅本机（包生成后） | `<回填>` |
| A7 | `build.cmd`（5.1） | ✅本机（临时 clone） | `[OK] Build succeeded: …dist\一键黑屏.exe`，exit 0 |
| A8 | `verify.cmd`（5.2） | ✅本机（临时 clone） | 七件全绿 `[OK] ALL VERIFIERS PASSED`，exit 0（全新 clone→构建→验证链路成立） |
| A9/A10 | `powershell -NoProfile -ExecutionPolicy Bypass -File install.ps1`（5.3） | ✅本机 | 见 §1 移植性行 |
| — | 双击 exe 冒烟（5.4） | ⚠️PC2 手动 | 图形步骤；按任意键退出（README:80 口径） |
| A16 | `Expand-Archive skills-三件套.zip`＋`dir`（6.2） | ✅本机（包生成后） | `<回填>` |
| A2b | `winget install --id GitHub.cli`（7） | ⚠️半代验 | `winget show` 证 ID 可达（`2.103.3`）；安装＝PC2 特有 |
| A15 | `gh --version`（7） | ✅本机 | `gh version 2.101.0`；`gh auth login` 交互＝PC2 手动 |
| A13 | `git fsck`（8） | ✅本机 | exit 0，仅 `dangling tree`（手册已注明属正常） |
| B2 | cleanup DryRun（9.2） | ✅本机 | 见 §3 |
| B3 | cleanup 真实执行（9.3） | 🚫绝不代跑 | 红线：唯一合法触发者＝用户在 PC2 验证后手动 |
| B1 | cleanup 无开关（9.3 注） | ✅本机 | 见 §3 拒绝路径 |

## 5. 验收清单对照（GOAL-V5D §5 八项）

| # | 要求 | 证据 |
|---|---|---|
| 1 | install.ps1 无硬编码＋幂等两遍 | §1 表（grep exit 1＋两遍 `[OK]`） |
| 2 | migrate/ 手册 md/html、bundle、skills zip 齐且含总图/步骤卡/排错表 | §2＋§3；`git bundle verify` 见 `<回填>` |
| 3 | SHA256SUMS.txt＋certutil 抽验两条 | `<回填>` |
| 4 | 手册含「在哪打开 PowerShell」图文＋每命令预期输出；≥15 块逐一冒烟 | §3 手册结构＋§4 表（22 块） |
| 5 | cleanup 无开关拒绝＋打印 D6；dry-run 落档 | §3 B1/B2 |
| 6 | 手册含五项勾选清单＋「验证前不删」警示 | 手册阶段 8 表＋首页红线框＋阶段 9 红线 |
| 7 | 本文档含包清单＋冒烟＋dry-run | §2/§4/§3 |
| 8 | git 干净＋大文件不入库 | Commit A/B 状态＋.gitignore 增量 |

## 6. 状态

**轨道 D 完成**——迁移包就位、手册冒烟完毕、清理脚本只生成未执行（红线遵守：本窗口未做任何真实删除）。下一步＝用户：拷 `migrate/` → PC2 照手册阶段 1–7 → 阶段 8 五项全勾 → 旧机阶段 9 亲手跑 cleanup-local.ps1。

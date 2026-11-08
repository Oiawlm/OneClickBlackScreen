# GOAL-V9I · 集成与发版 v1.1.0（I 轨）

> **一句话定位**：I 轨执行窗（GLM-5.3·目标模式）：把 review-v9（内含 A＋B）`--no-ff` 合入 main、版本单源 bump 1.1.0、构建并发布 v1.1.0（Setup＋更新器资产＋便携 zip＋SHA256SUMS 四资产）、真机安装冒烟＋应用内冒烟、清理 worktree/分支、留档 `VERIFY-V9.md` 并推送 main。　**事实源＝[RESEARCH-V9.md](RESEARCH-V9.md)（§3 D3-D5）＋[REVIEW-V9.md](REVIEW-V9.md) 总评与移交注记；发版惯例按 docs/README.md 路由表「发版（I 轨）」行读 archive 内上一份 VERIFY-V?I。**　日期：2026-11-05　行数预算：≤110 行。

## 1. 身份与冷启动

你是 I 轨执行窗（集成窗），产出集成结果与发版证据。按序自读（主检出 `D:\Download\Agent Vault\OneClickBlackScreen\OneClickBlackScreen-main\`）：1. `AGENTS.md`；2. `docs/handoff/2026-11-05-v9-installer/RESEARCH-V9.md` §3＋`PLAN-V9.md` §3/§6；3. `REVIEW-V9.md` 全文（总评须为 **GO**；NO-GO＝停下报告用户）；4. 按 docs/README.md 路由表发版行读 archive 内上一份 `VERIFY-V?I`（最近一次发版实操，资产上传/digest 惯例照它）；5. 本文件。**禁读**：archive 其余全文、`baseline/**`。

工作位置：**主检出**（集成窗独占写；本轨不建 worktree）。REVIEW §9 末尾「快速启动」块与本文件冲突时**以本文件为准**（主检出直改 main，不建 worktree、不建 integration 分支）。前置：`git status` 干净、`git ls-remote --heads origin feature/review-v9` 可见、REVIEW 移交注记中无未闭环的阻断项。

## 2. 背景与职责

本轮有产品代码变更（Updater.cs ARP 联动），按 AGENTS §5 红线必须走完整发版链才算交付；分发主渠道自 v1.1.0 起为 **Setup 安装包**（RESEARCH D2/D4）。你负责：合并→版本→构建→发版→双冒烟→零足迹审计→清理→留档，一次走完。

## 3. Phase 任务（依序）

**P1 合并**：main 上 `git merge --no-ff origin/review-v9 -m "merge: V9 …"`；`git diff origin/main^...HEAD --name-status` 与 REVIEW 合并态清单一致性核对。
**P2 版本与构建**：`src/AppInfo.cs` Version 1.0.2→**1.1.0**（全仓唯一版本改动点，`git grep -n "1\.0\.2" -- src` 改后零命中）；`README.md` 版本引用/下载链接处 1.0.2→1.1.0（仅此类行，其余零改动）；**使用说明.txt 联动（REVIEW B-5，先于资产构建——该文件入 Setup 与 zip 两种资产包）**：标题版本号对齐至当前修订 v8（现标题滞留 v5，正文更新记录已至 v7），正文顶部增一行「v8 更新（对应程序版本 1.1.0）：推荐 Setup 安装包安装；开始菜单新增「一键黑屏-设置」入口」（仅标题行＋该新增行，其余零改动）；随后 `build.cmd` 绿；`build-setup.cmd` 绿→产物 `dist\OneClickBlackScreen-Setup-1.1.0.exe`；便携 zip＝dist 三件套＋改后使用说明.txt（内容清单与 README 表述一致）。
**P3 验证×2**：`verify.cmd` 七件全绿**两轮独立实录**（闸门独占时段，撞 BUSY 等待）。
**P4 发版**：tag `v1.1.0`（ annotated）；GitHub Release**元数据与资产**——元数据沿 v1.0.2 Release 实例惯例：name＝`v1.1.0`、标记 **Latest**（非 pre-release、非 draft）、target＝tag、body＝`## v1.1.0 更新内容`四节（`### 新增`：Setup 安装包为推荐分发渠道——per-user 免管理员、开始菜单双入口、可完整卸载；`### 移除`：install.ps1 与 DeskBox 个人环境设施退出仓库；`### 开发者向`：更新器替换后同步「添加/删除程序」显示版本；验证套件行为零变化；`### 完整包说明`：四资产语义＋`certutil` 校验示例）；资产四件＝`OneClickBlackScreen-Setup-1.1.0.exe`＋`OneClickBlackScreen.exe`（更新器契约资产，名不变）＋便携 zip（ASCII 文件名，L-015；中文名只进 label）＋`SHA256SUMS.txt`（覆盖全部资产，digest 以上传后服务端返回为准逐项核对）。
**P5 真机安装冒烟**（RESEARCH D5）：①前快照＝settings.ini 三态＋计划任务三态＋`{autopf}` 目录与开始菜单现状；②运行 Setup 正常安装（真实 AppId，默认目录）→断言：安装目录四文件、开始菜单两快捷方式、HKCU 卸载键 DisplayVersion＝1.1.0；③启动已装 `一键黑屏.exe --settings`→设置窗打开→**不保存直接关闭**（不触用户设置/任务）；④「添加/删除程序」卸载→断言零残留（目录/快捷方式/卸载键全清）；⑤后快照与①逐项一致（用户状态字节级不变）。
**P6 应用内冒烟**：按上一份 VERIFY-V?I 惯例——设置窗「检查更新」对 v1.0.2 旧版二进制指向 latest＝v1.1.0 的更新链路（离线 mock 或实测按惯例）；已装 1.1.0 本体检查＝「已是最新」。
**P7 零足迹审计＋发版收尾＋清理**：三态审计（settings ABSENT 三态一致/任务三态/`_probe` 零落盘）；`docs/handoff/2026-11-05-v9-installer/VERIFY-V9.md`（六字段模板；含 P1-P6 全部真实输出与 RELEASE URL）；`git push origin main --follow-tags`；随后分支清理（用户 /goal 指令「该删除的分支就删除」，2026-11-05 起**取代** V6~V8「远端保留」惯例）——worktree×3（v9-a/v9-b/v9-r `git worktree remove`＋`prune`）；本地 `git branch -d`＝v9 三支＋已并入 main 的历史四支（exit-arm-v6/update-guard-v7/verify-env-v6/verify-isolation-v7）；远端＝全部 `feature/*` **逐支先证** `git merge-base --is-ancestor origin/<支> origin/main` 为真再 `git push origin --delete <支>`（V6×2＋V7×2＋V8×3＋V9×3＝10 支；任一非 ancestor＝停下报告，不强删）；终态检查：`src/` 除 AppInfo 版本行外零 diff、全仓 `grep -ri deskbox` 归零、README 下载节指向 v1.1.0、`git ls-remote --heads origin` 仅剩 main。

## 4. 铁律

写面白名单：合并提交带入的 A/B 改动＋`src/AppInfo.cs`(仅 Version 行)＋`README.md`(仅版本引用/下载链接行)＋`使用说明.txt`(仅 B-5 两处：标题行＋v8 更新行)＋`VERIFY-V9.md`(新)＋tag/Release。其余一律只读。**禁改**：verify/**、build.cmd/build.rsp、AGENTS.md、lessons、A/B 已合入内容（缺陷退回对应轨，不代修）。资产名全 ASCII；凭证不入任何文件（gh 已登录态操作，token 不落盘）；真实用户状态只读（冒烟不保存设置＝不写 settings.ini/不建任务）；不删仓库、不改写远端历史（删已实证并入 main 的分支指针不属改写历史，且以 ancestor 前置证明为限）；`migrate/cleanup-local.ps1` 不运行。

## 5. 验收清单（校验器逐条核对实据）

- [ ] main 含 `--no-ff` 合并提交且 `git ls-remote` main 与 tag v1.1.0 可见
- [ ] `git grep -n "1\.0\.2" -- src` 零命中；AppInfo Version＝1.1.0 全仓唯一版本号
- [ ] Release v1.1.0 四资产齐且服务端 digest 与 SHA256SUMS 逐项一致（输出留档）；元数据＝Latest、非 pre-release、非 draft，body 含四节更新内容（`gh release view` JSON 留档）
- [ ] VERIFY-V9.md 含：verify×2 全绿、真机冒烟 ①-⑤ 全断言＋前后快照一致（B-3 三处快捷方式/卸载自清重点）、应用内冒烟、零足迹三态、worktree/分支清理实录
- [ ] 分支清理：本地零 feature/*、`git ls-remote --heads origin` 仅剩 main（10 支删除实录＋逐支 ancestor 前置证明）；使用说明.txt B-5 两处落地且已入两种资产包
- [ ] 全仓 `grep -ri deskbox` 归零；README 下载节无 1.0.2 残留引用
- [ ] 用户真实状态：settings.ini/计划任务前后一致（快照留档）

## 6. 收尾动作

VERIFY-V9.md 记录 REVIEW 移交注记 14 条制式处置（闭环/备案；**B-3（真机快捷方式/卸载自清）与 B-5（使用说明联动）为 I 轨本职，必须闭环留档**）；报告：main 哈希、tag、Release URL、验收清单逐条 ✅/❌。V9 批次目录留待 V10 规划窗归档（PLAN §6）。

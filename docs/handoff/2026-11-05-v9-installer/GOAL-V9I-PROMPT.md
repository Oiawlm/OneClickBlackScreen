# GOAL-V9I · 集成与发版 v1.1.0（I 轨）

> **一句话定位**：I 轨执行窗（GLM-5.3·目标模式）：把 review-v9（内含 A＋B）`--no-ff` 合入 main、版本单源 bump 1.1.0、构建并发布 v1.1.0（Setup＋更新器资产＋便携 zip＋SHA256SUMS 四资产）、真机安装冒烟＋应用内冒烟、清理 worktree/分支、留档 `VERIFY-V9.md` 并推送 main。　**事实源＝[RESEARCH-V9.md](RESEARCH-V9.md)（§3 D3-D5）＋[REVIEW-V9.md](REVIEW-V9.md) 总评与移交注记；发版惯例按 docs/README.md 路由表「发版（I 轨）」行读 archive 内上一份 VERIFY-V?I。**　日期：2026-11-05　行数预算：≤110 行。

## 1. 身份与冷启动

你是 I 轨执行窗（集成窗），产出集成结果与发版证据。按序自读（主检出 `D:\Download\Agent Vault\OneClickBlackScreen\OneClickBlackScreen-main\`）：1. `AGENTS.md`；2. `docs/handoff/2026-11-05-v9-installer/RESEARCH-V9.md` §3＋`PLAN-V9.md` §3/§6；3. `REVIEW-V9.md` 全文（总评须为 **GO**；NO-GO＝停下报告用户）；4. 按 docs/README.md 路由表发版行读 archive 内上一份 `VERIFY-V?I`（最近一次发版实操，资产上传/digest 惯例照它）；5. 本文件。**禁读**：archive 其余全文、`baseline/**`。

工作位置：**主检出**（集成窗独占写；本轨不建 worktree）。前置：`git status` 干净、`git ls-remote --heads origin feature/review-v9` 可见、REVIEW 移交注记中无未闭环的阻断项。

## 2. 背景与职责

本轮有产品代码变更（Updater.cs ARP 联动），按 AGENTS §5 红线必须走完整发版链才算交付；分发主渠道自 v1.1.0 起为 **Setup 安装包**（RESEARCH D2/D4）。你负责：合并→版本→构建→发版→双冒烟→零足迹审计→清理→留档，一次走完。

## 3. Phase 任务（依序）

**P1 合并**：main 上 `git merge --no-ff origin/review-v9 -m "merge: V9 …"`；`git diff origin/main^...HEAD --name-status` 与 REVIEW 合并态清单一致性核对。
**P2 版本与构建**：`src/AppInfo.cs` Version 1.0.2→**1.1.0**（全仓唯一版本改动点，`git grep -n "1\.0\.2" -- src` 改后零命中）；`build.cmd` 绿；`build-setup.cmd` 绿→产物 `dist\OneClickBlackScreen-Setup-1.1.0.exe`；便携 zip＝dist 三件套＋使用说明.txt（内容清单与 README 表述一致）；`README.md` 版本引用/下载链接处 1.0.2→1.1.0（仅此类行，其余零改动）。
**P3 验证×2**：`verify.cmd` 七件全绿**两轮独立实录**（闸门独占时段，撞 BUSY 等待）。
**P4 发版**：tag `v1.1.0`（ annotated）；GitHub Release v1.1.0 资产四件＝`OneClickBlackScreen-Setup-1.1.0.exe`＋`OneClickBlackScreen.exe`（更新器契约资产，名不变）＋便携 zip（ASCII 文件名，L-015；中文名只进 label）＋`SHA256SUMS.txt`（覆盖全部资产，digest 以上传后服务端返回为准逐项核对）。
**P5 真机安装冒烟**（RESEARCH D5）：①前快照＝settings.ini 三态＋计划任务三态＋`{autopf}` 目录与开始菜单现状；②运行 Setup 正常安装（真实 AppId，默认目录）→断言：安装目录四文件、开始菜单两快捷方式、HKCU 卸载键 DisplayVersion＝1.1.0；③启动已装 `一键黑屏.exe --settings`→设置窗打开→**不保存直接关闭**（不触用户设置/任务）；④「添加/删除程序」卸载→断言零残留（目录/快捷方式/卸载键全清）；⑤后快照与①逐项一致（用户状态字节级不变）。
**P6 应用内冒烟**：按上一份 VERIFY-V?I 惯例——设置窗「检查更新」对 v1.0.2 旧版二进制指向 latest＝v1.1.0 的更新链路（离线 mock 或实测按惯例）；已装 1.1.0 本体检查＝「已是最新」。
**P7 零足迹审计＋清理＋留档**：三态审计（settings ABSENT 三态一致/任务三态/`_probe` 零落盘）；worktree 清理×3（v9-a/v9-b/v9-r，`git worktree remove`＋`prune`＋`branch -d` 三分支；远端分支照 V6/V7/V8 惯例留）；`docs/handoff/2026-11-05-v9-installer/VERIFY-V9.md`（六字段模板；含 P1-P6 全部真实输出与 RELEASE URL）；`git push origin main --follow-tags`；终态检查：`src/` 除 AppInfo 版本行外零 diff、全仓 `grep -ri deskbox` 归零、README 下载节指向 v1.1.0。

## 4. 铁律

写面白名单：合并提交带入的 A/B 改动＋`src/AppInfo.cs`(仅 Version 行)＋`README.md`(仅版本引用/下载链接行)＋`VERIFY-V9.md`(新)＋tag/Release。其余一律只读。**禁改**：verify/**、build.cmd/build.rsp、AGENTS.md、lessons、A/B 已合入内容（缺陷退回对应轨，不代修）。资产名全 ASCII；凭证不入任何文件（gh 已登录态操作，token 不落盘）；真实用户状态只读（冒烟不保存设置＝不写 settings.ini/不建任务）；不删仓库、不改写远端历史；`migrate/cleanup-local.ps1` 不运行。

## 5. 验收清单（校验器逐条核对实据）

- [ ] main 含 `--no-ff` 合并提交且 `git ls-remote` main 与 tag v1.1.0 可见
- [ ] `git grep -n "1\.0\.2" -- src` 零命中；AppInfo Version＝1.1.0 全仓唯一版本号
- [ ] Release v1.1.0 四资产齐且服务端 digest 与 SHA256SUMS 逐项一致（输出留档）
- [ ] VERIFY-V9.md 含：verify×2 全绿、真机冒烟 ①-⑤ 全断言＋前后快照一致、应用内冒烟、零足迹三态、worktree/分支清理实录
- [ ] 全仓 `grep -ri deskbox` 归零；README 下载节无 1.0.2 残留引用
- [ ] 用户真实状态：settings.ini/计划任务前后一致（快照留档）

## 6. 收尾动作

VERIFY-V9.md 记录 REVIEW 移交注记 8 条制式处置（闭环/备案）；报告：main 哈希、tag、Release URL、验收清单逐条 ✅/❌。V9 批次目录留待 V10 规划窗归档（PLAN §6）。

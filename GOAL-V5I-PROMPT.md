# GOAL-V5I · 集成与正式发布 v1.0.0（轨道 I）

> **一句话定位**：把 B/C 两分支按契约合并回 main（7 件验证全绿守门）→门面终稿（README/使用说明 v6）→tag v1.0.0→正式 Release（裸 exe＋zip＋SHA256SUMS）→发布后四重验收。
> **上游事实源**：[RESEARCH-V5.md](RESEARCH-V5.md) §4 并行契约、§5 I 系（I1–I5）、§5 跨轨常量登记表　**编排**：[PLAN-V5.md](PLAN-V5.md) §3 波 2（前置：A/B/C 三窗口已完成）。
> **日期**：2026-11-02　行数预算：≤180 行。

## 0. 身份与工作方式

- 你是执行窗口，主模型 GLM-5.3，以**目标模式（/goal）**工作：只产出落地改动与可核对证据。
- 第一性原理；**禁止打补丁**——合并冲突按「两分支设计意图都保留」裁决，不用「选一边」糊过去；门面终稿按单一事实源整理，不造副本。
- 尽量用内置 ZCode 子代理（截图比对/机械扫描可扇出 Flash 子代理）；wbx 未登录则不用。
- 门面终稿涉及 README **必须先调用 github-readme skill**；涉及设置窗描述与 fts 清单先读 fts-style-system。

## 1. 上下文承接（冷启动固定顺序）

1. 读 `RESEARCH-V5.md` 全文（§4 契约与常量登记表是你的裁决依据）与 `PLAN-V5.md` §2/§3
2. 读三轨交付物：`docs/PUBLISH-V5A.md`（A 的证据与 draft 登记）、`docs/VERIFY-V5B.md`＋`docs/COPY-V5B.md`、`docs/VERIFY-V5C.md`＋`docs/COPY-V5C.md`
3. 读 `git log --oneline main..feature/update-v5` 与 `git log --oneline main..feature/idle-v5`（两分支全部提交）与两分支 diff
4. 读现行 `README.md`（A 已改版）、`使用说明.txt`、`verify.cmd`、`src/AppInfo.cs`
- **前置校验**：两分支均存在且工作树干净；`gh auth status` 正常。任一前置不满足→停止并报告，不得带病集成。

## 2. 背景

A 轨道已建公开仓（含 draft `v0.9.0-test`）；B/C 两分支并行开发了自动更新与空闲唤起，共同触碰 `SettingsDialog.cs`/`SettingsDialogMetrics.cs`/`verify.cmd`/`VerifySettings.cs`/`Program.cs`/`build.rsp`（契约预期内的文本冲突，非设计冲突）。本轨道是全轮唯一能把功能合入 main、把门面定稿、把正式版本发出去的窗口（单一写者）。

## 3. Phase 任务

### Phase 1 · 合并 B（I1）
- `git checkout main`→`git merge --no-ff feature/update-v5`；冲突逐个按「B 的设计意图」裁决（另一分支尚未进入，无跨轨冲突）。
- 合并态跑 `build.cmd`＋`verify.cmd`（B 的六件套）全绿后才算本步完成。

### Phase 2 · 合并 C（I1）
- `git rebase main feature/idle-v5`（或等价融合）→冲突按「**两分支意图都保留**」裁决：`Program.cs`＝B 入口清理行＋C `--idle` 分支并存；`SettingsDialog/Metrics`＝页脚区与唤起区并存、`fixedChrome` 增量相加、previewCap 重算；`verify.cmd`＝七件编号 `[1/7]..[7/7]`（VerifyUpdate=6、VerifyIdle=7）；`VerifySettings` 几何断言纳入**全部**新区控件并复核 17→N 子控件计数与矩形复算；`build.rsp` 两新源齐。
- 回 main `git merge --no-ff feature/idle-v5`（若走了 rebase 则 fast-forward 语义仍保留 `--no-ff` 合并记录）。
- 合并态 `verify.cmd` **七件全绿**；`VerifySettings` 双主题双 DPI 抽查（沿 v4 第 6 查口径）留证。

### Phase 3 · 门面终稿（I2，唯一写者）
- `README.md`：并入 B/C 功能段（以 COPY-V5B/V5C 为底稿）＋更新「下载与校验」区（指向正式 Release）＋截图补拍（I5：设置窗含页脚与唤起区的新截图替换 `assets/readme/` 旧图）＋边界如实（更新触发时机/隐私口径/唤起由计划任务驱动）。
- `使用说明.txt` v6：新增「自动更新」与「自动唤起」两节（菜鸟口径：怎么检查、更新后怎么生效、怎么开/关唤起、计划任务位置、卸载时会被一并清理）＋隐私口径（仅设置窗打开时一次 HTTPS 到 api.github.com，遮罩永不联网）＋v5 变更摘要行。
- `docs/VERIFY-V5.md`：七件全绿命令与输出摘要＋两分支合并 diff 审计（逐文件列冲突裁决）＋发布证据（Phase 5 产出回填）。

### Phase 4 · 正式发布（I3）
1. 校验常量同源：`src/AppInfo.cs` 的 Version＝`1.0.0` 且 tag 将用 `v1.0.0`（登记表口径）。
2. `build.cmd` 最终产物→三资产：`OneClickBlackScreen.exe`（dist 产物重命名，裸）＋`一键黑屏-v1.0.0.zip`（exe＋同名 `.exe.config`＋`icon-settings.ico`＋使用说明.txt）＋`SHA256SUMS`（前两者，`哈希␣␣文件名` 每行一条，certutil 生成）。
3. 删演练 draft：`gh release delete v0.9.0-test --yes`（连同其未发布 tag 若有）。
4. `gh release create v1.0.0 OneClickBlackScreen.exe 一键黑屏-v1.0.0.zip SHA256SUMS --title "v1.0.0" --notes-file <发布说明>`（说明含：两项新功能、全量变更、SHA-256 教学一句）→ `git push origin main`＋`git push origin v1.0.0`。

### Phase 5 · 发布后四重验收（I4）
1. 真端点冒烟：运行设置窗→点「检查更新」→呈现「已是最新」（兼证 TLS/UA/解析全链路对真实 GitHub 成立）；证据截图入 VERIFY-V5。
2. 干净重建：临时目录 `git clone --depth 1 https://github.com/Oiawlm/OneClickBlackScreen` → `build.cmd` 成功 → `verify.cmd` 七件全绿（公开仓自足性）。
3. SHA256SUMS 自验：`certutil -hashfile` 比对 Release 上的 SHA256SUMS 逐行一致。
4. `gh release view v1.0.0 --json assets -q '.assets|length'`＝3 且 `isDraft=false`、`isPrerelease=false`。
- 全部证据回填 `docs/VERIFY-V5.md` 后提交推送。

## 4. 铁律

1. 不打补丁；合并裁决保留双方设计意图，弃任何一侧须在 VERIFY-V5 的 diff 审计中给理由。
2. §2 冻结面（RESEARCH-V5）逐字遵守；七件全绿是合入 main 的**唯一放行条件**。
3. tag 必须等于 AppInfo.Version（常量登记表）；资产名必须等于 AppInfo.UpdateAssetName。
4. 凭证不入库；`gh` 操作只用已登录会话，token 不写文件。
5. 删除仅限 `v0.9.0-test` draft 及其专属资产；不删任何分支（本地保留）、不动远端 main 历史。
6. RESEARCH-V5 有误→先登记其 §8 勘误。

## 5. 验收清单（Done when，全部可核对）

1. `git log --oneline -3 main` 显示 B、C 两个 `--no-ff` 合并点；`git status` 干净；
2. 合并态 `verify.cmd` 输出 `[OK] ALL VERIFIERS PASSED`（七件，[1/7]..[7/7]）；
3. `gh release view v1.0.0 --json isDraft,isPrerelease,assets -q '.isDraft,.isPrerelease,(.assets|length)'` 输出 `false false 3`；
4. `gh release view v0.9.0-test` 失败（draft 已删）；
5. `git ls-remote --tags origin` 含 `v1.0.0`；README「下载」节指向该 Release 且新截图已替换；
6. 使用说明.txt 含「自动更新」「自动唤起」两节与隐私口径；
7. `docs/VERIFY-V5.md` 存在：七件全绿输出＋diff 审计＋四重验收证据（含真端点「已是最新」截图与干净重建输出）；
8. `schtasks /query /tn OneClickBlackScreen-Idle` 退出码＝1（验证过程不留任务残留）。

## 6. 收尾动作

- 全部提交推送（main＋tag）；`docs/VERIFY-V5.md` 标注「轨道 I 完成」；
- 通知口径：PLAN-V5 §3 波 2 完成后由用户开 D 窗口（GOAL-V5D-PROMPT.md）；本地 feature 分支保留（D 波离线 bundle 会保全）。

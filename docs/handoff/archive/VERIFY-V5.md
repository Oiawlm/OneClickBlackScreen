# VERIFY-V5 · v1.0.0 集成与正式发布验证（轨道 I）

> **一句话定位**：轨道 I（GOAL-V5I）全部可核对证据——B/C 两分支合并回 main 的逐文件裁决审计、七件套全绿、门面终稿、Release v1.0.0 三资产、发布后四重验收。
> **上游**：[RESEARCH-V5.md](../RESEARCH-V5.md) §4 契约／§5 I1–I5　**编排**：[PLAN-V5.md](../PLAN-V5.md) §3 波 2　**日期**：2026-11-02　**执行**：GLM-5.3 目标模式
> **前置**：A/B/C 三轨交付（[PUBLISH-V5A.md](PUBLISH-V5A.md)、[VERIFY-V5B.md](VERIFY-V5B.md)、[VERIFY-V5C.md](VERIFY-V5C.md)）全部完成；两分支存在且工作树干净；`gh auth status`＝Oiawlm 已登录。

## 1. 合并（I1）

### 1.1 合并命令与拓扑

```
Phase 1  git checkout main
         git merge --no-ff feature/update-v5        → 47ebd25（零冲突，ort）
Phase 2  git rebase main feature/idle-v5           → 277872b 重放为 6eebe96（3 文件冲突，逐一裁决）
         git checkout main
         git merge --no-ff feature/idle-v5          → 6dc2530
终态     git log --oneline -3 main
         6dc2530 merge: V5C 空闲唤起合入 main（B+C 融合裁决）…
         6eebe96 feat: V5C 空闲自动唤起…
         47ebd25 merge: V5B 自动更新合入 main…
```

两个 `--no-ff` 合并点齐；`git diff main feature/idle-v5 --stat`＝0 行（main 树与已验证分支尖逐字一致）。合并后 `git status` 干净。

### 1.2 逐文件冲突裁决（diff 审计）

Phase 1（B 入 main）：**零冲突**——A 轨在 main 的写面（README/LICENSE/.gitattributes/.gitignore/assets/readme/docs）与 B 的写面（src 新文件＋Program 一行区＋SettingsDialog 页脚区＋Metrics 三常量＋verify.cmd/VerifySettings/VerifyUpdate）文件集不相交，契约（RESEARCH §4 所有权矩阵）按设计成立。

Phase 2（C rebase 于合并态）冲突 3 文件＋关键自动合并 3 文件，裁决如下（双方设计意图都保留，零丢弃）：

| 文件 | 冲突形态 | 裁决 |
|---|---|---|
| `src/Program.cs` | 自动合并 | B 的 `using System.IO`＋入口 `.old` 清理行与 C 的 `--idle` 分支＋`RunIdleCheck()` **并存**；`--settings`/无参分支逐字不动（冻结面） |
| `build.rsp` | 自动合并 | 三新源齐：`AppInfo.cs`/`Updater.cs`（B）＋`IdleWake.cs`（C） |
| `src/SettingsDialogMetrics.cs` | 自动合并 | B 页脚三常量（FooterRowHeightPx/FooterVersionZonePx/FooterButtonWidthPx）与 C 的 `IdleComboWidthPx` **并存**（C 块落按钮宽常量后、B 块落 Button-row budget 注释后，各自推导注释原样保留） |
| `src/SettingsDialog.cs` | 4 处冲突 | ①**Tab 序联合重排**：customColor=0、textBox=1、`idleCombo=2`（内容列）、`checkUpdate=3`、`applyUpdate=4`（页脚）、reset/cancel/try/save=5/6/7/8——B 原序（check=2）与 C 原序（idle=2）都让位于「视觉阅读序」这一双方共同设计意图；②字体区两块都保留（idleHintLabel 档＋页脚四控件 caption 档）；③**fixedChrome 增量相加**：`hint 区 + groupGap + labelHeight+labelGap+buttonRow+rowGap+hintHeight（唤起区）+ groupGap + hintHeight + groupGap + footerRow + groupGap（页脚）+ buttonRow`——即 B 的「hint 与按钮行之间插页脚」与 C 的「textBox 后插唤起区」两条增量同时成立，previewCap 重算自动生效；④方法区 `BeginApplyUpdate`（B）与 `SyncIdleTask`（C）两完整方法体都保留 |
| `verify.cmd` | 2 处冲突 | 统一七件编号 `[1/7]..[7/7]`：VerifyUpdate=6（B 位次）、VerifyIdle=7（C 位次，收 `%TARGET%` 参数）；编译行两段都保留 |
| `verify/VerifySettings.cs` | 1 处冲突＋自动合并 | fixedChrome 同式相加；期望字典自动合并成立＝17 原有＋唤起 3（idle label/idle combo/idle hint）＋页脚 4（version label/check/status/apply）＝**24 直系子控件**；分类器 C 的 idle 文本识别（自动唤起/无操作前缀）与 B 的「版本」前缀＋status 消除法**并存**（消除法仍唯一命中状态行——新增 static 均有文本分类）；文件头注释计数 21→24 |
| `src/MaskConfig.cs`、`verify/VerifyConfig.cs`、`verify/VerifyIdle.cs`、`src/IdleWake.cs`、docs | 无冲突 | C 独占写面（B 未触碰），B 侧无对应改动 |

弃侧记录：**零**。所有冲突均以「双方意图并存」解，无任何一侧改动被丢弃。

### 1.3 合并守门（铁律 2）

- Phase 1 合并态：`build.cmd` → `[OK] Build succeeded`；`verify.cmd` 六件 `[OK] ALL VERIFIERS PASSED`（日志 `_probe\verify-v5i-phase1.log`）。
- Phase 2 rebase 态（B+C 全量）：`build.cmd` → `[OK]`；`verify.cmd` **七件全绿**（`_probe\verify-v5i-phase2.log`）。
- 合并回 main 后复跑：`verify.cmd` 七件全绿（`_probe\verify-v5i-main.log`），输出摘要：

```
[1/7] VerifyConfig   PASS: 23 config tests (…/idle-key)
[2/7] VerifySettings PASS TOTAL: 6 settings checks (… + geometry mirror)
[3/7] VerifyMouseExit PASS TOTAL: 5/5 buttons; 30 movement messages; 10 wheel messages
[4/7] VerifyOverlay  PASS TOTAL: A-F; 1 displays; max InputIdle 124 ms (limit 2000)
[5/7] VerifyIcon     PASS TOTAL: 4 icon checks
[6/7] VerifyUpdate   PASS TOTAL: update checks (version table + offline full chain)
[7/7] VerifyIdle     PASS TOTAL: 5 idle checks (xml fields / scheduler loop / silent exit / decision table / live reading)
[OK] ALL VERIFIERS PASSED
```

- 验证结束后 `schtasks /query /tn OneClickBlackScreen-Idle` 退出码＝**1**（无任务残留）。
- 环境事件登记（如实）：Phase 1 前两轮 verify 在第 3 件失败（遮罩「开窗前即退，退出码 0」＝被真实键鼠输入关闭；第 4/5 颗按钮 PostMessage 失败＝窗口已毁）——与 PUBLISH-V5A §5.5 登记的「多窗口共享桌面竞态/用户在机」同类，第三轮全绿；代码零改动。

### 1.4 双主题双 DPI 抽查（沿 v4 第 6 查口径）

- 浅色：`_probe\v5i-light.png`（SnapV5C 真 exe PrintWindow 666×1247 @150%）；深色：`_probe\v5i-dark.png`（反射强制 ApplyTheme(false)，不触系统主题注册表）。
- 机器视觉逐项（两图均过）：「自动唤起」区块标签粗体＋下拉「关闭」双主题配色正确；两行提示原文完整；页脚「版本 1.0.0」＋「检查更新」在；按钮行四键齐、「保存」主样式；全图无重叠/截断/错位/脏块。深色「保存」字色的缩放误读沿用 VERIFY-V5B §6 登记（权威口径＝ThemePalette.AccentText＋第 6 查 Accent 采样锁定，已绿）。
- **125% 副屏本轮不可达**（DISPLAY2 已断开，仅 DISPLAY1@150%，与 VERIFY-V5B §6 同一环境事实）：几何由第 6 查 24 控件 ±1px 镜像（@144DPI 活屏复算）覆盖，S() 线性与 v4 125% 实证同源；登记为边界。

## 2. fts 增量核对（I 融合态）

- ☑ 零新颜色 token：唤起区走 palette.Surface/Text/TextSecondary/ButtonHover（C 落地），页脚走 Bg/TextSecondary/Accent/AccentText（B 落地），ThemePalette 本轮零改动。
- ☑ 档位制：唤起区行高全复用既有档（LabelRow/ButtonRow/Hint），间距零新档；页脚 24=LabelRow 档、80=cancel 同档；两新常数（IdleComboWidthPx/Footer 三常量）均带 em 推导注释，唯一登记处 SettingsDialogMetrics。
- ☑ 恒定几何：「立即更新」固定槽（OnHandleCreated 物化保留槽句柄，与网络状态解耦）；唤起区几何由第 6 查 24 控件镜像锁定——两新区并存后无任何控件矩形漂移。
- ☑ 单一数值源：fixedChrome 联合公式在 SettingsDialog.LayOutControls 与 VerifySettings 第 6 查两处同式（镜像锁定本义）；跨轨常量唯一登记处未动（版本/资产名/仓库名＝AppInfo，任务名＝IdleWake.TaskName）。
- ☑ 零动效（两新区均无动效诉求）。

## 3. 门面终稿（I2/I5）

- **README**（github-readme skill 全流程）：功能特性并入「空闲自动唤起」「应用内自动更新」两条；「绿色零依赖」条如实改写（遮罩不联网；唤起注册计划任务可关闭）；下载区三段式更新（zip 推荐＋裸 exe＋SHA256 教学，指向 v1.0.0 tag）；设置表格增「自动唤起」「页脚」两行＋「自动唤起怎么工作」段；边界节增更新触发时机/隐私口径/账户归属/卸载先关唤起；本地开发五件套→七件套；开发过程文档表纳入 VERIFY-V5。中文排版 15 条过（§四抽查）。
- **截图补拍（I5）**：`assets/readme/settings-light.png`/`settings-dark.png` 以融合态窗口重拍（666×1247 @150%，含唤起区＋页脚）；沿 PUBLISH-V5A P4 纪律——演示配置（#1F3A5F＋「屏幕休息中/按任意键退出」）＋用户 settings.ini 字节级备份恢复（`_probe\TakeReadmeShots.ps1`，全步骤 try/finally，恢复 SHA-256 ED6C6B22…56FF，无 MISMATCH）；机器视觉断言演示色入镜且**无个人通告文字/手机号**。mask-demo.png 沿用（遮罩 UI 本轮零改动）。
- **使用说明.txt v6**：新增【空闲自动唤起】【自动更新】两节（菜鸟口径：怎么开关、计划任务位置、怎么检查更新、更新后怎么生效、校验与失败处理）＋隐私口径（仅设置窗打开时一次 HTTPS 到 api.github.com，遮罩永不联网）＋v6/v5 变更摘要行＋卸载含任务清理＋开发者提示七件套/文档范围更新。

## 4. 正式发布（I3）

### 4.1 常量同源校验

`src/AppInfo.cs`：`Version = "1.0.0"`（＝tag `v1.0.0`，I3 验收项）、`RepoOwner/RepoName = Oiawlm/OneClickBlackScreen`、`UpdateAssetName = "OneClickBlackScreen.exe"`（＝裸资产名，铁律 3）。发布前 `build.cmd` 最终产物重建：`[OK] Build succeeded`。

### 4.2 三资产（_probe\release-v1.0.0，certutil 生成哈希）

| 资产 | 大小 | SHA-256 |
|---|---|---|
| `OneClickBlackScreen.exe`（dist 裸产物重命名，更新器直链名） | 265,728 B | `9A93CDD573C511DABB8CA544610E77B8E7BF315F455948FB369A3E2E72629CD1` |
| `OneClickBlackScreen-v1.0.0.zip`（label「一键黑屏 v1.0.0 完整包（exe＋config＋ico＋使用说明）」；E1 勘误后名） | 70,167 B | `00363518885CBD4EAF4C4B39919201F3306BB0DD7E4A2979C50EF4117FEF66A7` |
| `SHA256SUMS`（上两者，`哈希␣␣文件名` 每行一条） | 182 B | — |

zip 内含（下载实测回读）：`一键黑屏.exe` 265,728 B＋`一键黑屏.exe.config` 245 B＋`icon-settings.ico` 110,145 B＋`使用说明.txt` 7,855 B（内部文件名中文完好——平台限制只及资产名）。

### 4.3 E1 勘误事件（如实登记，RESEARCH-V5 §8 同源）

- 现象：zip 资产以中文名 `一键黑屏-v1.0.0.zip` 上传后，GitHub 返回资产名恒为 `-v1.0.0.zip`（中文字符被剥）。三次复现：Git Bash 直传、PowerShell BOM 脚本传、ASCII 名上传后 PATCH 改中文名（`\uXXXX` 转义 JSON 全 ASCII 信道）——同请求的 `label` 字段中文**完好存活**（对照实验实证），锁定为 **GitHub Releases 资产 name 字段服务端非 ASCII 消毒**（社区登记 [community #57748](https://github.com/orgs/community/discussions/57748)、[#209923](https://github.com/orgs/community/discussions/209923)），非本环境传输问题。
- 处置（根修非补丁）：勘误登记 E1→zip 资产定名 `OneClickBlackScreen-v1.0.0.zip`（ASCII 平台可达），中文身份保留在资产 `label`（Release 页与资产名一同显示，实测渲染「一键黑屏 v1.0.0 完整包（exe＋config＋ico＋使用说明）」）；SHA256SUMS 行名与 README 下载区/校验示例同步（main 7a820a5）；坏名资产经 API 按 ID 删除后以 ASCII 名重传，digest 全程不变（同一 zip 字节）。
- 事故中间态清理：全部经 `DELETE /releases/assets/:id`（仅删本次演练产生的坏名资产，铁律 5 范围内）。

### 4.4 发布序列（原始输出摘要）

```
gh release delete v0.9.0-test --yes            → draft 连同 2 资产删除；view 复核 "release not found"
git push origin main                            → 7639b51..dd38c19
git tag -a v1.0.0 …; git push origin v1.0.0     → * [new tag] v1.0.0（annotated，指向 dd38c19）
gh release create v1.0.0 …三资产 --notes-file   → https://github.com/Oiawlm/OneClickBlackScreen/releases/tag/v1.0.0
（E1 后）资产 PATCH 改名＋SHA256SUMS 重传        → 终态 false/false/3
```

发布说明（`_probe\release-notes-v1.0.0.md`）：两项新功能、全量变更、SHA-256 教学一句，齐。

## 5. 发布后四重验收（I4）

1. **真端点冒烟（LatestSmoke，v1.0.0 已发布后）**：真 dist exe 打开设置窗→静默检查无声（状态行空断言）→BM_CLICK「检查更新」→`[200ms] status='已是最新版本。'`；「立即更新」保留槽物化但**不可见**（等版本不揭示）；窗口稳定 WM_CLOSE 退出码 0；截图 `_probe\v5i-latest-smoke.png`。兼证 TLS1.2/自定义 UA/releases/latest 解析/同版比较全链路对真实 GitHub 成立（B 轨留 I 的缺口补齐）。
2. **干净重建（公开仓自足性）**：`git clone --depth 1 https://github.com/Oiawlm/OneClickBlackScreen`（临时目录 `D:\Download\Agent Vault\v5i-cleanroom`，HEAD=dd38c19）→ `build.cmd` `[OK] Build succeeded` → `verify.cmd` **七件全绿**（日志 `_probe-clean-verify.log` 于该目录：[1/7]..[7/7] 全 PASS TOTAL＋`[OK] ALL VERIFIERS PASSED`）。
3. **SHA256SUMS 自验**：`gh release download v1.0.0` 全量重下→逐行 `certutil -hashfile` 比对（`_probe\SumsCheck.ps1`）：`MATCH OneClickBlackScreen.exe 9A93CDD5…`＋`MATCH OneClickBlackScreen-v1.0.0.zip 00363518…`＋`SUMS SELF-CHECK: all lines match`。
4. **Release 状态**：`gh release view v1.0.0 --json isDraft,isPrerelease,assets -q '.isDraft,.isPrerelease,(.assets|length)'` → **`false false 3`**（资产名/label 见 §4.2）；`gh release view v0.9.0-test` → release not found（draft 已删）。

## 6. 验收清单对照（GOAL §5 逐项，全部实核）

| # | 验收项 | 结果 | 证据 |
|---|---|---|---|
| 1 | main 两个 `--no-ff` 合并点＋status 干净 | ✅ | `6dc2530`（C 合并）＋`47ebd25`（B 合并）；`git status --short`＝0 行 |
| 2 | 合并态 `verify.cmd` 七件 `[OK] ALL VERIFIERS PASSED` | ✅ | §1.3（`_probe\verify-v5i-main.log`，[1/7]..[7/7]） |
| 3 | release view → `false false 3` | ✅ | §5.4 |
| 4 | `gh release view v0.9.0-test` 失败 | ✅ | §4.4（"release not found"） |
| 5 | 远端含 `v1.0.0`；README 下载节指向该 Release＋新截图已替换 | ✅ | `git ls-remote --tags origin` 含 `v1.0.0`（annotated＋peel 两行）；README 下载区含 v1.0.0 直链与三资产名；settings-light/dark.png 已为融合态新图（dd38c19） |
| 6 | 使用说明.txt 含「自动更新」「自动唤起」两节＋隐私口径 | ✅ | 两节齐（10 处关键词命中）；隐私口径行 82–84（api.github.com/不收集/遮罩永不联网） |
| 7 | docs/VERIFY-V5.md 存在：七件全绿＋diff 审计＋四重验收证据 | ✅ | 本文件 §1–§5 |
| 8 | `schtasks /query /tn OneClickBlackScreen-Idle` 退出码 1 | ✅ | §5 全部验收后实测 exit=1 |

**收尾**：全部提交已推送（main→`7a820a5`，tag `v1.0.0`→dd38c19）；本地 feature 分支保留未删（D 波离线 bundle 保全）；RESEARCH-V5 §8 已登记 E1 勘误。

---

**轨道 I 完成**（PLAN-V5 §3 波 2）：v1.0.0 已正式发布并通过四重验收；后续衔接＝用户开 D 窗口（GOAL-V5D-PROMPT.md）。

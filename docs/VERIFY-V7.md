# VERIFY-V7 · v7 集成、v1.0.2 发布与移交闭环总证据（轨道 I）

> **一句话定位**：轨道 I（GOAL-V7I）全部可核对证据——R 轨移交注记 3 条处置（无 no-go，R-1 护栏知识传承置顶）、合并 762dffb（无冲突）、全绿 ×2＋联动 ×1、版本单源 1.0.2（含 VerifyUpdate 表联动与 grep 裁决表）、Release v1.0.2 三资产与双向 digest、「已是最新」真端点冒烟（零足迹）、install.ps1 落位、migrate/ 只读完整性。S/V 细证见 [VERIFY-V7S.md](VERIFY-V7S.md)/[VERIFY-V7V.md](VERIFY-V7V.md)；复检见 [REVIEW-V7.md](REVIEW-V7.md)。
> **日期**：2026-11-05　分支 main（合并自 `feature/verify-isolation-v7`，发布提交 04217f8）。

## 0. R-1 护栏知识传承（置顶）

**USERPROFILE 重定向的生效面＝进程创建时注入的环境块**：`Environment.GetFolderPath` 经注册表模板 `%USERPROFILE%\AppData\Local` 展开＋存在性校验，**进程内运行时** `Environment.SetEnvironmentVariable("USERPROFILE",…)` 对其**不生效**（R 轨 §2.2 探针双跑实证）。正确接缝＝`ProcessStartInfo.EnvironmentVariables`（子进程注入）＋预建 `AppData\Local`/`AppData\Roaming` 骨架——本轮全部沿用点：VerifyOverlay/VerifySettings/VerifyIdle（V 轨）与本轮 LatestSmoke 冒烟工具。**未来任何人改走进程内运行时修改会得到安慰剂**。

## 1. Phase 0 · 移交注记处置表（REVIEW-V7 §5；均不修码，无 no-go）

| # | 现象 | 处置 | 依据 |
|---|---|---|---|
| R-1 | VERIFY-V7V §0「展开于进程环境」欠精确（生效面＝进程创建时环境块） | **不修码＋传承**：护栏知识落本文 §0 置顶 | 现实现三件全走子进程注入，无实际缺陷；R 轨原建议「I 波知悉/在 VERIFY-V7 传承，无须回改冻结过程记录」采纳；改过程记录违 AGENTS §4 冻结区 |
| R-2 | verify.cmd `if errorlevel 1/2` 对负退出码（CLR 崩溃 -532462766）均判 false，崩溃轮可能漏过失败出口 | **不处置＋备案** | V 轨前即有的既有语义（历史各轮一致）；各验证器 Main 均 try/catch→return 1 兜底，实际暴露面小；V 轨铁律「既有语义零变化」正确未动——发布前改批处理判跳逻辑＝叠护栏，违 GOAL §4.1。修法（如 `%errorlevel%` 数值比较）留未来轮辖内 |
| R-3 | VerifySha256 单源化后「下载文件不可读」极端子情形异常类型 IOException→InvalidOperationException | **不处置＋备案** | 拒装终态与时序不变、U1 可观测主路径逐点保持（R 轨复推＋既有断言修复前后均 PASS）；触发条件＝DownloadTo 成功后文件立即缺失/锁死，复现成本高价值低 |
| 附 | VERIFY-V7V §8 两条（MaskConfig 产品级接缝／VerifyMouseExit 纯读） | 知悉、无动作 | R 轨未将其列为 I 波移交注记（属「供规划裁决」的未来产品方向）；本轮零足迹目标已由 USERPROFILE 接缝达成 |

判定：无一构成回退或发布阻断 → 按 GOAL Phase 1 继续。

## 2. Phase 1 · 合并与全绿（合并提交 762dffb）

- `git checkout main`（65f8797，V7 规划尖）→ `git merge --no-ff feature/verify-isolation-v7`＝**ort 合并，无冲突**（16 文件 +699/−108；写面不相交契约兑现）。**--no-ff 裁决**：main 为分支直接祖先，默认 merge 将 fast-forward 而无合并提交，GOAL §5.1 验收明文要求 origin/main「含合并提交」→ 显式 --no-ff 留合并提交。
- `build.cmd` → `[OK] Build succeeded: D:\...\dist\一键黑屏.exe`，exit 0。
- `verify.cmd` **连跑 2 次**（跑前 tasklist 确认无验证器/被测进程，AGENTS §2 会话独占）：

| 轮 | 末行 | 退出码 | PASS 行数 | PASS Z（零足迹绊线） |
|---|---|---|---|---|
| 1 | `[OK] ALL VERIFIERS PASSED` | `RUN1_EXIT=0` | 56 | VerifySettings＋VerifyOverlay 各 1 条 |
| 2 | `[OK] ALL VERIFIERS PASSED` | `RUN2_EXIT=0` | 56 | 同上（`PASS Z: user's real settings.ini untouched (SHA-256 before == after)`） |

末件实录（两轮同形）：`PASS TOTAL: 5 idle checks (xml fields / scheduler loop / silent exit / decision table / live reading)`；[6/7] 含 `PASS: concurrent replaces serialize - one effective swap, second flow no-op, backup intact (x50)`（S 轨成果在合并后 main 上复绿）。

## 3. Phase 2 · 版本单源 1.0.2（发布提交 04217f8）

- `src/AppInfo.cs:11`＝`public const string Version = "1.0.2";`（全仓唯一手写版本常量）。
- **VerifyUpdate 版本表联动**（V6 纪律）：镜像断言 `{1,0,1}`→`{1,0,2}`（`VerifyUpdate.cs:66-67`）；`v1.0.2 must read newer`→`v1.0.3`；新增 `v1.0.1 must read older`（旧版本读到更新提示对象，`:74`）；等值/裸/内空格/大写 V 示例与 mock 等值 tag（`v1.0.2`/`1.0.2`/`v 1.0.2`/`V1.0.2`/`v1.0.2-rc1`）随单源走 1.0.2；成功链 v9.9.9 不变。
- 联动复核：`build.cmd` 绿＋`verify.cmd` 全绿 ×1（`RUN3_EXIT=0`，末行 `[OK] ALL VERIFIERS PASSED`，[6/7] 首行 `PASS: version comparison table (newer/equal/older/malformed, fail-closed)`）。
- **grep 实录**（铁律 2）：`grep -rn "1\.0\.2" --include=*.cs` 恰 `src\AppInfo.cs:11`＋`verify\VerifyUpdate.cs` 7 处镜像断言；`grep -rn "1\.0\.1" --include=*.cs src/ verify/` 恰 1 处＝`v1.0.1 must read older` 断言。
- **全仓残留裁决表**（`git grep -ln "1\.0\.2"`）：AppInfo/VerifyUpdate＝单源＋镜像（本节）；README×3/使用说明×2＝本轮门面同步（§4）；RESEARCH-V7/PLAN-V7/GOAL-V7I/REVIEW-V7/VERIFY-V6＝规划与过程文档，彼时/届时事实，冻结不动。

## 4. Phase 3 · 门面同步（同提交 04217f8）

- **README.md**：下载段 `:40` tag 链接 v1.0.2、`:41` zip 名与显示名「一键黑屏 v1.0.2 完整包」、`:46` certutil 示例行；`:112` 本地开发段补「验证会注入全局键鼠——运行期间请勿操作键盘鼠标，多个验证不可并行（套件内置闸门，撞闸 BUSY）」；`:126` 开发过程文档表新增 `AGENTS.md` 行（多窗口协作契约）。
- **使用说明.txt**：`:7-8` 新条目「v7 更新（对应程序版本 1.0.2）：修复同时打开两个设置窗口、先后点击『立即更新』时可能出现的更新失败（并发更新会自动排队，正常使用不受影响）」；`:38` 页脚「当前版本号（1.0.2）」。

## 5. Phase 4 · 发布链（沿用 V5I/V6I 工艺）

**tag 与推送**：`git tag -a v1.0.2`（485da9d→04217f8，tag==AppInfo 一致）→ `git push origin main --follow-tags`＝`bab8336..04217f8 main -> main`＋`* [new tag] v1.0.2 -> v1.0.2`（顺带补推 V7 规划提交 65f8797——规划窗只提交未推送，本轮推送一并到达远端）。

**三资产**（staging `%TEMP%\v7i-release\`，PowerShell Compress-Archive 全程）：

| 资产 | 大小 | SHA-256 |
|---|---|---|
| `OneClickBlackScreen-v1.0.2.zip`（label「一键黑屏 v1.0.2 完整包（exe＋config＋ico＋使用说明）」） | 71,151 B | `bc64e4eb9b222f8675013676a9f7d7c0de2bf4562dd9093b037be70529b24169` |
| `OneClickBlackScreen.exe`（裸，＝AppInfo.UpdateAssetName） | 266,752 B | `b2b440b1364e957c2391a9d10e615d5ac67482490cd4cb8d783cf7bfb74bf126` |
| `SHA256SUMS`（上两者两行） | 187 B | — |

- zip 回读：`一键黑屏.exe` 266,752 B＋`.exe.config` 245 B＋`icon-settings.ico` 110,145 B＋`使用说明.txt` 8,367 B，中文名完好。
- `gh release view v1.0.2 --json isDraft,isPrerelease,assets`＝**`false false`，3 资产**，zip label 中文完好。
- **双向 digest**（`gh release download` 全量重下→certutil 复算）：`MATCH OneClickBlackScreen.exe`（local==remote `b2b440b1…`）＋`MATCH OneClickBlackScreen-v1.0.2.zip`（local==remote `bc64e4eb…`）＋`SUMS CHECK: 2 of 2 lines match`，脚本退出码 0。
- **「已是最新」真端点冒烟**（`%TEMP%\v7i-smoke\LatestSmoke.exe`，USERPROFILE 接缝零足迹，PostMessage BM_CLICK）：

```text
WINDOW_TITLE=一键黑屏 · 设置
FOOTER=版本 1.0.2                    ← 单源运行时实证
APPLY_BUTTON_VISIBLE=false           ← v1.0.2 已为 latest，无「立即更新」
BUTTON_READY_AFTER=1732ms            ← 打开时静默检查完成后按钮恢复使能
[1985ms] status='已是最新版本。'      ← api.github.com/releases/latest 真实应答
EXIT_CODE=0 · REAL_INI_BEFORE=absent · REAL_INI_AFTER=absent
SMOKE PASS: real endpoint answered 已是最新 for v1.0.2, zero footprint
```

- **install.ps1**：`[OK] 一键黑屏.lnk -> …dist\一键黑屏.exe`＋`[OK] 一键黑屏-设置.lnk -> … --settings  Icon='…icon-settings.ico,0'`＋完成行，exit 0。

## 6. Phase 5 · 完整性与终态

- **零足迹前后三态一致**（套件 ×3＋冒烟＋演练全程跑完后）：`REAL_INI_ABSENT · REAL_TASK_ABSENT · VERIFY_TASK_ABSENT · NO_RESIDUAL_PROCESS`；`_probe/` 不存在。
- **migrate/ 只读完整性**：`git status --short -- migrate/ SHA256SUMS.txt` 零输出（工作树零改动）；`git log -1 -- migrate/`＝3534dd0（V5D 时点，本轮未触碰）；`migrate/SHA256SUMS.txt` 自 digest＝`95b3899388a1c179fdb24e1a630a32ee7fa29d6f3c62c813fc5e771554e9076d`；三条本体（bundle/skills/settings）本机从未存在（MIGRATE-V5D A5 口径），以 git 侧不可变性为证（V6 同例）。
- **事实核对**（GOAL §6）：本轮未发现 RESEARCH-V7/PLAN-V7/AGENTS.md 事实错误；RESEARCH §7-2「完整发布链沿用 V6I」与实际执行一致。
- 推送终态见 §5；本文档提交后 `git status` 干净。

## 7. 事故与修正注记（如实）

1. **digest-check.ps1 首版两处脚本缺陷**：①正则尾部多余 `}` 致哈希行零匹配→空数组索引异常；②PowerShell 单匹配返回标量，`$out[0]` 取到字符。修脚本后全 MATCH、SUMS 2/2——资产本体全程无误（与 V6「SumsCheck 假 MISMATCH」同类教训：校验工具缺陷≠资产缺陷）。
2. **LatestSmoke 首版红（工具缺陷，产品无缺陷）**：BM_CLICK 落在打开时静默检查的按钮禁用窗内（`BeginCheck` 入口 `checkUpdateButton.Enabled=false`＝S 轨防并发重入设计内行为），禁用按钮对 BM_CLICK 不响应且不排队。修正＝点击前轮询 `IsWindowEnabled`（1732 ms 后就绪）＋状态轮询补「正在检查更新…」中间态捕捉；复跑全绿。
3. **Windows 编码三变体**（V6「Git Bash 路径转码伤中文名」教训的新例，全在临时目录工具、未触仓库）：①无 BOM .ps1 的中文字面量被 PS5.1 按 ANSI 误读→改 pattern 定位文件，脚本零非 ASCII 字面量；②csc 编译含中文串源需 UTF-8 BOM（写后 PowerShell 转换）；③bash 双引号吞 PowerShell `$` 变量→单引号传参。

## 8. 验收对照（GOAL-V7I §5 八项）

1✅ origin/main＝04217f8 含合并提交 762dffb＋S/V/R 全史（§2/§5）；`git ls-remote` 见 `refs/tags/v1.0.2`（§5）。2✅ 合并后 ×2＋联动后 ×1 全绿实录（§2/§3）。3✅ AppInfo=1.0.2＋grep 实录＋残留裁决表（§3）。4✅ Release 三资产存在、双向 certutil 一致（§5）。5✅ 真端点冒烟输出＋install 两 [OK]（§5）。6✅ README/使用说明引用处实录（§4）。7✅ 移交注记处置表（§1，含 R-1 传承 §0）。8✅ migrate/ 只读校验实录（§6）＋git status 干净（§6）。

## 9. 收尾

本轮 V7 全链（S→V→R→I）完成：**本轮无用户手动项**（迁移阶段 8/9 历史遗留与 V7 无关，仍按 MIGRATE-V6-PC2 口径由用户自行安排）；下一轮如需并行窗口，按 AGENTS.md §2（独立 worktree＋验证闸门）执行。

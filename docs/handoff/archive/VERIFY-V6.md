# VERIFY-V6 · v6 集成、v1.0.1 发布与 PC2 收尾总证据（轨道 I）

> **一句话定位**：轨道 I（GOAL-V6I）全部可核对证据——A 轨移交注记修补、A+B 合并、干净键态七件全绿×2、版本单源 1.0.1（含 VerifyUpdate 表联动）、Release v1.0.1 三资产与双向 digest 验证、「已是最新」真端点冒烟、install.ps1 落位、程序化冒烟、migrate/ 只读完整性。轨道 A/B 细证见 [VERIFY-V6A.md](VERIFY-V6A.md)/[VERIFY-V6B.md](VERIFY-V6B.md)；PC2 阶段 8 对照见 [MIGRATE-V6-PC2.md](MIGRATE-V6-PC2.md)。
> **日期**：2026-11-03　分支 main（自 5f4f058）。

## 1. Phase 0 · A 轨移交注记修补（VERIFY-V6B §5 实据）

- 实据核对：`verify/VerifyOverlay.cs` 场景 E 的 `keybd_event(Shift down)` 确在 `using (Process process = Launch())` 之前，Shift up 仅存在于 using 体 finally——`Launch()` 抛出则 up 永不执行＝VK_SHIFT 全局毒化（G/H 两场景已是外层 try/finally 正确模式，E 是漏网者）。
- 修补（提交 `5d8d2bb`，feature/exit-arm-v6，已推送）：E 段整收编 `try { using(Launch()){…} } finally { Shift up }`，与 G/H 同款配对；语义零变化（先按住再启动不变）。
- 修补后单跑：`verify\bin\VerifyOverlay.exe "dist\一键黑屏.exe"` → `PASS A … PASS H … PASS TOTAL: A-H; 1 displays; max InputIdle 234 ms (limit 2000)`，EXIT=0；跑后键态扫描 `scan: ALL KEYS UP (clean state)`。
- **环境事故实录（如实）**：本次单跑首试 FAIL `Keyboard key 0x87 failed to close overlay`——键态扫描见 `DOWN = 1B`：本机外部软件的幻影 ESC 再次出现（正是 RESEARCH-V6 §2 根因一环境，先于本轮任何注入）。以合成 up 清除（`cleared VK 0x1B`→`ALL KEYS UP`）后复跑即全绿——侧面实证新构建在该环境下不再致命（修复前该态＝SPACE 不退出）。

## 2. Phase 1 · 合并与全绿门

- `git checkout main`（5f4f058，干净）→ `git merge origin/feature/exit-arm-v6`＝**Fast-forward** 至 5d8d2bb（A 轨 2 提交：2f5f1b5 根修＋5d8d2bb 卫生）→ `git merge origin/feature/verify-env-v6`＝**ort 合并**（71dedde，无冲突；写面不相交契约兑现）。
- `build.cmd` → `[OK] Build succeeded: D:\...\dist\一键黑屏.exe`。
- 干净键态 `verify.cmd` **连跑 2 次**（每次前置扫描 `ALL KEYS UP`）：

| 轮 | 末行 | 退出码 | 七件逐项 |
|---|---|---|---|
| 1 | `[OK] ALL VERIFIERS PASSED` | 0（[OK] 行只可达于七件全过） | [1]23 config [2]6 settings [3]5/5 buttons+phantom click [4]A-H [5]4 icon [6]update [7]5 idle，全 PASS TOTAL |
| 2 | `[OK] ALL VERIFIERS PASSED` | `VERIFY_EXIT=0`（cmd 实录） | 同上（InputIdle 138/227 ms） |

## 3. Phase 2 · 版本单源 1.0.1（提交 35d7616）

- `src/AppInfo.cs:11`＝`Version = "1.0.1"`（全仓唯一手写版本常量）。
- **VerifyUpdate 版本表联动**（GOAL Phase 2 明文）：镜像断言 `{1,0,0}`→`{1,0,1}`；`v1.0.1 must read newer`→`v1.0.2`；等值用例随单源走 v1.0.1 并新增 `v1.0.0 must read older`（升版语义入表）；离线全链成功态仍用 v9.9.9（与任何 1.0.x 无耦合）。
- 重建＋第 3 次全量：`VERIFY_EXIT=0`，七件全绿（[6/7] VerifyUpdate `PASS TOTAL: update checks (version table + offline full chain)`＝表与单源联动确认）。
- **README**：下载段 v1.0.0→v1.0.1（tag 链接/zip 名/显示名/certutil 示例行），其余零重写。
- **使用说明.txt**：新增 `v6 更新（对应程序版本 1.0.1）` 修复条目（幻影按键≈1 秒自动解除阻断）；页脚行 `（1.0.0）`→`（1.0.1）`；1.0.0 历史条目按 changelog 语义保留。
- **`git grep -n "1\.0\.0"` 裁决表**（改后残留全量分类）：

| 残留处 | 裁决 |
|---|---|
| GOAL-V5*/V6*-PROMPT、PLAN-V5、RESEARCH-V5/V6 | 历史规划/过程文档，不动 |
| docs/（MIGRATE-V5D、PUBLISH-V5A、VERIFY-V5、VERIFY-V5B、VERIFY-V6A） | 历史证据文档（其中 v1.0.0 均为彼时事实），不动 |
| baseline/（manifest `1.0.0.0`、exe 二进制） | v1 冻结基线，铁律禁改 |
| migrate/（迁移手册 md/html 事实基准行） | migrate/** 只读铁律；且为迁移时点事实 |
| 使用说明.txt `对应程序版本 1.0.0` 行 | changelog 历史条目（1.0.0 功能引入时点），其上已有 1.0.1 新条目，产品口径以最新条目为准 |
| src/SettingsDialogMetrics.cs:55 注释 | 像宽估算注释引用「版本 1.0.0」示例文案（5 字符长度与 1.0.1 等长，估算仍准）；非版本常量，冻结文件不动 |

## 4. Phase 3 · Release v1.0.1（沿用 V5I 工艺，E1 口径）

三资产（staging `%TEMP%\v6i-release\`，PowerShell Compress-Archive 打包——Git Bash 路径转码会伤中文名，打包全程 PowerShell）：

| 资产 | 大小 | SHA-256（certutil 复算逐字一致） |
|---|---|---|
| `OneClickBlackScreen-v1.0.1.zip`（label「一键黑屏 v1.0.1 完整包（exe＋config＋ico＋使用说明）」） | 70,692 B | `D3E28AE018DAF9FA51EECA74132C4EBB9C1AFCAE818E18EEE883AEB20494A2A1` |
| `OneClickBlackScreen.exe`（裸，＝AppInfo.UpdateAssetName） | 266,240 B | `2FAC21B9F87A629EA6ED3EF81285AF95CD89ADD10BF4B23EC4CAF24FD1EE9D2B` |
| `SHA256SUMS`（上两者，哈希␣␣文件名两行，187 B） | — | — |

- zip 内含（ZipArchive 回读）：`一键黑屏.exe` 266,240 B＋`一键黑屏.exe.config` 245 B＋`icon-settings.ico` 110,145 B＋`使用说明.txt` 8,159 B，中文名完好。
- 发布序列实录：`git tag -a v1.0.1`（→35d7616，tag==AppInfo 一致）→ `git push origin main --follow-tags`（`5f4f058..35d7616`＋`* [new tag] v1.0.1`）→ `gh release create v1.0.1 <exe> <zip#label> <SHA256SUMS> --notes-file …` → release URL 返回。
- Release 状态：`gh release view v1.0.1 --json isDraft,isPrerelease,assets` → **`false false 3`**，zip label 中文完好渲染。
- **发布后下载自验**：`gh release download v1.0.1` 全量重下→certutil 逐行比对：`MATCH OneClickBlackScreen.exe 2fac21b9…`＋`MATCH OneClickBlackScreen-v1.0.1.zip d3e28ae0…`＋`SUMS CHECK: 2 of 2 lines match`（首跑曾现假 MISMATCH——校验脚本循环变量跨迭代残留复用旧哈希，修脚本后全 MATCH；资产本体无问题）。
- **真端点冒烟**（发布成功后，LatestSmoke 临时工具 `%TEMP%\v6i-smoke\`）：真 dist exe `--settings` 开窗→页脚 `版本 1.0.1`（单源运行时实证）→静默检查无声（状态行空）→BM_CLICK「检查更新」→**`[420ms] status='已是最新版本。'`**→「立即更新」槽在版式内但不可见→稳定退出码 0。兼证 TLS1.2/UA/releases/latest/同版比较对真实 GitHub 成立。

## 5. Phase 4 · PC2 收尾（细节对照见 MIGRATE-V6-PC2.md）

- **install.ps1**：两行 `[OK] 一键黑屏.lnk / 一键黑屏-设置.lnk -> …dist\一键黑屏.exe [--settings]`＋完成行，exit 0；WScript 回读目标/参数/图标全对。
- **遮罩冒烟**（探针 RESEARCH §8 配方）：`healthy EXITED after SPACE (exit code 0)`；收尾键态 `ALL KEYS UP`。
- **设置窗冒烟**（SettingsSmoke，阶段 8 第 5 项程序化镜像）：改遮罩文字 V6pre→V6ok→点「保存」→「已保存」确认框→`settings.ini` 落盘 `color=#123456 / text=V6ok`（字节精确）→关框退出码 0，**PASS**；用户配置快照/恢复（本轮机内无既有配置，详见 §7 事故注记）。
- **migrate/ 只读完整性**：工作树零改动；`git log -1 -- migrate/`＝3534dd0（V5D 时点，本轮未触碰）；SHA256SUMS.txt 三行 digest 与 V5D 登记逐字一致；三条本体（bundle/skills/settings 备份）PC2 走 GitHub 克隆路径从未存在（MIGRATE-V5D A5），故无本体可复算，以 git 侧不可变性为证（MIGRATE-V6-PC2 §5）。

## 6. 阶段 8 第 1/2 项补档实录

- §6.1 `git fsck`：**零输出**，exit 0；`git status`：`On branch main / nothing to commit, working tree clean`。
- §6.2 install.ps1 输出（§5 首条）。§6.3 SettingsSmoke 输出（§5 第三条）。

## 7. 事故与处置注记（如实）

1. **幻影 ESC 复现**（§1）：本机外部软件幻影 ESC 于 Phase 0 单跑前再次出现→首试红在 0x87→合成 up 清除后全绿。机器环境仍会自发制造该态，恰证 v1.0.1 修复的必要性。
2. **SettingsSmoke 首版自锁**：以 `SendMessage(BM_CLICK)` 同步点击「保存」→应用处理器弹模态确认框并在 SendMessage 内派发→工具阻塞在等待框关闭而关闭代码在其后＝自死锁；强杀工具致其 finally 配置恢复未执行。**核查**：`settings.ini` CreationTime=工具写入时点（15:55:58）→该文件本轮前**不存在**（NTFS 保留创建时间，覆写不重置）→无用户数据受损；删除测试态文件即恢复「无配置」原状。工具改 `PostMessage`（VerifySettings 同款）＋关框即退出预期（CloseWindow 先例）后全绿。
3. **SumsCheck 假 MISMATCH**（§4）：脚本变量残留缺陷，资产与哈希全程无误。
4. 键态卫生：全部注入工具跑后扫 `1..254` 均 `ALL KEYS UP`。

## 8. 推送终态

main＝35d7616＋本文档提交；`git ls-remote --tags origin` 含 `v1.0.1`（annotated aae48f8→35d7616）；`gh release view v1.0.1`＝`false false 3`。验收七项对照 GOAL-V6I §5：1✅(§8) 2✅(§2) 3✅(§4) 4✅(§3) 5✅(§5) 6✅(MIGRATE-V6-PC2) 7✅(§4)。

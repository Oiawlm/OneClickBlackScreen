# REVIEW-V11 · 复检预集成总证据（R 轨）

> **一句话定位**：R 轨复检留档——A∥B 双 `--no-ff` 零冲突预集成、RESEARCH §7 契约八行双侧取证（含裁决 D-R1：B 侧形状守卫补原始 `..` 拒绝恢复逐字同构）、合并态 build/build-setup/verify 全链绿、README/使用说明 单形态大改写实录。　**事实源＝[RESEARCH-V11.md](RESEARCH-V11.md)**。　**日期**：2026-11-12（R 窗权限挂起 8h 后由规划窗接管执行，过程见 §6 注记）。　行数预算：≤95 行。

## 1. P1 预集成

- worktree `v11-r`（分支 `feature/review-v11` 自 origin/main@c873d04）。
- `merge --no-ff origin/feature/app-v11`（e48056b，16 件 +775/−825）→ `merge --no-ff origin/feature/installer-v11`（643f3d4，3 件 +264/−38）——**零冲突**（写面零相交的构造性实证）。

## 2. P2 契约核对表（RESEARCH §7 八行；file:line＝合并树 HEAD）

| # | 契约 | A 侧证据 | B 侧证据 | 判定 |
|---|---|---|---|---|
| 1 | Setup 资产名前缀 | `src/AppInfo.cs:14` 常量；`src/Updater.cs:103` 引用 | `setup/OneClickBlackScreen.iss:94` OutputBaseFilename 同式；`build-setup.cmd:31/37/62` 提取＋互锁 | ✅ 四方同源 |
| 2 | `SHA256SUMS.txt` | `src/AppInfo.cs:15`；`src/Updater.cs:77` 匹配 | （I 轨 Release 生成；VerifyUpdate 断言 `<hex> *<name>`） | ✅ |
| 3 | 静默参数 | `src/Updater.cs:194` ＝冻结值逐字 | —（不涉） | ✅ |
| 4 | dist 恰四件 | `build.cmd` 双目标＋断言（实测 dist＝2 exe＋2 config，icon-settings.ico 缺席） | `.iss:167-171` [Files] 恰五条（四件＋使用说明） | ✅ |
| 5 | 设置 EXE 名/直指 | build 产物名 | `.iss:48` 常量；`.iss:211` `{group}\一键黑屏-设置` → `{app}\{#AppSettingsExeName}`（无参数/无 IconFilename） | ✅ |
| 6 | 五值＋空键清扫 | `src/AppPaths.cs` 常量（CommitDataRoot 对称双写/双删，VerifyPaths 断言） | `.iss:239-240` 常量；`:683-684` usUninstall 删两值；`:693` RegDeleteKeyIfEmpty | ✅ |
| 7 | 形状判定同构（含 `..` 拒绝） | `src/AppPaths.cs:108` 原始串 `IndexOf("..")` | `.iss:376-407`（**见 D-R1：原始串拒绝本轮补齐**） | ✅（D-R1 后） |
| 8 | `*.old` 过渡回收 | src 侧 DeleteStaleBackup 已删（grep 零命中） | `.iss:177-178` `[InstallDelete] {app}\*.old` | ✅ |

## 3. P3 合并态全链（真实输出留档于本提交描述与会话）

- `build.cmd` → `[OK] dist\一键黑屏.exe + 一键黑屏-设置.exe + 2 configs`（恰四件）。
- `build-setup.cmd` → `[OK] Setup built: dist\OneClickBlackScreen-Setup-1.2.0.exe`（版本未 bump 属预期；前缀提取互锁通过——B 移交注记 3 的首验项兑现）。
- `verify.cmd` → **[OK] ALL VERIFIERS PASSED，EXIT=0**（八件：Config/Settings/MouseExit/Overlay/Icon/Update/Idle/Paths；Icon 双 exe 七档、Update 单形态链、Paths 形状矩阵＋真实键三态 tripwire）。

## 4. 裁决 D-R1（沿 V10 b80df60 双侧修复＋V9 D-1 白名单外小改先例）

- **发现**：B 侧 `IsManagedDataDirectory` 以「归一化＋父根＋重建回环」判形（V10 原文未触碰，其移交注记 1 自报）；遍历型 `..`（`C:\x\..\BlackScreen`）被父根检查拒绝 ✓，但**组件名字面含 `..`**（`C:\y..\BlackScreen`）经回环判定可被收下；A 侧 D6 原始串扫描则拒绝——D6 明文「不展开、不归一化救回」，两侧不同构。
- **裁决**：契约本义＝原始串扫描（fail-closed）。B 侧 `.iss:391` 后补 `if Pos('..', Raw) > 0 then Exit;`（五值安全语义只紧不松：向导对病态 `..` 目录名的登记同样拒绝，与 A 侧回退默认行为一致）。
- **证明**：生产变体（build-setup.cmd 绿）＋测试变体（`ISCC /DTestId /DAppVersion /DDefaultDataRoot` 绿，产物 `-TEST.exe`）双编译通过；verify 不消费 .iss，§3 全绿结论不受影响。

## 5. P4/P5 行为文档改写实录

- **README.md**（CRLF 保真）：标语「一个安装包」；下载节单行 Setup-1.3.0＋`SHA256SUMS.txt` certutil 例；**新增安装位置段＝`%LOCALAPPDATA%\Programs\一键黑屏`**（地址栏直达＋设置>应用可查）；启动表改「两个程序，各管一事」双 EXE 零参数；页脚更新口径＝完整安装包静默覆盖＋自动重启；卸载段含「更新检查状态一并清除」；开发节 verify 八件套＋build-setup 四件＋产物名互锁；过程文档表指向 V11 当前批次＋archive 汇总行（README:131 两轮过期指针终结）；1.2.0 便携迁移指引句。
- **使用说明.txt**（UTF-8 BOM＋CRLF 保真，`file` 复验留档）：标题 v10；新增「v10 更新（对应程序版本 1.3.0）」行；【安装】删便携行＋写明确切默认路径＋迁移指引；【两个程序，各管一事】双 EXE；自动更新删「便携版直接替换程序文件」；边界节卸载口径含更新状态；`--settings` 兼容注记保留一处。
- **验收 grep**：`便携|zip|绿色|单文件` 全仓恰 3 处＝申报豁免（README:59 迁移指引／使用说明:7 历史行「便携版退役」／使用说明:29 迁移指引）；`LOCALAPPDATA.*Programs` 两文件各 1 命中；README `1\.3\.0`×4；`1\.2\.0` 仅使用说明:9 历史更新行。

## 6. 移交注记

- **→I 轨**：①S1 用户实装原地升级断言点＝双 exe 落位/ARP=1.3.0（对照 V10 S3 反例）/数据根哈希不变/[Run] 重启；②旧 lnk 带 `--settings` 兼容联测（B 移交注记 5-②）；③`*.old` 若实装机存在，装 1.3.0 后消失（表 8 真机版）；④D-R7 真机＝隔离目录自装自卸勿动用户实装；⑤Release 恰两资产＋SUMS `<64hex>␣*<文件名>`＋三方 digest；⑥清理 worktree×3＋分支×3（本地/远端 ancestor 证明）。
- **→V12/lessons 盘点**（B 移交注记 6 转录）：MSYS 改写原生 exe 开关（`MSYS_NO_PATHCONV=1`）、git bash GBK 通道中文文件名、gawk 剥 CRLF——三条工具坑候选入 lessons；A 轨 VerifySettings 隔离泄漏事故（已复原零丢失）已在其 VERIFY §事故留档。
- **过程注记**：R 窗（sess_5f8dbeee）因权限审批挂起 8h+，由规划窗接管执行本 GOAL（接管前 InterruptSession 防双写）；A/B 两窗为真实 GLM-5.3 执行窗跑通（e1a620f/2fb6e4c）。**单写者全程未破坏**（B 窗 idle、R 窗已中断）。

## 7. 快速启动命令（I 轨照抄）

```bash
cd "D:/Download/Agent Vault/OneClickBlackScreen/OneClickBlackScreen-WorkTree/v11-r"
git log --oneline -4          # 预集成链：B-merge → A-merge → installer(B) → app(A)
cmd //c build.cmd && cmd //c build-setup.cmd && cmd //c verify.cmd
```

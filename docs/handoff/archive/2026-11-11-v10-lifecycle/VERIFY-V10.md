# VERIFY-V10 · I 轨集成与发版 v1.2.0 总证据（真机全生命周期终验）

> **一句话定位**：轨道 I（GOAL-V10I）全部可核对证据——review-v10 `--no-ff` 零冲突合入（26a0d62）→版本单源 1.2.0＋四处联动（fdf0090）→八件全绿×2＋零足迹三态→tag v1.2.0＋Release 四资产三方 digest 全 MATCH→**真机全生命周期冒烟九场景**（装 v1.1.0 自定义目录→设置默认根→老链真实更新→Setup 静默重装 taskkill/[Run]→数据根迁移→遮罩提示行→卸载器守卫双拒载→完整卸载全盘零残留→用户接管实装 v1.2.0→便携自替换回归）→worktree/分支清理（本地 3＋远端 3 逐支双证）。事实源＝[RESEARCH-V10.md](RESEARCH-V10.md) §3/§7＋[REVIEW-V10.md](REVIEW-V10.md)（契约核对＋移交注记）。
> **日期**：2026-11-11　分支 main（合并 26a0d62←origin/feature/review-v10 b80df60；发布提交 fdf0090；tag v1.2.0→fdf0090）。　**RELEASE URL**：https://github.com/Oiawlm/OneClickBlackScreen/releases/tag/v1.2.0

## 0. 前置确认

主检出 `git status` 干净、main＝origin/main＝d170677；`git ls-remote` 见 `feature/review-v10@b80df60`；冷启动五读齐（AGENTS/RESEARCH 全文/REVIEW 全文/GOAL/VERIFY-V9 流程骨架）。GOAL 字面 `origin/review-v10` 无此引用，按实际远端名 `origin/feature/review-v10` 执行（V9 同惯例）。

## 1. P1 合并实录

`git merge --no-ff origin/feature/review-v10`＝**ort 零冲突**，合并提交 **26a0d62**（双父 d170677＋b80df60）。`git diff d170677..HEAD --name-status`＝19 件 2401+/127-，与 REVIEW §2 申报**精确一致**（A 轨 14∪B 轨 2（.iss＋build-setup…实为 .iss 内含）∪R 轨 3：README/使用说明/REVIEW-V10.md，另 build.rsp 一行 D-R2 已裁）。

## 2. P2 版本单源联动（发布提交 fdf0090，diff 恰 4 件＝白名单）

- `src/AppInfo.cs:11`＝`Version = "1.2.0"`（全仓唯一手写版本常量）；`grep -rn "1\.1\.0" src/`＝**零命中**。
- **VerifyUpdate 版本表联动**（D-1 纪律）：镜像 `{1,2,0}`；newer＝v1.2.1/v1.3.0/v2.0.0/v10.0.0（1.1.1/1.2.0-as-newer 清零）；older＝v0.9.9/v1.0.0/v1.0.1/**v1.1.0**（前版转 older，恰填 v1.0.2 旧槽）；equal/bare/whitespace/`-rc1`/大写 V 用例随版本走（V1.0.2→V1.1.0）。**P2 门 `grep -rn "1\.1\.1\|1\.0\.2" src/ verify/`＝零命中**（字面达标）；验收清单的 `grep 1\.1\.0 verify/ 零命中`与 P2②「前版转 older 用例」互斥——按 V9 §2 同构裁决：**VerifyUpdate 存留 1.1.0 恰 2 处**＝older 用例＋大写 V malformed 用例（裁决 D-1）。
- **README** 4 处（:40 tag 链接/:44 Setup 名/:45 zip 名与显示名/:53 certutil 例）全部 1.1.0→1.2.0，文件内 1.1.0 零残留。
- **使用说明.txt**：标题 v8→**v9**；:7 前新增「v9 更新（对应程序版本 1.2.0）：更新自动检测、预下载与一键秒装覆盖升级；安装与数据位置可自定义；卸载零残留。」（58 字符单行，六需求一句话清单；v8 行保留）；页脚（1.1.0）→（1.2.0）。四文件 BOM/CRLF 形态 `file` 复验前后一致。

## 3. P3 构建与验证×2（本窗独占时段）

`build.cmd` exit 0（exe 277,504 B）；`build-setup.cmd` exit 0（ISCC 1.953s）→`dist\OneClickBlackScreen-Setup-1.2.0.exe`（2,150,216 B，SHA-256 `fc282f32…29b8`）。`verify.cmd` **八件全绿×2**（各 40 PASS，EXIT=0×2，零 FAIL 零 BUSY；[8/8] VerifyPaths 形状矩阵/resolver 矩阵/更新标记/迁移管线/PASS Z 真实键三态 tripwire）。**零足迹三态**（P3 前/P3 后/终态）：真实 settings.ini ABSENT×3、HKCU `Software\BlackScreen` ABSENT（冒烟前）/计划任务 ABSENT×3、进程 ABSENT×3（默认根仅存 V9 以来空目录壳，三态一致；S3c 后整删）。

## 4. P4 发版链

无 CI（仓库无 workflows）→ main 先推（d170677..fdf0090）→ `git tag -a v1.2.0`→fdf0090（注记沿 v1.0.2/v1.1.0 风格）→ tag 先推。`gh release create`（tag 已在远端，不带 --target——V9 事故④口径）：**name=v1.2.0、isDraft=false、isPrerelease=false、Latest 端点→v1.2.0**；body 六节（更新内容[新增/变更/开发者向]/资产清单/校验说明/安装与卸载说明）沿 v1.1.0 实例扩充。四资产（ASCII 名，L-015；zip label「一键黑屏 v1.2.0 完整包（exe＋config＋ico＋使用说明）」，四条目中文名完好）：

| 资产 | 大小 | SHA-256 |
|---|---|---|
| `OneClickBlackScreen-Setup-1.2.0.exe` | 2,150,216 | `fc282f320249ce00da66c989afa263979d10abaf0b2dd9b053df355353e629b8` |
| `OneClickBlackScreen.exe`（更新器契约资产） | 277,504 | `16c286787153cfeb03480637892cb5ffc30b7ca67bd55d89b5459dbca1d01d2c` |
| `OneClickBlackScreen-v1.2.0.zip` | 76,627 | `37da91de75e367bd24aaa51b789e29247fbaafe998276cf97811c637305844a8` |
| `SHA256SUMS`（3 行 `<hex>␣␣<name>`，Updater.cs 契约格式） | 292 | `a1f7675d8512c56760cb62123955d7384f5bf8cc6f68084730c072016439711c` |

**三方 digest**：服务端 `assets[].digest`＝本地 certutil＝`gh release download` 全量重下 Get-FileHash 复算，逐项 MATCH（下载重算前三行与 SHA256SUMS `diff` 零差异）。

## 5. P5 真机全生命周期冒烟（动机器前已明示清单；截图/输出留档 %TEMP%\v10i-release）

- **S1 装 v1.1.0**（Release v1.1.0 资产下载，digest 与 SHA256SUMS/VERIFY-V9 §4 逐项 MATCH＝真 1.1.0）：向导交互安装自定义目录 `D:\OBS-Smoke\V10\Install`（Inno 自建全新目录，REVIEW 新坑口径）；ARP `DisplayVersion=1.1.0`＋`InstallLocation` ✓、六文件（4 payload＋unins000×2）✓、SM 组两 lnk ✓、桌面无（任务页「创建桌面快捷方式」默认不勾向导实证）✓、HKCU `Software\BlackScreen` ABSENT（v1.1.0 无登记——GOAL①登记断言按裁决 D-3 移至 v1.2.0 侧覆盖）。
- **S2 使用与设置**：设置窗改色（第 2 色块）保存→确认框→`%LOCALAPPDATA%\BlackScreen\settings.ini` 落**默认根**（color=#4A1F1F，SHA-256 `74d6e8ce…2735`）；窗口打开即自动检查→状态「**发现新版 v1.2.0**」＋「立即更新」亮起（真实端点检测 e2e）。
- **S3 应用内更新（老链，真实用户迁移路径）**：点「立即更新」→真端点下载裸 exe 资产→ReplaceSequence 自替换→「已更新到 v1.2.0」框。断言：安装目录 exe SHA-256＝**16c28678…（v1.2.0 资产逐字节）**、`.old`＝277541e4…（v1.1.0 资产）✓、settings.ini 哈希不变 ✓。**观察（非 V10 回归）**：ARP DisplayVersion 未跳 1.2.0——v1.1.0/v1.2.0 两 exe AssemblyVersion 同为 csc 默认 `0.0.0.0`，运行中 1.1.0 进程 `Assembly.LoadFrom(新exe)` 命中同名身份返回**进程内旧程序集**→SyncUninstallVersion 写回旧值（V9 时代潜在限制；新进程旁证文件本身 `AppInfo.Version=1.2.0/IsLiteral=True`；Setup 重装自愈，见 S3b；提请 V11，§8）。
- **S3b 新链机制（真实 Setup 静默重装，REVIEW 移交②）**：启动遮罩（pid 7476，互斥持有）→以契约四参数跑真实 Setup-1.2.0（cmd //c 包裹防 MSYS 参数改写）→exit 0。断言：**遮罩 pid 7476→25920（InitializeSetup taskkill 杀旧＋[Run] 无 skipifsilent 自动重启，全屏重现后 ESC 净退）** ✓、ARP `DisplayVersion=1.2.0`＋InstallLocation 记忆 ✓、HKCU `InstallDirectory=D:\OBS-Smoke\V10\Install` 首登记 ✓、settings.ini 哈希不变 ✓、`.old` 被重启后 1.2.0 DeleteStaleBackup 清除 ✓、使用说明.txt 刷新为 v1.2.0 版（10,275B）✓。
- **S3c 数据根迁移（D4 管线 e2e）**：预写 DataParentDirectory 模拟 B 向导登记（B 移交「向导改父目录→设置窗迁移接力」路径）→设置窗「更改…」→FolderBrowserDialog 预填→预览框「旧位置 C:\…\BlackScreen／新位置 D:\OBS-Smoke\V10\Data\BlackScreen」实证→确认→复制校验登记原子切换。断言：三登记齐全（InstallDirectory/DataDirectory/DataParentDirectory）✓、新根 settings.ini **哈希＝旧哈希逐字节一致** ✓、旧默认根**整删**（守卫式删除连空壳回收）✓、重启后页脚「当前：D:\OBS-Smoke\V10\Data\BlackScreen」运行时解析 ✓、再改色保存（#1F3A5F）**写入自定义根**（GOAL②断言）而默认根不复存在 ✓。
- **S3d 遮罩提示行（REVIEW 移交②）**：手写 `UpdateAvailable=v9.9.9`→启动遮罩→窗口级截图（CopyFromScreen＋右下角裁剪×2）→视觉模型逐字读出「**检测到新版本 v9.9.9**」右下角 ✓→ESC 退出→值删除还原；全程无 `LastUpdateCheck` 写入（D-R3 无戳休眠实机实证）。
- **S6 卸载器归属守卫（GOAL⑥，B ④a 实机复演）**：unins000 副本至 OldSite＋临时改登记为「别处」＝**错位拒载 exit 1 零修改**；删登记＝**缺失拒载 exit 1 零修改**（ARP/文件/数据根/登记逐项快照不变）；登记还原。
- **S5 完整卸载＋全盘零残留**：ARP 卸载器 `/VERYSILENT /SUPPRESSMSGBOXES /LOG=`（Windows 路径）→exit 0，日志尾「Uninstallation process succeeded.／Removed all? Yes」。扫描：自定义安装目录删 ✓、默认安装目录无 ✓、**自定义数据根（登记值）删除** ✓、**数据父目录保留**（三重守卫不碰父级）✓、默认数据根无 ✓、ARP 键删 ✓、`Software\BlackScreen` **键整体消失**（三值＋壳）✓、计划任务无 ✓、SM 组删/桌面无 ✓、进程无 ✓。
- **S7 二次安装＝用户接管实装（裁决 D-3，V9 D-5 同款）**：本窗启动 v1.2.0 交互向导后**用户经远程会话接管完成**：安装目录 `D:\App\一键黑屏`、数据父目录 `D:\Download\App Data`（数据根 `D:\Download\App Data\BlackScreen`）。断言：ARP `一键黑屏 1.2.0`＋InstallLocation ✓、**三登记齐全**（InstallDirectory/DataDirectory/DataParentDirectory）✓、默认根不存在（全在 D 盘）✓。**用户实装保留，本窗未做任何卸载**——原案「数据页默认→零登记→卸载删默认根」（D-R7 真实构建覆盖）因同一 AppId 无法在不破坏用户实装的前提下再走一轮装/卸，**留档提请 V11**（该路径与 S5 已验登记路径共用同一 DeleteManagedDataRoot 守卫函数，形状判定同构，残余风险极小；裁决 D-4）。
- **S4 便携形态回归**：v1.1.0 zip（Release 资产）解压至全新目录→设置窗检查更新→两次网络瞬断（「基础连接已经关闭」「无法连接到远程服务器」——API 200/1.4s 而资产 CDN 超时，纯网络侧；旧更新器诚实报错零半成品）→CDN 恢复后重试成功：exe 自替换为 **SHA-256 16c28678…（v1.2.0 资产逐字节）**＋`.old` 备份→重启页脚「版本 1.2.0」＋`.old` 被 DeleteStaleBackup 清除 ✓＋配置行指向用户登记根（AppPaths 单点跨副本生效实机实证）✓。
- **S8 现场清理**：`D:\OBS-Smoke` 整删、`%TEMP%\BlackScreenUpdate` 不存在；终态快照＝仅用户实装三登记＋ARP 1.2.0，默认根/任务/进程全 ABSENT。**事故**：S4 便携更新时旧码 SyncUninstallVersion 身份碰撞把**用户 ARP DisplayVersion 写回 1.1.0**（S3 同根因的可观察效应）——本窗即以 reg add 修复回 1.2.0（安装器原值），根因分析随 §8 提请 V11。

## 6. REVIEW 移交注记处置表

| # | 注记 | 处置 |
|---|---|---|
| ① | VerifyGate.cs 头注释 seven→eight | **备案→提请 V11**（一行文案，不在 I 轨白名单——GOAL 铁律「落入白名单才可动手」压过 REVIEW 闭环请求，冲突留档） |
| ②a | 手写 UpdateAvailable 肉眼确认提示行 | **闭环**——S3d 截图＋视觉模型逐字实证 |
| ②b | 遮罩运行中 Setup 静默更新 taskkill＋[Run] | **闭环**——S3b pid 7476→25920 实证 |
| ②c | 无登记卸载删默认数据根（D-R7） | **提请 V11**——用户实装保留致不可达（裁决 D-4） |
| ③ | 冒烟安装用全新目录 | **闭环**——S1 Install/S4 Portable 均 Inno/自建全新 |
| ④ | 使用说明 v9 行随 1.2.0 添加 | **闭环**——P2 落地入双资产 |
| ⑤ | 卸载器静默参数口径 | **闭环**——S5/S6 全程 `/VERYSILENT /SUPPRESSMSGBOXES`＋`/LOG=` Windows 路径，无意外弹窗 |
| ⑥ | 形状谓词修复三文件 verify×2 | **闭环**——P3 两轮八件全绿即终验 |

## 7. 裁决与事故注记（如实）

**裁决**：**D-1** 验收清单「`grep 1\.1\.0 src/ verify/ 零命中」与 P2②「前版 1.1.0 转 older 用例」互斥——按 V9 §2 同构纪律执行（src/ 硬零＋VerifyUpdate 存留恰 2 处 older/malformed 用例），P2 门（1.1.1|1.0.2）字面全零达标。**D-2** GOAL①「v1.1.0 安装时自定义数据位置＋三登记断言」与 v1.1.0 安装器能力（无数据页/无登记）物理不符——登记断言移至 v1.2.0 侧覆盖（S3b InstallDirectory＋S3c 数据双登记＋S7 用户实装三登记一次全验），事实以 RESEARCH §2.1 为准。**D-3** S7 向导被用户接管实装自选双目录——真实 AppId 断言不受影响，沿 V9 D-5 先例留档。**D-4** D-R7 默认根卸载覆盖不可达（见 §5-S7）——机制共路与 B 隔离佐证，提请 V11。**D-5** S4 两次网络瞬断非产品缺陷（报错诚实、零残留），CDN 恢复后同链路一次成功。
**事故**：①git-bash MSYS 参数改写把 `/VERYSILENT` 等变成路径致 S1 走了交互向导（顺势改 computer-use 驱动完成，后续静默一律 `cmd //c` 包裹）；②PS 内联 `$`/`$_` 被 bash 双引号吞×3（V9 事故③坑复发，全部改脚本文件化）；③CUA 对自绘 WinForms 树两次「response lost」＋遮罩无 UIA 树——改 UIA BoundingRectangle＋user32 坐标点击与 CopyFromScreen 窗口捕获完成（工具缺陷，产品无缺陷）；④PS 控制台 GBK 输出 vs UTF-8 grep 编码错配——iconv 转码解决；⑤**SyncUninstallVersion 同名程序集身份碰撞**（两版 exe AssemblyVersion 均 0.0.0.0）：S3 不跳版（旧进程读回旧值）＋S4 污染用户 ARP DisplayVersion（已修复回 1.2.0）——V9 时代潜在限制首次获得可观察实证，提请 V11（改读 update.Tag 或每次发版递增 AssemblyVersion）。

## 8. 清理实录与提请 V11

- **worktree×3**：`git worktree remove` v10-a/v10-b/v10-r 全 exit 0＋`prune`，`worktree list` 仅主检出。
- **本地 3 支**：app-v10(29859f8)/installer-v10(fdd16b5)/review-v10(b80df60) 均 `merge-base --is-ancestor` **origin/main＋tag v1.2.0 双证**后 `-d` 删除（「was …」实录）。
- **远端 3 支**：`git push origin --delete` 3×`[deleted]`；`ls-remote --heads`＝**仅 main（fdf0090）**；tag v1.2.0（对象 1b46c8e＋peeled fdf0090）在远端。
- **提请 V11**：①REVIEW 四项（旧构建+远端新版时 verify 写真实 UpdateAvailable 的零足迹口径；LastUpdateCheck/UpdateAvailable 完整卸载后残留归属；含 `..` 登记值词法/归一化残余；DataParentDirectory 设置窗改址不回写）；②VerifyGate.cs seven→eight 文案；③D-R7 默认根卸载真实构建覆盖；④SyncUninstallVersion 身份碰撞（S3/S4 双实证）——改读 update.Tag 或 AssemblyVersion 随版递增；⑤遮罩提示行文案打磨、安装向导文案用户测试、ets/304 缓存可选项（GOAL §7 原列）。

## 9. 验收清单对照（GOAL-V10I §6 七项）

1✅ release 提交 fdf0090 已推 main；`ls-remote --tags v1.2.0` 可见（§4/§8）。2✅ AppInfo 唯一「1.2.0」；src/ 1.1.0 零命中＋VerifyUpdate 恰 2 处纪律存留（§2，裁决 D-1）。3✅ `dist\OneClickBlackScreen-Setup-1.2.0.exe`（ASCII 名）存在；八件全绿×2（40 PASS/轮）输出留档（§3）。4✅ 四资产上传，三方 digest 全 MATCH（§4）。5✅ 本文 §5 含六场景逐条实录（自定义数据根更新前后哈希对照 `74d6e8ce…`＝`fefa993f…` 于 S3b/S3c/S5 三点留档；卸载后全盘零残留逐项清单 S5）。6✅ 零足迹三态（§3）；worktree×3＋分支本地 3/远端 3 双证后删、终态仅 main（§8）。7✅ `git diff --name-status b80df60..HEAD -- setup/ src/AppInfo.cs src/Updater.cs src/AppPaths.cs verify.cmd`＝仅 `M src/AppInfo.cs`，全量 diff＝白名单 4 件，无计划外改动（P7 实录）。

## 10. 收尾

本文档提交 `docs(I): VERIFY-V10 …` 后 `git push origin main`（tag 已先行）。V10 批次目录由 V11 规划窗归档（沿 D12 惯例）。临时工具（%TEMP%\v10i-release、/tmp 证据、D:\OBS-Smoke）均已弃置/整删；用户实装（D:\App\一键黑屏＋D:\Download\App Data\BlackScreen）保留完好、ARP 已复原本值。

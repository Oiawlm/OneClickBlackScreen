# VERIFY-V10A · A 轨执行证据（应用侧：更新链双形态＋数据根单点＋自动检测）

> **一句话定位**：A 轨（feature/app-v10）全部产出的真实命令证据——src 应用侧七件改动＋verify 八件全绿×2、VerifyPaths 解析矩阵与迁移 TEMP 演练、真实 `Software\BlackScreen` 键三态＋套件级前后快照、RESEARCH §7「A 侧」契约逐项代码对照、移交注记。　**事实源＝[RESEARCH-V10.md](RESEARCH-V10.md)**。　**日期**：2026-11-11　执行：GLM-5.3 A 轨窗（worktree `OneClickBlackScreen-WorkTree\v10-a`）。

## 1. 产出清单（diff 范围）

worktree `../OneClickBlackScreen-WorkTree/v10-a`（基于 origin/main d170677 建枝），`git add -A` 后 `git diff --cached --stat` 汇总：**13 files changed, 1470 insertions(+), 83 deletions(-)**。逐件：

| 文件 | 状态 | 内容 |
|---|---|---|
| `src/AppPaths.cs` | 新 | 数据根单点：HKCU `Software\BlackScreen` 可注入读写缝（`IRegistryStore`/`HkcuStore`，验证器注入 `-VERIFY-TEST` 键根）、受管形状判定唯一实现、`DataRoot/SettingsIniPath/DataParentDirectory`、更新标记（`UpdateAvailableTag`/`CompleteUpdateCheck`/`IsUpdateCheckDue` 24h 节流）、D4 迁移管线（复制→逐文件校验→登记→守卫式删旧）＋`CommitDataRoot`（默认态零登记） |
| `src/MaskConfig.cs` | 改 | `SettingsPath` 改走 `AppPaths.SettingsIniPath` 单点（直拼路径已删）；解析/原子写语义零变化（VerifyConfig 23 用例原样全绿） |
| `src/Updater.cs` | 改 | `Check()` 双资产解析（exe＋`OneClickBlackScreen-Setup-{tag剥v}.exe` 各携 URL/size/digest）＋超时重载；`IsInstalledForm` 形态判定（HKCU Uninstall `{AppId}_is1` 键存在，探针可注入）；`DownloadStaged` 预下载暂存（`%TEMP%\BlackScreenUpdate\` 落盘前清空、整程持 `Global\BlackScreen.Update` 互斥、失败清暂存）；`SetupSilentArguments`＋`SetupStartInfo`＋`LaunchSetup`（短临界区防双 Setup）；`ParseSha256Sums` 按资产名参数化；`TimedWebClient`（WebClient 无 Timeout 属性的 BCL 惯用解）。**便携形态 `ReplaceSequence`/`DeleteStaleBackup`/互斥链路逐行未动**（V7S 50 轮并发用例原样通过） |
| `src/Program.cs` | 改 | `RunOverlay` 挂两层分离的更新感知：`ReadUpdateHintTag()`（本地读 `UpdateAvailable`＋`IsNewer` 过滤，fail-closed）＋`BeginThrottledUpdateCheck()`（≥24h 才后台 Check，3s 短超时、403/失败全静默、仅完成态写标记）；遮罩互斥 `Local\BlackScreen.Mask` 原行未动（Program.cs:28） |
| `src/OverlayForm.cs` | 改 | `OverlayForm` 增 `updateHint` 字段＋`PaintUpdateHint` 静态纯绘制（右下角、小字号 max(12, h/60)、自动对比色、一角留边）；`OverlayGroup(config, updateHint)` 传参。纯展示，不参与退出判定 |
| `src/SettingsDialog.cs` | 改 | ①「数据位置」区（当前根展示＋「更改…」→浏览父目录→预览→迁移管线→重启提示并 Close）；②更新区预下载状态机（发现新版→写标记→后台预下载+校验进状态条→「立即更新」亮起＝已就绪语义；点击→安装形态 `LaunchSetup`＋本窗退出／便携形态 UI 线程 `ReplaceSequence` 原链路） |
| `src/SettingsDialogMetrics.cs` | 改 | 新增 `DataChangeButtonWidthPx=80`（复用 80 档，零新档位；fts 哲学层：档位制/克制/零新增字号乱档） |
| `verify/VerifyPaths.cs` | 新 | 第八件：形状矩阵＋解析矩阵＋更新标记矩阵＋迁移 TEMP 演练五案＋HKCU `-VERIFY-TEST` 键 finally 自清＋真实键/真实 ini 三态快照 |
| `verify/VerifyUpdate.cs` | 改 | 新增 `CheckDualAssetAndStaging`：资产名公式/静默参数构造断言（不真跑）、双资产 Check、形态探针注入、安装/便携两形态暂存、篡改哈希拒绝+暂存清理、缺 Setup 资产按形态报错、SUMS 兜底 Setup 行。**现有用例零删改**（版本表/哈希链/替换彩排/50 轮并发原样） |
| `verify/VerifySettings.cs` | 改 | 镜像扩展：fixedChrome 与布局公式加数据位置区三控件、KindOf 加 `数据位置/当前：/更改…` 识别、直接子控件 24→27 |
| `verify/VerifyOverlay.cs` | 改 | 新增 PASS I：`OverlayForm.PaintUpdateHint` 状态注入断言（有 tag＝仅右下四分之一象限有着色、中央文字区零侵入；无 tag＝整幅纯背景零像素） |
| `verify.cmd` | 改 | 八件化：VerifyConfig/VerifyOverlay 编译行挂接新依赖（AppPaths/OverlayForm/MaskPainter/MaskConfig），新增 VerifyPaths 编译行，序号 [1/8]..[8/8] |
| `build.rsp` | 改 | **白名单外一行**：新增 `src\AppPaths.cs`（裁决见 §6 移交注记①） |

## 2. 八件全绿×2（真实输出）

`cmd /c verify.cmd` 两轮，退出码均 0，各 **70 行 PASS、零 BUSY、零 FAIL**（L-012/L-017 全路径证据留档于本轮会话 /tmp/verify-round1.txt、verify-round2.txt，关键行如下）：

```
[1/8] VerifyConfig    PASS: 23 config tests (defaults/roundtrip/format/golden/corruption/escapes/atomic-write/idle-key)
[2/8] VerifySettings  PASS: geometry mirrored SettingsDialogMetrics (+-1px), ... ｜ PASS TOTAL: 6 settings checks
[3/8] VerifyMouseExit PASS TOTAL: 5/5 buttons; 30 movement messages; 10 wheel messages; ...; phantom-key click exit OK
[4/8] VerifyOverlay   PASS A..H + PASS I: update hint line - tagged paints the bottom-right quadrant only, untagged paints nothing
[5/8] VerifyIcon      PASS TOTAL: 4 icon checks
[6/8] VerifyUpdate    PASS: ... concurrent replaces serialize ... (x50) ｜ dual-asset Check ... ｜ installed-form staged predownload ... ｜ tampered Setup digest refused + staging cleaned ｜ missing Setup asset refuses the installed form (fail-closed by form) ｜ portable-form staged predownload (exe asset, verified) ｜ SHA256SUMS fallback verified the Setup asset line
[7/8] VerifyIdle      PASS TOTAL: 5 idle checks
[8/8] VerifyPaths     PASS: managed shape matrix ｜ resolver matrix ｜ update marks ｜ migration pipeline in TEMP ｜ PASS Z: real Software\BlackScreen key ... identical across snapshots #1/#2 (three-state tripwire)
[OK] ALL VERIFIERS PASSED
```

构建：`cmd /c build.cmd` → `[OK] Build succeeded: ...\dist\一键黑屏.exe`（AppInfo.Version 保持 1.1.0，本轮零改动）。

## 3. VerifyPaths 逐条断言（第八件实录）

**形状矩阵**（`IsManagedDataRoot` 唯一实现，B 侧同构判定靶）：

```
PASS: managed shape matrix (suffix \BlackScreen, case-insensitive, non-root, no trailing separator)
  通过例：D:\App\BlackScreen / D:\BlackScreen / \\server\share\BlackScreen / d:\app\blackscreen（大小写不敏感）/ "D:\App\BlackScreen "（首尾空白 trim 后通过）
  拒绝例：D:\（盘根）/ D:\BlackScreenX / D:\NotBlackScreen / D:\App\BlackScreen\（尾分隔符）/ "" / null
```

**解析矩阵**（D3）：无登记→默认根＋根下 settings.ini＋预填空；登记合法（`D:\Data\BlackScreen`）→登记值胜出；五种非法形状（错叶名/更长后缀/盘根/尾分隔符/纯空白）逐条回退默认（fail-closed 回退、绝不报错）；删登记→默认；`DataParentDirectory` 透传预填。

**更新标记**（D7）：`IsUpdateCheckDue`——无戳=休眠、垃圾戳=休眠、新戳=不 due、25h 旧戳=due；`CompleteUpdateCheck("v9.9.9")` 写 tag＋戳、`CompleteUpdateCheck(null)` 清 tag＋刷戳。

**迁移管线 TEMP 演练五案**（D4）：

| 案 | 场景 | 断言 |
|---|---|---|
| 1 | 仅 settings.ini | 新根字节级一致、登记提交新根、旧根守卫删除（内容 ⊆{settings.ini}） |
| 2 | 旧根多一个 user-notes.txt | 复制校验＋提交照常、旧根保留（守卫拒绝）、outcome 报旧根路径 |
| 3 | 目标 settings.ini 被锁（FileShare.None） | 迁移失败、**登记未动**（仍在旧根）、旧数据完好（原子性：失败在提交前） |
| 4 | 旧根不存在（新装机） | 无可复制、提交照常、旧根报告为已移除 |
| 5 | `CommitDataRoot(默认根)` | 登记值**删除**（默认态零登记，D3） |

**真实键三态**：`PASS Z: real Software\BlackScreen key and real settings.ini identical across snapshots #1/#2 (three-state tripwire)`（#1 进程起始 / #2 测试块后 / #3 退出路径 finally 内再验，不一致即抛）。

**HKCU 测试键隔离**：全部经 `Software\BlackScreen-VERIFY-TEST` 注入缝，finally `DeleteSubKeyTree` 自清＋断言键不存在（`test key tree still exists after cleanup` 守卫）。

**套件级零足迹**（本机真实状态，reg query 实录）：

```
套件前：reg query "HKCU\Software\BlackScreen" /s → 错误: 系统找不到指定的注册表项或值。(<key absent>)
套件后：同上 <key absent>            ← 两轮八件全跑完后
真实 settings.ini：VerifySettings/VerifyOverlay/VerifyPaths 各自 PASS Z（SHA-256 before==after）×2 轮
%TEMP%\BlackScreenUpdate：套件后不存在（VerifyUpdate finally 清理＋守卫）
```

## 4. VerifyUpdate 双形态断言实录（P6② 全项）

```
PASS: Setup asset name + silent command line pinned as data (no installer executed)
  SetupAssetNameFor("v9.9.9")=="OneClickBlackScreen-Setup-9.9.9.exe"、("1.2.3")==bare 形式
  SetupSilentArguments=="/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /FORCECLOSEAPPLICATIONS"
  SetupStartInfo(path).FileName/Arguments 逐字断言（Process.Start 未执行）
PASS: dual-asset Check (Setup + exe URLs/hashes on UpdateInfo)
PASS: installed-form staged predownload (staging reset, verified artifact resident)   ← 探针注入 true；stale-junk.bin 被清；暂存目录唯一文件=Setup 资产名
PASS: tampered Setup digest refused + staging cleaned                                  ← 失败路径整目录清空
PASS: missing Setup asset refuses the installed form (fail-closed by form)             ← exe-only 发布 + 安装形态 → "安装包资产"
PASS: portable-form staged predownload (exe asset, verified)                           ← 探针注入 false；暂存文件名=OneClickBlackScreen.exe
PASS: SHA256SUMS fallback verified the Setup asset line
现有用例零删改实证：版本表/全链路/篡改/兜底/404/等值 tag/替换彩排/50 轮并发——输出行与 V9 逐字相同（含 x50 并发序列化行）
```

## 5. RESEARCH §7 契约表「A 侧」逐项代码对照（R 轨核对靶）

| 契约项 | 冻结值 | A 侧实现锚点（feature/app-v10） |
|---|---|---|
| HKCU 键根 | `Software\BlackScreen` | `AppPaths.RegistryRoot`（src/AppPaths.cs:20） |
| DataDirectory | REG_SZ 完整路径；形状=以 `\BlackScreen` 结尾且非根；仅非默认存在 | 解析 `DataRoot`（:120）；形状 `IsManagedDataRoot`（:99，大小写不敏感＋trim＋`GetDirectoryName!=null` 非根判定，**唯一实现**）；默认态零登记 `CommitDataRoot`（:190 删值分支）；迁移写 `MigrateDataRoot`（:215） |
| DataParentDirectory | REG_SZ 预填 | `DataParentDirectory` 只读（:138 附近）；设置页仅预填（A 侧不写，§7 表列明 B 写） |
| InstallDirectory | REG_SZ＝{app}，uninsdeletevalue | A 侧**零触碰**：`grep -rn InstallDirectory src/*.cs` 零命中（实录） |
| LastUpdateCheck / UpdateAvailable | REG_SZ | `AppPaths` 标记组：值名常量（:24-25）、`CompleteUpdateCheck`（:162，戳 ISO-8601 "o" UTC＋tag 置/清）、`IsUpdateCheckDue`（:175，24h）；消费点=Program.cs:42/43、SettingsDialog BeginCheck/BEGINAPPLY |
| AppMutex | `Local\BlackScreen.Mask` 不变 | Program.cs:28 原行未动（git diff 该行零改动） |
| Setup 静默参数 | `/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /FORCECLOSEAPPLICATIONS` | `Updater.SetupSilentArguments`（src/Updater.cs:288）；启动器 `LaunchSetup`（:303，短临界区持 `Global\BlackScreen.Update`） |
| 更新资产名 | `OneClickBlackScreen-Setup-{ver}.exe`（ver=tag 剥 v） | `Updater.SetupAssetNameFor`（src/Updater.cs:159），与 build-setup.cmd `OutputBaseFilename=OneClickBlackScreen-Setup-{#AppVersion}` 互证 |
| 预下载暂存 | `%TEMP%\BlackScreenUpdate\`，先清旧再落 | `Updater.StagingDirectory`（:199）＋`DownloadStaged`（:209，ResetDirectory 先行） |
| 形状判定同构 | 「以 `\BlackScreen` 结尾＋非根」两侧逐字同实现 | A 侧在 AppPaths.IsManagedDataRoot（见上）；语义四要素供 R 对照 B 侧 `IsManagedDataDirectory`：①后缀 `\BlackScreen` 大小写不敏感 ②非根=存在父目录 ③尾分隔符拒绝 ④首尾空白 trim |
| 测试变体隔离 | 数据键加 `-TEST` 后缀 | VerifyPaths 全走 `Software\BlackScreen-VERIFY-TEST` 键＋finally 自清（§3） |

单点实证（GOAL 验收线）：`grep -n "LocalApplicationData.*BlackScreen" src/*.cs` → **零命中**（严于「仅 AppPaths.cs」：唯一拼接点 AppPaths.cs:112 经 `ManagedDirectoryName` 常量间接引用 "BlackScreen" 字面量；src 内 `LocalApplicationData` 其余两处均为注释——AppPaths.cs:11 单点声明、MaskConfig.cs:47 指路注释）。

## 6. 移交注记（R/I 轨重点核对项）

1. **【裁决：白名单外一行 build.rsp】** GOAL §4 白名单未列 `build.rsp`，但 build.rsp 逐文件显式列举 src 编译单元，`src/AppPaths.cs` 不加行则主程序无法编译——「白名单」与「P3 构建全绿硬验收」冲突的唯一可执行解＝加一行（沿 V9 D-1「GOAL 漏列而硬验收唯一解」先例照纪律处置留档）。R 轨若不认可，可改为 GOAL 勘误。
2. **【LastUpdateCheck 缺失＝休眠】** D7 原文「距 LastUpdateCheck≥24h→Check」对「无戳」未定义。实现取**无戳=不休眠不检查**（`IsUpdateCheckDue` null→false）：激活路径＝①设置窗检查**发现新版**（CompleteUpdateCheck(tag)）②遮罩完成过一次检查③应用内更新成功。动机：验证器子进程（遮罩/设置窗）若在无戳时也发起检查，会把 LastUpdateCheck 写进**真实 HKCU**（零足迹红线，本机真实键套件前后 absent 实证依赖此设计）；且首装用户「设置窗打开即查」仍 100% 覆盖检测面。副作用：从不打开设置窗且从未有新版的用户，遮罩不自查——与 v1.1.0 现状等同（单调改进，无回退）。**R 轨请裁决是否接受此语义**。
3. **【Check 对缺 exe 资产保持无条件报错】** GOAL P3「fail-closed 按形态分置」实现为：缺 Setup 资产→仅安装形态消费时报错（`DownloadStaged`「安装包资产」）；缺 exe 资产→Check 仍无条件抛「更新资产」（现有 VerifyUpdate 用例零删改的硬约束——本机 dev 曾装 v1.1.0，若 Check 按形态放行则 `assets:[]` 用例在装过的机器上必翻车）。净效果：真实发布（四资产齐）零差异；异常发布下安装形态在 Check 期即失败＝更保守。
4. **【暂存下载整程持全局互斥】** `DownloadStaged` 在 `Global\BlackScreen.Update` 互斥内完成 清空→下载→校验（入闸等待沿用 10s 上限，持闸时长=下载时长）：两设置窗实例并发预下载不会交叉写 `%TEMP%\BlackScreenUpdate\`；慢网下第二实例可能收「另一更新正在进行」＝可重试的诚实报错。R 轨如认为持闸过宽可改按文件名隔离，但当前实现与「一个暂存目录」契约（§7）最简一致。
5. **【遮罩提示行 e2e 局限】** VerifyOverlay PASS I 为**painter 级状态注入**（PaintUpdateHint 直调），非进程级 e2e——e2e 需向真实 HKCU 写 `UpdateAvailable`（零足迹禁止）。端到端链路（Program.cs:42 读标记→IsNewer→OverlayGroup→OverlayForm.OnPaint）由代码审查＋本机真实键 absent（提示必关）下 A/B/C 角点采样双重覆盖；I 轨真机冒烟建议补一场景：手写 `UpdateAvailable` 后启动遮罩肉眼确认提示行（见 §7 场景清单）。
6. **【DataParentDirectory A 侧只写不写】** §7 表 A 侧列＝「设置页预填读」，故设置窗迁移**不回写**预填值（B 向导页是唯一写者）；经由设置窗改址后下次预填可能仍是旧父目录（仅预填便利性，无功能影响）。
7. **【设置窗静默检查的网络面】** VerifySettings 子进程现在仍对真实 api.github.com 发一次静默 GET（v1.1.0 既有行为，U2）；V10 起若真有新版会被写 `UpdateAvailable` 到**真实 HKCU**（发现新版才写，本轮 latest==1.1.0 不触发）。未来轮次若「旧构建＋远端已有新版」时跑 verify，真实键会出现标记值——届时零足迹断言需按值清理或接受该行（提请 V11 规划注意）。
8. **【VerifyGate 注释未同步】** verify/VerifyGate.cs 头注释仍写「all seven verifiers」——该文件不在本轨白名单，未动；R 轨顺手改一个词即可。

## 7. 留给 I 轨真机冒烟的场景清单（建议）

1. 装形态全链：装 v1.1.0 →（B 轨合并后）应用内更新 → 预下载进度→一键秒装（Setup 静默）→遮罩自动重启（B [Run]）→ARP DisplayVersion=v1.2.0。
2. 便携形态全链：裸 exe 运行 → 应用内更新 → 预下载→UI 线程自替换 → 重启遮罩为 v1.2.0 → .old 清理。
3. 数据目录：设置窗「更改…」→迁移→重启生效→新根 settings.ini 字节一致→旧根删除；含旧根有杂文件的守卫保留分支。
4. 遮罩提示行：真实写 `HKCU\Software\BlackScreen\UpdateAvailable="v9.8.8"`（低版本 tag 不显示 / v9.9.9 显示）肉眼双态；24h 节流：写新戳→启动遮罩无网络行为（抓包或 LastUpdateCheck 不变）。
5. 完整卸载（B 轨产物）：改数据父目录→卸载→数据根（读登记，无登记用默认）删除、HKCU 两键零残留。

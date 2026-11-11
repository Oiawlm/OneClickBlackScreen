# REVIEW-V10 · R 轨复检预集成证据（合并 A/B＋契约核对＋行为文档）

> **一句话定位**：R 轨（feature/review-v10）全部产出——A/B 双 `--no-ff` 零冲突合并、RESEARCH §7 契约表 11 行逐行双侧取证（发现并修复行 10 形状谓词四点分歧）、合并树 verify 八件全绿＋B 隔离自测四阶段复跑＋build-setup 绿、README/使用说明 行为文档四处改写＋三处同性质联动。　**事实源＝[RESEARCH-V10.md](RESEARCH-V10.md)**。　**日期**：2026-11-11　执行：GLM-5.3 R 轨窗（worktree `OneClickBlackScreen-WorkTree\v10-r`）。

## 1. 背景

A（应用侧）与 B（安装器侧）按 RESEARCH §7 冻结表并行实现，R 轨职责＝证明两侧逐项同构、漂移在此波修复、行为文档随定型行为改写（PLAN §2）；I 轨只做发版联动。前置检查通过：`git ls-remote` 两轨分支均可见（app-v10=29859f8、installer-v10=fdd16b5）、feature/review-v10 无撞名。

## 2. 现状产出

**合并**：worktree 基于 origin/main d170677 建枝；A 轨两次 `--no-ff` 合并零冲突（A=14 件 1601+/83-，B=2 件 689+/21-；写面零相交符合 D9 设计）。

**P1 契约核对表（§7 全 11 行，双侧行号级）**：

| 契约项 | A 侧锚点 | B 侧锚点 | 结论 |
|---|---|---|---|
| HKCU 键根 | AppPaths.cs:20 `Software\BlackScreen` | .iss:54（测试变体 :47） | ✅ 一致 |
| DataDirectory 值名/形状/零登记 | :22 值名、:99 谓词、:120-126 解析、:190-197 零登记 | :196 值名、:327-349 谓词、:511-514 默认清 | ⚠️ 形状分歧→已修（见 §4 D-R1） |
| DataParentDirectory | :136-143 只读预填（A 不写） | :521 向导写 | ✅ 一致 |
| InstallDirectory | src/** 零命中（grep 实录） | :156 [Registry]+uninsdeletevalue、:548 归属校验 | ✅ 一致 |
| LastUpdateCheck/UpdateAvailable | :24-25/:152-183 | .iss 零命中（grep 实录） | ✅ 一致 |
| AppMutex `Local\BlackScreen.Mask` | Program.cs:28 原行未动（diff 零改动） | :41/:84/:444-454 三处一致 | ✅ 一致 |
| Setup 静默参数 | Updater.cs:288 逐字四参数 | [Setup] 指令＋自测③契约参数全链路实测 | ✅ 一致 |
| 更新资产名 | Updater.cs:159-164（tag 剥 v） | :71 OutputBaseFilename 未改 | ✅ 一致（与 build-setup 产物名互证） |
| 预下载暂存 | Updater.cs:199-201 `%TEMP%\BlackScreenUpdate` | .iss 零命中 | ✅ 一致 |
| 形状判定同构 | AppPaths.IsManagedDataRoot（修后） | .iss IsManagedDataDirectory（修后） | ✅ 修复后同构（§4 D-R1） |
| 测试变体隔离 | VerifyPaths `-VERIFY-TEST` 键＋finally 自清 | :47 `Software\BlackScreen-TEST` | ✅ 一致（后缀名不同但键面不相交，无冲突） |

**P2 A 轨复检**：diff 14 件 ⊆ GOAL-V10A §4 白名单＋build.rsp 一行（见 D-R2）；`grep "LocalApplicationData.*BlackScreen" src/*.cs` 零命中（SettingsPath 单点达标）；VerifyUpdate 断言面抽查（:313-322 资产名/静默参数逐字断言、形态分置 fail-closed、暂存清旧＋失败清空）与 VERIFY-V10A §4 实录一致；U2 边界＝Program.cs BeginThrottledUpdateCheck 节流守门＋后台线程＋3s 超时＋catch 全静默＋仅完成态写标记，提示行纯本地读＋IsNewer 过滤，达标。

**P3 B 轨复检**：diff 2 件全入白名单；.iss 对照 D2（AppMutex/taskkill/[Run] 无 skipifsilent＋`#ifndef TestId` 抑制 :167-182）/D4（数据页五重校验 :384-431：非空/全限定/非根/可建/可写探针）/D5（CurUninstallStepChanged :610-640＋三重守卫＋ConfirmUninstall 文案 :120）/D6（InitializeUninstall :536-560）逐项过；编码改前改后 `file` 双验 UTF-8 BOM＋CRLF。

**P4 合并树验证（真实输出）**：`build.cmd` → `[OK] Build succeeded`；`verify.cmd` **八件全绿**（[1/8]..[8/8]，`[OK] ALL VERIFIERS PASSED`，EXIT=0，零 BUSY 零 FAIL；VerifyPaths 形状矩阵以修复后语义通过：`parent non-root, fully qualified, no trailing separator`；PASS Z 真实键三态 tripwire 零足迹）；B 隔离自测四阶段复跑 **ALL-STAGES-PASS**（①静默装：六 payload＋InstallDirectory 登记＋默认态零数据登记＋真实键零新增＋[Run] 抑制 NOT_RUNNING；②登记切换：Parent1＋受管根创建＋真实键零新增；③覆盖更新契约参数：settings.ini SHA-256 前后一致 `0AF327F3…859A`＋三登记保留＋删除-恢复刷新证明；④静默卸载零残留：Install 删除＋受管根含 settings.ini 删除＋父目录保留＋三值清＋测试 ARP 键删＋计划任务清＋开始菜单/桌面 0/0；前后快照真实键 ABSENT 一致；测试树自清）；`build-setup.cmd` EXIT=0（ISCC 2.000s）＋测试变体 ISCC /DTestId EXIT=0（1.625s）。

**P5 行为文档（README 6 处/说明 7 处；diff 摘要 31+/23-）**：①README 下载与安装——Setup 用法列补「可自定义安装目录与数据位置」、卸载行改零残留；②README 边界与限制——隐私口径改两层（设置窗即查＋遮罩隔天静默查一次、空闲自检永不联网）、便携卸载补数据位置；③说明【自动更新】整段改「检查（含遮罩隔天补查）→预下载＋校验→遮罩提示行→一键秒装双形态」，替换 :67「更新为单文件替换」旧承诺；④说明卸载节——安装包版零残留＋守卫保留边界、便携版补数据位置；联动：说明隐私口径（RESEARCH D7 明令同步）＋:94 遮罩联网口径＋【保存后】数据位置＋「七件套」→「八件套」（VerifyPaths）＋README:77 页脚行为/README:80 数据位置。**版本引用零改动**：`grep -c "1\.1\.0"` README=4、说明=2，与改前持平；两文件 BOM＋CRLF 保真。

## 3. 踩坑（引 lessons）

- **L-012（零足迹）**：真实键/真实 ini 全程三态快照一致；B 自测走 /DTestId 四隔离位，测试树 TEMP 自清。
- **L-006（编码三变体）**：.iss/使用说明 保真编辑＋改后 `file` 复验 BOM＋CRLF。
- **本轮新坑（Inno 卸载目录回收语义）**：自测 harness 曾 `mkdir -p Install` 预创建安装目录→卸载后空目录残留误报缺陷。真相＝Inno 只卸载**自己创建**的目录（unins000.dat 记录驱动，日志无「Deleting directory」行）；用户选既有目录作 /DIR＝目录归属用户，卸载只清文件。复跑去掉预创建后四阶段全过（Inno 自身「Failed to delete directory (145). Will retry later」重试机制工作正常）。I 轨真机冒烟务必用全新目录装。

## 4. 决策与否决

- **D-R1 形状谓词四点分歧（P1 核心发现，产品流可达）**：A 设置窗 FolderBrowserDialog 可选 `D:\` 为父目录→现 A 谓词放行→迁移写入 `D:\BlackScreen` 登记→B 卸载拒绝该形状→不可抑制弹框中断＋数据残留＝零残留断裂。四点：父=盘根（A 放行/B 拒）、父=共享根（同）、非全限定 `\BlackScreen`（A 放行/B 拒）、尾分隔符（A 拒/B 归一化救回）。**裁决以 RESEARCH 为准＝B 读法正确**：「非根」非虚词（路径以 \BlackScreen 结尾者本身不可能是根，A 读法使该子句恒真无意义）；D4 向导「校验非根」对象＝所选父目录；fts 同构（B §6-4）；D5 删除安全；且 A 自己的迁移提示文案（SettingsDialog:889「必须位于非根目录下」）就是 B 读法。修复：A 侧 IsManagedDataRoot 收严（父目录非盘/共享根＋全限定，src/AppPaths.cs）；B 侧叶检查移到原始 trim 串杜绝尾分隔符救回（setup/OneClickBlackScreen.iss）；**VerifyPaths.cs 矩阵 :87-88 期望随裁决翻转＋新增非全限定拒绝用例**——白名单外小改，沿 V9 D-1/V10A build.rsp「白名单 vs 硬验收冲突唯一可执行解」先例照纪律处置（修复后 verify 八件全绿为证）。残余（接受留档）：含 `..` 的登记值 A 词法放行/B 归一化后拒绝——两侧写者均只写归一化值（A CommitDataRoot 走 GetFullPath、B 写 BuildDataRoot），仅手改注册表可达，两侧行为均保守安全。
- **D-R2 复核认可 A 轨 build.rsp 白名单外一行裁决**（AppPaths.cs 编译必需，V9 D-1 先例）。
- **D-R3 接受 A 注记②**：LastUpdateCheck 无戳＝休眠不检查——零足迹红线（验证器子进程不触真实键）所迫，检测面仍由「设置窗打开即查」100% 覆盖，较 v1.1.0 单调改进。
- **D-R4 接受 A 注记④**：预下载整程持 `Global\BlackScreen.Update` 互斥——与「单一暂存目录」契约（§7）最简一致，慢网第二实例收可重试诚实报错。
- **D-R5 接受 A 注记⑥**：设置窗迁移不回写 DataParentDirectory（B 向导页是唯一写者，§7 表列明；仅预填便利性）。
- **D-R6 接受 B 注记（数据登记走 [Code] 不走 [Registry]）**：uninsdeletevalue 会在守卫路径末尾无条件清值，违背「登记与数据互为可寻回指针」；InstallDirectory（永真值）仍走 [Registry] 承担 D5 兜底。
- **D-R7 接受 B 注记（测试变体 `#ifdef TestId` 跳过无登记默认根删除）**：隔离构建从构造上不可能删真实 %LOCALAPPDATA%\BlackScreen；真实默认根删除与登记路径共用同一守卫函数，由 I 轨真机冒烟覆盖。
- **否决**：不改 VerifyGate.cs 头注释（不在白名单，移交 I）；不把「更新成功后清 UpdateAvailable」扩展为卸载期清理（契约「B 不触碰」，提请 V11）。

## 5. 下一步

I 轨合并 feature/review-v10→AppInfo 1.2.0＋VerifyUpdate 版本表联动→全量验证→tag/Release 四资产→**真机全生命周期冒烟**（装 v1.1.0→改数据目录→应用内更新→完整卸载→全盘零残留）→清理 worktree/分支→VERIFY-V10.md。V10 批次目录由 V11 规划窗归档。

## 6. 快速启动命令

```bash
cd "D:/Download/Agent Vault/OneClickBlackScreen/OneClickBlackScreen-WorkTree/v10-r"
git log --oneline -5                      # 两次 --no-ff 合并＋本轨提交
cmd //c build.cmd && cmd //c verify.cmd   # 八件全绿（GUI 件勿动键鼠）
cmd //c build-setup.cmd                   # ISCC 绿
```

## 移交注记

**I 轨闭环项**：①VerifyGate.cs 头注释「all seven verifiers」改「eight」（A 注记⑧遗留，一行文案）；②真机冒烟补三场景——手写 `HKCU\Software\BlackScreen\UpdateAvailable="v9.9.9"` 启动遮罩肉眼确认右下角提示行（A 注记⑤：painter 级断言无法覆盖进程级 e2e）、遮罩运行中跑 Setup 静默更新断言 taskkill＋[Run] 自动重启（B 注记：无法在隔离自测演练）、无登记状态卸载删默认数据根（D-R7）；③冒烟安装务必用**全新目录**（§3 新坑：预创建目录不被 Inno 回收）；④发版时使用说明 v9 更新行随 1.2.0 添加（v8 行保留）；⑤卸载器静默参数口径：全静默必须 `/VERYSILENT /SUPPRESSMSGBOXES`，`/LOG=` 必须 Windows 形态路径（B 事故记录）；⑥形状谓词修复涉及 src/setup/verify 三文件（D-R1），I 轨合并后 verify×2 全绿即为终验。

**提请 V11 项**：①「旧构建＋远端已有新版」时 verify 会写真实 HKCU `UpdateAvailable`（A 注记⑦）——零足迹断言需按值清理或明文接受；②`LastUpdateCheck`/`UpdateAvailable` 在完整卸载后残留（契约「B 不触碰」vs 零残留口径，A 侧无卸载挂载点）——裁决归属；③含 `..` 登记值的词法/归一化残余差异（D-R1 残余段）；④DataParentDirectory 经设置窗改址后不回写（D-R5），预填可能停留旧值。

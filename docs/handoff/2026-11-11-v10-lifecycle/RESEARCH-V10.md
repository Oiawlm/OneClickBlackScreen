# RESEARCH-V10 · 覆盖更新/远端推送/自动检测/数据目录/安装目录/完整卸载——事实底稿与定案

> **一句话定位**：V10 轮唯一事实源——六需求（覆盖更新、远端更新推送、更新自动检测、数据目录自定义、自定义安装目录、完整卸载）的全部事实、fts 参照机制、外部最佳实践、用户定案与跨轨契约；GOAL 与本文件冲突时以本文件为准。　**来源**：用户 2026-11-11 /goal 需求＋一轮对齐（§6）＋规划窗双子代理调研（fts 仓库机制查证＋外部开源实践调研，26+26 次工具调用留档于本会话）。　**日期**：2026-11-11　行数预算：≤190 行。

## 1. 需求拆解（用户原话→可执行定义）

| # | 用户需求 | 可执行定义 |
|---|---|---|
| ① | 覆盖更新 | 新版本安装不丢数据、不留旧文件；安装形态走 Setup 静默覆盖重装（fts 模式），便携形态维持 exe 自替换 |
| ② | 远端更新推送 | GitHub Releases 作更新源（现状渠道），用户端无需打开设置窗即可感知新版（=④的检测面），发布链资产集不变 |
| ③ | 数据目录自定义 | 数据根默认 `%LOCALAPPDATA%\BlackScreen`，用户可改（设置窗＋安装向导两入口），迁移复制校验后原子切换 |
| ④ | 更新自动检测 | 检测全自动（打开设置窗即查＋启动遮罩时隔天静默查一次），发现新版＝遮罩低调提示＋设置窗预下载就绪 |
| ⑤ | 自定义安装目录 | 向导目录选择页（现状已有）＋InstallDirectory 归属登记＋卸载器归属守卫（防旧位置卸载器误删） |
| ⑥ | 完整卸载 | 卸载后机器零残留：程序文件＋快捷方式＋计划任务＋数据根＋HKCU 登记值全部清除，不询问（fts 同款＋守卫） |
| ⑦ | 并行高效/每需求提示词/模型选型/禁补丁 | 编排见 PLAN-V10 §2/§3；选型见 §3 D11 |

## 2. 事实底稿

### 2.1 本项目现状（v1.1.0，c3face3）

- **更新链（手动＋半自动）**：`src/Updater.cs` 手写 BCL 更新器——`Check()` 读 releases/latest（`AppInfo.RepoOwner/RepoName/UpdateApiBase`，资产名 `OneClickBlackScreen.exe`）、digest/SHA256SUMS 双通道哈希 fail-closed、`ReplaceSequence()` rename→.old→copy 自替换（`Global\BlackScreen.Update` 互斥＋回滚链＋`SyncUninstallVersion` ARP 联动）、`DeleteStaleBackup()` 启动清 .old。设置窗页脚「检查更新/立即更新」（`SettingsDialog.cs:668-777`），**打开设置窗已静默检查一次**（:13,:671）。**U2 边界：只有设置窗上网**（使用说明.txt:96 隐私口径 v6 承诺「只有打开设置窗口才会联网检查」）。
- **安装器（V9）**：`setup/OneClickBlackScreen.iss` per-user（`PrivilegesRequired=lowest`、`DefaultDirName={autopf}\一键黑屏`、AppId `{{5A17668F-45A0-48F0-B5B1-067E37EFA98D}` 与 Updater.cs:204 互证）；向导目录页开放（未设 DisableDirPage→⑤现状已可自定义）；同 AppId 覆盖装＝Inno 原生平滑升级（UsePreviousAppDir 记忆目录）。**无 [Run] 段**（装完不启动）、**无 AppMutex**、**无数据目录概念**。
- **卸载现状**：`[UninstallRun]` 只删计划任务 `OneClickBlackScreen-Idle`；`UninstalledAll` 文案明确**不删** `%LOCALAPPDATA%\BlackScreen`（提示手动删）——⑥要改掉的点。
- **数据存储**：`src/MaskConfig.cs:45-52` SettingsPath 写死 `%LOCALAPPDATA%\BlackScreen\settings.ini`（color/text/idle 三键，原子写）。
- **进程形态**：遮罩（Local\BlackScreen.Mask 单实例互斥，Program.cs:28）／设置窗（允许多开）／--idle 计划任务每分钟（只读配置+ IdleWake 判断，不上网）。**无常驻进程**→fts 的常驻调度器在此不可移植，检测只能挂「打开设置窗」与「启动遮罩」两时机。
- **验证**：verify.cmd 七件（56 PASS），VerifyUpdate=离线全链路（HttpListener mock+TEMP 替换彩排）；隔离自测=ISCC `/DTestId` 测试 AppId＋TEMP 目录（V9 D5）。
- **发布**：Release 四资产＝Setup-{ver}.exe＋OneClickBlackScreen.exe（裸 exe，便携/更新两用）＋zip＋SHA256SUMS；tag v{x.y.z}；版本单源 `src/AppInfo.cs`。

### 2.2 fts 参照机制（悬浮中转站 1.72.0，机制地图要点）

- **覆盖更新**：下载 Setup→SHA256SUMS 校验→排空保存管线→`/SILENT /SUPPRESSMSGBOXES /NORESTART`（+/LOG）启动安装器→应用退出；.iss `AppMutex` 探测＋`InitializeSetup` taskkill＋`[Run]` 无 skipifsilent（静默装完自动重启应用）。
- **远端推送**：releases/latest JSON＋tag 剥 v 严格比较＋资产精确名（Setup-{ver}.exe+SHA256SUMS.txt）＋下载后强制校验（哈希不过绝不执行）。
- **自动检测**：30s 首检＋24h 周期＋2h 空闲三触发收敛；**发现即全自动装**；头部按钮双态＋状态条，零弹窗；唯一可配置＝自动检查开关（默认开）；失败完全静默（落文件日志）。
- **数据目录**：数据根=<所选父>/悬浮中转站（受管形状）；HKCU 登记 DataDirectory＋DataParentDirectory；设置页＋安装向导（CreateInputDirPage）两入口；迁移=暂存复制→逐项校验→同卷原子改名→登记提交→自动重启→下次启动清旧目录；应用侧与安装器侧形状判定**同构互锁**。
- **安装目录**：InstallDirectory 归属登记（uninsdeletevalue）；归属守卫=旧版无登记必须先原地更新一次才可迁址；卸载器非当前安装位置直接拒绝执行。
- **完整卸载**：卸载删程序文件＋受管数据根（**不询问自动删**）＋Run 值＋登记值；三重守卫（位置归属/形状/绝不碰父目录与同级文件）；删除失败仅提示手动清理。

### 2.3 外部最佳实践（官方文档核对，来源 URL 留档于规划会话）

- GitHub API：无 token 60 req/h/IP；**304 免计限额无官方保证**（现行文档把豁免绑 Authorization）→按「低频（≤1 次/24h）＋403 静默降级」设计；版本以 tag_name 为单源，勿从文件名反解。
- Inno 覆盖装：同 AppId 直接覆盖（官方推荐）优于卸旧再装；`/VERYSILENT /SUPPRESSMSGBOXES /NORESTART`（+`/FORCECLOSEAPPLICATIONS`）；CloseApplications 默认 yes（Restart Manager）。
- 更新框架：Velopack 毁 Inno 链（过度设计）、WinSparkle/Squirrel 不适配（C++/失修）、AutoUpdater.NET 可用但**<200 行自写最贴合**（本项目已有手写 Updater，延续）。
- 卸载：`[UninstallDelete]` 无条件删不适合动态数据；`CurUninstallStepChanged` 是官方惯例位；[UninstallRun] 在文件删除**前**跑；`[Registry]` 值须带 uninsdeletevalue 才随卸载清。
- 目录惯例：per-user＋`%LOCALAPPDATA%` 数据主流；「程序文件不写数据、数据不进安装目录」；portable 判别=exe 旁 marker 文件。
- 社区共识：提示优于强制（FileZilla 事件后哈希校验是底线）；卸载残留折中=「询问默认保留」——**用户本轮明确要求零残留→走 fts 自动删＋守卫路线**。

## 3. 定案（D-xx；执行窗不得重议）

- **D1 更新执行模式＝预下载＋一键秒装**（用户 2026-11-11 定案）：检测全自动（设置窗打开即查＋启动遮罩时隔天静默查一次）；发现新版→遮罩角落低调提示一行＋设置窗后台预下载并 SHA-256 校验待用；用户点「立即更新」→瞬间安装→自动重启新版。下载耗时藏进后台，安装动作不打断正在改设置的窗口。
- **D2 覆盖更新双形态**：安装形态（HKCU Uninstall `{AppId}_is1` 键存在）＝下载 `OneClickBlackScreen-Setup-{tag剥v}.exe`→校验→`/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /FORCECLOSEAPPLICATIONS` 启动→应用自退出→Inno 记忆目录覆盖装→`[Run]`（无 skipifsilent）自动重启遮罩；便携形态（无登记）＝维持现有 exe 自替换链路原样。**Release 资产集不变**（四资产已含两用资产），仅更新器消费方式变。
- **D3 数据根单点＋受管形状**：新建 `src/AppPaths.cs` 作数据根解析唯一单点——默认 `%LOCALAPPDATA%\BlackScreen`；HKCU `Software\BlackScreen\DataDirectory` 存在且形状合法（以 `\BlackScreen` 结尾、非根）则用登记值；`MaskConfig.SettingsPath` 改走该单点。登记值仅非默认时存在；DataParentDirectory 记用户所选父（仅供预填）。应用侧与安装器侧形状判定必须同构（§7 契约）。
- **D4 迁移管线**（设置窗入口）：预览（旧/新路径）→确认→复制数据根→逐文件校验→写登记→守卫式删旧（仅当旧目录形状合法且内容 ⊆ {settings.ini}；失败提示手动删）→提示重启生效并关闭窗口。安装向导入口：SelectDir 后「数据位置」页（CreateInputDirPage，默认 %LOCALAPPDATA%，校验非根/可写探针），用户改过才写登记。
- **D5 完整卸载＝自动删不询问**（用户需求⑥明确）：`CurUninstallStepChanged(usUninstall)` 删数据根（读 DataDirectory，无登记用默认；形状守卫同构）＋登记值（[Registry] 带 uninsdeletevalue）；ConfirmUninstall 文案写明「将同时删除个人设置数据」；保留现有 [UninstallRun] 计划任务清理。
- **D6 卸载器归属守卫**（fts 模式）：`[Registry]` 写 `InstallDirectory`（=安装目录，uninsdeletevalue）；`InitializeUninstall` 校验卸载器所在目录==登记值，不符拒绝执行（防旧位置残留卸载器误删新位置数据）。
- **D7 自动检测挂载与状态**：①设置窗 OnLoad 静默检查（现状保留）→发现新版→后台预下载校验（暂存 `%TEMP%\BlackScreenUpdate\`，先清旧）→「立即更新」亮起；②遮罩启动→读本地标记（HKCU `UpdateAvailable`=tag，读必成、离线可用）→显示提示行；同时距 `LastUpdateCheck`≥24h 才后台 Check（超时约 3s、失败/403 静默）并更新两个值；③成功更新后清 UpdateAvailable。**U2 边界随此放宽**：遮罩进程允许一次节流 GET（后台线程，不阻塞显示）；使用说明隐私口径同步改写（⑦=R 轨）。
- **D8 遮罩提示行为纯展示**：右下角低调小字（如「检测到新版本 vX.Y.Z」，文案轨内定）；不可交互（遮罩鼠标退出机制不变）；VerifyOverlay 增状态注入断言。
- **D9 轨道划分＝写面零相交**：A 轨=src/**＋verify/**＋verify.cmd；B 轨=setup/**；R 轨=合并复检＋README/使用说明 行为文档；I 轨=发版（版本联动/tag/Release/真机全生命周期冒烟）。跨轨接口全部冻结于 §7。
- **D10 版本＝1.2.0**；I 轨写面必须含 `verify/VerifyUpdate.cs` 版本表联动（V9 D-1 教训：AppInfo bump 后版本用例随版本走）。
- **D11 模型选型**：四轨主模型全 **GLM-5.3**（A=更新链重构+数据层新建的深推理；B=.iss Pascal [Code] 精密守卫；R=证据审计；I=发版纪律）。**本轮无必须识图场景**——UI 沿用本项目 WinForms/ThemePalette 既有体系，fts 风格以 fts-style-system skill 文字规范（哲学六条+token 档位）为据，无需读图；Flash 仅限各窗口内机械子代理（grep/锚点核对），沿 V8/V9 惯例。
- **D12 V9 批次已归档**（本规划窗执行）：`git mv` 至 `docs/handoff/archive/2026-11-05-v9-installer/`，README.md:131 与 lessons.md L-017 出处已改全路径。

## 4. 各轨设计概要（细节见各 GOAL）

- **A 轨（应用侧）**：AppPaths 新建（含 HKCU 读抽象，供验证器注入隔离）→MaskConfig 挂接→Updater 双形态改造（Check 双资产解析/形态判定/预下载暂存/Setup 启动器；ReplaceSequence 原样保留）→SettingsDialog（数据位置区＋更新区预下载状态机）→OverlayForm 提示行→Program 遮罩挂载→verify 扩展（VerifyUpdate 双资产+命令行构造断言；VerifySettings 镜像扩展；VerifyOverlay 提示行；新 VerifyPaths=解析矩阵+迁移 TEMP 演练，HKCU 用测试键名自清零残留）→verify.cmd 八件。
- **B 轨（安装器侧）**：.iss 加 AppMutex/CloseApplications/`[Run]`（无 skipifsilent，测试变体抑制）/「数据位置」向导页/InstallDirectory+DataDirectory 登记（uninsdeletevalue）/卸载数据删除+归属守卫+文案；隔离自测扩展（装→改数据父目录→覆盖更新→数据保留断言→卸载→零残留扫描，全走测试 AppId）。
- **R 轨（复检预集成）**：feature/review-v10 两次 `--no-ff` 合并零冲突；A/B 产物对照 §7 契约逐项核（键名/形状/互斥/参数/资产名双侧同构）；合并树七件全绿＋B 隔离自测复跑；README/使用说明 行为文档改写（更新链/数据目录/卸载/隐私口径四处）。
- **I 轨（集成发版）**：合并 review-v10→AppInfo 1.2.0＋VerifyUpdate 版本表联动→build+Setup→verify×2 全绿→零足迹三态→tag v1.2.0→Release 四资产→**真机全生命周期冒烟**：装 v1.1.0（现有 Release）→改数据目录→应用内更新到 v1.2.0（预下载+秒装+重启断言）→完整卸载→全盘零残留扫描（安装目录/数据根新旧两处/HKCU 两键/计划任务/开始菜单/桌面）→清理 worktree/分支→VERIFY-V10.md。

## 5. 风险与预案

| 风险 | 预案（写入对应 GOAL） |
|---|---|
| 遮罩被网络检查拖累 | Check 走后台线程+短超时+全静默失败；显示路径只读本地标记（D7 两层分离） |
| Setup 启动后自退出失败/卡死 | 启动前排空（保存完成）；Process.Start 失败=更新失败回显；I 轨冒烟覆盖 |
| 双形态判定错（键存在但目录已手动删） | 形态判定只认 HKCU 键存在性；Setup 装回 Inno 记忆目录，无目录则默认——语义安全 |
| 迁移中断电/失败 | 复制→校验→登记的顺序保证旧目录完整时登记未动；失败清暂存提示，旧数据无损 |
| 卸载误删用户数据 | 三重守卫（归属/形状/登记同构）＋ConfirmUninstall 知情文案；B 轨自测断言「改父目录后卸载只删 BlackScreen 子目录」 |
| HKCU 测试污染真实键 | VerifyPaths 全走 `-VERIFY-TEST` 后缀键名＋finally 自清；套件前后快照三态留档 |
| A/B 契约漂移 | §7 冻结表＋R 轨逐项核对＋B 自测断言用 A 的常量值 |
| 编码破坏（BOM/CRLF） | .iss/使用说明 保真编辑＋`file` 复验（L-006） |
| 并发更新/互斥回归 | ReplaceSequence 互斥链路零改动；新增 Setup 路径 rides 同一 `Global\BlackScreen.Update` 互斥语义（A 轨设计内） |

## 6. 对齐记录（2026-11-11，一问一答）

1. 检测到新版本后下载与安装怎么执行？→ **预下载＋一键秒装（推荐项）**：检测全自动、下载藏后台、安装一键瞬间完成——兼顾 fts 的「无感升级」与本工具「无常驻进程、不打断设置窗」的形态。

## 7. 跨轨契约（A/B 并行接口冻结表；R 轨逐项核对）

| 契约项 | 值 | A 侧（src） | B 侧（.iss） |
|---|---|---|---|
| HKCU 键根 | `Software\BlackScreen` | AppPaths 读写 | [Registry] 声明 |
| DataDirectory | REG_SZ，数据根完整路径；形状＝以 `\BlackScreen` 结尾且非根；仅非默认时存在 | 迁移写/读+回退默认 | 向导页用户改过才写；卸载读（无登记用默认） |
| DataParentDirectory | REG_SZ，用户所选父目录（预填用） | 设置页预填读 | 向导页写 |
| InstallDirectory | REG_SZ＝`{app}`，uninsdeletevalue | 只读不写 | 安装写；InitializeUninstall 归属校验 |
| LastUpdateCheck / UpdateAvailable | REG_SZ（FILE_TIME/ISO 或 tag 串） | 遮罩节流检查读写 | 不触碰 |
| AppMutex | `Local\BlackScreen.Mask`（现有，Program.cs:28） | 不变 | AppMutex 指令＋InitializeSetup taskkill 探测 |
| Setup 静默参数 | `/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /FORCECLOSEAPPLICATIONS` | Updater 构造 | 行为实现（无 UI、无重启、强关） |
| 更新资产名 | `OneClickBlackScreen-Setup-{ver}.exe`（ver=tag 剥 v，与 build-setup.cmd 产物名一致） | Check 双资产解析 | OutputBaseFilename 不变（已一致） |
| 预下载暂存 | `%TEMP%\BlackScreenUpdate\`，先清旧再落 | Updater 暂存管理 | 不触碰 |
| 形状判定同构 | 「以 `\BlackScreen` 结尾＋非根」两侧逐字同实现 | AppPaths 内 | IsManagedDataDirectory 内；R 轨对照 |
| 测试变体隔离 | `/DTestId` 测试 AppId（现有 D5 机制）＋数据键加 `-TEST` 后缀 | VerifyPaths 键名后缀 | 自测数据页/卸载断言走测试键 |

## 8. 调研来源附录（核心 18 条；fts 侧证据在其仓库文件行号，见 §2.2）

- GitHub API 限流/条件请求/releases-latest：docs.github.com/en/rest/using-the-rest-api/rate-limits-for-the-rest-api；…/best-practices-for-using-the-rest-api；…/rest/releases/releases；github.com/community/community/discussions/24532
- Inno Setup 官方：jrsoftware.org/ishelp/topic_setupcmdline.htm（/VERYSILENT //SUPPRESSMSGBOXES //NORESTART 语义）；…/topic_setup_closeapplications.htm；…/topic_setup_appid.htm（同 AppId 覆盖装）；…/topic_setup_usepreviousappdir.htm；…/topic_registrysection.htm（uninsdeletevalue）；…/topic_scriptevents.htm（CurUninstallStepChanged）；…/topic_uninstalldeletesection.htm（无条件删除警示）；jrsoftware.org/isfaq.php（覆盖优于卸旧再装）；github.com/HeliumProject/InnoSetup/blob/master/Examples/UninstallCodeExample1.iss（官方卸载示例）
- 更新框架对比：github.com/ravibpatel/AutoUpdater.NET；github.com/velopack/velopack；github.com/vslavik/winsparkle；github.com/Squirrel/Squirrel.Windows（失修排除）
- 目录/portable 惯例：code.visualstudio.com/docs/setup/portable；superuser.com/questions/1827168；advancedinstaller.com/appdata-localappdata-programdata.html
- 社区共识：news.ycombinator.com/item?id=41474419（提示优于强制）；reddit.com/r/Windows11/comments/1qshxpa（卸载残留讨论）

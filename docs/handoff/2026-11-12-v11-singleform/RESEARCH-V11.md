# RESEARCH-V11 · 单一 Setup 形态＋双 EXE 免参数入口＋收官清欠——事实底稿与定案

> **一句话定位**：V11 轮唯一事实源——五需求（单一分发形态照 fts、双 EXE 免参数设置入口、安装目录确切路径文档化、卸载零残留兑现、V10 提请五项全量根修）的全部事实、fts 实证、用户定案与跨轨契约；GOAL 与本文件冲突时以本文件为准。　**来源**：用户 2026-11-12 /goal 追加定案（§6）＋规划窗双子代理调研（fts 仓库实证＋本仓 file:line 底稿，2026-11-12）。　**日期**：2026-11-12　行数预算：≤185 行。

## 1. 需求拆解（用户原话→可执行定义）

| # | 用户需求 | 可执行定义 |
|---|---|---|
| ① | 「什么安装版、便携版，我只要一个版本，悬浮中转站是哪种版本，你就做哪种版本」（2026-11-12） | 分发形态收敛为**唯一 Setup 安装包**（fts 实证同构，§2.1）：Release 资产恰两件＝Setup＋SHA256SUMS.txt；便携 zip／裸 exe 资产退役；全部便携叙述出文档 |
| ② | 原始目标「做成两个 EXE。不要让我自己去修改什么参数，才能进入设置界面」 | `一键黑屏.exe`（遮罩）＋`一键黑屏-设置.exe`（设置）双 EXE，双击即达、零参数；安装版设置快捷方式直指设置 EXE |
| ③ | 原始目标「安装目录在哪里？我到现在都没找到」 | README／使用说明写明确切默认路径（`%LOCALAPPDATA%\Programs\一键黑屏`）与查询途径（设置>应用/向导页） |
| ④ | 「全量收官」（2026-11-12） | V10 提请五项全部根修（VERIFY-V10 §8）：卸载残留键、ARP 版本碰撞、`..` 词法守卫、DataParentDirectory 回写、VerifyGate 文案、D-R7 覆盖、⑤内打磨/否决项 |
| ⑤ | 两层架构／并行／提示词／模型选型（原始目标） | 编排见 PLAN-V11（沿 V8~V10 已实证模式，不重议） |

## 2. 事实底稿

### 2.1 fts 分发形态实证（2026-11-12 子代理实测；仓库 `D:\Download\Agent Vault\悬浮中转站\悬浮中转站-main`，GitHub `Oiawlm/floating-transfer-station`）

- **仅安装包一种形态**：Release v1.72.0 资产恰两件＝`FloatingTransferStation-Setup-1.72.0.exe`＋`SHA256SUMS.txt`（gh 实测）；全仓 grep `便携|绿色|portable|zip|免安装` **零命中**（仅 NuGet 缓存噪音）；本机 `D:\App\悬浮中转站` 即安装产物（含 unins000.exe）。
- **per-user 同构**：`DefaultDirName={localappdata}\Programs\悬浮中转站`（.iss:18）、`PrivilegesRequired=lowest`（:22）；产物出生即发行名（`OutputBaseFilename` :29，禁止人工改名）。
- **资产名四方同源契约**（其 #26 修订①，v1.71.0 人工改名事故教训）：构建产物名＝更新器精确名＝README 链接名＝SHA256SUMS 条目名，单源常量 `UpdateService.cs:120`，构建脚本正则断言（build-release.ps1:81-83）＋跨语言测试互锁（design-contracts.md:40,280-282）。
- **更新链**：releases/latest 单点→精确名 Setup→SHA256SUMS.txt 强制校验→`/SILENT /SUPPRESSMSGBOXES /NORESTART`→应用退出→[Run] `nowait` 无 skipifsilent 自动重启（.iss:54-55）；SUMS 格式 `<64位小写hex>␣*<文件名>`（build-release.ps1:121-131）。
- **卸载**：usUninstall 删登记内容目录＋清登记值，失败仅提示手动清理（.iss:894-911）；InitializeUninstall 归属守卫（:858-865）——本项目 V10 已同构。
- fts 设置入口＝主界面齿轮按钮（常驻 UI 工具，形态不同**不可移植**）——本项目免参数设置入口的对应解＝双 EXE（需求②）。

### 2.2 本仓现状（v1.2.0，bad8de0）

- **构建**：`build.rsp` 单目标（`-out:dist\一键黑屏.exe`、`-win32icon:assets\icon-main.ico`、`-resource:assets\icon-settings.ico,settings.ico`、无 `/main`，11 个 src\*.cs 逐列）；`build.cmd:24` glob `dist\*.exe` 定位产物（dist 常驻 3 exe，**靠码页排序巧合取对**＝隐性缺陷）；:30 复制 app.config→`%FOUND%.config`、:36 复制 icon-settings.ico→dist。MakeIcon 只产 `assets\icon-main.ico`/`assets\icon-settings.ico`。
- **Setup 资产名模板三处分散**（fts 四方同源的反例）：`Updater.cs:159-164`（SetupAssetNameFor）＋`.iss:71`（OutputBaseFilename）＋`build-setup.cmd:47`（期望产物名）；AppInfo 无此常量。
- **更新链**：Check() 硬性要求裸 exe 资产（`Updater.cs:95-96`，资产退役后老版报错＝诚实失败，1.2.0 便携态迁移见 D1）；形态分叉＝`DownloadStaged:211-212`＋`SettingsDialog.cs:810`；便携自替换链＝ReplaceSequence(:349-396)/SyncUninstallVersion(:412-425)/InstalledVersionOf(:433-442)/DeleteStaleBackup(:451-471，Program.cs:17 调用)/DownloadAndVerify(:105-131)/DownloadAndApply(:135-146)/IsInstalledForm(:169-194)。
- **ARP 版本碰撞（提请④根因）**：两版 exe AssemblyVersion 均 csc 默认 0.0.0.0，旧进程 `Assembly.LoadFrom(新exe)` 命中同名程序集身份读回旧字面量→DisplayVersion 写旧值（V10 S3 不跳版/S4 污染用户 ARP 双实证）。
- **卸载残留（提请①）**：.iss 卸载面只删 DataDirectory/DataParentDirectory（:635-636）＋InstallDirectory（uninsdeletevalue :156）；**LastUpdateCheck/UpdateAvailable（AppPaths.cs:24-25，CompleteUpdateCheck :165-171 唯一写者）无人清，`Software\BlackScreen` 空键遗留**——README:58「卸载后无残留」公开承诺当前不成立。
- **形状守卫词法差异（提请①）**：`AppPaths.IsManagedDataRoot`(:100-109) 不展开 `..`（`C:\x\..\BlackScreen` 通过）；.iss `IsManagedDataDirectory`(:327-352) 经 ExpandFileName 展开——两侧判定可分歧。
- **CommitDataRoot 单值写（提请①）**：(:193-200) 只写/删 DataDirectory，不回写 DataParentDirectory（唯一写者＝.iss [Code]:526）——设置窗改址后父登记停留旧值。
- **入口现状（需求②缺口）**：Program.cs 单 Main 分派（:18-23）；免参数设置入口仅安装版快捷方式（.iss:174 `--settings`＋IconFilename）——**便携 zip 无任何免参数入口**（README:67/使用说明:31）。
- **验证面**：verify.cmd 已八件（:38-69）但 `VerifyGate.cs:4` 头注释仍 seven（提请②）、README:120 仍七件套；VerifyUpdate.cs 666 行三阶段（版本表 :67-68 断言 {1,2,0}／全链 mock :102-303 含 TEMP 替换彩排＋ReplaceSequence 竞争 50 轮／双形态 :310-443）；`verify.cmd:12` TARGET 同 glob 歧义。
- **文档债（需求③＋提请项）**：README:44/使用说明:25 只写「当前用户目录」无确切路径；README:131 将 v9 标「当前批次」已过期两轮；build-setup.cmd:12 注释仍指 V9B；便携叙述行号＝README L6/27/45/46/48/67/77/106/108、使用说明 L26/31/68/99/117。
- **用户实装态（I 轨素材）**：`D:\App\一键黑屏` 1.2.0＋数据 `D:\Download\App Data\BlackScreen`（三登记齐）＝真实原地升级场景。

## 3. 定案（D-xx；执行窗不得重议）

- **D1 单一形态＝Setup（用户定案，fts 同构）**：Release 资产恰两件＝`OneClickBlackScreen-Setup-{ver}.exe`＋`SHA256SUMS.txt`（含 .txt，与 fts 同名）；zip/裸 exe 资产退役。**1.2.0 便携态一次性迁移**＝文档指引下载 Setup 直装（老版 Check 因裸 exe 资产缺失报错属诚实失败；无登记者数据在默认根无缝继承，有登记者向导预填）。
- **D2 双 EXE 免参数入口（原始诉求②字面兑现）**：`一键黑屏.exe`＝遮罩（保留 `--settings`/`--idle`/用法框分派——旧快捷方式与计划任务兼容，`一键黑屏-设置.exe`＝设置（忽略任何参数直接开设置窗）；共享全部源码、两目标编译（各自 `/main`、各自内嵌图标：main↔icon-main.ico、settings↔icon-settings.ico）；**icon-settings.ico 退出 dist/打包面**（assets 仍为构建输入）；安装版「一键黑屏-设置」快捷方式改直指设置 EXE（去 `--settings`、去 IconFilename）。
- **D3 资产名四方同源（fts #26 教训）**：`AppInfo.cs` 增 `SetupAssetNamePrefix="OneClickBlackScreen-Setup-"`＋`ChecksumsAssetName="SHA256SUMS.txt"`，删 `UpdateAssetName`；`Updater.SetupAssetNameFor` 改引用常量；.iss OutputBaseFilename 与 build-setup.cmd 期望名与之互锁（§7 表）；SUMS 条目格式 `<64位小写hex>␣*<文件名>`。
- **D4 更新链单形态根修（提请④随之根除）**：删便携自替换链全部（§2.2 所列八件＋SettingsDialog:810-856 便携分支）；Check 恒要求 Setup 资产（缺失＝报错）；预下载暂存/`Global\BlackScreen.Update` 互斥/LaunchSetup 静默链保留；`.iss [InstallDelete]` 增 `{app}\*.old`——1.2.0 便携更新遗留 .old 由安装器一次性回收，应用侧不留清理代码。ARP DisplayVersion 由此恒由 Inno 重装写入，碰撞根因消除。
- **D5 卸载零残留兑现（提请①）**：usUninstall 增删 `LastUpdateCheck`/`UpdateAvailable`＋`Software\BlackScreen` 键空则删键（测试变体根同构）；README/使用说明「无残留」承诺由此成立。
- **D6 形状守卫词法同构（提请①）**：登记值 trim 后含 `..`→两侧（AppPaths.IsManagedDataRoot／.iss IsManagedDataDirectory）一致拒绝——不展开、不归一化救回（沿 V10 REVIEW「叶检查移原始 trim 串」裁决收尾）；父须存在且非盘根/共享根/无盘符（现状保留）。
- **D7 CommitDataRoot 对称登记（提请①）**：设置窗改址提交＝非默认根写 DataDirectory＋DataParentDirectory（＝所选父），回默认两值全删——与 .iss:518-528 行为同构（§7 表）。
- **D8 八件套文案对齐（提请②）**：VerifyGate.cs:4 seven→eight；README:120 七件→八件。
- **D9 构建产物显式命名**：build.cmd 两目标显式（`一键黑屏.exe`/`一键黑屏-设置.exe`＋各自 .config），删 glob 撞名面＋断言四件齐；verify.cmd TARGET 显式（遮罩/idle 类件→主 exe，设置类件→设置 exe）。
- **D10 D-R7 默认根卸载真实构建覆盖（提请③）**：B 轨隔离自测补「无登记→默认根数据→卸载→默认根删除」场景。**安全机制升级**：V10 测试变体「编译排除默认根删除」改为「测试变体经 ISCC `/D` 把默认数据根重定向到 %TEMP% 隔离路径」——删真实 `%LOCALAPPDATA%\BlackScreen` 在构造上仍不可能，但默认根删除代码路径在测试构建中**保持活跃**＝D-R7 真正可覆盖（生产构建默认根不变）。
- **D11 版本＝1.3.0**（形态＋入口变更，语义化 minor）；I 轨唯一版本改动点；VerifyUpdate 版本表镜像 {1,3,0}、前版 1.2.0 转 older 用例（V9 D-1 纪律沿袭：src/ grep `1\.2\.0` 硬零，VerifyUpdate 存留恰 2 处＝older＋malformed 用例）。
- **D12 提请⑤处置**：ETag/304 **否决**——检查频率≤2 次/日＋开窗即查，GitHub 无 token 限额 60 req/h/IP 余量两个数量级，且 304 豁免无官方保证（V10 §2.3），引入纯增复杂度；遮罩提示行文案「检测到新版本 vX.Y.Z」现状已足，R 轨文档口径统一收口；安装向导文案用户测试并入 I 轨真机冒烟观察项。
- **D13 模型选型**：四轨主模型全 **GLM-5.3**（A＝入口拆分＋更新链根修＋验证器重构的深推理；B＝.iss Pascal 守卫精密；R＝证据审计＋行为文档改写；I＝发版纪律）。**识图判断：本轮无必须识图场景**——UI 零视觉新增，文档改写纯文字（沿 V10 D11 结论）；若 I 轨冒烟需截图视觉验收，当次指定 Flash 会话执行（原生多模态）。Flash 仅各窗口内机械子代理（grep/锚点核对）。
- **D14 V10 批次归档（本规划窗执行，沿 D12 惯例）**：`git mv` 整目录入 archive/；全仓活引用面已证空（指向该路径的引用全在批次内部）；README:131 过期指针属 R 轨写面，本轮修。

## 4. 各轨设计概要（细节见各 GOAL）

- **A 轨**（应用＋构建＋验证，分支 `feature/app-v11`）：P1 AppInfo 资产常量｜P2 双 EXE 入口＋build 双目标显式化｜P3 Updater 单形态化（死代码根除）｜P4 AppPaths 词法守卫＋对称登记｜P5 SettingsDialog 更新分支合一｜P6 verify 重构（VerifyUpdate 阶段2/3、VerifyPaths 增断言、VerifySettings 切目标、VerifyGate 文案、verify.cmd 显式分派）。
- **B 轨**（安装器，分支 `feature/installer-v11`）：P1 [Files] 四件＋[InstallDelete] *.old｜P2 设置快捷方式直指｜P3 卸载清扫两值＋空键｜P4 默认根重定向机制＋D-R7 场景｜P5 build-setup.cmd 注释＋产物名互锁。**并行脚手架**：B 分支上 A 轨产物未合——自测用 csc 现编两个占位 winexe＋空 config 凑齐 §7 表 4 行四件（占位件放 worktree `dist\`，.gitignore 已挡、不入提交，仅本地供 ISCC）；build-setup.cmd 全链绿验收归 R 轨合并态。
- **R 轨**（复检＋预集成＋行为文档，分支 `feature/review-v11`）：双 --no-ff 合并＋§7 契约逐行核对＋build-setup 全链绿＋verify 八件全绿＋README/使用说明 单形态大改写（含需求③确切路径、README:131 指针、1.2.0 便携迁移指引）。
- **I 轨**（发版 v1.3.0＋真机冒烟，主检出直改）：版本联动→tag→Release 两资产→真机五场景（用户实装原地升级／设置 EXE 双入口／D-R7 真机／1.2.0 便携迁移实演／隔离完整卸载零残留）→清理。

## 5. 风险与预案

| 风险 | 预案 |
|---|---|
| 双 EXE 两编译→逻辑复制 | 入口差异仅 Main 分派三行（EnableVisualStyles/CompatibleTextRenderingDefault/RunSettings）；RunSettings 改 internal 共享，其余源码零分叉 |
| 旧「一键黑屏-设置」lnk（带 --settings 指主 exe）升级后失效 | D2 保留主 exe `--settings` 分派＝旧 lnk 兼容；计划任务 `--idle` 不变 |
| 1.2.0 便携用户更新报错体验 | 报错文案含资产名（诚实失败）；R 轨文档写一次性迁移指引（D1）；I 轨场景④实演 |
| B 轨占位 exe 自测失真 | .iss 断言面＝文件落位/快捷方式目标/登记/卸载清扫，均与 exe 内容无关；真实 exe 行为归 A verify＋I 真机；R 轨合并态全链绿补封 |
| csc 双 Main 冲突 | 两 rsp 各自显式 `/main:Program`／`/main:SettingsProgram` |
| verify.cmd 删 glob 后目标缺席 | 显式名＋启动断言四件齐（缺＝明示报错退出） |
| I 轨动用户实装（D:\App\一键黑屏 原地升 1.3.0） | 动前明示（沿 V10 S3 惯例）；用户实装**保留不卸**，完整卸载终验在隔离目录自装自卸 |

## 6. 对齐记录（2026-11-12，两问两答）

1. 「两个 EXE、免参数进设置」如何落地？→ 用户：**「什么安装版、便携版，我只要一个版本，悬浮中转站是哪种版本，你就做哪种版本」**→ 单一 Setup 形态（D1，§2.1 实证）＋双 EXE 入口随唯一形态落地（D2）。
2. V11 范围？→ 用户：**「全量收官（推荐）」**→ D4~D12 全量根修（提请五项清欠完毕）。

## 7. 跨轨契约（A/B 并行接口冻结表；R 轨逐项核对）

| # | 契约 | A 侧（src/verify/build） | B 侧（setup/） |
|---|---|---|---|
| 1 | Setup 资产名＝`SetupAssetNamePrefix`＋tag剥v＋`.exe` | AppInfo 常量＋Updater.SetupAssetNameFor 引用 | OutputBaseFilename 同式（:71 改） |
| 2 | 校验清单资产名＝`SHA256SUMS.txt`；条目 `<64hex>␣*<文件名>` | AppInfo.ChecksumsAssetName＋Check/SUMS 回退匹配 | （不涉；I 轨 Release 生成） |
| 3 | 静默参数＝`/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /FORCECLOSEAPPLICATIONS`（V10 实证冻结，不重议） | Updater.SetupSilentArguments | （不涉） |
| 4 | dist 产物恰四件＝一键黑屏.exe＋.exe.config＋一键黑屏-设置.exe＋.exe.config（icon-settings.ico **不**入 dist） | build.cmd 双目标＋断言 | .iss [Files] 打包此四件＋使用说明 |
| 5 | 设置 EXE 名＝`一键黑屏-设置.exe` | build 目标名 | AppSettingsExeName 常量＋[Icons] 设置快捷方式直指（无参数/无 IconFilename） |
| 6 | 数据登记＝HKCU `Software\BlackScreen` 五值（InstallDirectory/DataDirectory/DataParentDirectory/LastUpdateCheck/UpdateAvailable）；卸载清扫五值＋空键 | AppPaths（D6/D7） | usUninstall 清扫（D5；InstallDirectory 沿 uninsdeletevalue） |
| 7 | 形状判定同构＝trim 原始串含 `..`→拒；以 `\BlackScreen` 结尾（OrdinalIgnoreCase）；父存在且非盘根/共享根/无盘符 | IsManagedDataRoot | IsManagedDataDirectory（逐字同构，R 核对） |
| 8 | `.old` 过渡回收＝[InstallDelete] `{app}\*.old` | 删 DeleteStaleBackup（应用侧不留） | [InstallDelete] 实现 |

## 8. 调研来源附录

- fts 仓库（2026-11-12 子代理实测，路径见 §2.1）：.iss :18/:22/:29/:44-55/:858-865/:894-911；UpdateService.cs:120-121/:432；build-release.ps1:81-83/:121-131；design-contracts.md:40/:280-282；README.md:63-65；docs/releasing.md:5/:49；gh release v1.72.0 资产清单。
- 本仓 file:line：见 §2.2 各条（构建/资产名/更新链/卸载/守卫/入口/验证面/文档债）。
- 上轮档案：RESEARCH-V10 §2.2/§2.3/§7、VERIFY-V10 §5/§8（提请五项原文）、REVIEW-V10 裁决 D-R1~R7。

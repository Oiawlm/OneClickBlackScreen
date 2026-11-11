# RESEARCH-V9 · DeskBox 根除＋安装包分发——事实底稿与定案

> **一句话定位**：V9 轮唯一事实源——两条需求（清除本机个人习惯 DeskBox／制作可分发安装包）的全部事实、用户定案、设计参数与风险预案；GOAL 与本文件冲突时以本文件为准。　**来源**：用户 2026-11-05 /goal 需求＋两问对齐定案（§6）＋规划窗双子代理调研（代码机制查证＋安装包技术调研）。　**日期**：2026-11-05　行数预算：≤170 行。

## 1. 需求拆解（用户原话→可执行定义）

| # | 用户需求 | 可执行定义 |
|---|---|---|
| ① | 「deskbox 是什么…全部删除掉」 | DeskBox＝**本机个人的快捷方式启动器目录** `%USERPROFILE%\DeskBox\DeskBox`（非产品依赖）。仓库内与其相关内容全部根除，产物：全仓 `grep -ri deskbox` 归零 |
| ② | 「做一个安装包…要分发的…按这个系统适配」 | 制作 Inno Setup per-user 安装包：装完即含软件本体＋设置能力；目标＝同样 Windows 10/11 x64 的普通电脑开箱即用 |
| ③ | 两层架构／最高效并行／每需求一提示词／模型选型 | 编排定案见 PLAN-V9 §2/§3；本文件 §3 D6/D7 |
| ④ | 禁补丁式修改／最终简洁完整 | 全轮铁律，写入各 GOAL |

## 2. 事实底稿

### 2.1 需求①的事实（DeskBox 落点，2026-11-05 实测 49 处/16 文件）

- **install.ps1（整文件删除）**：唯一功能＝幂等同步两个 .lnk 进 DeskBox，指向仓库 `dist\一键黑屏.exe`——本机便利设施，与分发无关。
- **migrate/（冻结区，用户已授权手术）**：`迁移手册.md` 三处（:352 完成标志、:359 操作步骤、:495 完成标志）；`迁移手册.html` 对应处；`cleanup-local.ps1` 的 S2 节（:18 注释、:96-97 实现）——S2 整节仅服务 DeskBox，删节后注意节编号连续性。
- **docs/handoff/archive/ 11 份**（GOAL-V1×6处/V2×3/V5D×3、MIGRATE-V5D×6/V6-PC2×3、RESEARCH-V1×3/V5×2/V6×1、VERIFY-V1×1/V2×1）：历史过程记录中的 DeskBox 字样。
- **使用说明.txt:105**：提 install.ps1/DeskBox——该文件本轮归 **B 轨写面**（随安装文档整体改写消除），A 轨不动它。
- **README.md：0 处 deskbox**；有 install.ps1 提及（安装节）→ B 轨改写时消除。
- 编码事实（L-006 关联）：使用说明.txt/cleanup-local.ps1 为 **UTF-8 BOM**；迁移手册.md 无 BOM；均 CRLF——手术必须保真，改后 `file` 复验。
- **lessons.md 锚点安全实证**：坑库引用的 archive 出处文件（VERIFY-V6B/V7V/V4/V7/V7S/V6/V5、PUBLISH-V5A、REVIEW-V7、RESEARCH-V7 等）∩ 上列 11 份 DeskBox 文件＝**空集**（A 轨仍须复验留档）。

### 2.2 需求②的事实（分发形态）

- **构建形态**：非 net8/MSBuild——`C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe` 直编（build.cmd:5、build.rsp），**.NET Framework 4.x 单 exe**＋两伴生件（`一键黑屏.exe.config` DPI、`icon-settings.ico` 快捷方式图标）。Win10 1903+/Win11 内置 .NET FW 4.8 → **分发零运行时依赖**，无捆绑/自包含问题。
- **设置自愈**：默认值全在代码内（MaskConfig.cs:29-34、80-83），首启无 ini 即用纯黑默认，仅保存时才建目录落盘（:216-217）→「安装包含设置」由产品结构天然满足，安装器**不预置任何设置文件**（预置即第二事实源，违禁补丁原则）。
- **更新器约束（决定安装目录）**：就地 rename→.old→copy 替换 `Application.ExecutablePath`（Updater.cs:149-192，Global 互斥）→ 要求安装目录用户可写 → **per-user** 定案依据；装 Program Files 则非管理员 rename 必失败（:166-171）。
- **计划任务**：`OneClickBlackScreen-Idle` 由设置窗保存时创建/删除（SettingsDialog.cs:785-786），指向当前 exe 绝对路径——**安装器不建任务**（产品自管，职责单一）；卸载时应清理已存在的任务。
- **调研结论**（ShareX/KeePass 用 Inno；VS Code/Discord per-user；NSIS 有 CVE 史、WiX 过重、Velopack 毁单 exe 形态——均否决）：Inno Setup 6/7 活跃维护、免费可商用、`PrivilegesRequired=lowest`+`{autopf}` 原生 per-user、静默参数完备。来源清单存 VERIFY-V9B 附录（15 条 URL，含 jrsoftware/ShareX 实例/MS Learn）。
- **本机无 Inno Setup**（实测 PATH/Program Files 均无）→ 用户开 B 窗前一次性安装官方 Inno Setup 6（PLAN §5）。
- **发版链适配**：L-015 资产名必须 ASCII；更新器假定 Release 资产 `OneClickBlackScreen.exe`＋digest/SHA256SUMS（AppInfo.cs:12-14）——安装包为**新增**资产，不动更新器资产契约。

## 3. 定案（D-xx；执行窗不得重议）

- **D1 清除范围＝全部含冻结区**（用户 2026-11-05 授权）：手术原则＝仅动 DeskBox 相关行/节，其余内容零变化；全量 diff 留档供 R 复检。授权范围严格限 DeskBox 内容，越界即停。
- **D2 安装形态＝Inno Setup per-user**（用户定案）：参数表——`PrivilegesRequired=lowest`；`DefaultDirName={autopf}\一键黑屏`（解析到 `%LOCALAPPDATA%\Programs`）；固定 `AppId`（GUID 一次性生成写入 .iss）；安装内容四件＝exe＋exe.config＋icon-settings.ico＋使用说明.txt；快捷方式＝开始菜单「一键黑屏」＋「一键黑屏-设置」(`--settings`)、桌面主快捷方式默认不勾（opt-in）；向导中文化走 `[Messages]` 自定义核心文案（零外部 .isl 依赖）；卸载＝清已装文件＋快捷方式＋计划任务（`schtasks /delete` 容错），**不删** `%LOCALAPPDATA%\BlackScreen` 用户设置（完成页提示其位置）；同 AppId 覆盖安装＝平滑升级。
- **D3 更新器-ARP 联动**：Updater 替换成功后，若 HKCU Uninstall 键（Inno per-user 写 HKCU）存在则同步 `DisplayVersion`；键不存在＝no-op（未安装用户零影响，VerifyUpdate 绿即实证）。融入 ReplaceSequence 既有结构。
- **D4 版本与资产**：版本单源不动摇——`src/AppInfo.cs` 仅 I 轨 bump 至 **1.1.0**；安装包产物 `dist\OneClickBlackScreen-Setup-{版本}.exe`（ASCII，版本由 build-setup.cmd 从 AppInfo.cs 提取传 ISCC `/D`）；Release 资产＝Setup＋`OneClickBlackScreen.exe`（更新器契约不变）＋便携 zip＋SHA256SUMS。
- **D5 验证策略**：七件套语义**零变化**（verify.cmd/verify/** 不动）；B 轨自证＝**测试变体隔离自测**（.iss 经 `/D` 测试 AppId＋`/DIR=%TEMP%`＋`/VERYSILENT`＋`/NOICONS` 安装→断言文件/HKCU 测试卸载键/无真实快捷方式→静默卸载→零残留，全程不触用户真实状态）；I 轨＝真机冒烟（真实安装→`--settings` 启动→卸载→零残留；settings.ini/计划任务三态不变）。
- **D6 写面零相交**（A∥B 并行依据）：A＝install.ps1(删)/migrate×3/archive 仅 DeskBox 行/lessons 追加一条；B＝setup/(新)/build-setup.cmd(新)/README/使用说明/Updater.cs。无共同文件。
- **D7 模型选型**：四轨主模型全 **GLM-5.3**——本轮无视图内容轨（README/使用说明为文本文档改写，非视觉设计）；A 冻结区手术精度、B 新构建链判断力均需强推理；Flash 仅限窗口内机械子代理（grep 扫描/锚点核对），沿 V8 惯例。
- **D8 V8 批次归档**：VERIFY-V8.md:87 尾注移交本窗执行——`git mv` 整目录入 archive/＋活引用改全路径（docs/README.md:3、docs/lessons.md:3）。

## 4. 各轨设计概要（细节见各 GOAL）

- **A 轨（DeskBox 根除）**：清点复核→install.ps1 删除→migrate 手术→archive 手术→lessons 追加 L-017（本机个人环境约定不得混入仓库；分发只走标准 Windows 集成）→grep 归零＋build 绿＋纯件三件。不触闸门（与 B 并行无争用）。
- **B 轨（安装包）**：ISCC 探测（缺失即停报告）→setup/OneClickBlackScreen.iss（D2 参数表）→build-setup.cmd（版本单源提取）→Updater ARP 联动（D3）→README/使用说明 改写（**先读 github-readme skill**；安装节 Setup 推荐＋zip 便携备选，消除 install.ps1/DeskBox 字样）→七件套全绿＋隔离自测实录。
- **R 轨（复检预集成）**：feature/review-v9＝main＋A＋B 两次 `--no-ff` 零冲突；A 档案手术逐处抽审、B 的 .iss 对照 D2 参数逐项核、写面白名单核对、死链复扫、七件全绿。
- **I 轨（集成发版）**：合并 review-v9→AppInfo 1.1.0→build＋Setup 构建→verify×2→零足迹三态→tag v1.1.0→Release 四资产→真机安装冒烟＋应用内冒烟→清理 worktree/分支→VERIFY-V9.md（发版惯例按 docs/README.md 路由读 archive 上一份 VERIFY-V?I）。

## 5. 风险与预案

| 风险 | 预案（写入对应 GOAL） |
|---|---|
| 冻结区手术改坏非目标内容 | 逐处 diff 全量留档；R 轨抽审；白名单外路径零 diff |
| 编码破坏（BOM/CRLF/GBK） | 手术用保真编辑；改后 `file` 复验逐文件留档（L-006） |
| lessons 锚点漂移 | 锚点文件∩手术文件＝∅ 实证；非空即停下报告 |
| ISCC 缺失/版本不符 | B 轨探测顺序 PATH→`Program Files (x86)\Inno Setup 6`→明示报错；不自动装软件 |
| 安装向导乱码/中文资源缺失 | `[Messages]` 自定义中文核心文案，零外部文件依赖 |
| 安装包体积/杀软误报 | .NET FW 零依赖＋LZMA2，预期 Setup≈0.5-1MB；SmartScreen 无签名现状 README 已自认，不在本轮范围 |
| html/md 双版本漂移 | A 轨同步手术，grep 双归零即达标 |

## 6. 对齐记录（2026-11-05，两问两答）

1. 冻结区 DeskBox 是否清除？→ **全部清除（推荐项）**：授权覆盖 AGENTS §4，手术式最小删改＋diff 留档。
2. 安装形态？→ **per-user 免 UAC（推荐项）**：Inno Setup、%LOCALAPPDATA%\Programs、更新器零改动、zip 便携保留。

## 7. 规划层并行说明（回应「两层架构」）

规划层并行已由本窗**双子代理并发调研**实现（代码机制查证∥安装包技术调研，结果即本文件 §2）——再开多个规划窗只会重复冷启动＋重复读同一仓库/网络，纯协调开销。执行层＝A∥B 双 worktree 并行→R→I，为本项目写面零相交约束下的最大并行度（分析见 PLAN §2）。

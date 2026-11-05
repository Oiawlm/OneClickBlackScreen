# RESEARCH-V5 · 一键黑屏 v5「发布 + 自动更新 + 空闲唤起 + 迁移」事实源

> **一句话定位**：v5 轮唯一事实源——用户四项对齐定案（U 系）、冻结面（§2）、三路调研结论（R 系）、并行契约（§4）、全量决策表（§5）；执行窗口按 §5 实现，与 GOAL-V5A/B/C/I/D-PROMPT.md 冲突时以本文为准。
> **来源**：用户 /goal 需求 + AskUserQuestion 四项对齐（2026-11-02）＋三个 ZCode general-purpose 子代理联网调研（自更新/计划任务与空闲/发布与迁移，含本机实测，来源清单见 §3 末）＋ src/verify/build 脚本通读。
> **日期**：2026-11-02　行数预算：≤250 行。**优先级**：用户冻结约束 ＞ 本文定案 ＞ GOAL 提示词 ＞ 上游 skill 原文。

## 1. 需求与定案（U 系）

| # | 定案 |
|---|---|
| U1 | **唤起语义＝空闲驱动**：无操作 N 分钟→自动弹遮罩（护眼/离开即黑屏），任意键退出并重新计时；档位 关/5/10/15/30 分钟，默认关（用户原话「类似无操作多长时间电脑就休眠一样，你自行判断」） |
| U2 | **更新＝设置窗内一键更新**：打开设置窗静默检查一次＋手动「检查更新」；发现新版→显示版本→一键下载→校验→自动替换；遮罩模式**永不联网**、双击即遮罩冻结不变 |
| U3 | **仓库**：`Oiawlm/OneClickBlackScreen` 公开，MIT（英文原版 LICENSE），gh 已登录 Oiawlm（scopes 含 repo） |
| U4 | **迁移＝完整开发环境**：PC2 具备 git 仓库＋构建验证（csc 为 Windows 自带）＋ZCode 与 skills，可继续开发；附日常使用配置 |
| U5 | **并行最大化＋每需求一份提示词**：波次与模型选型见 [PLAN-V5.md](PLAN-V5.md) §3/§4（主责全 GLM-5.3，Flash 仅窗口内子代理扇出） |
| U6 | **本地清理＝最后一步**：D 轨道产出脚本、用户手动执行；PC2 验证通过前本机任何项目文件不删；范围默认见 D5 |
| U7 | **fts 风格延续**：B/C 新增设置窗 UI 强制走 fts-style-system＋SettingsDialogMetrics/ThemePalette 单源纪律（v4 体系不变） |

## 2. 冻结与可演进

**冻结（v1–v4 全继承）**：双击立即出遮罩；退出交互与 arm 防误触语义；`baseline/` 一字节不改；v1 四件套断言（VerifyConfig 18 例/VerifySettings 前 3 查/VerifyMouseExit/VerifyOverlay A–F）与第 5 查逐字不动；遮罩/预览/色板颜色只来自配置；`--settings` 语义；settings.ini **v1 两键**（color/text）语义（已知键非法→整文件回默认，仅限 v1 键，见 C5）；保存四按钮语义；ThemePalette/Metrics 存量键值不迁移不改名。

**可演进**：src 新增文件；`Program.cs` 受限演进（B：入口一行清理；C：新增 `--idle` 分支；既有两分支语义逐字不动）；MaskConfig 新增 idle 键；verify 套件 5→7 件（verify.cmd 步骤编号由 I 波统一）；README（A 初版、I 终稿）与 使用说明.txt（仅 I 终稿）；install.ps1/build.cmd 迁移硬化（D）。

## 3. 调研结论（R 系浓缩）

**R1 自更新**：Velopack 支持 .NET FW 4.6.2+ 但强制 NuGet DLL＋vpk/.NET SDK 打包链＋`%LocalAppData%` 目录结构——摧毁零依赖单 exe 形态，否决；Squirrel.Windows 官方弃用（issue #1469），否决。手写方案要点：`GET api.github.com/repos/{owner}/{repo}/releases/latest`（返回最新**非 draft 非 prerelease**，JSON 含 `tag_name`/`assets[].browser_download_url/size/digest`）；必须自定义 `User-Agent`（否则 403）；未认证限速 60 次/时/IP（设置窗打开才查一次，绰绰有余）；`browser_download_url` 302 跳短时效签名 URL，WebClient 默认自动跟随（https→https 无坑）；SemVer 去 `v` 前缀按三段数字逐段比较（禁字符串序）。运行中 exe 自替换：不能删除/覆盖但**可改名**——序列＝下载 tmp→校验→清残留 `.old`→`File.Move(自身, x.old)`→`File.Copy(新, 原路径)`→重启或下次启动即新版；`File.Replace` 对运行中 exe 抛占用异常，不可用。TLS：GitHub 强制 TLS1.2+；.NET 4.8 带 TargetFrameworkAttribute 时默认 SystemDefault 已含 1.2——本项目 app.config **无** supportedRuntime 标记，走防御性显式设置（B4）。MOTW：浏览器下载才写标记触发 SmartScreen；更新器 WebClient 下载不经 Shell **无 MOTW**，替换落位不触发 SmartScreen（杀软实时扫描仍覆盖）。先例：ShareX（GitHub Releases zip 更新）、Bulk Crap Uninstaller（UpdateSystem.NET 对接 Releases）、simplewall（设置页检查器）。

**R2 计划任务与空闲**：`/sc minute /mo N` 生成 TimeTrigger＋Repetition，但**电池双开关默认 true**（笔记本电池静默不触发）且命令行无参数可改——唯一解是 XML 导入置 false。本机实测（Win11）：XML 必须 UTF-16 带 BOM 否则 malformed；`/query /tn X /xml` 退出码 0/1＝存在/不存在；`/create /tn X /xml f /f` 幂等覆盖；中文任务名在 C# Unicode 调用无碍但**任务名固定 ASCII**。`ExecutionTimeLimit` 默认 PT72H，到期经 job object **强杀进程树**——遮罩是长存活交互进程，必须 `PT0S`。`IgnoreNew` 是特性：遮罩存活期跳过重复触发，退出按键本身刷新 lastInput，天然实现「退出→再空闲→再弹」。`InteractiveToken`（仅登录时运行）保证弹窗在用户桌面且 GetLastInputInfo 会话正确。GetLastInputInfo 返回**调用者会话**最后键鼠输入 tick；与 `Environment.TickCount` 同为 32 位 tick，用带符号环绕减法 `unchecked(TickCount - (int)dwTime)` 天然正确。Task Scheduler 原生 IdleTrigger 判定＝无输入**且** CPU 低于阈值，后台噪声可致永不触发——否决，改用「每分钟任务＋GetLastInputInfo 自检」纯输入驱动。

**R3 发布与仓库卫生**：`gh repo create <名> --public --source . --push` 只推当前分支；topics 小写连字符≤20；README 标杆＝QuickLook（C# WinForms 血缘最近）：badge→简介→功能→下载（指向 Releases）→用法→构建→许可，中文项目通行「中文为主＋英文另册」，英文版本轮缓做。发布前卫生：`git log -p` 全历史扫描（绝对路径用户名＝低敏感画像，放行；真凭证必须停下）；补 .gitignore（.vs/bin/obj/*.pdb）；.gitattributes＋`git add --renormalize .`；`core.quotepath false`。Release 附件：zip 为主（浏览器对裸 exe 有 MOTW/SmartScreen/AV 拦截加剧，zip 聚合 config/ico/说明）；裸 exe 供更新器直链（资产名固定 ASCII）；`SHA256SUMS`（每行 `哈希␣␣文件名`，GitHub 不自动生成需自传）；用户验证 `certutil -hashfile X SHA256`。LICENSE：MIT 英文原版（GPL 一切译本无法律效力，对照另附也不必）。更新器直链可用 `releases/latest/download/<固定资产名>`（302）。

**R4 迁移**：.NET FW 4.8 为 OS 组件（Win10 1903+/Win11 出厂自带，22H2+ 为 4.8.1），`C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe` **免安装可用**；Windows PowerShell 5.1 全系内置；`winget install --id Git.Git -e --source winget` 装 git；主路径＝新机 clone（天然验证完整性），`git bundle create x.bundle --all` 仅作无网备援；非技术验证＝`git fsck`＋`git status`＋对比 commit SHA＋逐文件 `certutil` 哈希。GitHub Actions windows runner 自带 .NET FW 4.8＋csc 可跑，但本轮不加 CI（极简哲学，登记为后续可选）。

**来源清单**（子代理报告全文留档于规划会话，关键出处）：docs.github.com/rest/releases/releases、rest/rate-limits、cli.github.com/manual（gh_repo_create/edit、gh_release_create）、docs.velopack.io、github.com/Squirrel/Squirrel.Windows#issue-1469、visualstudiomagazine.com/articles/2017/12/15/replace-running-app、github.blog/2018-02-23-weak-cryptographic-standards-removed、learn.microsoft.com（schtasks/getlastinputinfo/task-idle-conditions/dotnet ProcessStartInfo）、github.com/QL-Win/QuickLook、docs.github.com/articles/licensing-a-repository、choosealicense.com/no-permission、github.com/actions/runner-images（Windows2022-Readme）。

## 4. 并行契约（分支·所有权·跨轨常量）

**分支**：A 直接在 `main`；B 在 `feature/update-v5`；C 在 `feature/idle-v5`（均自当前 main 切出）；I 负责合并回 main；D 在合并后的 main。

**文件所有权矩阵**（×＝可写；冲突由 I 按合并顺序消化）：

| 文件/目录 | A | B | C | I | D |
|---|---|---|---|---|---|
| README.md | 初版 | 禁 | 禁 | 终稿 | 禁 |
| 使用说明.txt | 禁 | 禁 | 禁 | 终稿 | 禁 |
| src/AppInfo.cs、src/Updater.cs（新建） | — | × | 禁 | × | — |
| src/IdleWake.cs（新建）、src/MaskConfig.cs | — | 禁 | × | × | — |
| src/Program.cs | 禁 | 入口清理一行 | ×（--idle 分支） | × | — |
| src/SettingsDialog.cs、SettingsDialogMetrics.cs | — | ×（页脚区） | ×（唤起区） | ×（冲突裁决） | — |
| verify.cmd、VerifySettings.cs | — | × | × | ×（编号统一 [7/7]） | — |
| verify/VerifyUpdate.cs（新建，B） | — | × | 禁 | × | — |
| verify/VerifyIdle.cs（新建，C） | — | 禁 | × | × | — |
| LICENSE/.gitattributes/.gitignore/assets/readme/ | × | 禁 | 禁 | × | — |
| install.ps1、migrate/（新建）、cleanup 脚本 | 禁 | 禁 | 禁 | 禁 | × |
| build.rsp | — | ×（新源两行） | ×（新源一行） | × | — |

**合并顺序**：B 先（`--no-ff` 入 main）→ C rebase 于合并态重跑验证 → 合并 C → 全量 7 件守门。

**跨轨常量单一登记表**（第二副本即幽灵，发现即删）：

| 常量 | 值 | 唯一登记处 | 其他引用方 |
|---|---|---|---|
| 更新资产名 | `OneClickBlackScreen.exe` | `src/AppInfo.cs` | A/I 的 gh release 资产命名、SHA256SUMS 行名 |
| 计划任务名 | `OneClickBlackScreen-Idle` | `src/IdleWake.cs` | D 的 cleanup 脚本 |
| 应用版本 | `1.0.0` | `src/AppInfo.cs` | I 的 tag `v1.0.0`（tag==AppInfo 为 I 的验收项） |
| 仓库标识 | `Oiawlm/OneClickBlackScreen` | `src/AppInfo.cs` | A 建仓名、I 发布、D 手册 clone URL |

## 5. 决策表（唯一实现依据）

### B 系 · 自动更新
| # | 决策 |
|---|---|
| B1 | 手写 GitHub Releases 更新器（R1 否决 Velopack/Squirrel）；源码保持 csc 直编零依赖 |
| B2 | 新建 `src/AppInfo.cs` 单源：`Version="1.0.0"`、RepoOwner/RepoName/UpdateAssetName＋SemVer 三段比较器（`v?d.d.d` 非法→视为不更新，fail-closed） |
| B3 | 新建 `src/Updater.cs`：`releases/latest` JSON→tag/资产定位；WebClient＋自定义 User-Agent；302 自动跟随；`ThreadPool`＋`Control.Invoke`（随既有代码风格，不引入 async/await） |
| B4 | TLS 防御：首次请求前 `ServicePointManager.SecurityProtocol \|= Tls12`（app.config 无 supportedRuntime 标记，不依赖 4.8 SystemDefault 假设） |
| B5 | 校验链：`assets[].digest`（sha256:）为主验；缺 digest→解析 `SHA256SUMS` 资产；双缺→拒绝更新并明示（fail-closed）；下载与声明 size 不符亦拒 |
| B6 | 替换序列（R1 实证）：tmp 下载→哈希/大小验→清残留 `.old`→`Move(自身, 一键黑屏.exe.old)`→`Copy(新, 原路径)`→提示「已更新，双击主程序即新版」→关窗（**不原地重启**，进程即退形态天然不需要）；`Program.Main` 入口 best-effort 清 `.old`（所有模式，B 在 Program.cs 的唯一改动） |
| B7 | 设置窗页脚区：`版本 1.0.0` 标签＋`检查更新` 按钮＋内联状态文字（已是最新/发现新版 vX.Y.Z＋[立即更新]/正在下载…/失败原因）；Load 时静默检查（失败完全无声）；**不新增配置键**（自动检查不可关——隐私口径登记：仅设置窗打开时一次 HTTPS 请求 api.github.com，遮罩永不联网） |
| B8 | 验证：新建 `verify/VerifyUpdate.cs`（离线全链路：HttpListener 127.0.0.1 mock latest JSON＋资产＋digest；版本比较表；digest 篡改拒更；404/畸形 tag fail-closed；替换序列在临时目录副本上演练不碰真实 dist）；verify.cmd 串入；真端点冒烟归 I（I4） |
| B9 | 边界登记：exe-only 更新（未来 config/ico 变更需升级更新器为 zip 模式）；更新文件无 MOTW 不触发 SmartScreen；杀软实扫仍覆盖；无新版本时零写盘 |

### C 系 · 空闲唤起
| # | 决策 |
|---|---|
| C1 | 语义＝U1；档位 0/5/10/15/30 分钟（0=关，默认 0）；任意键退出即重新计时（`IgnoreNew`＋退出输入刷新 lastInput 天然实现） |
| C2 | 架构＝「计划任务 PT1M＋`--idle` 自检」纯输入驱动（R2 否决 IdleTrigger 的 CPU 复合判定） |
| C3 | 任务 XML 全字段定案：TimeTrigger（StartBoundary=注册时刻）＋Repetition `PT1M`（**无 Duration**）；`InteractiveToken`/`LeastPrivilege`；`DisallowStartIfOnBatteries=false`、`StopIfGoingOnBatteries=false`；`ExecutionTimeLimit=PT0S`；`MultipleInstancesPolicy=IgnoreNew`；`StartWhenAvailable=true`；Action=`"exe全路径" --idle` |
| C4 | `GetLastInputInfo` P/Invoke（LASTINPUTINFO/cbSize）；`unchecked(Environment.TickCount - (int)dwTime)` 环绕减法；阈值毫秒=分钟×60000；判定纯函数化（idleMs, thresholdMs）→bool 便于单测 |
| C5 | MaskConfig 新键 `idle=N`：缺失/非法/越档→按 0 处理且**不影响 v1 两键**（per-key 回退；v1 键保持「非法→整文件默认」原契约不变）；Save 追加第三行；档位外正整数 clamp 到档位表（登记） |
| C6 | 自愈：设置保存成功后同步任务（开启→以 `Application.ExecutablePath` 生成 XML 幂等 `/create /xml /f`；关闭→`/delete /f`；schtasks 失败→MessageBox 警告，配置不回滚，下次保存自愈） |
| C7 | XML 写 `%TEMP%` 临时文件，**UTF-16 带 BOM**；任务名 ASCII＝登记表值；C# 调用 `UseShellExecute=false`＋`CreateNoWindow=true`，含空格路径嵌套引号，退出码 0/1 语义 |
| C8 | 设置窗新区「自动唤起」：区块标签（Bold 10.5pt）＋ComboBox（DropDownList：关闭/5/10/15/30 分钟）＋两行 9pt hint（语义＋「Windows 计划任务驱动，无后台常驻、不联网」）；Metrics 单源新增（行高/间距沿既有档位推导，fixedChrome 增量重算 previewCap）；fts：零新 token、零动效、双主题走现有 palette |
| C9 | 验证：新建 `verify/VerifyIdle.cs`：①XML 字段断言（C3 全项＋Command==exe 路径）②真实注册→query 退出码 0→删除→query 退出码 1（try/finally 必清理）③负路径：写 idle=5 配置→启动 `--idle`→2 秒内静默退出且无窗口 ④判定纯函数档位表 ⑤活动机 `IdleMilliseconds()` 读数 sanity（<60000） |
| C10 | 边界登记：锁屏/安全桌面/UAC 不覆盖（继承 v1）；RDP 断开会话读数未定义不处理；真实「离开 N 分钟触发」属用户 UAT（自动化不可达，如实登记）；本波 README/使用说明 禁改，用户文案草稿落 `docs/COPY-V5C.md` 供 I 并入 |

### A 系 · GitHub 发布
| # | 决策 |
|---|---|
| A1 | 仓 `Oiawlm/OneClickBlackScreen` 公开；MIT 英文原版（`Copyright (c) 2026 Oiawlm`）；topics：windows/winforms/csharp/dotnet/black-screen/screen-mask/portable/utility；description 见 GOAL-V5A |
| A2 | README v5＝QuickLook 同构中文主；截图（遮罩＋设置窗浅/深）≥3 张入 `assets/readme/`；下载区含 SHA256 教学；开发过程文档（GOAL/RESEARCH/PLAN）保留并如实标注；英文版缓做（登记） |
| A3 | .gitignore 增补 `.wbx/` `.vs/` `*.pdb` `verify/bin/`；.gitattributes `* text=auto eol=crlf`＋ico/exe/png/jpg/zip binary；renormalize 后 `verify.cmd` 必须全绿（行尾归一不破坏构建链） |
| A4 | 历史卫生：`git log -p` 全扫描；用户名级路径低敏放行不重写历史；疑似真凭证→停止并报告用户，不推送 |
| A5 | 只推 main；本地 feature 分支不入公开仓（门面聚焦；全量分支由 D 的离线 bundle 保全） |
| A6 | draft `v0.9.0-test`（--draft --target main）演练资产命名与 SHASUMS 工艺；I 波删除后发正式 v1.0.0 |
| A7 | SHA256SUMS＝每行 `哈希␣␣文件名`（certutil 生成后整理）；README 教 `certutil -hashfile X SHA256` |
| A8 | 不加 GitHub Actions CI（极简哲学；windows runner 可行性已登记 R4，未来最小 workflow 可加） |

### I 系 · 集成与正式发布
| # | 决策 |
|---|---|
| I1 | 合并顺序：B `--no-ff` 入 main→C rebase 于合并态重跑其验证→合并 C→**全量 7 件 verify 全绿**守门 |
| I2 | 门面终稿（唯一写者）：README 并入更新/唤起功能段＋下载区 SHA256；使用说明.txt v6（更新/唤起两节＋边界如实告知＋隐私口径）；VerifySettings 几何断言纳入全部新区控件（复核 B/C 分支各自更新） |
| I3 | `tag v1.0.0` 且 `tag==AppInfo.Version`（验收项）；`gh release create v1.0.0`：资产 `OneClickBlackScreen.exe`（裸，更新器直链）＋`一键黑屏-v1.0.0.zip`（exe+config+ico+使用说明，人用）＋`SHA256SUMS`（两资产）；删 v0.9.0-test draft；push main＋tag |
| I4 | 发布后验收：真端点冒烟（设置窗检查→「已是最新」，兼证 TLS/B4）＋临时目录 `git clone --depth 1`→`build.cmd` 成功＋SHA256SUMS 自验（certutil 比对）＋`gh release view` 资产三件齐 |
| I5 | 截图补拍：设置窗（含两新区）替换 assets/readme/ 旧图，README 引用同步 |

### D 系 · 迁移与清理
| # | 决策 |
|---|---|
| D1 | 迁移就绪硬化（根治非补丁）：install.ps1 去除 `C:\Users\Lenovo` 硬编码→由 `$env:USERPROFILE` 派生（PC2 用户名不同即需要）；build.cmd/verify.cmd 的 csc 存在性 FAIL 提示保持既有口径 |
| D2 | 迁移主路径（PC2 在线）：`winget install --id Git.Git -e` →（可选 gh）→ `git clone https://github.com/Oiawlm/OneClickBlackScreen` → `git config core.quotepath false` → `build.cmd` → `verify.cmd` 全绿 → `install.ps1` → 双击遮罩冒烟；ZCode：官网安装＋登录＋skills 三件套（fts-style-system/glm-prompt-crafter/github-readme，wb-bridge 可选）从迁移包复制到 `%USERPROFILE%\.zcode\skills\`；`settings.ini` 按需复制（个性化） |
| D3 | 离线备援包：`git bundle create 一键黑屏.bundle --all`（含未推送分支，R3/A5 的保全）＋仓库工作区 zip＋skills zip＋手册自身；PC2 无网时 `git clone 一键黑屏.bundle` 全量还原 |
| D4 | 手册（`migrate/迁移手册.md`＋图表）：总流程图＋每步小图；每步含「在哪打开 PowerShell（开始菜单搜 / Win+X）」＋逐条可复制粘贴命令＋预期输出样例＋失败排查表；菜鸟零跳步口径（术语首次出现即解释）；图表用 mermaid 内嵌＋导出 PNG |
| D5 | 清理脚本 `migrate/cleanup-local.ps1`（D 生成、**用户手动执行、任何窗口不得代跑**）：`-ConfirmPC2Verified` 开关（缺省拒绝运行并列出 PC2 验证清单）；执行序＝打包留档（仓库 zip→桌面＋SHA256）→删：仓库目录、两个快捷方式 .lnk（存在才删）、`%LOCALAPPDATA%\BlackScreen`、计划任务 `OneClickBlackScreen-Idle`（存在才删）、ZCode 本工作区痕迹（可定位部分）；**保留**：`~/.zcode/skills`（用户级、非项目专属，登记待用户确认）；每步输出结果清单 |
| D6 | PC2 验证口径（迁移完成的判定）：`git fsck` 无错＋`git status` 干净＋`verify.cmd` 全绿＋双击遮罩冒烟＋设置窗打开冒烟；证据留档（截图），作为跑 cleanup 的前置 |

## 6. 验证面演进

verify 5 件→**7 件**：`[6] VerifyUpdate`（B）＋`[7] VerifyIdle`（C）；verify.cmd 步骤编号分支上各自续编、I 波统一为 [1/7]..[7/7]；VerifySettings 由 6 查扩展纳入新区控件几何（B/C 各自分支更新计数断言，I 复核）；`docs/VERIFY-V5.md`（I 产出）＝全绿命令与输出摘要＋两分支合并 diff 审计＋发布证据。

## 7. 模型与工具

主责全 GLM-5.3、Flash 仅窗口内子代理扇出、wbx 桥未登录降级内置子代理——裁决与理由见 [PLAN-V5.md](PLAN-V5.md) §4。

## 8. 勘误

**E1（I 波登记，2026-11-03）**：I3/A7 中 zip 资产名 `一键黑屏-v1.0.0.zip` **平台不可达**——GitHub Releases 资产 name 字段对非 ASCII 字符做服务端消毒（中文字符被剥除，实测 PATCH `name` 与 uploads 端点同样被剥，同请求的 `label` 字段中文完好存活；社区登记 [community #57748](https://github.com/orgs/community/discussions/57748)、[#209923](https://github.com/orgs/community/discussions/209923)）。**勘误后口径**：zip 资产名＝`OneClickBlackScreen-v1.0.0.zip`（ASCII），中文身份保留在资产 `label`（「一键黑屏 v1.0.0 完整包（exe＋config＋ico＋使用说明）」，Release 页与 `label` 一同显示）；SHA256SUMS 行名与 README 引用同步改用 ASCII 名。zip **内部**文件名不受影响（一键黑屏.exe 等照旧）。铁律 3「资产名必须等于 AppInfo.UpdateAssetName」不受影响（该条针对更新器直链的裸 exe，本就是 ASCII）。

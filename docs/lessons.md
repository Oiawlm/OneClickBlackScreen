# docs/lessons.md · 跨轮坑库

> **一句话定位**：全项目历史踩坑的单一汇总——每条「现象→根因→正解→出处→防再犯锚点」五要素；改任何代码/文档/流程前先来查，交接文档「踩坑」字段引此处编号。　**来源**：[RESEARCH-V8 §4.2](handoff/archive/2026-11-05-v8-structure/RESEARCH-V8.md) 12 条种子＋历史 VERIFY/REVIEW/PUBLISH 事故与移交注记提炼。　**日期**：2026-11-05　行数预算：≤180 行。
>
> **出处口径**：一律写**归档后路径** `docs/handoff/archive/...`（git mv 只改路径，内容与行号不变）；核内容时映射回归档前位置——VERIFY/REVIEW/PUBLISH 等系原在 `docs/` 下（去掉 `handoff/archive/` 段即回 `docs/`），RESEARCH/PLAN/GOAL 系原在仓库根目录。`AGENTS.md`、`src/**`、`verify.cmd` 不归档，按原路径引用。**收录纪律：宁缺毋滥——每条必须有可打开的真实出处，禁凭印象编造；新坑先补出处再入册。**

### L-001｜两窗共享单工作树，切分支互相劫持检出（V6B）
- 现象：B 窗提交一度落在 A 窗分支尖上；A 未提交的验证器修改被对方切分支卷走。
- 根因：checkout 是共享资源——同一检出同一时间只能呈现一个分支，第二窗 `git checkout` 即劫持全部工作区。
- 正解：并行轮一轨一独立 worktree（兄弟目录、`ls-remote` 撞名即停）；V8 起壳内 `OneClickBlackScreen-WorkTree\` 固化（docs/multi-window.md）。
- 出处：docs/handoff/archive/VERIFY-V6B.md:61-65（§7 并发工作树事故与处置）。
- 防再犯：AGENTS §2「一个 checkout 同一时间只归一个窗口」；docs/multi-window.md 建树模板。

### L-002｜验证器备份-覆写-恢复用户真实 settings.ini＝危险机制（V7V 根修＝删机制）
- 现象：早期 VerifyOverlay/VerifySettings 以「字节级备份真实 ini→覆写→finally 恢复」验证配置面；V5A 轮一次断言被置于 try/finally 之外，finally 未执行，用户真实 settings.ini 被演示配置覆盖、进程内备份丢失（靠终端回显字节级转码重建恢复）。
- 根因：测试碰用户状态本身即风险源——备份/恢复装甲再厚也有失守路径（异常、并发、进程被杀）。
- 正解：根修＝**删除机制而非加装甲**——「测试不碰用户状态的能力来自机制不存在」：改走 TEMP/子进程注入隔离（L-003/L-012），真实用户状态只读。
- 出处：删除定案 docs/handoff/archive/VERIFY-V7V.md:36（否决「保留备份-恢复＋报警＝给危险机制加装甲」）；实害案例 docs/handoff/archive/PUBLISH-V5A.md:149；早期形态 docs/handoff/archive/VERIFY-V4.md:46。
- 防再犯：AGENTS §2「真实用户状态只读/零足迹」；新验证器评审必问「此测试是否可能写用户状态」。

### L-003｜进程内运行时改 USERPROFILE 无效——生效面＝进程创建时的环境块
- 现象：进程内 `Environment.SetEnvironmentVariable("USERPROFILE",…)` 后 `GetFolderPath(LocalApplicationData)` 仍返回真实路径；VerifyIdle 原断言不依赖配置内容，隔离无效却一直绿灯（安慰剂测试）。
- 根因：known-folder 解析按每用户注册表模板展开于**进程创建时注入的环境块**（忽略 LOCALAPPDATA 环境变量＋存在性校验），进程内运行时改动不在生效面。
- 正解：隔离一律走**子进程启动前注入环境变量**（Overlay/Settings/Idle 验证器现行做法）；产品侧解析唯一点＝src/MaskConfig.cs:49。
- 出处：docs/handoff/archive/REVIEW-V7.md:52；机理 docs/handoff/archive/VERIFY-V7V.md:8；安慰剂 docs/handoff/archive/VERIFY-V7V.md:22。
- 防再犯：验证器隔离检查项「必须子进程注入」；勿信进程内 SetEnvironmentVariable（关联 L-009）。

### L-004｜两窗并行跑 GUI 验证器＝全局键鼠互扰（会话闸门根治）
- 现象：V5A 轮实测 B 轨设置窗/更新测试与 A 轨遮罩验证同桌面并发互相干扰；单实例互斥被并行验证器竞争致 PrintWindow 首轮失败。
- 根因：验证器注入全局键鼠/抢占前台——全局输入队列属整个登录会话，与另一窗口的 GUI 自动化天然相撞。
- 正解：会话级闸门 `Local\BlackScreen.VerifyGate` named mutex（`WaitOne(0)` 零超时探测）；撞闸 stderr `VERIFY BUSY`＋退出码 2；七件统一（纯件在内）；verify.cmd 先判 errorlevel 2 再判 1。
- 出处：事故 docs/handoff/archive/PUBLISH-V5A.md:148,150；根治 docs/handoff/archive/VERIFY-V7V.md:32；协议 AGENTS.md §2。
- 防再犯：AGENTS §2「验证套件会话级独占」——撞 BUSY＝等对方结束再跑，勿绕闸门、勿盲目重试风暴。

### L-005｜校验脚本自身缺陷报假红/假 MISMATCH（digest-check.ps1 两处）
- 现象：V7I digest 对比脚本首版两缺陷：①正则尾部多余 `}` 致哈希行零匹配→空数组索引异常；②PowerShell 单匹配返回标量，`$out[0]` 取到字符。V6 轮 SumsCheck 亦因脚本变量残留报假 MISMATCH。
- 根因：校验工具缺陷≠资产缺陷——脚本被默认正确，其异常被误读为资产问题。
- 正解：修脚本本身（正则/标量化），修后全 MATCH、SUMS 2/2；结论只对真实输出负责，工具报错先审工具。
- 出处：docs/handoff/archive/VERIFY-V7.md:85（§7 事故注记1）；同族前身 docs/handoff/archive/VERIFY-V6.md:74（§7 事故注记3）。
- 防再犯：发版链 digest/SUMS 环节把「脚本自身缺陷」列为第一怀疑项。

### L-006｜Windows 编码三变体（PS5.1 按 ANSI 读无 BOM／csc 中文源需 UTF-8 BOM／bash 双引号吞 $）
- 现象：①无 BOM .ps1 的中文字面量被 PowerShell 5.1 按 ANSI 误读；②csc 编译含中文串源码需 UTF-8 BOM（写后转换）；③bash 双引号内 PowerShell `$` 变量被展开吞掉→改单引号传参。
- 根因：Windows 工具链编码默认值各异（ANSI 代码页／UTF-8 BOM／bash 展开），非 ASCII 或变量串跨工具传递即变形；V5A 曾因 GBK 乱码靠字节级转码重建原文。
- 正解：临时工具脚本**零非 ASCII 字面量**（用 pattern 定位文件）；需编译含中文的源码写后转 UTF-8 BOM；bash 传 PowerShell 参数用单引号。
- 出处：docs/handoff/archive/VERIFY-V7.md:87（§7 事故注记3）；GBK 实害 docs/handoff/archive/PUBLISH-V5A.md:149。
- 防再犯：跨 shell 工具脚本 ASCII-only 纪律；写前查本条。

### L-007｜LatestSmoke 首版点中禁用按钮＝工具缺陷误红（产品无缺陷）
- 现象：BM_CLICK 落在「打开时静默检查」的按钮禁用窗内（`checkUpdateButton.Enabled=false`＝S 轨防并发重入的设计内行为），禁用按钮不响应也不排队点击→假红。
- 根因：UI 自动化时序假设错误——点击瞬间控件可能正处于合法中间态。
- 正解：点击前轮询 `IsWindowEnabled` 至就绪（实测 1732ms）＋状态轮询补「正在检查更新…」中间态捕捉；复跑全绿。
- 出处：docs/handoff/archive/VERIFY-V7.md:86（§7 事故注记2）。
- 防再犯：GUI 自动化点击一律先轮询可用态；红了先分「工具缺陷 vs 产品缺陷」再动手（同族 L-014）。

### L-008｜并发「立即更新」互删备份→「无法备份当前程序」（Global\ 互斥修复）
- 现象：`--settings` 多实例下两实例同入更新替换序列：实例2 的 `File.Delete(exe.old)` 删掉实例1 刚建的备份、`File.Move` 撞已改名目标→抛「无法备份当前程序」→回滚链连带失败；红基线 300/300 轮全相撞。
- 根因：exe 在全机共享路径（跨用户会话同文件），替换序列无互斥；会话级锁不够，必须 `Global\` 前缀。
- 正解：互斥融入 `ReplaceSequence` 内部（src/Updater.cs:149-192，锁名 `Global\BlackScreen.Update`＝src/Updater.cs:131）；获锁后复核 SHA256 相等则 no-op 成功；U1-U3 边界（单实例语义/设置窗多开/遮罩互斥）逐点保持。
- 出处：根因 docs/handoff/archive/RESEARCH-V7.md:21；红基线 docs/handoff/archive/VERIFY-V7S.md:14-18,22；修复 docs/handoff/archive/VERIFY-V7S.md:41；边界对照 :86-92。
- 防再犯：凡涉全机共享路径的写序列先问「跨会话互斥了吗」；发版冒烟含并发场景。

### L-009｜（R-1）机理表述欠精确会成为未来的安慰剂
- 现象：VERIFY-V7V §0 写「展开于**进程环境**」，实测生效面是**进程创建时注入的环境块**——未来有人改走进程内运行时修改会得到「以为隔离了」的安慰剂（现实现全走子进程注入，无实际缺陷）。
- 根因：过程文档的机理归纳精度不足（护栏知识，非缺陷）。
- 正解：机理表述以探针实测为准（双跑对照留档）；冻结的过程记录不回改，以移交注记＋坑库传承。
- 出处：docs/handoff/archive/REVIEW-V7.md:108（移交注记 R-1 全文）。
- 防再犯：写验证机理附复现命令；读者勿信文档结论超过实测（关联 L-003）。

### L-010｜（R-2）verify.cmd errorlevel 对负退出码判 false
- 现象：`if errorlevel 1/2` 对**负**退出码（如 CLR 未处理异常崩溃 -532462766）均判 false，崩溃轮理论上可漏过失败出口。
- 根因：cmd errorlevel 语义＝「≥N」，负码天然小于 1；既有缓解＝各验证器 Main 均 try/catch→return 1 兜底，实际暴露面小。
- 正解：备案不修（V 轨铁律「既有语义零变化」是正确裁决）；未来动 verify.cmd 时一并处理负码。
- 出处：docs/handoff/archive/REVIEW-V7.md:109（移交注记 R-2 全文）。
- 防再犯：改验证器退出路径保持 try/catch→正码兜底；I 轮留意崩溃轮真实退出码。

### L-011｜（R-3）单源化重构会改变极端子情形的异常类型
- 现象：VerifySha256 单源化后，「下载文件不可读」极端子情形（下载成功后立即缺失/锁死）的异常由 IOException 变为 InvalidOperationException（「SHA-256 校验失败…」）；拒装终态与时序不变，U1 可观测主路径逐点保持。
- 根因：异常处理收敛到统一入口后原始异常被包装替换——异常类型（可观测行为）变了而语义终态没变。
- 正解：判「不构成行为违反，记录在案」；依赖异常类型的调用方须知悉收敛点变化。
- 出处：docs/handoff/archive/REVIEW-V7.md:110（移交注记 R-3 全文）。
- 防再犯：重构收敛异常处理时，把「异常类型变化」列入移交注记；勿以 catch 类型做业务分支。

### L-012｜零足迹原则：用户状态只读，验证走 TEMP/改名隔离
- 现象：`%LOCALAPPDATA%\BlackScreen\settings.ini` 与计划任务 `OneClickBlackScreen-Idle` 属于用户；验证一律 TEMP/USERPROFILE 隔离，套件跑完用户状态**字节级不变**——V7V 审计实录：前后三态一致（真实 ini 不存在→不存在、任务无残留、_probe 无落盘）。
- 根因：任何「先用真状态再恢复」的方案都是 L-002 的危险机制；唯一安全态＝从未触碰。
- 正解：验证配置面走隔离环境（子进程注入，见 L-003）；跑前后对用户真状态做三态快照审计留档。
- 出处：原则 AGENTS.md §2（真实用户状态只读）；审计 docs/handoff/archive/VERIFY-V7V.md:93-96；复检 docs/handoff/archive/REVIEW-V7.md:27。
- 防再犯：AGENTS §2 条款；I 轮零足迹审计为发版链固定环节。

### L-013｜SendMessage 同步点击会弹模态框的按钮＝自死锁（V6 SettingsSmoke）
- 现象：同步 `SendMessage(BM_CLICK)` 点「保存」→处理器弹模态确认框并在 SendMessage 内派发→工具阻塞在等框关闭、而关框代码排在其后＝自死锁；强杀工具致 finally 配置恢复未执行。
- 根因：SendMessage 同步语义＋模态框内嵌消息循环＝不可重入环。
- 正解：自动化点会弹模态框的控件改异步（PostMessage/后台线程）；事故核查用 NTFS CreationTime（覆写不重置创建时间）判定「文件本轮前是否存在」，防误判用户数据受损（本轮实测：CreationTime＝工具写入时点→文件本轮前不存在→无用户数据受损）。
- 出处：docs/handoff/archive/VERIFY-V6.md:73（§7 事故注记2）。
- 防再犯：GUI 自动化守则「模态框控件＝异步点击」；事故核查先查 CreationTime。

### L-014｜机器环境噪声致验证假红（幻影键态／IME 子窗口）
- 现象：①本机外部软件残留幻影 ESC 键态（DOWN=0x1B）先于任何注入即存在，致键关闭断言假红；②第三方默认 IME 给被测窗挂 `OimeTsfNotifyWindow` 子窗口，致子控件数断言 17≠18 假红。
- 根因：被测进程外的机器状态（键态残留／默认输入法）污染断言输入——环境问题伪装成产品回归。
- 正解：①先扫键态、合成 keyup 清残留后复跑即全绿；②验证期临时 `Set-WinDefaultInputMethodOverride` 切英文输入法，**验证完成后还原原值**；假红先分环境/产品（同族 L-007）。
- 出处：docs/handoff/archive/VERIFY-V6.md:11,72；docs/handoff/archive/PUBLISH-V5A.md:147。
- 防再犯：验证前键态扫描入检查项；环境级临时改动必须「改/还」双列留档。

### L-015｜GitHub Release 资产名非 ASCII 被服务端剥除
- 现象：资产以中文名 `一键黑屏-v1.0.0.zip` 上传后，GitHub 返回的资产 name 恒为 `-v1.0.0.zip`（中文字符被剥），三次复现；锁定为服务端行为（社区登记 community #57748、#209923），非传输问题。
- 根因：GitHub Releases 资产 name 字段服务端非 ASCII 消毒（label 可存活，name 不行）。
- 正解：资产文件名一律 ASCII（产品英文名＋版本号）；中文名只可用于 Release label。
- 出处：docs/handoff/archive/VERIFY-V5.md:101。
- 防再犯：发版链资产命名检查项（I 轨）；SUMS 校验以服务端真实返回名为准。

### L-016｜全屏截图法结构性拍到用户实时桌面（隐私事故风险）
- 现象：演示截图用全屏 CopyFromScreen 拍到用户实时桌面（隐私不可用，图已弃用销毁）；改 PrintWindow 法首轮又撞单实例互斥被并行轨道竞争。
- 根因：CopyFromScreen 是屏幕级捕获，天然包含桌面一切；正确捕获对象是被测窗口而非屏幕。
- 正解：窗口级 PrintWindow＋被测程序自启自绘；截图类任务先自查「画面里有没有不该有的东西」再留档。
- 出处：docs/handoff/archive/PUBLISH-V5A.md:148（§事故注记3）。
- 防再犯：截图/录屏默认窗口级捕获；产物先审后存。

### L-017｜本机个人环境约定混入仓库分发面（V9）
- 现象：本机个人快捷方式启动器目录（个人工作流默认值）被写进仓库脚本与文档——install.ps1 整文件只为其同步 .lnk，migrate 手册/清理脚本与 archive 历史文档多处点名。
- 根因：个人工作流默认值被当作产品事实写进分发面；「这台电脑上如此」≠「每台电脑上应如此」。
- 正解：分发只走标准 Windows 集成（安装器/开始菜单快捷方式，V9 起由安装包承担），个人环境约定不入库；既有落点手术式根除（用户 2026-11-05 授权含冻结区）。
- 出处：用户 2026-11-05 指令（docs/handoff/2026-11-05-v9-installer/RESEARCH-V9.md §1①/§6-1）；install.ps1（本轮整文件删除）。
- 防再犯：新分发面评审问「这条在陌生电脑上成立吗」；不成立即个人约定，不入库。

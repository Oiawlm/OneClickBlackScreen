# VERIFY-V6B · 轨道 B 验证记录：验证器环境稳健化

> 日期：2026-11-03　分支：feature/verify-env-v6（自 main 5f4f058）　环境：PC2 Win11 26200＋微软拼音（RESEARCH-V6 §1 体检机）
> 事实源：[RESEARCH-V6.md](../RESEARCH-V6.md) §3（根因二）、§4（卫生审计项）。产物：`verify/VerifySettings.cs`（正向判别式）＋本证据文档。写面 ⊆ GOAL-V6B §4.2 白名单，`verify.cmd`/`src/**`/`migrate/**` 冻结未动。

## 1. 红基线（修复前 · 同机同会话两跑两红）

键态预扫描（csc 临时探针 `%TEMP%\v6b\KeyScan.exe`，GetAsyncKeyState(1..254) 两轮）：`CLEAN: no keys down (2 rounds)`——排除根因一连坐，失败纯出根因二。

| # | 命令 | 关键输出 |
|---|---|---|
| 1 | `cmd /c verify.cmd`（编译七件全过） | `[2/7] VerifySettings` 六 PASS 后 `FAIL: expected 24 direct children, saw 25`，套件 `EXIT=1` 中止于 [2/7] |
| 2 | `verify\bin\VerifySettings.exe "dist\一键黑屏.exe"` 单跑 | 前六查 PASS 后 `FAIL: expected 24 direct children, saw 25`（经 tail 管道留档，退出码被管道掩蔽，以 FAIL 行为准） |

**诱发记录**：无需任何诱发手段——本会话 TSF 注入持续在现（两跑两红）；对照 RESEARCH-V6 §3 首日实录（同机首跑 `saw 25`、次跑全绿 `saw 24`），非确定性成立，且红绿对照拿到了**同一会话同环境**的最强配对。

## 2. 判别式设计（verify/VerifySettings.cs，第 6 查子窗枚举）

改动两处（其余五查断言语义零改动）：

1. `EnumChildWindows` 回调：`GetParent==hwnd` 直属过滤后，新增 `GetClassName` 前缀判别 `IsAppOwnedClass`，非应用自有类不入 `children` 清单（复用本文件既有 `GetClassName` P/Invoke，自包含，无新增源文件）。
2. 新谓词 `IsAppOwnedClass`：类名以 `WindowsForms10.` 前缀开头（大小写不敏感，与本文件 `IsEditClass`/`IsComboClass`/`.WINDOW.8.` 判别同风格同址）。

**为何正向前缀而非黑名单（第一性推导）**：镜像锁的观察对象是「应用自有的 WinForms 控件树」。WinForms 注册其创建的每一个控件类时都携带 `WindowsForms10.` 前缀——这是所有权标识，本验证器的既有控件判别（EDIT/COMBOBOX/WINDOW.8 匹配均写 `.XXX.` 中缀）本就依赖该事实；实测枚举留档（RESEARCH-V6 §3）亦证 24 控件全部携带该前缀。据此正向选择＝把观察域校正为应用控件树本身；而按 `OimeTsfNotifyWindow` 等类名逐个剔除是黑名单——下一个环境注入别的类名（输入法换代/远程工具/辅助功能）即失效＝打补丁，禁。

**断言不是放宽**：expected 清单（24 件）与既有 ChildInfo 匹配逻辑、±1px 容差、类名/文本/矩形失败信息口径全部不变；变化仅在观察域从「裸子窗集合」校正为「应用自有控件集合」——应用丢控件（saw 23）与新增未镜像控件（saw 25）仍照常红。

## 3. 绿证据（修复后）

编译：与 `verify.cmd:22` 逐字一致的 csc 命令行重编译，零输出零警告。

| 证据 | 命令 | 结果 |
|---|---|---|
| 连续 ×3 单跑 | `verify\bin\VerifySettings.exe "dist\一键黑屏.exe"` ×3 | 三跑均 6 PASS＋`PASS TOTAL: 6 settings checks`，每跑 `EXIT=0`——且与 §1 红基线**同会话同 IME 活跃态**，判别式当场吃掉外来窗 |
| 干净键态全套件 | 键态扫描 CLEAN → `cmd /c verify.cmd` | 七件全绿，末行 `[OK] ALL VERIFIERS PASSED`，`EXIT=0`（[3/7] 5/5 buttons、[4/7] A–F、[6/7] 9 PASS、[7/7] 5 PASS） |

## 4. 注入卫生审计表（七件全覆盖 · 证据驱动 · 只修有实据者）

方法：grep 全量定位 `keybd_event|mouse_event|SendInput|MOUSEEVENTF|WM_KEYxx|0x020x` 调用点 → 逐点读上下文分析 down/up 在一切路径（含 Check 抛出提前退出）的配对；VerifyIdle 另查计划任务失败路径清理。

| 文件 | 核查点 | 实据 | 处置 |
|---|---|---|---|
| VerifyConfig.cs | 注入 API 零调用点（grep 零命中，纯配置逻辑） | 无 | 核查通过，零改动 |
| VerifySettings.cs | 仅 `PostMessage` WM_KEYDOWN/UP×2 处（:652-654/:714-716）：消息层投递不触 GetAsyncKeyState 全局键态；down/up 相邻成对，Check 皆在配对之后；进程 finally Kill 兜底 | 无 | 核查通过，零改动 |
| VerifyIcon.cs | 注入 API 零调用点（结构/字节校验） | 无 | 核查通过，零改动 |
| VerifyUpdate.cs | 注入 API 零调用点（离线链路） | 无 | 核查通过，零改动 |
| VerifyIdle.cs | ①`mouse_event` 仅 `MOUSEEVENTF_MOVE` ±1 相对移动×2（:167-168）：无按键态语义，相邻两行，不可能留按下态。②计划任务：`Ensure`→`try{Check(Exists)}finally{Remove}`（:83-92）＋`Main` catch 兜底再 `Remove`（:201-202）双保险；`IdleWake.Ensure` 注册成功后无任何可抛语句（src/IdleWake.cs:99-112，finally 仅吞异常删临时文件）——一切失败路径任务必清 | 无 | 核查通过，零改动 |
| VerifyMouseExit.cs（A 轨，只读） | 仅 `PostMessage` 鼠标消息（WM_ 0x0200/0201…消息层，不触全局键态）；唯一 down(:184)/up(:195) 之间的 StayOpen/Check 若抛出，由 finally 进程 Kill（:201-211）兜底——消息队列态随进程消亡，无跨测试残留 | 无 | 核查通过（A 轨写面维持原样） |
| VerifyOverlay.cs（A 轨，只读） | ①`ExitByKey`（:235-236）down/up 相邻，间隔无可抛语句，Check 在 up 后——一切路径成对。②场景 E：`keybd_event(Shift down)`（:357）位于 `using (Process process = Launch())` **之前**；Shift up 仅存在于 using 体 finally（:373） | **有**：`Launch()` 抛出（exe 缺失/被锁/映像损坏等）→ using 体不进入 → :373 永不执行 → **VK_SHIFT 全局按下态毒化**，后续一切遮罩布防失败（正是 RESEARCH-V6 §2 幻影键机制的验证器自造版） | **移交注记→I 波转 A 轨**（见 §5） |

**结论**：B 轨写面四件（Config/Settings/Icon/Update/Idle 中可改者）全部核查通过、零实据、一行未动——无投机性加固；唯一实据落在 A 轨写面 VerifyOverlay.cs。

## 5. A 轨移交注记（I 波转 A 补）

- **VerifyOverlay.cs 场景 E**：`:357` Shift down 与 `Launch()` 的先后使 up 配对依赖 using 体 finally。修法建议（供 A 参考）：将 Shift down/up 收编进同一 try/finally（down 移入 using 内 try 之前，或整段以 finally 首语句发 Shift up），保持「先按住再启动」的语义不变。B 轨不代改（写面铁律）。

## 6. 写面与冻结遵守（验收对照）

`git diff --name-only main...HEAD` ＝ `verify/VerifySettings.cs`＋`docs/VERIFY-V6B.md`（⊆ GOAL-V6B §4.2 白名单）；工作树无未跟踪临时文件（键态探针编译于 `%TEMP%\v6b`，仓库外，已清理）。不 merge、不发版（I 波辖内）。

## 7. 并发工作树事故与处置（会话运维注记）

- 经过：B 窗自 main 建分支 feature/verify-env-v6 后，A 窗在同一工作树切出 feature/exit-arm-v6 并持检出；B 提交 3d67b2a 一度误落在 A 分支尖（A 当时零提交、树上另有 A 未提交的 VerifyMouseExit/VerifyOverlay 修改）。
- 处置（单序列，全程未触碰 A 的任何文件内容）：①`git branch -f feature/verify-env-v6 3d67b2a`——提交归位 B 分支（其父＝main 5f4f058，无 A 工作卷入）；②A 分支 `git reset --mixed 5f4f058`——仅回移指针与索引，工作树不动；③树上 `checkout -- verify/VerifySettings.cs`＋删 docs/VERIFY-V6B.md——B 两文件摘出树（内容已在 B 分支保全）。复位后 A 树状态＝A 两文件修改，与事故前逐字一致。
- 结论：两分支拓扑干净——feature/verify-env-v6 ＝ main＋B 提交（本证据文档所在）；feature/exit-arm-v6 ＝ main（待 A 提交）。I 波合并无交叉污染。教训：A/B 并行须各用独立 worktree/clone，共享单工作树的分支切换会互相劫持检出。

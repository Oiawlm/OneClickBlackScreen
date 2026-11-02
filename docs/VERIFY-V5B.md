# VERIFY-V5B · 一键黑屏 v5 轨道 B「自动更新」验证记录

> 日期：2026-11-02　分支：feature/update-v5（自 main 735568b 切出）　事实源：[RESEARCH-V5.md](../RESEARCH-V5.md) §5 B 系（B1–B9）＋ §4 并行契约　执行：GOAL-V5B 目标模式
> 环境：Win11 26300；**本轮仅一块屏** DISPLAY1 2560×1600 @150%＝144DPI（v4 时期的 DISPLAY2 @125% 副屏本轮已断开，见 §6 边界）；系统浅色主题；用户系统代理 127.0.0.1:7897 活动中（未触碰）。

## 0. 并行隔离事件登记（对 I 波重要）

B/C 两条轨道窗口曾共用同一工作区检出：C 窗口在主工作区创建了 `feature/idle-v5` 并直改文件，B 切分支时其未提交改动被带入 `feature/update-v5`（检出同 commit，文件零重写）。处置：

1. C 的未提交改动已完整备份到仓库外：`D:\Download\Agent Vault\c-track-uncommitted-backup-20261102.patch`＋`IdleWake.cs.backup-20261102`；
2. 主工作区已切回 `feature/idle-v5`（改动随行，C 窗口可继续）；
3. B 全部工作迁移至独立 worktree `D:\Download\Agent Vault\一键黑屏-B-update`（与用户为 A 轨道建的 `一键黑屏-A-main` 同一模式；worktree 共享同一 .git，本分支提交即入主仓）。

I 波合并时请以 worktree 路径下的 `feature/update-v5` 为准；A-main worktree 的 main 亦在同一仓。

## 1. 构建与六件套验证（同一轮 dist）

```
cmd /c build.cmd   → [OK] Build succeeded   （worktree 一键黑屏-B-update）
cmd /c verify.cmd  → [OK] ALL VERIFIERS PASSED   日志 _probe\verify-v5b-final.log
基线（改动前，同 worktree）→ [OK] ALL VERIFIERS PASSED 五件套（_probe 系列未编号留档）
```

| 验证器 | 结果 |
|---|---|
| [1/6] VerifyConfig | PASS：18 config tests（零改动） |
| [2/6] VerifySettings | PASS：6 查——第 1–3/5 查逐字零改动；第 4 查背景采样条带断言零改动；**第 6 查几何镜像升级为 21 直系子控件**（17＋页脚 4：版本行/检查更新/状态/立即更新保留槽，均 ±1px 复算一致）＋ClientSize 660×991±1＋WorkingArea containment＋EDIT body 档＋主按钮 Accent 锁定（语义与 v4 相同，仅纳入新控件） |
| [3/6] VerifyMouseExit | PASS：5/5 buttons；30 moves；10 wheel（零改动） |
| [4/6] VerifyOverlay | PASS：A–F；1 displays；max InputIdle 162 ms（限 2000） |
| [5/6] VerifyIcon | PASS：4 icon checks（零改动） |
| [6/6] VerifyUpdate（新增） | PASS 全部：①版本比较表（新/等/旧/畸形 fail-closed）②全链路 Check→302→digest 验→替换（临时目录副本）③篡改 digest 拒更且目标未动 ④SHA256SUMS 回退完成 ⑤digest/SUMS 双缺与行不匹配双拒 ⑥大小不符拒 ⑦404/畸形 JSON/缺资产 fail-closed ⑧相等与畸形 tag 判无更新 ⑨替换序列（残留 .old 清除/回滚还原/只读 .old 阻断前置）＋UA 自定义头断言＋进度单调且终值 100 |

exe：`dist\一键黑屏.exe` 257024 B，SHA-256 `725B5555C97AD272CEFEFF721CAC6829A94F2CC872AA210C360CCF536A5E117C`。

## 2. 原始证据清单

| 证据 | 内容与落点（均在 worktree `_probe\`，未入 git） |
|---|---|
| verify 全绿日志 | `_probe\verify-v5b-final.log`（六件套 + 上述 PASS 行） |
| 浅色截图 | `_probe\v5b-light.png`（真 exe PrintWindow 666×1031 全窗口 @150%，页脚目检 4/4：版本 1.0.0 在/检查更新在/间距清晰无重叠/全窗无截断错位） |
| 深色截图 | `_probe\v5b-dark.png`＋页脚裁片 `v5b-dark-footer.png`（DarkProbe 反射强制 ApplyTheme(false)；页脚结构 4/4 通过；备注见 §6） |
| 150% 几何 dump | `_probe\dump-v5b-150.txt`（DumpLayoutDpi：页脚 版本[24,853,120,36]/检查[156,853,120,36]/状态[288,853,216,36]/立即更新[516,853,120,36]，与 S()×1.5 逐项精确一致；按钮行 y=919，ClientSize 660×991） |
| 离线冒烟 | `_probe\v5b-smoke-offline.png`＋探针 stdout（§5） |
| 真实端点冒烟 | `_probe\v5b-smoke-real.png`＋探针 stdout（§5） |
| 冒烟探针 | `_probe\UpdateSmoke.cs/.exe`（真消息 BM_CLICK、断言版本行/静默无声/失败明示/窗口稳定/退出 0） |
| 离线沙箱方法 | dist 副本＋exe.config 注入 `system.net/defaultProxy=127.0.0.1:9`（死端口）——不动用户系统代理 |
| 验收 grep | 版本幽灵 `0\.0\.0\|1\.0\.1` src/ **零命中**；`File.Replace` src/Updater.cs **零命中**（含注释，措辞已改写为"in-place replace API"）；`api\.github\.com` src/ **仅 AppInfo.cs:19 一处** |
| BOM/CRLF | 新增/改写 7 个 .cs 全部 UTF-8 BOM＋CRLF（`_probe\normalize-bTrack.ps1` 执行输出） |

## 3. 替换序列设计说明（B6，R1 实证）

- **三步**：清残留 `<exe>.old` → `File.Move(自身, .old)`（运行中镜像可改名不可原地替换，R1）→ `File.Copy(已验证新文件, 原路径, false)`。Move 失败＝未动自身即止；Copy 失败＝删半写文件→`Move(.old, 原路径)` 回滚，报具体原因。
- **两阶段 API**：`Updater.DownloadAndVerify`（线程池：解析期望哈希→下载 %TEMP% 随机名→大小→SHA-256）＋`Updater.ReplaceSequence`（由设置窗 **Control.Invoke 到 UI 线程执行**：窗口消息排队于替换之后，进程不会在 Copy 中途退出，杜绝半写镜像）；`DownloadAndApply` 为二者串联（VerifyUpdate 全链路用）。验证器以「源文件缺失→Copy 失败→回滚」真实演练回滚分支（`已还原` 断言＋目标字节还原＋.old 消费）。
- **.old 生命周期**：替换成功遗留 → 下次任意模式启动 `Program.Main` 入口 best-effort `File.Delete(exe+".old")`（Program.cs 本轨道唯一改动区，try/catch 空）。
- **不原地重启**（B6）：更新成功 MessageBox「已更新到 vX.Y.Z，关闭本窗口后双击主程序即运行新版」→Close；进程即退形态无需重启。

## 4. 隐私口径（U2/B7）

- **唯一联网触点＝设置窗**：打开时静默检查一次（GET `https://api.github.com/repos/Oiawlm/OneClickBlackScreen/releases/latest`，自定义 UA `OneClickBlackScreen-Updater/1.0.0`）；手动「检查更新」再查。遮罩模式与 `--idle`（C 轨道）零联网代码路径——网络代码仅存在于 SettingsDialog/Updater 协作链。
- 不收集、不上报、无遥测；请求只发向 AppInfo.UpdateApiBase 声明域（src/ 单点）。自动检查不可关（B7 定案：频率＝开窗一次，无新增配置键）。
- 静默检查失败**完全无声**（探针断言点击前状态行为空）；仅手动检查明示原因。

## 5. 手动冒烟登记（UpdateSmoke 真消息 E2E）

| 场景 | 操作 | 结果 |
|---|---|---|
| 页脚版本 | 打开设置窗读 STATIC 文本 | `版本 1.0.0`（==AppInfo.Version）✅ |
| 断网（等价） | dist 沙箱副本（死代理 127.0.0.1:9，系统代理未动）→ BM_CLICK 检查更新 | 静默阶段无声（按钮 disabled≈1.2s 后恢复、状态行空）；点击后 `正在检查更新…`→`检查失败：无法连接到远程服务器`；窗口存活、WM_CLOSE 退出码 0 ✅ |
| 真实端点 | 真 dist 直连（经用户代理）→ BM_CLICK | `检查失败：远程服务器返回错误: (404) 未找到。`（仓库尚无已发布 Release，符合预期；顺带实证 TLS1.2/UA/全链路真往返）；窗口稳定退出 0 ✅ |
| 发现新版→下载→替换（真端点） | —— | **未跑**：无已发布新版可发现，按 GOAL 留 I 波（I4 发布后冒烟）。离线全链路已由 VerifyUpdate 覆盖（含 302/digest/SUMS/回滚） |

物理拔线未执行（用户机器在线且代理活动）；以沙箱死代理的「连接被拒」WebException 路径等价覆盖——与断网同为 `WebException→检查失败` 同一异常链。

## 6. 边界登记（B9＋本轮事实）

- **exe-only 更新**：未来 config/ico 变更需把更新器升级为 zip 模式（B9 原文登记，本轮维持）。
- **无 MOTW/SmartScreen**：WebClient 下载不经 Shell，无 MOTW 标记，替换落位不触发 SmartScreen；杀软实时扫描仍覆盖（B9）。
- **无新版零写盘**：检查不落盘；下载仅在用户点「立即更新」后发生（B9）。
- **下载中途关窗**：UI 线程替换天然串行（§3）；但用户在下载阶段（替换前）关窗→进程退出杀死后台线程，可能遗留一个 %TEMP% 随机名半成品文件（无 finally 机会）——无害、不参与任何替换，登记不改码。
- **125% 副屏探针本轮不可达**：DISPLAY2 已断开（仅 DISPLAY1@150%）。页脚几何由第 6 查 21 控件 ±1px 镜像（@144DPI）＋`dump-v5b-150.txt` 精确复算覆盖；S() 线性与 v4 125% 实证同源，建议 I 波双屏环境复跑 DumpLayoutDpi place。
- **状态行宽度 144 逻辑 px**（固定预留「立即更新」槽，fts 恒定几何）：超长失败原因走 AutoEllipsis 省略号（前缀「检查失败：/更新失败：」始终可见）；完整原因已在 MessageBox/日志路径外无副本，属预期取舍。
- **视觉备注**：深色截图页脚「版本」灰字＝ThemePalette.TextSecondary（#9E9E9E on #202020≈6.1:1，与 v4 冻结 hint 档同对，零新颜色 token）；外部图像分析器对深色「保存」按钮字色在缩放下误读为白字，权威口径＝ThemePalette.AccentText（#000000，10.5:1）＋第 6 查 Accent 采样锁定。
- **验证器与真端点的关系**：VerifyUpdate 全离线（HttpListener 127.0.0.1 随机端口，非管理员实测可绑）；每次打开设置窗产生的真实静默检查在本轮真端点为 404（无已发布 Release）→ 状态行恒空 → 第 6 查子控件集合确定性成立。I 波发布后 tag==AppInfo.Version 亦恒空；仅当「本地版本落后于已发布最新版」时状态行会变化——几何不受影响（槽位固定），登记为本验证器的既知口径。
- **Program.cs 改动审计**：仅 Main 入口 B6 一行区＋`using System.IO;`，`--settings`/无参分支逐字不动（git diff 可核）。

## 7. fts 清单核对（本轮增量）

- ☑ 零新颜色 token（页脚全走 Bg/TextSecondary/Accent/AccentText 既有键，ThemePalette 零改动）。
- ☑ 档位制：页脚行高 24=既有 LabelRowHeightPx 档；间距 8=ButtonGapPx 档；按钮宽 80=cancel 同档；无新时长档（无动效）。
- ☑ 恒定几何：「立即更新」槽固定预留（隐藏不塌缩不重排，fts「按钮格子固定预留」正用）；第 6 查锁定其矩形（含隐藏态——OnHandleCreated 物化保留槽句柄，子控件集合与网络状态解耦）。
- ☑ 单源纪律：页脚三常量入 SettingsDialogMetrics（附推导）；版本/仓库名/资产名/基址唯一登记 AppInfo（RESEARCH §4 跨轨表一致）。
- ☑ 反例库过检：A1（无幽灵 token，grep 零命中）、A4（固定预留正用）、A16（无新档位）等无未处置命中。

---

**轨道 B 完成，待 I 波集成**（合并顺序见 RESEARCH-V5 §4：B `--no-ff` 入 main → C rebase 重验 → 全量 7 件守门；I3 要求 tag v1.0.0 == AppInfo.Version）。

# GOAL-V5C · 空闲自动唤起（轨道 C）

> **一句话定位**：无操作 N 分钟（关/5/10/15/30，默认关）→全部屏幕自动出现遮罩，任意键退出并重新计时——Windows 计划任务（PT1M）驱动 `--idle` 自检，GetLastInputInfo 纯输入判定，零后台常驻、不联网、免管理员。
> **上游事实源**：[RESEARCH-V5.md](RESEARCH-V5.md) §1 U1、§3 R2、§4 并行契约、§5 C 系决策（C1–C10，冲突时以 RESEARCH 为准）　**编排**：[PLAN-V5.md](PLAN-V5.md) §3 波 1。
> **日期**：2026-11-02　行数预算：≤200 行。

## 0. 身份与工作方式

- 你是执行窗口，主模型 GLM-5.3，以**目标模式（/goal）**工作：只产出落地改动与可核对证据；计划/待办/耗时/结论式回复不构成完成依据。
- 第一性原理；**禁止打补丁**——空闲唤起必须作为 Program/MaskConfig/SettingsDialog 体系的有机扩展（新分支、新键、新区），不是外挂脚本或常驻小工具。
- 尽量用内置 ZCode 子代理；wbx 未登录则不用。
- 涉及设置窗 UI **必须先调用 fts-style-system skill**，遵守 SettingsDialogMetrics/ThemePalette 单源纪律与 v4 六查口径。

## 1. 上下文承接（冷启动固定顺序）

1. 读 `RESEARCH-V5.md` 全文（实现唯一依据＝§5 C 系＋§3 R2＋§4 所有权矩阵＋§2 冻结面）
2. 读 `src/Program.cs`、`src/MaskConfig.cs`、`src/SettingsDialog.cs`、`src/SettingsDialogMetrics.cs`、`src/ThemePalette.cs`、`src/OverlayForm.cs`（只读，理解 OverlayGroup）
3. 读 `verify/VerifyConfig.cs`、`verify/VerifySettings.cs`、`verify.cmd`、`build.rsp`
- **禁改**（§4 矩阵）：`src/AppInfo.cs`/`src/Updater.cs`（B 轨道将建，你的分支上不存在）、`src/Program.cs` 的 `--settings` 与无参分支语义、`README.md`、`使用说明.txt`、`install.ps1`、LICENSE/.gitattributes 等 A 轨道写面。
- 与 B 并行：若 `feature/update-v5` 分支存在**不读不碰**（I 波负责合并；你基于 main 切分支）。

## 2. 背景

本软件 C# WinForms、.NET FW 4.8、csc 直编（`-langversion:5`）、零依赖、进程即退、单实例互斥（第二实例静默退出——计划任务重复触发天然安全）。R2 已定案：「每 1 分钟计划任务启动 `--idle` 自检」胜出：IdleTrigger 的系统空闲判定＝无输入**且** CPU 低（后台噪声可致永不触发）被否决；计划任务默认电池双开关 true（笔记本静默失效）与默认 PT72H 执行时限（到期强杀进程树——遮罩可能长存活）都必须由 XML 显式纠正；本机实测 XML 须 UTF-16 带 BOM、任务名定 ASCII。

## 3. Phase 任务

### Phase 0 · 分支与基线
- `git checkout -b feature/idle-v5`（自 main）；`build.cmd`＋`verify.cmd` 基线全绿（五件套）。

### Phase 1 · MaskConfig 扩展（C5）
- 新增 `IdleMinutes` 属性（int，默认 0＝关）；档位表 `{0,5,10,15,30}` 常量单点声明。
- 解析：新键 `idle=`——缺失/非法/负数→0；档位外正整数→clamp 到档位表；**任何情况下不影响 v1 两键**（v1 键保持「非法→整文件默认」原契约，新键 per-key 回退，属契约扩展并在类注释登记）。Save 追加第三行 `idle=N`。
- `VerifyConfig` 扩展：round-trip/缺省/非法（此时 color 与 text 仍有效）/档位 clamp 四类断言（沿既有 18 例风格续编）。

### Phase 2 · IdleWake（C3、C4、C7）
- 新建 `src/IdleWake.cs`：
  - `IdleMilliseconds()`：`GetLastInputInfo` P/Invoke（`LASTINPUTINFO{cbSize,dwTime}`，cbSize=Marshal.SizeOf），`unchecked(Environment.TickCount - (int)dwTime)` 环绕减法；判定纯函数 `ShouldShow(int idleMs, int thresholdMs)`。
  - `TaskName` 常量＝`OneClickBlackScreen-Idle`（§4 单一登记表值，D 轨道清理脚本同源）。
  - `BuildTaskXml(string exePath)`：C3 全字段——TimeTrigger（StartBoundary=注册时刻）＋Repetition `PT1M` 无 Duration；Principals `InteractiveToken`/`LeastPrivilege`；Settings `DisallowStartIfOnBatteries=false`、`StopIfGoingOnBatteries=false`、`ExecutionTimeLimit=PT0S`、`MultipleInstancesPolicy=IgnoreNew`、`StartWhenAvailable=true`；Action Command=exePath、Arguments=`--idle`。
  - `Ensure(exePath)`：XML 写 `%TEMP%` 随机文件（**Encoding.Unicode**＝UTF-16 BOM）→`schtasks /create /tn X /xml 文件 /f`（幂等覆盖）；`Remove()`：`/delete /tn X /f`；`Exists()`：`/query /tn X /xml` 退出码 0/1。ProcessStartInfo＝`UseShellExecute=false`＋`CreateNoWindow=true`＋嵌套引号路径；退出码非 0 抛带 stderr 摘要的异常。临时文件 try/finally 必删。

### Phase 3 · Program `--idle` 分支（C2）
- Main 增第三已知参数 `--idle`：`MaskConfig.Load()`→`IdleMinutes<=0` 直接 return 0（任务在但功能关＝幂等安全）→`ShouldShow(IdleMilliseconds(), 阈值ms)` 为 false return 0（活动期秒退、无窗口）→true 则走既有 `RunOverlay()`（复用互斥/遮罩全套，第二实例静默退出兜底）。无参与 `--settings` 分支逐字不动；未知参数用法提示不变。

### Phase 4 · 设置窗「自动唤起」区（C8、U7/fts）
- 位于遮罩文字区块之后、hintLabel 之前：区块标签「自动唤起」（Bold 10.5pt）＋ComboBox（DropDownList：`关闭/5 分钟/10 分钟/15 分钟/30 分钟`）＋两行 9pt hint（「无操作所选时长后，所有屏幕自动出现遮罩；按任意键退出并重新计时」「由 Windows 计划任务驱动，无后台常驻，不联网」）。
- `SettingsDialogMetrics` 新增该区档位（沿 4px 网格既有档推导并附注释）；fixedChrome 增量重算 previewCap（S2 containment 口径）；零新颜色 token；双主题走现有 palette；恢复默认→档位置「关闭」。
- `SaveAndClose`：`saving.Save()` 成功后同步任务（C6）——`IdleMinutes>0`→`IdleWake.Ensure(Application.ExecutablePath)`；`==0`→`Remove()`（任务不存在时静默成功）；失败→MessageBox 警告具体原因，**配置不回滚**（下次保存自愈），`保存并试用` 路径同样先同步任务再启动预览。
- `VerifySettings` 更新：几何断言纳入新区控件（沿第 4/6 查口径），原 6 查断言语义不变。

### Phase 5 · VerifyIdle 验证器（C9）
- 新建 `verify/VerifyIdle.cs`：①`BuildTaskXml` 字段断言（C3 全项＋Command==传入路径＋UTF-16 BOM 字节）②真实闭环：Ensure→Exists 退出码 0→Remove→Exists 退出码 1（try/finally 必清理，失败也删）③负路径：写临时 settings（idle=5）→以该配置路径环境启动 `exe --idle`→断言 2 秒内退出且无窗口（可用既有 VerifyOverlay 的进程探测手法）④`ShouldShow` 档位纯函数表 ⑤本机活动期 `IdleMilliseconds()` sanity（0≤读数<60000）。
- `verify.cmd` 串入第 6 件（本分支编号 [6/6]；与 B 的同名行冲突由 I 波统一为 [7/7]，契约 §4）；`build.rsp` 增 `src\IdleWake.cs`。

### Phase 6 · 收尾
- `docs/VERIFY-V5C.md`：全绿摘要＋XML 全字段实证＋真实闭环输出＋负路径证据＋UAT 边界登记（C10：真实「离开 N 分钟触发」自动化不可达，留给用户验收）。
- 用户可见文案草稿落 `docs/COPY-V5C.md` 供 I 并入门面（本波禁改 README/使用说明）。

## 4. 铁律

1. 不打补丁；`--idle` 是 Program 体系的第三协作分支，非脚本外挂。
2. §2 冻结面逐字遵守（v1 四件套、第 5 查、遮罩行为、`--settings`、v1 两键契约、存量 token 键值）。
3. 任务名/档位表/MaskConfig 契约扩展各有唯一登记处；发现第二副本即删。
4. `--idle` 模式零联网、零窗口残留；活动期（未达阈值）最迟 2 秒退出。
5. schtasks 一切子进程 `CreateNoWindow`；XML 临时文件 finally 必删。
6. 计划任务只属于当前用户（InteractiveToken），绝不要求管理员。
7. RESEARCH-V5 事实有误→先登记其 §8 勘误再实现。

## 5. 验收清单（Done when，全部可核对）

1. `git rev-parse --abbrev-ref HEAD`＝`feature/idle-v5`，`build.cmd` 输出 `[OK] Build succeeded`；
2. `verify.cmd` 输出 `[OK] ALL VERIFIERS PASSED`（六件全绿，含 VerifyIdle 与扩展后的 VerifyConfig/VerifySettings）；
3. `schtasks /query /tn OneClickBlackScreen-Idle` 在验证结束后退出码＝1（闭环清理干净，无残留任务）；
4. `git grep -n "DisallowStartIfOnBatteries\|ExecutionTimeLimit\|PT1M" src/IdleWake.cs` 三项齐且值为 false/PT0S/PT1M；
5. `git grep -n "idle=" src/MaskConfig.cs` 解析与保存各仅一处；VerifyConfig 扩展含「非法 idle 不伤 v1 两键」断言；
6. 设置窗新区双主题截图（浅/深）各一张入 docs/VERIFY-V5C.md 证据（沿 v4 目检口径）；
7. `docs/VERIFY-V5C.md` 与 `docs/COPY-V5C.md` 存在且含 UAT 边界登记；
8. 手动冒烟登记：设置窗选 5 分钟→保存→`schtasks /query` 存在→改「关闭」→保存→任务消失；`exe --idle`（活动期）秒退无窗。

## 6. 收尾动作

- 全部改动提交于本分支（不合并、不推送、不动 main 与 B 分支）；
- docs/VERIFY-V5C.md 末尾标注「轨道 C 完成，待 I 波集成」；后续衔接由用户开 I 窗口。

# VERIFY-V5C · 空闲自动唤起（轨道 C）验证交付

> **一句话定位**：`feature/idle-v5` 分支（自 main 切出）空闲自动唤起功能的全部可核对证据——六件验证器全绿、任务 XML 全字段实证、真实注册闭环、负路径、双主题目检、端到端冒烟。
> **上游**：[RESEARCH-V5.md](../RESEARCH-V5.md) §5 C1–C10／§3 R2／§4 所有权矩阵　**编排**：[PLAN-V5.md](../PLAN-V5.md) §3 波 1。
> **日期**：2026-11-02　**分支**：`feature/idle-v5`（commit 见 §7）

## 1. 全绿摘要

- `build.cmd` → `[OK] Build succeeded: D:\Download\Agent Vault\一键黑屏\dist\一键黑屏.exe`（build.rsp 新增 `src\IdleWake.cs` 一行）。
- `verify.cmd` → `[OK] ALL VERIFIERS PASSED`，六件输出：

```
[1/6] VerifyConfig
PASS: 23 config tests (defaults/roundtrip/format/golden/corruption/escapes/atomic-write/idle-key)
[2/6] VerifySettings
PASS TOTAL: 6 settings checks (3 original + theme background + Enter contract + geometry mirror)
[3/6] VerifyMouseExit
PASS TOTAL: 5/5 buttons; 30 movement messages; 10 wheel messages; 1 windows per launch
[4/6] VerifyOverlay
PASS TOTAL: A-F; 1 displays; max InputIdle 107 ms (limit 2000)
[5/6] VerifyIcon
PASS TOTAL: 4 icon checks
[6/6] VerifyIdle
PASS TOTAL: 5 idle checks (xml fields / scheduler loop / silent exit / decision table / live reading)
[OK] ALL VERIFIERS PASSED
```

- 验证结束后 `schtasks /query /tn OneClickBlackScreen-Idle` 退出码＝**1**（无残留任务，双保险：VerifyIdle try/finally 清理＋失败路径兜底 Remove）。

### 冻结面核对（RESEARCH §2）

- VerifyConfig 原 18 例**逐字未动**，仅续编 19–23 例与汇总行；第 5 查（Enter 契约）逐字未动且仍绿。
- 冻结得以成立的实现裁决（C5「Save 追加第三行」×「18 例/第 5 查逐字不动」的字节级断言互斥）：`idle=N` 第三行**仅在 IdleMinutes>0 时写出**——默认/关闭态保存与 v1 两行格式字节一致（例 23 断言双方向）。开启态保存即三行，C5 语义完整保留。
- `--settings` 与无参分支逐字不动（Program.cs 仅插入 `--idle` 一行＋一个私有方法）；未知参数用法提示不变。

## 2. 任务 XML 全字段实证（C3）

`IdleWake.BuildTaskXml(exe)` 真实输出（_probe\DumpXml，2026-11-02T22:48:43 本机导出；`encoding="UTF-16"` 声明＋文件由 `Encoding.Unicode` 写出＝UTF-16 LE 带 BOM，VerifyIdle PASS 1 逐字节断言 `FF FE` 前缀与解码回读一致）：

```xml
<?xml version="1.0" encoding="UTF-16"?>
<Task version="1.3" xmlns="http://schemas.microsoft.com/windows/2004/02/mit/task">
  <Triggers>
    <TimeTrigger>
      <StartBoundary>2026-11-02T22:48:43</StartBoundary>
      <Enabled>true</Enabled>
      <Repetition>
        <Interval>PT1M</Interval>
      </Repetition>
    </TimeTrigger>
  </Triggers>
  <Principals>
    <Principal id="Author">
      <LogonType>InteractiveToken</LogonType>
      <RunLevel>LeastPrivilege</RunLevel>
    </Principal>
  </Principals>
  <Settings>
    <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>
    <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
    <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
    <StartWhenAvailable>true</StartWhenAvailable>
    <ExecutionTimeLimit>PT0S</ExecutionTimeLimit>
  </Settings>
  <Actions Context="Author">
    <Exec>
      <Command>"D:\Download\Agent Vault\一键黑屏\dist\一键黑屏.exe"</Command>
      <Arguments>--idle</Arguments>
    </Exec>
  </Actions>
</Task>
```

- 字段断言（VerifyIdle PASS 1，13 项 Contains＋反断言）：PT1M 无 `<Duration>`（无限重复）；电池双开关显式 false（笔记本不断电静默）；ExecutionTimeLimit=PT0S（默认 PT72H 到期会经 job object 强杀进程树——遮罩长存活必须关掉）；IgnoreNew（遮罩存活期跳过重复触发，退出按键本身刷新 lastInput，天然「退出→再空闲→再弹」）；StartWhenAvailable；InteractiveToken＋LeastPrivilege（仅当前登录用户、免管理员）；Command 带嵌套引号（含空格路径如上）；Arguments=`--idle`；StartBoundary=注册时刻（TryParseExact 解析＋±1 分钟窗口断言）。
- `git grep -n "DisallowStartIfOnBatteries\|ExecutionTimeLimit\|PT1M" src/IdleWake.cs` → 三行齐，值 false/PT0S/PT1M（IdleWake.cs:63/75/78）。

## 3. 真实闭环与负路径（C9）

**VerifyIdle 五查**（真 schtasks、真 dist exe）：

1. `PASS 1: task xml carries the C3 field set, quotes the exe, writes UTF-16 BOM`。
2. `PASS 2: Ensure -> query exit 0 -> Remove -> query exit 1 (task cleaned up)`——真注册→真查询→真删除，try/finally 保证任何失败路径也清理。
3. `PASS 3: active-session --idle exited within 2 s, exit code 0, zero windows`——负路径：临时 LOCALAPPDATA 树写入 `idle=5` 配置（不碰用户真实 settings.ini），子进程枚举窗口每 50ms 断言零可见窗口，2 秒预算内静默退出（铁律 4）。
4. `PASS 4: ShouldShow decision table (8 cases, threshold edges included)`——纯函数表含阈值两侧边界（299999/300000/300001）、threshold≤0 恒 false。
5. `PASS 5: live IdleMilliseconds() = <读数> ms (0 <= reading < 60000)`——探针先合成鼠标输入刷新 lastInput，读数确定性落在活动期窗口。

**端到端冒烟（验收 #8，_probe\IdleSmoke，真设置窗＋真任务）**：

```
SMOKE A: saved 5 min -> task exists (query exit 0), config has idle=5
SMOKE B: saved off -> task gone (query exit 1), config back to v1 format
SMOKE C: --idle (idle=off config) exited instantly, code 0, zero windows
SMOKE TOTAL: A/B/C all passed
```

- A/B 用真实键盘消息（AttachThreadInput＋VK_DOWN/VK_UP → CBN_SELCHANGE 真实触发 SelectedIndexChanged）驱动设置窗下拉，BM_CLICK 点「保存」，关闭「已保存」确认框——完整走通 SaveAndClose→`Save()`→`SyncIdleTask()`（C6）链路；用户 settings.ini 字节级快照/恢复，任务最终删除。
- C 为「任务语义关但功能关」的幂等路径（IdleMinutes≤0 直接 return 0）。

## 4. MaskConfig 契约扩展（C5）

- 新键 `idle=N` per-key 回退：缺失/非法（`abc`/`-5`/空/`3.5`）→0 且 **v1 两键不受任何影响**（VerifyConfig 例 21 四组断言）；v1 两键保持「非法→整文件默认」原契约（原 18 例逐字仍在）。
- 档位 clamp 登记于 `ClampIdleMinutes`（1..4→5；档间取不超过手写值的最大档：6..9→5、11..14→10、16..29→15；>30→30——遮罩永不迟于手写值；例 22 十三组断言含 `int.MaxValue`）。
- 档位表唯一登记：`MaskConfig.IdleMinutesTiers = {0,5,10,15,30}`；设置窗下拉 items 由该数组生成（索引 1:1 映射），VerifyIdle/SettingsDialog 均为引用方，零副本。
- `git grep -n "idle=" src/MaskConfig.cs` → 恰两行：解析（:112，键分支处注释标注序列化形态）与保存（:220），各仅一处。

## 5. 设置窗「自动唤起」区（C8/U7/fts）

**结构**（遮罩文字区块后、hintLabel 前）：区块标签「自动唤起」（Bold 10.5pt＝Body Strong 档）＋ComboBox(DropDownList, OwnerDrawFixed)＋两行 9pt hint（原文见 §6 目检）；Tab 序插入 textBox 之后（reset/cancel/try/save 顺延）。

**Metrics 单源新增仅一个常量** `IdleComboWidthPx = 132`（注释附 em 推导：最宽项「30 分钟」≈47 文本＋下拉钮 ~17＋内缩 ~14≈78 平最小，132＝4px 网格留呼吸）；行高全部复用既有档（LabelRowHeightPx/ButtonRowHeightPx/HintHeightPx），间距零新档（8/12/20 既有 rung）；零新颜色 token（下拉 owner-draw 用 palette.Surface/Text/ButtonHover＋SystemColors，深浅两主题走现有 palette）；零动效。

**Combo 几何根修记录**（ComboProbe 实证，_probe\ComboProbe）：原生 DropDownList 自动高度＝字体驱动且 DPI 非线性（28@96／36@144，S() 任何常量都无法镜像）；`OwnerDrawFixed` 下 realized＝ItemHeight＋6 物理px（96/144 一致）且 ItemHeight 可控——LayOutControls 运行时标定 `chrome = Height - ItemHeight` 后置 `ItemHeight = S(32) - chrome`，行高精确落 `ButtonRowHeightPx` 档（144dpi 实测 48px＝S(32)，VerifySettings 第 6 查 ±1px 锁定）。Owner-draw 附带收益：深色主题下拉列表随 palette 渲染（原生 ComboLBox 列表永不跟深色应用主题的问题一并消除）。

**第 6 查镜像更新**：expected 字典纳入 `idle label/idle combo/idle hint` 三项（含 COMBOBOX 类识别）、fixedChrome 同式增量重算、子控件计数 17→20；第 1–5 查断言语义逐字未动（含第 4 查采样条带与全部可见子控件不相交断言——新区控件在内容列内，天然不相交）。

**SaveAndClose（C6）**：`Save()` 成功后 `SyncIdleTask`（>0→`Ensure(Application.ExecutablePath)`；==0→`Remove()`，任务不存在静默成功）；失败→MessageBox 警告具体原因、**配置不回滚**（下次保存自愈）；「保存并试用」路径同样先同步任务再启动预览。恢复默认→下拉置「关闭」。

## 6. 双主题目检（v4 口径）

| 图 | 证据 |
|---|---|
| 浅色截图 | `_probe\v5c-light.png`（SnapV5C 真 exe PrintWindow 666×1181 全窗口，主屏 150%，系统浅色） |
| 深色截图 | `_probe\v5c-dark.png`（反射强制 ApplyTheme(false)，Show 后调用使 DWM 帧属性同生失效修复，主屏 150%，不触系统主题注册表） |

机器视觉逐项（两图均过）：①「自动唤起」标签**粗体**且明显大于灰提示（两档字阶成立）；② 下拉框显示「关闭」、深浅主题各自底色（浅=窗白/深=Surface 深底浅字）、文字垂直居中无截断、下拉箭头正常；③ 两行提示原文完整：「无操作所选时长后，所有屏幕自动出现遮罩；按任意键退出并重新计时」「由 Windows 计划任务驱动，无后台常驻，不联网」；④ 深色标题栏深色（与 v4-dark-final 基线一致）；⑤ 保存=亮蓝底黑字（深色档黑对 #4CC2FF＝10.47:1，v4 冻结值），其余按钮深灰底浅字；⑥ 全图无重叠/截断/错位/脏块/洋红。截图含用户真实配置文本（对话框加载真实 settings.ini，与 v4 同口径）。

## 7. 验收清单对照（GOAL §5 逐项）

| # | 验收项 | 证据 |
|---|---|---|
| 1 | 分支名＋build 绿 | `feature/idle-v5`；`[OK] Build succeeded`（§1） |
| 2 | verify 六件全绿 | `[OK] ALL VERIFIERS PASSED`（§1） |
| 3 | 结束后任务查询退出码 1 | §1 末行实测＝1 |
| 4 | grep 三字段齐且值正确 | §2 末（IdleWake.cs:63/75/78） |
| 5 | `idle=` 解析/保存各一处＋非法不伤 v1 断言 | §4（:112/:220；VerifyConfig 例 21） |
| 6 | 双主题截图各一张入证据 | §6 两图 |
| 7 | 两 docs 存在且含 UAT 边界登记 | 本文件 §8＋[COPY-V5C.md](COPY-V5C.md) |
| 8 | 手动冒烟登记 | §3 冒烟 A/B/C（键盘驱动真设置窗） |

## 8. UAT 边界登记（C10，自动化不可达项）

- **真实「离开 N 分钟触发」**：自动化无法保证「N 分钟零输入」且遮罩弹出属全屏交互，未做自动验证——留给用户 UAT：选 5 分钟保存→停止键鼠 5 分钟→遮罩自动出现→按任意键退出→再静置应再次出现（IgnoreNew＋退出输入刷新 lastInput 的复合语义）。
- 锁屏/安全桌面/UAC 不覆盖（继承 v1）；RDP 断开会话读数未定义不处理。
- 任务 XML 导入行为随 Windows 版本（本机 Win11 26300 实测：UTF-16 BOM 必须、PT0S/IgnoreNew/电池双 false 全部接受）；旧版本 Windows 未覆盖。
- 计划任务归当前用户所有；若用户以其他账户登录，任务仍在原账户下（InteractiveToken 语义），此边界属 Task Scheduler 行为不处理。
- 下拉**展开列表**的像素级断言未做（ComboLBox 弹出层不在窗口 PrintWindow 内）；owner-draw 列表配色以源码（DrawIdleItem 走 palette）＋闭合态截图覆盖，登记为边界。

## 9. 与 I/D 波的交接

- verify.cmd 本分支编号 `[1/6]..[6/6]`；与 B 波同名行冲突由 I 波统一为 `[7/7]`（RESEARCH §4/§6）。
- 跨轨常量唯一登记处未变：任务名 `OneClickBlackScreen-Idle`＝`src/IdleWake.cs` `TaskName`（D 清理脚本同源引用）；本分支未建 AppInfo.cs/Updater.cs（B 轨道写面）。
- 用户可见文案草稿见 [COPY-V5C.md](COPY-V5C.md)（本波未改 README/使用说明——A/I 写面）。

---

**轨道 C 完成，待 I 波集成。**

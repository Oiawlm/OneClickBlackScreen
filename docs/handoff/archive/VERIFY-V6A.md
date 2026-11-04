# VERIFY-V6A · 退出健壮性根修证据（轨道 A：幻影键 × 布防闸门）

> **一句话**：布防从「1..254 全部键静止」改为「v1 宽限窗＋统一降级闩锁」——幻影键不再永久禁用键盘/鼠标双路退出，I1–I3 逐点保持。分支 `feature/exit-arm-v6`（自 main @ 5f4f058），2026-11-03。写面＝`src/OverlayForm.cs`、`verify/VerifyMouseExit.cs`、`verify/VerifyOverlay.cs`、本文。上游事实源＝RESEARCH-V6 §2/§8。

## 1. 红基线（Phase 0，dist＝未修复 v1.0.0）

- 旧码 `build.cmd`：`[OK] Build succeeded: D:\...\dist\一键黑屏.exe`
- 探针＝RESEARCH §8 配方的临时工具（csc 编译于 `%TEMP%\v6a-probe\`，**不入仓库**；ESC down 不 up → 启动 → 3s → SPACE down/up → 观察 5s）：
  - `Probe.exe <exe> healthy` → `healthy EXITED after SPACE (exit code 0)`（机器键态健康）
  - `Probe.exe <exe> phantom` → `phantom STILL RUNNING after SPACE (5s)` ← **缺陷复现成立**
- 键态收尾：探针 finally 注入 ESC up（一切路径配对，见 §8）。

## 2. 设计定案（Phase 1，先设计后编码）

### 2.1 字面参考方向的边界推演（重要发现，不可照抄）

RESEARCH §2 参考方向「快照集 L 完全不阻塞布防」与**冻结用例 E 冲突**。推演（用例 E：SHIFT 先于启动按住 → 启动 → ≈+400ms 注入 ESC down/up → +1200ms 断言遮罩未退出）：

| 时刻 | 事件 | 字面 L 设计下的状态 |
|---|---|---|
| t≈0 | SHIFT down（先于进程） | 快照 L={SHIFT} |
| ≈+100ms | armTimer tick 1 | SHIFT∈L 不阻塞、其余键全抬起 → **armed=true** |
| ≈+400ms | ESC down/up（Launch 内 400ms 睡眠后注入） | armed 且 ESC∉L → **遮罩退出** |
| ≈+1200ms | 用例 E 断言 `!HasExited` | **FAIL（"Overlay exited before arming"）** |

→ 字面设计击穿 E（`verify/VerifyOverlay.cs` 冻结断言），违反 I3「现有断言语义不得放宽」。真实启动键与幻影键在快照时刻**不可区分**（同为「状态按下、无事件」），唯一可靠判别是**未来是否被释放**——即必须有界的等待，再降级。

### 2.2 定案：统一宽限窗闩锁（grace-window latch）

一个机制统一覆盖启动快照、晚到幻影、长按启动键（复用既有 armTimer 100ms 节拍与 1..254 扫描，v1 同款循环演化）：

- 键被**观察到一次抬起** → 清计数＋解闩（此后该键可正常触发退出）；
- 键持续按下：未布防时逐 tick 计数，**< ArmGraceTicks(10 tick≈1s) 内照 v1 阻塞布防**；≥10 tick **降级置闩**——不再阻塞布防，仅压制该键自身的退出输入，直至轮询观察到抬起；
- **布防后新按下的键永不置闩**（慢点击/慢长按照 v1 直接可退出）；
- `armed` 单调不复位；计时器在「已布防且无待观察释放的闩」时停表——**干净键态第 1 tick 即布防并停表，与 v1 逐点一致**；
- OnKeyboard：armed && KEYDOWN/SYSKEYDOWN && 该键未闩 → 退出＋吞（吞事件面保持＝现状仅退出键）；**已闩键照常 CallNextHookEx 放行，不吞**；
- OnMouseUp(e.Button)：armed && 该按钮未闩 → 退出；启动点击的尾巴（已闩按钮 up 落在遮罩上）不退出（I1 鼠标面）。

参数约束：宽限 1s > 用例 E 的 ≈400ms 探针（余量 ≈600ms），且覆盖人类松开启动键的合理窗口；干净机器完全不受影响。

### 2.3 被否决备选（≥2）

1. **字面快照集 L（L 不阻塞布防）**——击穿冻结用例 E（§2.1 推演实锤）；冻结断言优先于参考方向。
2. **钩子事件真伪判别**（以「有无真实 KEYDOWN/自动重复事件」区分幻影）——键盘自动重复约 500ms 延迟才启动，与 E 的 400ms 探针构成竞态；VK_LBUTTON/RBUTTON 从不自动重复；验证器自身即注入者，「早于进程启动的注入」在钩子视野外。判别不可靠。
3. **超时强制 armed 看门狗**（旁路线程到点直接置 armed）——GOAL 明令禁止旁路/看门狗补丁；且无闩锁的超时布防会让长按启动键的自动重复在超时点直接退出，反噬 I1。

### 2.4 边界核验表（GOAL Phase 1 逐项要求）

| 边界 | 结论 |
|---|---|
| 启动键长按自动重复 | 已闩 → KEYDOWN 放行不退出；抬起解闩后再按可退出（新用例 G 锁定） |
| VK_LBUTTON/RBUTTON ∈ 1..254 | 参与同一扫描/闩锁；OnMouseUp 按钮级查闩；armed 后真实点击永不置闩 → 照 v1 退出 |
| 布防窗口内新出现幻影键 | 同一计数器：宽限窗内照 v1 阻塞、窗外降级置闩 → 布防必然完成（I2） |
| 闩锁键 KEYDOWN 放行不吞 | OnKeyboard 显式 fall-through 到 CallNextHookEx；吞事件面仍＝仅退出键 |
| armed 后慢点击/慢长按 | 永不置闩（仅未布防才计数）→ up 即退；VerifyMouseExit 五键 down→up 间隔 ≈0.8-1.2s 实证不误闩 |
| 启动点击尾巴 | 已闩按钮的 up 不退出（NotifyMouseUp 查闩）＝v1「armed 前不退出」同观感 |

## 3. 实现（Phase 2，仅 `src/OverlayForm.cs`，+105/-20 行）

四处内聚改动：`NativeMethods.KbdLlHook` 结构体（vkCode 提取）；ctor 匿名委托改 `ArmTick` 方法；`ArmTick`（§2.2 全逻辑）；`OnKeyboard` 闩锁放行；`NotifyMouseUp(e.Button)` 按钮闩锁＋`ButtonKey` 映射。C#5、零新增文件/引用、UI 零触碰。修复后 `build.cmd`：`[OK] Build succeeded: D:\...\dist\一键黑屏.exe`。

## 4. 验证器扩展（Phase 3）

- **VerifyOverlay 新增 G**（启动键自闩锁防误触）：SHIFT 先于启动按住 → 过宽限窗 → 注入 3 次自动重复 down → 断言不退出 → SHIFT up → 同键退出。
- **VerifyOverlay 新增 H**（幻影键回归）：ESC down 不 up → 启动 → 过宽限窗 → 断言幻影单独不关遮罩 → SPACE 必须退出。
- **VerifyMouseExit 新增幻影×鼠标路**：ESC down 不 up → 启动 → 过宽限窗 → 左键 down/up 消息 → 必须退出。
- 注入卫生：G/H/鼠标件全部 finally 强制配对 up（双层）；既有 A–F 与五键用例**原样零改动**（仅 PASS TOTAL 文案更新为 A-H / phantom-key click exit OK）。

## 5. 红绿两端实录（对 dist 同一命令，仅换构建产物）

### 5.1 修复前（旧 dist；验证器已含新用例）

```text
> verify\bin\VerifyOverlay.exe "dist\一键黑屏.exe"
PASS A … PASS F … PASS G: held launch key auto-repeat passed through; exit after release
FAIL: Keyboard key 0x20 failed to close overlay          ← H 红，exit=1

> verify\bin\VerifyMouseExit.exe "dist\一键黑屏.exe"
PASS: Left/Right/Middle/X1/X2 …（5/5 全 PASS）
FAIL after 5/5 buttons, 30 moves, 10 wheel messages: Left button up did not close overlay under phantom key   ← exit=1
```

（A–F＋G 在旧 dist 上全绿＝冻结语义未被新用例扰动；红仅落在新幻影回归件。注：VerifyMouseExit 首跑遇一次环境瞬态 `exited before showing windows (exit code 0)`，立即复跑即按预期红在幻影件——旧二进制上的一次性互斥竞态，与修复无关、不可复现。）

### 5.2 修复后（新 dist，两轮一致）

```text
> verify\bin\VerifyOverlay.exe "dist\一键黑屏.exe"
PASS A … PASS E: held key blocked arming, unarmed key ignored, exit after release
PASS F … PASS G … PASS H: phantom held key did not block arming; SPACE exited
PASS TOTAL: A-H; 1 displays; max InputIdle 450 ms (limit 2000)   ← exit=0（复跑 140 ms 同绿）

> verify\bin\VerifyMouseExit.exe "dist\一键黑屏.exe"
PASS: Left/Right/Middle/X1/X2 …（5/5 全 PASS）
PASS: Left click under phantom held key exited
PASS TOTAL: 5/5 buttons; 30 movement messages; 10 wheel messages; 1 windows per launch; phantom-key click exit OK   ← exit=0（复跑同绿）
```

### 5.3 探针 A/B（RESEARCH §8 同配方）

| dist | healthy | phantom |
|---|---|---|
| 修复前 v1.0.0 | EXITED after SPACE (exit code 0) | **STILL RUNNING after SPACE (5s)** |
| 修复后 | EXITED after SPACE (exit code 0) | **EXITED after SPACE (exit code 0)** |

## 6. 六件单跑全绿（验收 §5.3；命令＋末行实录）

| # | 命令 | 末行输出 |
|---|---|---|
| 1 | `verify\bin\VerifyConfig.exe` | `PASS: 23 config tests (defaults/roundtrip/format/golden/corruption/escapes/atomic-write/idle-key)` |
| 2 | `verify\bin\VerifyIcon.exe "dist\一键黑屏.exe"` | `PASS TOTAL: 4 icon checks` |
| 3 | `verify\bin\VerifyUpdate.exe` | `PASS TOTAL: update checks (version table + offline full chain)` |
| 4 | `verify\bin\VerifyIdle.exe "dist\一键黑屏.exe"` | `PASS TOTAL: 5 idle checks (xml fields / scheduler loop / silent exit / decision table / live reading)` |
| 5 | `verify\bin\VerifyMouseExit.exe "dist\一键黑屏.exe"` | `PASS TOTAL: 5/5 buttons; …; phantom-key click exit OK` |
| 6 | `verify\bin\VerifyOverlay.exe "dist\一键黑屏.exe"` | `PASS TOTAL: A-H; 1 displays; max InputIdle 450 ms (limit 2000)` |

（bin 经 verify.cmd 同款 csc 命令逐件重建；`[2/7] VerifySettings` 属 B 轨写面，本轮不跑不改。）

## 7. I1–I3 逐项对照

| 不变量 | 证据 |
|---|---|
| **I1 防误触**（启动键在被观察到一次抬起前不得自身触发退出） | G：长按 SHIFT 自动重复 3 次不退出、抬起后同键才退出；E 原样通过（400ms 探针落在宽限窗内）；鼠标面＝OnMouseUp 按钮闩锁挡住启动点击尾巴 |
| **I2 可用性**（幻影键不得阻止布防、不得禁用任一退出路） | H＋探针 A/B：幻影 1s 宽限后降级置闩、布防必然完成，SPACE 与左键点击双路均退出 |
| **I3 观感不变**（移动/滚轮不退、真实按键/点击退出、100ms 节拍、现有断言不放宽） | A–F＋五键用例零改动全绿（两轮）；干净键态第 1 tick 布防并停表＝v1 逐点；吞事件面仍＝仅退出键；宽限窗内（<1s）行为＝v1 等待抬起 |

唯一语义增量（属 I2 目标域、非断言放宽）：宽限窗外，被闩键**以外**的退出输入在 v1 中于该态永久不可用、现可用；干净机器与宽限窗内观感与 v1 逐点一致。

## 8. 卫生与边界

- 注入配对：G（三层 SHIFT up）、H（双层 ESC up）、VerifyMouseExit 幻影件（finally ESC up）、探针（finally ESC up）——一切路径含失败均配对；终检 `Probe.exe scan` → `scan: ALL KEYS UP (clean state)`（1..254 全抬起）。
- 探针与产物只落 `%TEMP%\v6a-probe\`；仓库 `git status` 除本白名单三源文件＋本文档外零改动、无未跟踪文件。
- 冻结面零触碰：verify.cmd/build.cmd/build.rsp/README/使用说明/migrate/baseline/其余 src/AppInfo 版本号均未动；不 merge、不发版（I 波辖内）。

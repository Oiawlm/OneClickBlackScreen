# GOAL-V6B · 验证器环境稳健化（轨道 B）

> **一句话定位**：根修 VerifySettings 的 IME 环境敏感（25 子窗非确定性失败）——子窗枚举改「应用所有权正向判别」（WinForms 类前缀）后断言 24；并完成全部验证器注入卫生审计（证据驱动，只修有实据者，禁止投机性加固）。
> **上游事实源**：[RESEARCH-V6.md](RESEARCH-V6.md) §3（根因二）、§4（卫生审计项）、§1（体检表）（冲突时以 RESEARCH 为准）　**编排**：[PLAN-V6.md](PLAN-V6.md) §3 波 1。
> **日期**：2026-11-03　行数预算：≤140 行。

## 0. 身份与工作方式

- 你是执行窗口，主模型 GLM-5.3，以**目标模式（/goal）**工作：只产出落地改动与可核对证据；计划/待办/耗时/结论式回复不构成完成依据。
- 第一性原理；**禁止打补丁**——镜像锁的断言语义要重设计为「观察应用自有控件树」，**不得**用类名黑名单逐个剔除（OimeTsfNotifyWindow 明天可能换成别的类名＝打补丁）。
- 尽量用内置 ZCode 子代理（注入配对扫描等机械任务可用 Flash 子代理扇出）；wbx 桥未登录则不用。
- 本轮无 UI/视图改动。

## 1. 上下文承接（冷启动固定顺序）

1. 读 `RESEARCH-V6.md` 全文（唯一事实源；实现依据＝§3 修复方向＋§4 审计项）
2. 读 `verify/VerifySettings.cs` 全文（重点：子窗枚举与 expected 清单、镜像锁既有口径、与 `src/SettingsDialogMetrics.cs`/`src/ThemePalette.cs` 的协同编译关系）
3. 读 `verify.cmd`（协同编译命令行与件序；**本轮冻结不改**）
4. 其余 verify 文件按审计需要 grep 定位读（keybd_event/mouse_event/schtasks 计划任务清理），不通读
- **禁改**：`src/**`（A 轨含 OverlayForm.cs 之外的一切也不归你）、`verify/VerifyMouseExit.cs`、`verify/VerifyOverlay.cs`（A 轨写面——它们的注入卫生由 A 轨负责，你只读审计并在证据文档登记移交注记）、`verify.cmd`/`build.cmd`/`build.rsp`、`README.md`、`migrate/**`、`baseline/**`。
- **禁读**：GOAL-V5*/RESEARCH-V1..V5 全文（防上下文膨胀）。

## 2. 背景

PC2（Win11 26200＋微软拼音）实测：微软 TSF 框架向设置窗注入隐藏子窗 `OimeTsfNotifyWindow`（vis=False），是否出现**非确定**——同一台机器 verify.cmd 第一次 `saw 25` 失败、第二次（该窗未出现）七件全绿（RESEARCH-V6 §3）。应用自有的 24 控件全部为 `WindowsForms10.*` 类前缀（实测枚举留档）。缺陷在验证器的断言方法学：对裸 EnumChildWindows 计数，把 OS 家具算进了应用控件树。另：本机曾实测到外部软件注入的幻影 ESC——验证器自身的注入卫生（down 必有一切路径配对 up）必须审计，防止验证器自己毒化环境。

## 3. Phase 任务

### Phase 0 · 分支与红基线
- `git checkout -b feature/verify-env-v6`（自 main）；`verify.cmd` 编译环节绿。
- 红基线：反复单跑 `verify\bin\VerifySettings.exe "dist\一键黑屏.exe"` 直至捕获一次 `saw 25`（IME 态非确定，可多次运行/开关输入法诱发；若多轮均 24，以 RESEARCH-V6 §3 第一次失败实录为红基线并在证据文档说明诱发尝试）。

### Phase 1 · 应用所有权正向判别（仅 `verify/VerifySettings.cs`）
- 子窗枚举后按「类名前缀 `WindowsForms10.`」正向过滤，再对过滤结果断言 expected 清单（24 件与既有 ChildInfo 匹配逻辑不变）。
- 判别式实现须自包含（本文件已有 GetClassName P/Invoke 可复用）；断言失败信息保留原可读口径（类名/文本/矩形）。
- 镜像锁其余查（颜色/几何/主题/回车语义）**断言语义一律不动**。

### Phase 2 · 注入卫生审计（全部 verify/*.cs，只读＋有实据才修）
- 扫描每个 keybd_event/mouse_event/SendInput 调用点：down 是否在一切路径（含 Check 失败提前 return/异常）有配对 up；VerifyIdle 的计划任务在失败路径是否清理。
- **只修有实据者**（能构造出一条「失败路径留下未配对 down/未清理任务」的具体序列才改）；修法＝把配对 up/清理纳入 finally 或统一收尾段，融入既有结构。无实据的文件一行不动，审计表登记「核查通过」。
- A 轨两文件（MouseExit/Overlay）**只读**：发现实据则写入证据文档「移交注记」（I 波转 A 补），不得直接改。

### Phase 3 · 证据与推送
- `docs/VERIFY-V6B.md`：红基线（诱发尝试记录）→绿输出（连续×3）、判别式设计说明（为何是正向前缀而非黑名单）、卫生审计表（逐文件：核查点/实据/处置）。
- `git push -u origin feature/verify-env-v6`。

## 4. 铁律

1. 不打补丁：黑名单剔除、按机器开关跳过断言、放宽 expected 计数——全部禁止。
2. 写面只限：`verify/VerifySettings.cs`、（仅实据触发）`verify/VerifyConfig.cs`/`VerifyIcon.cs`/`VerifyUpdate.cs`/`VerifyIdle.cs`、`docs/VERIFY-V6B.md`。
3. 本分支产品缺陷（幻影键）未修属预期：干净键态下六件本就绿；若单跑 VerifyMouseExit/VerifyOverlay 因环境键态失败，先清键态复跑（参考 RESEARCH-V6 §8 扫描配方），不得改这两个文件。
4. `verify.cmd` 件序/命令行冻结；如判别式需要协同编译新增源文件——不允许（复用本文件既有 P/Invoke，自包含）。
5. `migrate/**` 与 SHA256SUMS.txt 不可触碰；凭证不入任何文件。
6. 不 merge main、不发版本（I 波辖内）。

## 5. 验收清单（校验器只认实据）

1. `git log origin/feature/verify-env-v6` 可见分支已推送；`git diff --name-only main...HEAD` ⊆ §4.2 白名单。
2. `verify\bin\VerifySettings.exe "dist\一键黑屏.exe"` 连续 3 次运行全部 PASS（输出含既有 PASS 行；三次实录留档）。
3. 干净键态全套件绿：`verify.cmd` 末行 `[OK] ALL VERIFIERS PASSED`（一次实录留档；若 IME 态诱发不出红基线，此项即为绿主体证据）。
4. docs/VERIFY-V6B.md 含：判别式设计说明、卫生审计表（verify 七件全覆盖）、（如有）A 轨移交注记。
5. 工作树干净（无未跟踪临时文件）。

## 6. 收尾动作

- 证据文档落档后即停（不 merge、不发版）；提示用户按 PLAN-V6 §5 开 I 窗口。
- 若多次诱发仍无法复现 `saw 25`，判别式仍按 RESEARCH-V6 §3 事实实施（非确定性缺陷不因一次未复现而不修），并在证据文档如实记录诱发尝试。

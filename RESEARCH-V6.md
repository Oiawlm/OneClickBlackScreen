# RESEARCH-V6 · 「迁移后体检 → 退出健壮性根修」唯一事实源

> **一句话定位**：v6 轮（迁移后优化）唯一事实源：PC2 环境体检结论（.NET 零安装）、两个根因（幻影键×布防闸门＝产品无法退出；IME 子窗口×固定计数＝验证器非确定性断言）、修复方向与冻结不变量、并行与模型选型依据。执行窗口以本文为唯一事实源；与 GOAL 冲突时以本文为准。
> **来源**：用户 2026-11-03 /goal 需求＋两项对齐定案（§7）＋规划窗口本机实测（§1–§5，全部命令可复跑）。
> **日期**：2026-11-03　行数预算：≤170 行。

## 1. PC2 环境体检（.NET 问题的最终答案）

| 项 | 命令/位置 | 结果 |
|---|---|---|
| dotnet CLI | `where dotnet` | 不存在（正常——本项目不用 .NET SDK） |
| .NET Framework 运行时 | PowerShell `Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full'` | **Release 533509＝4.8.2**，系统级、Win11 出厂自带 |
| 编译器 | `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe` | 在位（Windows 自带） |
| 构建 | `build.cmd` | `[OK] Build succeeded: dist\一键黑屏.exe` |
| 验证（干净键态） | `verify.cmd` | `[OK] ALL VERIFIERS PASSED` 七件全绿（§3 第一次运行除外） |

**结论：无需安装任何 .NET 组件。** 项目铁律＝零依赖（README §本地开发）：运行时＝.NET Framework 4.8（Win10 1903+/11 出厂自带，**系统级已具备**），编译器＝Windows 自带 csc.exe。既不需要任何全局/系统级安装，也不存在「装进项目文件夹」的选项（无 SDK、无 NuGet、无 packages）。用户「可能需要安装 .NET」的前提不成立——本轮真正要修的是下面两个缺陷。

## 2. 根因一（产品级）：幻影键 × 布防闸门 ＝ 遮罩无法退出

**现象链**（全部本机实测，复现配方见 §8）：
1. 键态扫描 GetAsyncKeyState(1..254)：**VK_ESCAPE(0x1B) 被报为持续按下**（3 轮×1.5s 间隔，无人碰键盘）。
2. 该状态下启动遮罩：注入 SPACE down/up → 6s 不退出；再注入鼠标左键 up → 5s 仍不退出（键盘＋鼠标退出双灭）。
3. 注入合成 ESC up 清除幻影态后再启动：SPACE 注入 → 正常退出。
4. 确定性复现：注入 ESC down（不 up）→ 启动遮罩 → SPACE → **不退出**（缺陷复现成立）。

**机制**：`src/OverlayForm.cs` OverlayGroup——退出两条路（WH_KEYBOARD_LL 键盘钩子＋遮罩窗 OnMouseUp）共用闸门 `armed`；armTimer 每 100ms 轮询要求 **1..254 全部键抬起**。一个幻影键 → armed 永 false → 双路全灭。该循环属 v1 冻结基线（`baseline/BlackScreen.cs:44-46` 同款）：**语义冻结，实现存在单点脆弱性**。

**幻影键来源**：本机 ESC down 为**合成注入**（合成 up 即清除），且**早于本项目任何验证器首次注入 ESC**（时间线：MaskProbe 失败先于 VerifyOverlay 首跑）→ 源于本机其他软件（远程/自动化类工具注入 down 不补 up，真实世界确有发生——本机即实例）。仓库内 `VerifyOverlay.cs:301/322/362-363` 存在 ESC 注入但为成对 down/up（卫生审计归轨道 B）。

**冻结不变量（修复必须同时满足）**：
- **I1 防误触**：遮罩出现时仍按住的键（启动键）在**被观察到一次抬起前**不得自身触发退出（README:83「启动时按键未松开→先松开，再按键即可退出」）。
- **I2 可用性（新增，本轮修复目的）**：任何「GetAsyncKeyState 报按下但无真实按键事件」的幻影键状态，不得阻止布防完成、不得禁用键盘或鼠标任一退出路径。
- **I3 观感不变**：只移动/滚轮永不退出；真实点击任意键退出；100ms 布防节拍与计时语义不变——健康机器上可观测行为与 v1 逐点一致（由验证器套件镜像锁定，现有断言语义不得放宽）。

**首选设计方向**（供参考；执行窗口须第一性重推导并验证，不得照抄）：启动时（Show 前）快照全局按下集 L；布防条件改为「**L 以外的键**全部抬起」（幻影键/启动键 ∈ L 不阻塞布防）；L 中键维护「待抬起闩锁」——其 KEYDOWN **不触发退出**（但照常 CallNextHookEx 放行，不得吞事件——现状仅退出键被吞）直至轮询观察到其抬起。可选加固：布防期间连续 N tick（如 500ms）按住的键并入 L（覆盖「布防窗口内新出现的幻影键」，语义等价于长按启动键）。注意 VK_LBUTTON/RBUTTON 亦在 1..254 轮询域；鼠标退出为事件驱动（OnMouseUp），真实点击不受闩锁影响。

## 3. 根因二（验证器级）：IME 子窗口 × 固定计数 ＝ 非确定性断言

- verify.cmd 第 1 次运行：`[2/7] VerifySettings FAIL: expected 24 direct children, saw 25`。
- 本机子窗枚举探针：[1..24] 全部为 `WindowsForms10.*` 类（应用自有 24 控件齐全），**[25] `OimeTsfNotifyWindow` vis=False**——Windows TSF 输入法框架（微软拼音）向含文本输入控件的窗口注入的隐藏通知子窗，是否出现随 IME/系统状态**非确定**（来源佐证：Microsoft Learn/社区关于 TSF 通知窗与中文 IME 的讨论）。
- verify.cmd 第 2 次运行（干净键态）：**七件全绿**——该子窗未出现。同一台机器两次结果不同＝断言方法学缺陷实锤。
- 第 1 次运行中止于 [2/7]，[3][4] 未跑；随后单独补跑失败（"did not close overlay"）系**根因一连坐**（当时幻影 ESC 仍在）；干净态下 [3][4] 随全量套件通过。→ VerifyMouseExit/VerifyOverlay 自身无缺陷，但缺幻影键回归用例（归轨道 A）。

**修复方向（根修，非黑名单补丁）**：镜像锁本意是断言「应用自有的 WinForms 控件树」；枚举应**正向判别应用所有权**（类名前缀 `WindowsForms10.`）后断言 24，而非对裸 EnumChildWindows 计数。**禁止**按 `OimeTsfNotifyWindow` 等类名黑名单剔除（下一个环境注入别的类＝打补丁）。

## 4. 卫生审计项（轨道 B）

- **注入卫生**：所有 keybd_event/mouse_event 的 down 必须在**一切路径（含失败提前退出）**上有配对 up——验证器自身绝不能制造幻影键（毒化后续所有测试且难以归因）。
- **VerifyIdle**：计划任务在失败路径上必须清理（现通过项含 Ensure→Remove 闭环，审计失败分支）。

## 5. PC2 落位现状（迁移阶段 8 对照，依据 migrate/迁移手册.md 阶段 8）

| 阶段8 项 | 现状 | 归属 |
|---|---|---|
| 1 git fsck | 迁移时已验（docs/MIGRATE-V5D.md §4 A13） | I 波补档 |
| 2 git status | clean（会话起始快照） | I 波补档 |
| 3 verify 七件全绿 | 修复后达成（本轮 A＋B 的目的） | I 波 |
| 4 双击冒烟（真实人手） | **待用户亲手**（验证的正是真实输入路径） | 用户 |
| 5 设置窗冒烟＋保存 | 程序化可代验＋用户可复核 | I 波 |
| — DeskBox 快捷方式 | 未安装（install.ps1 未在 PC2 跑过） | I 波 |
| — 旧机清理（阶段 9） | 待用户在旧机亲手跑 cleanup-local.ps1（红线：AI 绝不代跑） | 用户 |

## 6. 并行分析与模型选型（依据）

- 共享可变资源＝git 工作树与 src/、verify/ 文件。A（`src/OverlayForm.cs`＋`verify/VerifyMouseExit.cs`＋`verify/VerifyOverlay.cs`）与 B（`verify/VerifySettings.cs`＋其余 verify 审计）**写面不相交** → 完全并行；I 依赖 A＋B。verify.cmd/build.cmd/README 本轮**全员冻结**（单一写者：无人改）。
- 三轨全部 GLM-5.3 主责：A＝冻结语义保持＋并发闩锁设计＋P/Invoke（不变量密度最高）；B＝断言语义重设计＋非确定性环境推理；I＝合并裁决＋发布工艺＋证据纪律。本轮**无视图轨**（SettingsDialog/布局零改动；若未来有视图需求，沿用 V5 裁决：视图轨＝GLM-5.3，Flash 不碰 fts 镜像锁定）。**Flash 的正确位置＝各窗口内子代理扇出**（基线/引用 grep 扫描、注入配对扫描、README 版本引用扫描）。
- 子代理与桥：优先内置 ZCode 子代理；wbx 桥若已登录可用于外部调研扇出，未登录一律内置，不阻塞。

## 7. 对齐定案（用户 2026-11-03）

1. 仓库结构轨（V1–V5 过程文档迁 docs/history/）：**先不着急**——本轮不做，根目录维持 v5 现状。
2. 推送节奏：**每轮完成默认推送** GitHub 公开仓。产品代码将变更（轨道 A）→ 需发 **v1.0.1** Release（AppInfo 单源 1.0.0→1.0.1，README 下载段同步，Release 工艺沿用 V5I：tag＋zip＋exe＋SHA256SUMS＋应用内「已是最新」冒烟）。

## 8. 复现配方（自包含，供执行窗口与校验器）

```text
键态扫描：GetAsyncKeyState(1..254) & 0x8000 逐键打印（无人操作时预期全空）。
幻影键复现：keybd_event(0x1B, down, 不 up) → 启动 dist\一键黑屏.exe → 等 3s
  → keybd_event(0x20, down+up) → 遮罩 5s 内不退出 ＝ 复现（v1.0.0 现状）
  → 收尾必须 keybd_event(0x1B, up)（清理环境，铁律）。
健康基线：干净键态启动 → SPACE 注入 → 退出成功（v1.0.0 现状即如此）。
注：探针可用 csc.exe 编译临时小工具实现（规划窗口实测源码见本节语义，不附文件）。
```

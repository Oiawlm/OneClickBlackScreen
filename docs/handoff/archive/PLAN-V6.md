# PLAN-V6 · 「迁移后体检 → 退出健壮性根修」执行编排

> **一句话定位**：本轮（v6）总编排——需求→轨道映射、并行波次与写面契约、每窗口模型选型、用户开窗操作序列；执行窗口的**事实源与设计定案在 [RESEARCH-V6.md](RESEARCH-V6.md)**，本文件只管「谁、何时、用什么模型、贴哪份提示词」。
> **来源**：用户 2026-11-03 /goal 需求＋两项对齐定案（RESEARCH-V6 §7）＋规划窗口本机实测（RESEARCH-V6 §1–§5）。
> **日期**：2026-11-03　行数预算：≤110 行。与 RESEARCH-V6 冲突时以 RESEARCH-V6 为准。

## 1. 需求 → 轨道映射

| 用户需求 | 轨道 | 产物 |
|---|---|---|
| ① .NET 组件检查（装不装/装哪） | （已完成） | 结论＝**零安装**（RESEARCH-V6 §1，实测证据齐；无执行窗口） |
| ② 遮罩无法退出（幻影键×布防闸门） | **A** | `src/OverlayForm.cs` 布防根修（快照＋闩锁，冻结不变量 I1–I3）＋VerifyMouseExit/VerifyOverlay 新增幻影键与防误触回归用例 |
| ③ 验证器环境敏感（IME 25 子窗＋注入卫生） | **B** | `verify/VerifySettings.cs` 应用所有权正向判别＋全部验证器注入卫生审计（证据驱动，只修有实据者） |
| ④ 集成、v1.0.1 发布、PC2 收尾 | **I** | main 合并 A+B→七件全绿→AppInfo 1.0.1＋README 下载段→tag/Release 三资产→已是最新冒烟→install.ps1→docs/MIGRATE-V6-PC2.md 阶段 8 证据→推送 |
| ⑤ 并行分析＋每需求一份提示词＋模型选型 | 本文件＋3 份 GOAL | GOAL-V6A/B/I-PROMPT.md |

## 2. 并行分析（第一性原理）

共享可变资源只有 git 工作树与源文件。本轮把两条修复轨的**写面切到不相交文件集**（单一写者协议）：

- **A 写面**：`src/OverlayForm.cs`、`verify/VerifyMouseExit.cs`、`verify/VerifyOverlay.cs`、分支 `feature/exit-arm-v6`、证据 `docs/VERIFY-V6A.md`。
- **B 写面**：`verify/VerifySettings.cs`＋（仅当审计有实据）`verify/VerifyConfig.cs`/`VerifyIcon.cs`/`VerifyUpdate.cs`/`VerifyIdle.cs`、分支 `feature/verify-env-v6`、证据 `docs/VERIFY-V6B.md`。
- **全员冻结**：`verify.cmd`、`build.cmd`、`build.rsp`、`README.md`、`使用说明.txt`、`migrate/**`、`baseline/**`、RESEARCH/PLAN/GOAL 文档（I 波除外其辖内）。
- A 分支上 `[2/7] VerifySettings` 预期红（B 并行未合）→ A 的完成判据＝其余六件单跑全绿＋新用例绿；B 分支上产品缺陷未修，但干净键态下六件本就绿→ B 判据＝VerifySettings 单跑连续绿＋全套件干净态绿。全量七件绿在 I 波会师——**互不阻塞，无需错峰**。

## 3. 波次表（执行序列）

| 波 | 窗口 | 输入提示词 | 模型 | 依赖 | 完成判据（校验器可核对，详见各 GOAL） |
|---|---|---|---|---|---|
| **1（并行×2）** | A | GOAL-V6A-PROMPT.md | GLM-5.3 | 无 | 分支推送；`build.cmd` 绿；VerifyConfig/Icon/Update/Idle/MouseExit/Overlay 单跑全绿且含新幻影键用例（修复前红→修复后绿的输出留档 docs/VERIFY-V6A.md） |
| | B | GOAL-V6B-PROMPT.md | GLM-5.3 | 无 | 分支推送；VerifySettings 单跑×3 连续绿（含 IME 态）；全套件干净键态绿；卫生审计表落档 docs/VERIFY-V6B.md |
| **2** | I | GOAL-V6I-PROMPT.md | GLM-5.3 | A＋B | main 合并后 `verify.cmd` 七件全绿×2；tag v1.0.1＋Release 三资产与 SHA256SUMS；设置窗「已是最新」真端点冒烟；install.ps1 两 [OK]；docs/MIGRATE-V6-PC2.md 阶段 8 五项对照（第 4 项标注待用户）；main＋tag 已推送 |
| **3（用户手动）** | — | 迁移手册阶段 8 第 4 项 | — | I | 用户亲手双击冒烟（真实输入路径）→ 四项勾齐获清理准入证 → 旧机阶段 9 亲手跑 cleanup-local.ps1（任何 AI 窗口不得代跑） |

## 4. 模型选型裁决（用户问题：GLM-5.3 vs GLM-5.3 Flash）

| 轨道 | 主模型 | 裁决理由 | Flash 的用武之地（仅限窗口内子代理扇出） |
|---|---|---|---|
| A 退出根修 | **GLM-5.3** | 冻结语义保持（I1–I3）＋布防闩锁并发设计＋P/Invoke＋C#5 零依赖约束，不变量密度全轮最高 | baseline 与 verify 的行为引用 grep 扫描 |
| B 验证器稳健化 | **GLM-5.3** | 断言语义重设计（观察者所有权原则）＋非确定性环境推理＋注入卫生判断力 | 全验证器注入配对 grep 扫描 |
| I 集成发布 | **GLM-5.3** | 合并裁决＋Release 工艺＋版本单源纪律＋证据文档单一写者 | README/使用说明 版本引用扫描 |

**结论**：三轨主责全部 GLM-5.3——本轮没有一条轨道具备「规格完全定死＋零不变量＋低精度要求」三条件（Flash 主责门槛）。**本轮无视图需求**（SettingsDialog/布局零改动；用户问及视图选型，裁决＝不适用；若未来出现视图轨，沿用 V5 先例：视图＝GLM-5.3，Flash 不碰 fts 镜像锁定与 DPI 断言）。Flash 的正确位置＝执行窗口内部的 ZCode 子代理扇出（上表右列）。执行窗口优先用内置子代理；wbx 桥未登录一律走内置，不阻塞。

## 5. 用户操作手册（怎么开执行窗口）

1. **波 1 可同时开 2 个 ZCode 会话**（A、B 互不知晓、只经仓库文件协作——写面不相交保证不打架）；每波新会话。
2. **选模型**：会话模型切 GLM-5.3（每窗口同）；权限档位「完全访问」或「自动编辑」（目标模式长任务官方建议）。
3. **开目标模式**：`/goal` 后粘贴对应提示词**文件完整内容**（如 `GOAL-V6A-PROMPT.md`），由执行窗口按其 §1 顺序冷启动自读。
4. **验收**：目标模式由独立校验器按提示词内「验收清单」自动核对（命令/输出/文件实据）；未达成自动续跑。
5. **波间衔接**：A、B 两窗口均判完成 → 开 I 窗口；I 完成（v1.0.1 已发布、阶段 8 证据落档）→ 用户亲手双击冒烟勾第 4 项 → 旧机阶段 9。
6. **红线**：migrate/ 迁移包与 SHA256SUMS.txt 不可触碰（I 波只做只读校验）；cleanup-local.ps1 永远由用户在旧机手动执行；任何窗口不得执行删除仓库类动作。

## 6. 收尾链

A∥B（波 1 并行）→ I（波 2 合并＋v1.0.1 发布＋PC2 证据）→ 用户真实双击冒烟（阶段 8 第 4 项，验证的恰是本轮修复的真实输入路径）→ 旧机阶段 9 清理。**顺序不可颠倒**：阶段 8 五项未齐前，旧机任何项目文件不删。

# PLAN-V9 · 「DeskBox 根除＋安装包分发」执行编排

> **一句话定位**：V9 轮总编排——两条需求→双轨映射、A∥B 双窗 worktree 并行波次与写面契约、模型选型、用户开窗操作序列；执行窗口的**事实源与设计定案在 [RESEARCH-V9.md](RESEARCH-V9.md)**，本文件只管「谁、何时、用什么模型、贴哪份提示词」；跨窗口协作规则见 [AGENTS.md](../../../AGENTS.md)（自动注入）。　**来源**：用户 2026-11-05 /goal 需求＋两问对齐（RESEARCH §6）＋规划窗双子代理调研。　**日期**：2026-11-05　行数预算：≤110 行。与 RESEARCH-V9 冲突时以 RESEARCH-V9 为准。

## 1. 需求 → 轨道映射

| 用户需求 | 归属 | 产物 |
|---|---|---|
| ① 清除 DeskBox（含冻结区，已授权） | **A** | install.ps1 删除＋migrate×3＋archive 11 份手术＋lessons L-017；全仓 grep 归零 |
| ② 安装包（可分发） | **B**＋**I** | setup/＋build-setup.cmd＋Updater ARP 联动＋README/使用说明 改写（B）；v1.1.0 发版＋四资产＋真机冒烟（I） |
| ③ 两层架构／最高效并行 | 本文件 §2 | 规划层并行＝规划窗双子代理（已成，RESEARCH §7）；执行层＝A∥B→R→I |
| ④ 每需求一提示词＋模型选型 | 本文件 §3/§4 | 四份 GOAL（本批次目录）；全 GLM-5.3 主责＋窗口内 Flash 子代理 |
| ⑤ 禁补丁式／最终简洁 | 各 GOAL 铁律 | A＝删除替代修补；B＝安装器接管快捷方式/卸载职责，产品自管设置与任务 |

## 2. 编排分析（第一性原理）

- **为何 A∥B 敢并行**：写面零相交（RESEARCH §3 D6 实证）——A 全在清除面（install.ps1/migrate/archive/lessons 追加），B 全在新建设施与活文档（setup/build-setup/README/使用说明/Updater.cs）；且 A 只跑纯件验证不触闸门，B 独占闸门跑 GUI 四件，无争用。
- **为何 B 不再拆**（安装器∥文档两条子轨）：文档准确性依赖安装器实际行为（目录/快捷方式/卸载语义已由 RESEARCH D2 定死，但实现偏差仍需同窗对齐）；再拆即制造写面协调与合并裁决——打补丁式拆分（V8 §2 同款裁决）。
- **A 快 B 慢不等价串行**：A 完成即推送；B 独立推进至推送；R 只等两者都齐——先完成者不阻塞后完成者任何工作。
- **R 预集成照 V8 惯例**：R 分支自 main 依次 merge A、merge B，冲突提前暴露裁定；I 只 `--no-ff` 合并 R 分支，专注发版链。
- **本轮有产品代码变更**（Updater.cs）→ 按 AGENTS §5 红线必须走完整发版链（版本单源 1.1.0→tag→Release→应用内冒烟），I 轨承担。
- **V8 批次归档（D8）**：规划窗本轮已执行 `git mv docs/handoff/2026-11-05-v8-structure docs/handoff/archive/`＋活引用改全路径，随本批规划文档一并提交。

## 3. 波次表（波1 双窗并行；分支推送后才开下一波）

| 波 | 窗口 | 输入提示词（本目录） | 模型 | 依赖 | 完成判据（校验器可核对，详见各 GOAL） |
|---|---|---|---|---|---|
| 1a | A DeskBox 根除 | GOAL-V9A-PROMPT.md | GLM-5.3 | 无 | 分支推送；diff ⊆ 白名单；全仓 `grep -ri deskbox`（排除 .git）零命中；build 绿＋纯件三件 exit 0；编码 BOM/CRLF 复验不变；lessons 锚点交集∅实证 |
| 1b | B 安装包 | GOAL-V9B-PROMPT.md | GLM-5.3 | 无¹ | 分支推送；diff ⊆ 白名单；Setup 产物 ASCII 名+版本取自 AppInfo 单源；七件套全绿；隔离自测（测试 AppId＋TEMP 目录）全断言＋零残留实录 |
| 2 | R 复检预集成 | GOAL-V9R-PROMPT.md | GLM-5.3 | A+B 均推送 | feature/review-v9 两次 `--no-ff` 合并零冲突（或裁定记录）；.iss 对照 D2 参数逐项核；A 档案手术抽审；七件全绿；REVIEW-V9.md 留档推送 |
| 3 | I 集成发版 | GOAL-V9I-PROMPT.md | GLM-5.3 | R 推送 | main 合并 review-v9；AppInfo 1.1.0 唯一命中；Setup-1.1.0 构建；verify×2 全绿；零足迹三态；tag v1.1.0＋Release 四资产；真机安装冒烟＋应用内冒烟；worktree/分支清理；VERIFY-V9.md 留档 |

¹ B 窗前置＝用户已装 Inno Setup 6（§5 步骤 1）；ISCC 探测失败即停下报告，不自行安装软件。

## 4. 模型选型裁决

| 轨道 | 主模型 | 裁决理由 | Flash 用武之地（仅限窗口内子代理） |
|---|---|---|---|
| A DeskBox 根除 | **GLM-5.3** | 冻结区手术精度（16 文件逐处最小删改）＋编码保真判断 | 全仓 grep 复扫、lessons 锚点文件清单核对 |
| B 安装包 | **GLM-5.3** | 新构建链设计（.iss/build-setup/ARP 联动）＋文档门面改写判断力 | 死链扫描、资产名 ASCII 校验扫描 |
| R 复检预集成 | **GLM-5.3** | 证据审计＋合并裁决＋.iss 参数逐项核对 | diff 规模统计、链接存在性复扫 |
| I 集成发版 | **GLM-5.3** | 终验纪律＋发版链完整性＋清理不残留 | 版本引用扫描、资产清单核对 |

本轮无视图/UI 内容轨（README/使用说明＝文本改写，无新视觉件），故无 Flash 主责场景（V8 §4 同款结论）。

## 5. 用户操作手册（怎么开执行窗口）

1. **先装 Inno Setup 6（一次性）**：官网 jrsoftware.org 或 GitHub jrsoftware/issrc releases 下载安装（默认路径即可）；B 轨 build-setup.cmd 探测 PATH 与 `Program Files (x86)\Inno Setup 6`。
2. **波 1 双窗并行**：开**两个全新 ZCode 会话**（模型均 GLM-5.3、权限档「完全访问」），分别 `/goal` 粘贴 `docs/handoff/2026-11-05-v9-installer/GOAL-V9A-PROMPT.md` 与 `GOAL-V9B-PROMPT.md` **完整内容**。两窗各自按其 §1 在 `OneClickBlackScreen-WorkTree\` 建本轨 worktree（命令在 GOAL 内）。**B 窗会跑 GUI 验证器注入全局键鼠——其间请勿操作键盘鼠标**；A 窗不触闸门。请勿在任一窗口工作期间手动改动仓库文件。
3. **波 2/波 3 串行**：A、B 分支都推送后（`git ls-remote --heads origin` 可见）开 R 窗（GOAL-V9R）；R 推送后开 I 窗（GOAL-V9I）。I 的全量验证＋真机冒烟时段同样勿动键鼠。
4. **验收**：目标模式由独立校验器按各 GOAL「验收清单」自动核对；未达成自动续跑。
5. **红线**：AGENTS.md §5 全文适用——不删仓库、不改写远端历史、`migrate\cleanup-local.ps1` 永远由用户在旧机亲手执行（A 轨只**编辑**该文件的 DeskBox 节，不运行它）、凭证不入文件。

## 6. 收尾链

A（清除）∥ B（安装包）→ R（预集成＋复检）→ I（合并＋发版 v1.1.0＋终验＋清理）→ 完成。本轮**有发版**（产品代码变更：Updater.cs）；若 R 发现缺陷→移交注记由 I 闭环（或退回对应轨重开）。V9 批次目录由 V10 规划窗归档（沿 D8 惯例）。

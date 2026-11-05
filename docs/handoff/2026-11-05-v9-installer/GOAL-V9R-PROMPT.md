# GOAL-V9R · 复检与预集成（R 轨）

> **一句话定位**：R 轨执行窗（GLM-5.3·目标模式）：复检 A/B 两轨全部产出（档案手术逐处抽审＋.iss 参数对照定案逐项核＋白名单核对），在 `feature/review-v9` 分支完成两次 `--no-ff` 预集成合并与合并态全量验证，留档 `REVIEW-V9.md` 并推送。　**事实源＝[RESEARCH-V9.md](RESEARCH-V9.md)，冲突以它为准；两轨实据＝其 VERIFY-V9A/V9B.md。**　日期：2026-11-05　行数预算：≤110 行。

## 1. 身份与冷启动

你是 R 轨执行窗，产出复检证据与预集成分支，不产出讨论。按序自读（主检出 `D:\Download\Agent Vault\OneClickBlackScreen\OneClickBlackScreen-main\`）：1. `AGENTS.md`；2. `docs/handoff/2026-11-05-v9-installer/RESEARCH-V9.md` 全文＋`PLAN-V9.md` §3；3. `VERIFY-V9A.md`、`VERIFY-V9B.md` 全文；4. `GOAL-V9A-PROMPT.md` §4/§6、`GOAL-V9B-PROMPT.md` §4/§6（验收口径）；5. 本文件。**禁读**：archive 全文（抽审目标文件按 diff 定位）、`baseline/**`。

前置检查：`git ls-remote --heads origin` 确认 `feature/deskbox-purge-v9` 与 `feature/installer-v9` 均在（缺任一＝停下报告，等波 1 齐再动）。然后建 worktree：

```bash
cd "D:/Download/Agent Vault/OneClickBlackScreen/OneClickBlackScreen-main"
git fetch origin
git ls-remote --heads origin feature/review-v9   # 必须输出为空；非空＝撞名，停下报告
git worktree add "../OneClickBlackScreen-WorkTree/v9-r" -b feature/review-v9 origin/main
cd "../OneClickBlackScreen-WorkTree/v9-r"         # 本窗全部工作在此，主检出只读
```

## 2. 背景与职责边界

A/B 已并行完成（写面零相交，RESEARCH D6）；本轨职责＝合并前把住质量：A 动了冻结区（用户授权范围＝仅 DeskBox 内容），B 动了产品码（Updater.cs）与分发面——两者都是高敏感改动，逐项实证后才允许 I 轨合入发版。发现缺陷：小缺陷记移交注记由 I 闭环；结构性缺陷（如越出授权范围/白名单外 diff/验证假绿）＝总评 NO-GO 并退回对应轨。

## 3. Phase 任务（依序）

**P1 A 轨复检**：①拉取 A 分支逐文件审 `git diff origin/main...origin/feature/deskbox-purge-v9 -- <各手术文件>`——每处改动是否仅涉 DeskBox、句法自洽、无顺手改；②`grep -ri deskbox`（排除 .git）在该分支工作树零命中自证；③lessons L-017 五要素与锚点交集∅复验；④migrate 手术后节编号/上下文自洽（打开 cleanup-local.ps1 手术段上下文读）；⑤编码 `file` 抽验全部手术文件。
**P2 B 轨复检**：①`setup/OneClickBlackScreen.iss` 对照 RESEARCH §3 D2 **逐参数勾验**（PrivilegesRequired/DefaultDirName/AppId 固定/四件内容/快捷方式三处/opt-in 桌面/卸载清任务且留设置/TestId 变体入口）；②`build-setup.cmd` 版本取自 AppInfo.cs 单源实证（改 AppInfo 试验性跑一次？**禁改 src/AppInfo.cs**——改为审脚本提取逻辑＋实际产物名含 1.0.2）；③Updater ARP 联动代码路径审（键存在才写/no-op 语义/无补丁式旁路）＋VerifyUpdate 绿；④README/使用说明 安装节改写质量与零 DeskBox/install.ps1 字样；⑤隔离自测实录完整性与三态快照。
**P3 白名单核对**：两分支各自 `git diff --name-status origin/main...<分支>` 逐路径对照其 GOAL §4 白名单——白名单外任何路径＝NO-GO。
**P4 预集成**：在 review-v9 分支 `git merge --no-ff origin/feature/deskbox-purge-v9` → `git merge --no-ff origin/feature/installer-v9`；冲突＝逐个裁定并在 REVIEW 记录（写面零相交预期零冲突，出冲突即说明申报失实——重点追查）。
**P5 合并态验证**：`build.cmd` 绿；`build-setup.cmd` 绿（产物重建）；`verify.cmd` 七件全绿（闸门独占时段，撞 BUSY 等待）；死链复扫（全仓 markdown 相对链接，Flash 子代理扇出）；合并态 `grep -ri deskbox` 归零复证。
**P6 留档推送**：`docs/handoff/2026-11-05-v9-installer/REVIEW-V9.md`（六字段模板照 docs/handoff/README.md：背景/现状产出/踩坑引 L 编号/决策与否决/下一步/快速启动命令；含 P1-P5 全部实据与总评 **GO/NO-GO**）；提交；`git push -u origin feature/review-v9`。

## 4. 铁律

写面白名单：`docs/handoff/2026-11-05-v9-installer/REVIEW-V9.md`(新)＋review-v9 分支上的合并提交。**禁改**任何产品/文档源文件（发现问题记移交注记，不代修）；`src/AppInfo.cs`、verify/**、AGENTS.md 全冻结；主检出只读；证据＝真实命令输出；撞闸守 AGENTS §2；不删仓库、凭证不入文件。

## 5. 验收清单（校验器逐条核对实据）

- [ ] `git ls-remote --heads origin feature/review-v9` 可见；合并态＝main＋A＋B（两条 merge 提交，`--no-ff`）
- [ ] REVIEW-V9.md 含：A 逐文件手术审语＋B 参数勾验表（D2 逐项 ✅）＋双白名单核对结论＋合并零冲突（或裁定记录）
- [ ] 合并态：build 绿＋Setup 构建绿＋七件全绿＋死链零＋`grep -ri deskbox` 归零（输出留档）
- [ ] 总评 GO/NO-GO 明确；NO-GO 附退回轨道与理由
- [ ] 无任何源文件代修记录（铁律 5）

## 6. 收尾动作

REVIEW-V9.md 末尾「移交注记」逐条给出处置建议（闭环/备案/退回）；推送后报告分支名、提交哈希、总评、验收清单逐条 ✅/❌。I 轨以你的分支为合入对象。

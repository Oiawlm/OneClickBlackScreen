# GOAL-V10R · 复检预集成＋契约核对＋行为文档（R 轨）

> **一句话定位**：R 轨执行窗（GLM-5.3·目标模式）：合并 A/B 两轨为 feature/review-v10 预集成、RESEARCH §7 跨轨契约表逐项双侧同构核对、合并树八件全绿＋隔离自测复跑、README/使用说明 行为文档改写，REVIEW-V10.md 留档推送。　**事实源＝[RESEARCH-V10.md](RESEARCH-V10.md)（§3 定案/§7 契约表），冲突以它为准。**　日期：2026-11-11　行数预算：≤120 行。

## 1. 身份与冷启动

你是 R 轨复检窗，产出复检证据与预集成分支，不产出讨论。按序自读（全部在主检出 `D:\Download\Agent Vault\OneClickBlackScreen\OneClickBlackScreen-main\`）：1. `AGENTS.md`；2. `docs/handoff/2026-11-11-v10-lifecycle/RESEARCH-V10.md` 全文；3. `PLAN-V10.md`；4. `VERIFY-V10A.md`＋`VERIFY-V10B.md`（两轨证据，含各自移交注记）；5. 本文件。**禁读**：archive 全文（grep 定位例外）、`baseline/**`、`migrate/**`。
**前置检查**：`git ls-remote --heads origin feature/app-v10 feature/installer-v10` 两分支均可见——缺任一＝停下报告（不得替跑缺席轨）。
建 worktree（主检出根执行；命令照抄，git bash）：

```bash
cd "D:/Download/Agent Vault/OneClickBlackScreen/OneClickBlackScreen-main"
git fetch origin
git ls-remote --heads origin feature/review-v10   # 必须输出为空；非空＝撞名，停下报告
git worktree add "../OneClickBlackScreen-WorkTree/v10-r" -b feature/review-v10 origin/main
cd "../OneClickBlackScreen-WorkTree/v10-r"        # 本窗全部工作在此，主检出只读
git merge --no-ff origin/feature/app-v10 -m "merge: V10 A 轨应用侧合入 review"
git merge --no-ff origin/feature/installer-v10 -m "merge: V10 B 轨安装器侧合入 review"
```

任一 merge 冲突＝停下逐项裁定留档（RESEARCH 为准），不得强推解决；两轨写面零相交是设计前提，冲突本身即偏差信号。

## 2. 背景（为什么）

A（应用侧 src/verify）与 B（安装器侧 setup）并行实现同一套契约（RESEARCH §7）——并行的前提是接口冻结，你的核心职责＝**证明两侧实现逐项同构**（键名/形状/互斥/静默参数/资产名/暂存路径），任何漂移在此波修复或退回；行为文档（README/使用说明）描述的是合并后的定型行为，故从 V9 的执行轨移到本轨；I 轨只做发版与版本引用联动。

## 3. Phase 任务（依序）

**P1 契约逐项核对**：RESEARCH §7 表 11 行逐行双侧取证——A 侧（src/AppPaths.cs、src/Updater.cs 等）与 B 侧（setup/OneClickBlackScreen.iss）各自的真实代码行 grep 留档，值不一致＝缺陷（修复以 RESEARCH 为准，改动落入对应文件并在 REVIEW 记裁决；跨轨同文件修复需在白名单内）。
**P2 A 轨产物复检**：白名单核对（`git diff --name-status origin/main...origin/feature/app-v10` ⊆ GOAL-V10A §4）；VerifyPaths/VerifyUpdate 扩展的断言面抽查（双资产 fail-closed 语义、形态判定、命令行构造）；SettingsPath 单点化 grep 复扫；U2 放宽边界审查（遮罩仅一次节流 GET、失败全静默）。
**P3 B 轨产物复检**：白名单核对；.iss 新增段对照 RESEARCH D2/D4-D6 逐项（AppMutex 值/[Run] 无 skipifsilent＋测试变体抑制/数据页校验/归属守卫/卸载删数据＋守卫/ConfirmUninstall 文案）；编码 `file` 复验（UTF-8 BOM＋CRLF）。
**P4 合并树全量验证**：`verify.cmd` 八件全绿（GUI 件撞 `VERIFY BUSY`＝等对方结束再跑）；B 轨隔离自测在合并树上复跑一轮（四阶段全断言）；`build-setup.cmd` 绿。
**P5 行为文档改写**（四处，先以 Skill 工具加载 `github-readme`）：①`README.md` 下载与安装节——补「安装时可自定义安装目录与数据位置」「卸载将完整删除程序与设置数据（零残留）」；②`README.md` 边界与限制节——隐私口径改「打开设置窗即查更新；启动遮罩时隔天静默检查一次（仅版本号查询，失败无感）」；③`使用说明.txt`【自动更新】节——改「预下载＋一键秒装」流程＋遮罩提示行说明＋「更新为完整安装包覆盖（安装版）/单文件替换（便携版）」两形态如实表述（替换 :67 旧承诺）；④`使用说明.txt` 卸载节——改「完整卸载将同时删除个人设置数据」。**版本号引用一律不动**（保持 1.1.0，I 轨联动）；`使用说明.txt` 保持 UTF-8 BOM＋CRLF（L-006）。
**P6 留档推送**：`docs/handoff/2026-11-11-v10-lifecycle/REVIEW-V10.md`（六字段：背景/现状产出/踩坑引 lessons 相关条目/决策与否决/下一步/快速启动命令；含 P1 契约核对表全量留档、P2/P3 抽查记录、P4 输出、P5 diff 摘要、移交注记）；提交 `review(R): V10 …`；`git push -u origin feature/review-v10`。

## 4. 铁律

写面白名单（对 main 的 diff 必须全部落入）：`README.md`、`使用说明.txt`、`docs/handoff/2026-11-11-v10-lifecycle/REVIEW-V10.md`(新)，以及 P1 裁决裁定的契约修复落点（仅限 `src/AppPaths.cs`/`src/Updater.cs`/`setup/OneClickBlackScreen.iss` 内的契约值对齐，每处附裁决记录）。白名单外一律只读。**禁改**：`src/AppInfo.cs`、`verify.cmd`（A 轨已定八件形态）、`build-setup.cmd`、AGENTS.md、archive/**、baseline/**、migrate/**。发现 A/B 轨缺陷：小（契约值对齐/文案）＝本轨修复留档；大（结构性）＝移交注记提请退回重开，不越权重写。证据＝真实命令输出；主检出只读；GUI 件闸门纪律（AGENTS §2）；不删仓库、凭证不入文件。

## 5. 预期触碰文件（冲突面申报，供 I 预判）

README.md/使用说明.txt 仅本轨改行为描述（I 轨随后只做版本引用联动）；契约修复落点若涉及 src/setup，diff 必须逐处附裁决。

## 6. 验收清单（校验器逐条核对实据）

- [ ] `git ls-remote --heads origin feature/review-v10` 可见且含 A+B 两次 `--no-ff` 合并
- [ ] REVIEW-V10.md 含 RESEARCH §7 契约表 11 行逐行双侧取证（grep 行号级留档），零未解释偏差
- [ ] 合并树 `verify.cmd` 八件全绿＋`build-setup.cmd` exit 0（真实输出贴档）
- [ ] B 隔离自测复跑四阶段全断言实录
- [ ] README/使用说明 四处改写落地且 `grep -n "1\.1\.0" README.md 使用说明.txt` 命中数不低于改前（版本引用未误删）；使用说明.txt `file` 复验 BOM＋CRLF
- [ ] `git diff --name-status origin/main...HEAD` ⊆ §4 白名单

## 7. 收尾动作

REVIEW-V10.md 移交注记分「I 轨闭环项」与「提请 V11 项」两栏（沿 V9 惯例）；推送后报告：分支名、提交哈希、验收清单逐条 ✅/❌、契约核对发现的偏差与处置汇总。I 轨将合并你的分支并发版 v1.2.0。

# RESEARCH-V8 · 「项目结构与合作协议根修」唯一事实源

> **一句话定位**：v8 轮唯一事实源：五项需求现状体检（息屏已满足/设置判 GO/根目录 45 份轮次文档无序/无 worktree 落地/无路由与坑库）、目标结构定案（fts 壳形+三层渐进披露+handoff 批次制+lessons 坑库，两张路由表定稿文本在 §3）、S∥L 首次并行编排依据与写面契约。执行窗口以本文为唯一事实源；与 GOAL 冲突时以本文为准。
> **来源**：用户 2026-11-05 /goal 需求＋三项对齐定案（§7）＋规划窗口三路子代理实测（fts 项目组织逐文件调研、设置界面×fts 风格系统逐维对照、agents.md 官方/Anthropic/GitHub spec-kit/claude-handoff/agents-remember/社区 worktree 实践外部佐证）。
> **日期**：2026-11-05　行数预算：≤170 行。

## 1. 需求→现状体检总表

| # | 用户需求 | 现状（实据） | 差距 | 归属 |
|---|---|---|---|---|
| 1 | fts 式文件夹组织，勿在项目外散落文件 | 仓库根＝`Agent Vault\OneClickBlackScreen\`（git 检出），无壳层、无 worktree 目录；并行轮从未落地（AGENTS §2 只有条款） | 壳形未建 | S 轨＋规划窗基座（§2） |
| 2 | 参考官方文档管理项目结构/命名/位置 | 根目录堆 45 份轮次文档（RESEARCH×7＋PLAN×3＋GOAL×16）＋docs/ 19 份 VERIFY；无目录语义文档 | 无路由、无归档、无命名规范 | S＋L 轨 |
| 3 | 设置界面整理（参考本地 UI / fts skill） | 子代理逐维对照 fts 风格系统：语义 token 单源（ThemePalette 11 键双主题）、间距 4px 阶梯/字号两档、恒定几何（更新按钮固定槽 `SettingsDialog.cs:344-352`）、反例库 20 条零命中——**判 GO**；仅圆角/动效缺失，属 WinForms 技术栈不可得或体系允许的退化（fts 明文允许「立即定格终值」），投入产出比为负 | 无需返工 | 不设轨（§4） |
| 4 | 息屏显示（空闲后显示屏保/默认文字/自定义文字） | **V5C 已实现**：`IdleWake.cs` GetLastInputInfo＋每分钟计划任务、档位 0/5/10/15/30（`MaskConfig.cs:23`）、自定义文字 200 字；用户对齐定案「现状已满足」 | 无 | 不设轨（零产品变更、不发版） |
| 5 | README／渐进式披露／上下文交接／worktree 并行 | README 门面 v1.0.2 刚翻新（链接指向根目录旧位置）；冷启动路由单层（AGENTS §1 直指根目录 V 号文档）；踩坑记录散在 19 份 VERIFY 的移交/事故注记里无汇总 | 三层路由＋批次制＋坑库全缺 | S＋L＋I 轨 |
| 6 | 最高效率并行开发＋每需求一提示词＋模型选型 | V7 全串行（当时用户裁决减少失误优先）；本轮用户明确要求并行 | 首次双轨 worktree 并行 | PLAN §2/§3 |

## 2. 目标结构定案（用户定案形式；执行窗口不得偏离）

### 2.1 壳形（用户 2026-11-05 原文给出：「OneClickBlackScreen/（总目录）→ OneClickBlackScreen-main/（主线）＋ OneClickBlackScreen-WorkTree/（工作树）」）

```text
Agent Vault\OneClickBlackScreen\            ← 壳（总目录）：只允许下面两项，禁放任何散物
├── OneClickBlackScreen-main\               ← 主检出（现仓库根全部内容移入；常驻 main，集成窗独占写；
│                                              例外写动作＝规划窗提交轮次文档）——规划窗本轮已搬家完成
└── OneClickBlackScreen-WorkTree\           ← worktree 父目录：一轨一分支一子目录（slug=v8-<轨>），
                                               由各执行窗自建自清（规程＝docs/multi-window.md）
```

- 佐证：fts `multi-window.md:19` 禁 worktree 入仓库内（git clean -fdx 连带清空＋干扰工具扫描）；业界主流＝兄弟目录 `<仓库名>-<分支>`（Kempé 2026-03、r/ClaudeAI 多帖）；「一个 worktree＝一个分支＝一个代理」铁律。
- 搬家＝纯磁盘 mv（git 自包含，不产生提交、远端无感知）；由规划窗在无其他窗口持旧路径会话时完成（§5 风险 R1）。

### 2.2 仓库内目标结构（OneClickBlackScreen-main 内）

```text
AGENTS.md                 ≤60 行三层路由改写＋冻结区路径更新＋worktree 一行引用（S 轨）
README.md                开发过程文档表改指新结构（S 轨）；其余门面不动（无产品变更）
使用说明.txt / src/ verify/ tools/ assets/ baseline/ migrate/ dist/ / 构建脚本  全部不动
docs/
├── README.md            任务路由枢纽＝渐进式披露第 2 层（L 轨，新）
├── lessons.md           坑库：现象→根因→正解→出处→防再犯锚点（L 轨，新）
├── multi-window.md      worktree 创建/推送/清理规程（S 轨，新）
└── handoff/
    ├── README.md        批次制规范＋交接模板（L 轨，新）
    ├── 2026-11-05-v8-structure/   本轮批次：本文件＋PLAN-V8＋GOAL-V8*＋执行窗产出的 VERIFY-V8*
    └── archive/         V1~V7 全部 45 份历史轮次文档平铺原名（S 轨 git mv，内容冻结不变）
```

- 归档清单（S 轨逐份核对，`git mv` 保留历史）：根目录 `RESEARCH-V1..V7.md`×7、`PLAN-V5/V6/V7.md`×3、`GOAL-V1..V7V-PROMPT.md`×16、`docs/{VERIFY-*,REVIEW-V7,COPY-*,MIGRATE-*,PUBLISH-*}.md`×19，合计 45 份 → `docs/handoff/archive/`。
- 佐证：GitHub spec-kit `.specify/specs/<feature>/` 三段式惯例；philschmid「任务专属文档放独立目录不撑大根文件」；Anthropic「只往里加内容不验证效果」为 AGENTS.md 第一大误用——本轮三层化后根 AGENTS.md 反而更短。
- 交接模板 8 字段裁剪为 6（claude-handoff 实证「What We Tried 失败尝试」最防重复踩坑，保留）：背景／现状与产出／踩坑（引 lessons 编号）／决策与否决／下一步／快速启动命令。

## 3. 两张路由表定稿文本（防 S/L 并行漂移：两轨照抄本节，R 复检逐字核对）

### 3.1 AGENTS.md §1 冷启动路由（S 轨照抄替换，其余条款仅 §2 增一行、§4 改路径）

```text
1. 冷启动路由
- 三层渐进披露：先读 AGENTS.md（本文件，常驻）→ docs/README.md（任务路由枢纽）→
  按路由表读当前批次 docs/handoff/<最新批次>/（RESEARCH→PLAN→本窗 GOAL）；冲突时以 RESEARCH 为准。
- 禁读 docs/handoff/archive/ 历史批次全文（过程记录，防上下文膨胀）；要惯例只 grep 定位。
```

§2 追加一行：`并行轮一轨一 worktree（壳内 OneClickBlackScreen-WorkTree\，禁入仓库内），规程＝docs/multi-window.md。`
§4 冻结区第二行改为：`docs/handoff/archive/**（V1~V7 历史轮次文档，归档只读）`；其余不变。

### 3.2 docs/README.md 路由表主体（L 轨照抄为文件骨架，再按 §4.2 扩充）

| 任务 | 先读 | 再读 | 别读 |
|---|---|---|---|
| 任意新窗口冷启动 | AGENTS.md | 本表选路 | archive 全文 |
| 执行轮次任务 | 当前批次 GOAL | 批次内 RESEARCH/PLAN | 历史批次全文 |
| 查踩坑／勿改区／「这个能不能动」 | docs/lessons.md | 条目出处锚点 | — |
| 多窗口并行／worktree | docs/multi-window.md | AGENTS §2 | — |
| 发版（I 轨） | 批次 GOAL-V?I | archive 内上一份 VERIFY-V?I | 其余历史 |
| 跑验证 | verify.cmd 头注释 | AGENTS §2 独占资源 | — |

双镜像规则（fts 实证）：AGENTS §1 与 docs/README.md 互为镜像，改任一必须同步另一（写入两文件头注释；R 复检核对）。

## 4. 轨道设计

### 4.1 S 轨（结构归档＋协议演进；分支 feature/structure-v8，worktree v8-s）
写面：根目录 45 份 git mv、`AGENTS.md`、`README.md`（仅开发过程文档表＋该表内链接）、`docs/multi-window.md`(新)、`.gitignore`(仅核查：若未来 .zcode/ 落仓库根需忽略则补一行，无则不动)。
1. `git mv` §2.2 归档清单 → `docs/handoff/archive/`（先建目录；逐份核对 45/45，`git status` 全为 renamed）。
2. AGENTS.md 按 §3.1 照抄改写（行数预算仍 ≤60，超了先删冗余）；README.md 开发过程文档表改指 `docs/handoff/archive/`、`docs/lessons.md`、`docs/multi-window.md`，正文其余零改动。
3. 新建 `docs/multi-window.md`（≤80 行）：worktree 创建模板（`git fetch`→`git ls-remote --heads origin <分支>` 查重、撞名即停→`git worktree add "..\..\OneClickBlackScreen-WorkTree\v8-<轨>" -b <分支> origin/main`→`push -u`）、清理模板（`git worktree remove`＋`prune`＋`branch -d`，合并即清理）、壳内禁散物、主检出写权归属（集成窗＋规划窗例外）。
4. 验证：`build.cmd` 绿；纯件 VerifyConfig/VerifyIcon/VerifyUpdate 三件跑绿（无全局输入，可与 L 并行）；全仓死链扫描（README/AGENTS/multi-window 内相对链接逐条存在性核对，机械扫描用 Flash 子代理）；`git log --follow` 抽查 3 份归档文件历史保留。

### 4.2 L 轨（路由枢纽＋坑库＋交接规范；分支 feature/docs-router-v8，worktree v8-l）
写面：`docs/README.md`(新)、`docs/lessons.md`(新)、`docs/handoff/README.md`(新)。只读 archive 待归档位（引用一律写归档后路径 `docs/handoff/archive/...`）。
1. `docs/README.md`：§3.2 路由表照抄＋每行一两句展开（何时读/别读理由）＋与 AGENTS §1 双镜像声明。≤90 行。
2. `docs/lessons.md`：坑库。条目格式 `L-00x｜现象｜根因｜正解｜出处(file:line/文档节)｜防再犯锚点`。初始 ≥12 条，种子（必须收录，出处 L 轨自查补全 file:line）：①V6B 共享单工作树分支劫持；②V7V VerifyOverlay 真实 ini 备份-覆写-恢复危险机制（根修＝删机制非装甲）；③USERPROFILE 运行时改无效（生效面＝进程创建时环境块）；④V7V 验证并行全局输入互扰（根治＝闸门 BUSY/exit2）；⑤V7I digest 脚本缺陷两处；⑥Windows 编码三变体；⑦V7I LatestSmoke 首版撞按钮禁用窗；⑧并发立即更新失败（v1.0.2 修，Global\ 互斥）；⑨⑩⑪REVIEW-V7 R-1/R-2/R-3 移交注记；⑫零足迹原则（用户状态只读）。其余从 archive 各 VERIFY 的事故注记提炼，宁缺毋滥——每条必须真实出处可打开。≤180 行。
3. `docs/handoff/README.md`：批次制规范（`<yyyy-mm-dd>-<slug>/`＝启动日＋kebab；批次内沿用 `VERIFY-V<n><轨>.md` 既有命名；完结批次整目录 git mv 入 archive/；三桶政策本项目取「保留/归档」两桶，不删除）＋六字段交接模板（§2.2）。
4. 验证：路由演练（模拟新窗口冷启动按 docs/README.md 表走到当前批次 GOAL；本轨 worktree 基于 main 建、S 尚未归档，凡指向归档件的路径按「归档后路径书写＋映射回本 worktree 现存位置核验内容」口径执行，R 在合并态复核真实路径逐条可打开）；lessons 条目出处抽查 ≥1/3 且内容相符（Flash 子代理按同一口径核对）；行数预算达标。

### 4.3 不设轨结论的依据（如实告知用户）
- 设置界面 GO 证据：ThemePalette 语义 token 单源＋ApplyPalette 原位重刷（`SettingsDialog.cs:388-442`）＝fts 动态资源换装的 WinForms 等价物；`SettingsDialogMetrics` 单源＋VerifySettings check 6 镜像＝锁定测试等价；反例 20 条零命中。圆角缺失＝Win11 DWM 已给窗口壳系统圆角、控件级手工绘制恰是 fts 反例 A6/A13 警告方向。
- 息屏已满足：README:23-27 功能清单即用户所述行为；用户定案不动。

## 5. 并行编排依据与风险表

- **S∥L 写面不相交**（§4 白名单无共同文件，L 三件全为新文件）；S 跑纯件验证（VerifyConfig/Icon/Update 无全局输入，AGENTS §2）不触闸门，L 不跑验证——无 V7 场景B 三冲突；各自 worktree 的 `dist\`/`verify\bin\` 物理隔离。R/I 时段独占跑全套。**本轮即 V7 根治成果（闸门+零足迹+协议）的首次实战实证**。
- 镜像漂移风险（AGENTS §1 归 S、docs/README.md 归 L）→ §3 两表定稿文本照抄＋R 逐字核对。
- | 风险 | 缓解 |
  |---|---|
  | R1 搬家时旧路径会话失效 | 规划窗已在本轮提交后、用户开执行窗前完成搬家；用户开新窗一律从 `OneClickBlackScreen-main\` 打开 |
  | R2 归档移动破坏外部引用 | 全仓死链扫描（S 轨）＋R 复扫；.zcode 可视化产物在仓库外不受影响 |
  | R3 git mv 后 git log 断史 | `--follow` 抽查（S 轨验收项）；mv 单次原子提交便于追溯 |
  | R4 并行下 R 预合并遇冲突 | 写面不相交论证在前；真冲突 R 记录并裁定，I 参照执行 |

## 6. 编排与模型选型（依据）

- 波次：波1＝S∥L（两窗同时开，各自 worktree）→ 波2＝R 复检（分支 feature/review-v8＝自 main merge S＋merge L 预演集成，复检合并态并写 REVIEW-V8.md）→ 波3＝I 集成（main 上 `--no-ff` 合并 feature/review-v8（内含 S+L），全套 verify ×2＋零足迹审计＋worktree/分支清理＋推送）。I 不重复合并 S/L，直接采信 R 预演结果（V7 模式演化：R 从「只读复检」升级为「预集成＋复检」，I 专注终验与收尾）。
- 模型：四窗主责全 GLM-5.3（S＝git 批量操作精度＋协议演进判断；L＝45 份历史文档提炼与路由设计的判断力；R＝证据审计＋合并裁决；I＝发布纪律）。Flash 仅窗口内子代理机械扫描（死链 grep、归档清单核对、lessons 出处核对）。本轮无视图/UI 变更轨，无 Flash 主责场景。

## 7. 对齐定案（用户 2026-11-05）

1. 息屏功能：现状已满足（V5C 实现在案），本轮零产品变更、不发版、AppInfo 不动。
2. worktree：完整 fts 壳形（§2.1 用户原文形式），由规划窗搬家落地。
3. 历史文档处置：用户授权规划窗自行判断 → 定案全量归档（§2.2，git mv 保留历史，内容冻结不变）。

## 8. 轨道写面契约（单一写者；GOAL 引用为准）

| 轨 | 分支（自） | 写面白名单 | 证据文档 |
|---|---|---|---|
| S 结构归档 | feature/structure-v8（main） | §4.1 所列 | docs/handoff/2026-11-05-v8-structure/VERIFY-V8S.md |
| L 路由坑库 | feature/docs-router-v8（main） | §4.2 所列 | 同目录 VERIFY-V8L.md |
| R 复检预集成 | feature/review-v8（main→merge S→merge L） | 同目录 REVIEW-V8.md | 同左 |
| I 集成 | main（--no-ff 合并 feature/review-v8） | 同目录 VERIFY-V8.md＋worktree/分支清理 | 同左 |

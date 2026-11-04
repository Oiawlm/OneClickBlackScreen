# GOAL-V5D · 迁移包与本地清理（轨道 D，最后一步）

> **一句话定位**：把项目完整迁到第二台电脑的一切准备好——迁移就绪硬化（install.ps1 去本机硬编码）、离线备援包、菜鸟级图文迁移手册（每一步：在哪打开 PowerShell＋可复制粘贴命令＋预期输出＋排错表）、本地清理脚本（生成但不执行，PC2 验证通过后由用户手动跑）。
> **上游事实源**：[RESEARCH-V5.md](RESEARCH-V5.md) §1 U4/U6、§3 R4、§5 D 系（D1–D6）、§4 常量登记表　**编排**：[PLAN-V5.md](PLAN-V5.md) §3 波 3（前置：I 轨道已发布 v1.0.0）。
> **日期**：2026-11-02　行数预算：≤190 行。

## 0. 身份与工作方式

- 你是执行窗口，主模型 GLM-5.3，以**目标模式（/goal）**工作：只产出落地改动与可核对证据。
- 第一性原理；**禁止打补丁**——install.ps1 硬化是根治（路径派生自环境），不是加参数绕过；手册是体系（总图→阶段→步骤→排错四层），不是命令堆砌。
- 尽量用内置 ZCode 子代理；机械校验（每条命令逐一实跑冒烟、图表导出）可扇出 Flash 子代理；wbx 未登录则不用。
- 手册是给**完全不懂技术的菜鸟**看的：任何术语（仓库/克隆/终端）首次出现必须用一句话解释；任何需要打字的地方都给可复制粘贴块；禁止「自行判断」「酌情」类措辞。

## 1. 上下文承接（冷启动固定顺序）

1. 读 `RESEARCH-V5.md`（§5 D 系＋§3 R4）与 `PLAN-V5.md` §6（收尾链铁则）
2. 读 `install.ps1`、`build.cmd`、`verify.cmd`、`使用说明.txt`（v6）、`README.md`
3. 读 `docs/VERIFY-V5.md`（I 波发布证据，手册要引用 Release 地址与 SHA256SUMS）
4. `gh release view v1.0.0` 确认正式 Release 在（clone 路径与资产是手册的事实基础）
- **前置校验**：main 干净且含 I 波合并；`gh repo view Oiawlm/OneClickBlackScreen` 可达。不满足→停止报告。

## 2. 背景

项目即将迁往第二台 Windows 电脑（U4＝完整开发环境），原机随后清理（U6）。R4 已证：csc 与 PowerShell 5.1 为 Windows 自带、Win10 1903+/Win11 出厂含 .NET FW 4.8——PC2 只需装 git（winget 一条命令）即可 clone＋build＋verify。主路径＝GitHub clone；离线备援＝git bundle（含未推送的本地分支）。本机硬编码 `C:\Users\Lenovo\...` 在 install.ps1（DeskBox 路径），PC2 用户名不同即失效——先根治再打包。

## 3. Phase 任务

### Phase 1 · 迁移就绪硬化（D1）
- `install.ps1`：DeskBox 路径改 `$env:USERPROFILE\DeskBox\DeskBox` 派生（去两处硬编码；回读校验逻辑不变）；顶部加一行注释说明可改的目标目录变量。本机实跑一次幂等验证（DeskBox 快捷方式重建成功）。
- `git status` 确认硬化后 `verify.cmd` 仍全绿（脚本不影响构建，但按惯例守门）。

### Phase 2 · 迁移包（D3）
- 新建 `migrate/` 目录：
  - `迁移手册.md`（Phase 3，主交付物）＋`迁移手册.html`（同一内容的可打印版，图表内嵌）
  - `一键黑屏.bundle`：`git bundle create migrate/一键黑屏.bundle --all`（含全部本地分支＝A5 不推分支的保全）
  - `skills-三件套.zip`：`fts-style-system`/`glm-prompt-crafter`/`github-readme` 三个用户级 skill 目录打包（wb-bridge 可选附上）
  - `settings-可选备份.ini`：复制 `%LOCALAPPDATA%\BlackScreen\settings.ini`（手册标注「个性化配置，不想带走可删」）
- 生成 `migrate/SHA256SUMS.txt`（bundle＋zip＋ini 全部条目）。

### Phase 3 · 菜鸟级迁移手册（D4，核心交付物）
- 四层结构：①总流程图（阶段级，mermaid＋导出 PNG 双载体）②每阶段小图③步骤卡（编号、目的一句、在哪操作、逐条复制粘贴命令、**预期输出样例**、完成标志）④排错表（症状→原因→照抄修复命令）。
- 步骤卡内容（按序）：
  - PC2 准备：在哪打开 PowerShell（开始菜单搜「PowerShell」回车＋Win+X 路径双写法）、`winget install --id Git.Git -e --source winget`（含 winget 不存在时的离线安装链接）、git 首次配置（user.name/user.email，给照抄行）
  - 主路径（在线）：`git clone https://github.com/Oiawlm/OneClickBlackScreen` → `cd` → `git config core.quotepath false` → `build.cmd`（在哪个目录用什么打开——「文件资源管理器进入目录→地址栏输 cmd 回车」这种零门槛路径也要写）→ `verify.cmd` 全绿判读 → `install.ps1`（PowerSphere 执行策略一条放行命令照抄）→ 双击冒烟
  - 备援路径（离线）：迁移包从旧机拷到 PC2（U 盘/微信传输二选一）→ `git clone 一键黑屏.bundle OneClickBlackScreen` 全量还原（含分支）
  - ZCode 与 skills：官网装 ZCode＋登录 → `%USERPROFILE%\.zcode\skills\` 建法与解压 skills-三件套.zip（每步图形化描述：按什么键、点什么按钮）
  - gh（可选，未来要发布才装）：winget 一条＋`gh auth login` 交互步骤
- 每条命令**逐条实跑冒烟**（可扇出 Flash 子代理在临时目录验证 winget/clone/build 链路可复制性；涉及 PC2 特有步骤如实标注「本机无法代验，按预期输出比对」）。
- 收尾：PC2 验证口径（D6 五项：fsck/status/verify 全绿/双击遮罩/设置窗）打印式勾选清单——这是回旧机跑 cleanup 的准入证。

### Phase 4 · 本地清理脚本（D5）
- 新建 `migrate/cleanup-local.ps1`：参数 `-ConfirmPC2Verified`（**缺省即拒绝运行**并打印 D6 清单提示）；执行序：
  1. 打包留档：仓库 zip→桌面（`一键黑屏-最终留档-日期.zip`）＋SHA256 打印（用户日后自行处置）
  2. 删除：仓库目录（脚本所在仓库自身——用「标记后自删」或先复制脚本到桌面再执行，写清操作序）；DeskBox 两个 .lnk（存在才删）；`%LOCALAPPDATA%\BlackScreen`；计划任务 `OneClickBlackScreen-Idle`（`schtasks /delete /tn ... /f`，退出码 1＝本就不存在＝放行）；ZCode 本工作区痕迹（只**列出**可定位项供界面内手动移除，不盲删）
  3. **保留**：`%USERPROFILE%\.zcode\skills`（用户级、非项目专属——输出一句说明，最终去留由用户定）
  4. 每步打印结果清单；任何一步失败→停在后置步骤前，报告已完成/未完成。
- 脚本头部注释＝红线声明：只应由用户在 PC2 验证通过后手动执行；任何 AI 窗口不得代跑。
- **本窗口只生成、只做 dry-run 打印演练（`-WhatIf` 式列表输出），绝不真删。**

### Phase 5 · 收尾
- `docs/MIGRATE-V5D.md`：包清单＋SHA256＋命令冒烟记录＋dry-run 输出。
- 手册首页放「总流程图」与「三句话版本」（急用速览）；排版用 github-readme 技巧（表格/折叠块）。

## 4. 铁律

1. 不打补丁；手册是体系不是命令堆；install.ps1 硬化是根治。
2. **清理脚本绝不执行**（含真删演练）；删除动作唯一合法触发者＝用户在 PC2 验证后手动运行。
3. 凭证不入迁移包（bundle/zip 不含 `.wbx/`、token、密钥——打包前逐项检查）。
4. 手册事实必须与仓库/Release 实况一致（地址、资产名、SHA256 以 `gh`/certutil 实测为准，不凭记忆写）。
5. 每条给用户的命令先自己跑通（本机可跑的必须跑）；跑不了的标「本机未代验」。
6. RESEARCH-V5 有误→先登记其 §8 勘误。

## 5. 验收清单（Done when，全部可核对）

1. `install.ps1` 无 `C:\Users\Lenovo` 字样（`git grep -n "C:\\\\Users\\\\Lenovo" install.ps1` 零命中）且本机幂等实跑两遍均 `[OK]`；
2. `migrate/` 四件齐：迁移手册.md（≥1 总流程图＋每阶段步骤卡＋排错表）、迁移手册.html、一键黑屏.bundle（`git bundle verify` 输出含全部分支）、skills-三件套.zip；
3. `migrate/SHA256SUMS.txt` 存在且 `certutil` 抽验两条一致；
4. 手册含「在哪打开 PowerShell」图文化步骤与每命令预期输出样例；命令块总数≥15 且逐一冒烟记录在 docs/MIGRATE-V5D.md；
5. `migrate/cleanup-local.ps1` 无 `-ConfirmPC2Verified` 运行时拒绝并打印 D6 清单（实跑一次证明拒绝路径）；dry-run 输出落 docs/MIGRATE-V5D.md；
6. 手册含 PC2 验证五项勾选清单与「验证通过前不删旧机任何东西」警示；
7. `docs/MIGRATE-V5D.md` 存在含包清单＋冒烟记录＋dry-run 证据；
8. `git status` 干净（migrate/ 与硬化改动已提交；`migrate/*.zip|*.bundle|*.ini` 是否入库遵循 .gitignore 扩充——大文件不入库、登记路径即可）。

## 6. 收尾动作

- 提交推送；`docs/MIGRATE-V5D.md` 标注「轨道 D 完成，待用户执行 PC2 迁移→验证→本地清理」；
- 把「migrate/ 产物清单＋下一步三句话」（拷包→PC2 照手册做→验证后跑 cleanup）写入 PLAN-V5 §6 之后作为用户终局提示。

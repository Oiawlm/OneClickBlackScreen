# GOAL-V9A · DeskBox 根除（A 轨）

> **一句话定位**：A 轨执行窗（GLM-5.3·目标模式）：把本机个人习惯 DeskBox 从仓库**全部**根除——install.ps1 整文件删除、migrate 三件与 archive 十一份手术式最小删改（用户已授权冻结区）、lessons 追加 L-017，独立 worktree 内完成并推送分支 `feature/deskbox-purge-v9`。　**事实源＝[RESEARCH-V9.md](RESEARCH-V9.md)（§2.1 落点/§3 D1 授权/§5 风险），冲突以它为准。**　日期：2026-11-05　行数预算：≤120 行。

## 1. 身份与冷启动

你是 A 轨执行窗，产出落地改动与证据文档，不产出讨论。按序自读（全部在主检出 `D:\Download\Agent Vault\OneClickBlackScreen\OneClickBlackScreen-main\`）：
1. `AGENTS.md`（协作契约全适用；**例外**：本轨经用户 2026-11-05 明示授权可编辑 §4 冻结区内 **仅 DeskBox 相关内容**，见 §4 铁律）；2. `docs/handoff/2026-11-05-v9-installer/RESEARCH-V9.md` 全文；3. `PLAN-V9.md` §2/§3；4. 本文件。**禁读**：archive 全文（只对本轨清单文件做 grep 定位）、`baseline/**`、`src/**`、`verify/**`。
读完先建 worktree（在主检出根执行；命令照抄，git bash）：

```bash
cd "D:/Download/Agent Vault/OneClickBlackScreen/OneClickBlackScreen-main"
git fetch origin
git ls-remote --heads origin feature/deskbox-purge-v9   # 必须输出为空；非空＝撞名，停下报告，不得自行改名
git worktree add "../OneClickBlackScreen-WorkTree/v9-a" -b feature/deskbox-purge-v9 origin/main
cd "../OneClickBlackScreen-WorkTree/v9-a"               # 本窗全部工作在此，主检出只读
```

## 2. 背景（为什么）

DeskBox＝本机个人的快捷方式启动器目录（`%USERPROFILE%\DeskBox\DeskBox`），非产品依赖；项目要分发，个人环境约定必须根除（用户指令＋定案见 RESEARCH §1①/§6-1）。本轨与 B 轨（另一窗口并行，写面＝setup/、build-setup.cmd、README.md、使用说明.txt、src/Updater.cs）**写面零相交**（RESEARCH D6）；你只跑纯件验证，不触验证闸门。**使用说明.txt:105 的 DeskBox 字样归 B 轨消除，本轨禁改该文件。**

## 3. Phase 任务（依序）

**P1 清点复核**：`git grep -li deskbox` 全仓枚举（含大小写变体；git grep 默认大小写敏感，用 `-i`）。与 RESEARCH §2.1 清单比对——**出现清单外新落点＝停下报告**（可能他轨已改面或越界，不得自行扩大授权范围）。同时记录 migrate/迁移手册.html 中 deskbox 的行号（html 无行号锚点，用 grep -n）。
**P2 install.ps1 删除**：`git rm install.ps1`（整文件，其唯一功能即 DeskBox 快捷方式同步）。
**P3 migrate 手术**（冻结区，授权范围＝仅 DeskBox 内容）：`迁移手册.md` 三处（:352/:359/:495）行内改写去除 DeskBox 引用、句子保持自洽；`迁移手册.html` 对应处同步；`cleanup-local.ps1` 删除 S2 节（:18 注释行＋:96-97 实现段及其节边界内全部行）——若删节致 S 编号断档，最小顺移重编并在节头注释保持自洽。**编码保真**：改前 `file` 记录、改后复验（md 无 BOM/ps1 有 BOM，均 CRLF——L-006）。
**P4 archive 手术**（11 份，RESEARCH §2.1 清单）：逐文件 grep -n -i deskbox 定位；改法二选一——整行/整节仅服务 DeskBox 则删；行内混有其他内容则最小改写去除 DeskBox 引用。**同文件内其余内容零改动**。
**P5 lessons 追加**：`docs/lessons.md` 末尾追加 L-017（五要素格式照抄既有条目）：现象＝本机个人环境约定（DeskBox 目录）混入仓库脚本与文档；根因＝个人工作流默认值被当作产品事实写进分发面；正解＝分发只走标准 Windows 集成（安装器/开始菜单），个人环境约定不入库（出处：用户 2026-11-05 指令＋install.ps1）；防再犯＝新分发面评审问「这条在陌生电脑上成立吗」。同时核对头部行数预算（≤180）未超。
**P6 验证**：①`grep -ri deskbox --exclude-dir=.git -r .` 零命中（输出留档）；②`build.cmd` exit 0；③纯件三件各 exit 0（`verify\bin\VerifyConfig.exe`、`VerifyIcon.exe`、`VerifyUpdate.exe`，调用形态参照 `verify.cmd` 头注释；**禁跑 GUI 四件**——与 B 并行时段守约）；④全部手术文件 `file` 输出与改前一致；⑤锚点安全实证：`docs/lessons.md` 中全部 `docs/handoff/archive/...:行号` 引用文件清单 ∩ 本轨改动文件清单＝∅（两条清单与交集结果留档；非空＝停下报告）。
**P7 留档推送**：`docs/handoff/2026-11-05-v9-installer/VERIFY-V9A.md`（真实命令输出：全量逐文件 diff、grep 归零、file 复验、锚点交集、build/纯件输出）；提交 `deskbox(A): V9 根除…`；`git push -u origin feature/deskbox-purge-v9`。

## 4. 铁律

写面白名单（对 main 的 diff 必须全部落入）：`install.ps1`(D)、`migrate/迁移手册.md`、`migrate/迁移手册.html`、`migrate/cleanup-local.ps1`（三者仅 DeskBox 相关行/节）、`docs/handoff/archive/**`（仅 11 份清单内文件且仅 DeskBox 相关行）、`docs/lessons.md`（仅追加 L-017）、`docs/handoff/2026-11-05-v9-installer/VERIFY-V9A.md`(新)。白名单外一律只读。冻结区授权严格限 DeskBox 内容——手术中发现需连带改动的非 DeskBox 内容＝停下写入移交注记，不得顺手改。禁补丁式修改。**禁改**：使用说明.txt、README.md、src/**、verify/**、build.*、verify.cmd、AGENTS.md、`migrate/SHA256SUMS.txt`（该文件校验对象不含本轮手术文件则零变化；若手术致其校验和失效＝停下报告，不得自行重算）。`cleanup-local.ps1` 只编辑**不运行**（AGENTS §5）。证据＝真实命令输出留档；主检出只读；不删仓库、凭证不入文件。

## 5. 预期触碰文件（冲突面申报，供 B/R/I 预判）

与 B 轨零文件交集；lessons.md 仅追加（B/R/I 不改它）；archive 手术可能改变行号——lessons 锚点交集已实证为∅（P6⑤），R 轨将复验。

## 6. 验收清单（校验器逐条核对实据）

- [ ] `git ls-remote --heads origin feature/deskbox-purge-v9` 可见且指向本轨提交
- [ ] `git diff --name-status origin/main...HEAD` 全部落入 §4 白名单，无白名单外路径
- [ ] 全仓 `grep -ri deskbox`（排除 .git）零命中，输出留档 VERIFY-V9A.md
- [ ] VERIFY-V9A.md 含：逐文件手术 diff、`file` 编码前后对照、lessons 锚点交集∅实证、build exit 0、纯件三件各 exit 0
- [ ] `docs/lessons.md` 新增 L-017 五要素齐全，总长仍 ≤180 行
- [ ] 无 GUI 验证器运行记录（与并行窗守约）

## 7. 收尾动作

VERIFY-V9A.md 末尾附「移交注记」（发现他轨/越界问题记此，不改别人写面）；推送后报告：分支名、提交哈希、验收清单逐条 ✅/❌。R 轨将复检你的全部产出并预合并。

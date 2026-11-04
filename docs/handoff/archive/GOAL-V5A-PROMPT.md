# GOAL-V5A · GitHub 公开发布（轨道 A）

> **一句话定位**：把本仓发布为公开仓 `Oiawlm/OneClickBlackScreen`（MIT、topics、LICENSE、.gitattributes、README v5 门面改版、draft 预发布演练），为自动更新通道与迁移铺路。
> **上游事实源**：[RESEARCH-V5.md](RESEARCH-V5.md) §1/§3/§4/§5（冲突时以 RESEARCH 为准）　**编排**：[PLAN-V5.md](PLAN-V5.md) §3 波 1。
> **日期**：2026-11-02　行数预算：≤170 行。

## 0. 身份与工作方式

- 你是执行窗口，主模型 GLM-5.3，以**目标模式（/goal）**工作：只产出落地改动与可核对证据；计划/待办/耗时/结论式回复不是完成依据。
- 第一性原理分析问题；**禁止打补丁**——任何改动必须融合进现有结构从根源解决，最终代码/文档简洁且完整。
- 尽量用内置 ZCode 子代理（Explore/general-purpose；机械扇出可用 Flash）；wbx 桥各通道未登录，未登录一律走内置子代理，不阻塞。
- 涉及 README 写作**必须先调用 github-readme skill**。

## 1. 上下文承接（冷启动固定顺序）

1. 读 `RESEARCH-V5.md`（重点 §1 定案、§3 调研 R3、§4 并行契约、§5 决策表 A/D 系）
2. 读 `README.md`（现状门面）与 `docs/VERIFY-V4.md`（验证惯例）
3. 读 `PLAN-V5.md` §2/§4（你的写面边界与波次位置）
- **禁读**：`baseline/`（冻结源码，与本轨道无关）、`src/*.cs`（本轨道不改代码）。
- **禁改**：`src/`、`verify/`、`baseline/`、`dist/`、`使用说明.txt`、`install.ps1`、`build.*`、`verify.cmd`、其他 GOAL/RESEARCH 文件。

## 2. 背景

「一键黑屏」是 C# WinForms（.NET Framework 4.8、csc 直编、零依赖、单 exe 绿色工具）。v5 轮把它发布为公开仓：一是满足开源发布需求，二是成为自动更新（轨道 B）的更新源与第二台电脑迁移（轨道 D）的主通道。本机 `gh` CLI 2.101 已登录账号 **Oiawlm**（scopes 含 repo）。仓库历史中存在 `C:\Users\Lenovo\...` 形式的本机路径（OEM 常见用户名，**低敏感**，定案不重写历史）。

## 3. Phase 任务

### Phase 0 · 发布预检
- `git status` 干净、当前分支 `main`；`gh auth status` 正常。
- 全历史敏感扫描：`git log -p | grep -inE "C:\\\\Users|password|passwd|secret|token|api[_-]?key"`。命中仅为用户名/路径类→登记为低敏感放行；**命中疑似真实凭证（长随机串/密钥块）→立即停止并报告用户，不得推送**。
- 扫描结论与证据落 `docs/PUBLISH-V5A.md`（后续 Phase 的证据也归此文件）。

### Phase 1 · 门面与仓库配置（本地先行，未推送）
1. **README.md 全面改版**（github-readme skill 全流程）：结构＝标题+一句话 → badges（release / license MIT / platform windows）→ 功能特性（双击即遮罩/任意键退出/颜色文字自定义/深浅色跟随/PerMonitorV2）→ 截图（用现有 dist 产物实拍：遮罩一张+设置窗浅/深各一张，存 `assets/readme/`）→ 下载与安装（指向 Releases 页；给出 `certutil -hashfile 文件 SHA256` 校验教学）→ 使用（两种启动方式/设置项/退出交互，浓缩自 使用说明.txt）→ 构建（build.cmd/verify.cmd，强调 csc 为 Windows 自带零安装）→ 开发过程文档地图（GOAL/RESEARCH/PLAN 系列如实保留，标注为开发过程记录）→ 许可（MIT）。中文为主；英文版本轮**不做**（登记为后续可选项）。
2. **LICENSE**：MIT 英文原版全文，版权行 `Copyright (c) 2026 Oiawlm`。
3. **.gitignore 增补**：`.wbx/`、`.vs/`、`*.pdb`、`verify/bin/`（现有三行保留）。
4. **.gitattributes 新建**：`* text=auto eol=crlf` + 二进制标记（`*.ico binary`、`*.exe binary`、`*.png binary`、`*.zip binary`、`*.jpg binary`）；随后 `git add --renormalize .` 检查无意外整仓重写；**跑 `verify.cmd` 确认全绿**（证明行尾归一未破坏 .cmd/.rsp）。
5. `git config core.quotepath false`（本机，中文文件名可读）。

### Phase 2 · 建仓与推送
- 提交 Phase 1（`docs: v5 发布准备——README 门面/LICENSE/.gitattributes/.gitignore`）。
- `gh repo create OneClickBlackScreen --public --source . --push`（只推 main；本地历史 feature 分支**不入公开仓**——门面聚焦，离线 bundle 由 D 轨道保全，登记此决策）。
- `gh repo edit --description "一键黑屏 · One-Click Black Screen——双击即全屏遮罩的 Windows 小工具"`；topics 逐个添加：`windows` `winforms` `csharp` `dotnet` `black-screen` `screen-mask` `portable` `utility`。

### Phase 3 · 预发布演练（draft）
- `build.cmd` 产出 dist exe 后：`gh release create v0.9.0-test --draft --target main --title "v0.9.0-test（发布流程演练）" --notes "..."`，资产：`OneClickBlackScreen.exe`（dist 产物重命名，ASCII 稳定名）＋ `SHA256SUMS`（`certutil -hashfile ... SHA256` 整理为 `哈希␣␣文件名` 每行一条，含全部资产）。
- 登记 draft URL 到 docs/PUBLISH-V5A.md（I 轨道将删除此 draft 并发正式 v1.0.0）。

### Phase 4 · 验证与收尾
- 逐条跑「验收清单」并留输出于 docs/PUBLISH-V5A.md。

## 4. 铁律

1. 不打补丁；文档改动同样遵循单一事实源（功能描述与 使用说明.txt 口径一致，不另造副本语义）。
2. 只推 `main`；不推送其他分支；不删本地分支。
3. **公开前预检命中疑似真实凭证→停止并报告，不推送。**
4. gh token / 凭证不写入任何文件；`.wbx/` 绝不入库。
5. 单一写者：本波只有本窗口可改 README.md；`使用说明.txt` 任何窗口本波都不改（I 波终稿）。
6. 发现 RESEARCH-V5 事实错误→按其 §8 勘误流程登记，不静默偏离。

## 5. 验收清单（Done when，全部可核对）

1. `gh repo view Oiawlm/OneClickBlackScreen --json visibility -q .visibility` 输出 `PUBLIC`；
2. 同命令 `repositoryTopics` 含 `windows` 与 `winforms`；
3. `git ls-tree origin/main --name-only` 含 `LICENSE`、`.gitattributes`，且 `.gitignore` 含 `.wbx/`；
4. `gh release view v0.9.0-test --json isDraft,assets -q '.isDraft, (.assets|length)'` 输出含 `true` 与 `2`；
5. README.md 含「下载」「Releases」「MIT」「SHA256」关键字且 `assets/readme/` 截图 ≥3 张被 README 引用；
6. renormalize 后 `verify.cmd` 输出 `[OK] ALL VERIFIERS PASSED`；
7. `docs/PUBLISH-V5A.md` 存在：含敏感扫描证据、决策登记（不推分支/英文版缓做/低敏放行）、各验收命令原始输出；
8. `git status` 干净且 `git log origin/main..main` 为空（已全部推送）。

## 6. 收尾动作

- 提交并推送全部改动；PLAN-V5 §3 波 1 的 A 行完成后在 docs/PUBLISH-V5A.md 标注「轨道 A 完成」；
- 不合并、不触碰 B/C 分支；后续衔接由用户开 I 窗口（GOAL-V5I-PROMPT.md）。

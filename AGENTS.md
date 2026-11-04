# AGENTS.md · OneClickBlackScreen 多窗口协作契约

> **一句话定位**：本仓库所有 AI 窗口（规划/执行/复检/集成）共用的项目级规则——独占资源、单一写者、冻结区、红线与工作原则；ZCode 各窗口自动注入，提示词不再重复。　**来源**：V7 轮规划窗口依 RESEARCH-V7 §3/§6/§8 定稿。　**日期**：2026-11-04　行数预算：≤60 行。

## 1. 冷启动路由

- 先读本轮（当前最大 V 号）`RESEARCH-V<n>.md` → `PLAN-V<n>.md` → 本窗口对应 `GOAL-V<n>?-PROMPT.md`；冲突时以 RESEARCH 为准。
- 禁读历史轮 RESEARCH/PLAN/GOAL 全文（过程记录，防上下文膨胀）；要惯例只 grep 定位。

## 2. 独占资源（多窗口并发冲突的真实来源）

- **验证套件是会话级独占**：`verify.cmd` 及单跑 GUI 验证器（VerifySettings/MouseExit/Overlay/Idle）注入全局键鼠、抢占前台——全局输入队列属于整个登录会话，**同一台机器同一时间至多一个窗口跑它们**。V7 起验证器内置闸门（`Local\BlackScreen.VerifyGate`）：撞闸输出 BUSY、退出码 2；正确应对＝等对方结束再跑，**不得绕过闸门、不得盲目重试风暴**。纯件 VerifyConfig/VerifyIcon/VerifyUpdate 无全局副作用。
- **一个 checkout 同一时间只归一个窗口**：build/verify 写 `dist\` 与 `verify\bin\` 固定路径，共用必撞文件锁。未来并行轮必须各用独立 git worktree（V6B 事故教训：共享单工作树的分支切换会互相劫持检出）。
- **真实用户状态只读**：`%LOCALAPPDATA%\BlackScreen\settings.ini` 与计划任务 `OneClickBlackScreen-Idle` 属于用户；验证一律走 TEMP/改名隔离，套件跑完用户状态字节级不变（零足迹）。

## 3. 单一写者

- 每条轨道的写面白名单在其 GOAL 铁律声明；白名单之外一律只读。
- 发现他轨/越界问题：写入自己的证据文档「移交注记」，不得直接改别人的写面；由复检/集成波转补。

## 4. 冻结区（任何窗口不得写）

- `baseline/**`（v1 行为基准）；`migrate/**` 与 `SHA256SUMS.txt`（迁移归档，只读校验）。
- 历史 RESEARCH/PLAN/GOAL 与 docs/ 既有证据文档（过程记录；I 轨发版辖内除外）。
- 版本号唯一手写来源＝`src/AppInfo.cs`，仅 I 轨可改。

## 5. 红线

- 不删除仓库、不改写远端历史；`migrate\cleanup-local.ps1` 永远由用户在旧机亲手执行，任何 AI 窗口不得代跑。
- 凭证（token/密码）不入任何文件；产品代码变更必须走完整发版链（版本单源→tag→Release→应用内冒烟）后才算交付。

## 6. 工作原则

- 第一性原理分析；**禁止打补丁**：任何修改/修复/新增必须重构融入既有结构、从根源解决，最终代码简洁完整，不为凑需求叠旁路。
- 尽量使用内置 ZCode 子代理（Explore 查证/general-purpose 调研扇出；机械扫描可用 Flash 子代理）。
- 上下文只经文件传递，不依赖会话记忆；证据＝真实命令输出留档 docs/，不是结论式转述。

# PUBLISH-V5A · GitHub 公开发布证据（轨道 A）

> 日期：2026-11-02　轨道：A（GOAL-V5A）　分支：main　执行：GLM-5.3 目标模式
> 上游：[RESEARCH-V5.md](../RESEARCH-V5.md) §5 A 系决策表 A1–A8；产物：公开仓 `Oiawlm/OneClickBlackScreen`。

## 0. 发布预检（Phase 0）

### 0.1 基线状态

```
$ git status          → On branch main / nothing to commit, working tree clean
$ git branch          → main（当前）+ feature/frontend-v2, feature/frontend-v3,
                        feature/mask-customization, feature/style-v4（仅本地，按 A5 不推送）
$ gh auth status      → ✓ Logged in to github.com account Oiawlm (keyring)
                        Token scopes: 'delete_repo', 'gist', 'read:org', 'repo', 'workflow'
```

### 0.2 全历史敏感扫描

命令（GOAL 原文）：

```
git log -p | grep -inE "C:\\\\Users|password|passwd|secret|token|api[_-]?key"   → 29 行命中
```

**环境勘误（登记）**：本机 Git Bash（MSYS）对双引号内 `\\\\` 存在参数改写，该 ERE 模式实际退化为
不匹配任何 `C:\Users` 字面路径（独立验证：`git log -p | grep -icE "C:\\\\Users"` → **0**，
而 `grep -icF 'C:\Users'` → **25**）。为覆盖路径面，追加固定串全量扫描：

```
git log -p | grep -icF 'C:\Users'    → 25 行命中（全部为 C:\Users\Lenovo\... 形式）
```

命中逐类判定（两类合并共 54 行，互不重叠）：

| 类别 | 数量 | 判定 |
|---|---|---|
| 文档政策文本（「gh token / 凭证不写入任何文件」「凭证不入库/不入迁移包」等 GOAL/RESEARCH/PLAN 条款本身） | 21 | 非凭证，是安全政策声明 |
| 「幽灵 token」/「token 单一数值源」等 fts 设计术语、`InteractiveToken`（计划任务 principal 类型）、`TimeTrigger` 等 | 8 | 技术术语 |
| `C:\Users\Lenovo\...` 本机路径（README 来源行、RESEARCH-V4 skill 路径、GOAL-V5D 手册命令示例等） | 25 | **低敏感**：Lenovo＝OEM 预置用户名，非真实人名/账号；按 RESEARCH-V5 A4 定案放行，不重写历史 |

真实凭证模式独立扫描（长随机串/密钥块）：

```
git log -p | grep -inE "gh[pousr]_[A-Za-z0-9]{20,}|github_pat_|AKIA[0-9A-Z]{16}|BEGIN (RSA|EC|OPENSSH|PRIVATE)|bearer +[A-Za-z0-9._-]{20,}|xox[bap]-|sk-[A-Za-z0-9]{20,}"
  → 零命中
git grep -inE "gh[pousr]_[A-Za-z0-9]{20,}|github_pat_|AKIA[0-9A-Z]{16}|BEGIN (RSA|EC|OPENSSH|PRIVATE)" HEAD
  → 零命中（当前树）
```

**结论：无真实凭证，全部命中为低敏感（用户名路径/政策文本/术语）→ 放行推送（A4）。**

另：本机 `%LOCALAPPDATA%\BlackScreen\settings.ini` 含用户个人通告文字（不在仓内、不入截图）——
README 截图一律使用探针自带演示配置（`color=#1F3A5F`＋演示文字），探针字节级备份恢复用户配置。

## 决策登记

| # | 决策 | 依据 |
|---|---|---|
| P1 | 只推 `main`；本地 4 个 feature/* 分支不入公开仓（门面聚焦；全量分支由 D 轨道离线 bundle 保全） | A5 |
| P2 | README 英文版本轮不做，登记为后续可选项 | A2 |
| P3 | 历史中 `C:\Users\Lenovo` 路径低敏放行，不重写历史 | A4 |
| P4 | 截图使用探针演示配置，杜绝用户个人配置（含手机号）入镜 | §0.2 另注 |

## 1. 门面与仓库配置（Phase 1）

（待补：README/LICENSE/.gitignore/.gitattributes/renormalize/verify 证据）

## 2. 建仓与推送（Phase 2）

（待补）

## 3. 预发布演练（Phase 3）

（待补）

## 4. 验收清单核对（Phase 4）

（待补）

# 多窗口并行规程 · worktree 创建 / 推送 / 清理

> **一句话定位**：并行轮「一轨一分支一 worktree」的落地规程——壳形与路径规范、创建模板（查重＋撞名即停）、清理模板（合并即清）、写权归属与壳内禁散物；与 AGENTS.md §2 worktree 条款互为引用。　**来源**：RESEARCH-V8 §2.1/§4.1。　**日期**：2026-11-05　行数预算：≤80 行。

## 1. 壳形与路径规范

```text
Agent Vault\OneClickBlackScreen\            ← 壳（总目录）：只允许下面两项，禁放任何散物
├── OneClickBlackScreen-main\               ← 主检出（常驻 main）：集成窗独占写；例外＝规划窗提交轮次文档
└── OneClickBlackScreen-WorkTree\           ← worktree 父目录：一轨一分支一子目录，由各执行窗自建自清
```

- 子目录命名＝`v<n>-<轨>`（如 `v8-s`、`v8-l`）；对应分支＝`feature/<slug>-v<n>`（如 `feature/structure-v8`）。
- **禁入仓库内**：worktree 不得建在检出内部——`git clean -fdx` 会连带清空它，且干扰全仓工具扫描；业界惯例即兄弟目录。
- 各 worktree 的 `dist\`、`verify\bin\` 构建产物物理隔离，天然不撞文件锁（AGENTS §2）。

## 2. 创建模板（执行窗在 GOAL §1 照抄；git bash，自主检出根执行）

```bash
cd "D:/Download/Agent Vault/OneClickBlackScreen/OneClickBlackScreen-main"
git fetch origin
git ls-remote --heads origin <分支>               # 必须输出为空；非空＝撞名，停下报告，不得自行改名
git worktree add "../OneClickBlackScreen-WorkTree/v<n>-<轨>" -b <分支> origin/main
cd "../OneClickBlackScreen-WorkTree/v<n>-<轨>"    # 本窗全部工作在此，主检出只读
```

- 轨内完成后 `git push -u origin <分支>`；全程不在主检出上切分支、不推主检出。
- **撞名即停**：远端已有同名分支＝他窗占位或上轮残留未清——报告用户裁决，不得覆盖、不得换名续跑。

## 3. 清理模板（合并即清理；对应轨被合并进 main 后由集成窗执行）

```bash
cd "D:/Download/Agent Vault/OneClickBlackScreen/OneClickBlackScreen-main"
git worktree remove "../OneClickBlackScreen-WorkTree/v<n>-<轨>"
git worktree prune
git branch -d <分支>        # 已合并才删得动；`-D` 强删仅集成窗终验后对确认残留的分支使用
```

## 4. 写权归属（单一写者）

| 位置 | 写权 |
|---|---|
| `OneClickBlackScreen-main\`（主检出） | 集成窗（合并/发版/终验）；例外＝规划窗提交轮次文档 |
| `OneClickBlackScreen-WorkTree\v<n>-<轨>\` | 该轨执行窗独占，轨外窗口一律只读 |
| 壳根 `OneClickBlackScreen\` | 禁放任何散物——只允许 `-main` 与 `-WorkTree` 两项 |

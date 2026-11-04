# PUBLISH-V5A · GitHub 公开发布证据（轨道 A）

> 日期：2026-11-02　轨道：A（GOAL-V5A）　分支：main（独立 worktree 执行，见 §5 执行纪事）　执行：GLM-5.3 目标模式
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
README 截图一律使用探针演示配置（`color=#1F3A5F`＋演示文字「屏幕休息中/按任意键退出」），
探针字节级备份恢复用户配置。

## 决策登记

| # | 决策 | 依据 |
|---|---|---|
| P1 | 只推 `main`；本地 4 个 feature/* 分支不入公开仓（门面聚焦；全量分支由 D 轨道离线 bundle 保全） | A5 |
| P2 | README 英文版本轮不做，登记为后续可选项 | A2 |
| P3 | 历史中 `C:\Users\Lenovo` 路径低敏放行，不重写历史 | A4 |
| P4 | 截图使用演示配置，杜绝用户个人配置（含手机号）入镜 | §0.2 另注 |
| P5 | A 轨道在独立 git worktree（`..\一键黑屏-A-main`）执行：共享工作树当时被 B 轨道切至 feature/update-v5 且含 C 轨道未提交 WIP，直接切 main 会破坏并行轨道现场 | PLAN-V5 §4 单一写者 |
| P6 | 不加 GitHub Actions CI（极简哲学，登记） | A8 |

## 1. 门面与仓库配置（Phase 1）

- **构建基线**（worktree 内，干净 main@735568b）：`cmd /c build.cmd` → `[OK] Build succeeded`，
  dist exe SHA-256 `7ac83427798d22427ab83b7c264f9ae850412e9350a7bf35a197f5487238dcff`（244,736 B）。
- **截图三张**（`assets/readme/`，全部 PrintWindow 窗口级导出，桌面像素零入镜）：
  - `mask-demo.png` 2560×1600（物理像素，演示配置 #1F3A5F＋文字；像素断言：四角/中心=演示色、白色文字像素 9,617）
  - `settings-light.png` 666×965（SnapSettings 真 exe PrintWindow，系统浅色）
  - `settings-dark.png` 446×646（DarkProbe 反射强制 ApplyTheme(false)，不触碰系统主题；v4 证据同口径尺寸）
  - 调色板断言：light bg#F5F5F5×58,772 / 主按钮#0067C0×1,425 / 预览#1F3A5F×56,737；dark bg#202020×25,428 / #4CC2FF×643 / 预览×25,009
- **README.md** 全面改版：QuickLook 同构中文主（badges→功能→截图→下载与 SHA256 教学→使用→边界如实→数据与卸载→构建→开发过程文档→MIT）；旧文档地图信息零丢失（并入「📖 开发过程文档」节）。
- **LICENSE**：MIT 英文原版全文，`Copyright (c) 2026 Oiawlm`。
- **.gitignore**：`_probe/ dist/ *.exe`（原三行）＋ `.wbx/ .vs/ *.pdb verify/bin/`（增补四行）。
- **.gitattributes**：`* text=auto eol=crlf` ＋ `*.ico/*.exe/*.png/*.zip/*.jpg binary`。
- **renormalize**：`git add --renormalize .` → 仅 `.gitignore`＋`README.md` 两个预期改动，**无整仓重写**。
- **renormalize 后验证**：`cmd /c verify.cmd` → 五件套全 PASS，末行 `[OK] ALL VERIFIERS PASSED`
  （VerifySettings 6 查含几何镜像 17 子控件 ±1px；基线轮与 renormalize 后各跑一次，均全绿）。
- `git config core.quotepath false`（仓库级，中文文件名可读）。

## 2. 建仓与推送（Phase 2）

```
$ git commit -m "docs: v5 发布准备——README 门面/LICENSE/.gitattributes/.gitignore ..."
  → [main 7ee77ca] 8 files changed, 225 insertions(+), 36 deletions(-)
$ gh repo create Oiawlm/OneClickBlackScreen --public --description "一键黑屏 · One-Click Black Screen——双击即全屏遮罩的 Windows 小工具"
  → https://github.com/Oiawlm/OneClickBlackScreen
$ git push -u origin main
  → * [new branch] main -> main ；branch 'main' set up to track 'origin/main'
$ gh repo edit --add-topic × 8（windows/winforms/csharp/dotnet/black-screen/screen-mask/portable/utility）
  → 逐个 OK
$ gh repo view -q '.visibility, topics, description'
  → PUBLIC / windows,winforms,csharp,dotnet,black-screen,screen-mask,portable,utility / （同 description）
```

注：`gh repo create --source .` 不识别 worktree（`.git` 为文件），故分两步（先建仓再挂 remote 推送），效果等同且只推 main。

## 3. 预发布演练（Phase 3，draft）

```
$ gh release create v0.9.0-test --draft --target 7ee77ca3277e286099b99d03fb94657280d27dd8 \
    --title "v0.9.0-test（发布流程演练）" --notes-file ... \
    OneClickBlackScreen.exe SHA256SUMS
  → https://github.com/Oiawlm/OneClickBlackScreen/releases/tag/untagged-5a799031c84bb282105f
$ gh release view v0.9.0-test --json isDraft,assets -q '.isDraft, (.assets|length)'
  → true / 2（资产：OneClickBlackScreen.exe、SHA256SUMS）
```

`SHA256SUMS` 内容（`哈希␣␣文件名`，certutil 生成）：

```
7ac83427798d22427ab83b7c264f9ae850412e9350a7bf35a197f5487238dcff  OneClickBlackScreen.exe
```

（draft URL 呈 untagged- 前缀＝GitHub 对 draft 的常态，发布时落 tag；I 轨道将删除此 draft 并发正式 v1.0.0。）

## 4. 验收清单核对（Phase 4，逐项原始输出）

| # | 验收项 | 结果 | 原始输出 |
|---|---|---|---|
| 1 | `gh repo view … -q .visibility` | ✅ | `PUBLIC` |
| 2 | 同命令 `repositoryTopics` 含 windows 与 winforms | ✅ | `windows,winforms,csharp,dotnet,black-screen,screen-mask,portable,utility` |
| 3 | `git ls-tree origin/main --name-only` 含 LICENSE、.gitattributes；.gitignore 含 .wbx/ | ✅ | 树含 `.gitattributes`/`LICENSE`；`.gitignore` 七行含 `.wbx/` |
| 4 | `gh release view v0.9.0-test` isDraft/assets | ✅ | `true` / `2` |
| 5 | README 含「下载/Releases/MIT/SHA256」且 ≥3 张截图被引用 | ✅ | 关键字 3/2/2/3 处；`assets/readme/{mask-demo,settings-light,settings-dark}.png` 各引用 1 次 |
| 6 | renormalize 后 verify.cmd 全绿 | ✅ | `[OK] ALL VERIFIERS PASSED`（五件套，Settings 6 查） |
| 7 | 本文件存在且证据齐全 | ✅ | 即本文件（§0–§4＋决策登记＋执行纪事） |
| 8 | `git status` 干净且 `git log origin/main..main` 为空 | ✅ | 见下方终态输出 |

终态（PUBLISH 本文件提交并推送后复核）：

```
$ git status --short        → （空）
$ git log origin/main..main --oneline | wc -l   → 0
```

徽章可达性抽查（skill §六 质量门）：Release/License/Platform 三个 shields.io 徽章 HTTP 状态均 `200`。

## 5. 执行纪事（如实记录，供并行轨道与后续轮次参考）

1. **共享工作树被并行轨道占用**：A 启动时共享目录已被 B 轨道切至 `feature/update-v5`，且工作树含 C 轨道对 `src/MaskConfig.cs` 的未提交 WIP（`ClampIdleMinutes` 半成品，共享树内 build.cmd 必失败）。A 改用 `git worktree add ..\一键黑屏-A-main main` 独立执行，全程零接触 B/C 现场（P5）。
2. **IME 子窗口干扰第 6 查（环境因素，非代码回归）**：干净 main 上 VerifySettings 报 `expected 17 direct children, saw 18`——第 18 个子窗口为 `OimeTsfNotifyWindow`（微软拼音 TSF 通知窗）。根因：本机「默认输入法替代」被设为第三方中文 IME（豆包输入法，`0804:{9D2B2E2B-…}`），新进程默认挂 IME；v3/v4 验证期会话活动布局为英文故未出现。处置：验证期间临时 `Set-WinDefaultInputMethodOverride 0409:00000409`（子窗口数复测 17/0 IME），**验证全部完成后已还原为原值（豆包输入法，已复核回读）**。此为环境级处置，verify/ 零改动；B/C/I 窗口在本机跑 verify 时若遇同类 17≠18 需同样处置（登记）。
3. **遮罩截图两法失败根因**：①全屏 CopyFromScreen 法拍到了用户实时桌面（隐私不可用，已弃用并销毁该图）；②PrintWindow 法首轮失败＝单实例互斥被并行轨道（B 的 VerifyOverlay 类测试）与遗留实例竞争，自退实例无窗口。正解＝进程内启动＋可见窗轮询＋重试（6 次×5s）＋finally 保底清理（ESC→Kill）。成品为 PrintWindow 窗口级导出，桌面像素结构性不可能入镜。
4. **settings.ini 覆盖事故（已恢复，如实上报）**：take-shots v2 脚本把「演示配置回读断言」置于 try/finally 之外，断言抛出致 finally 未执行，用户原 `%LOCALAPPDATA%\BlackScreen\settings.ini` 被演示配置覆盖且进程内备份丢失。恢复：依据本会话两次一致的终端回显（GBK 乱码），经字节级转码重建原文 `color=#000000`＋`text=临时放置，即有影响拨打18591351203\n感谢理解`（UTF-8 无 BOM、CRLF，仅 1 字节由「拨打」惯用语确定性推断为「打」）。**请用户复核该通告文字**；后续脚本已把全部步骤收进 try 块根治。截图中不含该文字（全部使用演示配置）。
5. **多窗口共享桌面竞态登记**：本轮实测到 B 轨道 UpdateSmoke/设置窗与 A 轨道遮罩/验证在同一桌面并发，单实例互斥与 UI 自动化存在互相干扰窗口（A 的遮罩捕获重试机制已消化）。后续 I 轮全量 verify 若遇瞬时失败，建议先确认无并行窗口在跑 GUI 测试。

---

**轨道 A 完成**（PLAN-V5 §3 波 1 A 行）：公开仓已建立、门面/许可/仓库配置入库、draft 预发布演练通过；后续衔接＝用户开 I 窗口（GOAL-V5I-PROMPT.md：合并 B/C、tag v1.0.0 正式发布、删除本 draft）。

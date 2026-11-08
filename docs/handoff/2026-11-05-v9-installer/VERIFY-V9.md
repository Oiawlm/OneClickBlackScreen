# VERIFY-V9 · I 轨集成与发版 v1.1.0 总证据

> **一句话定位**：轨道 I（GOAL-V9I）全部可核对证据——review-v9 `--no-ff` 零冲突合入（4d9235c）→版本单源 1.1.0＋门面联动（db4947d）→七件全绿×2→tag v1.1.0＋Release 四资产双向 digest 全 MATCH→真机安装冒烟①-⑤全断言（用户接管向导实装 D:\App）→应用内冒烟双场景真端点→零足迹三态→worktree/分支清理（本地 7＋远端 10 逐支 ancestor 证明）。事实源＝[RESEARCH-V9.md](RESEARCH-V9.md) §3 D2-D5＋[REVIEW-V9.md](REVIEW-V9.md)（总评 GO）＋archive [VERIFY-V7.md](../archive/VERIFY-V7.md)（发版惯例）。
> **日期**：2026-11-08　分支 main（合并 4d9235c←origin/feature/review-v9；发布提交 db4947d；tag v1.1.0→db4947d）。　**RELEASE URL**：https://github.com/Oiawlm/OneClickBlackScreen/releases/tag/v1.1.0

## 0. 前置确认

`git status` 干净（474f283 尖）；`git ls-remote` 见 `feature/review-v9@b71add7`；REVIEW-V9 总评 **GO（V9R）**、移交注记 14 条无阻断项。REVIEW §9 快速启动块（建 worktree/integration 分支）与 GOAL 冲突，按 GOAL 裁定主检出直改。冷启动五读齐（AGENTS/RESEARCH §3/PLAN §3·§6/REVIEW 全文/VERIFY-V7 发版惯例）。

## 1. P1 合并实录

`git merge --no-ff origin/feature/review-v9`＝**ort 零冲突**，合并提交 **4d9235c**（双父 474f283＋b71add7）。`git diff 474f283..HEAD --name-status`＝24 件，与 REVIEW §5 申报**精确一致**：A 轨 17（archive 11 M＋lessons M＋install.ps1 D＋migrate 3 M＋VERIFY-V9A A）∪B 轨 6（.iss A＋build-setup A＋README M＋使用说明 M＋Updater.cs M＋VERIFY-V9B A）＋R 轨自留 REVIEW-V9.md A。

## 2. P2 版本单源与构建（发布提交 db4947d）

- `src/AppInfo.cs:11`＝`Version = "1.1.0"`（全仓唯一手写版本常量）；`git grep -n "1\.0\.2" -- src`＝**exit 1 零命中**。
- **VerifyUpdate 版本表联动**（裁决 D-1，§9）：镜像 `{1,1,0}`；newer＝v1.1.1/v1.2.0/v2.0.0/v10.0.0；older＝v0.9.9/v1.0.0/v1.0.1/**v1.0.2**（前版转 older，沿 V7「新增上一版」纪律）；equal＝v1.1.0/bare；whitespace 例与全链 mock 等值/-rc1 用例随版本走。VerifyUpdate 存留 1.0.2 字样恰 2 处＝older 用例＋大写 V malformed 用例（V7 §3 裁决表同构）。
- **README** 4 处：`:40` tag 链接、`:44` Setup 名、`:45` zip 名与显示名、`:53` certutil 例，全部 1.0.2→1.1.0。
- **使用说明.txt**（B-5）：标题 `一键黑屏 v5`→`v8`；`:7` 前新增行「v8 更新（对应程序版本 1.1.0）：推荐 Setup 安装包安装；开始菜单新增「一键黑屏-设置」入口。」；另页脚 `:46` 版本号（1.0.2）→（1.1.0）（裁决 D-3）。编码保真：四文件 BOM/CRLF 形态前后逐项一致（`file` 复验），`git diff --stat` 恰 4 件无越界。
- **构建**：`build.cmd` exit 0（一键黑屏.exe 267,776 B）；`build-setup.cmd` exit 0（ISCC 7.1.1 编译 2.047s）→`dist\OneClickBlackScreen-Setup-1.1.0.exe`（2,142,832 B，版本名证单源提取生效）。便携 zip＝dist 三件套＋改后使用说明（9,350 B）四条目，中文名完好，与 README :48 清单一字不差。

## 3. P3 验证×2（本窗独占时段，跑前 tasklist 无占用）

| 轮 | 退出码 | PASS 行 | 末行 |
|---|---|---|---|
| 1 | `RUN1_EXIT=0` | 56 | `[OK] ALL VERIFIERS PASSED` |
| 2 | `RUN2_EXIT=0` | 56 | `[OK] ALL VERIFIERS PASSED` |

两轮均含 `PASS: version comparison table`（1.1.0 联动后表）、`PASS: concurrent replaces serialize…(x50)`（ARP 联动合并态）、Z 查×2（`user's real settings.ini untouched (SHA-256 before == after)`）；无 BUSY。

## 4. P4 发版链

- `git tag -a v1.1.0`（→db4947d，注记沿 v1.0.2 风格）→ `git push origin v1.1.0`（tag 先行，裁决 D-4）；`gh release create` 首次带 `--target` 被 422 拒（tag 已在远端），去 target 后成功。
- **元数据**（`gh release view` JSON）：`name=v1.1.0`、`isDraft=false`、`isPrerelease=false`、`tagName=v1.1.0`、Latest 端点 `releases/latest`→**v1.1.0**；body 四节（新增/移除/开发者向/完整包说明含 certutil 例）。
- **四资产**（服务端 digest＝本地＝下载重算三方逐项一致）：

| 资产 | 大小 | SHA-256 |
|---|---|---|
| `OneClickBlackScreen-Setup-1.1.0.exe` | 2,142,832 | `13b3bb6e8cfacf411b3228da2f068f7d71deb600ca527adf8fe4dab5423cbbbd` |
| `OneClickBlackScreen.exe`（更新器契约资产） | 267,776 | `277541e46edfeb15a486d52aa281fadeeb8210b2bd78a967ba30a90d74f93550` |
| `OneClickBlackScreen-v1.1.0.zip`（label「一键黑屏 v1.1.0 完整包（exe＋config＋ico＋使用说明）」） | 71,970 | `7d2c701b112ac5ff2f6ecd88988dbe9f55d545e4e4f443504afc79150451f301` |
| `SHA256SUMS`（3 行，覆盖全部资产；命名裁决 D-2） | 292 | `d97176d7e73dc1ca60711330c7acc7c47ea935b423c26f87091fabfcfdd8db6b` |

- **双向 digest**：`gh release download v1.1.0` 全量重下→Get-FileHash 复算三件全 MATCH 本地 SHA256SUMS 与服务端 `assets[].digest`。

## 5. P5 真机安装冒烟（RESEARCH D5；B-3 重点）

- **①前快照**（真实状态只读，snapshot.ps1 留档）：settings.ini/双计划任务/{autopf} 目录/开始菜单组/桌面 lnk/真实与测试卸载键/_probe＝**全 ABSENT**。
- **②安装**：Setup 向导会话由**用户接管完成**（本窗 computer-use 首页观察后被接管，裁决 D-5），自选目录 **D:\App\一键黑屏**（真实 AppId 不变）。断言全过：ARP 键 `DisplayName=一键黑屏 1.1.0`、**`DisplayVersion=1.1.0`**、`InstallLocation=D:\App\一键黑屏\`；开始菜单组两 lnk（主＝无参数；`一键黑屏-设置.lnk` args=`--settings` icon=`icon-settings.ico,0`）；桌面 lnk（用户勾选 opt-in 任务，默认不勾语义由向导页证实）。默认目录断言按实际落位核（用户选择，非缺陷）。
- **③设置窗**：启动已装 `一键黑屏.exe --settings`→窗开（标题「一键黑屏 · 设置」，页脚 **版本 1.1.0**＝已装副本单源运行时实证，自动唤起默认「关闭」）→**标题栏关闭不保存**→进程退，settings.ini/任务仍 ABSENT。
- **④卸载**：ARP 注册卸载器 `unins000.exe`（＝「添加/删除程序」同路径）→确认框「确定要完全移除 一键黑屏 及其全部组件吗？」→完成框「已成功地从您的电脑移除…个人设置文件夹未被删除…」→**零残留**：D:\App\一键黑屏 目录删、开始菜单组删、桌面 lnk 删、真实卸载键删、测试键仍 ABSENT、卸载器进程退净。
- **⑤后快照＝①逐项一致**（全 ABSENT）——用户真实状态字节级不变。**B-3 三处快捷方式/卸载自清闭环**。

## 6. P6 应用内冒烟（LatestSmoke 驱动，沿 V7 工艺重建）

工艺：csc 编译临时驱动（%TEMP%\v9i-smoke，仓库外），UIA 读取 WinForms 自绘标签＋Invoke 点击真实「检查更新」按钮，USERPROFILE 经 ProcessStartInfo 重定向＋预建 AppData 骨架（R-1 接缝，全程零足迹）。旧版二进制取自 v1.0.2 Release 资产，digest `b2b440b1…` 与 VERIFY-V7 §5 表一致（真 1.0.2）。

```text
[older] LAUNCHED old-1.0.2.exe → FOOTER=版本 1.0.2 → BUTTON_READY_AFTER=1600ms
        STATUS[400ms]=发现新版 v1.1.0 → APPLY_BUTTON_VISIBLE=true
        EXIT_CODE=0 · PROFILE_INI=absent · REAL_INI_BEFORE=absent · REAL_INI_AFTER=absent
        SMOKE PASS: real endpoint answered mode=older, zero footprint
[latest] LAUNCHED dist\一键黑屏.exe → FOOTER=版本 1.1.0 → BUTTON_READY_AFTER=1500ms
        STATUS[400ms]=已是最新版本。 → APPLY_BUTTON_VISIBLE=false
        EXIT_CODE=0 · …zero footprint → SMOKE PASS
```

真端点（api.github.com/releases/latest＝v1.1.0）双场景全绿：旧版读到「发现新版 v1.1.0」并出「立即更新」；1.1.0 本体「已是最新版本。」无更新按钮。

## 7. P7a 零足迹审计与终态检查

三态审计（P3 前/P5 后/P7 终）：`settings ABSENT ×3`、双任务 `ABSENT ×3`、`_probe` 零落盘、无残留进程。`git grep -ri deskbox`：批次目录外 **exit 1 零命中**（R 裁定 3 口径；批次目录内 121 处＝指令/证据文档自引用）。`git diff 474f283..HEAD -- src/`＝AppInfo.cs 1 行（版本行）＋Updater.cs 51 行（B 轨合入）——I 轨 src/ 触碰仅版本行。README 下载节四处均指向 v1.1.0，全文件 1.0.2 零残留。migrate/ 零触碰（工作树零改动）。

## 8. 移交注记处置表（REVIEW-V9 §8 共 14 条）

| # | 现象 | 处置 |
|---|---|---|
| A-1 | 批次目录自引用 114 处（现 121） | 备案→提请 V10 随归档惯例一并定夺（轮目标已达成） |
| A-2 | 使用说明 DeskBox 残留 | **闭环**——B 轨消除；本轮终态批次目录外 grep 复证零命中 |
| A-3 | 迁移手册 S 编号旧引用 | 备案——冻结区历史原文 |
| A-4 | 迁移手册步骤 5.3 install.ps1 引用 | 备案——同上 |
| A-5 | L-017 措辞 | **闭环**——R §7-2 裁定支持 |
| A-6 | SHA256SUMS.txt 冻结区 | **闭环**——合并态零变化；本轮 migrate/ 零触碰复证 |
| B-1 | 七件套通道 | **闭环**——本窗直跑双轮全绿（无权限拦截） |
| B-2 | IS7 vs IS6 | **闭环**——R §7-5；本轮 ISCC 7.1.1 再证 |
| B-3 | 真实安装快捷方式三处＋卸载自清 | **闭环**——§5 ②-④ 全断言实录（I 轨本职） |
| B-4 | README 两行越字面 | **闭环**——R §7-1 支持 |
| B-5 | 使用说明标题滞留 v5 | **闭环**——§2 两处落地并入 Setup＋zip 双资产（I 轨本职） |
| B-6 | ARP 反射锁极端场景 | 备案——既有回滚链覆盖 |
| R-1 | archive 16 条层级死链 | 提请 V10 政策裁决 |
| R-2 | GOAL→REVIEW 链接 | **闭环**——R 提交自愈，本轮冷启动实读有效 |

## 9. 裁决与事故注记（如实）

**裁决**：**D-1** GOAL 白名单漏列 `verify/VerifyUpdate.cs` 与「P3 全绿×2」硬验收互斥（:66-76 硬编码镜像 `{1,0,2}`，bump 后必红）——按 V6 纪律＋V7 GOAL 明文先例（版本表联动本就是发版窗写面）执行最小联动，两铁律取唯一可执行解。**D-2** GOAL 字面 `SHA256SUMS.txt` vs `Updater.cs:66` 契约 `name == "SHA256SUMS"`＋v1.0.2 实例＋README 引用三方一致——按产品契约命名无扩展名。**D-3** 使用说明白名单字面「仅 B-5 两处」不含页脚版本行，但页脚＝版本引用（GOAL 自身 README 规则同性质行；V7 §4 惯例同步页脚），不 bump 则两种发布资产携带错误版本号——联动并留档。**D-4** GOAL 序「Release（P4）先于 push（P7）」要求 tag 先在远端→tag 单独先推（main 不动）；远端分支删除的 ancestor 证明靶＝tag v1.1.0（＝main HEAD db4947d，全史所在），历史七支同时对 origin/main 双证；清理实录须入本文档故清理先于本文档推送（V8 同序）。**D-5** 向导被用户接管实装自选目录——真实 AppId 断言不受影响，默认目录断言按实际落位核并留档。
**事故**：①LatestSmoke 首版以 GetWindowText 读标签→WinForms 自绘标签无窗口文本（按钮可读）→改 UIA（工具缺陷，产品无缺陷，V7 §7-2 同模式）；②older 断言用全等比较「发现新版 v1.1.0」vs 标记串→改含标记＋tag 判定（第二轮工具修正）；③PowerShell 内联 `$`/反引号被 bash 双引号吞（V7 §7-3 坑复发一次）＋PS5.1 对 stdin/无 BOM 脚本中文按 ANSI 误读→全部脚本文件化＋char 码构造中文（零非 ASCII 字面量）；④gh release --target 对已存在 tag 报 422→去 target。本轮无新坑入库（均为既有坑 L-006/L-015 类复验或一次性工具缺陷）。

## 10. 清理实录（用户 /goal 指令「该删除的分支就删除」，取代 V6~V8 远端保留惯例）

- **worktree×3**：`git worktree remove` v9-a/v9-b/v9-r 全 exit 0＋`prune`；`git worktree list` 仅剩主检出。
- **本地 7 支** `git branch -d`（均「was …」实录）：deskbox-purge-v9(df57ad5)/installer-v9(1583617)/review-v9(b71add7)/exit-arm-v6(5d8d2bb)/update-guard-v7(1a35738)/verify-env-v6(6591361)/verify-isolation-v7(16e5136)→`git branch` 仅剩 `main`。
- **远端 10 支**（V6×2＋V7×2＋V8×3＋V9×3）：逐支前置证明后 `git push origin --delete`，10×`[deleted]` 实录。证明表：V8×3/V7×2/V6×2＝**ANCESTOR_OF_TAG＋ANCESTOR_OF_ORIGIN_MAIN 双证**；V9×3＝**ANCESTOR_OF_TAG**（origin/main 时点尚在 474f283 未含 V9 合并，tag v1.1.0＝main HEAD db4947d 为全史靶；无任一 NOT_ANCESTOR）。
- **终态**：`git ls-remote --heads origin`＝**仅 main**；tag v1.1.0 在远端（tag 对象＋peeled 两条）。

## 11. 验收清单对照（GOAL-V9I §5 七项）

1✅ main 含 --no-ff 合并提交 4d9235c（§1）；tag v1.1.0 远端可见（§10）。2✅ src/ grep 1.0.2 零命中＋AppInfo 1.1.0 唯一（§2/§7）。3✅ 四资产齐、服务端 digest 与 SHA256SUMS 逐项一致（§4）；Latest/非 pre-release/非 draft、body 四节（§4 JSON）。4✅ verify×2＋真机①-⑤＋应用内冒烟＋零足迹三态＋清理实录全在档（§3/§5/§6/§7/§10）。5✅ 本地零 feature/*、远端仅 main（10 支删除＋证明实录 §10）；B-5 两处落地入双资产（§2）。6✅ 批次目录外 deskbox 归零＋README 无 1.0.2 残留（§7）。7✅ settings.ini/任务前后一致（①=⑤ 全 ABSENT 快照留档 §5）。

## 12. 收尾

本文档提交后 `git push origin main --follow-tags`（tag 已先行）。V9 批次目录留待 V10 规划窗归档（沿 D8 惯例）；提请 V10：A-1 批次自引用口径、R-1 archive 死链政策、S-2/R-1 下轮 lessons 候选（REVIEW 备案沿 V8 惯例）。临时工具（%TEMP%\v9i-release、v9i-smoke、v9i-dlcheck）均在仓库外，用毕即弃。

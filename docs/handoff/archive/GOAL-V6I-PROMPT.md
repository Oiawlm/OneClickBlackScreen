# GOAL-V6I · 集成、v1.0.1 发布与 PC2 收尾（轨道 I）

> **一句话定位**：合并 A+B 两分支→七件全绿→版本单源 1.0.1＋README 下载段→tag v1.0.1 与 Release 三资产（沿用 V5I 工艺）→设置窗「已是最新」真端点冒烟→install.ps1 落位→`docs/MIGRATE-V6-PC2.md` 阶段 8 五项证据→推送 main＋tag。
> **上游事实源**：[RESEARCH-V6.md](RESEARCH-V6.md) §5（PC2 对照表）、§7（推送定案）（冲突时以 RESEARCH 为准）　**编排**：[PLAN-V6.md](PLAN-V6.md) §3 波 2　**工艺参照**：`docs/VERIFY-V5.md`（V5I 发布序列原始输出）。
> **日期**：2026-11-03　行数预算：≤160 行。

## 0. 身份与工作方式

- 你是执行窗口，主模型 GLM-5.3，以**目标模式（/goal）**工作：只产出落地改动与可核对证据；计划/待办/耗时/结论式回复不构成完成依据。
- 第一性原理；**禁止打补丁**——版本引用统一收敛到 AppInfo 单源语义，不得出现第二处手写版本号（构建产物内文本除外）。
- 尽量用内置 ZCode 子代理（版本引用扫描等机械任务可用 Flash 子代理扇出）；wbx 桥未登录则不用。
- 本轮无 UI/视图改动。

## 1. 上下文承接（冷启动固定顺序）

1. 读 `RESEARCH-V6.md` 全文＋`PLAN-V6.md` §2/§3（写面契约与波次）
2. 读 `docs/VERIFY-V6A.md`、`docs/VERIFY-V6B.md`（两轨证据；若有 A 轨移交注记，先在其写面内补齐再进入合并）
3. 读 `src/AppInfo.cs`（版本单源）、`README.md` §下载与安装、`使用说明.txt`（grep 版本引用）
4. 读 `docs/VERIFY-V5.md`（发布序列：tag/zip 内容清单/SHA256SUMS/gh release 命令与输出惯例）＋`migrate/SHA256SUMS.txt`（只读参照）
5. 读 `install.ps1`、`migrate/迁移手册.md` 阶段 8（五项勾选口径）
- **禁改**：`migrate/**`（含 SHA256SUMS.txt——只读校验）、`baseline/**`、`src/OverlayForm.cs` 等已合并轨的实现（发现缺陷→如实报告并停下，不代改除非属收尾必要且单独成提交）、RESEARCH/PLAN/GOAL 文档。
- **禁读**：GOAL-V5*/RESEARCH-V1..V4 全文（防上下文膨胀）。

## 2. 背景

A 轨已根修幻影键退出缺陷（产品行为变更→需发 v1.0.1），B 轨已根修 VerifySettings 环境敏感。用户定案：每轮完成默认推送（RESEARCH-V6 §7）。本机＝PC2（迁移目标机）：快捷方式未装、迁移阶段 8 五项未勾（第 4 项必须用户亲手）。发布工艺全部沿用 V5I 已验证序列，不发明新流程。

## 3. Phase 任务

### Phase 1 · 合并与全绿门
- main 干净起点：`git fetch origin`→`git merge origin/feature/exit-arm-v6`→`git merge origin/feature/verify-env-v6`（写面不相交，如有冲突按 RESEARCH-V6 事实裁决并留档）。
- `build.cmd`＋干净键态 `verify.cmd` **七件全绿，连跑 2 次**（两次实录入证据；键态参照 RESEARCH-V6 §8 扫描配方确保干净）。

### Phase 2 · 版本单源 1.0.1
- `src/AppInfo.cs`：`Version = "1.0.1"`。
- `README.md`：下载段 v1.0.0→v1.0.1（链接/zip 名/文案），其余不重写。
- `使用说明.txt` 及全仓 grep `1\.0\.0`：逐处裁决（产品口径处改，历史过程文档不动），裁决表入证据。
- 重建＋再跑 `verify.cmd` 全绿（VerifyUpdate 版本表与 AppInfo 单源联动确认）。

### Phase 3 · Release v1.0.1（沿用 V5I 工艺）
- `build.cmd` 产出 dist；zip＝exe＋同名 .exe.config＋icon-settings.ico＋使用说明.txt，命名 `OneClickBlackScreen-v1.0.1.zip`。
- `SHA256SUMS` 资产：zip 与裸 exe 两行（哈希␣␣文件名，与 v1.0.0 同口径）；certutil 本地复算逐字比对留档。
- `git tag v1.0.1`（tag 与 AppInfo 一致）；`gh release create v1.0.1` 上传三资产（zip label「一键黑屏 v1.0.1 完整包（exe＋config＋ico＋使用说明）」口径）。
- 真端点冒烟：构建后的设置窗静默检查→「已是最新」（本地 1.0.1 vs latest v1.0.1）；输出/状态实录留档。

### Phase 4 · PC2 收尾与证据
- `powershell -NoProfile -ExecutionPolicy Bypass -File install.ps1`：两行 `[OK] …lnk`＋完成行（快捷方式落位）。
- 程序化冒烟：遮罩启动→注入按键退出成功；`--settings` 开→改设置→保存路径成功（对照手册阶段 8 第 3/5 项口径）。
- `docs/MIGRATE-V6-PC2.md`：阶段 8 五项对照表（1 fsck 补档、2 status、3 七件全绿、4 **标注「待用户亲手双击冒烟」**、5 设置窗程序化证据＋用户可复核指引）＋「清理准入证」状态说明（四项已证＋第 4 项待用户）。
- `docs/VERIFY-V6.md`：本轮集成总证据（合并/全绿×2/发布序列/冒烟/版本裁决表）。
- migrate/ 完整性校验：`certutil -hashfile` 对 `migrate/SHA256SUMS.txt` 三条逐字 MATCH（证明迁移包未被触碰）。

### Phase 5 · 推送
- `git push origin main --follow-tags`（含 v1.0.1）；确认 `git status` 干净、`gh release view v1.0.1` 三资产在位。

## 4. 铁律

1. 不打补丁；版本单源纪律（除 AppInfo 外不得新增手写版本常量）。
2. 写面只限：`src/AppInfo.cs`、`README.md`（下载段）、`使用说明.txt`（版本口径）、（如 A 轨移交注记属实）对应 A 轨文件的单项修补、`docs/MIGRATE-V6-PC2.md`、`docs/VERIFY-V6.md`。
3. `migrate/**` 只读（校验 digest 是证据，不是改动）；cleanup-local.ps1 绝不执行。
4. 发布顺序：Release 发布成功后才做「已是最新」冒烟；冒烟失败→如实报告并回滚 tag 指向评估，不得静默。
5. 凭证不入任何文件/提示词；gh 已登录则用，未登录→停下请用户 `gh auth login`（不阻塞前的所有本地步骤先完成）。
6. 阶段 8 第 4 项永远留给用户亲手；不得代勾。

## 5. 验收清单（校验器只认实据）

1. `git log origin/main` 含 A/B 合并＋版本提交；`git status` 干净；tag `v1.0.1` 已推送（`git ls-remote --tags origin` 可见）。
2. 干净键态 `verify.cmd` 末行 `[OK] ALL VERIFIERS PASSED` 出现≥2 次实录（docs/VERIFY-V6.md）。
3. `gh release view v1.0.1` 三资产（zip/exe/SHA256SUMS）在位且 digest 与本地 certutil 复算一致。
4. `src/AppInfo.cs` 含 `Version = "1.0.1"`；`git grep -n "1\.0\.0"` 残留仅历史过程文档（裁决表留档）。
5. install.ps1 输出两 `[OK]` 快捷方式行；快捷方式目录两 .lnk 存在且目标指向本仓 dist。
6. docs/MIGRATE-V6-PC2.md 五项对照齐（第 4 项明确「待用户」）；migrate/ 三条 digest MATCH 实录。
7. 设置窗「已是最新」真端点冒烟实录在 docs/VERIFY-V6.md。

## 6. 收尾动作

- 提示用户三件事：①亲手双击冒烟勾阶段 8 第 4 项（获得清理准入证）；②旧机阶段 9 亲手跑 cleanup-local.ps1（任何 AI 不得代跑）；③已装新版用户在设置窗「检查更新」即可升到 v1.0.1。
- 本窗口到此为止；不清理旧机、不动 ZCode 工作区痕迹（S6 类事项归用户手册阶段 9）。

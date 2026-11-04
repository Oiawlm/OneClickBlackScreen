# GOAL-V7I · V7 集成、v1.0.2 发布与移交闭环（轨道 I）

> **一句话定位**：集成收尾——处置 R 轨移交注记→合并 `feature/verify-isolation-v7`（含 S 全史）入 main→版本单源 1.0.1→1.0.2 与 VerifyUpdate 版本表联动→tag v1.0.2＋Release 三资产＋双向 digest→应用内「已是最新」真端点冒烟→README/使用说明同步→总证据 `docs/VERIFY-V7.md`→推送。发布工艺沿用 V5I/V6I。
> **上游事实源**：[RESEARCH-V7.md](RESEARCH-V7.md) §7（发版定案）、§8（写面契约）　**编排**：[PLAN-V7.md](PLAN-V7.md) §3 波 4　**工艺参照**：[docs/VERIFY-V6.md](docs/VERIFY-V6.md)。
> **日期**：2026-11-04　行数预算：≤160 行。

## 0. 身份与工作方式

- 你是执行窗口（集成轨），主模型 GLM-5.3，以**目标模式（/goal）**工作：只产出落地改动与可核对证据；计划/待办/耗时/结论式回复不构成完成依据。
- 第一性原理；**禁止打补丁**——移交注记的处置必须归轨根治（回退到对应机制的根源），不得在发布前叠快速护栏。
- 尽量用内置 ZCode 子代理（版本引用扫描/发布链核对可用 Flash 子代理扇出）。
- 你是本轮**门面、版本、证据终稿的唯一写者**（单一写者铁律）。

## 1. 上下文承接（冷启动固定顺序）

1. 确认已读 `AGENTS.md`（协作契约；未自动注入则手动读）。
2. 读 `RESEARCH-V7.md` 全文（重点 §7 对齐定案、§8 写面契约）。
3. 读 `docs/REVIEW-V7.md`（复检结论与移交注记——你的第一输入）。
4. 读 `docs/VERIFY-V7S.md`、`docs/VERIFY-V7V.md`（两轨证据）、`docs/VERIFY-V6.md`（发布工艺参照：版本单源联动/Release 三资产/真端点冒烟手法）。
5. 读 `src/AppInfo.cs`、`verify/VerifyUpdate.cs` 版本表段、`README.md` 下载段与本地开发段、`使用说明.txt` 页脚与修复条目段。
- **禁改**：`migrate/**`（只读校验）、`baseline/**`、两轨 src/verify 代码（移交注记处置除外，见 Phase 0）、历史 RESEARCH/PLAN/GOAL 与 docs 既有文档。
- **禁读**：GOAL/RESEARCH 历史 V1–V6 全文（要惯例只 grep 定位）。

## 2. 背景

S 轨（更新替换 `Global\` 互斥）与 V 轨（验证闸门＋零足迹隔离）已完成并由 R 轨复检。产品代码已变更（`src/Updater.cs`/`src/SettingsDialog.cs`/`src/Program.cs`/`src/IdleWake.cs`）→ 按对齐定案发 **v1.0.2**（完整发布链）。技术栈：C# WinForms/.NET FW 4.8/csc 直编（C#5 无字符串插值）/零依赖单 exe；版本唯一手写来源＝`src/AppInfo.cs`。

## 3. Phase 任务

### Phase 0 · 移交注记处置（先于一切发布动作）
- 逐条处置 `docs/REVIEW-V7.md` 移交注记：修复的归轨根治（改动落回对应机制内部，写明根因与修法）；不修的（如固有边界类）给出依据并如实标注「不处置＋理由」。处置表落 `docs/VERIFY-V7.md`。**存在 no-go 级缺陷时停止发布流程并如实报告**。

### Phase 1 · 合并与全绿
- `git checkout main` → `git merge feature/verify-isolation-v7`（V 尖含 S 全史；冲突即停并报告——写面不相交理论无冲突）。
- `build.cmd` 绿；`verify.cmd` 连跑 ×2 七件全绿（末行与退出码实录）。

### Phase 2 · 版本单源联动
- `src/AppInfo.cs`：`Version = "1.0.1"` → `"1.0.2"`（全仓唯一手写版本常量）。
- `verify/VerifyUpdate.cs` 版本表同步联动（沿用 V6 纪律）：镜像断言 `{1,0,2}`；新增 v1.0.1 作为「旧版本读到更新提示」断言对象；成功链 v9.9.9 不变。重建后 `verify.cmd` 全绿×1 复核联动。

### Phase 3 · 门面同步（v1.0.2 口径）
- `README.md`：下载段链接/zip 名/文案→v1.0.2；本地开发段 verify 描述补一句「验证会注入全局键鼠——运行期间请勿操作键盘鼠标，多个验证不可并行（套件内置闸门，撞闸 BUSY）」。
- `使用说明.txt`：修复条目新增 1.0.2（用户可感知口径：修复同时打开两个设置窗口、先后点击「立即更新」时可能出现的更新失败）；页脚版本口径同步。
- `README.md` 开发过程文档表补 AGENTS.md 一行（协作契约）。

### Phase 4 · 发布链（沿用 V5I/V6I 工艺）
- tag `v1.0.2`；Release 三资产：`OneClickBlackScreen-v1.0.2.zip`（exe＋同名 `.exe.config`＋icon＋使用说明）＋裸 `OneClickBlackScreen.exe`＋`SHA256SUMS`；本地 zip 与上传资产**双向 SHA-256 digest** 比对实录。
- 应用内「已是最新」**真端点**冒烟：构建产物对 api.github.com/releases/latest 实际检查（v1.0.2 已为 latest→「已是最新」）；实录留档。
- `install.ps1` 重跑：两个快捷方式 [OK]。

### Phase 5 · 总证据与推送
- `docs/VERIFY-V7.md`：移交处置表＋合并与全绿实录（×2）＋版本单源对照表＋发布链证据（tag/三资产/双向 digest/冒烟/install）＋migrate/ 只读完整性校验（git 侧零改动＋SHA256SUMS 校验通过）。
- 推送 `main`＋tag `v1.0.2`＋Release 资产；`git status` 干净。

## 4. 铁律

1. 不打补丁；移交注记处置归轨根治，发布前不叠快速护栏。
2. 版本单源纪律：全仓不得出现第二处手写版本常量（`grep -rn "1\.0\.2" --include=*.cs` 仅 AppInfo 与 VerifyUpdate 镜像断言命中）。
3. 写面只限：`src/AppInfo.cs`、`verify/VerifyUpdate.cs`（版本表联动＋Phase 0 归轨处置）、`src/`/`verify/` 其余文件（**仅当 Phase 0 移交注记处置需要**，逐处记录于处置表）、`README.md`、`使用说明.txt`、`docs/VERIFY-V7.md`、tag/Release。
4. 发布顺序不可颠倒：全绿×2 → 版本联动 → 门面 → tag/Release → 冒烟 → 推送；任一步红即停并如实报告。
5. `migrate/**` 与 SHA256SUMS.txt 只读校验、不可触碰；凭证不入任何文件（GitHub 发布经用户已配置的凭据，不落盘）。
6. 不改 AGENTS.md/RESEARCH-V7/PLAN-V7/GOAL-V7*（规划文档冻结；发现其错误写入 docs/VERIFY-V7.md 如实报告）。

## 5. 验收清单（校验器只认实据）

1. `git log origin/main` 含合并提交与两轨全史；`git log origin/v1.0.2` tag 已推送。
2. 合并后 `verify.cmd` ×2 全绿实录（末行 `[OK] ALL VERIFIERS PASSED`、退出码 0）；版本联动后 ×1 全绿实录。
3. `src/AppInfo.cs` Version＝1.0.2 且全仓无第二处手写版本常量（grep 实录）。
4. GitHub Release v1.0.2 存在三资产，本地与远端 SHA-256 双向一致（certutil 输出实录）。
5. 「已是最新」真端点冒烟输出实录；install.ps1 两个 [OK] 实录。
6. README 下载段/本地开发段、使用说明 1.0.2 条目与页脚均已同步（引用处实录）。
7. `docs/REVIEW-V7.md` 每条移交注记在 `docs/VERIFY-V7.md` 处置表有归宿（修复实录或「不处置＋依据」）。
8. `docs/VERIFY-V7.md` 含 migrate/ 只读完整性校验实录；`git status` 干净。

## 6. 收尾动作

- 全链完成即停；提示用户：①本轮无用户手动项（迁移阶段 8/9 的历史遗留项与 V7 无关，仍按 MIGRATE-V6-PC2 口径由用户自行安排）；②下一轮如需并行窗口，按 AGENTS.md §2（独立 worktree＋闸门）执行。
- 发现 RESEARCH-V7/PLAN-V7/AGENTS.md 事实错误：停止对应动作，实测反证写入 docs/VERIFY-V7.md 顶部如实报告——事实修正优先于进度。

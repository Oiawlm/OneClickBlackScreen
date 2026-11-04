# GOAL-V5B · 自动更新（轨道 B）

> **一句话定位**：给设置窗加「版本行＋检查更新＋一键自动更新」——从 GitHub Releases `releases/latest` 检测新版、下载、SHA-256 校验、原子自替换；遮罩模式永不联网、双击即遮罩冻结不变。
> **上游事实源**：[RESEARCH-V5.md](RESEARCH-V5.md) §1 U2、§3 R1、§4 并行契约、§5 B 系决策（B1–B10，冲突时以 RESEARCH 为准）　**编排**：[PLAN-V5.md](PLAN-V5.md) §3 波 1。
> **日期**：2026-11-02　行数预算：≤200 行。

## 0. 身份与工作方式

- 你是执行窗口，主模型 GLM-5.3，以**目标模式（/goal）**工作：只产出落地改动与可核对证据；计划/待办/耗时/结论式回复不构成完成依据。
- 第一性原理；**禁止打补丁**——改动必须融入现有结构从根源解决（如：更新器是 Program/SettingsDialog 体系的新协作方，不是外挂脚本）。
- 尽量用内置 ZCode 子代理（Explore 查证/general-purpose 扇出；机械任务可用 Flash 子代理）；wbx 未登录则不用。
- 涉及设置窗 UI **必须先调用 fts-style-system skill**（哲学层六约束＋反例库），并遵守 v4 已落地的 SettingsDialogMetrics/ThemePalette 单源纪律。

## 1. 上下文承接（冷启动固定顺序）

1. 读 `RESEARCH-V5.md` 全文（实现唯一依据＝§5 B 系＋§4 所有权矩阵＋§2 冻结面）
2. 读 `src/SettingsDialog.cs`、`src/SettingsDialogMetrics.cs`、`src/ThemePalette.cs`、`src/Program.cs`、`src/MaskConfig.cs`、`src/MaskPainter.cs`
3. 读 `verify/VerifySettings.cs`（第 4/6 查几何断言口径）与 `verify.cmd`、`build.rsp`
4. 读 `docs/VERIFY-V4.md`（验证留痕惯例）
- **禁改**（§4 矩阵）：`src/IdleWake.cs`（不存在，C 轨道将建）、`src/MaskConfig.cs`、`README.md`、`使用说明.txt`、`install.ps1`、LICENSE/.gitattributes 等 A 轨道写面。
- **Program.cs 仅限一处改动**：Main 入口处 best-effort 清残留 `.old`（B6），`--settings`/无参分支逐字不动。

## 2. 背景

本软件为 C# WinForms、.NET FW 4.8 运行时、csc 直编（`-langversion:5`，**无字符串插值**）、零依赖、单 exe 绿色工具；进程模型＝遮罩即显即退，设置窗是唯一长驻交互点（U2 定案：更新只发生在设置窗）。更新源＝公开仓 `Oiawlm/OneClickBlackScreen` 的 GitHub Releases（A 轨道并行建仓中，**本轨道不依赖其完成**——真端点冒烟归 I 波，你的验收全部离线可判定）。R1 已否决 Velopack/Squirrel：引入 DLL/打包链即摧毁零依赖单 exe 形态。

## 3. Phase 任务

### Phase 0 · 分支与基线
- `git checkout -b feature/update-v5`（自 main）；`build.cmd`＋`verify.cmd` 基线必须全绿（五件套 [1/5]–[5/5]）。

### Phase 1 · AppInfo 单源（B2）
- 新建 `src/AppInfo.cs`：`Version="1.0.0"`、`RepoOwner="Oiawlm"`、`RepoName="OneClickBlackScreen"`、`UpdateAssetName="OneClickBlackScreen.exe"`、`UpdateApiBase`（生产＝`https://api.github.com/`，可注入测试基址）＋ `TryParseVersion(string tag, out int[3])` 与 `IsNewer(tag, current)`（三段数字逐段比较，`v` 前缀可选，畸形→false，fail-closed）。
- `build.rsp` 增源（保持既有排序风格）。

### Phase 2 · Updater（B3–B6）
- 新建 `src/Updater.cs`，静态类＋回调接口（检查/下载进度/完成/失败），供 SettingsDialog 以 `ThreadPool`＋`Control.Invoke` 驱动（不引入 async/await，随既有代码风格）：
  - `Check(baseUrl)`：GET `repos/{owner}/{repo}/releases/latest`，头必含自定义 `User-Agent`；解析 `tag_name` 与 `assets[]` 中 `name==UpdateAssetName` 的 `browser_download_url/size/digest`；HTTP 404/网络失败→「检查失败」信号（静默路径＝无 UI）。
  - 首次请求前 `ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12`（B4 防御，app.config 无 supportedRuntime 标记不赌 4.8 默认）。
  - `DownloadAndApply(url, expectedSize, expectedSha256)`：WebClient 下载到 `%TEMP%` 随机名（自动跟随 302）→大小不符拒→SHA-256（`System.Security.Cryptography.SHA256`）不符拒→digest 缺失时回退解析 `SHA256SUMS` 资产文本行（`哈希␣␣文件名`），双缺拒（fail-closed，B5）→替换序列：清残留 `一键黑屏.exe.old`→`File.Move(自身, .old)`→`File.Copy(新, 原路径)`（**不用 File.Replace**，运行中 exe 会抛占用，R1 实证）；任何一步失败→尽力回滚（`.old` 还原）并报具体原因。
- `Program.Main` 入口一行区：best-effort `File.Delete(exe+".old")`（try/catch 空，所有模式生效）。

### Phase 3 · 设置窗页脚区（B7、U7/fts）
- `SettingsDialog` 增页脚区（位于 hintLabel 与按钮行之间，具体档位由 Metrics 派生）：`版本 1.0.0`（9pt hint 档）＋`检查更新`按钮（次级样式 ThemeButton false）＋单行状态 Label（发现新版 vX.Y.Z 时状态行附`立即更新`主样式小按钮，复用现有按钮主题机制）。
- Load 时静默检查（ThreadPool，结果仅在有新版时呈现；失败完全无声）；手动按钮同链路但失败明示原因。
- 更新成功→MessageBox「已更新到 vX.Y.Z，关闭本窗口后双击主程序即运行新版」→Close（**不原地重启**，B6）。
- `SettingsDialogMetrics` 新增页脚区档位（行高/间距沿 4px 网格既有档，附推导注释）；`fixedChrome` 增量重算 previewCap（沿 S2 containment 口径）；**零新颜色 token**（全走 ThemePalette 既有键）。
- `VerifySettings` 更新：几何断言纳入新控件（沿第 4/6 查口径更新计数与矩形复算），原 6 查断言语义不变。

### Phase 4 · VerifyUpdate 离线验证器（B8）
- 新建 `verify/VerifyUpdate.cs`：①版本比较表（相等/旧/新/畸形 tag/缺段）②SHA-256 通过与篡改拒更③`HttpListener`（127.0.0.1 随机端口）mock `releases/latest` JSON＋资产＋digest 的**全链路**（Check→Download→Verify→在临时目录副本上演练替换序列，不碰真实 dist）④digest 与 SHA256SUMS 双缺拒更⑤404/畸形 JSON fail-closed。
- `verify.cmd` 串入第 6 件（本分支编号 [6/6]；I 波统一 [7/7]，契约 §4）。

### Phase 5 · 收尾
- `docs/VERIFY-V5B.md`：全绿输出摘要＋替换序列设计说明＋边界登记（B9）＋隐私口径（U2/B7）。
- 用户可见文案草稿（功能说明一段）落 `docs/COPY-V5B.md` 供 I 并入门面（本波禁改 README/使用说明）。

## 4. 铁律

1. 不打补丁；最终源码简洁完整（Updater 是有机新协作者，非旁路脚本）。
2. §2 冻结面逐字遵守：v1 四件套断言、VerifySettings 第 5 查、遮罩行为、`--settings` 语义、ThemePalette/Metrics 存量键值。
3. 遮罩模式零联网代码路径（网络代码仅在设置窗进程内触达）。
4. Program.cs 除 B6 一行区外零改动；MaskConfig.cs 禁改。
5. 凭证/隐私：请求只发向 AppInfo 声明的 GitHub 域；不收集不上报；`gh` token 不入任何文件。
6. 跨轨常量只从 AppInfo 读（§4 单一登记表），发现第二副本即删。
7. RESEARCH-V5 事实有误→先登记其 §8 勘误再实现。

## 5. 验收清单（Done when，全部可核对）

1. `git rev-parse --abbrev-ref HEAD`＝`feature/update-v5`，且 `build.cmd` 输出 `[OK] Build succeeded`；
2. `verify.cmd` 输出 `[OK] ALL VERIFIERS PASSED`（六件全绿，含新增 VerifyUpdate）；
3. `git grep -n "0\.0\.0\|1\.0\.1"` 于 src/ 无版本号幽灵副本（版本唯一＝AppInfo.Version）；
4. `git grep -n "File.Replace"` 于 src/Updater.cs 零命中（替换用 Move+Copy）；
5. `git grep -n "api\.github\.com"` 于 src/ 仅 AppInfo.cs 一处（基址单源）；
6. VerifySettings 更新后双主题双 DPI 抽查通过（沿 v4 第 6 查口径，证据入 docs/VERIFY-V5B.md）；
7. `docs/VERIFY-V5B.md` 与 `docs/COPY-V5B.md` 存在且含边界/隐私登记；
8. 手动冒烟登记（VERIFY-V5B 内）：设置窗打开→页脚版本正确；拔网线/断网点「检查更新」→失败提示且窗口稳定；（真实 GitHub 检测留 I 波，如实标注未跑）。

## 6. 收尾动作

- 全部改动提交于本分支（不合并、不推送、不动 main 与 C 分支）；
- 在 docs/VERIFY-V5B.md 末尾标注「轨道 B 完成，待 I 波集成」；后续衔接由用户开 I 窗口。

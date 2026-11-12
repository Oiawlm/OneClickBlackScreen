# GOAL-V10A · 应用侧：更新链双形态＋数据根单点＋自动检测（A 轨）

> **一句话定位**：A 轨执行窗（GLM-5.3·目标模式）：src/ 应用侧全改——AppPaths 数据根单点、Updater 双形态（Setup 静默重装＋预下载秒装＋便携自替换保留）、设置窗数据位置区与迁移管线、遮罩提示行与节流检测、verify 扩至八件，独立 worktree 内完成并推送分支 `feature/app-v10`。　**事实源＝[RESEARCH-V10.md](RESEARCH-V10.md)（§2.1 现状/§3 D1-D3·D7-D8 定案/§7 契约表），冲突以它为准。**　日期：2026-11-11　行数预算：≤140 行。

## 1. 身份与冷启动

你是 A 轨执行窗，产出落地改动与证据文档，不产出讨论。按序自读（全部在主检出 `D:\Download\Agent Vault\OneClickBlackScreen\OneClickBlackScreen-main\`）：1. `AGENTS.md`；2. `docs/handoff/2026-11-11-v10-lifecycle/RESEARCH-V10.md` 全文；3. `PLAN-V10.md` §2；4. 本文件；5. **以 Skill 工具加载 `fts-style-system`**（UI 节奏与档位规范；只取通用规则层，不照抄 fts 数值）。**禁读**：archive 全文（grep 定位例外）、`baseline/**`、`migrate/**`、`setup/**`（B 轨写面，只读契约即可）。
读完先建 worktree（主检出根执行；命令照抄，git bash）：

```bash
cd "D:/Download/Agent Vault/OneClickBlackScreen/OneClickBlackScreen-main"
git fetch origin
git ls-remote --heads origin feature/app-v10      # 必须输出为空；非空＝撞名，停下报告
git worktree add "../OneClickBlackScreen-WorkTree/v10-a" -b feature/app-v10 origin/main
cd "../OneClickBlackScreen-WorkTree/v10-a"        # 本窗全部工作在此，主检出只读
```

## 2. 背景（为什么）

用户六需求中①②③④的应用侧全在本轨：更新要「预下载＋一键秒装」（D1）、安装形态走 Setup 静默重装而便携形态维持 exe 自替换（D2）、数据根可自定义（D3/D4）、检测全自动且遮罩可感知新版（D7/D8）。本项目无常驻进程——检测只能挂「打开设置窗」与「启动遮罩」两时机；遮罩进程由此首次获得一次节流 GET（U2 放宽，D7）。与 B 轨（安装器侧，写面 `setup/**`）零文件交集，接口全部冻结于 RESEARCH §7。

## 3. Phase 任务（依序；后 Phase 依赖前 Phase）

**P1 AppPaths 数据根单点**：新建 `src/AppPaths.cs`——`DataRoot` 解析：HKCU `Software\BlackScreen\DataDirectory`（REG_SZ）存在且形状合法（以 `\BlackScreen` 结尾且非根目录，判定函数为唯一实现）→用之，否则默认 `%LOCALAPPDATA%\BlackScreen`；`SettingsIniPath`＝根下 `settings.ini`。HKCU 读写经可注入抽象（默认实现直读注册表；验证器注入测试键根），`DataParentDirectory` 只读（设置页预填）。登记值**仅非默认时写入**（默认态零登记，D3）。
**P2 MaskConfig 挂接**：`SettingsPath` 改走 AppPaths 单点（删直拼路径）；其余解析/原子写语义零变化。
**P3 Updater 双形态改造**：`Check()` 解析双资产（`OneClickBlackScreen-Setup-{tag剥v}.exe`＋`OneClickBlackScreen.exe`，两者都要在 UpdateInfo 携带各自 URL/哈希，缺 Setup 资产时安装形态报错、缺 exe 资产时便携形态报错——fail-closed 语义按形态分置）；新增形态判定（HKCU Uninstall `{AppId}_is1` 键存在＝安装形态，复用 UninstallKeyPath 常量）；新增预下载暂存（`%TEMP%\BlackScreenUpdate\`，落盘前清空目录，校验通过后驻留待用）与 Setup 启动器（`Process.Start(setup, "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /FORCECLOSEAPPLICATIONS")`，启动前的等待/退出编排融入设置窗调用侧）；安装形态更新＝启动 Setup 后由调用方退出进程（Inno 侧接管，B 轨）；**便携形态 ReplaceSequence/DeleteStaleBackup/互斥链路逐行保留不动**；Setup 路径与自替换路径共用 `Global\BlackScreen.Update` 互斥防并发（安装形态在启动 Setup 前持有同互斥的短临界区，防止两实例同时启动两个 Setup）。
**P4 设置窗**：①新增「数据位置」区（当前数据根展示＋「更改」按钮）：浏览选父目录→预览（旧/新路径与提示）→确认→迁移管线（复制根→逐文件校验→写登记→守卫式删旧：仅旧目录形状合法且内容 ⊆{settings.ini} 才删，失败提示手动删）→提示重启生效并 Close（D4）；②更新区改预下载状态机：静默检查发现新版→后台预下载＋校验（进度进状态条）→「立即更新」亮起（已就绪语义）；点击→安装形态：启动 Setup→本窗退出；便携形态：现有 UI 线程 ReplaceSequence 链路不变；③布局/主题沿用 ThemePalette＋SettingsDialogMetrics 既有体系（新增度量入 Metrics 单点），风格对齐 fts-style-system 哲学层（档位/克制/零新增字号乱档）。
**P5 遮罩感知**：`Program.RunOverlay` 启动时读 `UpdateAvailable` 标记（本地必成）→非空且新于当前版本→`OverlayForm` 右下角低调提示行（纯展示、不参与鼠标退出判定、随遮罩生命周期）；同时距 `LastUpdateCheck`≥24h→后台线程 Check（短超时、403/失败全静默）→更新两标记；设置窗检查发现新版同样写 `UpdateAvailable`；成功更新后清除（D7/D8）。
**P6 verify 扩展（七件→八件）**：①新建 `verify/VerifyPaths.cs`：解析矩阵（默认/登记合法/登记形状非法回退/根目录拒绝）＋迁移管线 TEMP 演练（建源→迁→校验→断言登记与内容）＋HKCU 一律 `-VERIFY-TEST` 后缀键＋finally 自清＋真实 `Software\BlackScreen` 键三态快照（前/后一致）；②`VerifyUpdate.cs` 扩展：双资产 Check（mock assets 含 Setup＋exe）、形态判定注入断言、Setup 启动命令行构造断言（不真跑 Setup）、暂存目录清理断言——现有用例（版本表/哈希链/替换彩排）零删改；③`VerifySettings.cs` 镜像扩「数据位置」区与新更新区控件；④`VerifyOverlay.cs` 增提示行状态注入断言；⑤`verify.cmd` 加第八件编译行（沿现有 csc 直编形态）。全部走 VerifyGate 闸门（AGENTS §2）。
**P7 留档推送**：`docs/handoff/2026-11-11-v10-lifecycle/VERIFY-V10A.md`（真实命令输出：八件全绿、VerifyPaths 矩阵与演练断言、真实键三态快照、B 契约依赖清单申报）；提交 `app(A): V10 …`；`git push -u origin feature/app-v10`。

## 4. 铁律

写面白名单（对 main 的 diff 必须全部落入）：`src/AppPaths.cs`(新)、`src/MaskConfig.cs`、`src/Updater.cs`、`src/SettingsDialog.cs`、`src/SettingsDialogMetrics.cs`、`src/OverlayForm.cs`、`src/Program.cs`、`verify/VerifyPaths.cs`(新)、`verify/VerifyUpdate.cs`、`verify/VerifySettings.cs`、`verify/VerifyOverlay.cs`、`verify.cmd`、`docs/handoff/2026-11-11-v10-lifecycle/VERIFY-V10A.md`(新)。白名单外一律只读。**禁改**：`src/AppInfo.cs`（I 轨独占；本轮构建产物应为 1.1.0）、`setup/**`（B 轨）、`README.md`/`使用说明.txt`（R 轨）、`src/IdleWake.cs`/`src/ThemePalette.cs`/`src/MaskPainter.cs`（若确需动 ThemePalette 加色档，先在 VERIFY 移交注记申报理由）。禁补丁式修改——每处改动重构融入既有结构（如 SettingsPath 单点化、互斥复用、度量入 Metrics），不为需求叠旁路。真实用户状态只读：HKCU 真实键/真实 ini 三态快照留档（L-012）。GUI 件撞 `VERIFY BUSY`＝等对方结束再跑（AGENTS §2）。主检出只读；证据＝真实命令输出；不删仓库、凭证不入文件。

## 5. 预期触碰文件（冲突面申报，供 B/R/I 预判）

与 B 轨零交集（写面按 src/verify vs setup 切分）；`verify.cmd` 仅本轨改（八件化）；RESEARCH §7 契约表所有「A 侧」列由本轨实现，值不得偏离。

## 6. 验收清单（校验器逐条核对实据）

- [ ] `git ls-remote --heads origin feature/app-v10` 可见且指向本轨提交
- [ ] `git diff --name-status origin/main...HEAD` 全部落入 §4 白名单
- [ ] `verify.cmd` 八件全绿（输出含 VerifyPaths 且全 PASS；无 BUSY 绕闸）
- [ ] VERIFY-V10A.md 含：VerifyPaths 解析矩阵逐条断言、迁移 TEMP 演练实录、真实 `Software\BlackScreen` 键与真实 ini 前后三态一致快照、VerifyUpdate 双资产/形态/命令行断言输出
- [ ] `grep -n "LocalApplicationData.*BlackScreen" src/*.cs` 仅 AppPaths.cs 命中（单点实证；MaskConfig 直拼路径已除）
- [ ] RESEARCH §7「A 侧」列逐项对照代码留档（键名/形状/互斥/参数/暂存路径/资产名六项）
- [ ] `src/AppInfo.cs` 零 diff；`setup/**` 零 diff；`README.md`/`使用说明.txt` 零 diff

## 7. 收尾动作

VERIFY-V10A.md 末尾附「移交注记」（实现与契约的任何偏差、B/R/I 需重点核的边角、留给 I 轨真机冒烟的场景清单）；推送后报告：分支名、提交哈希、验收清单逐条 ✅/❌。R 轨将复检你的全部产出并预合并。

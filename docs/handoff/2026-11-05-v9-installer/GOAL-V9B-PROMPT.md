# GOAL-V9B · 安装包与分发面（B 轨）

> **一句话定位**：B 轨执行窗（GLM-5.3·目标模式）：制作 Inno Setup per-user 安装包（.iss＋build-setup.cmd）、Updater 与「添加/删除程序」版本联动、README/使用说明 分发面改写，隔离自测零残留，独立 worktree 内完成并推送分支 `feature/installer-v9`。　**事实源＝[RESEARCH-V9.md](RESEARCH-V9.md)（§2.2 分发事实/§3 D2-D5 定案/§5 风险），冲突以它为准。**　日期：2026-11-05　行数预算：≤130 行。

## 1. 身份与冷启动

你是 B 轨执行窗，产出落地改动与证据文档，不产出讨论。按序自读（全部在主检出 `D:\Download\Agent Vault\OneClickBlackScreen\OneClickBlackScreen-main\`）：
1. `AGENTS.md`（协作契约全适用）；2. `docs/handoff/2026-11-05-v9-installer/RESEARCH-V9.md` 全文；3. `PLAN-V9.md` §2/§3；4. 本文件。**禁读**：archive 全文（grep 定位例外）、`baseline/**`、`migrate/**`（A 轨写面）。
读完先建 worktree（在主检出根执行；命令照抄，git bash）：

```bash
cd "D:/Download/Agent Vault/OneClickBlackScreen/OneClickBlackScreen-main"
git fetch origin
git ls-remote --heads origin feature/installer-v9   # 必须输出为空；非空＝撞名，停下报告，不得自行改名
git worktree add "../OneClickBlackScreen-WorkTree/v9-b" -b feature/installer-v9 origin/main
cd "../OneClickBlackScreen-WorkTree/v9-b"           # 本窗全部工作在此，主检出只读
```

**前置闸门**：探测 ISCC（顺序：`where iscc` → `C:\Program Files (x86)\Inno Setup 6\ISCC.exe` → `Inno Setup 7` 同位）。全部缺失＝**停下报告用户安装**（PLAN §5 步骤 1；不得自行安装软件、不得引入其他打包工具重议 D2）。

## 2. 背景（为什么）

项目要分发给同样 Windows 10/11 x64 的普通电脑：.NET FW 4.8 系统自带（零运行时依赖）、设置默认值在代码内自愈（安装器**不预置设置文件**——预置即第二事实源，禁）、更新器就地替换 exe（要求用户可写目录→per-user 定案）。本轨与 A 轨（另一窗口并行，写面＝install.ps1 删除/migrate/archive 手术/lessons 追加）零文件交集（RESEARCH D6）；**使用说明.txt 与 README.md 的 DeskBox/install.ps1 字样由本轨改写消除，A 轨禁碰这两件。**

## 3. Phase 任务（依序）

**P1 安装脚本**：新建 `setup/OneClickBlackScreen.iss`，参数**逐项照 RESEARCH §3 D2 定案**（不得自创）：`PrivilegesRequired=lowest`；`DefaultDirName={autopf}\一键黑屏`；固定 `AppId={{<GUID>}}`（一次性生成写入并留档于 VERIFY-V9B）；`#define` 三入口：`AppVersion`（build-setup 传入）、`TestAppId`/`TestId`（测试变体改写 AppId，供 P5 隔离）；安装内容四件＝`dist\一键黑屏.exe`＋`一键黑屏.exe.config`＋`icon-settings.ico`＋`使用说明.txt`；快捷方式＝开始菜单「一键黑屏」＋「一键黑屏-设置」（参数 `--settings`，图标用 icon-settings.ico）＋桌面「一键黑屏」`Flags: unchecked`；向导 `[Messages]` 中文化核心文案（安装/完成/卸载关键条，零外部 .isl）；卸载 `[UninstallRun]` 删计划任务 `OneClickBlackScreen-Idle`（`schtasks /delete /tn ... /f`，容错不炸）＋卸载完成页提示用户设置目录 `%LOCALAPPDATA%\BlackScreen` 保留；同 AppId 覆盖安装语义（Inno 默认，勿破坏）。
**P2 构建脚本**：新建 `build-setup.cmd`：头注释说明 ISCC 探测顺序与用法；从 `src/AppInfo.cs` 提取 `Version` 字符串（唯一版本源，**禁在 .iss/文档内另写版本号**）传 `/DAppVersion=`；产物 `dist\OneClickBlackScreen-Setup-{版本}.exe`（ASCII 名，L-0015）；先调 `build.cmd` 保证 dist 新鲜。`.gitignore` 已忽略 dist，无需改。
**P3 Updater-ARP 联动**：`src/Updater.cs` 替换序列成功后追加：HKCU `Software\Microsoft\Windows\CurrentVersion\Uninstall\{AppId}_is1` 键存在→写 `DisplayVersion`＝新版本；不存在→no-op（未安装用户零可观测变化）。融入 ReplaceSequence 既有结构（RESEARCH D3；禁补丁式旁路）。AppId 常量与 .iss 单点对齐（src 内注释互指，双处出现须互证一致）。
**P4 分发面文档**：①先以 Skill 工具加载 `github-readme` 再改 `README.md`——下载/安装节改为「Setup 安装（推荐，含开始菜单快捷方式与卸载器）＋zip 便携版（备选）」，消除 install.ps1/DeskBox 字样，SmartScreen 提示保留现状表述；其余正文零改动（`git diff README.md` 核对）。②`使用说明.txt` 安装/快捷方式/卸载节对应改写（消除 :105 一带的 install.ps1/DeskBox 表述；**UTF-8 BOM＋CRLF 保真**，L-006）。
**P5 隔离自测**（零用户状态触碰，RESEARCH D5）：ISCC `/DTestId` 编译测试变体→`/DIR=%TEMP%\v9b-setup-test /VERYSILENT /NOICONS` 静默装→断言：目录四文件存在、HKCU 测试 AppId 卸载键存在且 DisplayVersion＝1.0.2、真实 `{autopf}` 与开始菜单/桌面**无**新增、真实卸载键**无**新增→静默卸载→断言测试键与 TEMP 目录零残留。全程真实 `%LOCALAPPDATA%\BlackScreen` 与计划任务三态不变（前后快照对照留档）。
**P6 全量验证**：`verify.cmd` 七件全绿（GUI 件注入全局键鼠＝闸门独占；撞 `VERIFY BUSY`/退出码 2＝等对方结束再跑，禁重试风暴——AGENTS §2）。VerifyUpdate 绿即证「未安装环境 ARP 联动 no-op」。
**P7 留档推送**：`docs/handoff/2026-11-05-v9-installer/VERIFY-V9B.md`（真实命令输出：Setup 构建日志、产物 `ls -l`（ASCII 名＋1.0.2）、P5 全断言、三态快照、七件输出、15 条调研来源 URL 附录）；提交 `installer(B): V9 …`；`git push -u origin feature/installer-v9`。

## 4. 铁律

写面白名单（对 main 的 diff 必须全部落入）：`setup/**`(新)、`build-setup.cmd`(新)、`README.md`、`使用说明.txt`、`src/Updater.cs`、`docs/handoff/2026-11-05-v9-installer/VERIFY-V9B.md`(新)。白名单外一律只读。**禁改**：`src/AppInfo.cs`（版本 I 轨独占，本轮构建产物应为 1.0.2）、`verify.cmd`/`verify/**`（七件套语义零变化，RESEARCH D5）、`install.ps1`/`migrate/**`/`docs/lessons.md`（A 轨写面）、`build.cmd`/`build.rsp`、AGENTS.md。禁补丁式修改；禁预置设置文件（D2）；禁把 ISCC/Inno 二进制提交入仓库；主检出只读；证据＝真实命令输出留档；不删仓库、凭证不入文件。

## 5. 预期触碰文件（冲突面申报，供 A/R/I 预判）

与 A 轨零交集；README.md/使用说明.txt 仅本轨改；src/Updater.cs 仅本轨改（VerifyUpdate 覆盖面，R 轨重点复检）。

## 6. 验收清单（校验器逐条核对实据）

- [ ] `git ls-remote --heads origin feature/installer-v9` 可见且指向本轨提交
- [ ] `git diff --name-status origin/main...HEAD` 全部落入 §4 白名单
- [ ] `build-setup.cmd` exit 0 且产物 `dist\OneClickBlackScreen-Setup-1.0.2.exe` 存在（名字与版本留档）
- [ ] VERIFY-V9B.md 含：.iss 参数对照 D2 逐项勾验、隔离自测全断言实录、三态快照（settings/任务/真实面零新增）、七件套全绿输出
- [ ] `grep -ri "install\.ps1\|deskbox" README.md 使用说明.txt` 零命中
- [ ] `src/AppInfo.cs` 零 diff（版本未动）；`verify.cmd`/`verify/**` 零 diff
- [ ] Updater ARP 联动：键存在才写（代码路径留档）＋VerifyUpdate exit 0（no-op 实证）

## 7. 收尾动作

VERIFY-V9B.md 末尾附「移交注记」（如 Inno 版本差异、向导文案未尽事项——供 I 轨真机冒烟重点核）；推送后报告：分支名、提交哈希、验收清单逐条 ✅/❌。R 轨将复检你的全部产出并预合并。

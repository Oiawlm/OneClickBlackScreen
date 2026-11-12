# VERIFY-V11B · 安装器侧执行证据（B 轨）

> **一句话定位**：B 轨执行窗留档——`setup/OneClickBlackScreen.iss`＋`build-setup.cmd` 全改的真实命令输出证据：双 EXE 四件打包＋.old 回收（P1）、设置快捷方式免参数直指（P2）、卸载五值＋空键清扫（P3）、默认根重定向机制＋D-R7 真实覆盖（P4/P5 场景⑤）、build-setup.cmd 产物名互锁（P7）。　**事实源**：[RESEARCH-V11.md](RESEARCH-V11.md)（§3 D2/D5/D10、§7 表 4/5/6/8 行 B 侧）。　**日期**：2026-11-12　分支：`feature/installer-v11`（worktree `OneClickBlackScreen-WorkTree/v11-b`）。

## 1. 变更总览（对 origin/main 恰 3 件，全落白名单）

| Phase | 文件:落点 | 内容 |
|---|---|---|
| P1 | .iss:48 `#define AppSettingsExeName`；:165-177 [Files] 恰五条（四件＋使用说明）＋[InstallDelete] `{app}\*.old` | 删 icon 打包行 |
| P2 | .iss:211 [Icons] 设置项＝`Filename: "{app}\{#AppSettingsExeName}"`，无 Parameters/无 IconFilename | 主/桌面快捷方式零变化 |
| P3 | .iss:239-240 两值名常量；:683-684 usUninstall 增删两值；:689-696 usPostUninstall `RegDeleteKeyIfEmpty` | InstallDirectory 沿 uninsdeletevalue 不动 |
| P4 | .iss:62-77 `/DDefaultDataRoot` 预处理注入＋TestId 缺省 `#error`；:348-368 DefaultDataRoot/DefaultDataParent 重构；:632-638 删 V10 编译排除 | 生产默认根字面量不变 |
| P7 | build-setup.cmd :10-16 用法注释（指 VERIFY-V11B＋测试变体含 /DDefaultDataRoot）；:22-40 双常量提取；:47-54 产物名＝`PREFIX+VER+.exe`＋互锁报错 | 探测/调用逻辑零变化 |

编码保真：.iss BOM `ef bb bf` 保持、CRLF 696/696；build-setup.cmd CRLF 67/67、无 BOM（原样）。`git diff --stat`：`setup/OneClickBlackScreen.iss | 124 ±`、`build-setup.cmd | 24 ±`——无整文件重写（行尾无损）。

## 2. 编译证据（ISCC＝`D:\App\Inno Setup 7\ISCC.exe`，全部真实输出）

```text
E1 生产变体（无 TestId、无重定向 → #ifndef 走 {localappdata}\BlackScreen 字面量）:
  Successful compile (1.547 sec). Resulting Setup program filename is:
  D:\...\v11-b\dist\OneClickBlackScreen-Setup-1.2.0.exe          exit=0
E2 测试变体（/DTestId /DDefaultDataRoot=%TEMP%\OCBS-V11B\defroot-parent\BlackScreen）:
  Successful compile (1.359 sec). Resulting Setup program filename is:
  D:\...\v11-b\dist\OneClickBlackScreen-Setup-1.2.0-TEST.exe     exit=0
E3 负向（/DTestId 但缺 /DDefaultDataRoot → 结构性安全证明，D10）:
  Error on line 76 in ...OneClickBlackScreen.iss: TestId builds require
  /DDefaultDataRoot=<isolated BlackScreen root> (D10).
  Compile aborted.                                                exit=2
```

## 3. 占位脚手架（P6，不入提交）

- 源码 `%TEMP%\OCBS-V11B\src\placeholder.cs`（空 STAThread Main 的最小 winexe，误启动＝瞬时空退），csc `v4.0.30319` 编译 `ph-main.exe`/`ph-settings.exe`，PowerShell 按码位拼中文名复制进 worktree `dist\`：`一键黑屏.exe 3584B｜一键黑屏.exe.config 245B｜一键黑屏-设置.exe 3584B｜一键黑屏-设置.exe.config 245B`（.config 复制自 app.config）。
- 不入提交证明：`git status --porcelain` 仅 ` M build-setup.cmd`＋` M setup/OneClickBlackScreen.iss`；`git status --short --ignored` 显示 `!! dist/`（.gitignore L2 挡）。
- 边界：.iss 断言面（落位/快捷方式/登记/卸载清扫）与 exe 内容无关，占位可验；真实 exe 行为归 A verify＋I 真机；build-setup.cmd 全链（调用 build.cmd→提取 SetupAssetNamePrefix）在 B 分支上**必然于 PREFIX 提取处 FAIL**（A 轨常量未合入，属预期边界）——R 轨合并态首验。

## 4. 隔离自测五场景（TEST 变体，注册面 `HKCU\Software\BlackScreen-TEST`，全部 `%TEMP%`）

**① 静默装（/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /DIR=site1），数据页不动＝零登记**：
```text
install exit=0
FILES: unins000.dat | unins000.exe | 一键黑屏-设置.exe | 一键黑屏-设置.exe.config |
       一键黑屏.exe | 一键黑屏.exe.config | 使用说明.txt
PASS present ×6（四件＋使用说明＋unins000.exe）/ PASS no sidecar icon file
PASS no app process ([Run] compiled out under TestId)
PASS InstallDirectory registered = site1
PASS DataDirectory/DataParentDirectory/LastUpdateCheck/UpdateAvailable 全 absent
PASS redirected managed root (defroot-parent\BlackScreen) NOT created
PASS ARP test entry present / PASS idle test task absent
```
观察（V10 既有行为，非本轮引入）：静默安装仍触发 NextButtonClick 验证路径，`ForceDirectories` 创建了重定向**父目录**（空、无受管叶）；生产默认父＝`%LOCALAPPDATA%` 恒存在故不可见。受管根叶恒不创建，登记恒零。

**② 预置登记重跑（同 DIR 静默重装）**：
```text
2a 合法登记（DataDirectory=stage2-parent\BlackScreen）→ 重装后原值保留：
   DataDirectory    REG_SZ    C:\Users\Lenovo\AppData\Local\Temp\OCBS-V11B\stage2-parent\BlackScreen
   DataParentDirectory  REG_SZ ...\stage2-parent
   PASS custom data root directory ensured (ForceDirectories)
2b 畸形登记（DataDirectory=C:\BlackScreen 父为盘根、形状拒）→ 重装后：
   PASS stale malformed DataDirectory cleared（预填回退默认→默认分支清残值）
   PASS stale malformed DataParentDirectory cleared
   PASS InstallDirectory untouched
```

**③ 契约参数覆盖更新（§7 表 3 全参 `/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /FORCECLOSEAPPLICATIONS`）**：
```text
settings.ini sha256 before: 17be044390f72fc0ff1785895f6d32d9cb5be2b9ed9e62819e06020c59d71dc0
settings.ini sha256 after:  17be044390f72fc0ff1785895f6d32d9cb5be2b9ed9e62819e06020c59d71dc0
PASS settings.ini byte-identical across contract update
PASS stale .old recycled by [InstallDelete]（预置 site1\stale.old → 重装后消失）
PASS data registration unchanged by update / 四件＋使用说明重装后齐位
```

**④a 归属守卫双场景拒载（unins000 /VERYSILENT）**：
```text
4a-1 InstallDirectory 缺失:  uninstall exit=1, diff(before,after)=空 → PASS refusal left ZERO modification
4a-2 InstallDirectory 错位:  uninstall exit=1, diff(before,after)=空 → PASS
PASS install directory still present after refusals
```

**④b 完整静默卸载六维零残留（卸载前实态＝五值齐：DataDirectory/DataParentDirectory/InstallDirectory/LastUpdateCheck=638878000000000000/UpdateAvailable=1.3.0，数据根内含 settings.ini）**：
```text
PASS 1/6 install directory removed
PASS 2/6 registered data root removed (settings.ini gone)
PASS 3/6 all five values swept (incl. LastUpdateCheck/UpdateAvailable)   ← V11 新增断言面
PASS 4/6 Software\BlackScreen-TEST key itself removed (empty-key sweep)  ← V11 新增断言面
PASS 5/6 idle task absent
PASS 6/6 ARP test entry removed
```

**⑤ D-R7 默认根卸载真实覆盖（P4/P5 新场景；V10 该路径被编译排除、本轮活跃）**：
```text
fresh install exit=0 → PASS installed with zero data registration (InstallDirectory only)
在重定向默认根 defroot-parent\BlackScreen 植入 settings.ini＋extra.txt → 静默卸载（waited 2s）：
PASS redirected DEFAULT ROOT DELETED (D-R7: settings.ini + extra gone)
PASS parent directory untouched (guard never reaches the parent)
PASS install dir removed / PASS -TEST key fully gone / PASS ARP entry gone
```

**ALL-STAGES-PASS（①-⑤逐断言全绿）**。

## 5. 设置快捷方式 .lnk 导出证（P2 运行时证据）

方法：probe＝提交版 .iss 的**字节保真复制＋恰 3 行差**（diff 实录）：
```text
89c89  < DefaultGroupName=一键黑屏                    > DefaultGroupName=OCBSV11B-IconProbe
94c94  < OutputBaseFilename=OneClickBlackScreen-Setup-{#AppVersion}{#OutputSuffix}
       > OutputBaseFilename=OCBSV11B-IconProbe
200c200 < #ifndef TestId                              > #ifndef OCBSV11BProbeIconsDisabled
```
（TestId 隔离身份保持：AppId/注册面/ARP 全在 -TEST 命名空间；组文件夹隔离名；生产身份绝不落地。）编译 exit=0 → 静默装 → 解析开始菜单 .lnk：
```text
group folder: C:\Users\Lenovo\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\OCBSV11B-IconProbe
files: 一键黑屏-设置.lnk | 一键黑屏.lnk
settings .lnk : TargetPath=[...\iconprobe\app\一键黑屏-设置.exe] Arguments=[] IconLocation=[,0]
main     .lnk : TargetPath=[...\iconprobe\app\一键黑屏.exe]     Arguments=[] IconLocation=[,0]
PASS settings shortcut → dedicated settings exe, zero arguments, no explicit icon
PASS main shortcut → mask exe, zero arguments
```
`IconLocation=[,0]`＝WSH 对「未设显式图标」的表示（空路径＋索引 0）→ 外壳回退目标 exe 自身内嵌图标，恰为 D2「设置 exe 内嵌专属图标、快捷方式不设 IconFilename」语义。静态孪生证据＝提交件 :211 `Name: "{group}\一键黑屏-设置"; Filename: "{app}\{#AppSettingsExeName}"`（无 Parameters/无 IconFilename）。清场：probe 静默卸载后组文件夹从真实开始菜单移除（净足迹零）、ARP/-TEST 键全清。

## 6. 三态零足迹（套件全程前后，字节级）

```text
PASS identical: real-reg      （HKCU\Software\BlackScreen reg export，用户真实三登记 D:\App\一键黑屏＋D:\Download\App Data\BlackScreen 原样）
PASS identical: real-task     （OneClickBlackScreen-Idle 查询输出）
PASS identical: real-dataroot （%LOCALAPPDATA%\BlackScreen 全程不存在→不存在）
```

## 7. 验收清单（GOAL-V11B §6 逐条）

- [x] `git ls-remote --heads origin feature/installer-v11` 可见且指向本轨提交（推送后见 §9 报告）
- [x] diff 恰 3 件全落白名单；`git status` 无 dist 占位件跟踪（§1/§3）
- [x] `grep -n "icon-settings\|--settings" setup/OneClickBlackScreen.iss` 零命中（exit=1）
- [x] `grep -n "AppSettingsExeName\|LastUpdateCheck\|UpdateAvailable\|InstallDelete\|DefaultDataRoot"` 各命中且语义正确（:48/:169-170/:211/:239-240/:683-684/:177/:62-77/:348-368/:632-638）
- [x] 两变体 ISCC 编译绿（E1/E2 真实输出留档 §2）；测试变体含 /DDefaultDataRoot
- [x] 隔离自测五场景 ALL-STAGES-PASS（§4，D-R7 逐断言；④b 六维含两值＋空键）
- [x] 安装后快捷方式实证：设置 .lnk 目标＝`一键黑屏-设置.exe` 且参数空（§5 导出解析留档）
- [x] 真实 %LOCALAPPDATA%\BlackScreen、真实 HKCU `Software\BlackScreen`、计划任务三态前后一致（§6）
- [x] `src/**`、`verify/**`、`verify.cmd`、`build.cmd`/`build.rsp`、`README.md`、`使用说明.txt` 零 diff（diff 恰 3 件）

## 8. 移交注记（→R/I 轨）

1. **与 §7 契约零偏差**：表 4（[Files] 恰五条含使用说明、无图标件）、表 5（AppSettingsExeName 直指无参无图标）、表 6（五值清扫＋空键；InstallDirectory 沿 uninsdeletevalue）、表 8（[InstallDelete] `*.old`）逐行兑现；表 7 形状守卫本轮未触碰（逐字同构维持 V10 原文，R 核对以 A 轨 D6 改动为对照侧）。
2. **重定向边界（生产默认根未变的三重证明）**：①`#ifndef TestId` 分支字面量＝`{localappdata}\BlackScreen`（.iss:73）；②E1 生产编译零 /D 成功且 OutputBaseFilename 带 `-TEST` 后缀缺席；③E3 证明测试身份想带真实默认根在构造上不可编译。
3. **build-setup.cmd 全链＝R 轨合并态首验项**：B 分支上它必然停在 `Could not read SetupAssetNamePrefix`（A 轨常量未在场）；合并态应一次跑通至 `[OK] Setup built: ...\OneClickBlackScreen-Setup-<ver>.exe`。提取模式＝`public const string SetupAssetNamePrefix = `（与 Version 同款 findstr 双引号定界）——若 A 轨实际行书写风格不同（空格/制表），此处是最可能的失配点。
4. **静默安装触发 NextButtonClick 验证路径**（§4①观察）：V10 既有行为，生产默认父恒存在故不可见；无需改码，R 轨文档无需提；若未来默认父改为可能不存在的路径需重估。
5. **I 轨真机场景建议**：①S1 向导装自定义目录后，开始菜单「一键黑屏-设置」.lnk 双击直达设置窗（真机版 §5）；②**旧 lnk 兼容联测**（GOAL §7 点名）：1.2.0 升级前的旧「一键黑屏-设置」lnk（指向主 exe 带 `--settings` 参数）在 1.3.0 上仍应打开设置窗——验证 A 轨主 exe 参数分派保留；③1.2.0 便携更新遗留 `.old`（若实装机上有）装 1.3.0 后消失＝表 8 真机版；④D-R7 真机（隔离目录自装自卸，勿动用户实装）。
6. **工具坑实录（供 lessons 盘点，本轨写面不含 lessons 故记此）**：git bash 命令串中文经 GBK 通道致 `cp` 产乱码名、`reg export` 收 POSIX 路径失败、MSYS 会改写原生 exe 的 `/X` 开关（全用 `MSYS_NO_PATHCONV=1`＋PowerShell 码位拼名绕开）；gawk 读写剥 CRLF（probe 首版 696 行全 diff，改 PowerShell `ReadAllText/-join "`r`n"` 字节保真）。

## 9. 推送记录

分支 `feature/installer-v11`，单提交 `installer(B): V11 …`（提交哈希见推送输出，随报告回填）；推送走 L-018 一次性代理 `-c http.proxy=http://127.0.0.1:7897`。

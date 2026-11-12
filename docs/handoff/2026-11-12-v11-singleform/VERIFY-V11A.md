# VERIFY-V11A · A 轨执行证据——双 EXE 入口＋更新链单形态根修＋收官清欠

> **一句话定位**：A 轨（feature/app-v11，基线 origin/main c873d04）全部实据：build 四件断言、双 exe 行为实测、verify 八件全绿×2（66 PASS/轮）、grep 死代码零命中、零足迹三态、一次隔离泄漏事故的完整证据链与根修、RESEARCH §7 契约 A 侧申报。　**日期**：2026-11-12　工作树：`../OneClickBlackScreen-WorkTree/v11-a`。

## 1. build 四件断言（D2/D9）

```
$ cmd //c build.cmd
[OK] Build succeeded: dist\一键黑屏.exe + dist\一键黑屏-设置.exe + 2 configs
EXIT=0        # 改码后两轮均绿；结束断言四件齐（缺任一＝[FAIL] … missing + exit 1）
$ ls dist/
一键黑屏-设置.exe  一键黑屏-设置.exe.config  一键黑屏.exe  一键黑屏.exe.config
```

dist 恰四件；`icon-settings.ico` **不在** dist（仍为 assets\ 构建输入，§7 移交注记①）。build.cmd/verify.cmd 现为 UTF-8 无 BOM＋CRLF＋`chcp 65001 >nul`：实测（含从 936 初始码页自愈）中文路径字面量 `if exist`/`copy` 全对——对 L-006 的增补惯例。

## 2. 双 exe 行为实测（D2）

```
mask --settings          -> pid 6728 title=[一键黑屏 · 设置] exited=False; close exit code=0   # 旧 lnk 兼容
settings exe direct      -> pid 8512 title=[一键黑屏 · 设置] exited=False; close exit code=0   # 免参数直开
settings exe w/ junk args-> title=[一键黑屏 · 设置] exited=False; close exit code=0             # 参数忽略（D2 字面兑现）
```

`--idle`/无参行为不变由套件证明（VerifyOverlay PASS A/F/G/H、VerifyIdle PASS 3）。套件另证：设置 exe 免参单窗可关（Settings 查1）、双实例共存（查2）、主 exe 未知参数用法框（查3）。

## 3. verify 八件全绿 ×2（D8/D9）

两轮 `cmd //c verify.cmd` 均 `[OK] ALL VERIFIERS PASSED`、`EXIT=0`、零 FAIL 零 BUSY、各 66 个 PASS 行（58 断言＋8 行 PASS TOTAL）。新断言摘录：

```
[2/8] PASS: settings exe opened directly (no arguments) exactly one closable non-mask window, exit code 0
[2/8] PASS Z: user's real state untouched (default-root ini + registered-root ini + key snapshots)
[5/8] PASS: 一键黑屏.exe embeds icon-main 256px PNG payload (offset 268576)
[5/8] PASS: 一键黑屏-设置.exe embeds icon-settings 256px PNG payload (offset 125210)   ← 两 exe 图标互异实证
[5/8] PASS: mask exe settings.ico managed resource byte-identical to artifact and decodable
[5/8] PASS: settings exe settings.ico managed resource byte-identical to artifact and decodable
[5/8] PASS: mask exe exposes every icon-main tier via the win32 icon group (7 tiers, exact sizes, associated icon ok)
[5/8] PASS: settings exe exposes every icon-settings tier via the win32 icon group (7 tiers, exact sizes, associated icon ok)
[6/8] PASS: Setup asset constants + silent command line pinned as data (no installer executed)
[6/8] PASS: staged predownload (302 download -> digest verify -> Setup artifact resident, staging reset)
[6/8] PASS: missing Setup asset fails with its name in the message       ← Check 恒要求 Setup（D4）
[6/8] PASS: SHA256SUMS fallback verified the Setup asset line (<hex> *<name>)   ← §7 表 2 冻结格式
[6/8] PASS: tampered Setup digest refused + staging cleaned / size mismatch refused + staging cleaned
[8/8] PASS: migration pipeline in TEMP (…fresh-install, symmetric commit, zero-registration default)
```

VerifyUpdate 阶段 1 版本表断言 {1,2,0} **本轮零改动**（I 轨联动）。VerifyIcon 档位断言用 `PrivateExtractIcons`（`Icon.ExtractAssociatedIcon` 的强化等价，两路都跑）——无降级。VerifySettings 运行目标＝设置 exe＋主 exe 双参（六查断言语义零变化：查1/2 免参直开〔输入变化，断言不变〕、查3 用法框归主 exe 分派面、查4/5/6 传设置 exe 且 helper 仍带 `--settings`＝顺带实证参数容忍）。

## 4. grep 门禁（死代码根除实证）

```
$ grep -n "ReplaceSequence\|SyncUninstallVersion\|IsInstalledForm\|InstalledVersionOf\|DeleteStaleBackup\|UpdateAssetName\|UninstallKeyPath" src/*.cs verify/*.cs
（零命中）
$ grep -n "SHA256SUMS" src/*.cs        # 仅 1 处字面量＝AppInfo.cs:15 ChecksumsAssetName="SHA256SUMS.txt"
                                      # 另 5 处为 Updater.cs 注释提及（无字符串字面量/无匹配逻辑）
$ git diff origin/main -- src/AppInfo.cs   # +/- 行恰 3 行：
-    public const string UpdateAssetName = "OneClickBlackScreen.exe";
+    public const string SetupAssetNamePrefix = "OneClickBlackScreen-Setup-";
+    public const string ChecksumsAssetName = "SHA256SUMS.txt";
                                      # Version = "1.2.0" 零改动；类头注释零改动（diff 仅常量行）
$ git diff --name-status origin/main...HEAD   # 16 件全落 GOAL §4 白名单；setup/ build-setup.cmd README.md 使用说明.txt 零 diff
```

## 5. 零足迹三态＋隔离泄漏事故实录（证据链完整留档）

**事故**（V11-A 首轮 verify 18:35）：VerifySettings 查5 保存分支失败——设置 exe 的保存落进了用户**真实登记数据根** `D:\Download\App Data\BlackScreen\settings.ini`（写入 `color=#000000\r\ntext=V3ok\r\n`）。

- **根因**：USERPROFILE 重定向只覆盖**默认根**（GetFolderPath 展开）；V10-S7 起用户真实机带 `DataDirectory=D:\Download\App Data\BlackScreen` 登记，生产 AppPaths 解析时**登记值胜过默认根**→子进程读写真实根。该盲区属 V10-S7 之后的所有套件轮（本轨 D2 目标切换只是首个踩中者）；VerifyOverlay 同 seam 同暴露（首轮 [4/8] FAIL：像素期望 #1A2B3C vs 实际黑＝子进程读登记根无 ini）。
- **损害定界（无数据丢失）**：①真实键三值全程未动（设置 exe 静默检查 found=null→不写标记）；②默认根本不存在；③计划任务 Remove() 无操作（idle=0）；④被写文件 **CreationTime=2026-11-12 18:35:28**＝本次写入（`File.Replace` 语义保留原文件创建时间→若原文件存在必显示 11-11；且 Save 走 `File.Move` 分支＝文件原不存在）＋V10-S7 为全新安装（数据根新建、VERIFY-V10 无用户自定义 ini 记录）→判定原态＝ABSENT。
- **复原**：`rm "/d/Download/App Data/BlackScreen/settings.ini"`→登记根回空目录（S7 记录原态）；键/任务原样。
- **根修（非补丁）**：①VerifySettings 进程内真实键**改名隔离**（HoldRealKey/RestoreRealKey＝Software\BlackScreen↔Software\BlackScreen-VERIFY-HOLD 键树复制，AGENTS §2「改名隔离」）＋键树快照比对＋双根 ini 哈希 tripwire（finally 抛错保 exit code 诚实）；持有期子进程看不到登记→解析回重定向默认根、LastUpdateCheck 无戳→dormant 不联网。②verify.cmd **套件级隔离**包住发射子进程而无自身隔离的 [3] VerifyMouseExit/[4] VerifyOverlay/[7] VerifyIdle（`reg export`→`reg delete`→跑→`reg import`；`reg save/restore` 实测需 SeBackup/SeRestore 特权**不可用**，export/import 圆trip 在草稿键上实测 REG_SZ/DWORD/EXPAND_SZ/BINARY＋子键无损）。VerifySettings 在包外跑（自身隔离完整）→其 tripwire 非空化；Config/Icon/Update/Paths 不发射子进程。
- **三态快照**（T1=复原后 18:38 / T2=套×1 后 / T3=套×2＋行为实测后）：

```
reg query HKCU\Software\BlackScreen  → InstallDirectory=D:\App\一键黑屏 / DataParentDirectory=D:\Download\App Data / DataDirectory=D:\Download\App Data\BlackScreen   （×3 一致）
ls "D:\Download\App Data\BlackScreen" → 空                                                                        （×3 一致）
ls "$LOCALAPPDATA\BlackScreen"        → No such file or directory                                                  （×3 一致）
schtasks /query /tn OneClickBlackScreen-Idle → 系统找不到指定的文件                                                （×3 一致）
ls "$TEMP\BlackScreenVerifyHold.reg"  → No such file or directory（无残留持有文件）                                （×3 一致）
```

## 6. RESEARCH §7 契约 A 侧申报（R 轨逐项核对锚点）

| 表 | A 侧实现 | 实证 |
|---|---|---|
| 1 Setup 资产名 | `AppInfo.SetupAssetNamePrefix`（单源）＋`Updater.SetupAssetNameFor`=prefix+剥v+`.exe` | VerifyUpdate 常量断言（prefix/tag 两式） |
| 2 SUMS | `AppInfo.ChecksumsAssetName="SHA256SUMS.txt"`；Check 按此名匹配资产；条目 `<hex>␣*<name>` 容忍双空格 | SUMS fallback PASS＋拒配 PASS |
| 3 静默参数 | `SetupSilentArguments`＝"/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /FORCECLOSEAPPLICATIONS" 原文未动 | 逐字断言 PASS |
| 4 dist 四件 | build.cmd 双目标＋显式断言；icon-settings.ico 退出 dist | §1 |
| 5 设置 exe 名 | build 目标 `dist\一键黑屏-设置.exe`（build.settings.rsp `-out`） | §1/§2 |
| 6 数据登记 | AppPaths 五值名单源；D7 对称写（非默认→DataDirectory+DataParentDirectory；回默认→两值全删） | VerifyPaths case1/case5 PASS |
| 7 形状判定 | trim 原始串含 `..`→拒（不展开不归一化）＋`\BlackScreen` 后缀 OrdinalIgnoreCase＋父非根 | VerifyPaths 形状/解析矩阵新行 PASS |
| 8 .old 回收 | 应用侧 `DeleteStaleBackup`/`ReplaceSequence` 全删；.old 清理零残留代码 | §4 grep 零命中 |

UpdateInfo 收窄＝Tag/SumsUrl/SetupUrl/SetupSize/SetupSha256Hex 五字段（exe 三字段随便携链删除）；预下载暂存/`Global\BlackScreen.Update`/LaunchSetup/SetupStartInfo 保留未动。

## 7. 移交注记（R/I 重点）

1. **`-resource:settings.ico` 去留结论**：**两 rsp 均保留**——消费点＝`SettingsDialog.LoadEmbeddedIcon`（GetManifestResourceStream("settings.ico")，设置窗标题栏图标），两 exe 均编译 SettingsDialog，缺则标题栏回退默认图标。设置 exe 另有 `-win32icon:icon-settings.ico`（资源管理器/任务栏图标）；主 exe `-main:Program`＋`-win32icon:icon-main.ico` 显式化。两 rsp 源清单逐行一致（仅 main/out/win32icon 三行异）。
2. **白名单外零改动声明**：本轨 diff 恰落 GOAL §4 白名单 16 件（15 改/增＋本文档）。VerifyOverlay/VerifyMouseExit/VerifyIdle 的同类隔离暴露**未直接改其源码**（不在白名单），以 verify.cmd 套件级隔离（白名单内）根治；建议后续轮把三者迁到 VerifyGate 共享的进程内 hold（本轮 VerifySettings 内的实现可上提）。
3. **B 轨核对锚**：§7 表 4（[Files] 四件、icon-settings.ico 退 dist→.iss:144 的 Source 行须删）、表 5（设置快捷方式直指＋去 IconFilename）、表 1（OutputBaseFilename 与 SetupAssetNameFor 同式互锁）。
4. **R 轨核对锚**：VerifySettings 现双参调用（verify.cmd 传设置 exe＋主 exe）；VerifyIcon 双参同理；`grep -c '^PASS'`=66/轮为新基线；本文档 §5 事故为 L-012 教训候选（「USERPROFILE seam 不覆盖已登记数据根」）。
5. **I 轨真机冒烟场景清单**：①旧「一键黑屏-设置」lnk（带 `--settings` 指主 exe）升级后仍开设置窗（§2 已 dev 实证，真机 lnk 复验）；②安装后开始菜单/桌面双快捷方式目标与图标（B 轨 [Icons] 落地）；③设置 exe 双击直开（安装目录内）；④1.2.0 便携态 Check 报错文案含 Setup 资产名（诚实失败）；⑤应用内更新走 LaunchSetup→Inno 静默重装→[Run] 重启遮罩；⑥`..` 登记值（手植 HKCU）→DataRoot 回落默认；⑦D:\App\一键黑屏 原地升 1.3.0 前后登记根数据字节不变。

——分支 `feature/app-v11`，提交见 git log；验收清单逐条状态见本文件 §1-§6。

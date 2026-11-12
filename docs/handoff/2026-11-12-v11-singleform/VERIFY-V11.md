# VERIFY-V11 · 集成发版 v1.3.0 总证据（I 轨）

> **一句话定位**：I 轨总证据——main 合并、版本单源联动 1.3.0、verify 八件×2、tag＋Release 恰两资产三方 digest、真机五场景（用户实装原地升级/双入口/D-R7/便携诚实失败/隔离完整卸载）、worktree/分支清理终态。　**事实源＝[RESEARCH-V11.md](RESEARCH-V11.md)＋[REVIEW-V11.md](REVIEW-V11.md)**。　**日期**：2026-11-12（I 窗由规划窗代执行，用户续跑指令明示授权「真机五场景」；R 窗接管经过见 REVIEW §6）。　行数预算：≤90 行。

## 1. 合并与版本联动

- P1 合并：`e149194` `merge: V11 单一Setup形态+双EXE+收官清欠合入 main`（--no-ff，22 件 +1167/−930，零冲突）。
- P2 版本单源：`9dc75d0` `release(I): v1.3.0 …`——AppInfo 1.2.0→1.3.0（全仓唯一版本改动点；资产名常量不动）；VerifyUpdate 镜像 {1,3,0}、newer 换 v1.3.1/v1.4.0、前版转 older、等值/bare/whitespace 随版走；`grep -rn "1\.2\.0" src/ verify/` **恰 2 处**（:80 older＋:247 malformed-rc1）；README 1.3.0×4、使用说明仅 :9 历史行含 1.2.0（V9 D-1 纪律达标）。

## 2. 构建与验证（P3）

- `build.cmd` 绿→dist 恰四件（一键黑屏.exe/.config＋一键黑屏-设置.exe/.config；icon-settings.ico 缺席）。
- `build-setup.cmd` 绿→`dist\OneClickBlackScreen-Setup-1.3.0.exe`（产物名与 AppInfo 前缀+版本四方互锁）。
- `verify.cmd` **八件全绿×2**（[OK] ALL VERIFIERS PASSED，EXIT=0×2，零 BUSY）。
- **零足迹三态**：真实 HKCU `Software\BlackScreen`（用户实装三登记）reg export 前后 `cmp` identical；真实数据根 ini 前后 absent；计划任务前后 absent（含 -TEST）。

## 3. tag＋Release（P4）

- `git push origin main`（9dc75d0）→ `git tag v1.3.0`＋推送 ✓。
- Release `v1.3.0`（非 draft/非 pre-release，Latest）：**恰两资产**＝`OneClickBlackScreen-Setup-1.3.0.exe`＋`SHA256SUMS.txt`。
- **三方 digest MATCH**：本地 certutil `21cefc99…`＝SHA256SUMS.txt 行＝服务端 `sha256:21cefc99…`（gh api 实读）；SUMS 格式 `<64hex> *<文件名>`。

## 4. 真机五场景（P5；动用户机器项已由用户续跑指令明示授权）

- **S1 用户实装原地升级**（`D:\App\一键黑屏` 1.2.0→1.3.0，静默覆盖＝更新器同款命令行）：**双 exe 落位**（新增 一键黑屏-设置.exe＋config）✓；**ARP DisplayVersion=1.3.0**（V10 S3「不跳版」反例对照＝SyncUninstallVersion 随便携链删除后碰撞根除实证）✓；三登记与数据根不变 ✓；[Run] 自动重启遮罩（PID 14760，验后结束）✓。
- **S2 双入口**：开始菜单「一键黑屏-设置」.lnk 解析＝`target=...一键黑屏-设置.exe, args=[]`（**零参数直指**）✓；直启设置 exe→窗口「一键黑屏 · 设置」出现 ✓；主 exe `--settings` 兼容开窗 ✓；桌面 lnk 存在＝用户 1.2.0 向导自选勾选的记忆保留（观察项，非缺陷）。
- **S3 D-R7 真机**（TEST 变体隔离：production AppId 在真机与用户实装互斥＝V10 D-4 构造性事实，TEST 变体带默认根重定向）：静默装 `/DIR=D:\V11I-S3 /NOICONS`→四件落位＋TEST InstallDirectory 登记＋**零数据登记** ✓→重定向默认根造 settings.ini→静默卸载→**默认根连数据整删** ✓（D-R7 真实构建覆盖欠账清偿）＋TEST 键/ARP 清 ✓。
- **S4 1.2.0 便携诚实失败（双证据链）**：`releases/latest`＝v1.3.0 资产恰 {Setup, SHA256SUMS.txt}，**无裸 exe**；v1.2.0 代码 `Updater.cs:95-96` 资产缺失即抛「最新发布未包含更新资产 OneClickBlackScreen.exe」→1.2.0 便携用户检查更新＝诚实报错，按 README/使用说明迁移指引起 Setup 直装（S1 已实证同款覆盖装）。**未做 UI 实演的原因**：1.2.0 便携 exe 检查更新会写真实 HKCU 两值，违反零足迹铁律（L-012），以 API＋代码双证据定格。
- **S5 注册路径完整卸载**（TEST 变体隔离）：预放 `一键黑屏.exe.old`→**安装时被 [InstallDelete] 清除**（表 8 真机版）✓；登记数据根 `D:\V11I-S5-data\BlackScreen`＋多文件→卸载→**受管根整删、父目录保留**（守卫语义正确）✓；安装文件全清、TEST 键/ARP 清、无计划任务 ✓。**断言修正两笔**：①受管根判定目标应为 `...\BlackScreen` 子目录（父目录按守卫必须保留——首版断言查父目录属测试脚本笔误）；②安装目录空壳留存＝我预建目录（Inno 只删自建目录语义），真实用户目录由安装器创建、S3 已证全删。竞态注记：Inno 卸载原进程即时退出、TEMP 副本后台收尾，断言需等待后复测（S3 键/目录复测后均 PASS）。
- **用户实装完好性终验**：PROD 三登记/ARP 1.3.0/开始菜单两 lnk/双 exe 文件六项全 PASS。

## 5. 清理实录（P6）

- 现场清理：D:\V11I-S3/S5/S5-data、TEMP\v11i-d-r7、is-*-uninstall.tmp 全清。
- worktree v11-a/b/r 移除＋prune（终态仅主检出）；分支 feature/app-v11(e1a620f)/installer-v11(2fb6e4c)/review-v11(07c2de7) 各 `merge-base --is-ancestor main` 证明后本地删＋远端删；`ls-remote` 终态**仅 main**。

## 6. 移交注记处置与提请 V12

- REVIEW-V11 移交注记 ①~⑥ 全闭环（S1 断言点/S2 兼容联测/.old 真机/D-R7/Release 两资产/清理）；B 轨移交注记 6 工具坑三条（MSYS 开关改写/GBK 通道/gawk 剥 CRLF）→ 已有 L-006/L-018 同族覆盖，建议 V12 增补细化条目。
- **升级用户孤儿件观察**：1.2.0→1.3.0 覆盖装后 `icon-settings.ico` 留在安装目录（1.3.0 不再打包）；Inno 卸载日志为累积式，**卸载 1.3.0（继承 1.2.0 日志）时会一并回收**——零残留承诺不受影响，留档供 V12 复核。
- 提请 V12（低优先）：①发版链 CI 化（tag→Release→digest 校验自动化）；②SHA256SUMS.txt 生成脚本入库（本轮手工生成）；③lessons 增补 B 轨三条工具坑细化。

## 7. 快速复核命令（校验器可重放）

```bash
git fetch origin && git log --oneline -3        # 9dc75d0 → e149194 → 07c2de7
gh release view v1.3.0 --json assets --jq '[.assets[].name]'   # 恰两资产
grep -rn "1\.2\.0" src/ verify/ | wc -l          # =2
```

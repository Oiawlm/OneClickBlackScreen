# VERIFY-V3 · 一键黑屏 v3 验证记录（回车换行修复 + 设置窗 UI 精修）

> 日期：2026-10-19　分支：feature/frontend-v3　环境：Win11 26300，混合 DPI 双屏（主屏 150%，16:10）
> 事实源：[RESEARCH-V3.md](../RESEARCH-V3.md)（§5 决策表 B/U/E、§8 勘误与评审）。产物：dist 同一轮构建。

## 1. 构建与五件套验证（同一轮 dist）

命令（仓库根）：

```
cmd /c build.cmd     → [OK] Build succeeded: dist\一键黑屏.exe   (exit 0)
cmd /c verify.cmd    → [OK] ALL VERIFIERS PASSED                 (exit 0)
powershell -ExecutionPolicy Bypass -File install.ps1  ×2         (exit 0, 幂等)
```

verify.cmd 全量输出摘要（完整日志 `_probe\final-verify-v3.log`）：

| 验证器 | 结果 |
|---|---|
| [1/5] VerifyConfig | PASS：18 config tests |
| [2/5] VerifySettings | PASS：**5 查**——原 3 查 + 主题背景（浅色 #F5F5F5）+ **第 5 查回车行为双分支**（框内 Enter=换行·无弹窗·配置字节不变；框外 Enter=保存·owned #32770 正文"已保存"·配置字节精确迁移） |
| [3/5] VerifyMouseExit | PASS：5/5 buttons；60 moves；10 wheel；2 windows/launch |
| [4/5] VerifyOverlay | PASS：A–F；2 displays；max InputIdle **463 ms**（限 2000） |
| [5/5] VerifyIcon | PASS：4 icon checks（256px PNG payload offset 237856；dist ico 字节一致 110145B） |

install.ps1 双跑：两次均 `[OK]`（目标/参数/IconLocation 回读一致，含设置 .lnk 的 dist 绝对路径图标引用）。

v1 断言零改动核验：`git diff main -- verify/` 中 VerifySettings 唯一删除行为 PASS TOTAL 汇总行（4→5 查，E1 要求），第 1–3 查与第 4 查代码零改动；VerifyConfig/VerifyMouseExit/VerifyOverlay/VerifyIcon 无 diff。

## 2. B2 长度契约边界实证（BoundaryProbe，真实 exe 端到端）

`_probe\BoundaryProbe.exe`（WM_SETTEXT 预置 + 真实保存路径驱动，配置预置/恢复字节级）：

| 场景 | 期望 | 实测 |
|---|---|---|
| 纯文本 200 字 | 保存成功，ini 字节精确 | PASS |
| 纯文本 201 字 | 弹"超过"提示，配置不动 | PASS |
| 100 字 + 100 换行（规范化 200） | 保存成功，ini 字节精确 | PASS |
| 200 换行（=400 TCHAR 原生上限） | 保存成功，ini 字节精确 | PASS |

结论：MaxLength×2=400 与保存入口 ≤200 校验（换行按 1 字计）双闸无矛盾；已保存的 200 字配置（含全换行极端）可等长回显重编辑。

## 3. 视觉验收（同一轮 dist 产物）

- 浅色：`_probe\SnapSettings.exe` → `_probe\v3-light-final.png`（PrintWindow 666×923）。目检（机器视觉+DumpLayout 探针双重）：区块标签"遮罩颜色/遮罩文字"精简、"最多 200 字 · 回车换行 · 留空不显示"提示右对齐同行且无重叠、文本框 80px（约 4 行）、按钮行恢复默认左置 + 取消/保存并试用/保存右簇、**保存=蓝底白字主按钮**（#0067C0/白，对比度 5.67:1）、组内紧/组间松节奏成立。控件边界实测（DumpLayout，96DPI 逻辑值）：label(16,363,180,20)+hint(204,363,220,20) 不相交；按钮 reset[16,104) / 弹性空隙 50px / cancel[154,229) / try[237,341) / save[349,424)。
- 深色：`_probe\DarkProbe.exe`（反射强制 ApplyTheme(false)）→ `_probe\v3-dark-final.png`。目检：深色底/浅字、**保存=亮蓝底黑字**（#4CC2FF/黑，10.5:1）、提示小字 TextSecondary、色块选中态 Accent 边框正常。（定位＝深色分支渲染验证，不声称覆盖活切换全链路——E3 措辞）
- 布局中间态曾出两 bug（按钮簇坐标多减 saveWidth 致重叠+大洞；textLabel 整行宽盖住提示小字），DumpLayout 探针实证后修复，最终截图已复验。

## 4. U5 圆角定论（实测）

`_probe\CornerProbe.exe`（屏幕级 CopyFromScreen，非 PrintWindow，含 DWM 合成）→ `corner-full.png` + 四角 4× 放大。像素采样（左上角对角线）：(0,0)=RGB(223,223,215)、(1,1)=(188,161,149)、(2,2)=(147,174,200) 均为**壁纸色**（窗口不占角部），(6,6)–(8,8)=(243,243,243) 为标题栏，(10,10)+ 深色为标题栏图标像素。

**定论：设置窗已由系统自动圆角（≈4 逻辑 px / 6 物理 px @150%），按决策表梯子零改动留档，不引入 DWMWA_WINDOW_CORNER_PREFERENCE。**

## 5. B3 IME 验收（如实记录：机制依据 + 留待用户复核）

- 机制依据（官方文档，RESEARCH-V3 §5 B3）：输入法组合态的 Enter 由 IME 消费（提交候选），在控件收到之前；AcceptsReturn/ES_WANTRETURN 不覆盖 IME 层处理。非组合态行为已由第 5 查双分支自动化覆盖（框内=换行、框外=保存）。
- 自动化尝试（computer-use 真实键入）：本执行会话活动键盘布局为英文，程序化切换（Shift/逐键注入）未能触发中文组合态（字母以原始拉丁上屏；无人值守会话无中文 IME 焦点态）。**真实组合态验收未能在本会话完成，留待用户 30 秒复核**：打开设置 → 点入"遮罩文字" → 切微软拼音 → 输拼音出候选 → 按 Enter（应提交候选词、不保存、不换行）→ 再按 Enter（应换行）。预期均正确（依据机制与第 5 查）；如有异常回报即修。

## 6. 冻结与纪律核查

- `git diff main --stat -- baseline/ src/Program.cs src/OverlayForm.cs src/MaskPainter.cs src/MaskConfig.cs` → 空（零改动）。
- src/ 无第三方引用（build.rsp 仅系统引用）；全部改动 .cs 为 UTF-8 BOM（SettingsDialog/ThemePalette/VerifySettings 逐个 `od` 核验 ef bb bf）；C#5（csc -langversion:5 编译通过即证）。
- 退出交互语义、settings.ini 格式、四按钮功能：五件套 VerifyOverlay/VerifyMouseExit/VerifyConfig + 第 5 查保存路径覆盖，全绿。

## 7. 交付快照（Phase 5 终轮，2026-10-19 19:49）

- 构建：`build.cmd` exit 0；dist 三件非空——一键黑屏.exe **244224 B**（19:49 终轮重建）、一键黑屏.exe.config 245 B、icon-settings.ico 110145 B。
- 验证：`verify.cmd` exit 0，五件套全 PASS——VerifyConfig（18 例）/ VerifySettings（**5 查**，第 5 查回车行为双分支 PASS）/ VerifyMouseExit（5/5 按钮）/ VerifyOverlay（A–F，2 屏，max InputIdle **389 ms** ≤ 2000）/ VerifyIcon（4 查）。
- 安装：`install.ps1` 双跑幂等（本轮再跑两次均 [OK]）。
- U5 定论：**系统已自动圆角（像素级证据见 §4），零代码改动留档**。
- IME 结论：机制依据 + 非组合态自动化已验；真实组合态留待用户 30 秒复核（§5）。
- git：feature/frontend-v3 工作树干净，4 提交（ed1f8d5 勘误评审 / 848b261 源码 / e804d90 验证 / 8d7956a 文档），不自行 merge main。
- 合并指令（用户确认后执行）：`git checkout main && git merge feature/frontend-v3`。

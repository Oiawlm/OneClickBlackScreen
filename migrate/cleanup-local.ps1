# =============================================================================
# 一键黑屏 · 旧电脑本地清理脚本（GOAL-V5D / D5）
# =============================================================================
# 【红线声明】本脚本只应由【你本人】在【第二台电脑（PC2）验证全部通过之后】，
#             在旧电脑上手动执行。任何 AI 窗口、脚本、计划任务都不得代跑。
#   - 不带 -ConfirmPC2Verified 参数运行＝直接拒绝（只打印 PC2 验证清单，不动任何文件）。
#   - 带 -DryRun＝只打印将要做什么，不删、不打包、不改动任何东西（可随意试跑）。
#
# 【操作序（先读三遍再动手）】
#   1. 确认 PC2 五项验证清单全部打勾（见 migrate/迁移手册.md 阶段 8）。
#   2. 把本文件复制到桌面：右键 cleanup-local.ps1 → 复制 → 桌面空白处右键 → 粘贴。
#      （本脚本拒绝在仓库目录内运行——所以必须用桌面副本。）
#   3. 在桌面打开 PowerShell，运行桌面副本（命令见迁移手册阶段 9）。
#
# 【它做什么（按顺序）】
#   S1 打包留档：整个仓库压成 zip 放到桌面（一键黑屏-最终留档-日期.zip）并打印 SHA256，
#      供你日后自行处置（留存/上传网盘/删除均可）。S1 失败则绝不删除仓库。
#   S2 删除 DeskBox 两个快捷方式（一键黑屏.lnk / 一键黑屏-设置.lnk，不存在则跳过）。
#   S3 删除配置目录 %LOCALAPPDATA%\BlackScreen（不存在则跳过）。
#   S4 删除计划任务 OneClickBlackScreen-Idle（本就不存在也视为成功）。
#   S5 删除仓库目录（只有 S1 留档成功才会执行）。
#   S6 列出 ZCode 本工作区痕迹（只列出不删除，按清单在 ZCode 界面内手动移除）。
#   S7 声明保留 %USERPROFILE%\.zcode\skills（用户级、非本项目专属，去留由你定）。
#   任何一步失败→停在后置步骤前，打印已完成/未完成清单。
# =============================================================================
param(
    [switch]$ConfirmPC2Verified,   # PC2 验证通过的确认开关（缺省＝拒绝运行）
    [switch]$DryRun,               # 演练模式：只打印计划，不动任何文件
    [string]$RepoPath = 'D:\Download\Agent Vault\一键黑屏'  # 旧电脑上的仓库位置
)
$ErrorActionPreference = 'Stop'

# ---------- 0. 门槛检查 ----------
function Show-D6Checklist {
    Write-Host ''
    Write-Host '================ PC2 验证五项清单（D6，全部打勾才有资格清理旧机） ================' -ForegroundColor Yellow
    Write-Host '  [ ] 1. git fsck 无错误输出（仓库完整性）'
    Write-Host '  [ ] 2. git status 干净（无未提交改动）'
    Write-Host '  [ ] 3. verify.cmd 七件套全部 [OK] ALL VERIFIERS PASSED'
    Write-Host '  [ ] 4. 双击 一键黑屏.exe 出现全屏遮罩，按任意键关闭'
    Write-Host '  [ ] 5. 设置窗口能打开、能改一项设置并保存成功'
    Write-Host '=====================================================================' -ForegroundColor Yellow
    Write-Host '逐项操作方法见 migrate/迁移手册.md 阶段 8。五项全勾后再来运行本脚本。'
}

if (-not $ConfirmPC2Verified) {
    Write-Host '[REFUSED] 未带 -ConfirmPC2Verified 参数，本脚本拒绝运行，未触碰任何文件。' -ForegroundColor Red
    Show-D6Checklist
    exit 1
}

if (-not (Test-Path $RepoPath)) {
    Write-Host "[FAIL] 仓库目录不存在：$RepoPath （可用 -RepoPath 参数指定实际位置）" -ForegroundColor Red
    exit 1
}

# 拒绝在仓库目录内运行（强制「复制到桌面再跑」的操作序，避免脚本删到自己正在读的文件）。
if ($PSScriptRoot -and ($PSScriptRoot -like "$RepoPath*")) {
    Write-Host '[REFUSED] 本脚本正位于仓库目录内。请先把它复制到桌面，再运行桌面副本。' -ForegroundColor Red
    exit 1
}

$desktop = [Environment]::GetFolderPath('Desktop')
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$archiveZip = Join-Path $desktop "一键黑屏-最终留档-$stamp.zip"
$results = [System.Collections.ArrayList]::new()
function Record([string]$step, [string]$state, [string]$detail) {
    $null = $script:results.Add(('{0,-4} {1,-6} {2}' -f $step, $state, $detail))
    Write-Host ('{0,-4} {1,-6} {2}' -f $step, $state, $detail)
}

Write-Host ''
Write-Host "模式：$(@{ $true = 'DryRun 演练（不改动任何文件）'; $false = '真实清理' }[$DryRun.IsPresent])"
Write-Host "仓库：$RepoPath"
Write-Host "桌面：$desktop"
Write-Host ''

# ---------- S1 打包留档 ----------
try {
    if ($DryRun) {
        Record 'S1' '[DRY]' "将把整个仓库压缩到桌面：$archiveZip 并打印其 SHA256"
    } else {
        if (Test-Path $archiveZip) { Remove-Item $archiveZip -Force }
        Compress-Archive -Path (Join-Path $RepoPath '*') -DestinationPath $archiveZip -Force
        $hash = (Get-FileHash $archiveZip -Algorithm SHA256).Hash
        $size = '{0:N0} 字节' -f (Get-Item $archiveZip).Length
        Record 'S1' '[OK]' "留档完成 $archiveZip（$size）"
        Record 'S1' '[OK]' "SHA256 = $hash"
    }
    $s1ok = $true
} catch {
    Record 'S1' '[FAIL]' $_.Exception.Message
    $s1ok = $false
}

# ---------- S2 DeskBox 两个快捷方式 ----------
$lnkDir = Join-Path $env:USERPROFILE 'DeskBox\DeskBox'
foreach ($name in @('一键黑屏.lnk', '一键黑屏-设置.lnk')) {
    $lnk = Join-Path $lnkDir $name
    try {
        if ($DryRun) {
            Record 'S2' '[DRY]' "$(if (Test-Path $lnk) { "将删除 $lnk" } else { "不存在，跳过 $name" })"
        } elseif (Test-Path $lnk) {
            Remove-Item $lnk -Force
            Record 'S2' '[OK]' "已删除 $lnk"
        } else {
            Record 'S2' '[SKIP]' "本就不存在：$name"
        }
    } catch { Record 'S2' '[FAIL]' $_.Exception.Message; $s1ok = $false }
}

# ---------- S3 %LOCALAPPDATA%\BlackScreen ----------
$cfgDir = Join-Path $env:LOCALAPPDATA 'BlackScreen'
try {
    if ($DryRun) {
        Record 'S3' '[DRY]' "$(if (Test-Path $cfgDir) { "将删除配置目录 $cfgDir" } else { '不存在，跳过' })"
    } elseif (Test-Path $cfgDir) {
        Remove-Item $cfgDir -Recurse -Force
        Record 'S3' '[OK]' "已删除 $cfgDir"
    } else {
        Record 'S3' '[SKIP]' "本就不存在：$cfgDir"
    }
} catch { Record 'S3' '[FAIL]' $_.Exception.Message }

# ---------- S4 计划任务 ----------
try {
    if ($DryRun) {
        Record 'S4' '[DRY]' '将执行 schtasks /delete /tn OneClickBlackScreen-Idle /f（本就不存在则跳过）'
    } else {
        schtasks.exe /delete /tn 'OneClickBlackScreen-Idle' /f *> $null
        if ($LASTEXITCODE -eq 0) {
            Record 'S4' '[OK]' '已删除计划任务 OneClickBlackScreen-Idle'
        } elseif ($LASTEXITCODE -eq 1) {
            Record 'S4' '[SKIP]' '计划任务本就不存在（退出码 1＝放行）'
        } else {
            Record 'S4' '[FAIL]' "schtasks 退出码 $LASTEXITCODE"
        }
    }
} catch { Record 'S4' '[FAIL]' $_.Exception.Message }

# ---------- S5 删除仓库目录 ----------
try {
    if (-not $s1ok) {
        Record 'S5' '[SKIP]' 'S1 留档未成功，按铁则跳过仓库删除（旧文件一件不删）'
    } elseif ($DryRun) {
        Record 'S5' '[DRY]' "将删除整个仓库目录 $RepoPath"
    } else {
        Remove-Item $RepoPath -Recurse -Force
        Record 'S5' '[OK]' "已删除 $RepoPath"
    }
} catch { Record 'S5' '[FAIL]' $_.Exception.Message }

# ---------- S6 ZCode 工作区痕迹（只列不删） ----------
Record 'S6' '[LIST]' 'ZCode 本工作区可定位痕迹（请到 ZCode 界面内手动移除，本脚本不盲删）：'
$zcodeRoot = 'D:\Download\ZCodeData\.zcode\v2'
foreach ($p in @(
    (Join-Path $zcodeRoot 'tasks-index.sqlite'),
    (Join-Path $zcodeRoot 'visualizations'),
    (Join-Path $zcodeRoot 'checkpoints')
)) {
    if (Test-Path $p) { Record 'S6' '[LIST]' "  存在：$p" }
}
Record 'S6' '[LIST]' '  提示：tasks-index.sqlite 是所有工作区共用的任务索引（删库会影响其他工作区，'
Record 'S6' '[LIST]' '        建议只在 ZCode 应用的会话/任务列表里删除本项目相关条目，不动文件本身。'

# ---------- S7 保留声明 ----------
Record 'S7' '[KEEP]' "$env:USERPROFILE\.zcode\skills 已保留：这是用户级技能库、非本项目专属，"
Record 'S7' '[KEEP]' '        其他项目仍在用；最终去留由你自行决定（删除不影响一键黑屏）。'

# ---------- 汇总 ----------
Write-Host ''
Write-Host '==================== 执行结果汇总 ===================='
$results | ForEach-Object { Write-Host $_ }
$failed = $results | Where-Object { $_ -match '\[FAIL\]' }
if ($failed) {
    Write-Host ''
    Write-Host '存在失败步骤：已停在后置受影响步骤前。上方清单即已完成/未完成全貌。' -ForegroundColor Red
    exit 1
}
Write-Host ''
if ($DryRun) { Write-Host '[DONE] DryRun 演练结束：未改动任何文件。' } 
else { Write-Host '[DONE] 清理完成。桌面留档 zip 由你自行处置（建议先传一份到网盘/U盘再删）。' }
exit 0

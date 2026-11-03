# 一键黑屏 v2 安装脚本：幂等同步 DeskBox 双快捷方式，指向仓库 dist 产物。
# 快捷方式目标目录 = 当前用户的 DeskBox（$env:USERPROFILE\DeskBox\DeskBox，随登录用户名自动适配，
# 换电脑/换用户名无需改脚本；要装到别处可改下方 $deskBox 一行）。
# 只创建/更新以下两个 .lnk，不触碰目录内其他文件：
#   一键黑屏.lnk      （无参数 = 立即遮罩）
#   一键黑屏-设置.lnk  （--settings = 设置窗口）
$ErrorActionPreference = 'Stop'

$repo = Split-Path -Parent $MyInvocation.MyCommand.Path
$distDir = Join-Path $repo 'dist'
$exe = Join-Path $distDir '一键黑屏.exe'
if (-not (Test-Path $exe)) {
    Write-Host "[FAIL] 未找到 $exe ；请先在仓库根运行 build.cmd 生成产物。"
    exit 1
}

# 快捷方式目标目录：默认当前用户 DeskBox（可在上面一行按需改成其他目录）。
$deskBox = Join-Path $env:USERPROFILE 'DeskBox\DeskBox'
if (-not (Test-Path $deskBox)) { New-Item -ItemType Directory -Path $deskBox | Out-Null }

$shell = New-Object -ComObject WScript.Shell
$iconSettings = Join-Path $distDir 'icon-settings.ico'
if (-not (Test-Path $iconSettings)) {
    Write-Host "[FAIL] 未找到 $iconSettings ；请先在仓库根运行 build.cmd 生成产物。"
    exit 1
}
$targets = @(
    @{ Name = '一键黑屏.lnk';     Arguments = '';            IconLocation = '';                                   Description = '一键黑屏 v2 - 全屏遮罩' },
    @{ Name = '一键黑屏-设置.lnk'; Arguments = '--settings'; IconLocation = "$iconSettings,0";                   Description = '一键黑屏 v2 - 设置' }
)

foreach ($target in $targets) {
    $lnkPath = Join-Path $deskBox $target.Name
    $lnk = $shell.CreateShortcut($lnkPath)
    $lnk.TargetPath = $exe
    $lnk.Arguments = $target.Arguments
    if ($target.IconLocation) { $lnk.IconLocation = $target.IconLocation }
    $lnk.WorkingDirectory = $distDir
    $lnk.Description = $target.Description
    $lnk.Save()

    # 写后回读校验目标/参数/图标（证明幂等生效）。
    # 未自定义图标时 WScript 回读 IconLocation 为 ',0'（空路径+索引），视同未设置。
    $check = $shell.CreateShortcut($lnkPath)
    $readIcon = [string]$check.IconLocation
    if ($readIcon -eq ',0') { $readIcon = '' }
    if ($check.TargetPath -ne $exe -or $check.Arguments -ne $target.Arguments -or
        $readIcon -ne $target.IconLocation) {
        Write-Host "[FAIL] 快捷方式校验失败: $lnkPath"
        Write-Host ("  期望 IconLocation='{0}' 实际='{1}'" -f $target.IconLocation, $check.IconLocation)
        exit 1
    }
    Write-Host ("[OK] {0}  ->  {1} {2}  Icon='{3}'" -f $target.Name, $check.TargetPath, $check.Arguments, $readIcon)
}

Write-Host "[OK] install.ps1 完成（幂等，可重复运行；旧 exe 与其他文件未触碰）"
exit 0

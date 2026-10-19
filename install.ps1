# 一键黑屏 v2 安装脚本：幂等同步 DeskBox 双快捷方式，指向仓库 dist 产物。
# 只创建/更新以下两个 .lnk，不触碰目录内其他文件：
#   C:\Users\Lenovo\DeskBox\DeskBox\一键黑屏.lnk      （无参数 = 立即遮罩）
#   C:\Users\Lenovo\DeskBox\DeskBox\一键黑屏-设置.lnk  （--settings = 设置窗口）
$ErrorActionPreference = 'Stop'

$repo = Split-Path -Parent $MyInvocation.MyCommand.Path
$distDir = Join-Path $repo 'dist'
$exe = Join-Path $distDir '一键黑屏.exe'
if (-not (Test-Path $exe)) {
    Write-Host "[FAIL] 未找到 $exe ；请先在仓库根运行 build.cmd 生成产物。"
    exit 1
}

$deskBox = 'C:\Users\Lenovo\DeskBox\DeskBox'
if (-not (Test-Path $deskBox)) { New-Item -ItemType Directory -Path $deskBox | Out-Null }

$shell = New-Object -ComObject WScript.Shell
$targets = @(
    @{ Name = '一键黑屏.lnk';     Arguments = '';            Description = '一键黑屏 v2 - 全屏遮罩' },
    @{ Name = '一键黑屏-设置.lnk'; Arguments = '--settings'; Description = '一键黑屏 v2 - 设置' }
)

foreach ($target in $targets) {
    $lnkPath = Join-Path $deskBox $target.Name
    $lnk = $shell.CreateShortcut($lnkPath)
    $lnk.TargetPath = $exe
    $lnk.Arguments = $target.Arguments
    $lnk.WorkingDirectory = $distDir
    $lnk.Description = $target.Description
    $lnk.Save()

    # 写后回读校验目标与参数（证明幂等生效）。
    $check = $shell.CreateShortcut($lnkPath)
    if ($check.TargetPath -ne $exe -or $check.Arguments -ne $target.Arguments) {
        Write-Host "[FAIL] 快捷方式校验失败: $lnkPath"
        exit 1
    }
    Write-Host ("[OK] {0}  ->  {1} {2}" -f $target.Name, $check.TargetPath, $check.Arguments)
}

Write-Host "[OK] install.ps1 完成（幂等，可重复运行；旧 exe 与其他文件未触碰）"
exit 0

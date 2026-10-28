# README 多语言结构一致性校验
#
# 以 README.md（英文主版本）为基准，校验各语种译文的结构与代码块是否与主版本一致。
# 只校验结构（标题层级、折叠块、代码围栏、表格行、徽章、图片），不校验译文质量。
#
# 用法：
#   pwsh -File check-readme-structure.ps1          # 校验，失败退出码 1（CI 使用）
#   pwsh -File check-readme-structure.ps1 -Root D:\src\MyLib
param(
    [String]$Root = $PSScriptRoot,
    [String]$Config = "readme-languages.json"
)

$ErrorActionPreference = "Stop"

$configPath = Join-Path $Root $Config
if (!(Test-Path -LiteralPath $configPath)) { throw "未找到语言配置：$configPath" }

$langs = @((Get-Content -LiteralPath $configPath -Raw -Encoding UTF8 | ConvertFrom-Json).languages)
$master = Join-Path $Root 'README.md'
if (!(Test-Path -LiteralPath $master)) { throw "未找到主版本 README.md" }

# 统计结构特征。只取与语言无关的指标，避免译文长度差异造成误判
function Get-Shape([String[]]$lines) {
    [pscustomobject]@{
        Head2   = @($lines | Where-Object { $_ -match '^## ' }).Count
        Head3   = @($lines | Where-Object { $_ -match '^### ' }).Count
        Details = @($lines | Where-Object { $_ -match '^<details>' }).Count
        Summary = @($lines | Where-Object { $_ -match '^<summary>' }).Count
        Fences  = @($lines | Where-Object { $_ -match '^\s*```' }).Count
        Tables  = @($lines | Where-Object { $_ -match '^\| ' }).Count
        Badges  = @($lines | Where-Object { $_ -match 'img\.shields\.io' }).Count
        Mermaid = @($lines | Where-Object { $_ -match '^```mermaid' }).Count
        Breaks  = @($lines | Where-Object { $_ -match '^---+$' }).Count
    }
}

# 代码块比对策略：只比对真正的代码（带代码语言标记的围栏，以及以 FROM 开头的 Dockerfile），
# 注释、字符串字面量、日志样例、ASCII 图与 mermaid 图都允许按语种翻译
$CodeTags = @('csharp', 'cs', 'bash', 'sh', 'json', 'xml', 'powershell', 'ps1', 'yaml', 'yml')

function Get-Blocks([String[]]$lines) {
    $in = $false
    $tag = ''
    $cur = @()
    $blocks = @()
    foreach ($l in $lines) {
        if ($l -match '^\s*```(\S*)') {
            if ($in) {
                $first = if ($cur.Count -gt 0) { $cur[0] } else { '' }
                $blocks += if ($CodeTags -contains $tag -or $first.StartsWith('FROM')) { $cur -join "`n" } else { '' }
                $cur = @()
                $in = $false
                $tag = ''
            }
            else {
                $in = $true
                $tag = $Matches[1].ToLower()
                $cur = @()
            }
            continue
        }
        if (!$in) { continue }

        $t = $l.Trim()
        if ($t -eq '' -or $t.StartsWith('#') -or $t.StartsWith('//') -or $t.StartsWith('--') -or $t.StartsWith('<!--') -or $t.StartsWith('%%')) { continue }

        # 去掉行尾注释与字符串字面量，只比较代码骨架
        $t = [regex]::Replace($t, '\s*//.*$', '')
        $t = [regex]::Replace($t, '"(?:[^"\\]|\\.)*"', '""')
        $t = $t.Trim()
        if ($t -eq '') { continue }

        $cur += $t
    }

    return $blocks
}
$masterLines = Get-Content -LiteralPath $master
$refShape = Get-Shape $masterLines
$refBlocks = Get-Blocks $masterLines
$errors = 0

foreach ($item in $langs) {
    if ($item.file -eq 'README.md') { continue }

    $path = Join-Path $Root $item.file
    if (!(Test-Path -LiteralPath $path)) { Write-Host "✗ $($item.file)：文件不存在"; $errors++; continue }

    $lines = Get-Content -LiteralPath $path
    $shape = Get-Shape $lines
    $diffs = @()
    foreach ($p in 'Head2', 'Head3', 'Details', 'Summary', 'Fences', 'Tables', 'Badges', 'Mermaid', 'Breaks') {
        if ($shape.$p -ne $refShape.$p) { $diffs += "$p 期望 $($refShape.$p) 实际 $($shape.$p)" }
    }

    # 代码块内容必须与英文版一致（命令、配置、示例代码不翻译）
    $blocks = Get-Blocks $lines
    if ($blocks.Count -ne $refBlocks.Count) {
        $diffs += "代码块数量 期望 $($refBlocks.Count) 实际 $($blocks.Count)"
    }
    else {
        for ($i = 0; $i -lt $blocks.Count; $i++) {
            if ($blocks[$i] -eq '' -or $refBlocks[$i] -eq '') { continue }
            if ($blocks[$i] -cne $refBlocks[$i]) { $diffs += "第 $($i + 1) 个代码块内容与英文版不一致"; break }
        }
    }

    if ($diffs.Count -gt 0) {
        $errors++
        Write-Host "✗ $($item.file)"
        foreach ($d in $diffs) { Write-Host "  $d" }
    }
}

if ($errors -gt 0) {
    Write-Host "`nREADME 结构校验失败：$errors 个文件与 README.md 结构不一致。请按英文版补齐对应语种译文。" -ForegroundColor Red
    exit 1
}

Write-Host "README 结构校验通过：$($langs.Count - 1) 个语种与 README.md 一致" -ForegroundColor Green

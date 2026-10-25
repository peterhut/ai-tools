#Requires -Version 7.0
# Dependency-free integration checks against real temporary Git repositories.
[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false
$gitExecutable = (Get-Command git -CommandType Application | Select-Object -First 1).Source
$collector = Join-Path $PSScriptRoot 'Get-MaintainabilityHistory.ps1'
$temporary = Join-Path ([IO.Path]::GetTempPath()) ('maintainability-history-test-' + [Guid]::NewGuid().ToString('N'))
[void] [IO.Directory]::CreateDirectory($temporary)
$repository = Join-Path $temporary 'repo with spaces'

function Git {
    param([string[]] $Arguments)
    $output = & $gitExecutable -C $repository @Arguments 2>&1
    if ($LASTEXITCODE -ne 0) { throw "Fixture Git failed: $output" }
    return ($output -join "`n")
}
function Assert {
    param([bool] $Condition, [string] $Message)
    if (-not $Condition) { throw $Message }
}
function Write-Fixture {
    param([string] $Name, [string] $Text)
    [IO.File]::WriteAllText((Join-Path $repository $Name), $Text, [Text.UTF8Encoding]::new($false))
}
function Commit {
    param([string] $Message)
    $null = Git @('add', '--all')
    $null = Git @('commit', '--allow-empty', '-m', $Message)
}
function Collect {
    param([string[]] $ExtraArguments = @())
    $output = & pwsh -NoProfile -File $collector -RepositoryPath $repository @ExtraArguments 2>&1
    if ($LASTEXITCODE -ne 0) { throw "Collector failed: $output" }
    return ($output -join "`n" | ConvertFrom-Json)
}

try {
    [void] [IO.Directory]::CreateDirectory($repository)
    $null = Git @('init', '--initial-branch=main')
    $null = Git @('config', 'user.name', 'History fixture')
    $null = Git @('config', 'user.email', 'history-fixture@example.invalid')
    $null = Git @('config', 'commit.gpgsign', 'false')
    $hooks = Join-Path $temporary 'empty-hooks'
    [void] [IO.Directory]::CreateDirectory($hooks)
    $null = Git @('config', 'core.hooksPath', $hooks)
    Write-Fixture 'A.cs' 'a0'
    Write-Fixture 'B.cs' 'b0'
    Commit 'initial'
    Write-Fixture 'A.cs' 'a1'
    Write-Fixture 'B.cs' 'b1'
    Commit 'joint change'
    Write-Fixture 'B.cs' 'b2'
    Commit 'B alone'
    Write-Fixture 'A.cs' 'a2'
    Write-Fixture 'B.cs' 'b3'
    Write-Fixture 'C.cs' 'c'
    Write-Fixture 'D.cs' 'd'
    Write-Fixture 'E.cs' 'e'
    Commit 'broad change'
    Commit 'empty change'

    $data = Collect @('-MaxFilesPerCouplingCommit', '2')
    Assert ($data.selection.selectedCommitCount -eq 5) 'Root and empty commits must be sampled.'
    Assert ($data.eligibleCouplingCommitCount -eq 3) 'Broad and empty changes must be excluded only from coupling.'
    $a = @($data.paths | Where-Object path -CEQ 'A.cs')[0]
    $b = @($data.paths | Where-Object path -CEQ 'B.cs')[0]
    Assert ($a.changeCount -eq 3 -and $b.changeCount -eq 4) 'Broad changes still contribute activity.'
    Assert ($a.eligibleChangeCount -eq 2 -and $b.eligibleChangeCount -eq 3) 'Pair denominators must use the eligible population.'
    $pair = @($data.pairs | Where-Object { $_.pathA -ceq 'A.cs' -and $_.pathB -ceq 'B.cs' })[0]
    Assert ($pair.jointChangeCount -eq 2 -and $pair.associationConfidenceAtoB -eq 1) 'Joint count and forward proportion are incorrect.'
    Assert ([Math]::Abs($pair.associationConfidenceBtoA - 2.0 / 3) -lt 0.000001) 'Single-file changes must remain in the reverse denominator.'
    Assert ([Math]::Abs($pair.normalizedSupport - 2.0 / 3) -lt 0.000001) 'Normalized support must use eligible change count.'

    $filtered = Collect @('-MaxFilesPerCouplingCommit', '2', '-ExcludePath', 'B.cs')
    Assert (@($filtered.paths | Where-Object path -CEQ 'B.cs').Count -eq 0) 'Explicit exclusions must be applied.'
    Assert ($filtered.eligibleCouplingCommitCount -eq 2) 'Empty-after-filter commits must not enter coupling population.'

    $newName = 'renamed ü file.cs'
    $null = Git @('mv', 'B.cs', $newName)
    Commit 'rename'
    $renamed = Collect @('-Top', '100')
    Assert ($renamed.totalRenames -eq 1) 'Rename must be captured.'
    Assert ($renamed.renames[0].oldPath -ceq 'B.cs' -and $renamed.renames[0].path -ceq $newName) 'NUL parsing must preserve Unicode and spaces.'
    Assert (@($renamed.paths | Where-Object path -CEQ $newName)[0].changeCount -eq 1) 'Raw path identities must not silently join old names.'
    Write-Fixture 'untracked.txt' 'uncommitted'
    $dirty = Collect @('-MaxCommits', '1')
    Assert ($dirty.workingTreeDirty -and $dirty.selection.olderHistoryOutsideWindow) 'Dirty status and truncated window must be explicit.'
    Assert ($dirty.totalChangedPaths -eq 1 -and $dirty.pairs.Count -eq 0) 'Uncommitted paths must not enter committed counts; pairs must remain an array.'
    Remove-Item -LiteralPath (Join-Path $repository 'untracked.txt')

    $branchBase = (Git @('rev-parse', 'HEAD')).Trim()
    $null = Git @('checkout', '-b', 'feature')
    Write-Fixture 'feature.cs' 'v1'
    Commit 'feature one'
    Write-Fixture 'feature.cs' 'v2'
    Commit 'feature two'
    $null = Git @('checkout', 'main')
    Write-Fixture 'main.cs' 'main'
    Commit 'main change'
    $null = Git @('merge', '--no-ff', 'feature', '-m', 'integrate feature')
    $merged = Collect @('-MaxCommits', '3', '-Top', '100')
    Assert ($merged.commits[0].parentCount -eq 2) 'Fixture merge must be selected.'
    Assert (@($merged.paths | Where-Object path -CEQ 'feature.cs')[0].changeCount -eq 1) 'A merge must count its integrated first-parent diff once.'
    Assert (@($merged.commits | Where-Object { $_.subject -in @('feature one', 'feature two') }).Count -eq 0) 'Branch-internal commits must not be double-counted.'
    $pinned = Collect @('-Revision', $branchBase, '-MaxCommits', '1')
    Assert ($pinned.revision -eq $branchBase) 'Explicit revisions must be honored.'

    $badOutput = & pwsh -NoProfile -File $collector -RepositoryPath $repository -Revision '--help' 2>&1
    Assert ($LASTEXITCODE -ne 0) 'Invalid option-like revisions must fail rather than become Git options.'
    $outputFile = Join-Path $temporary 'report.json'
    $null = & pwsh -NoProfile -File $collector -RepositoryPath $repository -OutputPath $outputFile
    Assert ($LASTEXITCODE -eq 0 -and (Test-Path -LiteralPath $outputFile)) 'File output must work.'
    Assert ((Get-Content -LiteralPath $outputFile -Raw | ConvertFrom-Json).revision -eq (Git @('rev-parse', 'HEAD')).Trim()) 'Saved JSON must pin the same revision.'

    $clone = Join-Path $temporary 'shallow'
    $uri = [Uri]::new($repository + [IO.Path]::DirectorySeparatorChar).AbsoluteUri
    $null = Git @('clone', '--depth=1', '--branch=main', $uri, $clone)
    $shallowOutput = & pwsh -NoProfile -File $collector -RepositoryPath $clone 2>&1
    Assert ($LASTEXITCODE -eq 0) 'Shallow repositories must report limitations without fabricating root additions.'
    $shallowData = $shallowOutput -join "`n" | ConvertFrom-Json
    Assert ($shallowData.shallowHistory -and $shallowData.selection.analyzedCommitCount -eq 0) 'Missing first-parent data must be omitted, not treated as a root.'
    Assert ($shallowData.commits[0].omission -eq 'missing-first-parent' -and $shallowData.paths.Count -eq 0) 'Shallow omission and empty arrays must be explicit.'

    'History collector integration checks passed: counts, denominators, exclusions, root/empty/merge commits, renames, Unicode, dirty state, pinning, invalid refs, JSON and shallow history.'
}
finally {
    Remove-Item -LiteralPath $temporary -Recurse -Force
}

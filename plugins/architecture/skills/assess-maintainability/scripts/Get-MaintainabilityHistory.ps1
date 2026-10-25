#Requires -Version 7.0
<#
.SYNOPSIS
Collect bounded first-parent activity and local co-change evidence as JSON.
.DESCRIPTION
Read-only Git analysis. Counts paths, not semantic modules or line churn. Renames
are recorded under their destination without joining older path identities.
Directional association confidence uses only eligible change units, including
single-file changes. It is not confidence in a maintainability finding.
#>
[CmdletBinding()]
param(
    [string] $RepositoryPath = '.',
    [string] $Revision = 'HEAD',
    [ValidateRange(1, 5000)] [int] $MaxCommits = 50,
    [ValidateRange(2, 500)] [int] $MaxFilesPerCouplingCommit = 30,
    [string[]] $ExcludePath = @(),
    [ValidateRange(1, 500)] [int] $Top = 20,
    [string] $OutputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repository = (Resolve-Path -LiteralPath $RepositoryPath).Path

function Invoke-Git {
    param([string[]] $GitArguments, [switch] $AllowFailure)
    $info = [System.Diagnostics.ProcessStartInfo]::new('git')
    $info.UseShellExecute = $false
    $info.RedirectStandardOutput = $true
    $info.RedirectStandardError = $true
    $info.StandardOutputEncoding = [System.Text.Encoding]::UTF8
    $info.StandardErrorEncoding = [System.Text.Encoding]::UTF8
    foreach ($argument in (@('-C', $repository) + $GitArguments)) {
        $info.ArgumentList.Add($argument)
    }
    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $info
    try {
        [void] $process.Start()
        $outTask = $process.StandardOutput.ReadToEndAsync()
        $errTask = $process.StandardError.ReadToEndAsync()
        $process.WaitForExit()
        $stdout = $outTask.GetAwaiter().GetResult()
        $stderr = $errTask.GetAwaiter().GetResult()
        if ($process.ExitCode -ne 0 -and -not $AllowFailure) {
            throw "Git failed ($($process.ExitCode)): $stderr"
        }
        return [pscustomobject]@{ Text = $stdout; ExitCode = $process.ExitCode }
    }
    finally { $process.Dispose() }
}

function Get-GitText {
    param([string[]] $GitArguments)
    return (Invoke-Git -GitArguments $GitArguments).Text
}

$root = (Get-GitText @('rev-parse', '--show-toplevel')).TrimEnd("`r", "`n")
# Path arguments and revisions go through ArgumentList, never a shell expression.
$headCommit = (Get-GitText @('rev-parse', '--verify', '--end-of-options', "$Revision^{commit}")).Trim()
$shallow = (Get-GitText @('rev-parse', '--is-shallow-repository')).Trim() -eq 'true'
$dirty = (Get-GitText @('status', '--porcelain=v1', '-z')).Length -gt 0
$gitVersion = (Get-GitText @('--version')).Trim()
$sample = @( (Get-GitText @('rev-list', '--first-parent', "--max-count=$($MaxCommits + 1)", $headCommit)) -split '\r?\n' | Where-Object { $_ } )
$commits = @($sample | Select-Object -First $MaxCommits)
$pathCounts = [System.Collections.Generic.Dictionary[string, object]]::new([StringComparer]::Ordinal)
$pairCounts = [System.Collections.Generic.Dictionary[string, int]]::new([StringComparer]::Ordinal)
$changes = [System.Collections.Generic.List[object]]::new()
$renames = [System.Collections.Generic.List[object]]::new()
$eligibleCount = 0
$analyzedCount = 0

foreach ($commit in $commits) {
    $metadata = (Get-GitText @('show', '-s', '--format=%cI%x00%P%x00%s', $commit)).TrimEnd("`r", "`n").Split([char]0, 3)
    $parents = @($metadata[1] -split ' ' | Where-Object { $_ })
    if ($shallow) {
        # Pretty-format parents can hide a shallow boundary. Inspect actual headers.
        $headers = (Get-GitText @('cat-file', '-p', $commit)) -split '\r?\n\r?\n', 2
        $parents = @($headers[0] -split '\r?\n' | Where-Object { $_.StartsWith('parent ') } | ForEach-Object { $_.Substring(7) })
    }
    $entry = [ordered]@{
        commit = $commit
        committedAt = $metadata[0]
        subject = $metadata[2]
        parentCount = $parents.Count
        includedPathCount = 0
        excludedPathCount = 0
        eligibleForCoupling = $false
        omission = $null
    }
    if ($parents.Count -gt 0 -and $shallow) {
        $parentCheck = Invoke-Git -GitArguments @('cat-file', '-e', "$($parents[0])^{commit}") -AllowFailure
        if ($parentCheck.ExitCode -ne 0) {
            $entry.omission = 'missing-first-parent'
            $changes.Add([pscustomobject]$entry)
            continue
        }
    }
    $diffArguments = @('diff-tree', '--no-commit-id', '--name-status', '-r', '-z', '--find-renames')
    if ($parents.Count -gt 0) { $diffArguments += @($parents[0], $commit) }
    else { $diffArguments += @('--root', $commit) }
    $tokens = (Get-GitText $diffArguments).Split([char]0)
    $paths = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    for ($index = 0; $index -lt $tokens.Length -and $tokens[$index];) {
        $status = $tokens[$index++]
        $path = $tokens[$index++]
        if ($status.StartsWith('R') -or $status.StartsWith('C')) {
            $oldPath = $path
            $path = $tokens[$index++]
            $renames.Add([pscustomobject]@{ commit = $commit; status = $status; oldPath = $oldPath; path = $path })
        }
        $excluded = $false
        foreach ($pattern in $ExcludePath) {
            if ($path -clike $pattern) { $excluded = $true; break }
        }
        if ($excluded) { $entry.excludedPathCount++; continue }
        [void] $paths.Add($path)
    }
    $analyzedCount++
    [string[]] $sortedPaths = @($paths)
    [Array]::Sort($sortedPaths, [StringComparer]::Ordinal)
    $entry.includedPathCount = $sortedPaths.Length
    $entry.eligibleForCoupling = $sortedPaths.Length -gt 0 -and $sortedPaths.Length -le $MaxFilesPerCouplingCommit
    if ($sortedPaths.Length -eq 0) { $entry.omission = 'no-included-paths' }
    elseif (-not $entry.eligibleForCoupling) { $entry.omission = 'too-many-paths-for-coupling' }
    if ($entry.eligibleForCoupling) { $eligibleCount++ }
    foreach ($path in $sortedPaths) {
        if (-not $pathCounts.ContainsKey($path)) {
            $pathCounts.Add($path, [pscustomobject]@{ path = $path; changeCount = 0; eligibleChangeCount = 0 })
        }
        $pathCounts[$path].changeCount++
        if ($entry.eligibleForCoupling) { $pathCounts[$path].eligibleChangeCount++ }
    }
    if ($entry.eligibleForCoupling) {
        for ($left = 0; $left -lt $sortedPaths.Length; $left++) {
            for ($right = $left + 1; $right -lt $sortedPaths.Length; $right++) {
                $key = $sortedPaths[$left] + [char]0 + $sortedPaths[$right]
                if (-not $pairCounts.ContainsKey($key)) { $pairCounts.Add($key, 0) }
                $pairCounts[$key]++
            }
        }
    }
    $changes.Add([pscustomobject]$entry)
}

$pairs = @(
    foreach ($pair in $pairCounts.GetEnumerator()) {
        $names = $pair.Key.Split([char]0)
        $countA = $pathCounts[$names[0]].eligibleChangeCount
        $countB = $pathCounts[$names[1]].eligibleChangeCount
        [pscustomobject]@{
            pathA = $names[0]; pathB = $names[1]; jointChangeCount = $pair.Value
            eligibleChangeCountA = $countA; eligibleChangeCountB = $countB
            associationConfidenceAtoB = $pair.Value / [double]$countA
            associationConfidenceBtoA = $pair.Value / [double]$countB
            normalizedSupport = $pair.Value / [double]$eligibleCount
        }
    }
)
$result = [ordered]@{
    schemaVersion = 1
    repositoryRoot = $root
    revision = $headCommit
    gitVersion = $gitVersion
    workingTreeDirty = $dirty
    shallowHistory = $shallow
    selection = [ordered]@{
        kind = 'first-parent-window'; requestedCommitLimit = $MaxCommits
        selectedCommitCount = $commits.Count; analyzedCommitCount = $analyzedCount
        olderHistoryOutsideWindow = $sample.Count -gt $MaxCommits
        newestCommit = $commits[0]; oldestCommit = $commits[-1]
        newestCommittedAt = $changes[0].committedAt; oldestCommittedAt = $changes[$changes.Count - 1].committedAt
    }
    definitions = [ordered]@{
        changeUnit = 'first-parent commit; merge diff against first parent; root against empty tree'
        pathIdentity = 'raw repository path; rename counted at destination; older names not joined'
        changeFrequency = 'included analyzed commits touching path, including broad commits'
        couplingPopulation = 'commits with 1..maxFilesPerCouplingCommit included paths; includes single-file changes'
        associationConfidence = 'jointChangeCount / eligibleChangeCount of antecedent; not finding confidence or a CodeScene score'
        normalizedSupport = 'jointChangeCount / eligibleCouplingCommitCount'
        lineChurn = 'not measured'
        renameRecordsScope = 'all analyzed changes before path exclusions; display limited by topLimit'
    }
    filters = [ordered]@{
        excludePath = @($ExcludePath); wildcardSemantics = 'case-sensitive PowerShell -clike on repository-relative destination paths'
        maxFilesPerCouplingCommit = $MaxFilesPerCouplingCommit
    }
    eligibleCouplingCommitCount = $eligibleCount
    totalChangedPaths = $pathCounts.Count
    totalPairs = $pairs.Count
    totalRenames = $renames.Count
    topLimit = $Top
    paths = @($pathCounts.Values | Sort-Object -Property @{ Expression = 'changeCount'; Descending = $true }, path -CaseSensitive | Select-Object -First $Top)
    pairs = @($pairs | Sort-Object -Property @{ Expression = 'jointChangeCount'; Descending = $true }, pathA, pathB -CaseSensitive | Select-Object -First $Top)
    renames = @($renames | Select-Object -First $Top)
    commits = @($changes)
    limitations = @(
        'No semantic module grouping, rename identity normalization, defect classification, or Code Health score.'
        'Pair filtering changes denominators; broad commits still count toward activity.'
        'Remote freshness is not verified; selected history can contain merges, squashes, or ordinary commits.'
        'High association confidence with few observations is not evidence of architectural harm.'
    )
}
$json = $result | ConvertTo-Json -Depth 8
if ($OutputPath) {
    $destination = [System.IO.Path]::GetFullPath($OutputPath)
    [System.IO.File]::WriteAllText($destination, $json + "`n", [System.Text.UTF8Encoding]::new($false))
}
else { $json }

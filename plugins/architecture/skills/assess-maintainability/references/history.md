# Selecting and interpreting history

## Pin the unit before counting

Resolve the requested revision to a commit and record it. Inspect checkout status and shallow-history status. Use the selected branch's ancestry; `git log --all` includes unrelated branches and tool checkpoint refs.

### Optional PowerShell collector

With PowerShell 7 and Git, run `scripts/Get-MaintainabilityHistory.ps1` from this skill. It emits JSON for a bounded first-parent window, with pinned SHAs, change frequency, local co-change counts, directional association confidence, exclusions, and omitted units. Example from PowerShell, substituting the installed skill's script path:

```powershell
$reportPath = Join-Path ([IO.Path]::GetTempPath()) ('maintainability-' + [Guid]::NewGuid().ToString('N') + '.json')
& /path/to/skill/scripts/Get-MaintainabilityHistory.ps1 -RepositoryPath . -Revision HEAD -OutputPath $reportPath
$report = Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json
$report.selection
$report.paths | Format-Table path, changeCount, eligibleChangeCount
$report.pairs | Select-Object -First 5
```

Defaults: `-MaxCommits 50`, `-Top 20` displayed paths/pairs/renames, no path exclusions, and `-MaxFilesPerCouplingCommit 30`. Broad commits still count toward change frequency but are omitted from **both** joint counts and association-rule denominators. Single-path eligible commits remain in denominators. These are adjustable discovery/resource bounds, not research-derived cutoffs or CodeScene defaults. JSON reports the full eligible population count and each selected commit's inclusion/omission. An omitted redesign may contain the most important architectural change; inspect it selectively rather than infer absent coupling from its omission.

Use `-ExcludePath @('docs/*', 'generated/*')` only when those exclusions fit the question. Patterns use case-sensitive PowerShell wildcards on repository-relative paths, not Git pathspecs. Record filters; they can change whether a broad commit becomes eligible. Tests and generated paths are not silently excluded. Rankings are raw activity/support counts, not severity or priority.

The collector counts raw paths, including deleted paths. A rename counts at its destination for that transaction; earlier names remain separate. Rename records, sampled before exclusions, help identify where manual reconstruction is needed. It neither groups semantic modules nor normalizes identity across splits/moves. A missing parent in shallow history is reported and skipped rather than treated as a root. It does not fetch, inspect remote freshness, classify defects, measure line churn, infer Code Health, or compare complexity trends.

For an explicit range, use ordinary Git below rather than silently substituting a capped window. When PowerShell is unavailable, the same Git commands remain the fallback. Use the [signal definitions](signals.md) to interpret ratios, and inspect actual diffs before recommending anything. The dependency-free `scripts/Test-MaintainabilityHistory.ps1` checks collection against real temporary repositories.

### Ordinary Git

For the default window, start with:

```sh
git rev-parse HEAD
git status --short
git rev-parse --is-shallow-repository
git log -50 --first-parent --format='%H %cs %s' HEAD
```

Substitute the resolved revision for `HEAD` when requested. Record the oldest and newest included commit, the actual count, and the observation date range. Commit dates describe repository activity, not the age of every affected design. If fewer than 50 are available, use what is available and explain whether this reflects the repository root or missing history.

Inspect path changes for selected commits before loading full patches. For a merge, explicitly compare against its first parent, so a feature's net integrated change is the unit rather than all branch-internal commits:

```sh
git show --diff-merges=first-parent --find-renames --stat COMMIT
git diff --find-renames COMMIT^1 COMMIT -- relevant/path
git show COMMIT:relevant/path
```

For a root commit, use `git show --root` rather than a nonexistent parent. Quote paths and validate caller-supplied revisions as data; these examples use placeholders, not literal ref names to execute.

A first-parent commit can be a squash-merged PR, an ordinary commit, or a merge. Confirm the repository convention from history and report the actual unit. Never count internal commits and their containing merge as independent observations of the same change.

## Supplied PR or range

- **PR:** pin base/head SHAs and compare the base/head merge base with the PR head. Read the title/description and relevant discussion when available to understand intent, migration, or accepted trade-offs. One PR can seed an assessment but cannot by itself establish an accumulating trend.
- **Explicit commit range:** honor its semantics. `A..B` selects commits reachable from B excluding A; endpoint source differences are `git diff A B`. A three-dot diff compares a merge base to B and can answer a different question. Disclose the comparison used.
- **Historical PR:** assess its selected revision, then check whether a present-day recommendation has already been resolved. Label a retrospective exercise separately from advice about today's code.
- **Working tree:** include it only when requested or clearly part of the task. Record staged/unstaged/untracked scope separately; history does not describe those edits.

If the selected revision differs from the active checkout, inspect `git show` output or use an isolated checkout for checks. Keep users' active branches intact. Record remote freshness when relevant; do not imply cached refs were verified against the server.

## Co-change requires interpretation

Use recurring co-change to ask why responsibilities coordinate. Inspect the changes themselves before drawing a conclusion. Common confounders include:

- A shared protocol or schema legitimately changes its producers and consumers.
- Tests, fixtures, docs, migrations, generated files, and lockfiles support one behavior change.
- A large merge or formatting sweep aggregates unrelated work.
- A package extraction moves behavior without increasing its coupling.
- A file is a frequent co-change partner simply because it changes often.
- Renames, splits, squash policy, bots, and missing history alter observed counts.

Use targeted rename-aware diffs and search for moved symbols. `git log --follow` supports a single file, not general tracking of a module split. Reconstruct only the transitions needed for the claim and disclose gaps. Keep runtime, data, and implicit-order dependencies in view even when no import connects the files.

Without a reliable historical grouping, describe selected observed changes rather than report a coupling percentage. If using an existing tool, retain its definition and denominator. A changed analyzer, rule, exclusions, module grouping, or baseline can change measurements without changing architecture; compare equivalent observations and record those differences.

## Existing checks and baselines

Discover checks from repository scripts/configuration and relevant tests. Examples include architecture tests, import/layer rules, cycle checks, public API baselines, and package dependency checks. Use existing tool output as evidence; avoid adding a generic analysis stack just to run an assessment.

A baseline accepts known violations as a starting inventory; it does not declare them healthy. Compare against it without silently refreshing it. A newly introduced violation and an old accepted exception have different maintenance implications. A passing check proves only its configured scope, not overall maintainability.

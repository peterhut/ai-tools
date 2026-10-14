# Maintainability assessment

One bounded maintenance task deserves attention when the explorer data contract next changes: clarify and enforce the relationship between command-line and embedded-browser validation. No broad restructuring is supported by this review.

## Scope and limits

Repository: `/home/peter/.t3/worktrees/ai-tools/t3code-534d01ad`. Assessed committed revision: `6c60f3606b45676862eae507ba11b4b19b4d311d`. Default discovery window: latest 50 first-parent commits, yielding all 18 available first-parent commits, from root `c2eb42f99a01992e7bad297d5a43c08c3aac1f5d` (2026-07-13) through assessed HEAD (2026-10-14). History is not shallow; the shorter window reaches the repository root. Ordinary, squash-style, and merge commits occur; the Spotify integration at HEAD was treated as one first-parent integration, not as its branch-internal commits too. Selected merge/path summaries and rename-aware cleanup history were inspected. No numerical coupling score was used.

The checkout is dirty. Modified marketplace files, untracked `GLOSSARY-MAP.md`, `plugins/architecture/`, and `docs/architecture-review/` were excluded from assessment. The candidate assessment skill and its history reference were read solely as execution instructions. Evaluation notes were not read. All assessed source paths were unchanged tracked files. `origin/main` locally equals HEAD; no fetch was performed, so server freshness is unknown. PR discussion and uncommitted intended behavior were unavailable as evidence.

Read repository instructions, committed README, committed glossary, plugin manifests, selected history and explorer validators/tests, and AgentAwake's activity and power-policy interfaces. Candidate questions were: (1) must a data-contract change coordinate multiple validators; (2) do recent activity-detection fixes force power-policy code to understand agent internals? Spotify is a new integration in this window and was inventoried, not deeply assessed; its workflow and ADR remain material unexplored areas. Explorer rendering/layout internals were not broadly reviewed.

## Recommendation: align validator obligations at the next contract change

**Disposition:** recommend a bounded action alongside the next explorer schema change; medium priority, high confidence in the present inconsistency. This is a present maintenance concern, not demonstrated architectural deterioration or a documented rule violation.

At assessed HEAD, `plugins/visualisation/skills/architecture-views/scripts/preflight-architecture-explorer.mjs:24` owns a standalone `validateArchitectureData`; `assets/architecture-explorer.html:465` independently owns `validate` (asset path relative to the same skill). The latter additionally checks responsibilities arrays at line 510 and rejects unknown fields. The verifier imports standalone validation at `scripts/verify-architecture-explorer.mjs:7`; the browser applies its embedded validator before opening at asset line 1834. Thus passing the documented browser-independent preflight does not mean the browser will accept the same data.

A browser-free probe directly exercised both existing functions with one otherwise valid graph: both returned no errors. Changing only `catalog[0].responsibilities` to a string left standalone validation successful, while embedded validation returned `catalog[0].responsibilities must be an array when present.` This establishes disagreement for that input; it does not establish a user incident.

History demonstrates current change pressure: `d368b0c` added relationship types/asynchronous fields to the standalone validator and browser contract together. Earlier hardening `c3e54dd` changed validation and rendering. A plausible next optional relationship field requires knowing the browser's allowed-field list and standalone type rules, then checking both acceptance paths. Leaving that obligation implicit preserves a way for changes or generated artifacts to pass one validation stage and fail the next.

**Proportionate options:** first decide which invariants both entry points promise, then exercise those agreed invariants with shared contract fixtures. Alternatively localize common validation while retaining a standalone generated HTML artifact. The latter adds packaging/generation obligations and should not introduce an external runtime dependency merely to remove duplication. Leaving the present design alone is reasonable between contract changes; revisit when another field, comparison rule, or renderer is added, or if preflight/browser disagreements affect users. A future check should enforce agreed acceptance behavior, not assume identical diagnostics or all browser-specific checks.

**Counterevidence:** standalone HTML delivery and browser-independent preflight are legitimate separate entry points; the browser may deliberately be stricter. No inspected document explicitly promises identical validation. The recommendation therefore concerns making shared obligations explicit, not mechanically merging every check. Regression suites already exercise invalid endpoints, parent cycles, and duplicate edges, so validation is not wholly untested.

## Dismissed candidates

The duplicate `tools/preflight-architecture-explorer.mjs` copy visible in `d368b0c` is already deleted by `f0c84b4`, which also moves regression scripts into the plugin. Do not recommend fixing that resolved packaging duplication.

Recent AgentAwake fixes (`6caba66`, `a3f8534`) warranted checking whether provider-specific detection leaked into sleep policy. At HEAD, `OpenCodeActivity.GetSnapshotAsync` contains server discovery/status parsing, and the main loop consumes an activity count (`tools/AgentAwake.cs:73–87`, class at line 505). Codex transcript/lock rules remain in named helpers. The release decision at lines 101–140 uses agent activity and power settings rather than HTTP/protocol details. A new provider would still need explicit wiring and diagnostics, but the examined history does not establish unnecessary cross-responsibility coordination. Accept this arrangement; file length and repeated fixes alone do not justify extraction.

## Checks and evidence limits

Commands run successfully:

- `git rev-parse HEAD`, `git status --short`, `git rev-parse --is-shallow-repository`, `git log -50 --first-parent ... HEAD`, and selected `git show`/`git diff` inspections.
- `node --check plugins/visualisation/skills/architecture-views/scripts/preflight-architecture-explorer.mjs` and the corresponding `verify-architecture-explorer.mjs`: exit 0; syntax only.
- `node --input-type=module` with a disposable inline probe importing existing standalone validation and evaluating only the unchanged embedded `validate` function plus its enum constants via `node:vm`. Baseline: both accepted. Responsibilities-string variant: standalone accepted, embedded rejected. No browser, DOM, external dependency, or full application execution was involved.

Existing browser regression suites were inspected but not run because this assessment excludes browser/runtime setup. AgentAwake's Windows self-test was not run on Linux. Plugin validators were not run: no plugin edits occurred, and package validity would not test the investigated validator parity. No code, constraints, baselines, issues, or external application state changed.

## Execution observations

No actionable skill-workflow defect was encountered. Defaults yielded a reproducible bounded history window and allowed dismissal of the already-resolved script duplication. The requirement to seek counterevidence was useful for distinguishing independently deployed validation entry points from an unjustified demand for one source file. The recommendation's contract boundary remains inferred; the skill correctly permits that distinction rather than requiring an invented documented violation.

The initial assessment used 14 shell invocations, sometimes batching multiple reads. One additional invocation saved and reran the probe. This describes investigation activity, not a measure of architecture quality.

## Reproducing the acceptance disagreement

At the assessed revision, run this browser-free probe from the repository root. It extracts only the embedded validation function and its enum constants; it does not run the viewer or load browser dependencies.

```sh
node --input-type=module <<'JS'
import fs from 'node:fs';
import vm from 'node:vm';
import { validateArchitectureData } from './plugins/visualisation/skills/architecture-views/scripts/preflight-architecture-explorer.mjs';
const html = fs.readFileSync('plugins/visualisation/skills/architecture-views/assets/architecture-explorer.html', 'utf8');
const constants = html.split('\n').filter(line => /const (relationshipTypes|layoutDirections|layoutDensities|layoutRoutings) =/.test(line)).join('\n');
const start = html.indexOf('    function validate(data) {');
const end = html.indexOf('    function activeView()', start);
const browserValidate = vm.runInNewContext(constants + '\n' + html.slice(start, end) + '\nvalidate');
const data = {
  meta: { title: 'Probe', question: 'Schema consistency', repositoryRoot: '/fixture' },
  catalog: [{ id: 'a', name: 'A', kind: 'Container', evidenceState: 'observed', confidence: 'high', evidence: [] }],
  relationships: [],
  views: [{ id: 'main', label: 'Main', level: 'Container', perspective: 'Logical', scenario: '', renderer: 'graph', nodes: [{ id: 'a' }], edges: [] }]
};
console.log(JSON.stringify({ probe: 'valid baseline', cli: validateArchitectureData(data), browserFunction: browserValidate(data) }));
data.catalog[0].responsibilities = 'incorrect scalar';
console.log(JSON.stringify({ probe: 'responsibilities scalar', cli: validateArchitectureData(data), browserFunction: browserValidate(data) }));
JS
```

Observed output, independently reproduced by the parent agent:

```json
{"probe":"valid baseline","cli":[],"browserFunction":[]}
{"probe":"responsibilities scalar","cli":[],"browserFunction":["catalog[0].responsibilities must be an array when present."]}
```

The disagreement is observed. Whether that check belongs to both validators remains a maintainer decision; no visualisation code was changed.

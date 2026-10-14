# Architecture-aware review: first-pass research

Read date: 2026-10-14. This is a bounded primary-source survey for an investigative skill, not a settled design or an exhaustive literature review. Skill instructions below are practitioner heuristics; benchmark results apply to their experimental settings. None of these sources validates token use, file count, or tool calls as a general architecture quality score.

The subsequent [research foundations](research-foundations.md) grounds the implemented skill's language and signals in information hiding, evolutionary coupling, relative churn research, and distinct CodeScene metrics. It is the focused source note for the research and PowerShell collector update.

## Existing skills and reusable ideas

Matt Pocock's [codebase-design](https://github.com/mattpocock/skills/blob/main/skills/engineering/codebase-design/SKILL.md) treats a module's interface as everything callers must know, including ordering, invariants, errors, configuration, and performance. Depth concerns capability behind that interface; locality concerns concentrating knowledge, changes, bugs, and verification. It explicitly rejects implementation/interface line ratios because padding would improve the metric. Its deletion thought experiment asks whether removing a module eliminates complexity or redistributes it to callers. These are useful review questions, not measured laws. Rules about one versus multiple adapters should remain contestable heuristics rather than universal rejection criteria.

His [improve-codebase-architecture](https://github.com/mattpocock/skills/blob/main/skills/engineering/improve-codebase-architecture/SKILL.md) already scopes exploration by a named concern or recently changed hotspots, consults glossary and ADRs, and investigates navigation friction, shallow modules, leaked knowledge, and testability. It separates candidate discovery from interface design and labels recommendation strength. This supports selective exploration and evidence against an existing architectural decision before reopening it. Its scope is a refactoring opportunity survey, not attribution of degradation to a PR.

His [retro](https://github.com/mattpocock/skills/blob/main/skills/engineering/retro/SKILL.md) examines actual session evidence for navigation problems, expensive tools, missing information, and ineffective guardrails. It directs mechanical rules into deterministic checks and first looks for existing checks. This gives a useful competing explanation for costly agent work: poor tooling or discoverability can cause exploration overhead without defective module design. Its claim that review requires no exploration does not fit the surrounding-code investigation proposed here.

pstack's [minimize-reader-load](https://github.com/cursor/plugins/blob/main/pstack/skills/principle-minimize-reader-load/SKILL.md) separates layers traversed from hidden or mutable state held in mind. It asks whether a layer compresses knowledge or merely repeats a surface. Its informal 30-second reader test is a heuristic, not a validated measurement protocol. A blanket ban on single-caller wrappers or adapters would be stronger than the evidence warrants.

pstack's [blast-radius](https://github.com/cursor/plugins/blob/main/pstack/skills/blast-radius/SKILL.md) goes beyond callers to serialization, database schemas, lifecycle timing, pinned library code, and downstream consumers. It asks reviewers to identify pivotal safety assumptions and prove them through code or execution, recording unproven claims explicitly. This is primarily correctness analysis; reuse the evidence discipline without requiring runtime proof of every future maintenance concern.

pstack's [interrogate](https://github.com/cursor/plugins/blob/main/pstack/skills/interrogate/SKILL.md) separates independent review generation from lead adjudication, distinguishes actionable concerns from trade-offs and dismissed claims, and forbids automatic application. Agreement between models is presented as signal, but agreement alone cannot establish correctness or replace code evidence.

## What the benchmark evidence supports

| Source | What it evaluates | Relevance and limit |
| --- | --- | --- |
| [SlopCodeBench, paper](https://arxiv.org/abs/2603.24755), [implementation](https://github.com/SprocketLab/slop-code-bench) | Agents repeatedly extend their own implementations as specifications evolve; measures correctness plus duplication/redundancy and concentration of complexity. | Supports evaluating trajectories beyond one passing patch. Its structural measures are proxies, not proof that a specific boundary change raises future context cost. The repository describes the benchmark as an evolving evaluation primitive. |
| [SWE-Milestone, published paper](https://proceedings.mlr.press/v306/deng26d.html) | Reconstructed milestone dependency graphs and continuous task streams; evaluates preservation of system integrity and accumulated errors. | Reports a gap between isolated and continuous performance in its evaluated models/frameworks. This motivates multi-step validation, but does not isolate architecture as the cause or test the proposed reviewer. |
| [SWE-CI, paper](https://arxiv.org/abs/2603.03823), [full text v1](https://arxiv.org/html/2603.03823v1) | Repository evolution tasks executed through repeated analysis, implementation, and CI feedback. | Adds repeated maintenance and regression-sensitive outcomes. CI success is constrained by test coverage; it does not establish that a codebase is easy to understand or extend. |

Version caveat: [arXiv 2603.13428 v1](https://arxiv.org/html/2603.13428v1) is titled **EvoClaw**, whereas the published source above is **SWE-Milestone**. Do not silently combine numbers from different versions or imply these are independent replications. This pass inspected benchmark descriptions and methods at an initial level; it did not reproduce experiments, audit datasets, or independently establish their causal claims. It intentionally omits leaderboard rankings and pooled performance numbers.

The evidence supports the need to test sustained evolution. The stronger hypothesis—better architecture keeps context and reasoning cost stable as the repository grows—remains unverified here. Agent model, tooling, task difficulty, documentation, test quality, caching, and exploration strategy are plausible confounders. These are methodological cautions inferred from the proposed measurement, not reported experimental conclusions from these papers.

## History and deterministic checks

[CodeScene's change-coupling documentation](https://docs.enterprise.codescene.io/versions/6.4.3/guides/technical/change-coupling.html) defines co-evolution through shared commits, same-developer time windows, or shared ticket references. It explicitly says coupling can be appropriate and recommends comparing observed dependencies with architectural principles, then investigating code. It describes retrospective or weekly architectural feedback as useful scopes. Co-change is evidence of a relationship, not proof of inappropriate responsibility.

A lightweight Git implementation could retrieve path histories and recurring co-change pairs without a commercial dependency. Interpretation would still need explicit choices about time window, commit versus PR aggregation, renames, minimum observations, generated files, lockfiles, formatting sweeps, and large migrations. Those choices are proposed safeguards, not an implementation of CodeScene's proprietary algorithms. Raw pair frequency alone would conflate activity with coupling strength.

Existing tools can enforce explicit structural constraints:

| Ecosystem | Existing capability | Appropriate reviewer role |
| --- | --- | --- |
| JS/TS | [dependency-cruiser](https://github.com/sverweij/dependency-cruiser) extracts dependency graphs and validates configured rules, including cycles and forbidden import paths. | Read and run the repository's rules; compare base and head to attribute newly introduced violations. |
| Python | [Import Linter](https://import-linter.readthedocs.io/en/stable/contract_types/) supports forbidden/protected imports, layers, independence, and acyclic siblings. | Use declared contracts as evidence of intent rather than infer all allowed relationships from directories. |
| Java | [ArchUnit](https://www.archunit.org/userguide/html/000_Index.html) checks code dependencies and architecture rules, including layers and slices. | Use executable architecture tests where present instead of repeatedly asking an LLM to rediscover mechanical violations. |

Tool capabilities are established in their documentation. Their usefulness for this skill is an engineering inference. Static import analysis does not cover every runtime, configuration, data-schema, or cross-process dependency; no graph proves correct ownership of business rules. Tool setup is optional and language specific, and adopting a new constraint requires a design decision.

## Boundaries of a useful first experiment

A PR can provide evidence of a new dependency, newly exposed caller knowledge, an additional copy of a rule, or a change crossing an existing contract. Claiming an accumulating trend needs multiple comparable historical observations. Claiming a future task became harder needs either a concrete dependency/knowledge argument or a follow-up task experiment; changed-file count alone is insufficient. These distinctions are conclusions from this survey, not validated classifier rules.

For evaluation, retain healthy examples that legitimately touch several modules, migrations that remove old coupling, intentional redundancy at external boundaries, and wrappers that enforce important invariants. Compare ordinary review with the added architecture review on fixed base/head pairs, then have maintainers adjudicate evidence and actionability before seeing which method produced each finding. Track incremental useful findings, unsupported findings, investigation cost, and missing context separately. A later paired follow-up task can test whether accepted recommendations improve effort and regressions under a fixed task/model/tool setup. This is a proposed experiment, not evidence that the skill works.

Open research gaps: systematic evidence on review precision; reliable stopping rules for context expansion; historical-coupling normalization; reproducible measures of required knowledge; and causal benefit of an architectural review intervention. No skill implementation, plugin release, or architectural decision is made by this note.

## Follow-up: recurring analysis and targeted assessment

Read date: 2026-10-14. The discussion has shifted toward recurring structural analysis and targeted architecture assessment. The earlier PR experiment remains background research, not an agreed scope. The observations below do not choose a schedule, require historical change pressure, or determine the detail of remediation advice.

### Existing capabilities worth reusing

[CodeScene's current Delta Analysis documentation](https://docs.enterprise.codescene.io/latest/guides/delta/automated-delta-analyses.html) describes comparison against the current code baseline, quality gates for decline and hotspot goals, and recognition of improvements. It also detects missing members of historically co-changing file groups, explicitly acknowledging that a broken pattern can mean either successful refactoring or an omission. This illustrates an important distinction: recurring measurement can identify a changed pattern without establishing whether that change is good. These are documented vendor features, not independent evidence that the score predicts architecture quality.

[CodeScene Complexity Trends](https://docs.enterprise.codescene.io/latest/guides/technical/complexity-trends.html) recomputes complexity on historical hotspot versions, supports drilling into diffs between sample points, and warns that samples can encompass several commits. Its indentation-based metric is sensitive to formatting and does not understand every complex language construct; the documentation explicitly calls it a heuristic. It also documents a limitation when diffing renamed or moved files. A falling per-file score can therefore require investigation of extracted code elsewhere, rather than imply reduced system complexity.

[CodeScene change-coupling views](https://docs.enterprise.codescene.io/latest/guides/technical/change-coupling.html) distinguish strengthening, weakening, and stable relationships and support investigation of selected clusters. The documented workflow includes retrospective or weekly feedback, but that cadence is a product suggestion, not a requirement for this skill.

[dependency-cruiser's baseline support](https://github.com/sverweij/dependency-cruiser/blob/main/doc/cli.md) records existing violations, can suppress known ones, reports stale entries, and offers a `shrink-only` mode that removes resolved entries without accepting new ones. It also provides focused graph views around selected modules. [ArchUnit's frozen rules](https://www.archunit.org/userguide/html/000_Index.html#_freezing_arch_rules) similarly store existing violations and report additions; its documentation warns that refreezing accepts the current violations and succeeds. These existing mechanisms support gradual adoption without requiring old debt to be fixed immediately. A baseline is an accepted starting inventory, not evidence of health.

### What is useful with little history

The following are proposed uses of existing capabilities, not tool features or validated scoring methods:

- A current snapshot can expose dependency cycles, forbidden relationships, bypassed public entry points, and declared rules that no longer match actual structure. An assessment can investigate a specific relationship through callers, state, and rule ownership without claiming a trend.
- A first assessment can record a small set of observed concerns, their evidence, and important uncertainty. Later comparable observations permit change claims; no historic reconstruction is necessary to begin prospectively.
- User-reported friction, a difficult recent change, or a concrete subsystem question can select an assessment target when hotspot rankings lack enough data. Structural debt can be investigated independently of activity, while its priority remains a separate decision.
- Re-running the same check on unchanged source and configuration can test reproducibility. This establishes measurement reliability only; it does not establish that the measured relationship is harmful.

### Comparability and history pitfalls

For a proposed recurring graph comparison, retain source revision, dirty-worktree status, analyzer/version, rule/configuration revision, scope/exclusions, resolver/build settings, and module grouping. Otherwise a changed graph may reflect changed observation rather than changed source structure. Sort and compare semantic records rather than rendered diagram layout. Treat rule changes and baseline acceptance as explicit events; silently refreshing a baseline can erase the signal under investigation. These are proposed measurement safeguards, not a new architecture policy.

History scope is consequential. [Git's log documentation](https://git-scm.com/docs/git-log) specifies that `--all` traverses all refs, whereas `--first-parent` follows only the first parent at merges and changes the default merge diff. It also states that following renamed paths with `--follow` cannot handle multiple files. Therefore a repository's merge strategy, squash policy, branch/checkpoint refs, and rename handling affect the observations available for coupling analysis. Choose and report the history unit; do not present commit counts as interchangeable with independent requirements. Sparse history supports less confident temporal conclusions, not evidence of low coupling.

A useful assessment output could distinguish: observed present structure; comparable changes since a named observation; historical evidence where sufficient; and an investigated maintenance consequence with alternatives considered. This is a candidate evidence structure for discussion. The value of recurring analysis would still need evaluation through useful confirmed concerns, unsupported alerts, investigation effort, and repeatability; repeated score production alone would not demonstrate success.

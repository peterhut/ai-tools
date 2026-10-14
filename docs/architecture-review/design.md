# Architecture maintainability assessment

Status: experimental skill implemented after agreement on scope and pilot choices. Supporting sources and limitations are in [research.md](research.md). Maintainer evaluation remains open.

## Current direction

- The user pivoted the initial scope from PR review to continuous structural analysis and architecture assessment. A PR can expose a maintainability concern without being the appropriate unit for planning its remediation.
- The initial deliverable is a manually invoked skill doing analysis from PRs or Git data. A scheduled service, CI workflow, or persistent measurement system is outside this initial scope.
- Assessment targets are prioritized by change pressure and worsening evidence, rather than structural size or complexity alone. Explicit architectural violations remain eligible for investigation.
- The output is a maintenance recommendation for prioritization, not an implementation-ready remediation plan. Detailed remediation design follows a maintainer's decision to act.
- Maintainer judgment remains the chosen initial evidence of value, rather than measured improvement in subsequent agent task cost. The earlier comparison against ordinary PR review must be reconsidered for the new scope.
- An undocumented, inferred ownership boundary may support a risk finding when concrete code evidence and a plausible future-change consequence support it. The reviewer must identify the boundary as inferred and distinguish the risk from an explicit architectural rule violation.
- Without an explicit PR or range, inspect the latest 50 first-parent commits on the selected revision. Disclose the sample and merge conventions; the bound is for investigation cost, not statistical validity or a claim that commits are independent requirements.
- Sparse history does not prevent investigating a present concern. Distinguish a present maintainability concern supported by code and a concrete change scenario from demonstrated degradation, which needs comparable historical evidence.
- Use local examples to test the procedure and a second, more modular repository before claiming general usefulness. The user requested Photonic as a more interesting analysis target; local read access is confirmed.

The earlier one-PR-first experiment is superseded. The previous questions about PR-specific attribution and comparison are no longer the active frontier. The pivot does not establish that all architecture fixes should be deferred from PRs.

## Repository facts relevant to the pilot

- The repository originally had only the visualisation plugin. The architecture plugin now contains the experimental assessment skill described below.
- Architecture-view discovery and change-context references already describe selective investigation, evidence roles, merge-base comparisons, and descriptive change fan-out. Reuse these ideas without making visualization a requirement of review.
- The architecture explorer has several successive changes suitable for candidate retrospective cases. The Spotify CLI offers a small documented workflow but little longitudinal history. AgentAwake can help test whether the reviewer avoids treating a single large file as inherently problematic. None of these candidates has yet been adjudicated as healthy or problematic.
- T3 checkpoint refs must not be treated as development changes. The available first-parent history is small; it cannot establish a robust degradation trend by itself.
- Photonic is available at `/home/peter/repos/photonic` and `/home/peter/.t3/worktrees/photonic/t3code-978fccd9`. The base checkout is older than the available worktree. The worktree contains package-level responsibilities, domain context, ADRs, and architectural tests; pilot scoping must pin a revision and distinguish mainline from branch-only changes.

## Initial implementation

The `architecture` plugin packages `assess-maintainability`: pin a history window, select a few evidence-backed candidate questions, investigate relevant code and intent with counterevidence, then recommend maintenance priorities. It starts with up to three candidates and expands context only to resolve named uncertainties. This is a discovery bound, not a quota of findings. A history reference covers merge units, moves, sparse evidence, and existing checks. A PowerShell 7 helper collects reproducible history observations; ordinary Git remains the fallback. No analysis service, scheduler, or persistent architecture inventory is required.

The initial release was `0.1.0`. The current minor release is `0.2.0`: it adds source-backed signal interpretation and deterministic history collection, with the same base version in both manifests and a generated Codex cachebuster. Both repository marketplaces register the plugin; the visualisation plugin is unchanged. The repository's required original release helpers were recovered from official `openai/codex` commit `985cf47a4eb6084b2ff6b30ebdb1216acda85bb4`, under `codex-rs/skills/src/assets/samples/plugin-creator/scripts/`, because they were not installed locally. They are used from temporary storage rather than copied into the assessment plugin.

## Research grounding and collection

[Research foundations](research-foundations.md) connects the language to Parnas's information hiding, Zimmermann et al.'s evolutionary coupling, Nagappan and Ball's relative-churn evidence, and CodeScene's distinct hotspot, coupling, complexity-trend, and Code Health signals. The packaged signal guide keeps research evidence, vendor capabilities, and this skill's heuristics separate. Activity is a relevance signal; the assessed design consequence establishes concern. Source references do not validate the assessment skill itself.

The optional collector counts first-parent changes, preserving single-path changes in association-rule denominators. A configurable broad-change cutoff omits units from both pair counts and their denominators while retaining their activity. It reports raw-path identities, rename observations, exclusions, sparse history, and bounded output. It does not infer semantic modules, line churn, defect labels, complexity trends, or commercial scores. These deliberately narrow mechanics keep historical observation distinct from architectural interpretation.

The [independent signal trial](photonic-signals-trial.md) ran the revised skill with the collector against pinned Photonic, investigated two candidates, and made no new recommendation after inspecting later localization and explicit boundaries. This exercises the new workflow without establishing detection sensitivity or maintainer acceptance.

## Evaluation still required

The [Photonic pilot](photonic-pilot.md) and [local skill forward-test](ai-tools-trial.md) exercised evidence gathering and investigation limits. Photonic produced three dismissed candidates and no new recommendation; ai-tools produced one recommendation about validation obligations. See [evaluation.md](evaluation.md) for checks and limitations. A maintainer must judge the recommendation and dismissals; an agent assessment is not adjudicated ground truth. The pilot cannot establish that later tasks need fewer tokens or produce fewer regressions.

Resolved output and prioritization terminology is recorded in [GLOSSARY.md](GLOSSARY.md). Structural analysis supplies observations; the assessment investigates selected observations and decides whether they support a maintainability concern. Repeated observations alone do not establish degradation or mandate remediation.

The current choices are inexpensive to reverse, so they are recorded here rather than in an ADR. The existing root glossary concerns Spotify playlist management and is unchanged; the glossary map distinguishes it from the architecture-assessment context.

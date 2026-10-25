---
name: assess-maintainability
description: Assess architectural maintainability using development hotspots, change coupling, information hiding, and relevant code/history evidence. Use to investigate maintenance priorities from PRs, Git history, or existing CodeScene results. Produces recommendations rather than a merge verdict or refactoring implementation.
---

# Assess maintainability

Find evidence that ordinary changes require unnecessary coordination or knowledge, and recommend whether an area merits maintenance. Work from change activity into a small architectural slice. A quiet assessment with no recommendations is a valid result.

Use information hiding as the design question: which changeable decision should a module encapsulate, and where must other modules know that decision? Use evolutionary/change coupling as historical evidence of co-change, distinct from structural dependencies such as imports. These concepts guide investigation; they are not automatic defect classifications. [Signals and research](references/signals.md) defines the terms, source evidence, and interpretation limits.

## 1. Establish the evidence window

Pin the repository, assessed revision, and history selection. Honor a supplied PR, range, subsystem, or maintenance question. Otherwise use the latest 50 first-parent commits ending at the selected revision (default `HEAD`). This is a discovery bound, not a sample-size claim. Record whether local refs are stale, the checkout is dirty, or history is shallow or truncated. Separate uncommitted edits from committed evidence.

Read [history.md](references/history.md) when selecting or interpreting Git/PR evidence. It covers merge semantics, rename and refactor confounders, and safe use of co-change signals. Use local Git where sufficient; PR descriptions can clarify intent when access is available. Missing PR access is a limitation, not a reason to invent intent or require a connector.

When PowerShell 7 and Git are available, use the bundled [history collector](scripts/Get-MaintainabilityHistory.ps1) for a reproducible recent-commit window; read the invocation and limitations in [history.md](references/history.md). It collects evidence, not maintenance scores. For an explicit range or unavailable PowerShell, use ordinary Git instead. Save collector output in scratch space and inspect summaries before loading detailed evidence.

Locate repository instructions, domain terminology/context, relevant ADRs, project manifests, and existing architecture checks. Read their relevant parts as a candidate emerges. Keep documented intent distinct from observed behavior when they conflict.

Done when another reviewer could reproduce the revision and evidence window and distinguish observations from missing evidence.

## 2. Select a few investigation targets

Survey commit subjects and changed paths, then selected change summaries. Group changes by the repository's actual responsibilities; folders are only a starting hypothesis. Use development hotspots (concentrated change frequency) to locate active areas, and change coupling to investigate recurring joint changes. Read [signals.md](references/signals.md) when interpreting these signals or existing CodeScene output.

Keep activity and severity separate: frequent changes establish relevance; code-supported change propagation, duplicated decisions, or comparable declining measures can establish a concern. Planned work can also create change pressure even without a historical hotspot. Explicit architectural violations are eligible. Use CodeScene data when already available, preserving its measurement/configuration; otherwise describe local observations without inventing Code Health scores or a commercial ranking formula.

Use up to three candidate questions initially, fewer when evidence is thin. For each, name the maintenance consequence to investigate and the observation that led to it. Examples:

- Does adding one policy now require updating several independent copies of its rule?
- Which representation, ordering, lifecycle, or policy decision has escaped its owning module, increasing change propagation?
- Does a dependency make an otherwise local change require coordination with an unrelated responsibility?

Co-change, churn, dependency edges, large files, and indirection are investigation signals. They do not establish defects or urgency. A contract change, migration, generated output, tests, or deliberate adapter can legitimately span many paths. Check whether a later refactor already removed the suspected problem.

Done when each selected candidate has a concrete question and evidence worth following; drop candidates based only on size, style, or a preferred architecture.

## 3. Investigate and try to dismiss each candidate

Read the implicated behavior, relevant callers/callees, tests, and architectural intent. Expand only to resolve a named uncertainty. Before each expansion, identify what evidence could support or dismiss the candidate; avoid a default whole-repository read.

For each candidate:

1. Trace the rule, invariant, or decision and the responsibility that currently owns it. Follow runtime/configuration/data dependencies when static imports do not explain the behavior.
2. Inspect selected earlier versions or changes to determine whether it is a present concern, demonstrated worsening, resolved issue, or accepted trade-off. Follow moved behavior across renames and package splits.
3. Seek counterevidence: a justified shared contract, intentionally separate semantics, a migration in progress, an interface that already hides the relevant decision, an executable rule, or an ADR's explicit debt and revisit condition.
4. State a concrete plausible next change and the extra knowledge, coordination, or verification the observed structure would require. Identify the evidence behind that causal argument. Avoid claims about saved tokens, regressions, or time that were not measured.
5. Reuse relevant existing deterministic checks where practical. Record exact commands and results, or explain why they were not run. Treat a pass as evidence only for the encoded rule; a static graph cannot establish correct business ownership. Propose a new check only for a precise, agreed constraint that existing tools can enforce.

Stop when supported, dismissed, or unresolved within the relevant slice. An unresolved suspicion belongs in limitations, not recommendations. Sparse history can support a present concern; claiming deterioration requires comparable earlier and current evidence. An inferred boundary can support a risk with code evidence, but it is not an explicit rule violation.

Done when every candidate has an evidence-backed disposition and every recommendation survives the counterevidence considered.

## 4. Recommend maintenance priorities

Return a concise assessment with:

- **Scope and limits:** pinned revision, window/change unit, examined responsibilities, checks run, missing context, and material unexplored areas.
- **Recommendations, if any:** the observation (e.g. hotspot or co-change), investigated design concern (e.g. information leakage), code/history citations, documented rule or inferred boundary, present versus worsening condition, concrete future-change consequence, and why it merits attention under current change pressure. Distinguish a measured signal from its interpretation.
- **Proportionate options:** a bounded improvement direction, relevant trade-offs, the option of leaving it alone, and a condition for revisiting or acting. Favor localization or a smaller caller obligation when it removes demonstrated complexity; adding abstractions is not an objective.
- **Disposition:** recommend action, monitor under a stated condition, or accept the trade-off. Explain confidence from the evidence and distinguish it from priority. An actionable confirmed rule violation may need prompt correction even though this assessment is not a merge gate.
- **Brief counterevidence:** why the strongest dismissed candidates did not merit recommendations. Omit a catalogue of speculative smells.

Assess both existing concerns and worsening structure; a recommendation need not blame one PR. Cite the smallest set of revision-specific paths/symbols/lines and commits needed to reproduce each claim. When a numerical coupling measure is used, disclose counts, formula, denominator, exclusions, grouping, and window. Association-rule confidence is an observed conditional proportion; keep it distinct from confidence in a finding. Raw counts and metric trends do not establish causation or validated maintenance savings.

Write recommendations for prioritization, not a detailed refactoring plan. Return the assessment in the conversation unless the user requests a saved artifact. Keep scratch inventories disposable. Changes to code, architecture constraints, baselines, issues, or PRs require their own task authorization.

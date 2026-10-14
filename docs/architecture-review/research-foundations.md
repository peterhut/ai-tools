# Maintainability assessment: research foundations

Read date: 2026-10-14. This supplements [the earlier survey](research.md) with established software-engineering concepts and precise signal definitions. It distinguishes design arguments, historical empirical results, vendor capabilities, and our proposed assessment heuristics. None validates this assessment skill or promises quantitative savings.

## Design foundation: information hiding

Parnas, **On the Criteria To Be Used in Decomposing Systems into Modules** (CACM, 1972), argues for decomposing around difficult or likely-to-change design decisions and hiding each decision behind a module interface. His examples show representation changes propagating when several modules know the representation. He also examines unnecessary ordering guarantees and sequences required to invoke routines. A module is a responsibility assignment rather than necessarily one subroutine. Hierarchical dependencies and a decomposition that hides decisions are independent properties; an acyclic graph alone does not establish information hiding. This is a design argument illustrated through contrasting decompositions, not a measured predictor of maintenance effort. [Original paper, full-text transcription](https://www.cs.lafayette.edu/~gexia/cs301/resources/parnas.html), especially “Comparison,” “The Criteria,” and “Hierarchical Structure”; [publication DOI](https://doi.org/10.1145/361598.361623). The transcription explicitly warns that it may contain errors.

**Application heuristic:** translate “caller knowledge” into a concrete design decision that callers must know: representation, ordering, lifecycle, policy, or error semantics. Trace its propagation during a plausible change. This operationalizes the design argument; it does not make every shared contract or multi-module change a defect.

## History foundation: evolutionary coupling

Zimmermann, Weißgerber, Diehl, and Zeller, **Mining Version Histories to Guide Software Changes** (TSE, 2005), mine historical transactions for association rules between changed entities, including relationships unavailable to program analysis. Their evaluation concerns recommending related changes, not classifying architecture as healthy. Section 5.1 defines, for transaction set `D` and changed-entity sets `A`, `B`:

- `freq_D(A)` = number of transactions containing every entity in `A`.
- Support count of `A → B` = `freq_D(A ∪ B)`.
- Confidence of `A → B` = `freq_D(A ∪ B) / freq_D(A)` when the denominator is nonzero.
- Normalized support = support count divided by `|D|` (footnote 4).

Confidence is directional. The paper explicitly identifies normalization by transaction size and changes per item as further research, and describes precision/coverage trade-offs. Its reported predictive performance belongs to its projects, preprocessing, and evaluation. [Author-hosted full paper](https://thomas-zimmermann.com/publications/files/zimmermann-tse-2005.pdf), §§5.1, 7–8.

**Application heuristic:** call observed repeated joint changes *co-change* or *evolutionary/change coupling*. Keep this separate from structural dependencies such as imports. Report counts alongside any ratio, transaction definition, exclusions, window, renames, and grouping. A small denominator can yield high confidence; frequent changes and broad transactions create more opportunities for co-change. Inspect the code and change intent before inferring an unnecessary dependency. These safeguards do not reproduce a commercial algorithm.

## Churn foundation: normalization and outcome limits

Nagappan and Ball, **Use of Relative Code Churn Measures to Predict System Defect Density** (ICSE, 2005), compare absolute and relative churn measures in Windows Server 2003/SP1. Relative measures relate churn to size and temporal extent; they perform better in the reported defect-density models. The outcome is defects per size at binary/component level, not architectural maintainability or future change effort. Section 5.5 explicitly limits generalization from one very large system and notes that version control misses overlapping intermediate edits and can distort duration measures. [Microsoft Research full paper](https://www.microsoft.com/en-us/research/wp-content/uploads/2016/02/icse05churn.pdf).

**Application heuristic:** distinguish change frequency (transactions touching an entity) from line churn (added/deleted/changed lines). Neither is elapsed effort. Use activity to locate investigation targets and state size/activity confounding; do not borrow a defect-prediction result as a validated maintenance score.

## CodeScene: distinct documented signals

These are product definitions and vendor claims, not independent validation of the proposed assessment.

| Signal | Documented meaning | Assessment limit |
| --- | --- | --- |
| Hotspots | Current documentation identifies concentrated development activity and separates hotspot priority from Code Health severity. Prioritized refactoring targets combine frequency, health, and additional coordination information. [Technical Debt](https://docs.enterprise.codescene.io/latest/guides/technical/hotspots.html) | Activity establishes relevance, not a defect, measured effort, or severity. Do not claim its current ranking is a fixed `activity × size` formula. |
| Historical size/activity hotspot view | Version 3.3.9 explicitly uses file LOC as a complexity proxy and change frequency as an effort proxy, looking for overlap. [Versioned Hotspots documentation](https://docs.enterprise.codescene.io/versions/3.3.9/guides/technical/hotspots.html) | A proxy is not the measured property. This historical description must not be substituted for current Code Health or ranking semantics. |
| Change coupling | Supports shared commits, same-programmer time windows, or shared ticket IDs. Coupling may be appropriate; unexpected relationships invite code investigation. It also documents “sum of couplings,” counting repeated commit-level coupling to peers. [Change Coupling](https://docs.enterprise.codescene.io/latest/guides/technical/change-coupling.html) | The inspected guide does not specify an exact pairwise degree formula. Do not label directional confidence, Jaccard, or another local ratio “CodeScene coupling.” |
| Complexity trends | Recomputes complexity on historical hotspot versions; the metric is indentation based. Sample-point diffs may span several commits. Layout/style and unsupported complex constructs can bias it; the guide explicitly calls it a heuristic. [Complexity Trends](https://docs.enterprise.codescene.io/latest/guides/technical/complexity-trends.html) | Comparable measurements support a trend claim; they do not establish its cause. A falling file score can reflect extraction: inspect where responsibility moved. |
| Code Health | Vendor aggregate of source-code factors, including size, cohesion, complexity, and duplication. Rules and thresholds can be customized; disabling rules can improve a reported baseline without changing behavior. [Code Health](https://docs.enterprise.codescene.io/latest/guides/technical/code-health.html) | Preserve tool/version/configuration when comparing actual outputs. Do not invent a Code Health score from manual reading. Vendor assertions about cost/defect prediction are not guarantees for this repository or skill. |

## Proposed terminology and decision rule

These are assessment conventions, not additional research findings:

- **Change pressure:** observed recent activity or a stated upcoming need. Keep the source explicit; quiet history does not imply good structure.
- **Present maintainability concern:** code-supported propagation of knowledge, duplicated decisions, or unnecessary coordination for a named plausible change.
- **Worsening concern:** a present concern plus comparable earlier/current evidence that the obligation increased. More commits or a higher rank alone cannot establish deterioration.
- **Priority:** reason to act given change pressure, consequence, and alternatives. **Confidence:** strength of evidence for the concern. Keep them separate.
- **Recommendation:** investigate a history signal, test competing explanations, and recommend action only after identifying the concrete maintenance consequence. Accepted contracts, tests tracking production behavior, migrations, and moved responsibilities can dismiss a candidate. Repeated fixes establish recurring activity only until the repair causes are inspected.

The resulting method uses history for discovery and prioritization, information hiding for the causal design question, and code plus counterevidence for adjudication. Its effectiveness remains an evaluation question.

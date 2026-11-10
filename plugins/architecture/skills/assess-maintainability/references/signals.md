# Signals and research basis

Use these concepts to select questions and interpret evidence. Research motivates the questions; a repository-specific recommendation still needs code evidence and counterevidence. The investigation bounds and prioritization procedure are this skill's heuristics, not validated results from the cited work.

## Information hiding and change propagation

Parnas organizes modules around design decisions that should be hidden, including decisions likely to change. Ask which representation, ordering, lifecycle, or policy decision other modules must know, and what a change to it would propagate. An acyclic dependency graph does not by itself prove effective information hiding. This is a design argument, not a numerical predictor. [Parnas, 1972](https://doi.org/10.1145/361598.361623); [full-text transcription](https://www.cs.lafayette.edu/~gexia/cs301/resources/parnas.html).

Use the idea concretely: identify the decision, its current owner, callers that know it, and a plausible change requiring coordinated updates. Legitimate shared contracts and independently different policies can explain the same surface pattern.

## Scenario-led structural comparisons

Use a structural comparison only when it helps resolve a selected maintenance question. Start with a relevant scenario: the anticipated change, its conditions, the required response, and behavior or constraints that must remain intact. ATAM connects quality goals, scenarios, and architectural approaches to risks and tradeoffs. This bounded comparison adapts that reasoning; it is not a full ATAM evaluation or a validated predictor of agent effort. [SEI ATAM collection](https://www.sei.cmu.edu/library/architecture-tradeoff-analysis-method-collection/).

For example, when considering another pricing policy, trace which responsibilities must change, what callers must know, and what verification is needed. Use existing requirements where available; label a reviewer-proposed scenario and explain its relevance rather than treating it as a new requirement.

Select observations that answer the question, with different limits on their interpretation:

| Observation | What to inspect | Interpretation limit |
| --- | --- | --- |
| Dependencies | Direct edges, dependencies crossing a declared boundary, or indirect reachability, with named edge kind and direction. | Counts and reachability describe the chosen graph; they do not establish semantic coupling or actual change effort. |
| Quantity | Modules, services, files, or configuration points at one declared granularity. | Fewer nodes can result from regrouping while internal obligations remain; follow relocated behavior. |
| Chain length | The relevant import, call, or build path, with its responsibilities and counting convention. | A selected path is not a whole-graph maximum. Explain cycle handling where relevant; no universal hop limit establishes quality. |
| Semantic variety | Different mechanisms for the same responsibility and whether their semantics, lifecycle, or constraints justify the differences. | Describe concrete differences qualitatively; there is no calibrated diversity score here. |

Design-structure-matrix research compares software designs using direct and indirect dependencies. Its propagation-cost measure represents potential reachability-based change impact, not raw edge count, observed historical co-change, or measured developer effort. It supports careful structural characterization, not adopting a four-axis quality score. [MacCormack, Rusnak and Baldwin, 2006; author manuscript, pp. 11–14](https://www.hbs.edu/ris/Publication%20Files/05-016.pdf).

Keep structural chain length distinct from module depth as useful behavior behind a small interface. A facade may add a module and lengthen a path while reducing callers' knowledge of changeable details. Inspect those caller obligations and the responsibilities of intermediate modules before recommending flattening. Information hiding supplies the design rationale; neither hiding a dependency nor shortening a chain alone proves improvement. [Parnas, 1972](https://doi.org/10.1145/361598.361623).

For a before/after claim, pin both revisions and the slice, preserve relevant behavior and constraints, and use equivalent node granularity, edge definitions, classification, and exclusions. Record the tool/version/configuration or manual method, evidence window where applicable, and the actual counts or path examples. Inspect code moves and changed grouping or extraction rules before interpreting a delta. Partial evidence stays partial; missing measurements remain unknown.

Report the scenario, observations, supported consequences for knowledge/coordination/verification, and countervailing costs. An unimplemented alternative has predicted effects, not measured results. Use only the observations needed, retain uncertainty and counterevidence, and keep the existing option of leaving the structure alone. No aggregate score, mandatory measurement set, or new analysis service is implied.

## Development hotspots: relevance, then severity

CodeScene distinguishes concentrated development activity from Code Health and uses both when prioritizing maintenance. Borrow that separation: first identify frequently changed areas, then investigate whether their structure creates a maintenance cost. Large, stable code and frequently changed but well-encapsulated code can both warrant no recommendation. [CodeScene Technical Debt](https://codescene.io/docs/guides/technical/hotspots.html).

Local change frequency is the number of selected change units touching a path or responsibility. Line churn is an amount of changed code; neither measures elapsed effort. Describe the selected unit and window. Planned work is a separate source of relevance and should be identified as such.

Nagappan and Ball's relative churn measures predicted defect density better than absolute measures in their Windows Server study. That result motivates care about normalization; it does not validate a maintainability score, generalize a defect predictor to every repository, or establish that churn caused defects. [Nagappan and Ball, 2005](https://www.microsoft.com/en-us/research/publication/use-of-relative-code-churn-measures-to-predict-system-defect-density/).

## Change coupling: an observed relationship

Evolutionary coupling mines historical changes for related entities, including relationships missing from static program analysis. Zimmermann et al. study association rules for recommending related changes, not architectural health classification. [Mining Version Histories to Guide Software Changes, 2005, §5.1](https://thomas-zimmermann.com/publications/files/zimmermann-tse-2005.pdf).

For a declared set of eligible change units:

- `n(A)` counts units touching A; `n(B)` counts those touching B.
- `n(A,B)` is their joint count, or support count.
- Directional association confidence `A → B` is `n(A,B) / n(A)`; the reverse uses `n(B)`.
- Normalized support is `n(A,B) / N`, where N is the number of eligible units.

Eight joint changes among ten A changes and eighty B changes means 0.8 for `A → B` and 0.1 for `B → A`. Neither number is a probability that the architecture is defective. Small denominators, frequent partners, broad transactions, and the chosen unit affect interpretation. The paper itself notes unresolved normalization questions. Report counts and inspect the underlying changes.

CodeScene supports commit-, time-, and ticket-based relationships and explicitly allows useful coupling, such as source and tests. Its documented product signal is not necessarily the local association rule above. Preserve vendor metric names and definitions when available; call the script's output local co-change evidence. [CodeScene Change Coupling](https://codescene.io/docs/guides/technical/change-coupling.html).

## Trends and existing CodeScene evidence

| Observation | Question to investigate | Bound on the claim |
| --- | --- | --- |
| Repeated changes in an active area | Are changes localized around a hidden decision, or repeatedly coordinated across unrelated responsibilities? | Frequency alone proves activity, not a design problem. |
| Co-change across a supposed boundary | Is this one legitimate contract, supporting tests, a migration, or knowledge leaking across responsibilities? | Historical co-change and structural dependency are different observations. |
| Comparable complexity or Code Health decline | What behavior, responsibility, or caller obligation changed with it? | Measure changes, code moves, and formatting can change the observation without worsening maintainability. |
| Repeated confirmed repairs | Do they address the same decision or invariant in scattered places? | A “fix” subject alone does not establish a defect, shared root cause, or architectural cause. |

CodeScene's Complexity Trends recompute historical measurements; its indentation-based metric is explicitly heuristic and sensitive to formatting and language constructs. Compare equivalent measurements and inspect where extracted behavior moved before declaring improvement or decline. [Complexity Trends](https://codescene.io/docs/guides/technical/complexity-trends.html).

Code Health is CodeScene's aggregate of source factors, with configurable rules. Use an actual available result with revision, tool/version, rules, exclusions, and component mapping. A missing score stays missing. The skill neither recreates Code Health nor adopts every size/smell rule as a recommendation. Vendor claims about predictive value are not evidence that this skill has been validated. [Code Health](https://codescene.io/docs/guides/technical/code-health.html).

Use the reasoning chain **observed signal → investigated decision/dependency → concrete change consequence → priority and options**. Converging evidence can support a recommendation, but no composite risk score or threshold is required. Confidence in a finding concerns the strength of its evidence; priority concerns why to act now.

Review provider: GitHub Copilot (`acpRegistry_copilot`)
Model: `claude-sonnet-5.5`
Task ID: `node:delegated-task:command%3Amcp%3A30092782-b1fb-40b3-abeb-8dccf3360f98%3Adelegate-task%3Acouncil-copilot-sonnet55-plan-review-r1`
Scope: read-only design review; no implementation authorization.

# Council plan review, round 1

## Overall assessment

The design's core is sound and faithful to the user's decisions. The quality-attribute and scenario analysis, proportionality, assigned emphasis, honest fallback, dissent preservation and no-implementation boundary all hold together. Meeting mechanics have grown relative to the analysis they serve. Most of my concerns are about under-specified mechanics, not the direction.

Several load-bearing rules live only in the research notes, not in `design.md`. These include the fact/premise/preference taxonomy, the moderator's labelled substantive claims, "no material concern", and prior-involvement disclosure. A skill written from `design.md` alone would lose them.

## Material findings

**1. The main-thread moderator is probably the implementation's author. (Design judgment, high impact.)**
- Location: `design.md` ("Main-thread moderation is the default"); `moderator-model-selection.md` and `moderation-research.md` list "authored the change" only as a risk.
- Trigger: council's stated target is agent-generated changes, so the host agent usually wrote the change.
- Consequence: the author sets the agenda and the lenses, classifies disagreements, calls closure and writes the readout. The cited self-preference evidence (Panickssery) argues against this.
- Recommendation: require disclosure of authorship or prior involvement in the readout. Prefer a fresh moderator when the host wrote the change, instead of treating it as only an option.

**2. A fresh moderator conflicts with hosts that forbid nested spawning. (Unresolved question.)**
- Location: `design.md` delivery brief ("fresh moderator… main thread keeps the user conversation"); the `matt-skills-integration-research.md` note says upstream leaves recursive-spawn prevention to the harness.
- Trigger: the moderator is a subagent and cannot spawn the reviewers.
- Consequence: the main thread must still dispatch and relay every turn. On a cheap host that keeps the cost, and the relay is where distortion enters.
- Recommendation: state who dispatches. A workable split is moderator as judgment-only (returns the next-turn instructions) and main thread as dispatcher. Disclose that the relay remains.

**3. The word "priorities" is used for two different things. (Demonstrable inconsistency.)**
- Location: `design.md` ("assigns distinct quality-attribute priorities"); `GLOSSARY.md` says "perspective… emphasis".
- Trigger: in ATAM, priorities are the stakeholders' ranked quality drivers (the utility tree). Here they are a reviewer's assigned emphasis.
- Consequence: reviewers, or the readout, may treat an assigned emphasis as the system's real need. That undermines the "fitness, not maximisation" aim.
- Recommendation: use "assigned emphasis" for reviewer lenses and "quality drivers" for system needs. Add glossary entries for scenario, tradeoff point, sensitivity point and the reviewer/readout roles. The design also uses lens, perspective and emphasis interchangeably.

**4. The boundary between fact, missing premise and priority disagreement is not in the design. (Gap.)**
- Location: the taxonomy is in `agenda-led-process-research.md`, while `design.md` has only "resolve factual disputes where possible".
- Trigger: a disagreement over retry durability is a priority difference when the repo or user states the priority, and a missing premise when it does not.
- Consequence: the same disagreement can be suspended, reported as dissent, or reported as a conditional recommendation, depending on the moderator.
- Recommendation: adopt a one-line rule. If evidence or the user brief states the driver, it is settled. If it is not stated and changes the outcome, it is a missing premise. If shared facts and drivers leave a real tradeoff, it is dissent. Also say that a suspended item still gets conditional branches and a record of what it depends on, so the readout doesn't become a list of questions.

**5. Agenda omissions have a single anchor, and the fidelity check does not cover them. (Judgment.)**
- Location: `design.md` ("a reviewer checks the synthesis for material omissions"); `agenda-led-process-research.md` caveats.
- Trigger: the moderator frames the agenda and assigns the lenses. All three reviewers prepare inside that frame.
- Consequence: the reviewers and the fidelity check share the moderator's blind spot. The note concedes this, and the design adds no mitigation beyond amendment rights.
- Recommendation:
  - Make amendment mandatory output. Each reviewer reports "added / challenged / none" plus inspected coverage.
  - List excluded or deferred items, with reasons, in the readout.
  - Don't claim coverage beyond what was inspected.

**6. The fidelity checker may not be the person whose dissent is at stake. (Gap.)**
- Location: `design.md` ("a reviewer checks the synthesis"); NASA's source has the dissenter's own representative approve the account.
- Trigger: the checker may be a majority-view reviewer.
- Consequence: the checker cannot confirm that a preserved dissent is stated correctly.
- Recommendation: each preserved dissent is confirmed by its originator, with no veto. Also say that the moderator's own substantive claims are labelled and challengeable. That is currently only in the research.

**7. Unanimity has no verification rule. (Design judgment.)**
- Location: `design.md` ("may converge on the same recommendation"); correlated-errors evidence in `model-diversity-research.md`, section 2.
- Trigger: all reviewers agree, including on the same lens. About 60% of paired wrong answers coincide in the cited study.
- Consequence: headcount is rejected in principle, but nothing in the mechanics acts on it. Agreement on a high-consequence item is accepted as is.
- Recommendation: for consequential unanimous items, require an evidence trace and a stated "what would make the opposite true". Do not manufacture dissent.

**8. Progress-based stopping has no operational test, and the evidence against long debate is only partly absorbed. (Judgment.)**
- Location: `design.md` ("Discussion depth"); Debate-or-Vote results in `model-diversity-research.md`, section 3.
- Trigger: the "identifiable expected contribution" is the moderator's own call. There is no host budget by default, no agenda-size bound, and the number of items is unbounded. The cost is items × 3 reviewers × exchanges.
- Consequence: runaway cost, and revisions driven by conformity. The research found debate rounds lowering accuracy. The design asks that revisions be recorded with reasons but does not require them to cite new evidence or a rebuttal.
- Recommendation:
  - Require each revision to name the evidence or argument that caused it.
  - Treat "changed to agree" without that as unexplained.
  - Cap agenda items at a stated reporting level, or require ranking by consequence and explicit deferral of the tail.
  - Defaults can be set without a hard round cap. The "no numeric ceiling" choice is the user's, but the consequence is cost and conformity risk.

**9. "No material concern" is only in the research. (Gap.)**
- Location: the allowance is in `model-diversity-research.md`, section 5; `design.md` has "do not manufacture conflict" only.
- Trigger: the overcorrection study shows requiring objections raises false rejection.
- Consequence: council's thesis covers both over- and under-engineering. A change that is proportionate as built needs a valid outcome, like the `assess-maintainability` quiet result.
- Recommendation: state it in the design, per item and per run.

**10. The "genuine subagent" requirement is undefined, and statelessness is unaddressed. (Unresolved question.)**
- Location: `design.md` ("genuine host-provided subagents"; "In every delegated discussion turn, supply…").
- Trigger: a host that can only run fresh one-shot subagents, or can't make them read-only or give them separate contexts.
- Consequence: re-supplying state each turn is sound. But then "private preparation" and "objector responds" mean fresh instances working from stored records. The skill should say that is acceptable and that the initial records are the only continuity.
- Recommendation: define "genuine" as separate context with no access to peers' output, read-only on repo evidence, and inability to shape the host's answer. State that the read-only guarantee is a prerequisite, or disclose its absence.

**11. Evidence changing mid-review is acknowledged but unspecified. (Gap.)**
- Location: `design.md` ("Pin the comparison and current evidence at the start; disclose… changes").
- Trigger: working-tree input, or a user edit or push while reviewers read a shared tree.
- Consequence: reviewers see different evidence, which the research already names as a disagreement confounder.
- Recommendation: pin a commit range, or a snapshot or patch for working-tree input. Log each reviewer's inspected evidence. Reopen affected items when the evidence changes.

**12. Plugin packaging is wider than "add a skill". (Demonstrable.)**
- Location: `plugins/architecture/plugin.json`, `.codex-plugin/plugin.json`, `README.md` lines 11, 23 and 123; `agents/openai.yaml`.
- Trigger: both manifests describe maintainability from Git evidence only, and the README lists the plugin as "first skill `assess-maintainability`".
- Consequence: stale descriptions, keywords and default prompts. "Explicit invocation only" also needs host metadata (`disable-model-invocation` and the Codex equivalent). The existing skill has no such metadata to copy.
- Recommendation: widen the plugin descriptions and README. Add per-host invocation metadata and check it per host. Do not link into `docs/council/` from the skill, because installed plugin copies won't have it. Ship the reference guide inside the skill.

## Defensible dissent

- **End-only user interaction.** The decision is deliberate, and the research already says no early-interruption exception is implied. The consequence is that agenda items built on guessed scenarios cost three reviewers' work. A cheap alternative is to accept optional constraints at invocation and allow one batched pre-agenda question only when the change shows no discoverable requirement and the host is interactive. I am not claiming error. Horvitz supports asking when consequential uncertainty exists.
- **Assigned emphasis plus model diversity.** The decision is deliberate, but the research itself warns that combining them confounds attribution. A pilot cannot say which dimension helped unless it also varies lens alone.
- **Dropping the separate independent-review mode.** Defensible. The consequence is lost breadth: no reviewer sweeps the change for omissions. Finding 5 recommends reporting coverage honestly, not restoring a mode.

## Resolve before writing the skill versus pilot

**Resolve first (1–4, 6, 9, 10, 12):**
- Moderator authorship and relay topology.
- Emphasis versus driver vocabulary.
- The fact / premise / priority rule.
- Dissent confirmation by the originator.
- "No material concern" as a valid outcome.
- The definition of "genuine subagent".
- Packaging, invocation metadata and the README.

**Pilot instead (5, 7, 8, 11):**
- Agenda coverage and the unanimity rule.
- Stopping and conformity.
- Mid-review evidence changes.
- Whether a lens-only variation shows benefit.
- Moderator-quality comparison on a cheap host.

## Source and packet coverage

Read in full: `design.md`, `GLOSSARY.md`, all six research notes, `AGENTS.md`, `GLOSSARY-MAP.md`, the architecture plugin and both manifests, the marketplace entry and `README.md` sections. I did not read `review-round-1-brief.md` beyond noting it exists.

- **Verified live:** arXiv pages for 2603.28914, 2504.20781 and 2508.17536 exist with the cited titles. The ReConcile StrategyQA figures (79.0 vs 72.2) match. The two SEI PDFs return 200.
- **Not verified:**
  - The Debate-or-Vote Table 1 numbers (not extractable from the HTML).
  - The doi.org link (HTTP 403).
  - The vendor model pages (GPT-6.1 Sol, Luna, Opus 5.5).
  - The NASA and Reuters pages.
  - The Matt skills pinned commit.
  - The TOSEM publication date.
  - The 2603.28914 student-group counts.
- **Not located:** `validate_plugin.py` and `update_plugin_cachebuster.py` (my filesystem search timed out), so I could not confirm the validation flow.
- **Not inspected:** the installed Matt skill paths.

Model names and capabilities should not be hard-coded into the skill. They age, and the moderator-selection note correctly treats them as unestablished.


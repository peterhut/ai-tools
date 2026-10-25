# Response to council plan review, round 1

Status: the clarifications below were incorporated into [the plan](design.md) at the user's request after round one and subsequently implemented after separate authorization. The user's previously settled choices remain authoritative. See [the full review](review-round-1.md), [its complete brief](review-round-1-brief.md), and [validation limits](validation.md).

Reviewer: GitHub Copilot / Claude Sonnet 5.5. This was an independent read-only design review, not a complete independent verification of every cited source.

## Clarifications incorporated before authoring

1. **Reviewer emphasis versus system requirements (finding 3).** Use the canonical term council perspective, described as an assigned emphasis. Reserve quality drivers for the system's evidenced needs, constraints, and stakeholder priorities. Assignment of a reliability perspective does not establish a reliability requirement or grant it more weight than the actual context warrants. Keep general ATAM definitions in the skill's principle/reference material rather than expanding the domain glossary with every standard architecture term.

2. **Classify the unresolved issue (finding 4).** A factual disagreement concerns inspectable behavior or evidence and calls for investigation. A missing user premise is a consequential requirement or preference the available evidence cannot establish; suspend the affected conclusion and preserve useful conditional branches. Substantive dissent can remain when reviewers share facts and constraints but assess a real tradeoff differently. Documented drivers are evidence, not immunity from the agreed ability to challenge requirements; reopening an accepted ADR still requires changed context or assumptions.

3. **Make coverage and a quiet outcome explicit (findings 5 and 9).** Private preparation should identify the evidence inspected and any consequential agenda addition/challenge, or state none. The readout must disclose material exclusions, deferrals, and inspection gaps. No material concern is valid per item and per run. These are small output requirements, not a separate complete-review mode or another meeting phase.

4. **Check dissent with its originator (finding 6).** For each material dissent retained in the account, give its originating reviewer a narrow fidelity check; that reviewer can correct a distortion without vetoing conclusions. If unavailable, preserve the recorded objection and disclose the unchecked portrayal rather than dropping it or blocking indefinitely. Label substantive claims introduced by the moderator and expose them to the same evidence-based challenge.

5. **Operationalize evidence and progress (findings 7 and 8).** Every material conclusion needs supporting evidence and relevant assumptions, including unanimous conclusions. Private positions should include the meaningful alternative or condition that would change the assessment. A revision must identify the evidence, rebuttal, or changed premise that caused it; convergence alone is not an explanation. Rank agenda items by consequence and dependency and disclose material deferrals when an actual budget constrains coverage. Preserve progress-based stopping and the user's rejection of an additional fixed numeric ceiling; do not add a special mandatory unanimity round or force dissent.

6. **Clarify execution and continuity (findings 2 and 10).** The main thread can dispatch both the moderator and reviewers as siblings. A fresh moderator handles substantive facilitation and synthesis and returns the next requested exchanges; the host relays the relevant source records and instructions faithfully, without needing nested spawning. Stateful and fresh one-shot subagents are both acceptable: a logical reviewer continues from its recorded position, evidence, objections, and replies, and any changed model or missing continuity is disclosed. Genuine subagents means actual separately executed agents with separate contexts; initial preparation does not receive peers' conclusions. Read-only is the review task boundary, with host enforcement used where available. Do not invent a universal sandbox guarantee or require reviewers to be incapable of influencing the host's answer, since contributing to the account is their purpose.

7. **Pin evidence and finish packaging (findings 11 and 12).** Define the evidence identity before authoring: immutable commit references or a stable snapshot/patch for working-tree input, with relevant inspected evidence recorded. If evidence changes, qualify the affected conclusions and reassess the affected items rather than merge inconsistent versions. The implementation should refresh plugin descriptions and relevant README material, set explicit-invocation metadata on supported hosts, ship references inside the skill package, and use relative installed-package paths. Updating both manifests, the Codex cachebuster, and validation remains mandatory. Snapshot handling belongs in the initial contract; its effectiveness can then be tested in a pilot.

## Concern to acknowledge without changing the agreed default

**Moderator prior involvement (finding 1).** Disclose authorship or prior substantive involvement and make the existing fresh-moderator option prominent in that case. This is a plausible risk, but the cited self-preference research does not establish that a main-thread author must not moderate an architectural review or that a fresh moderator eliminates bias. The primary paper studies model self-recognition and preference for its own generated text; the original research note explicitly limits transfer to code architecture. Keep the main-thread default unless the user chooses to change it. A fresh-context comparison can be evaluated, without conflating context freshness with a different model's capability.

Primary source checked for this response: [Panickssery et al., NeurIPS 2024](https://proceedings.neurips.cc/paper_files/paper/2024/hash/7f1f0218e45f5414c79c0679633e47bc-Abstract-Conference.html).

## Deliberate choices retained

- User interaction stays after the internal council; no intermediate clarification exception is added.
- Optional invocation context can be supplied by the user without creating an obligatory setup interview.
- Assigned quality-attribute emphases and optional model-family diversity remain complementary. A pilot must isolate conditions before attributing benefits to one dimension.
- No separate full independent PR-review mode is introduced.
- No small fixed debate limit or additional numeric ceiling is introduced.
- The output remains recommendations and eligible proposed ADR drafts, not implementation changes.

## Validation implications

The forward-test should assess these behavioral invariants in realistic small fixtures, not count headings or require a finding quota. Source checks that the reviewer did not complete should remain labeled as such; its successful plan review is not evidence that every host, source, model pairing, or runtime mechanism works. The repository's required helper paths still need to be located during implementation before claiming the plugin release flow has been validated.

## State after the plan update

The user first instructed the main thread to update the plan, then separately authorized implementation. The clarifications were incorporated while retaining the agreed defaults; validation records what was exercised. The external review itself did not authorize implementation. A further external review must be a new delegated task containing the original brief, the review, these responses, and any unresolved objections.


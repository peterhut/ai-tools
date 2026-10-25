# Agenda-led council: bounded process verification

Research date: 2026-10-25. Design input only; does not settle Q15 or implement council. Read alongside [moderation research](moderation-research.md) and [model-diversity research](model-diversity-research.md). This follow-up examines the proposed sequence: moderator pre-review → provisional agenda → independent reviewer exploration and positions → explain and debate each item → moderator account to the user at the end.

## Finding and evidence boundary

An amendable agenda followed by independently prepared positions and focused exchange is a defensible candidate. The sources below supply human process precedents, not evidence that this AI adaptation improves PR review. They do not establish that every reviewer must first perform a complete independent PR review. Independent preparation, breadth of inspection, and completeness of coverage are separate choices.

The existing benchmark research gives reasons to preserve initial assessments and evaluate discussion separately. It does not compare complete independent PR reviews against agenda-led exploration, so it cannot decide between them. No additional empirical study is needed for this bounded verification.

## Three verified primary sources

### SEI: preliminary analysis, then broader scenario generation

Mario R. Barbacci, *SEI Architecture Analysis Techniques and When to Use Them*, CMU/SEI-2002-TN-005 (October 2002), §§3.1–3.2, printed pp. 11–13; §4.3, p. 17. [Primary report](https://sei.cmu.edu/documents/1990/2002_004_001_13946.pdf#page=21).

**Supported sequence:** Phase 1 presents the method, customer business drivers, and the architect's architecture overview; evaluators identify architectural approaches; participants build and prioritize a utility tree; evaluators analyze approaches against it. Phase 2 first recaps those results. Wider stakeholder brainstorming then generates and prioritizes scenarios explicitly unrestricted by the initial tree's quality attributes. Analysis continues against these scenarios before results are presented. Section 4.3 describes possible tailoring with mostly developers in Phase 1 and different stakeholder groups in later Phase 2 sessions. [SEI, §§3.1–3.2 and 4.3](https://sei.cmu.edu/documents/1990/2002_004_001_13946.pdf).

**Limit and adaptation:** this is team preparation informed by customer and architect input, not one moderator privately deciding the agenda. Broader scenario generation can expand the evaluation focus; calling that an amendable council agenda is our interpretation. ATAM does not specify private reviewer positions or complete independent PR reviews. Replacing stakeholder participation with models, and deferring the user until the end, are additional adaptations.

### IETF: answer objections rather than accumulate agreement

Pete Resnick, *On Consensus and Humming in the IETF*, RFC 7282 (June 2014), introduction and §§2–6. It is Informational and explicitly does not change IETF procedures. [Primary RFC](https://www.rfc-editor.org/rfc/rfc7282.html).

**Supported handling:** elicit the reasons for disagreement; examine objections and consequences; explain why an objection is accommodated or why another tradeoff outweighs it. Rough consensus can coexist with continuing dissent, but an unanswered substantive technical objection cannot be erased by many supporters. Silence or capitulation does not answer an objection. A chair needs technical judgment to assess the response. [RFC 7282, §§2–3](https://www.rfc-editor.org/rfc/rfc7282.html#section-2), [§6](https://www.rfc-editor.org/rfc/rfc7282.html#section-6).

**Limit and adaptation:** the RFC supplies principles, not the proposed stage order or a required turn count. Council can borrow explicit objection-and-response tracking; its moderator has no inherited authority to decide project priorities or approve a PR. A council discussion may end with an unresolved objection, provided the report identifies it rather than claiming consensus.

### NASA: preserve competing accounts and distinguish the decision

*NASA Space Flight Program and Project Management Requirements*, NPR 7120.5F, Chapter 3, §§3.4.1–3.4.5, inspected with Change 4. [Primary directive](https://nodis3.gsfc.nasa.gov/displayDir.cfm?Internal_ID=N_PR_7120_005F_&page_name=Chapter3).

**Supported record:** when time permits, disagreeing parties jointly document agreed facts, differing positions, rationale, impacts, and recommendations, with each view's representative approving the account. Management's disposition is separately documented and provided to the dissenter. Agreement, disagreement with willingness to support, and Formal Dissent are distinct states. [NASA, §3.4](https://nodis3.gsfc.nasa.gov/displayDir.cfm?Internal_ID=N_PR_7120_005F_&page_name=Chapter3).

**Limit and adaptation:** a council fidelity check can borrow faithful representation while allowing correction of a missing objection. Giving reviewers a narrow correction opportunity rather than NASA's approval rights is a deliberate departure; it is not an editorial veto. NASA's institutional escalation and decision authority are not transferred to agents.

## Candidate stage and turn protocol

Everything below is a proposed AI procedure, not an adopted workflow or a source-prescribed protocol. Carry forward the user's constraints: default three reviewers from multiple capable model families with disclosed fallback; main-thread moderator by default, with a fresh moderator option especially for a cheaper host; recommendations and eligible ADR drafts only.

1. **Moderator preparation.** Pin the implemented change and relevant repository context. Inspect requirements, code, existing rationale, and known user concerns. Produce a provisional agenda of consequential questions, each with a concrete scenario, evidence links, known constraints, and unknown premises. Label inferred requirements. Keep the moderator's preferred answer out of the initial questions where possible; its assumptions remain visible and challengeable. Record any prior involvement in the change.

2. **Independent exploration and agenda amendment.** Give all three reviewers the same scope, evidence access, broad remit, and provisional questions, without peers' positions or the moderator's proposed answers. Each explores enough surrounding code and documentation to derive its own position, including whether the question or requirement is wrong. Require an explicit opportunity to add or replace agenda items and report important coverage gaps. Do not require three complete written PR reviews. Each returns a concise initial record: claim, inspected evidence, assumptions, consequences, alternative, and what would change its assessment; “no material concern” is permitted. Preserve these records before exchange.

3. **Publish the working agenda.** The moderator merges duplicates, retains distinct premises, records additions and deferrals with reasons, and orders items by consequence and dependency. Every reviewer can challenge an omission. For a newly added item, reviewers privately prepare an initial position before seeing peers' answers to it. If a time budget prevents preparation or discussion, mark that gap; do not imply full coverage.

4. **Run these turns for each item.**
   - **Explain:** reveal each prepared position and its evidence before rebuttals. Nobody must defend a predetermined side. A reviewer may agree or have insufficient evidence.
   - **Challenge:** the moderator identifies the strongest material objection and asks a named reviewer to address that exact claim, including its requirement or premise. Others may add a distinct objection, not repeat a vote.
   - **Answer and revise:** the respondent supplies evidence or a tradeoff argument; the objector then states whether that answers the concern and why. Record revisions with their cause. A moderator's substantive counterclaim receives the same scrutiny.
   - **Disposition:** distinguish a factual resolution, conditional recommendation, unresolved substantive dissent, missing user premise, or deferred investigation. Consider another targeted exchange only when a specific unanswered argument or obtainable evidence could change the outcome. These turns are a candidate structure, not a settled round or cost limit. Exhausting the budget leaves an open item, not agreement.

5. **Check fidelity and report to the user.** Have a reviewer check the draft synthesis for material omissions and misrepresentation. Preserve consequential unresolved objections and explicit inspection gaps. Present a concise narrative of the main tradeoff, decisive evidence, changes in position, and meaningful dissent before recommendations. End with the missing user premises and preference choices, each paired with conditional consequences. Under the proposed end-only interaction, do not invent answers or repeatedly debate an unknown user priority. If an answer is essential, the affected recommendation remains conditional or withheld until the user responds.

An eventual user answer can trigger focused reconsideration of affected items. Reopen an accepted ADR only for changed context or assumptions, identifying that change; an alternative preferred by a reviewer alone is insufficient. Offer an ADR draft only under the existing all-three test: meaningful reversal cost, surprising without context, and a genuine tradeoff.

## Classify the disagreement before choosing the next turn

- **Factual disagreement:** incompatible claims about inspectable behavior, contracts, or evidence. Inspect or reproduce where appropriate; record what is established and what remains uncertain. Headcount cannot establish the fact.
- **Missing user premise:** an undocumented promise, workload expectation, or requirement that the repository cannot establish. Search available evidence first; then preserve explicit branches and ask at the final user handoff. An agent's inference is not a user answer.
- **Value preference:** competing priorities under shared facts, such as operational simplicity versus recovery guarantees. Explain consequences and recommend with stated conditions; preserve defensible dissent for the user to weigh. Do not relabel a priority as a factual defect.
- **Artifact preservation:** a separate output obligation across all three cases. Keep enough of the initial claims, evidence, revisions, responses, and final dispositions to explain the recommendation without requiring full meeting minutes. Keep accepted rationale distinct from proposed or inferred rationale. New draft files are allowed and may be discarded if the user declines them; this is not permission to delete existing documents or accepted ADRs.

## Worked item: retry queue (entirely fictional)

This example invents a PR, files, behavior, positions, and evidence; none describes this repository.

**Preparation:** a fictional PR adds an in-memory retry queue. The moderator asks, “For a worker crash after a failed delivery, what recovery guarantee justifies persistence?” Its tentative concern is queue loss, not an established requirement.

**Independent positions:** reviewer A traces enqueue and restart paths and concludes queued work is lost; it recommends persistence if accepted work must survive restart. B independently traces a fictional durable upstream job ledger and initially thinks replay makes local persistence redundant. C reads the fictional API contract, finds no recovery-time promise, and adds a missing premise: whether eventual upstream replay is sufficient. All three prepare before reading peers' conclusions.

**Explain, challenge, answer:** B cites a fictional replay test. A points out that the test covers unfinished jobs but does not bound recovery delay. B verifies that distinction and revises: upstream replay prevents permanent loss, but timely recovery is unproven. C notes that local durability also brings storage and recovery complexity. No one needs to invent a guaranteed latency requirement to sustain the discussion.

**Disposition and final handoff:** the factual claim narrows from “work disappears permanently” to “local queued state disappears; upstream replay can recover work; recovery delay is unspecified.” The missing user premise is the required delay after restart. The value choice is whether faster recovery warrants durable local state. A conditionally favors persistence; B favors the simpler queue if delayed replay is acceptable; C withholds an unconditional recommendation pending the delay requirement. The moderator reports those conditions and dissent to the user at the end. The item record preserves A's original concern, B's correction, the cited fictional evidence, and the unanswered premise. Any ADR remains proposed and subject to the eligibility test; no implementation follows automatically.

## Caveats that remain design choices

A common agenda already frames reviewer attention: private positions are independent of peers' conclusions, not independent of the moderator's framing or statistically independent. Neutral questions, amendment rights, and retained initial records are proposed safeguards, not validated cures for anchoring. They can still miss issues the moderator and all reviewers overlook. Complete independent reviews may broaden discovery at additional cost; this evidence does not quantify that benefit or make them mandatory.

Deferring the user reduces interruptions but can spend reasoning on the wrong premises. Conditional closure contains that cost without supplying stakeholder priorities through models. Whether this tradeoff, the inspection breadth, and the exchange budget are acceptable remains for the design interview, including Q15.

# Council implementation plan

Status: first implementation completed after the GitHub Copilot / Sonnet 5.5 [round-one review](review-round-1.md) and the user's authorization. See the [validation record](validation.md) for actual checks, observed failures, corrections, and remaining limitations.

## Purpose and scope

Council is an explicitly user-invoked, lightweight, retrospective ATAM-inspired review of an implemented change. It recovers consequential design decisions and asks whether their benefits justify their costs in the system's actual context. The core is quality-attribute analysis and proportionality; deliberation serves that analysis.

Inputs are an implemented PR, branch or commit range, or working-tree change. Council may challenge requested behavior and requirements, while distinguishing that advice from compliance with the current specification. A separate full independent PR-review mode is outside this plan. Reviewers inspect relevant evidence and challenge agenda omissions without promising exhaustive correctness, standards, or change-wide coverage.

Outputs are a concise council account, recommendations, and eligible proposed ADR files. Invoking council does not authorize implementation changes, acceptance of ADRs, approval or merging of a PR, or unrelated external actions.

## ATAM concepts and council perspectives

For each consequential choice, recover the architectural approach, affected quality attributes, concrete scenarios, assumptions, and plausible alternatives. Distinguish observed effects, documented rationale, inferred rationale, and unknown author intent.

Use these concepts consistently:

- **Quality drivers:** the system's evidenced quality requirements, constraints, and stakeholder priorities. Missing drivers remain explicit.
- **Quality-attribute scenario:** a concrete situation, relevant conditions, and required response against which to assess a design. Use an observable response where supported; label hypothetical branches and unknown thresholds rather than inventing requirements.
- **Tradeoff point:** a consequential design choice affecting competing qualities and their costs.
- **Sensitivity point:** a decision or parameter whose change can materially affect an important quality.
- **Council perspective:** an assigned emphasis on particular attributes and countervailing costs during review. It is not a stakeholder priority or a prescribed conclusion.

Assign complementary perspectives per agenda item before private preparation. For a retry mechanism these might concern reliability/availability, performance/resource use, and modifiability/operational simplicity. Select different attributes when the change warrants them, including security where relevant. Every reviewer still considers cross-attribute consequences and overall fitness. A reliability perspective can conclude that added durability is excessive; a simplicity perspective can conclude that its cost is justified.

Model diversity and perspective diversity are complementary. Prefer different capable model families when available, with a disclosed same-model fallback. Use a straightforward roster and explicit perspectives, with per-run user overrides. There is no built-in family-to-attribute mapping, elaborate assignment procedure, learned routing, scoring system, mandatory rotation, voting, or numerical confidence scheme.

The test is whether required quality is achieved at a proportionate complexity, operational, and resource cost. Maximizing each attribute independently is not the objective. A proportionate implementation, no material concern, or no recommendation is a valid outcome per item and per run.

## Participants and execution

The default is three genuine subagent reviewers and main-thread moderation. A fresh moderator is available when requested, especially for a cheaper host model or when the host authored or previously advocated the implementation. Disclose relevant authorship or prior substantive involvement in the readout. Keep the agreed main-thread default; the self-preference evidence does not establish that a fresh moderator is universally necessary or eliminates bias.

The roles are:

- **Moderator:** frames the agenda and perspectives, directs substantive exchanges, identifies unanswered objections, assesses progress, and writes the account. Its own substantive claims are labelled and open to the same evidence-based challenge.
- **Reviewers:** independently investigate, form positions, challenge claims and omissions, answer objections, and identify what would change their assessment.
- **Dispatcher:** the main thread invokes agents and relays instructions and source records. When moderation is delegated, the fresh moderator returns requested next exchanges and synthesis; the host dispatches the moderator and reviewers as siblings. Nested spawning is not required.
- **User:** commissions the review, supplies actual priorities or missing requirements when needed, and retains decision authority.

Keep the workflow portable across T3, GitHub Copilot, Cursor, or another capable host. Discover actual delegation, model, and option capabilities at execution time; honor explicit choices and report substitutions. Prefer T3 orchestration when available and appropriate, without making it a prerequisite or hard-coding model names.

Genuine subagents are actual separately executed agents with separate contexts, not personas narrated by the host. Initial preparation receives the shared factual brief and assigned perspective without peers' conclusions. Use clean briefs and avoid inherited peer conclusions during that preparation. The task boundary is read-only investigation of repository evidence; use host enforcement where available and describe relevant limitations accurately rather than promising a universal sandbox guarantee. If genuine separate subagents are unavailable, report that council cannot run.

Stateful and fresh one-shot subagents are acceptable. Continuity belongs to the logical reviewer's preserved position and evidence, objections, replies, and revisions; supply those records on subsequent turns. Disclose a changed model or missing continuity rather than implying unchanged participation. On T3, each delegated discussion round uses a new delegated task with the complete relevant brief and prior state, following the host's current orchestration contract.

## Review procedure

### 1. Establish stable evidence and propose the agenda

Pin the baseline and target to immutable commit references. For working-tree input, preserve the baseline, tracked patch, and relevant untracked contents in a stable snapshot or equivalent evidence record; a branch name or mutable tree alone is not the reviewed revision. Avoid disturbing the user's source worktree.

Read relevant requirements, glossaries and context maps, existing ADRs, surrounding code, and supplied concerns. Identify consequential choices and scenarios, then propose an amendable agenda with evidence, actual quality drivers, unknown premises, and competing consequences. Give reviewers the common facts without prescribing preferred answers.

Record the evidence identity in review briefs and what each reviewer inspected. If the source changes during review, distinguish the versions, qualify affected conclusions, and refresh or reopen affected items before claiming they apply to the new evidence. Do not combine incompatible snapshots into one finding.

### 2. Prepare private positions and amend the agenda

Give each reviewer the common brief and a distinct relevant council perspective. Each independently inspects sufficient evidence and records:

- Its assessment, supporting evidence, assumptions, and cross-attribute consequences.
- A meaningful alternative or condition that would change its assessment.
- Inspected coverage and any consequential agenda addition, challenge, or gap; stating none is valid.

Preserve these positions before revealing peers' conclusions. For a newly added item, obtain private preparation before its debate. The moderator merges duplicates while retaining different premises, ranks items by consequence and dependency, and records reasons for material exclusions or deferrals. Amendment reporting is a preparation output, not a new full-review phase.

### 3. Explain, challenge, answer, and record the disposition

For each item, reveal prepared positions and identify the consequential differences. Direct a concrete objection to a reviewer who can answer it, return the reply to the objector for response or revision, and involve others when they have a distinct contribution. Agreement does not require a manufactured debate.

Classify the unresolved issue before choosing the next action:

- **Factual disagreement:** incompatible claims about inspectable behavior, contracts, or evidence. Investigate and record what is established and what remains uncertain.
- **Missing user premise:** an outcome-changing requirement or preference that the available evidence cannot establish. Suspend the affected conclusion, preserve useful conditional branches, and name what information is missing and why it matters.
- **Substantive dissent:** a real difference in assessment despite shared facts and established constraints. Explain the competing consequences and preserve the defensible disagreement.

Documented requirements remain evidence rather than immunity from challenge. Reopening an accepted ADR still requires identifying a changed context or assumption.

Each material conclusion, including a unanimous one, identifies its evidence, relevant assumptions, and meaningful alternative or countercondition. Agreement counts are not verification. Each revision names the evidence, rebuttal, or changed premise that caused it; an unexplained move to agreement is not an evidentiary resolution.

Before another exchange, name the material unresolved claim and what another contribution could change. Continue through multiple steps while developing evidence, answering objections, exploring feasible alternatives, or revising assessments for stated reasons. Stop when exchanges repeat without substantive development, when the remaining disagreement is an understood tradeoff, or when missing user information prevents progress. Suspend affected items and continue independent ones.

There is no small fixed round limit or additional numeric ceiling. Respect explicit caller and host budgets; when they constrain coverage, prioritize consequential items and disclose material deferrals and unfinished analysis. A budget ending does not establish agreement or completion.

### 4. Synthesize and check fidelity

Write a concise, significance-first account of the central tradeoffs, strongest arguments, decisive evidence, why positions changed, and meaningful remaining dissent. Keep this account distinct from the recommendations and the user's eventual decisions. Full meeting minutes are not required.

A reviewer checks for material omissions and misrepresentation. Additionally, give the originator of each material retained dissent a narrow check of its portrayal. Corrections concern fidelity, not a veto over conclusions. If the originating reviewer is unavailable, preserve the recorded objection and disclose that its portrayal was not confirmed; do not erase dissent or block indefinitely.

Keep supporting claims traceable to reviewed evidence. Include material inspection gaps, exclusions, deferrals, and suspended items with reasons, without overwhelming the narrative. A suspended item should still explain useful conditional outcomes rather than becoming an unexplained question.

### 5. Give the readout and request only necessary clarification

The internal council finishes before the moderator's user-facing readout. Present the account before recommendations, then ask only the currently answerable user decisions or missing premises, explaining their consequences and the moderator's recommendation where possible.

The user stays outside internal deliberation. There are no intermediate clarification requests or obligatory setup interview; the user may supply constraints at invocation. Use grilling's question-dependency discipline when needed, without requiring the user to replay the debate or adjudicate every disagreement. If nothing needs clarification, the review can end with its report.

An answer that selects an already analyzed branch updates the recommendation directly. An answer that materially changes a premise can trigger targeted reconsideration of affected items. Preserve outstanding dissent and uncertainty after the user's decision where they remain relevant.

## Rationale and artifacts

Use Matt Pocock's ADR eligibility policy: all three must hold—meaningful reversal cost, surprising without context, and a genuine tradeoff among alternatives. Missing a functional requirement alone does not establish eligibility.

Follow the concise context/decision/reason format and the repository's established ADR location and numbering. Unconfirmed rationale stays proposed and distinguishes observation, inference, and unknowns. Draft files are permitted; council-created drafts may be removed when the user declines follow-up. This does not authorize deleting pre-existing documentation or treating saved drafts as accepted decisions.

The report stays in the conversation by default; save it when requested. Preserve enough substantive review state to explain conclusions and resume affected items, without requiring a full transcript or a new reporting system.

## Integration, packaging, and validation

Package `council` beside `assess-maintainability` in the existing `architecture` plugin. Reuse Matt's relevant installed disciplines where available while retaining self-contained, attributed essentials. Neither his entire collection nor another complete review workflow is required.

Keep `SKILL.md` concise, with focused deliberation and host guidance and an attributed primary-source guide inside the installed skill package. Link using package-relative paths; installed skills must not depend on this repository's `docs/council/` research files or personal absolute paths.

Update plugin descriptions and relevant README material to cover both maintainability assessment and council. Set explicit-user-invocation metadata for supported hosts. The historical Codex validator conflicts with the other hosts' frontmatter flag, so the implementation uses a Codex policy and a thin Copilot/Cursor wrapper loading the same instructions; the plugin remains self-contained. Adding a skill is a minor plugin release: update both manifests to the same base semantic version, use the required cachebuster helper for the Codex manifest, and validate the plugin. Keep marketplace metadata versions unchanged unless the marketplace definition changes. The recovered official helper revision and executed commands are recorded in validation.

Validate skill structure, local links, invocation metadata, and manifests. Use an independent forward-test on small isolated fixtures to assess meaningful behavior: scenario-based tradeoffs and proportionality, real objection/reply/revision handling, missing-premise suspension and conditional outcomes, quiet results, dissent fidelity, and evidence identity. Exercise fallback behavior where feasible. A fixture is not proof of usefulness across real PRs, hosts, or model combinations; report what was and was not tested.

During later evaluation, compare useful grounded considerations, unsupported objections, missed tradeoffs, user effort, cost, and latency against a capable single reviewer. Separate model diversity, assigned perspectives, discussion, and context freshness before attributing benefits. Do not introduce that evaluation machinery into ordinary council runs.

## Research and review record

Retain primary sources in the final skill with what each source supports and the limits of transfer. This is not full ATAM, and none of the selected evidence validates the complete proposed real-PR multi-model workflow.

- [Model diversity and model-to-perspective fit](model-diversity-research.md): empirical studies, pstack precedents, direct ATAM-related evidence, and limits on family-specialty claims.
- [Matt skills integration](matt-skills-integration-research.md): pinned upstream sources, installed differences, reusable disciplines, and ADR conventions.
- [Moderation and reporting](moderation-research.md): SEI, IETF, NASA, and Reuters principles, distinguished from AI adaptations.
- [Moderator model selection](moderator-model-selection.md): dated first-party guidance and candidate evaluation criteria, not permanent model rankings.
- [Agenda-led process](agenda-led-process-research.md): focused preparation, agenda amendment, objection/reply mechanics, and the limits of human-process analogies.
- [User interaction](user-interaction-research.md): attention burden, waiting latency, delayed-clarification costs, and no direct proof that end-only interaction is faster.
- [Original prompt and review brief](review-round-1-brief.md), [full round-one review](review-round-1.md), and [dispositions](review-round-1-response.md): historical inputs and the responses incorporated in this revision.

The external reviewer did not independently verify every cited source or the repository helper flow. Preserve those limitations instead of treating plan review as source or runtime certification.

## Implementation state

The plan incorporates the requested review clarifications while retaining the agreed main-thread moderator default with a fresh option, end-only user clarification, optional model diversity, mandatory genuine subagents, assigned council perspectives, no separate full-review mode, and progress-based stopping without a fixed numeric ceiling.

The user subsequently authorized implementation. The skill, shared references, per-host entrypoints, plugin metadata, and README have been created or updated for plugin 0.3.0. Structural checks, an integrated trial, and focused follow-up checks are recorded. The integrated trial overran its budget and was not a complete end-to-end pass; deadline/probe guidance was corrected, a focused R3 fidelity review completed, and no-delegation/expired-deadline checks passed. Consult the validation record rather than treating this plan as proof of general effectiveness or cross-host runtime support.

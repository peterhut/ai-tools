# Deliberation records and turns

Read when starting item discussion or resuming a suspended item. The structure below
is a compact working brief, not a mandatory user-facing form or a finding quota.

## Item and private preparation

An agenda item identifies the inferred choice, quality drivers, affected attributes,
scenario/unknown requirement, relevant evidence, and a meaningful alternative.
Record observed behavior separately from inferred rationale or historical intent.

A private task brief can say:

> Assess item A against evidence snapshot S at the supplied paths. Your council
> perspective is P. Evaluate fitness against the shared quality drivers and
> scenario, including cross-attribute costs and simpler alternatives. Inspect
> relevant evidence read-only. Return your assessment, evidence, assumptions,
> strongest alternative or condition that changes your assessment, inspected
> coverage, and agenda additions/challenges or none. No peer positions are supplied.
> A proportionate implementation or no material concern is a valid result.
> Investigate inspectable facts; preserve unknown user premises for the moderator.
> Do not change implementation files, ask the user, or dispatch additional agents.
> This is a council tradeoff assessment, not a code-quality or standards review;
> do not invoke other review skills unless the moderator names one.

The moderator assigns emphasis, not a position to defend. Capture private positions
before sharing peer results. Agenda amendment is an explicit preparation output;
it does not require another complete change-wide review.

## Exchange

For each material tension:

1. **Explain:** reveal the prepared positions and evidence.
2. **Challenge:** identify a concrete consequential objection and a respondent.
3. **Answer:** the respondent investigates or explains the tradeoff; return its
   answer to the objector for response, concession, revision, or reasoned dissent.
4. **Disposition:** record what is established, what remains disputed, and what
   evidence or user premise would change the assessment.

A continuation brief includes the item and snapshot, common facts/drivers, the
recipient's preserved position and perspective, relevant peers' actual positions,
the objection and replies, changes and reasons, and the specific next contribution.
For example:

> R2 claims upstream replay prevents permanent loss, citing ledger.py and its
> replay test. R1 objects that acknowledged work may be marked complete before
> a failed delivery is queued. Investigate that acknowledgment window against
> snapshot S and answer R1's objection. Retain your earlier assessment separately,
> name any evidence/rebuttal causing a revision, and state remaining uncertainty.

Return the answer to R1. A new unrelated review is not a reply. A concession needs a
reason; repeated support or a majority does not verify a disputed claim. Any
substantive moderator claim follows the same process.

When a reviewer runs a probe or benchmark, retain the command, reviewed revision,
conditions, and result so another participant can trace the empirical claim.
Keep generated artifacts within the allowed scratch scope and configure temporary
paths for checks that write. An untraceable measurement is not an established fact;
withdraw it as support rather than presenting plausible numbers as evidence.

## Classification and completion

**Fact:** investigate incompatible claims about behavior, contract, or source evidence.
If evidence cannot be obtained, state that limitation rather than guessing.

**Missing user premise:** search the available requirements first. If an unstated
requirement or preference changes the outcome, suspend that conclusion. Record
useful conditional branches and the exact missing premise; continue other items.
An assigned emphasis does not supply the missing system requirement.

**Substantive dissent:** shared facts and constraints leave a defensible difference
in tradeoff assessment. Preserve its rationale and consequences, including what
would change it. Established requirements may be challenged as advice; an accepted
ADR is reopened only for a named change in context or assumptions.

Before continuing, name the unresolved claim and expected new contribution. After
the exchange, record what actually developed. Continue on material evidence, answers,
alternatives, or reasoned revisions; close repeated positions or understood dissent.
A user-only premise suspends the affected conclusion until the final readout.
Explicit resource limits end work with open issues honestly recorded, not consensus.

No material concern, justified complexity, conditional advice, substantive dissent,
and suspension are all valid results. All material conclusions need evidence and
assumptions, even when unanimous; preserve meaningful counterconditions rather than
creating an extra adversarial round merely because reviewers agree.

## Fictional example

A PR adds a durable retry queue. Its drivers do not document a restart guarantee.

- Reliability/availability preparation finds that local persistence survives a worker
  crash and conditionally supports it for independently durable accepted work.
- Performance/resource preparation finds that an upstream durable ledger can replay
  unfinished jobs and questions duplicate storage and writes.
- Modifiability/operations preparation identifies schema migrations and queue recovery
  procedures, and asks what recovery delay is acceptable.

The first objection concerns whether upstream replay covers acknowledged work.
Inspection resolves the acknowledgment path but finds no recovery-time promise.
One reviewer revises a permanent-loss claim in response to that evidence. Another
maintains that recovery delay could justify local persistence.

Close the factual issue. Suspend the unconditional recommendation on the missing
recovery requirement; preserve the conditional durability argument and its costs.
The readout explains the narrowed issue and asks about the required recovery behavior.
If the answer selects an analyzed branch, update directly; a changed contract can
reopen only this item. None of these invented facts describes the current repository.

## Fidelity and readout

A reviewer checks the draft account for material omissions. Each retained material
dissent's originator checks its portrayal, not the conclusion. Return distorted
claims for correction; lack of confirmation is disclosed without suppressing the
recorded dissent or blocking forever.

Keep scope/snapshot, actual roster, relevant prior involvement, and material coverage
gaps discoverable. Lead with the central tradeoffs, strongest arguments, decisive
evidence, and what changed; give recommendations and then necessary user questions.
Include material exclusions, deferrals, conditional branches, and missing premises,
without requiring the user to read chronological meeting notes.

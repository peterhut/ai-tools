---
name: council
description: Convene an ATAM-inspired council to assess an implemented change's quality-attribute tradeoffs and proportionality.
---

# Council

Run when the user explicitly requests council. Recover the consequential design
decisions in an implemented change and evaluate whether the required quality earns
its complexity, operational burden, and resource cost. Deliver recommendations and
eligible proposed ADRs; implementation changes are a separate task.

## 1. Prepare the review

**Execution.** Before dispatching, read [host execution](references/hosts.md) for
capability discovery, private contexts, fresh moderation, and turn continuity.
Use three genuine subagent reviewers by default. Prefer different capable model
families, with disclosed same-model fallback and explicit user overrides. Separate
subagents are required; explain inability to run when unavailable.

If the caller supplies a time or resource budget, establish its deadline/allowance
at the start and check remaining capacity before each dispatch or wait. Reserve
capacity for the readout; a long call is not permission to start another exchange
after the budget ends. Report overruns and incomplete checks honestly.

The main thread moderates unless a fresh moderator is requested. Disclose relevant
authorship or prior advocacy, and keep the fresh option available, including for
a cheaper host model. Model diversity and assigned perspectives are complementary;
choose them simply, without family specialties, scoring, or assignment machinery.

**Evidence.** Resolve the supplied PR, range, branch, or working-tree change.
Establish its baseline and target before reviewing. Use immutable commit IDs or a
stable snapshot containing the baseline, tracked patch, relevant untracked files,
and required surrounding context. Give reviewers the same evidence identity and
record inspected coverage. Preserve the user's source worktree. If essential scope
cannot be established, report the missing input rather than reviewing a guessed
change. This is not an obligatory setup interview.

Read the relevant requirements, glossary/context map, ADRs, surrounding code, and
supplied constraints. Establish an amendable agenda of consequential design choices,
ordered by consequence and dependency. No separate full PR-audit phase is required.

## 2. Frame quality tradeoffs and assign perspectives

ATAM supplies the analysis vocabulary, not a full workshop or voting process:

- **Quality drivers:** evidenced system needs, constraints, and stakeholder priorities.
- **Scenario:** a concrete stimulus/situation, relevant conditions, and required
  response. Use observable responses where supported; label unknown thresholds.
- **Architectural approach:** the implementation choice serving those qualities.
- **Tradeoff point:** a consequential choice affecting competing qualities and costs.
- **Sensitivity point:** a decision or parameter whose change materially affects
  an important quality.
- **Council perspective:** a reviewer's assigned emphasis, not a system priority
  or a prescribed conclusion.

For each item, give reviewers the same factual brief and evaluation questions,
with distinct relevant perspectives **before private preparation**. For retries,
these might be reliability/availability, performance/resource use, and
modifiability/operational simplicity; choose other attributes, such as security,
when the change warrants them. Reviewers can challenge assignments and premises.

Evaluate required quality and countervailing costs together. Each perspective
considers the whole tradeoff: reliability scrutiny may reject unnecessary
persistence, while simplicity scrutiny may justify it. Distinguish observed effects,
documented rationale, inferred rationale, and unknown author intent.

## 3. Obtain private positions

Launch separate preparations without peers' conclusions or prescribed answers.
Ask each reviewer for:

- Assessment, inspected evidence, assumptions, and cross-attribute consequences.
- A meaningful alternative or condition that would change the assessment.
- Inspected coverage and consequential agenda additions, challenges, or gaps;
  explicitly stating none is valid.

Preserve initial positions before revealing them. Merge duplicate agenda items
without erasing different premises. Record reasons for material exclusions or
deferrals; newly added items receive private preparation before discussion.
A proportionate implementation and **no material concern** are valid outcomes
per item and for the whole review. There is no objection quota.

## 4. Deliberate on each item

Before exchanging arguments, read the [deliberation guide](references/deliberation.md)
for turn briefs and disposition examples. Reveal positions, route concrete material
objections to respondents, and return replies to objectors for response or revision.
Label the moderator's own substantive claims and expose them to the same scrutiny.

Classify what remains unresolved:

- **Fact:** inspect behavior or evidence and establish what is known.
- **Missing user premise:** suspend the affected conclusion, retain useful
  conditional branches, and name the unanswered requirement and its consequence.
  Continue independent items.
- **Substantive dissent:** shared facts and constraints still admit different
  defensible tradeoff assessments. Preserve the reasoning.

Requirements can be challenged; a proposed replacement does not make current code
compliant with the current specification. Reopen an accepted ADR only when its
context or assumptions changed, identifying that change.

Each material conclusion, including unanimous ones, needs evidence, assumptions,
and a meaningful alternative or countercondition. Each revision names the evidence,
rebuttal, or changed premise that caused it. Headcount or unexplained convergence
does not establish a fact or resolve an objection.

**Continue on progress.** Before another exchange, name the unresolved claim and
what a further contribution could change. Permit multiple steps while evidence,
answers, alternatives, or reasoned revisions develop. Stop when exchanges only
repeat, the remaining disagreement is an understood tradeoff, or a missing user
premise prevents progress. Agreement needs no manufactured debate. There is no
small fixed round cap or extra numeric ceiling; honor explicit caller/host budgets,
and report material deferrals or unfinished work when they end.

If evidence changes, distinguish versions and reassess affected items before
claiming conclusions apply to the new revision. Do not merge incompatible snapshots.

## 5. Check the account and give the readout

Write a concise account of the main tradeoffs, strongest arguments, decisive
evidence, why positions changed, and meaningful remaining dissent. A reviewer checks
for material omissions or misrepresentation. Give each retained material dissent's
originator a narrow fidelity check as well: correction of portrayal, not a veto.
If unavailable, preserve the recorded objection and disclose the unchecked portrayal.

Then present:

1. The significance-first account, with traceable evidence, relevant prior
   involvement, and material coverage gaps, exclusions, deferrals, or suspended items.
2. Recommendations with reasons and conditions, distinct from the user's decisions.
3. Only necessary clarification or decision questions, explaining consequences and
   a recommendation where supportable.

The user stays outside internal deliberation: no intermediate clarification
requests. Inspect facts yourself; suspend missing user premises instead of guessing
or repeatedly debating them. Use dependent question rounds only after the readout
when necessary. If no clarification is needed, finish without a forced interview.

An answer selecting an analyzed branch updates the recommendation directly.
A materially changed premise can reopen affected items without restarting the
whole council. Keep the report in the conversation unless a saved report is requested.

## 6. Preserve eligible rationale

Follow the repository's ADR conventions. Matt Pocock's eligibility test requires
**all three**: meaningful reversal cost, surprising without context, and a genuine
tradeoff among alternatives. Missing a functional requirement alone is insufficient.

An eligible ADR draft can be saved in the established context's ADR directory with
the next available number and `status: proposed`. Use a short title and 1–3 sentences
covering context, proposed decision, and reason; add alternatives/consequences only
when useful. Label unconfirmed rationale and the missing premise. Saving a draft
does not accept it. Remove only unchanged drafts created by this council when the
user declines follow-up; preserve pre-existing or subsequently edited documentation.

Use installed `domain-modeling`, `codebase-design`, or `grilling` disciplines
where helpful within this review's scope. Essential rules above are self-contained;
another full workflow is not a prerequisite. For module tradeoffs, reuse interface,
depth, seam, leverage, and locality vocabulary without turning review into a refactor.

## Research and evaluation

For the primary sources, borrowed mechanisms, evidence limits, and evaluation
questions, read the [reference guide](references/sources.md) when investigating
the method or evaluating the skill. It includes ATAM, rationale research, Matt's
skills, pstack, deliberation/reporting precedents, and empirical model studies.
Council is ATAM-inspired; these sources do not validate this complete AI workflow.

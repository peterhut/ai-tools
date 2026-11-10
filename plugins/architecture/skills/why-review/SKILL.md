---
name: why-review
description: Surface consequential, non-obvious implementation decisions in a PR, branch, commit range, or working-tree diff and check whether their rationale is supported. Use when asked to discover hidden assumptions, ask why choices were made, or review decision rationale; produces a few evidence-backed questions rather than a general code review.
---

# Why review

Discover decisions a reviewer might not realize were choices at all. Start with
requirements and the implementation, then investigate the rationale. A choice can
satisfy the specification and repository standards while still carrying a tradeoff
worth discussing. Questions seek understanding; they are not defect findings.

Use ordinary repository tools in one reviewer context. Other skills, models,
services, and a decision database are optional, not prerequisites. Read the
[research reference](references/research.md) when explaining the method, checking
its foundations, or evaluating its effectiveness.

## 1. Establish the change and requirements

Resolve the supplied PR, branch comparison, commit range, or working-tree change.
Record the baseline and target commits; for a branch use its merge base with the
intended base branch. For local edits include staged, unstaged, and relevant
untracked files, keeping them distinct from committed changes. Preserve the source
worktree. If the comparison cannot be established, request only the missing scope.

Read the user's requirements, acceptance criteria, and applicable repository
instructions. Separate the requested outcomes and constraints from the
implementer's proposed solution or retrospective explanation. Defer the latter
until candidate discovery. When a PR body mixes both, extract the requirements
without treating its design claims as established facts.

Identify the required outcome separately from the mechanism chosen to achieve it.
Existing contracts, security requirements, and operational constraints still apply
when absent from the feature request. Treat an explicitly mandated mechanism as a
constraint until its authorized owner changes it.

Proceed with available evidence when an issue or requirement is inaccessible;
state the gap and keep requirement-dependent judgments conditional. If you already
authored the change or read its rationale, disclose that exposure rather than
claiming a blind or independent review.

## 2. Discover decisions before reading their defense

Read the actual diff and enough surrounding code, callers, and tests to understand
its behavior. Identify choices not dictated by the requirements, including changed
defaults, business exceptions, compatibility or scope changes, boundaries,
abstractions, dependencies, caching, ordering, retries, and authorization policy.
These are prompts for discovery, not a checklist demanding a finding in each area.

For each candidate, privately record:

- The concrete choice and its location in the change.
- The problem it appears to address, the required outcome, and the mechanism
  chosen. Label a problem inferred from the solution as a hypothesis.
- A plausible alternative preserving the same obligations, including keeping the
  previous behavior when relevant.
- A material consequence: user behavior, future change cost, performance,
  reliability, security, or operational burden.

Keep candidates only when choosing differently would have a meaningful consequence.
Names, formatting, routine idioms, and speculative future flexibility seldom earn
a question. Distinguish introduced choices from existing behavior the change merely
uses. Finish this initial inventory before consulting the author's explanation;
the inventory is scratch work, not a promised list of objections.

## 3. Investigate whether each question is already answered

Now read the implementer's explanation and relevant requirements, conventions,
ADRs, code comments, and tests. Use targeted Git history or linked discussions when
they could settle a specific uncertainty. Available repository evidence is enough
to begin; missing external access is a limitation, not a reason to invent intent.

For each consequential candidate, work through these problem-first questions in
order. Use the available requirements, code, tests, Git history, and rationale;
when evidence answers a question, continue to the next one.

1. **Problem:** What problem does this change address? Distinguish a documented
   need or observed failure from a reviewer-inferred problem. The proposed
   solution alone does not establish that the problem exists.
2. **Evidence:** What supports that problem and the required response? Cite the
   relevant sources and distinguish encoded behavior from evidence of a business
   need. Missing evidence leaves the problem hypothesis open.
3. **Assumptions:** What must be true for this solution to work? Identify the
   material assumptions about conditions, failures, callers, and existing
   protection covering the same scope.
4. **Consequences:** What happens if those assumptions are wrong? State the
   concrete effect on required behavior, users, reliability, or cost. Keep
   evidenced effects separate from predictions and use existing measures and
   targets where relevant.
5. **Checks:** How can the material assumptions be checked? Inspect available
   evidence and use suitable existing checks within the review's scope. Where
   uncertainty remains, identify a focused check that could support or challenge
   the assumption. Distinguish checks performed and their results from proposed
   checks; a validation plan is not validation evidence.
6. **Alternatives:** Could another solution address the same problem with less
   cost while preserving the required behavior and constraints? Compare credible
   alternatives, including existing behavior, with benefits and costs separate.
   If evidence supports a different problem framing, examine it explicitly while
   keeping agreed requirements intact. For removal of a safeguard, explain why
   its guarantee would survive: the safeguard itself may enforce the invariant.

Keep unresolved premises open as you continue independent questions. Record what
evidence is missing rather than inventing a problem, a threshold, or a validation
result. The six questions guide investigation; the final report includes only
decisions whose unresolved premises have a material consequence.

Seek evidence that would dismiss the candidate: a mandated contract, an established
convention, an accepted tradeoff whose context still holds, a measured constraint,
or an alternative that fails a requirement. Compare the explanation with actual
behavior and constraints. A plausible story is not evidence that it motivated
the original choice.

Treat the mechanism's claimed contribution as supported, contradicted by evidence,
or still unknown. Missing justification or incident history does not establish
unnecessary functionality. This is scenario and tradeoff reasoning, not a numerical
worth score or a separate approval gate.

Keep these distinct: **observed behavior**, **documented rationale**, **inference**,
and **unknown intent**. If rationale is absent, say it was not found in the sources
checked. That absence alone establishes neither a defect nor a need to redesign.

Dismiss adequately justified candidates. Retain a question only when a material
premise, alternative, or tradeoff remains unresolved or the explanation conflicts
with evidence. Reopen an established decision only when the change affects its
context or assumptions, naming what changed. Stop each investigation when the
question is answered, worth asking, or limited by unavailable evidence; expand the
code slice only to resolve a named uncertainty.

## 4. Return the questions worth the reviewer's attention

State the reviewed comparison, relevant requirements, and material coverage limits
briefly. Return only a handful of consequential questions, ordered by impact and
dependency. Combine candidates that share one underlying decision. For each:

- **Ask why:** name the precise choice, with revision-specific path/symbol/line
  references and the relevant requirement or convention.
- **Explain the stakes:** describe the supported benefit and countervailing cost,
  using conditional language for unmeasured effects. Include a realistic alternative.
- **Show what remains open:** summarize the rationale found, or where it was
  sought, the unresolved assumption and consequence if wrong, and the evidence or
  focused check that would settle it. Label a proposed check as proposed.

For example: “The shared client already retries transient failures, while this
change adds an application retry loop. Which additional failure scenario requires
the second layer, and how does the combined attempt budget fit the request deadline?
Keeping retries in the shared client avoids multiplying attempts but may leave an
application-specific recovery need uncovered.” Attach real code references in an
actual review; this example supplies no evidence about a repository.

An empty result is valid: report that no consequential unexplained decisions were
found within the examined scope. Omit a catalogue of dismissed candidates and a
merge verdict. A confirmed requirement violation should be labeled directly rather
than disguised as an open design question; keep any such note distinct and brief.

Suggest recording rationale only when its context is surprising, its tradeoff is
consequential, and future maintainers would need it to revisit a costly decision.
Prefer the smallest useful existing home. The review produces questions and
recommendations in the conversation; editing code, creating ADRs, posting comments,
or launching a broader review requires the corresponding task authorization.

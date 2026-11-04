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
- The requirement it serves, or the assumption it appears to add.
- A plausible alternative, including keeping the previous behavior when relevant.
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

For each candidate, seek evidence that would dismiss it: a mandated contract, an
established convention, an accepted tradeoff whose context still holds, a measured
constraint, or an alternative that fails a requirement. Compare the explanation
with actual behavior and constraints. A plausible story is not evidence that it
motivated the original choice; tests demonstrate encoded behavior, not necessarily
the business reason for it.

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
  sought, and identify the missing premise or evidence that would settle the choice.

For example: “Why cache prices across requests in `PriceService.Calculate`? The
requirement asks for current prices; the diff adds a five-minute TTL. Caching may
reduce calls but permits stale results. What freshness allowance or measured load
justifies this over calculating on demand?” Attach real code references in an
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

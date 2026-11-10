# Research for a lightweight why-review

Verified 2026-11-03 against source files and original papers. The final message in
the [shared conversation](https://chatgpt.com/share/6aea5485-cc7c-83ed-8fd0-d6246c98d080)
supplied the design scope, not research evidence. Repository precedents are pinned below.
**Scope choice:** discover consequential, non-obvious decisions from a PR/diff and
its requirements before consulting the implementer's rationale; investigate the
survivors and return a few questions grounded in code. An empty result is valid.

Evidence labels: **precedent** = another skill's instructions; **empirical** = study
findings; **conceptual/practitioner** = argument or experience report; **inference**
= our adaptation. None establishes that this complete skill works.

## Verified workflow precedents

### George Nurijanian problem-first: make the assumptions inspectable

**Precedent.** The author's June 2026 article starts from a proposed solution,
examines the underlying problem, makes assumptions explicit with consequences if
wrong and validation checks, considers alternative problem framings, and reports
evidence status. The public description was checked on 2026-11-10; the complete
installable skill was not available for inspection. Its examples and time-saving
claims are practitioner reports, not a controlled review evaluation. [Article][problem-first]

**Inference.** Adapt that discipline into six ordered PR-review questions in
`SKILL.md`. Use requirements, code, tests and Git history to answer them; keep
inferred problems and proposed checks distinct from established needs and observed
results. The ordered investigation and concise reporting are our adaptation,
without a required number of alternative framings or the article's eight-section
output. Neither the description nor its reported experience validates this
adaptation's review accuracy. [Problem-first description][problem-first]

### pstack why: explain an identified decision

**Precedent.** `cursor/plugins/pstack/skills/why/SKILL.md` investigates motivation
behind a named target. It first establishes code/history anchors, then consults
seven evidence categories, and separates findings, inferences, competing hypotheses
and unknowns. This is historical explanation, not a prescribed scan for unnoticed
decisions across a PR. Its references explicitly reject reconstructing intent from
code alone and treating absent evidence as evidence of absence. [Why][why],
[epistemics][epistemics]

**Inference.** Borrow code anchors, cited rationale and honest unknowns. Bound the
search to relevant repository material and linked discussions; the comprehensive
seven-category investigation is unnecessary for this task. The decision-discovery
pass and delayed rationale are our additions, not this precedent's process. [Why][why]

### pstack interrogate: demonstrated failures, with permission to find none

**Precedent.** The current `pstack/skills/interrogate/SKILL.md` requires executable
evidence, reruns candidate findings, merges shared root causes and explicitly
accepts no verified findings. Its current output is verified/dropped findings,
not the importance-classification scheme suggested by the saved conversation.
It supplies intent from user messages, commits and the PR description. [Interrogate][interrogate]

**Inference.** Borrow traceability, deduplication and permission for an empty result.
A why question may expose an unresolved tradeoff without demonstrating a failure;
do not present it as a defect or import the executable-failure gate. [Interrogate][interrogate]

### Matt Pocock code-review: distinguish requirements from standards

**Precedent.** The current path is `skills/engineering/code-review/SKILL.md`.
It pins a comparison point, locates specification and repository standards, and
keeps their assessments distinct. The specification brief explicitly includes
unrequested behavior; when no spec is available, its review is skipped explicitly.
Its standards review also includes heuristic code smells, subject to repo overrides.
[Code review][code-review]

**Inference.** Establish intended behavior separately from the choices implementing
it. A spec-compatible cache, fallback or abstraction can still warrant a why
question. Avoid duplicating standards findings or treating every implementation
detail absent from a specification as scope creep. [Code review][code-review]

### Matt Pocock grill-me: interrogate decisions, inspect facts yourself

**Precedent.** The current `skills/productivity/grill-me/SKILL.md` is a short alias
to `grilling`. That skill interviews through dependent rounds of decisions, gives
recommendations, and says to investigate discoverable facts rather than asking the
user for them. [Grill-me][grill-me], [grilling][grilling]

**Inference.** Inspect the repository before spending the author's attention.
Borrow questions that expose assumptions, but replace exhaustive interviewing with
a small final selection; do not require traversing a whole decision tree. [Grilling][grilling]

## Empirical findings and their limits

**Bacchelli and Bird, ICSE 2013.** Their Microsoft study combined 17 developer/tester/
architect meetings, classification of 570 comments in 200 discussion threads, and
surveys receiving 165 manager and 873 programmer responses. Understanding changes
and their rationale emerged as a central challenge; considering alternative
solutions was also a reported review motivation. These are observational and
self-report findings, not a trial of this skill. [Paper, §§III, IV.C, VI][bacchelli]

**Counterweight to delaying rationale.** The study reports that author context and
direction helped reviewers, and unfamiliar code required broader investigation.
It therefore supports eventually reading the explanation and surrounding code;
it does not show that withholding rationale improves review. Knowledge transfer
and team awareness were difficult to quantify from review traces. [§§VI–VIII][bacchelli]

**Wyrich, Merz and Graziotin, ICPC 2022.** A randomized experiment with 256
participants (206 students; 50 in the professional/researcher group) varied task
descriptions and two Java snippets. Initial difficulty information influenced
subjective code-comprehensibility ratings. The study did not measure actual
comprehension with additional tasks; its authors limit generalization to individual
snippets and discuss replication on larger systems. [§§3–5.2][anchoring]

**Inference.** Recording initial candidate decisions before reading an author's
defense is a plausible precaution against framing, not a proven debiasing method.
This experiment did not test PR rationale, decision discovery, review accuracy or
AI reviewers. Revisit initial candidates after context to avoid preserving the
reviewer's own first impression. [Anchoring study][anchoring], [review context][bacchelli]

**Tavantzis et al., Cognitive Debt in the GenAI Era (2026).** This preprint reviews
76 grey-literature sources about lost software understanding. Practitioners report
that working code can conceal gaps in understanding and propose rationale capture,
smaller reviews and comprehension checks. It synthesizes practitioner accounts;
it does not establish causal effects, prevalence, or the efficacy of those remedies.
The manuscript was under review at ICSE-SEIP when inspected. [Abstract, §§III–VI][cognitive]

**Inference.** Discovering choices worth explaining may help expose an understanding
gap that functional checks leave open. An AI-generated explanation alone does not
demonstrate shared human understanding; whether this skill helps requires evaluation.
[Cognitive debt study][cognitive]

## Conceptual and practitioner foundations

### Required outcomes, contribution, and proportionality

Sources in this subsection checked 2026-11-04.

**Established method.** ATAM evaluates architecture against quality-attribute
goals. It connects business drivers, concrete scenarios and architectural approaches,
then identifies risks, non-risks, sensitivity points and tradeoff points. Its full
stakeholder evaluation is broader than this PR-review skill. [SEI ATAM collection][atam]

**Inference.** For a consequential mechanism, identify the required outcome,
the situation and conditions in which it contributes, existing protection, and
a smaller alternative preserving the same obligations. Compare benefits and costs
with relevant evidence and existing targets. These focused prompts adapt ATAM's
scenario-based reasoning; they neither implement full ATAM nor establish a measured
product-value score or a validated improvement in AI review accuracy. An unknown
premise stays unknown, and an explicitly mandated mechanism remains a constraint
until its authorized owner changes it. [ATAM][atam], [review understanding][bacchelli]

**Practitioner evidence.** Amazon describes retries as improving resilience to
transient failures while potentially amplifying load across layers; side effects
require attention to idempotency, and circuit breakers add recovery and testing
costs. Its guidance supports asking what an additional protection contributes in
the actual stack, not assuming either the protection or its removal is justified.
[Timeouts, retries, and backoff with jitter][retries]

**Inference.** Trace whether the guarantee would remain true without a safeguard;
the guard itself may be its enforcer. Absence of incidents or of a recorded reason
does not prove redundancy. Use code-supported counterfactuals and retain unresolved
premises as questions. This is an evidence discipline rather than a deletion rule.
[Rationale epistemics][epistemics]

### Understanding and recording rationale

**Storey, From Technical Debt to Cognitive and Intent Debt (2026).** The proposed
Triple Debt Model distinguishes changeability problems in code, eroding shared
understanding, and missing externalized goals, constraints and rationale. It is a
conceptual framework with practical arguments, not a controlled intervention study.
[Author's manuscript, v4][intent]

**Inference.** Ask for the missing business or design premise when code explains
what happens but cannot establish why that outcome was wanted. This motivates
rationale review without automatically requiring more documentation. [Storey][intent]

**Naur, Programming as Theory Building (1985).** In §4, Naur distinguishes knowing
how a program maps to its problem, justifying its particular shape (including
estimates and alternatives), and adapting it to new demands. His argument draws
on programming experience and cases; it is not a controlled review study.
[Original article, §§2–4, pp. 254–256][naur]

**Inference.** Ask about the relationship between a concrete choice and its
constraints. A plausible explanation generated from code cannot establish the
historical author's intent or recover all tacit knowledge. Naur supplies conceptual
motivation, not evidence that this workflow prevents AI-related cognitive debt.
[Naur][naur], [pstack epistemics][epistemics]

**Nygard, Documenting Architecture Decisions (2011).** The original proposal
records significant decisions with context and consequences, including negative
and neutral consequences. It warns against blind acceptance or reversal when
motivation is unknown. Its experience report describes early positive feedback
from rotating developers, not a controlled effectiveness evaluation. [Original post][adr]

**Inference.** Reuse context/choice/consequences when reading existing rationale.
A PR comment or linked explanation may suffice; no ADR, new decision record or
architecture redesign is required by this task or evidence. [Nygard][adr]

## Boundaries and evaluation

Missing written rationale is an information gap, not a defect, proof that no thought
occurred, or an instruction to document everything. An explanation can exist outside
the sources inspected; report that limit. [Epistemics][epistemics]
This note does not validate the complete skill, reduced cognitive debt or improved
defect detection. The proposed workflow requires no model selection, delegation,
orchestration or mandatory ADR creation.
A useful pilot would compare rationale-first and rationale-later review on real
changes, including changes deserving no question, with human assessment of material
questions, false premises, missed decisions and author effort. That evaluation
design is our proposal; question count alone is not a success measure.

## Primary sources inspected

[why]: https://github.com/cursor/plugins/blob/6ea74587e384e1a8035c09ae33a46fe3f881eb9b/pstack/skills/why/SKILL.md
[epistemics]: https://github.com/cursor/plugins/blob/6ea74587e384e1a8035c09ae33a46fe3f881eb9b/pstack/skills/why/references/epistemics.md
[interrogate]: https://github.com/cursor/plugins/blob/6ea74587e384e1a8035c09ae33a46fe3f881eb9b/pstack/skills/interrogate/SKILL.md
[code-review]: https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/code-review/SKILL.md
[grill-me]: https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/productivity/grill-me/SKILL.md
[grilling]: https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/productivity/grilling/SKILL.md
[bacchelli]: https://www.microsoft.com/en-us/research/wp-content/uploads/2016/02/ICSE202013-codereview.pdf
[anchoring]: https://arxiv.org/pdf/2203.13705v1
[cognitive]: https://arxiv.org/html/2610.37888v1
[intent]: https://arxiv.org/abs/2603.22106v4
[naur]: https://ingenieria-de-software-i.github.io/assets/bibliografia/programming-as-theory-building.pdf
[adr]: https://www.cognitect.com/blog/2011/11/15/documenting-architecture-decisions
[atam]: https://www.sei.cmu.edu/library/architecture-tradeoff-analysis-method-collection/
[retries]: https://aws.amazon.com/builders-library/timeouts-retries-and-backoff-with-jitter/
[problem-first]: https://nurijanian.substack.com/p/problem-first-a-simple-skill-to-invert

Naur's link is a course-hosted facsimile of the original nine-page journal article;
the [author's archive](https://www.naur.com/comp/) also identifies the work.

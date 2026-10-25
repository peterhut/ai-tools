# Council plan review — round 1

Review the proposed council skill as a DESIGN, not as an implemented skill or a code diff. The user explicitly requested this review through GitHub Copilot with Sonnet 5.5. No council skill exists yet. Do not implement anything, change any files, install anything, publish anything, or spawn further agents. This is read-only review; return findings in your final answer.

Workspace: /home/peter/.t3/worktrees/ai-tools/t3-cd499c49
Use the shared working tree: the plan and research are uncommitted and must be read there.

## Review packet — read every file

Current plan (source of truth for latest decisions):
- docs/council/design.md
Domain vocabulary:
- docs/council/GLOSSARY.md
All collected research:
- docs/council/model-diversity-research.md (including the later model-to-perspective-fit section)
- docs/council/matt-skills-integration-research.md
- docs/council/moderation-research.md
- docs/council/moderator-model-selection.md
- docs/council/agenda-led-process-research.md
- docs/council/user-interaction-research.md

Each research note has primary-source links and limitations. Follow those links selectively when needed to evaluate a material claim. Also inspect the relevant existing architecture plugin and root AGENTS.md, GLOSSARY-MAP.md, and README.md. The installed Matt skill paths in the research note can be inspected if useful; do not invoke their workflows. Historical recommendations inside research notes are evidence inputs, not overrides of the latest design. Report any source or artifact you could not access.

## Latest user decisions and important resolutions

The original prompt below is the starting brief, not the final design. The subsequent interview settled:
- Retrospective, lightweight ATAM-inspired review of implemented changes. Quality attributes, concrete scenarios, architectural approaches, tradeoff/sensitivity points, inferred rationale, and proportionality are the substantive core; avoid replacing that core with meeting ceremony.
- Moderator inspects change and proposes an AMENDABLE agenda. Three genuine subagent reviewers prepare positions privately before seeing peers' conclusions. The moderator explicitly gives them different relevant quality-attribute priorities BEFORE preparation, while sharing facts and evaluation questions. Reviewers may challenge priorities, premises, or omitted items. No predetermined opinions or fixed vendor personas.
- Prefer capable model-family diversity but allow disclosed same-model fallback. Model diversity and perspective diversity are complementary. No built-in family-to-attribute mapping, learned routing, numerical scoring, mandatory rotation, or elaborate assignment procedure.
- Main thread moderates by default. A fresh moderator option is available, especially if the host is a cheaper model. Any model choice must preserve actual host capability; no claim of a permanent best moderator.
- Real item-level deliberation: positions, concrete objections, replies returned for response/revision, and supported disposition. Multiple exchanges are allowed while making material progress. No small fixed round limit or extra numeric ceiling; honor explicit user/host budgets. Do not force consensus, manufacture conflict, or treat silence/headcount as validation.
- User stays outside internal deliberation. Inspectable facts are investigated by agents. Suspend affected items when missing user premises block meaningful progress; continue independent items. Give a checked, concise newspaper-like account before recommendations, retaining meaningful dissent and decisive evidence. Ask only necessary questions after the readout. An answer can select a pre-analyzed branch or trigger focused reconsideration of affected items.
- A reviewer checks the synthesis for material omissions/misrepresentation, without veto over conclusions.
- Recommendations and ADR drafts only; no implementation changes. ADRs use Matt's all-three threshold: meaningful reversal cost, surprising without context, genuine tradeoff. Inferred/unconfirmed rationale stays proposed. Draft files are allowed and council-created drafts may be removed if the user declines follow-up. Reopen accepted ADRs only when context or assumptions have changed.
- Portable across T3, Copilot, Cursor, or another host with GENUINE subagents. Model diversity is optional; subagents are required. Reuse Matt's available disciplines, with self-contained attributed essentials; do not require his whole collection or another complete review workflow.
- Explicit user invocation only. Include the primary references in the final skill for evaluation.
- A separate full independent PR-review mode has been questioned and is omitted from the proposed delivery: focused preparation plus agenda amendments, without three mandatory full audits or generic correctness/standards reviews.
- Proposed implementation defaults: package council in the existing architecture plugin, concise entrypoint and focused references, update both manifests as minor release with Codex cachebuster and required validation, and use an isolated behavioral forward-test. These delivery details still await final shared-understanding confirmation.
- User requested this external design review BEFORE implementation. Do not interpret the original skill-creation intent or recent individual agreements as permission to implement.

## Prior findings, responses, and unresolved objections for this review round

This is the FIRST external plan-review round; there are no prior plan-review findings to continue. Earlier delegated work was research, and its findings/responses/limitations are preserved in the packet.
Key questions addressed in the interview: restore ATAM/quality attributes as the core; explicitly assign perspectives rather than hope families supply them; replace complete blind PR reviews with agenda-led preparation; keep user clarification after internal deliberation; preserve dissent; drop built-in family mappings; use progress-based stopping.
Potential areas still deserving scrutiny rather than silent assumptions: practical progress/closure criteria, agenda anchoring/coverage, quality-scenario sufficiency under missing requirements, dissent fidelity and moderator influence, freshness/context transfer, and host-specific delegation/model access.

## What to review

Give your own independent assessment; do not manufacture findings to satisfy a quota. Challenge the plan even where the user made a deliberate choice, but identify the choice and its concrete consequence rather than treating preference differences as factual errors.

Assess:
1. Fidelity to the original intellectual aim and latest user decisions; ATAM-inspired substance versus orchestration overhead.
2. Whether assigned quality priorities plus optional model diversity give coherent competing perspectives, including overall fitness and proportionality rather than independently maximizing attributes.
3. What fails in realistic changes: weak documentation, absent requirements, accepted ADRs, all reviewers agreeing, erroneous consensus, agenda omissions, fresh moderator on a cheap host, same-model fallback, unsupported host capabilities, or changes to evidence mid-review.
4. Actual objection/reply/revision mechanics, progress-based stopping, and the boundary between unresolved fact, missing user premise, and defensible priority disagreement.
5. Evidence quality: primary-source fidelity, limitations of benchmark transfer, research contradictions, distinction between initial five studies and later ATAM evidence, and any unsupported novelty or vendor-specialty claims.
6. Fit with Matt's reusable skills and the existing plugin, self-contained/portable execution, source discoverability, concise maintainable instructions, and minimum meaningful validation.

Return:
- A concise overall assessment.
- Material findings, each with plan/source location, concrete trigger or failure scenario, consequence, and smallest useful recommendation. Distinguish demonstrable inconsistency from design judgment and unresolved question.
- Any defensible dissent from the proposed choices.
- Which issues should be resolved before writing the skill versus evaluated in a pilot.
- A short source/packet coverage note.
Do not provide a full rewrite or approve implementation on the user's behalf.

## Original opening user prompt and supplied research

The original content follows, with link-display glyphs omitted and tabular/list layout normalized. Its claims are historical input, not automatically verified facts or instructions to restart the grilling interview.

$grill-with-docs New skill called council. Based on previous research: Your council idea is not fundamentally novel—and that’s good news. There is substantial software-engineering research we can reuse. In fact, I found an established architecture evaluation method that is remarkably close to what you’re describing.

The novel part would be adapting those principles to multiple independent AI coding agents reviewing an actual implementation, particularly when nobody has explicitly documented the design choices the code embodies.

My strongest recommendation is to use the terminology of Architecture Tradeoff Analysis, Design Rationale, and Quality Attributes, rather than inventing a new framework around AI personas.

1. The closest match: Architecture Tradeoff Analysis Method (ATAM)

Architecture Tradeoff Analysis Method (ATAM)

Developed at Carnegie Mellon’s Software Engineering Institute, beginning in the late 1990s.

Established software-architecture evaluation methodology

ATAM was created to assess how architectural decisions affect competing quality attributes such as performance, modifiability, security, and availability.

Its central idea is that improving one quality attribute often makes another worse. Rather than judging architecture against vague best practices, ATAM evaluates architectural choices against the system’s actual requirements and scenarios.

That is almost exactly your concern about code being unnecessarily safe, unnecessarily optimized, or more complex than its requirements justify.

Particularly useful ATAM terminology

Term — Meaning for your council
Quality attribute — A desired property, such as performance, reliability, security, or modifiability
Architectural approach — A design choice made to achieve a particular quality
Tradeoff point — A decision that affects multiple quality attributes
Sensitivity point — A decision where a small change can substantially affect one quality attribute
Risk — A design decision that may compromise a required quality
Quality attribute scenario — A concrete situation used to determine what quality is actually required

For example, the decision to retry failed requests affects reliability, latency, downstream load, and complexity. That’s a tradeoff point.

The exact retry policy might be a sensitivity point: changing from two retries to ten could dramatically increase downstream load during an outage.

And a scenario could specify: When the downstream service is unavailable for five minutes, at most 0.1% of accepted transactions may be lost.

Without such a scenario, the council cannot determine whether sophisticated retry handling is justified.

The important adaptation

Traditional ATAM is primarily used to evaluate architectural proposals with stakeholder input. Your council would additionally work backward:

Existing implementation / PR
Recover implicit design decisions
Reliability
Performance
Simplicity
Identify tradeoffs and assumptions
Compare against actual quality requirements

I would call this a lightweight, retrospective ATAM-inspired review, rather than claim it implements the full ATAM process.

That preserves the research foundation without importing a heavyweight methodology.

2. Design rationale: recovering the decisions nobody documented

This is the second particularly strong connection.

Design rationale refers to the reasoning underlying a design decision: the problem, alternatives, assumptions, and reasons for selecting one solution.

There’s even very recent research directly relevant to your idea:

Using LLMs in Generating Design Rationale for Software Architecture Decisions, published in ACM Transactions on Software Engineering and Methodology on July 16, 2026.

The paper investigates using LLMs to generate and recover architectural design rationale—specifically because that rationale is often poorly documented by developers.

That makes a useful distinction for the council:

* Decision recovery: What choices can we infer from the implementation?
* Decision evaluation: Are those choices appropriate for the requirements?
* Decision documentation: Which conclusions are worth preserving for future work?

The first is related to design-rationale recovery; the second relates to ATAM; the third relates to Architecture Decision Records (ADRs).

Architecture Decision Records

The established ADR format is Context → Decision → Consequences, with alternatives and rationale included where relevant. Martin Fowler’s March 2026 description explicitly emphasizes documenting the alternatives and their pros and cons.

However, I wouldn’t generate an ADR for every finding. That would create exactly the documentation overhead you’ve previously wanted to avoid.

Instead, have the council propose an ADR only when a decision is significant, non-obvious, and likely to be questioned or accidentally reversed by a future contributor.

3. Matt Pocock: several strong building blocks

I examined the current mattpocock/skills repository.

codebase-design

Introduces vocabulary around deep modules, interfaces, seams, locality, and leverage. These are useful concepts for judging the cost of an implementation’s complexity, rather than just counting abstractions.

Particularly useful: its design-it-twice approach, which explores structurally different alternatives and compares them.

improve-codebase-architecture

Finds architectural friction and opportunities to make modules deeper. It examines whether understanding a concept requires navigating numerous modules and whether an abstraction earns its cost.

It also distinguishes strong findings from speculative suggestions, and avoids repeatedly challenging existing ADRs without good reason.

code-review

Uses parallel subagents to evaluate two separate concerns: standards and compliance with the originating specification.

This is an excellent example of decomposing a review into distinct concerns, but your council would add a third question: Are the design choices appropriate, even if they satisfy the specification and coding standards?

The most important thing to borrow from Matt is the separation of reusable engineering principles from the workflow that invokes them.

Your council shouldn’t duplicate a full architecture skill. It should orchestrate the investigation and apply existing design vocabulary where useful.

4. pstack: surprisingly close to the multi-model implementation you want

I also inspected Lauren Tan’s pstack in its original Cursor plugin.

Three skills stand out.

interrogate — most similar orchestration

Runs independent read-only reviewers across configured models, with a common prompt and shared rubric. Crucially, it explicitly relies on model diversity rather than assigned personas.

This is close to Theo’s screenshot. I’d borrow its orchestration mechanics, but extend its analysis from adversarial defect review to architectural tradeoffs.

arena — strongest candidate for exploring alternatives

Runs multiple candidate solutions in parallel, then uses an independent model to judge the alternatives against a task-specific rubric.

This introduces a useful mechanism beyond reviewing the existing implementation: asking whether a substantially simpler design could satisfy the same constraints.

architect — design-space exploration

Combines how, why, and arena. It deliberately requests at least two structurally different design candidates before settling on an approach.

This closely matches your desire to reveal decisions that were effectively made by default when the implementation was generated.

Other pstack principles worth reusing are Laziness Protocol, Minimize Reader Load, Attack the Premise, and Exhaust the Design Space. These provide specific tests against unnecessary work, accidental complexity, and insufficient exploration of alternatives.

A meaningful difference from pstack

pstack’s interrogate intentionally uses the same review rubric for every model.

For defect finding, that makes sense. For your tradeoff analysis, it might be useful to give reviewers different priorities after a shared independent assessment.

I’d experiment with the following hybrid:

Stage 1: Generalist blind review
Same rubric
GPT
Claude
Gemini
Identify meaningful design decisions
Aggregate findings without suppressing differences

Stage 2: Perspective-specific challenge
Reliability
Performance
Simplicity
ATAM-inspired tradeoff synthesis

This combines genuine model independence with the purposeful diversity you’re looking for.

5. Which concepts should we actually adopt?

Here’s my proposed selection.

Concept — Origin — Adopt?
Quality attributes — ATAM — Yes, as the vocabulary for review perspectives
Tradeoff and sensitivity points — ATAM — Yes, these should be central findings
Quality attribute scenarios — ATAM — Yes, but lightweight and only when useful
Design rationale recovery — Software architecture research — Yes, central objective
ADRs — Established architecture practice — Optional output, not mandatory
Independent model review — pstack interrogate — Yes
Design it twice — Matt Pocock / pstack architect — On demand
Deep modules and locality — Matt Pocock — Reuse existing principles
Full ATAM workshops and scoring — ATAM — No, too heavyweight for ordinary PRs
Formal voting and numerical confidence — Council frameworks — Probably not necessary initially

6. Proposed council: an implementation tradeoff review

Rather than define the council as three AI engineers arguing, I’d frame it as:

A multi-model, ATAM-inspired review that recovers implicit design decisions, evaluates their tradeoffs against system requirements, and identifies unjustified complexity or risk.

The output could be as small as this:

Design tradeoff: Retry mechanism

Illustrative

Decision inferred
Persistent retry queue instead of a bounded in-process retry.

Quality attributes affected
Reliability ↑ Complexity ↑ Operational cost ↑

Assumption requiring validation
Operations must survive a complete process restart.

Alternative
A bounded in-process retry would be simpler but could lose operations during a restart.

Council conclusion
The implementation may be appropriate, but its additional complexity is justified only if durable processing is a real requirement.

Suggested action
Clarify the durability requirement before accepting or simplifying the design.

There’s an important methodological limitation: the council can infer what a design accomplishes, but it cannot reliably know what the author intended without supporting evidence. That distinction should be explicit in every report.

7. Where I see genuine room for innovation

The individual ingredients are established. What seems less established is the combination of:

* Retrospective decision recovery from AI-generated code, rather than evaluating only an explicitly proposed architecture.
* Dynamic selection of competing quality attributes, based on which design tensions the implementation actually introduces.
* Multi-model examination of the same decision, distinguishing genuine differences in assessment from factual mistakes.
* Proportionality analysis, examining whether a quality improvement is worth the complexity and cost in the specific context.
* Selective preservation of rationale, producing an ADR only when future agents would benefit from knowing why an alternative was rejected.

I haven’t found an existing skill that deliberately combines all five. That doesn’t prove no such tool exists, but it suggests that your idea is more than a rebranding of pstack’s interrogate.

My recommendation

I would start with a deliberately small council skill, built around three concepts:

1. Discover decisions. Reconstruct meaningful implementation choices and their alternatives.

2. Identify tradeoffs. Apply ATAM quality attributes and concrete scenarios, with independent models contributing competing perspectives.

3. Challenge proportionality. Decide whether the added performance, safety, flexibility, or abstraction is actually worth its cost for this system.

Use pstack’s interrogate as the reference for multi-model orchestration, Matt’s codebase-design as a source of maintainability vocabulary, and ATAM as the principal intellectual foundation.

And I’d keep an important distinction: the council should evaluate the fitness of a design for its context, not optimize every quality attribute independently. That is both the strongest connection to established software engineering and the feature that makes this idea especially valuable for reviewing agent-generated implementations.


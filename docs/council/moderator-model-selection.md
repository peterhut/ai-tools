# Model selection for council moderation

Research date: 2026-10-25. This note provides design input, not a settled model policy or a moderation benchmark.

## Verified first-party guidance

OpenAI describes GPT-6.1 Sol as suitable for complex coding and professional work at lower cost than Astra. Its model-selection guide associates Sol at extra-high effort with decisions built from conflicting evidence, and Luna with scoped tasks, triage, and frequent automations. The guide recommends comparing the same inputs and keeping the lightest setting that meets the task's quality bar. These are vendor recommendations, not evidence from council reviews. [Sol model](https://developers.openai.com/api/docs/models/gpt-6.1-sol), [model-selection guide](https://developers.openai.com/api/docs/guides/model-selection).

GPT-6 Luna is documented as an efficient model for focused, high-volume tasks. That positioning does not establish that it can reliably adjudicate competing architectural arguments or preserve minority concerns during compression. [Luna model](https://developers.openai.com/api/docs/models/gpt-6-luna).

Anthropic documents Claude Opus 5.5 for long-running agentic coding and knowledge work. Its selection guide recommends task-specific benchmarks, actual prompts and data, and comparing quality, edge cases, cost, and effort settings. Neither inspected vendor guide establishes whether Opus 5.5 is a better council moderator than GPT-6.1 Sol. [Opus model](https://platform.claude.com/docs/en/models/opus-5-5/overview), [Claude model-selection guide](https://platform.claude.com/docs/en/about-claude/models/choosing-a-model).

The app's live catalog was inspected for available model IDs and supported options. Catalog presence and provider configuration establish potential execution paths, not comparative performance. A future council run must discover available models and settings at execution time; this note does not fix a reviewer roster or require a particular provider.

## Proposed capability requirements

The moderation role may need to select consequential agenda items, identify implicit requirements, compare alternatives fairly, request source verification, separate factual disagreement from conflicting priorities, formulate useful user questions, and retain substantive dissent through synthesis. This is a proposed task contract; the interview has not finalized it.

Evaluate candidates on whether they:

- Trace each material conclusion to inspected evidence and stated requirements.
- Recognize unsupported claims, missing requirements, and defensible differences in priorities.
- Represent the strongest surviving objection and its disposition accurately.
- Identify answers that would change the recommendation, without asking the user for inspectable facts.
- Revise conclusions when premises change and avoid inventing rationale or agreement.
- Produce a concise account whose recommendations follow from its evidence.

The labels intelligent, thorough, and cheap are insufficient selection criteria. More output is not evidence of better moderation. Record model identity, effort, execution support, cost, latency, and concrete failures when comparing candidates.

## Allocation options, not settled decisions

**Main-thread moderator:** retains the full user conversation and reduces handoff and coordination. If it authored the implementation, its prior commitment may influence agenda selection or synthesis. Reviewer-originated agenda proposals and review of the final synthesis are candidate safeguards; they do not prove neutrality.

**Separate moderator:** starts without the host's full implementation conversation and can receive a deliberately bounded evidence packet. This changes context exposure and enables different model selection, but adds cost and handoff risk. It remains a fallible model, not an independent guarantee. The main thread still needs to relay user answers and deliver the result faithfully.

**Separate prose summarizer:** could rephrase an already checked account into concise prose, but must not silently make new substantive decisions or erase unresolved objections. A lower-cost model is a candidate for this bounded transformation only if fidelity is checked. Adding it is not required merely because moderation and summarization are distinct responsibilities.

## Provisional recommendation and comparison

Start with a capable reasoning model for substantive moderation. GPT-6.1 Sol and Claude Opus 5.5 are candidates supported by their documented workload positioning, without an established ranking for this task. Treat Luna as a candidate for a constrained reporting step, or for moderation only after it meets the same evaluation criteria.

Before adding a mandatory fourth agent or selecting a specific model, compare moderator outputs on the same evidence and independent reviewer submissions. Inspect missed material objections, unsupported dismissals, requirement changes, fidelity of dissent, usefulness of user questions, cost, and latency. Separately compare host-context and fresh-context moderation; changing both context and model at once would obscure what caused a difference. Preserve human assessment as the decision criterion rather than asking a candidate moderator to be its own sole judge.

These recommendations do not authorize an evaluation run, implement a council, settle a fourth-agent requirement, or assume a permanent best model.

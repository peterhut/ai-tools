# Sources and evaluation

Read when investigating the method, checking a borrowed rule, or evaluating council.
These are foundations and experiments, not evidence that this complete workflow
improves real PR review. The source selection was assembled in October 2026.
Follow primary sources when extending a claim; distinguish study results from
council's proposed adaptations.

## Architecture evaluation and rationale

- [SEI ATAM collection](https://www.sei.cmu.edu/library/architecture-tradeoff-analysis-method-collection/)
  explains evaluation against quality goals, concrete scenarios, architectural
  approaches, risks, sensitivity/tradeoff points, and risk themes. Council borrows
  this vocabulary and fitness-for-context reasoning, not full workshops, voting,
  utility-tree scoring, or a claim to implement ATAM.
- [Architecture Analysis Techniques and When to Use Them](https://sei.cmu.edu/documents/1990/2002_004_001_13946.pdf),
  CMU/SEI-2002-TN-005, §§3–3.2 and 4.3, describes preliminary focused analysis and
  broader stakeholder scenario generation. This supports an amendable evaluation
  focus; moderator-only agenda formation and end-only user involvement are AI
  adaptations, not that source's procedure.
- [WIN-T ATAM case study](https://www.sei.cmu.edu/documents/2080/2005_004_001_14522.pdf),
  CMU/SEI-2005-TN-027, §§4.1–4.10, illustrates distinct facilitation, questioner,
  scribe, and analysis responsibilities, sometimes combined in one person, and
  synthesis into risk themes. It does not establish an optimal agent count.
- [Towards Supporting Quality Architecture Evaluation with LLM Tools](https://arxiv.org/html/2603.28914v1),
  Capilla et al. (2026), examines MS Copilot on an architecture-course ATAM exercise.
  It adds direct quality-scenario evaluation evidence, including useful risks and
  tradeoffs and expert rejection of some insufficiently contextualized risks.
  It does not compare model families or test real-PR multi-model deliberation.
- [Using LLMs in Generating Design Rationale for Software Architecture Decisions](https://doi.org/10.1145/3785010),
  Zhou et al., TOSEM (2026), with [accessible manuscript](https://arxiv.org/html/2504.20781v3),
  evaluates 100 architecture problems, five older models, and several prompting
  strategies. Rationale generation can contribute useful arguments absent from
  recorded human rationale, but overlap metrics and older-model results do not
  establish modern family specialties or reveal the original author's intent.

## Reusable engineering disciplines

Inspected Matt Pocock source revision:
[`5beb35b040e3de97e54fa9e386d7e42d15c295c2`](https://github.com/mattpocock/skills/tree/5beb35b040e3de97e54fa9e386d7e42d15c295c2).

- [Domain modeling](https://github.com/mattpocock/skills/blob/5beb35b040e3de97e54fa9e386d7e42d15c295c2/skills/engineering/domain-modeling/SKILL.md)
  and [ADR format](https://github.com/mattpocock/skills/blob/5beb35b040e3de97e54fa9e386d7e42d15c295c2/skills/engineering/domain-modeling/ADR-FORMAT.md)
  supply consistent domain language and the all-three ADR threshold: meaningful
  reversal cost, surprising context, and real tradeoff. The concise ADR format
  allows proposed status. Council adopts that policy while retaining its
  recommendations-and-drafts scope.
- [Codebase design](https://github.com/mattpocock/skills/blob/5beb35b040e3de97e54fa9e386d7e42d15c295c2/skills/engineering/codebase-design/SKILL.md)
  supplies interface, depth, seam, leverage, and locality vocabulary.
  [Design It Twice](https://github.com/mattpocock/skills/blob/5beb35b040e3de97e54fa9e386d7e42d15c295c2/skills/engineering/codebase-design/DESIGN-IT-TWICE.md)
  is an optional method for a consequential alternative-design question, not an
  unconditional redesign stage or already-defined peer debate.
- [Grilling](https://github.com/mattpocock/skills/blob/5beb35b040e3de97e54fa9e386d7e42d15c295c2/skills/productivity/grilling/SKILL.md)
  supplies dependent question rounds and the separation of inspectable facts from
  user decisions. Council uses this discipline only for necessary clarification
  after its readout; it does not require full user participation in deliberation.
- [Code review](https://github.com/mattpocock/skills/blob/5beb35b040e3de97e54fa9e386d7e42d15c295c2/skills/engineering/code-review/SKILL.md)
  separates standards and specification compliance.
  [Improve codebase architecture](https://github.com/mattpocock/skills/blob/5beb35b040e3de97e54fa9e386d7e42d15c295c2/skills/engineering/improve-codebase-architecture/SKILL.md)
  investigates architectural friction. Council adds contextual tradeoff deliberation;
  neither full workflow is a prerequisite. Installed versions can differ, so follow
  the target repository's actual conventions.

pstack source revision:
[`25e2e7a8c0d58a1f99171bbb33c550d43047fcbe`](https://github.com/sadbytes/pstack/tree/25e2e7a8c0d58a1f99171bbb33c550d43047fcbe).

- [Interrogate](https://github.com/sadbytes/pstack/blob/25e2e7a8c0d58a1f99171bbb33c550d43047fcbe/skills/interrogate/SKILL.md)
  is a precedent for independent model reviews with common facts and traceable
  dispositions. Repeated findings are not automatically calibrated confidence.
- [Arena](https://github.com/sadbytes/pstack/blob/25e2e7a8c0d58a1f99171bbb33c550d43047fcbe/skills/arena/SKILL.md)
  explores candidates and cross-judging. Its source instructions are workflow
  precedents, not efficacy studies or required council generation stages.

## Moderation, dissent, and reporting

- [IETF RFC 7282](https://www.rfc-editor.org/rfc/rfc7282.html), §§2–6, is Informational
  guidance: examine reasons for objections, distinguish considering an objection
  from accommodating it, and provide a reasoned response. Supporter counts or
  capitulation cannot answer a technical concern. Council borrows objection handling,
  not the IETF chair's institutional decision authority.
- [NASA NPR 7120.5F, Chapter 3](https://nodis3.gsfc.nasa.gov/displayDir.cfm?Internal_ID=N_PR_7120_005F_&page_name=Chapter3),
  §3.4, distinguishes facts, competing positions/rationales, impacts, recommendations,
  and management disposition. Council adapts faithful representation into a narrow
  dissent-originator check; it does not import NASA's approval or escalation system.
- [Reuters standards and values](https://reutersagency.com/about/standards-values/)
  guide accurate sourcing, faithful paraphrase, context, unknowns, and fair treatment
  without equal space for every view. Council's significance-first account is an
  adaptation, with recommendations distinct from the reported discussion.

## Empirical model and review evidence

- [ReConcile](https://aclanthology.org/2024.acl-long.381/),
  [full text v3](https://arxiv.org/html/2309.13007v3), finds heterogeneous-team gains
  on reasoning benchmarks. Its system combines model choice, discussion, confidence
  weighting, and corrective examples. This does not establish an optimal council
  roster, a family specialty, or superiority to repeated use of the strongest model.
- [Correlated Errors in Large Language Models](https://arxiv.org/html/2506.07962v1)
  finds shared errors, including across providers and architectures. Family diversity
  is a selection heuristic rather than statistical independence or confidence.
  Its conditional wrong-answer agreement rates do not predict PR-review accuracy.
- [Debate or Vote](https://arxiv.org/html/2508.17536v1) separates initial aggregation
  from debate on benchmark tasks. More rounds are not reliably better. Its theory
  has specific assumptions and its findings do not impose a round cap on council;
  measure what discussion adds and whether revisions have evidentiary reasons.
- [LLM Evaluators Recognize and Favor Their Own Generations](https://proceedings.neurips.cc/paper_files/paper/2024/hash/7f1f0218e45f5414c79c0679633e47bc-Abstract-Conference.html)
  documents self-recognition/preference in judging generated text. It motivates
  scrutiny of synthesis but does not prove that author-moderated code review is
  necessarily biased or that a fresh/different-family moderator eliminates bias.
- [Are LLMs reliable code reviewers?](https://link.springer.com/article/10.1007/s10515-026-00638-5)
  studies requirement-conformance judgments on paired correct/buggy functions.
  Prompt demands can increase rejection of correct code. Its rates do not forecast
  council outcomes; allow quiet results and measure unsupported objections.

## User attention and clarification

- [The Cost of Interrupted Work: More Speed and Stress](https://www.ics.uci.edu/~gmark/chi08-mark.pdf),
  Mark et al. (CHI 2008), found faster interrupted task-work with greater reported
  workload/stress. The timing measure excludes interruption handling; it establishes
  neither slower nor faster total council completion.
- [Principles of Mixed-Initiative User Interfaces](https://www.microsoft.com/en-us/research/wp-content/uploads/2016/11/chi99horvitz.pdf),
  Horvitz (CHI 1999), weighs clarification value against disturbance and uncertain
  goals. End-only council interaction is a deliberate workflow choice, not a result
  proven by that framework. Account for delayed-clarification rework as well as
  attention burden and synchronous waiting.

## Host and model guidance

- [Codex/ChatGPT skill metadata](https://learn.chatgpt.com/docs/build-skills),
  [Copilot CLI skill reference](https://docs.github.com/en/copilot/reference/copilot-cli-reference/cli-command-reference#skills-reference),
  [Copilot plugin paths](https://docs.github.com/en/copilot/reference/copilot-cli-reference/cli-plugin-reference),
  [Cursor skills](https://cursor.com/docs/skills), and
  [Cursor subagents](https://cursor.com/docs/subagents)
  describe host configuration, explicit invocation, and separate contexts.
  Discover live capabilities rather than assuming every surface exposes them.
- [OpenAI model selection](https://developers.openai.com/api/docs/guides/model-selection)
  and [Anthropic model selection](https://platform.claude.com/docs/en/about-claude/models/choosing-a-model)
  recommend task-specific quality/cost/latency evaluation.
  Inspected [Sol](https://developers.openai.com/api/docs/models/gpt-6.1-sol),
  [Luna](https://developers.openai.com/api/docs/models/gpt-6-luna), and
  [Opus](https://platform.claude.com/docs/en/models/opus-5-5/overview) pages supplied
  workload positioning, not a council-moderator ranking. Exact available models,
  effort options, prices, and capabilities change; these historical examples are
  not runtime defaults or family-to-perspective assignments.

## Evaluate the adaptation

Compare material evidence-backed considerations, missed tradeoffs, unsupported
objections, and user resolution effort against a capable single reviewer. Include
changes that merit no concern. Record actual model/settings, evidence access,
coverage, substitutions, time/cost where observable, and fidelity of dissent.

Vary model diversity, assigned perspectives, discussion, and context freshness
separately before claiming which mechanism helped. Compare preserved initial
positions with the final account: what useful concern disappeared, what false
concern was amplified, and what evidence actually caused a revision? Human
adjudication should not be replaced by the council's own judge. A small pilot can
inform a practical choice, not prove general effectiveness.

# Model diversity in council review

Research date: 2026-10-25. Scope: design research for reviewing an implemented change or PR and its considerations beyond correctness. This note selects five empirical papers and one source-code comparison; it does not settle the council workflow or implement a skill.

## What the evidence supports

Different model families can supply complementary assessments, but family labels do not establish independent judgment. There is positive evidence for heterogeneous teams on bounded reasoning tasks, and substantial evidence that models still share errors. Discussion and synthesis are separate interventions whose benefits need testing. [ReConcile](https://arxiv.org/html/2309.13007v3), [Correlated Errors](https://arxiv.org/html/2506.07962v1), [Debate or Vote](https://arxiv.org/html/2508.17536v1).

For council, the plausible benefit is an additional **material, evidence-backed consideration** that the strongest individual reviewer missed. More comments, different wording, or greater agreement would not demonstrate that benefit. This is a proposed success criterion, not a result established by these papers.

## Selected empirical evidence

### 1. Heterogeneous reasoning can help, with important confounds

Chen, Saha, and Bansal, [ReConcile: Round-Table Conference Improves Reasoning via Consensus among Diverse LLMs](https://aclanthology.org/2024.acl-long.381/) (ACL 2024; [full text](https://arxiv.org/html/2309.13007v3), §6.2, Tables 5, 7–8).

**Study result.** On StrategyQA, the full ChatGPT/Bard/Claude2 system scored 79.0%, versus 72.2% when all three agents used ChatGPT: a 6.8 percentage-point difference. Initial explanations were also less similar across models by the paper's BERTScore measure. Team accuracy rose from 74.3% before discussion to 79.0% after two rounds, then 78.7% after three. The system combined diversity, confidence weighting, and examples of corrective human explanations.

**Limit.** Replacing the team with ChatGPT also changes constituent capability; this is not a clean demonstration that diversity beats repeated use of the strongest constituent. Textual dissimilarity is not proof of independent errors. These were reasoning benchmarks, not architectural assessments.

**Proposed adaptation.** Test whether another family contributes useful evidence or alternatives. Do not assume that heterogeneous discussion or confidence-weighted voting transfers as a package.

### 2. Cross-family agreement can still reflect shared mistakes

Kim, Garg, Peng, and Garg, [Correlated Errors in Large Language Models](https://arxiv.org/html/2506.07962v1) (2025; inspected arXiv v1, §§3–4).

**Study result.** Analysis covered more than 350 models across leaderboard and résumé assessments. On HELM, when both models were wrong, their wrong answers agreed about 60% of the time; uniform selection among the three wrong choices would agree one-third of the time. Shared provider or architecture predicted more agreement, but more accurate models also shared errors across providers and architectures. Using a model's answers as evaluation labels inflated apparent performance of weaker models sharing those errors.

**Limit.** Conditional wrong-answer agreement is not a general correlation coefficient or an estimate of PR-review accuracy. Observational associations do not identify which training differences caused complementarity.

**Proposed adaptation.** Treat family diversity as a useful selection heuristic, not calibrated confidence. Record exact model identities and observed complementary findings; retain an evidence-backed minority concern even when others miss it.

### 3. Independent aggregation deserves its own baseline

Choi, Zhu, and Li, [Debate or Vote: Which Yields Better Decisions in Multi-Agent Large Language Models?](https://arxiv.org/html/2508.17536v1) (2025; inspected arXiv v1, §§3, 6, Appendix H).

**Study result.** Across seven benchmark datasets, much of the gain attributed to debate came from aggregating initial answers. For five Qwen2.5-7B agents, Table 1 reports mean accuracy of 76.91% for initial majority voting, versus 73.77% for two-round decentralized debate and 70.50% for five rounds. Debate sometimes helped on individual tasks. Their theoretical no-expected-improvement result depends on a specified homogeneous Bayesian update model. Their “heterogeneous” extension varies personas, rather than model families; open-ended summarization uses a different readout.

**Limit.** This does not prove all debate is harmful or establish a universal social-conformity law. A list of design considerations has no natural majority-vote answer.

**Proposed adaptation.** Preserve independent first assessments and compare synthesis with and without discussion. A changed view could require new evidence or a specific rebuttal, rather than an instruction to reach agreement. That safeguard remains untested for council.

### 4. A synthesizer or cross-judge is another fallible model

Panickssery, Bowman, and Feng, [LLM Evaluators Recognize and Favor Their Own Generations](https://proceedings.neurips.cc/paper_files/paper/2024/hash/7f1f0218e45f5414c79c0679633e47bc-Abstract-Conference.html) (NeurIPS 2024; [paper](https://proceedings.neurips.cc/paper_files/paper/2024/file/7f1f0218e45f5414c79c0679633e47bc-Paper-Conference.pdf), §§2–4).

**Study result.** In summarization experiments, models could recognize their own generations and favor them beyond human quality judgments. Fine-tuning interventions found a relationship between self-recognition and self-preference, with controls addressing alternative explanations. The authors distinguish model-level evidence from an unresolved example-level causal explanation.

**Limit.** This is evidence about judging generated text, not proof that a model favors its own code architecture, nor that a different-family judge eliminates bias.

**Proposed adaptation.** Consider hiding reviewer identity and randomizing finding order during adjudication, while preserving provenance for the final report. Judge claims against repository evidence and explicit criteria. Anonymity cannot hide all stylistic cues, and neither this safeguard nor cross-family judging guarantees neutrality. Human evaluation should remain independent of whichever model produced the review.

### 5. Code-review evidence warns against rewarding extra objections

Jin and Chen, [Are LLMs reliable code reviewers? Systematic overcorrection in requirement conformance judgement](https://link.springer.com/article/10.1007/s10515-026-00638-5) (Automated Software Engineering, 2026; §§3–5, Table 2).

**Study result.** Five models assessed paired correct and buggy implementations from HumanEval, MBPP, and QuixBugs under three prompts. Requiring explanations and repairs could increase rejection of correct code. For example, the authors report GPT-4o's HumanEval false-rejection rate rising from 26.2% under the direct prompt to 73.2% under the fuller prompt. Prompt effects and false-acceptance tradeoffs varied across models and benchmarks.

**Limit.** This evaluates function-level requirement conformance, not maintainability, operational tradeoffs, or heterogeneous review teams. The numerical rates should not be forecast onto council.

**Proposed adaptation.** Permit “no material concern”; assess unsupported objections and review burden alongside discoveries. Requiring every reviewer to find problems, propose repairs, or challenge everything could reward noise. That risk is a hypothesis for council, not an experimentally measured effect here.

## pstack as a workflow precedent

Inspected repository snapshot: `sadbytes/pstack` at commit `25e2e7a8c0d58a1f99171bbb33c550d43047fcbe`. These are source instructions, not efficacy studies, and are not invoked by this research.

- [interrogate](https://github.com/sadbytes/pstack/blob/25e2e7a8c0d58a1f99171bbb33c550d43047fcbe/skills/interrogate/SKILL.md) sends the same intent, diff, context, and rubric to multiple model reviewers, then deduplicates findings and records lead dispositions, disagreements, and dismissals. Its assumption that repeated findings are automatically the highest-confidence signal needs qualification in light of correlated-error evidence.
- [arena](https://github.com/sadbytes/pstack/blob/25e2e7a8c0d58a1f99171bbb33c550d43047fcbe/skills/arena/SKILL.md) generates competing artifacts, uses a rubric and a cross-judge preferably from another family, chooses a base, and grafts selected ideas. That is a different task from reviewing one implemented change. Its traceable judgments are a possible precedent; its generation/picking/grafting stages do not establish a council review contract.

## Candidate mechanisms to investigate, not workflow decisions

1. **Complementary discovery.** Give reviewers a common scope and factual evidence packet, with separate access to repository exploration and no visibility into peers' initial findings. This controls exposure to peer conclusions; it does not make their errors statistically independent. Log what each actually inspected, since unequal evidence can explain disagreement.
2. **Separate family diversity from assigned perspectives.** A family comparison can use the same broad questions about relevant constraints, alternatives, consequences, and missing rationale. Assigning one family security and another maintainability would confound model effects with coverage and make corroboration hard to interpret. Perspectives could be a separate experiment; the literature does not justify vendor personality assignments.
3. **Challenge claims selectively.** If exchange is tested, ask another reviewer to verify or rebut a concrete concern against source evidence, including whether the alleged requirement exists. Preserve the initial record and the reason for revision. An extra round could stop when it produces no material evidence; a fixed round or cost cap is also an option to test.
4. **Make synthesis auditable.** Compare the union of initial concerns with the final report. Distinguish independently raised concerns from later agreement, and explicit rebuttal from silence. Retain reasons for dismissal and unresolved disagreements. Corroboration can prioritize verification, but should not itself establish severity or require action.
5. **Describe the fallback honestly.** Same-model independent runs can still sample different assessments. Report exact versions, provider/family information where known, and substitutions; do not call multiple sessions independent model families. Provider identity and model lineage are different, and closed training histories limit certainty. Whether cross-family review earns its extra cost is an empirical question.

## Limits of transfer to architecture and broader considerations

None of the selected studies directly demonstrates improved review of architecture, justified tradeoffs, or preservation of rationale in real PRs. Most target a discrete correct answer; council can have several defensible outcomes under different future requirements and constraints. A concern may be sensible but irrelevant to this change, and an alternative may be better on one dimension but worse overall.

The transfer proposed here is therefore about mechanisms to test: complementary discovery, resistance to premature convergence, and traceable adjudication. It does not establish a best family combination, number of reviewers, debate format, confidence formula, or merge gate. Nor can these studies determine when an ADR is required. Missing documented rationale, inferred rationale, and a demonstrated design defect need separate treatment in the design interview.

## A lightweight local validation proposal

This is an experiment sketch for discussion, not an adopted process or an evaluation run performed here.

- **Choose the comparator first.** Use a few calibration changes to identify the strongest available single model for this task. Freeze its prompt and settings before a separate set of roughly 8–12 varied PRs. Include small changes with no material concern and changes where broader considerations matter. Hide historical review conclusions and later outcomes during assessment.
- **Compare three conditions on the same changes.** A: the strongest single model with an adequate reasoning/exploration budget. B: several independent runs of that model. C: the same number of independent reviewers across capable families. B and C use the same evidence access, rubric, output constraints, and synthesis procedure. Give A a comparable total budget as a principal comparison, and record the ordinary one-pass result where affordable. This distinguishes additional computation from family diversity.
- **Hold adjudication steady.** For B and C, use the same lead model and prompt, hide identities, and shuffle findings. Record tokens, actual monetary cost, elapsed time, evidence access, dropouts, and fallback. Equal calls or tokens alone do not imply equal capability or cost across providers; disclose residual budget differences.
- **Have a maintainer assess blinded findings.** Score concrete evidence, relevance to the change, consequence, and usefulness for a decision. Record valid unique considerations beyond A, unsupported objections, severity mistakes, and time needed to resolve them. Several defensible tradeoffs can coexist. Use two human ratings on disputed material findings if feasible; do not use the council's own judge as its sole evaluator.
- **Measure the discovery/synthesis gap.** Which useful findings appeared in raw reviews but vanished in synthesis? Which false concerns were amplified? Count overlap among both accepted and rejected concerns, rather than treating different wording as diversity. A historical issue list can anchor recall, but is not exhaustive ground truth; call recovery against it “known-issue recovery.”
- **Test discussion only if there is something to resolve.** On a few disagreement cases, compare the frozen independent synthesis against one bounded evidence-focused challenge round. Inspect whether changes came from new evidence, a valid rebuttal, or unsupported convergence. This isolates discussion's incremental value.
- **Decide what would justify the cost before running.** For example: additional material considerations a maintainer accepts, without an unacceptable increase in unsupported concerns or resolution time. Report per-PR paired results and uncertainty; a small pilot supports a practical decision, not a general claim of superiority. Revisit model selection when versions materially change.

## Questions this research makes concrete

- Is council's main benefit broader discovery, better adjudication of tradeoffs, or both? Those require different success measures.
- Should all reviewers independently assess the same considerations, or should some effort cover explicit gaps? How much redundant coverage is worth paying for?
- Is synthesis allowed to omit an evidence-backed minority concern, or must its disposition remain visible?
- What evidence warrants a challenge round, and what cost or latency limit makes it worthwhile?
- Who decides whether a consideration is material, and what improvement over the strongest single reviewer would justify multi-family review?

These questions can now be discussed without assuming model personalities, treating consensus as verification, or importing a benchmark's debate protocol wholesale.

## model-to-perspective fit

Follow-up research: 2026-10-25. This bounded follow-up inspected two additional primary studies, including the rationale paper's accessible arXiv v3. **No evidence was found in these sources for assigning one model family reliability and another performance.** This is a finding about this selection, not a claim that no such evidence exists. Task-fit pairing is a testable possibility; neither family labels nor deliberation establish it.

- **Closer evidence for architecture evaluation, without a family comparison.** [Towards Supporting Quality Architecture Evaluation with LLM Tools](https://arxiv.org/html/2603.28914v1) (2026, v1, §§IV–VI) compares MS Copilot with student teams and experienced architect co-authors on one architecture/ATAM exercise. Seven of nine student groups were retained. Copilot added risks, sensitivity points, and tradeoffs, but experts rejected several inferred risks as insufficiently critical or contextualized; the authors found no clear outperformance of experts on tradeoff analysis. This supports investigating contextual quality-scenario assistance, not assigning families to attributes: there is no controlled comparison of model families or model-by-attribute fit. The earlier “none of the selected studies” limit above applies to the **initial five empirical studies**. This new study supplies direct architecture-evaluation evidence, but still does not test real-PR, multi-model deliberation.
- **Model/prompt/metric differences are not attribute specialties.** [Using LLMs in Generating Design Rationale for Software Architecture Decisions](https://doi.org/10.1145/3785010) ([inspected arXiv v3](https://arxiv.org/html/2504.20781v3), §§3.2–3.5, 4.1–4.3, Tables 5–8) evaluates 100 architecture problems using gpt-3.5-turbo, gpt-4-0613, gemini-1.0-pro, llama3-8B, and mistral-7B. Gemini-1.0-pro leads zero-shot F1; gpt-3.5-turbo leads F1 with structured reasoning and agents, while gpt-4-0613 leads recall across all three. These metrics measure overlap with recorded human rationale; unmatched arguments can still be helpful. The five-role pipeline identifies aspects, retrieves information, analyzes, reviews, and synthesizes. Its comparison bundles roles, retrieval, and extra calls; it does not isolate role assignment from exchange effects or test family-to-attribute routing. Results for these older individual models cannot establish modern family specialties, and rationale generation differs from reviewing an implemented PR.

**Proposed practical policy and evaluation; not adopted.** Keep a varied roster of capable models. As already agreed in the design interview, the moderator assigns distinct quality priorities on the provisional agenda **before private preparation**, with shared facts and freedom to examine cross-attribute effects. Do not expect discussion to supply a missing lens or reveal a reliable family specialty. Consider only soft, revisable pairing preferences supported by observed performance on comparable cases, tied to exact model versions, settings, tools, and context rather than vendor reputation.

To evaluate pairing, rotate models across lenses on the same or matched agenda items, and have another model independently cross-check a sample under the **same lens**. Hold evidence access and budgets comparable; blind maintainer ratings of material, grounded considerations, missed tradeoffs, unsupported concerns, and review cost. Compare evidence-based pairing with balanced rotation on held-out cases. Require repeatable **relative benefit on that lens**, beyond a model being stronger overall, before preferring a pairing. Frozen private positions measure preparation; additional findings, corrections, and lost concerns after discussion measure its separate contribution. Recheck preferences after model changes. Rotation and cross-checks prevent permanently confounding a model's contribution with the perspective it was assigned; they do not themselves prove family-level fit.

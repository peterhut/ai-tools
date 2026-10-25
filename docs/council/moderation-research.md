# Council moderation research

Research date: 2026-10-25. Status: input to the design interview, not an accepted workflow. Four external primary sources are selected below. The delegated research task created this note and spawned no further agents.

## Design context and limits

Council reviews an implemented change or PR beyond correctness and may challenge its requirements. The interview direction is independent initial broad reviews, followed by perspectives chosen from actual tensions and real exchange between reviewers. Different model families are preferred, with a disclosed same-model fallback. Interactive grilling should draw on that debate. Outputs remain recommendations and ADR drafts. These are task constraints, not conclusions from the sources; see [design.md](design.md) and the accompanying [model-diversity research](model-diversity-research.md).

The user has suggested an agenda, a moderator, and a concise final account before recommendations, resembling a newspaper report rather than full minutes. Whether the main thread moderates and summarizes or a separate fourth agent does so remains open.

The sources describe human practices in different institutions. They supply mechanisms worth considering, not evidence that those mechanisms improve AI councils. This note does not import complete workshops, voting, management hierarchies, or institutional decision rights.

## Selected source facts

### 1. SEI ATAM: turn concerns into scenarios and report risk themes

In *Using the SEI Architecture Tradeoff Analysis Method to Evaluate WIN-T: A Case Study* (Clements, Bergey, and Mason, September 2005), the team leader and project liaison prepared participation and documentation; the evaluation leader helped prepare the business-driver and architecture presentations. Table 1 combines team leader/evaluation leader in one person and questioner/scribe in another. These are distinguishable duties, not necessarily separate people. Sections 4.5–4.7 describe facilitated scenario construction and analysis: utility-tree scenarios are ranked by importance and difficulty, while broader stakeholder scenarios are prioritized through voting. Evaluators facilitate walkthroughs; stakeholders contribute substantive analysis. Sections 4.9–4.10 synthesize individual risks into themes and explain consequences for business drivers. [SEI case study, §§4.1–4.10, printed pp. 11–43](https://www.sei.cmu.edu/documents/2080/2005_004_001_14522.pdf).

**Candidate adaptation:** prepare a small agenda of concrete situations the PR must handle, then connect findings to the user's goals. Borrow scenario specificity and thematic reporting; omit voting and workshop scale. The case supports distinguishing functions while allowing combined assignments; it does not establish an optimal agent count.

### 2. IETF: examine objections rather than count supporters

RFC 7282, *On Consensus and Humming in the IETF* (June 2014), is explicitly Informational, not a new procedural rule. Section 2 asks for the reasons behind objections. Section 3 distinguishes considering an issue from accommodating it: a chair may recognize rough consensus despite continuing dissent when the tradeoff has been examined and justified. That judgment requires technical understanding and is appealable. Section 6 rejects ignoring an unanswered technical objection merely because many people support proceeding. Sections 4–5 treat polling as a possible way to begin discussion, not a decision substitute, and reject consensus as an end in itself. [RFC 7282, introduction and §§2–6](https://www.rfc-editor.org/rfc/rfc7282.html).

**Candidate adaptation:** track material objections and their answers, without a vote or unanimity requirement. A moderator needs enough understanding to detect a non-answer; facilitation cannot be wholly content-blind. Council's moderator could close a discussion with explicit disagreement remaining, but would not inherit an IETF chair's authority to determine the project's decision.

### 3. NASA: preserve the issue, competing rationales, and disposition

NASA's NPR 7120.5F, Chapter 3, §3.4, distinguishes agreement, disagreement with willingness to support, and Formal Dissent. When time permits, disagreeing parties jointly document agreed facts, competing positions, rationales, impacts, and recommendations, with each view's representative approving that account. Management's decision is separately documented and supplied to the dissenter; further appeal is possible. Section 3.3.1 retains the program/project manager's responsibility alongside the Technical Authority process. These arrangements distinguish recording a view, reviewing it, and exercising institutional authority. [NASA NPR 7120.5F, §§3.3.1 and 3.4](https://nodis3.gsfc.nasa.gov/displayDir.cfm?Internal_ID=N_PR_7120_005F_&page_name=Chapter3).

**Candidate adaptation:** keep a compact account of a consequential disagreement and let its participants correct misrepresentation. That check concerns fidelity, not permission to suppress an inconvenient finding. The user's decision, if given, belongs separately from the agents' positions. Do not reproduce NASA's escalation chain, approvals, or technical-authority structure for ordinary PR review.

### 4. Reuters: report faithfully without equal-space balance

Reuters' *Standards and Values* requires accurate sourcing, cross-checking, context, and explicit acknowledgment of unknowns. Its quotes guidance permits paraphrase and reported speech while warning against selective quotation that distorts a speaker's meaning. “Take no side, tell all sides” says fairness does not always require equal space. “Opinion and Analysis” distinguishes fact-based reporting from labeled opinion. Its checking-back guidance allows factual verification while resisting source control over editorial output. [Reuters standards, “Sourcing,” “Quotes,” “Checking back with sources,” “Take no side, tell all sides,” and “Opinion and Analysis”](https://reutersagency.com/about/standards-values/).

**Candidate adaptation:** the final account can select and paraphrase the consequential exchange, attribute disputed claims, and separate what happened from subsequent recommendations. Give a lone well-supported objection adequate prominence; repetition or agent headcount need not earn space. A short, significance-first lead is a proposed council writing choice here, not a format prescribed by this source.

## Responsibilities to distinguish

The following allocation is a design proposal drawn from those mechanisms, not an institutional rule or evidence about AI performance.

- **Moderator:** establishes scope, assembles and orders the agenda, directs claims to the reviewer best placed to answer, identifies unanswered objections, brings user decisions into the discussion, and explains why another round would or would not help. Can challenge an unsupported assertion without becoming an extra advocate. Any substantive position it introduces should be labeled and exposed to reviewer challenge.
- **Substantive evaluators:** inspect the change and surrounding evidence, challenge assumptions and requirements, propose concrete alternatives, answer one another, and state what would change their assessment. A later perspective is a useful question or responsibility, not a command to defend a predetermined side.
- **Reporter/summarizer:** explains the central tension, decisive evidence, movement in positions, and remaining uncertainty. Preserves important dissent and checks its portrayal. Selects what matters without inventing a compromise or converting a recommendation into a decision.
- **Decision authority:** remains with the user or the repository's actual authorized decision-maker. Ending deliberation, reporting convergence, accepting risk, accepting an ADR, and authorizing implementation are different acts. Council supplies advice and drafts; it does not approve or merge the PR.

Distinguishing these functions does not itself require separate agents for each.

## Lean candidate protocol for discussion

Everything in this section is a proposed AI adaptation. It is deliberately smaller than the source processes and is not settled policy.

1. **Frame the review.** Fix the implemented change/PR boundary, relevant requirements, available evidence, and user concerns. The user may suggest agenda items. The moderator treats that agenda as a starting point that can be amended after inspection, including by a challenge to the requirements.
2. **Collect independent broad reviews.** Give evaluators the same broad remit before showing them one another's conclusions. Record the actual model families and any same-model fallback. Ask for evidence, consequential scenarios, assumptions, and alternatives; avoid assigning permanent opposing camps.
3. **Form the working agenda from real tensions.** Merge duplicates without erasing differing premises. Order a small set by consequence, uncertainty, and dependence on earlier questions. An item could read: “Keeping the old configuration format lowers migration effort but requires two validation paths; what compatibility promise justifies that cost?” Assign follow-up perspectives only now. Keep deferred material concerns visible with a reason for deferral.
4. **Exchange arguments.** For each material tension, send the concrete claim, evidence, and objection to another evaluator; return its answer to the originator for response or revision. Require engagement with the specific argument, not a fresh disconnected review. Preserve concessions and the strongest remaining counterargument. One exchange followed by a targeted follow-up is a possible starting budget, not an agreed limit.
5. **Bring consequential choices to the user.** The moderator asks the currently answerable questions, informed by the exchange, with a recommendation and its tradeoff. Agents investigate inspectable facts. User priorities, compatibility promises, and acceptable costs belong to the user. Feed answers back only to affected agenda items; revisit an earlier conclusion when its premise changes. Grilling can continue through dependent choices without requiring a complete re-review each time.
6. **Close with an explicit state.** Each material item has an evidence-backed conclusion, a clearly recorded disagreement, or a named missing fact/user decision. Further internal rounds need a specific expected contribution: new evidence, an unanswered counterargument, or a changed premise. Repetition alone is insufficient. A resource limit or user request can end the session with open issues; it cannot convert them into agreement. Offer one brief opportunity to correct a lost objection or misrepresented position.
7. **Report, then recommend.** First give a concise account: the main finding and stakes, why the central tradeoff mattered, what changed through discussion, and the meaningful dissent or uncertainty that remains. Then provide recommendations with their reasons and conditions. Attach only eligible proposed ADR drafts, following the local all-three test: meaningful reversal cost, surprising without context, and a genuine tradeoff. Unconfirmed rationale stays unconfirmed. See [the interview's ADR policy](design.md#rationale-preservation) and [verified local skill details](matt-skills-integration-research.md#adr-and-domain-documentation-verified-facts).

For an unresolved item, a compact supporting record could contain: question; shared facts with evidence; competing claims and consequences; response to the strongest objection; current positions; missing evidence or user choice; and disposition with reason. This is an optional supporting record, not a required full transcript or a new ADR format. Unsupported speculation can be identified and set aside without receiving a manufactured opposing position.

## Main thread or a separate fourth agent?

**Main thread as moderator and reporter, with three substantive evaluators:** keeps the user interview and its context together and avoids an additional handoff. The risk is concentrated control over agenda, questioning, closure, and portrayal, especially if the main thread also authored the change or already favors an approach. Reviewer corrections to the account could expose distortion, but cannot establish independence.

**Separate fourth agent as moderator/reporter, with the main thread handling the user conversation:** makes the process assignment explicit and can give moderation sustained attention. It also adds context transfer, cost, and latency; filtered user answers can distort deliberation. The dedicated agent still combines facilitation and reporting, and a separate context or model does not guarantee neutrality. Ownership of user questions and final wording would need to be explicit to avoid competing coordinators.

**Possible intermediate choice:** let the main thread moderate and draft the account, then ask an existing evaluator to check only whether evidence and dissent were represented accurately. This adds a check without a permanent extra coordinator, but does not remove agenda bias or provide an independent substantive judgment.

None of the four sources selects between these topologies. The SEI example demonstrates combined duties in a human team; the other sources help specify responsibilities and accountability. The interview still needs to choose an arrangement and assess its cost and usefulness.

## Drawbacks and decisions still open

- **Agenda compression:** prioritization can hide a consequential minority concern. Decide how deferred concerns remain visible and when the user should see them.
- **Anchoring after independence:** exchange may cause persuasive convergence rather than better reasoning. Decide how much original assessment to retain and how to distinguish new evidence from mere adoption of another agent's phrasing.
- **Moderator influence:** choosing questions and declaring repetition are substantive judgments. Decide whether the moderator may recommend during the interview, how it labels its own claims, and who checks their treatment.
- **Stopping versus coverage:** a small internal budget limits cost but can leave important objections unresolved. Choose a default budget, extension trigger, and presentation of unfinished work; do not make either unanimity or exhaustion the success criterion.
- **Readable account versus auditability:** short reporting can erase premises; extensive records can overwhelm. Decide whether supporting issue records are always retained or only for consequential dissent, and what evidence must remain directly inspectable.
- **Fidelity check versus editorial veto:** asking reviewers whether their views were understood can improve accuracy or invite endless revision. Decide the narrow correction opportunity and how unresolved disputes about the account itself are disclosed.

These are interview questions, not reasons to implement a council now. A later trial could assess whether the arrangement surfaces consequential evidence and usable choices at acceptable cost, while retaining unresolved dissent. The human sources do not establish that outcome for AI.

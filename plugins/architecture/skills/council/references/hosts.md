# Host execution

Read before the first dispatch. Use the current host's actual tool schemas; names,
available models, options, and permission controls vary. The workflow requires
separate executed agents, not host-authored conversations between personas.

## Capability and invocation

Discover available delegation and explicit model choices. Prefer capable model
families where available, disclose the roster and any same-model fallback, and
honor user-specified models/settings. Family diversity is optional; actual separate
review contexts are required. If unsupported, explain the missing capability.

Codex's entrypoint uses `agents/openai.yaml` with
`policy.allow_implicit_invocation: false`. Copilot/Cursor use the
`explicit-skills/council/SKILL.md` entrypoint with
`disable-model-invocation: true`, loading this same skill body. The legacy Copilot
manifest searches that entrypoint directory before the canonical skills directory.
Keep the linked plugin layout intact. This split preserves explicit invocation
while passing the repository-mandated historical Codex plugin validator, which
rejects that frontmatter flag.
The [historical validator check](https://github.com/openai/codex/blob/985cf47a4eb6084b2ff6b30ebdb1216acda85bb4/codex-rs/skills/src/assets/samples/plugin-creator/scripts/validate_plugin.py#L469-L475)
is the packaging constraint; it is not a runtime dependency.

Do not silently replace a user-selected model or mode. Report an unavailable
selection or relevant execution limitation. Read-only is the review task boundary;
use tool/sandbox enforcement where offered, without claiming that every host
technically prevents writes.

## Separate preparation and continuity

Use clean task briefs without inherited peer conclusions for initial preparation.
Pass the evidence identity/paths, agenda, quality drivers and unknowns, assigned
perspective, common evaluation questions, and read-only scope. Ask reviewers to
investigate facts, not ask the user mid-deliberation, and not dispatch extra agents.

All initial positions are recorded before peer conclusions are revealed. If the
host inherits conversation history, ensure later preparations do not inherit earlier
review outputs. Supply the skill's relevant review contract in each task; do not
assume the child can see this invocation.

A logical reviewer may continue in a persistent context or in fresh one-shot agents.
For each continuation, supply its initial position, evidence, previous objections
and replies, revisions and reasons, changed premises, and the precise next question.
Keep initial positions distinguishable from later agreement. Disclose changed models,
missing continuity, failures, and coverage gaps; never fabricate a missing response.

Retain task IDs and the substantive records needed for subsequent exchanges and
fidelity checks. These are working records, not a required full transcript for the user.

## Fresh moderator

Keep user conversation and dispatch in the main thread. When a fresh moderator is
requested, send it the factual scope and user constraints rather than the host's
preferred answers. It can inspect evidence, propose the agenda/perspectives, identify
next exchanges, assess replies, and draft the account.

The host invokes the moderator and reviewers as sibling tasks and relays their
relevant records faithfully. It need not grant nested-spawn capability. Return
reviewer outputs to the moderator as source records, with model/role provenance,
rather than replacing them with the dispatcher's interpretation. The moderator's
next requests specify the logical recipient and question. Material new substantive
claims by either host or moderator receive normal reviewer scrutiny.

A fresh context does not guarantee neutrality, remove all same-family bias, or
eliminate host relay cost. Disclose relevant prior involvement.

## T3 orchestration when available

- Call `orchestrator_capabilities` for the live provider/model catalog, options,
  inherited settings, and enabled providers. Use its search when a named model is
  absent from the short list. A native tool's model list is not the full catalog.
- Follow the live contract when choosing native delegation versus `delegate_task`.
  Use T3 delegation for cross-provider work, unsupported native selections, or
  requested/inherited model options that native tools cannot reliably apply.
  Pass advertised options in `target.options`, and check returned identity/options.
- Each discussion round uses a new `delegate_task` with a distinct
  `clientRequestId`, stable across retries of that round. Include the original
  item brief, previous findings/replies/revisions, and unresolved objections.
  Preserve the returned `taskId`; do not continue a review through the backing
  child thread's send-message tool.
- Use an immutable checkout/snapshot that agents can actually read. A T3 isolated
  worktree starts at a commit and does not copy uncommitted changes; supply any
  frozen working-tree material explicitly rather than assuming it is present.
- Async completion is delivered by the host. Continue useful work or yield; use
  bounded status checks only when a result is needed mid-turn, and do not duplicate
  active work. Ordinary top-level thread creation is not a subagent mechanism.
- For a caller time budget, check the clock before dispatch and waits, and bound
  supported wait timeouts by remaining time with room to write the readout. Stop
  new exchanges at the deadline. Cancel unfinished child work when appropriate;
  disclose an uninterruptible call or overrun rather than claiming a hard guarantee.

Tool prefixes can be normalized by a harness. If catalog discovery initially omits
T3 tools, follow the host's documented lazy-tool discovery or ACP MCP transport
before concluding they are unavailable. Do not invent CLI transports or install an
orchestrator during a review.

## Other hosts

Use the available native task/subagent tools in Copilot, Cursor, or another host.
Confirm actual model control and clean context support rather than assuming a
particular tool name, configuration flag, or plan tier. If model selection is not
exposed, disclose it and use separate same-model contexts when available.

Follow the same initial-preparation and complete-turn-brief rules. Do not weaken
independence into simulated personas, or misrepresent a tool-enforced substitution
as the user's requested model. The [source guide](sources.md#host-and-model-guidance)
links current host documentation; successful execution in one host is not a test
of the others.

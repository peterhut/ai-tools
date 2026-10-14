# Independent maintainability signal trial

**Result:** No new maintenance action justified in the two examined slices. Accept the present trade-offs; retain ADR-0027/0029's existing operational revisit triggers. This is a bounded behavioral validation, not a complete maintainability judgment.

## Reproducible scope and measured signals

Repository `/tmp/photonic-architecture-pilot-20261014`, revision `3f5c709640ce02b24a34c64cc525d72ee5b81db3`. Checkout clean; history not shallow. Remote `origin` exists but freshness was not checked or fetched. Read AGENTS.md, CONTEXT.md, RCDA summary and relevant ADRs 0027, 0029, 0030; did not read architecture-review notes, prior reports, intended answers or redesign plan.

Initial collection command, run from the skill repository:

```sh
pwsh -NoProfile -File /home/peter/.t3/worktrees/ai-tools/t3code-534d01ad/plugins/architecture/skills/assess-maintainability/scripts/Get-MaintainabilityHistory.ps1 -RepositoryPath /tmp/photonic-architecture-pilot-20261014 -Revision 3f5c709640ce02b24a34c64cc525d72ee5b81db3 -MaxCommits 50 -Top 20 -MaxFilesPerCouplingCommit 30 -OutputPath /tmp/photonic-signal-history.json
```

Exit 0. No exclusions. All 50 selected first-parent commits analyzed, from `483a38a4dca1b0a3150de95c421b65523522796e` (2026-06-26) through selected revision (2026-10-03); older history exists. Unit is first-parent commit, merges compared with first parent, root with empty tree. History mixes ordinary/squash-style commits and one merge; branch-internal commits were not separately counted. Frequency includes broad commits. Coupling population is 36 commits touching 1–30 paths; 14 broader commits omitted from both joint counts and denominators. Tests/docs remain included. No churn, Code Health, maintenance effort or comparable complexity trend measured.

Measured raw-path observations:

- Todo `Pages/Todos.razor`: 15 touches (9 eligible); `Pages/Admin.razor`: 8 (4 eligible). Joint changes: 4, so observed Admin→Todos = 4/4, Todos→Admin = 4/9, normalized support = 4/36. These proportions describe this eligible history only.
- Old path `src/Photonic.AspNetCore/Subscriptions/InvalidationLogDispatcher.cs`: 8 touches (4 eligible).
- 255 rename records overall; only first 20 emitted with `-Top 20`. Collector explicitly does not join identities. LiveStore→Photonic renaming and the later Server namespace/package redesign prevent interpreting raw ranks as current semantic module rankings. No normalized module-coupling estimate was inferred.

## Investigation 1: Do jointly changing Todo pages repeat framework/auth lifecycle decisions?

**Interpretation and dismissal:** Activity merits inspection, but selected history supplies substantial counterevidence. `cb8b179` removes custom `ITodoAuthClient`, per-page access checks and navigation; both pages now use standard authorization attributes. `5edf9db` subsequently replaces component subscription plumbing with bindings. At the pinned revision, `samples/TodoApp/TodoApp.Client/Pages/Todos.razor:104` owns four declared bindings, initializes them at line 130, and submits filter/search parameters through `BindTodosAsync` at line 244. `Pages/Admin.razor:61` owns one binding, initialized at line 67. They share supported framework contracts while owning different application interactions.

A plausible next Todo filter should change its query and page binding input; no inspected evidence requires Admin changes. `905fff3` changes Todos' interactive-readiness marker and E2E synchronization rather than another auth abstraction: `tests/TodoApp.E2ETests/TodoE2ETestHarness.cs:59` waits for that marker. Do not classify all co-change or recent race fixes as one architectural cause. Accept current structure. Revisit if a new runtime lifecycle rule actually has to be repeated in multiple pages. Confidence moderate within this small slice; no deterioration claim.

## Investigation 2: Does active invalidation dispatch still own persistence and subscription lifecycle decisions?

**Interpretation and dismissal:** Earlier activity included bounded draining (`e320404`) and snapshot reuse (`c36c25a`), each supported by focused tests. Rename-aware `git diff --find-renames 5edf9db^ 5edf9db -- src/Photonic.AspNetCore/Server/Subscriptions/InvalidationLogDispatcher.cs src/Photonic.AspNetCore/Subscriptions/InvalidationLogDispatcher.cs` shows the dispatcher lost its `TDbContext`/EF dependency and now opens an `IInvalidationLog` session. Current `Server/Subscriptions/InvalidationLogDispatcher.cs:103` uses that session; line 184 captures one subscription snapshot; lines 280 and 420 delegate work to the Instance. `SubscriptionInstance.cs:183` owns phase decisions and watermark coalescing; line 273 fences completion against phase changes. ADR-0029 explicitly assigns retry scheduling/cursor acknowledgement to the dispatcher and lifecycle ordering to the Instance. This is a shared runtime protocol, not demonstrated accidental duplication.

For a future persistence-provider change, the inspected dispatcher no longer needs provider types. For a lifecycle rule change, the Instance has the relevant operation; ordering still requires protocol verification, but no extra spread beyond the documented contract was established. `tests/Photonic.UnitTests/Architecture/PackageBoundaryTests.cs:29` encodes ADR-0030's dependency constraint; acceptance tests cover paused publication, independent establishment and inline disposal. These checks were inspected, not executed.

Accept the current boundary. Additional indexing would add ownership complexity without new workload evidence; ADR-0027 already makes production-shaped predicate cost the revisit condition. ADR-0029 already records terminal-delivery debt and an observed repeated-failure trigger. No fresh operational evidence establishes either trigger. Confidence moderate for ownership localization; no claim about performance in production or comprehensive correctness.

## Limits and workflow observations

No builds/tests, remote PR/issue fetching, code edits, external writes or new constraints. Existing test sources and ADR-recorded verification are counterevidence, not fresh passing results. Unexamined areas include source generation, content transfer, client reducers and persistence capture. Broad redesign commits dominate recent structural change; old-path ranking alone would have over-prioritized concerns already addressed.

No concrete collector or skill defect encountered. Collector output made omitted populations and rename limitations explicit, and the skill's dismissal/current-revision guidance prevented activity from becoming severity. Two routine inspection assumptions needed correction: `python` was unavailable (`python3` worked), and historical dispatcher/test paths had moved (resolved with `rg --files`). Neither was a collector failure. Documentation reading was a noticeable cost, but remained relevant and bounded; no hypothetical hardening request is inferred.

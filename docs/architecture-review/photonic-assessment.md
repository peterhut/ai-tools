# Photonic bounded maintainability assessment

Pinned checkout: `/tmp/photonic-architecture-pilot-20261014`, HEAD `3f5c709640ce02b24a34c64cc525d72ee5b81db3`.

Source: [pinned Photonic tree](https://github.com/peterhut/photonic/tree/3f5c709640ce02b24a34c64cc525d72ee5b81db3). Repository-relative paths and lines below refer to this revision. This exploratory assessment informed the skill draft; the separate [ai-tools trial](ai-tools-trial.md) forward-tested the written skill.

## Result and prioritization

**Zero new maintenance findings warrant prioritization from this evidence.** The strongest change-pressure signals were Subscription lifecycle work, SQL invalidation infrastructure, and generated contract/dispatch metadata. Focused history inspection found recent consolidation, feature-driven changes and explicitly accepted limitations rather than demonstrated worsening of ownership. This is a bounded negative result, not a clean bill of health or maintainer adjudication.

Retain the existing debt triggers in `docs/limitations.md:28–39`; no independent evidence here justifies accelerating them. The proportionate option is to leave these structures alone until a concrete change scenario or an existing trigger demonstrates cost. No implementation plan is proposed.

## Evidence scope

Read `AGENTS.md`, `CONTEXT.md`, `docs/RCDA-summary.md`, `docs/agents/domain.md`, ADRs 0004, 0005, 0025, 0027, 0028, 0029, 0030 and 0032, backlog and limitations. Surveyed exactly the latest 50 first-parent commits reachable from pinned HEAD, ending at `483a38a` (not `--all`), through subjects, shortstats and path-touch counts. Used fuller stats selectively. Counts included historical names and were selection signals, not normalized defect or team-ownership metrics.

Investigated only the three candidates below, comparing selected diffs and current source/tests. Followed moves using `git log --follow`, rename-aware diffs and explicit old/current paths when Git did not detect moves. Read tests as disconfirming evidence; did not run them. PR numbers in local commit subjects are Git evidence only: no PRs, issues, CI or external services were fetched. No builds or repository writes occurred. This requested report is the sole resulting artifact outside the checkout.

Approximately eight read orchestration calls containing 20 shell invocations preceded report creation; each often batched several reads/history queries. Some broad outputs were truncated, then narrowed. Not every touched file or all 50 diffs were read. No token count is available or inferred.

## Dismissed candidate 1: Subscription lifecycle complexity and repeated adapter changes

**Pressure and future cost.** Dispatcher/client paths were among frequently touched source paths. Parameter-change commit `02227b17d2e63b96095ba824eaa807ceedde240c` changed the client Subscription, server coordinator and SignalR adapter together (155/211/89 changed lines in the selected stat). A future revision, reconnect or publication change could require understanding ordering across client and server lifetimes.

**Older/current ownership.** `1ef6e9efaf2bd1ab688613bbdd489745363b0b86` deepened the client lifecycle module; its `src/Photonic/Internal/Subscription.cs` was 511 lines versus 485 now. Length alone is not the argument: current source owns revision completion and retained Snapshot identity (`src/Photonic/Internal/Subscription.cs:94–132`), typed updates prevent wrong-contract updates, and the redesign adds an explicit `IsMaintainable` fence for rejected input rather than duplicating recovery policy.

Server files moved from `src/Photonic.AspNetCore/Subscriptions/` to `src/Photonic.AspNetCore/Server/Subscriptions/` in `5edf9db506f6f01dbbf555e5187153f3f2d2ca47`. Git follow found only the new Instance path, so the old file was compared explicitly: the old Instance at `02227b1` was already 486 lines versus 507 now. This is not a newly introduced 507-line responsibility. The same redesign reduced hub code by 128 deletions/16 insertions, ServerPhotonicClient by 234/129 and dispatcher by 234/120. The Instance owns the phase machine and fenced publication (`src/Photonic.AspNetCore/Server/Subscriptions/SubscriptionInstance.cs:18–25`, `:127–172`, `:183`). The hub delegates establishment (`src/Photonic.AspNetCore/Hosting/SubscriptionHub.cs:105`), and dispatcher calls instance work (`src/Photonic.AspNetCore/Server/Subscriptions/InvalidationLogDispatcher.cs:278`).

**Disconfirmation/rule status.** ADR-0029 explicitly chooses instance ownership over runtime-wide serialization. `tests/Photonic.UnitTests/Subscriptions/ServerLifecycleAcceptanceTests.cs:91`, `:119`, `:142`, `:179` exercise publication/invalidation, independent establishment and callback reentrancy. `SubscriptionModelTests.cs:18` drives generated event sequences. These are inspected test claims, not verified passes. Terminal-delivery and barrier limitations are already accepted debt (`docs/limitations.md:28–30`). The recent recovery fixes `4d3a850` and `905fff3` concern test/sample setup, not demonstrated runtime ownership spreading.

**Decision.** Dismiss as new degradation. Leave policy together; reconsider if a real lifecycle change again needs parallel policy edits in both transport adapters, or a registered debt trigger occurs. Splitting by file length could recreate the documented ordering spread. Moderate confidence: concurrency correctness was not exhaustively audited and tests were not run.

## Dismissed candidate 2: SQL bulk capture and server/provider coupling

**Pressure and future cost.** `src/Photonic.EFCore.SqlServer/Writes/BulkInvalidationDbCommandInterceptor.cs` has 853 lines of transaction/interception handling. A new EF SQL shape or second provider could raise upgrade/testing cost or tempt server policy to depend on persistence mechanics.

**History across moves.** Automatic capture originated in `c5aab2f8f3a1406e2e0d5589b5e7b0ef02c4c474`. `c84a026fbb2b7615f6b482cd91ae342bd7f0282c` moved top-level fault-support types into their own files, explaining some file-count growth. `076ecb3f38d348fee2423f1a3af039cfde7d359f` replaced a flag-heavy attempt record with opt-out, owned-transaction and caller-transaction variants. Those remain nested with their owner (`BulkInvalidationDbCommandInterceptor.cs:800`), not spread over unrelated modules.

`5edf9db506f6f01dbbf555e5187153f3f2d2ca47` moved the interceptor from `src/Photonic.AspNetCore/Subscriptions/` into the SQL package with only four insertions/three deletions on the renamed file. Capture/analyzer files likewise largely moved with namespace changes. The meaningful ownership change is the provider-backed session (`src/Photonic.EFCore.SqlServer/Log/InvalidationLog.cs:11–39`) behind `src/Photonic.AspNetCore/Server/Subscriptions/IInvalidationLogSession.cs:1`. Runtime policy no longer needs an application DbContext type.

**Disconfirmation/rule status.** ADR-0025 explicitly accepts a provider/version-specific recognizer and fail-closed unknown SQL. ADR-0030 documents the internal provider seam and namespace boundary. Executable guards exist at `tests/Photonic.UnitTests/Architecture/PackageBoundaryTests.cs:13–44` and `:48–68`. Internal extensibility, migration type-name coupling and no separate Server assembly are registered accepted debt (`docs/limitations.md:37–39`), not newly discovered rule violations.

**Decision.** Dismiss as demonstrated worsening. Leave the internal seam unchanged until a second provider/host or supported EF shape supplies actual pressure. A generalized public provider API would add compatibility/correctness obligations without observed benefit. Moderate confidence: selected transaction-history diffs and seam were inspected, not every interception branch or SQL shape.

## Dismissed candidate 3: Generator growth and metadata/factory coupling

**Pressure and future cost.** ContractCatalogGenerator and HandlerModuleGenerator are 658 and 588 lines. Future result shapes could require synchronized runtime/generated changes; multi-catalog applications can encounter factory naming collisions.

**Change evidence.** `4dcc631ac1619de941801d686bb21537eeba1173` introduced catalogs/modules to replace central registrations. `5edf9db506f6f01dbbf555e5187153f3f2d2ca47` added typed Subscription factories while replacing integer wire-kind metadata with `Photonic.ContractKind`. Current factory emission selects types/cardinality and delegates (`src/Photonic.Generators/ContractCatalogGenerator.cs:355–366`); it does not implement lifecycle or transport behavior. Mirrored constants at `src/Photonic.Generators/HandlerModuleGenerator.cs:17–19` remain a synchronization point, explicitly explained by the analyzer's inability to reference the runtime assembly. No repeated drift or repair was found in selected history.

**Disconfirmation/rule status.** ADR-0004 makes one immutable descriptor the Registered Name/transport metadata carrier; ADR-0005 leaves execution policy handwritten. ADR-0028 chooses typed factories for wrong-contract compile-time rejection. `tests/Photonic.Generators.Tests/SubscriptionFactoryGeneratorTests.cs:54`, `:87`, `:129`, `:152`, `:197` cover inference, wrong contracts, namespaces, Large Content exclusion and delegation. Nullable-result and class-name gaps are accepted debt (`docs/limitations.md:31–32`), with no inspected active nullable/multi-catalog change establishing that a trigger fired.

**Decision.** Dismiss as a new maintenance finding. Leave the generator together while classification/emission share a model. Revisit existing naming/nullable debt at its trigger; extraction becomes an option if a concrete surface needs independently evolving behavior. High confidence that added factories are bounded delegation; moderate confidence about future consumers because only repository consumers were considered.

## Limits

This is maintenance prioritization from bounded recent Git evidence and selective source inspection. It does not establish performance, concurrency safety, comprehensive architecture compliance, deployed behavior, skill effectiveness or maintainer acceptance. Existing debt and potential future costs remain real; this evidence does not show that structural rework now has greater economic value than waiting for documented triggers.

## Subsequent deterministic check

The parent agent ran only the three `PackageBoundaryTests` against the isolated pinned clone. The initial default `dotnet test` stopped at restore because NU1903 package advisory warnings were treated as errors. A second run retained advisory warnings and made NU1903 nonfatal for this check; all three tests passed:

```sh
dotnet test tests/Photonic.UnitTests/Photonic.UnitTests.csproj -c Release \
  --filter FullyQualifiedName~PackageBoundaryTests --nologo \
  -p:WarningsNotAsErrors=NU1903 \
  --logger 'trx;LogFileName=package-boundaries.trx' \
  --results-directory /tmp/photonic-architecture-results
```

The test result covers the encoded dependency rules only. It does not mean the normal build passed or that dependency advisories were resolved. No tracked Photonic source or configuration changed; SQL, browser, lifecycle, and generator suites were not run.

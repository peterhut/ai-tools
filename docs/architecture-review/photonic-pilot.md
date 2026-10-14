# Photonic pilot: access and suitability

Access checked on 2026-10-14. This is pilot scoping, not a maintainability assessment or a finding of degradation.

The subsequent [bounded assessment](photonic-assessment.md) used a fresh isolated clone pinned to live main `3f5c709640ce02b24a34c64cc525d72ee5b81db3`. It investigated three candidates and made no new maintenance recommendations. The access-check observations below describe the earlier local copies, not the assessed snapshot. See [evaluation.md](evaluation.md) for deterministic checks, the separate skill trial, and remaining validation limits.

## Accessible revisions

- Repository: `https://github.com/peterhut/photonic`.
- GitHub access verified with `git ls-remote origin HEAD refs/heads/main`: live main is `3f5c709640ce02b24a34c64cc525d72ee5b81db3`.
- Base checkout: `/home/peter/repos/photonic`, clean at `a272429433642b88d46682964171f331c0fbf5fe`.
- Additional worktree: `/home/peter/.t3/worktrees/photonic/t3code-978fccd9`, clean at `83beb46c5720043c10e42616fd90c8778e63455f`, branch `t3code/unit-test-skills`.
- Cached `origin/main` is `905fff3b599cda886133f0a0d3448d11918f502f`, behind live main. The live main object is not available locally. A future assessment should obtain a pinned isolated snapshot rather than analyze an old checkout as if it were current main or switch the user's active branch.

## Why it is a useful candidate

The newer local worktree documents separate packages for shared contracts, generators, ASP.NET Core server policy, SQL Server persistence, Blazor clients, and architectural testing. `CONTEXT.md`, `docs/RCDA-summary.md`, and ADRs establish terminology, architectural intent, and accepted trade-offs. Package-boundary tests provide an opportunity to reuse deterministic checks rather than rediscover their rules through LLM judgment.

The available branch has 74 first-parent commits, enough to exercise a bounded history sample, without implying statistical sufficiency. Recent history includes a broad library redesign and test reorganization. Those changes must be interpreted before treating file movement or cross-package edits as coupling or deterioration.

No Photonic files, branches, issues, or PRs were changed by this access check. No build or test suite was run.

## Candidate assessment questions

These are investigation targets from the available branch, not confirmed problems. Revalidate them against the selected pilot revision.

- Subscription lifecycle ownership: do parameter updates, replacement, and recovery remain localized in the Subscription and server Subscription Instance, with adapters translating events and effects? See commits `02227b1` and `5edf9db`, ADRs 0028, 0029, and 0032, and `src/Photonic/Internal/Subscription.cs` plus `src/Photonic.AspNetCore/Server/Subscriptions/`.
- Server policy versus persistence: does the SQL Server adapter separation localize decisions without requiring repeated cross-package coordination? See redesign commit `5edf9db`, ADR-0030, and the `Photonic.AspNetCore` and `Photonic.EFCore.SqlServer` packages. Deliberate friend access and version pins are part of the documented trade-off.
- Invalidation optimization ownership: do bounded draining and snapshot reuse preserve localized dispatch coordination and Instance-owned lifecycle? See commits `e320404` and `c36c25a`, ADR-0027, `InvalidationLogDispatcher.cs`, `SubscriptionStore.cs`, and the subscription-store benchmarks. Documented deferred optimizations are not automatically defects.

Existing checks to consult: `tests/Photonic.UnitTests/Architecture/PackageBoundaryTests.cs` uses NetArchTest and assembly-reference assertions; `Directory.Build.props` configures API analyzers and package validation; `eng/verify-packages.ps1` checks package dependencies and consumers. Their availability was inspected, but they were not executed.

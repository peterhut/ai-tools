# Evidence-led discovery

Discover only enough architecture to answer the framed question. Keep inventories in scratch space and regenerate them from the repository.

## Evidence roles

Use these sources in this order of authority for different kinds of claim:

1. **Source and configuration**: current implementation and runtime reality.
2. **`CONTEXT-MAP.md` / `CONTEXT.md`**: canonical domain terms and meaning that code cannot reveal reliably.
3. **Accepted ADRs**: architectural intent and trade-offs. Respect supersession metadata.
4. **Tests, telemetry, and generated metadata**: corroborating behavior.
5. **Inference**: allowed when evidence is incomplete, but label it and state the basis.

Code can disagree with an ADR because implementation drifted or a migration is incomplete. Report both as `observed` and `intended`; do not choose one silently.

## Broad-to-narrow scan

1. Locate repository instructions, context documents, ADRs, solution/workspace files, and recent relevant changes when scope is otherwise unclear.
2. Inspect manifests and dependency edges (`*.sln*`, project/package manifests, lock files).
3. Inspect executable entry points, composition roots, DI registration, route/message registration, and generated contract catalogs.
4. Inspect runtime and infrastructure definitions (AppHost, compose, Kubernetes, IaC, environment/configuration bindings).
5. Search for external clients, connection strings, queues, data stores, authentication providers, protocols, and ownership boundaries.
6. Descend into the feature/module named by the question. Read direct callers and callees until every claimed edge has evidence.

Prefer `rg --files` and targeted `rg` searches. Do not read every source file merely to claim completeness.

## Perspective-specific signals

### Logical

- Domain glossary, aggregates/value objects, commands/queries, public contracts.
- Module responsibilities, DI registrations, interfaces with current adapters.
- Data ownership and invariant enforcement.

Use domain names in the view; retain code identifiers as source links or secondary labels when they help navigation.

### Process

- HTTP/UI/message entry point and authenticated actor.
- Dispatch/call chain, external calls, reads/writes, transaction boundaries.
- Async/concurrent behavior, waits, invalidation/events, retries and failure paths.
- End-to-end/browser tests and traces when available.

For each step record initiator, receiver, action, mechanism, and evidence. Arrow direction follows the call or data flow. Do not infer temporal order from a static dependency alone.

### Development

- Project/package references and build graph.
- Source namespaces/folders and feature slices.
- Generated code boundaries and shared contracts.
- Unit, integration, and end-to-end test ownership.

Do not treat every project as a deployable container. A source project can be a development boundary within one runtime.

### Physical

- Deployable executables and stores mapped to deployment nodes/environments.
- Startup ordering, health dependencies, ports/protocols, secrets/config injection.
- Local emulators/containers versus published cloud resources.
- Scaling, replicas, jobs, volumes, regions, and network boundaries when configured.

Separate the logical store/application from its local and deployed instances. Do not promote an infrastructure node into a logical container.

## Evidence map

Keep a scratch table while exploring:

| Claim | Kind | Evidence | Confidence |
| --- | --- | --- | --- |
| concise element or relationship | observed / documented / inferred / conflict | file and symbol/line, ADR, config, trace | high / medium / low |

Before delivery, remove any unsupported visual edge, or label it as an inference. Cite the smallest set of load-bearing files that lets another agent reproduce the view.

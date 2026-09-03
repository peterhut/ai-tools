# Architecture exploration dimensions

The dimensions are orthogonal coordinates, not a menu of diagram types.

## C4 level: zoom

| Level | Focus | Typical evidence | Exclude by default |
| --- | --- | --- | --- |
| Landscape / Context | People, the system(s) in scope, and externally owned systems | product context, authentication/integration config, public entry points | internal modules and infrastructure detail |
| Container | Separately runnable/deployable applications, workers, stores, brokers, and client applications inside one system | AppHost/compose/Kubernetes, executables, database and broker configuration | internal classes and physical replicas |
| Component | Responsibility-bearing modules inside one container | feature/module layout, handlers, DI registration, controllers/endpoints, repositories and clients | every class or helper |
| Code / module | Load-bearing code elements inside one component | public contracts, domain types, direct calls, tests | exhaustive class/member inventories |

"Container" means a runtime boundary, not necessarily a Docker container. Landscape is a portfolio-wide context view; Context centers one system. Code/module detail decays fastest, so scope it tightly and point to source.

A scoped view may include higher-level actors, sibling containers, stores, or external systems when they are necessary to explain relationships that cross the scope boundary. Label them as context and show internal detail only for the element being zoomed into.

## 4+1 perspective: question

Each diagram selects exactly one perspective. Perspectives are not blended inside a single diagram; if a request needs more than one, use multiple coordinated diagrams in the same artifact, each with its own metadata.

| Perspective | Question answered | Useful relationships |
| --- | --- | --- |
| Logical | What concepts/elements exist, what responsibilities do they have, and how are they related? | owns, contains, depends on, implements, persists |
| Process | What runs or communicates, in what order, and where do concurrency, state changes, or failures occur? | calls, returns, publishes, waits for, retries, commits |
| Development | How is the system organized for implementation, build, testing, and ownership? | project/package reference, source module, generated contract, test coverage |
| Physical | Where and how does it execute across environments? | deploys to, instance of, connects over, starts after, scales on |

The C4 level controls abstraction; the perspective controls which facts matter. For example, a Component/Logical view shows responsibilities and dependencies, while Component/Process shows collaborations over time.

## Scenario: scope

A scenario selects or highlights a slice at any level within the chosen perspective; this function is the same in either projection. A broad current-state view may omit it, while change context requires one. It is never a fifth C4 level or an additional perspective.

- Name it as a scope slice or highlight: `transaction workspace`, `import path`, `application startup`.
- Define what is included or highlighted in the chosen perspective; do not use the scenario to add a second perspective.
- If the question asks for a trigger, observable end state, ordered path, or material branches, select the Process perspective explicitly and trace only the branches that explain the architecture.
- Within the selected perspective, choose the representation that best answers the question: a sequence/dynamic/activity diagram for Process order, or a filtered static view for a structural Logical question.
- If a high-level scenario hides an important sub-flow, link to or produce a second, narrower view instead of overcrowding one diagram.

If a request clearly asks for multiple perspectives, keep the diagrams coordinated but separate. Do not use a scenario to smuggle Process, Development, or Physical relationships into a Logical diagram.

## Defaults when the prompt omits coordinates

- "Architecture overview" → Context or Container + Logical, no scenario.
- "How does X work?" → Component + Process + scenario X.
- "Where does this run?" → Container + Physical.
- "How is the codebase organized?" → Container or Component + Development.
- "What classes implement X?" → Code/module + Logical, scoped to X.

State inferred coordinates so the user can redirect the exploration without treating the choice as architectural fact.

---
name: architecture-views
description: Explore and visualize a codebase's current architecture or the context of a branch, pull request, or working-tree change as an ephemeral, evidence-backed view. Use for architecture overviews, C4 zoom levels, 4+1 perspectives, runtime scenarios, source organization, deployment topology, or questions such as "how does this work?", "what changed?", "where is this change?", or "show the blast radius". Use improve-codebase-architecture instead when the goal is to find refactoring opportunities.
---

# Architecture Views

Answer one architecture question from the repository or one comparison range. Treat every visualization as a disposable projection of evidence, not as a second architecture model.

## Choose the projection

- **Current state** answers what exists or how it works now. This remains the default.
- **Change context** locates one change scenario in the system between a base and head. Use it for a branch, pull request, commit range, or working-tree change, and read [references/change-context.md](references/change-context.md) before exploring the diff.

Change context is a mode of this skill, not a durable architecture history. Generate it from the requested comparison every time.

## Frame the question

Choose three coordinates for each view:

- **C4 level**: Landscape/Context, Container, Component, or Code/module.
- **4+1 perspective**: exactly one of Logical, Process, Development, or Physical.
- **Scenario**: an optional scope or highlight within that perspective, never another level or perspective.

Infer omitted coordinates from the question and audience. Ask only when different choices would materially change the answer. If the question needs multiple perspectives, create coordinated, perspective-pure views with shared element identities. Read [references/dimensions.md](references/dimensions.md) when choosing or explaining coordinates.

## Explore

1. Read applicable `AGENTS.md` files, then `CONTEXT-MAP.md`/`CONTEXT.md` and relevant ADRs when present. Domain docs supply terminology and intent; they do not override contradictory current code.
2. Follow high-signal evidence from broad to narrow: manifests and project references, runtime/deployment configuration, entry points and composition roots, then only the modules needed for the question. For Process, trace the selected scenario end to end. For change context, let the scenario select the primary change rather than treating every changed file as diagram content.
3. Maintain a scratch evidence map. Every element and relationship must be observed, documented, inferred, or in conflict, with confidence and source evidence. Surface contradictions instead of reconciling them silently.

Read [references/discovery.md](references/discovery.md) before a broad scan, scenario trace, or claim about Development/Physical structure.

## Represent

Use prose alone when it answers the question clearly. When a visualization materially improves the answer, use the HTML evidence explorer in [assets/architecture-explorer.html](assets/architecture-explorer.html) as the sole built-in visual output. Use another format only when the user explicitly requests it; repository use of LikeC4, Structurizr, PlantUML, draw.io, or another tool is not an implicit switch.

The explorer uses:

- Cytoscape.js with ELK semantic layout for Logical, Development, and Physical graph views.
- Mermaid sequence diagrams inside the same HTML shell for Process scenarios where order matters.
- One renderer-neutral element catalog with stable IDs across tabs; view definitions contain only membership, grouping, emphasis, and layout direction.
- An optional comparison overlay where added, modified, removed, and unchanged context take visual priority over evidence state.

Keep each view perspective-pure. Use multiple tabs only when coordinated views materially answer the named question. A soft threshold of roughly 25–35 visible nodes is a signal to split, group, or raise the level, not a hard cap.

Read [references/html-explorer.md](references/html-explorer.md) before producing a visual artifact. It defines the data contract, notation, generation workflow, and verification gate.

## Keep it ephemeral

- Build the JSON and generated HTML in a fresh OS temporary directory unless the user explicitly requests a repository artifact.
- Generate from current evidence on every run. Do not add an architecture inventory, model, cache, or diagram folder to the repository.
- The HTML is a viewer: pan, zoom, search, filter, inspect, collapse, session-only drag, and browser-side PNG export are allowed; model editing is outside its scope.
- Open the generated file in a browser and inspect every view. The built-in VS Code browser is sufficient; no specialized extension or Node runtime is required.
- If pinned browser dependencies cannot load, help the user enable access. If they decline or access remains unavailable, return the evidence-backed prose answer and disclose that the visual was not rendered.

## Deliver

State the selected level, perspective, and scenario; for change context, also state the comparison range. Answer the architecture question; link the temporary HTML when one was useful; cite the load-bearing source files and ADRs; and distinguish observed reality, documented intent, inference, and conflict. The viewer's Export control can produce a disposable PNG of the active view for a pull request description, issue, document, or other sharing. Suggest one useful adjacent perspective after answering, without silently generating it.

The exploration is complete when every visible node and edge is supported, each view answers one named question without unrelated detail, and the delivered artifact has passed the verification gate.

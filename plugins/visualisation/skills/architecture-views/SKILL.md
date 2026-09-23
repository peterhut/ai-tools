---
name: architecture-views
description: Explore and visualize a codebase's current architecture or the context of a branch, pull request, or working-tree change as an ephemeral, evidence-backed view. Use for architecture overviews, C4 zoom levels, 4+1 perspectives, runtime scenarios, source organization, deployment topology, or questions such as "how does this work?", "what changed?", "where is this change?", or "show the blast radius".
---

# Architecture Views

Answer one architecture question from the repository or one comparison range. Treat every visualization as a disposable projection of evidence, not as a second architecture model.

## Choose the projection

- **Current state** answers what exists or how it works now. This remains the default.
- **Change context** locates one change scenario in the system between a base and head. Use it for a branch, pull request, commit range, or working-tree change, and read [references/change-context.md](references/change-context.md) before exploring the diff.

## Frame the question

Choose three coordinates for each view:

- **C4 level**: Landscape/Context, Container, Component, or Code/module.
- **4+1 perspective**: exactly one of Logical, Process, Development, or Physical.
- **Scenario**: a scope or highlight within that perspective, derived from the question and available evidence; never another level or perspective.

Infer omitted coordinates from the question and audience. Ask only when different choices would materially change the answer. If the question needs multiple perspectives, create coordinated, perspective-pure views with shared element identities. Read [references/dimensions.md](references/dimensions.md) when choosing or explaining coordinates.

## Explore

1. Read applicable `AGENTS.md` files, then `CONTEXT-MAP.md`/`CONTEXT.md` and relevant ADRs when present. Domain docs supply terminology and intent; they do not override contradictory current code.
2. Follow high-signal evidence from broad to narrow: manifests and project references, runtime/deployment configuration, entry points and composition roots, then only the modules needed for the question. When selected, use the scenario to bound the relevant slice rather than treating all available evidence as diagram content. For Process, trace the selected scenario end to end.
3. Maintain a scratch evidence map. Every element and relationship must be observed, documented, inferred, or in conflict, with confidence and source evidence. Surface contradictions instead of reconciling them silently.

Read [references/discovery.md](references/discovery.md) before a broad scan, scenario trace, or claim about Development/Physical structure.

## Represent

Use prose alone when it answers the question clearly. When a visualization materially improves the answer, use the HTML evidence explorer in [assets/architecture-explorer.html](assets/architecture-explorer.html) as the sole built-in visual output. Use another format only when the user explicitly requests it; repository use of LikeC4, Structurizr, PlantUML, draw.io, or another tool is not an implicit switch.

The explorer uses:

- Cytoscape.js with ELK semantic layout for Logical, Development, and Physical graph views.
- Mermaid sequence diagrams inside the same HTML shell for Process scenarios where order matters.
- One renderer-neutral element catalog with stable IDs across tabs; view definitions contain membership, grouping, emphasis, layout preferences, and optional overview/detail navigation.
- Optional view families with one primary overview and narrowly scoped detail views linked by shared catalog identities.
- An optional comparison overlay where added, modified, removed, and unchanged context take visual priority over evidence state.

Keep each view perspective-pure. Use multiple tabs only when coordinated views materially answer the named question. A soft threshold of roughly 25–35 visible nodes is a signal to split, group, or raise the level, not a hard cap.

When a candidate remains unreadable after bounded layout retries, prefer a view family over an overloaded graph: raise the overview one C4 level when that preserves the question, then add no more than two focused detail views. Same-level splits are valid when promotion would hide the answer. Keep the 4+1 perspective consistent unless the question genuinely requires a Process view.

Read [references/html-explorer.md](references/html-explorer.md) before producing a visual artifact. It defines the data contract, notation, generation workflow, and verification gate.

Run the explorer's data preflight before opening a preview. It rejects duplicate identities, missing graph endpoints, compound-parent cycles, invalid drilldowns, renderer-specific omissions, and multiple hero relationships with repair-oriented diagnostics. Use optional `displayName` and `displayLabel` fields for concise graph/sequence text while retaining full names, labels, and evidence in the details panel and search. Sequence input is normalized to Mermaid-safe participant aliases and message text; if Mermaid still rejects it, keep the original source, report the parse line, and deliver the ordered readable fallback rather than hiding the failure behind an error card.

For agent-owned presentation quality, use a bounded candidate loop rather than silently accepting the first valid graph:

1. Run the advisory complexity preflight and generate the documented default layout.
2. Render the complete view family at the intended sharing viewport and export representative PNGs.
3. Inspect diagnostics and the exact PNGs for labels, boundaries, routing, and initial fit.
4. Try at most five layout candidates per view, within a shared ten-minute budget for the entire family (including decomposition and all detail retries). Start with the authored default, then try right/spacious/orthogonal, down/spacious/orthogonal, right/spacious/straight, and down/spacious/straight, skipping duplicates. Preserve evidence and stop early when visual inspection passes. A verifier invocation has a 120-second ceiling; do not start another if the family budget is exhausted.
5. If all candidates remain unreadable, generate the bounded overview/detail view family described above. Preserve the best candidate and report whether the result is verified, has readability concerns, or could not be verified.

The verifier collects evidence for these decisions; it does not choose candidates or edit the architecture model autonomously. Treat exact post-layout geometry errors as candidate failures and follow the attached repair suggestion before retrying. Cytoscape taxi routing exposes endpoints and labels but not its internal bend points, so taxi segment obstruction, corridor, and route-rhythm checks remain visual-inspection responsibilities.

Keep a temporary candidate log with view id, profile, elapsed time, exact PNG/report paths, inspection findings, and selection/rejection reasons. Preserve the best candidate and record its selection reason. Inspect exports at 960 CSS pixels wide by default (override for the intended destination), targeting at least 12 CSS pixels for essential text and no obscured labels. The verifier's `visual inspection pending` result is not approval: only the inspecting agent may conclude `verified`. On budget exhaustion, report `readability concerns` for rendered but unsuitable output or `verification unavailable` when rendering could not be established.

## Keep it ephemeral

- Build the JSON and generated HTML in a fresh OS temporary directory unless the user explicitly requests a repository artifact.
- Generate from current evidence on every run. Do not add an architecture inventory, model, cache, or diagram folder to the repository.
- The HTML is a viewer: pan, zoom, search, filter, inspect, collapse, session-only drag, ephemeral layout presets, overview/detail navigation, and browser-side PNG export are allowed; model editing is outside its scope. Node pinning is deferred until constrained layout is proven safe.
- Open the generated file in a browser and inspect every view. The built-in VS Code browser is sufficient; no specialized extension or Node runtime is required.
- If pinned browser dependencies cannot load, help the user enable access. If they decline or access remains unavailable, return the evidence-backed prose answer and disclose that the visual was not rendered.
- When a browser surface is unavailable, an optional headless verifier may render the local file at a fixed target viewport, exercise the explorer, export each renderer, and return diagnostics. The agent owns candidate selection and retries; the viewer does not run an autonomous optimizer.

## Deliver

State the selected level and perspective, plus the scenario when one scopes the view; for change context, also state the comparison range. Answer the architecture question; link the temporary HTML when one was useful; cite the load-bearing source files and ADRs; and distinguish observed reality, documented intent, inference, and conflict. The viewer's Export control can produce a disposable PNG of the active view for a pull request description, issue, document, or other sharing. Suggest one useful adjacent perspective after answering, without silently generating it.

The exploration is complete when every visible node and edge is supported, each view answers one named question without unrelated detail, and the delivered artifact has passed the verification gate.

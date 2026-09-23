# HTML evidence explorer

Use this reference only when architecture exploration benefits from a visual. The generated HTML is disposable; the repository remains the source of truth.

## Renderer-neutral data

Copy `assets/architecture-explorer.html` into a fresh temporary directory and replace its single `__ARCHITECTURE_DATA_BASE64__` placeholder with UTF-8 JSON encoded as Base64.

The root object has this shape:

```json
{
  "meta": {
    "title": "Architecture question",
    "question": "The one question these views answer",
    "repository": "display name",
    "repositoryRoot": "absolute/path/for/source/links",
    "generatedAt": "ISO-8601 timestamp",
    "artifactId": "optional stable content identity",
    "primaryView": "optional-default-view-id"
  },
  "comparison": {
    "base": "base label",
    "head": "head label",
    "range": "optional exact range"
  },
  "catalog": [],
  "relationships": [],
  "views": []
}
```

Omit `comparison` for a current-state exploration. Its presence activates change-context rendering and requires non-empty `base` and `head` strings. Read [change-context.md](change-context.md) before authoring this mode.

### Catalog elements

Every element requires:

- `id`: stable across all views; namespace IDs such as `person:user`, `container:web`, or `module:billing`.
- `name` and `kind`: full evidence label and explicit architectural type.
- `displayName`: optional short label for graph/participant rendering. Keep `name` unchanged so the details panel and search retain the full source terminology.
- `evidenceState`: `observed`, `documented`, `inferred`, or `conflict`.
- `confidence`: `high`, `medium`, or `low`.
- `evidence`: zero or more `{ "path": "repo/relative/path", "line": 1, "note": "why this supports the claim" }` objects.

For change context, every element visible in a graph also requires `changeState`: `added`, `modified`, `removed`, or `unchanged`. `unchanged` means contextual rather than irrelevant and renders as **Context**. Omit `changeState` entirely for current-state artifacts.

Use `technology`, `scope`, `description`, and `responsibilities` when they add information. Put long descriptions, caveats, and evidence in the details panel, not the graph label. Regular graph nodes are rendered as structured cards: architectural kind and evidence state are compact corner badges, `displayName` (or `name` when omitted) is the primary text, and `technology` is a smaller italic secondary line. Keep display labels and technology concise enough to survive the renderer's two-line truncation; the details panel remains the complete view.

### Relationships

Every relationship requires `id`, `source`, `target`, a specific directional `label`, `evidenceState`, `confidence`, and evidence. Add `displayLabel` when a shorter graph/sequence label improves readability; the full `label` remains visible in relationship details and search. Add `technology` for a protocol or mechanism and `explanation` for an inference or conflict. Both endpoints must exist in the catalog.

For change context, relationships visible in graph views require the same `changeState` values as catalog elements. Add `emphasis: "hero"` to the one relationship that best expresses the scenario; use `"muted"` only when a necessary relationship should recede. Omit `emphasis` for the normal treatment.

Do not infer temporal order from a static dependency. Process order belongs in a Process view supported by runtime code, tests, traces, or an explicitly labelled inference.

### Views

Every view requires `id`, `label`, `level`, exactly one `perspective`, `scenario`, and `renderer`. Use an empty `scenario` for a broad current-state view with no selected slice; change context requires a non-empty scenario.

Views may also include a `backView` id and `drilldowns` such as `{ "element": "container:web", "targetView": "web-components", "label": "Open web details" }`. These create a bounded overview/detail family without duplicating the catalog. `meta.primaryView` selects the initial overview; otherwise the first view is used.

For a graph view:

```json
{
  "renderer": "graph",
  "layout": { "direction": "RIGHT", "density": "BALANCED", "routing": "ORTHOGONAL" },
  "direction": "RIGHT",
  "nodes": [{ "id": "container:web", "parent": "system:product" }],
  "edges": ["relationship-id"],
  "drilldowns": [{ "element": "container:web", "targetView": "web-components", "label": "Open web details" }]
}
```

`direction` is `RIGHT` or `DOWN`. `layout.direction` takes precedence when both are present. `layout.density` is `COMPACT`, `BALANCED`, or `SPACIOUS`; `layout.routing` is `ORTHOGONAL` or `STRAIGHT`. Parent references create ELK compound boundaries. Nodes and edges reference the shared catalog; do not redefine elements inside a view.

For a Process sequence:

```json
{
  "renderer": "sequence",
  "participants": ["person:user", "container:web"],
  "mermaid": "sequenceDiagram\n  actor user\n  participant web\n  user->>web: Sends request"
}
```

Prefer Mermaid sequence syntax for ordered collaborations. The explorer normalizes participant aliases and message text before rendering, converts self-messages to readable Notes when necessary, and preserves the original Mermaid source in fallback diagnostics. Use a graph view only when topology or branching matters more than time.

## Preflight and regeneration

The explorer validates the embedded data before attempting to load Cytoscape, ELK, or Mermaid. It rejects duplicate ids, missing endpoints, invalid renderer fields, compound-parent cycles, invalid drilldowns, and renderer-specific required fields with repair-oriented diagnostics. Generate graph views only after this preflight passes; geometry checks then run after layout and report exact overlaps, label collisions, endpoint problems, and viewport overflow with suggestions such as shortening `displayName`/`displayLabel` values or splitting a view.

The runtime exposes a content-derived artifact fingerprint in `window.__architectureExplorer.diagnostics()`. Generators may set `meta.artifactId` for a human-readable identity; otherwise the fingerprint is used. When a preview host is recovered, reopen or reload the current HTML and compare the reported identity before investigating renderer errors. A stale identity is reported separately from schema, dependency, and Mermaid failures, so manual timestamp cache-busting is not required to establish which payload loaded.

## Evidence and notation

Evidence state and architectural type are separate dimensions:

- **Observed**: source/configuration currently implements the claim.
- **Documented**: domain documentation or an accepted ADR states the claim.
- **Inferred**: evidence suggests the claim but does not establish it.
- **Conflict**: credible evidence supports incompatible readings.

Change state is a third, orthogonal dimension used only when `comparison` exists. In that mode it owns colour and visual weight: added, modified, removed, and unchanged context. Evidence remains in the details panel; only inferred and conflict states receive small graph-level marks. Do not use evidence state as a proxy for change state.

Use a minimal C4-inspired vocabulary:

- rounded rectangles for software systems, containers, components, services, and modules;
- a person pictogram for people, a cylinder/database pictogram for stores, and a queue pictogram for brokers/topics;
- labelled nested enclosures for groups and deployment nodes;
- an explicit `External` scope treatment rather than a separate external shape;
- directed relationships with action labels and protocol/mechanism where relevant;
- vendor/cloud/device icons only when explicitly requested or materially useful.

Type labels and the generated legend are authoritative. Shape, colour, border, and evidence marks are redundant cues; no meaning may depend on colour alone. UML-style symbols are informal visual shorthand, not formal UML semantics.

## Generate without Node

On PowerShell, the complete substitution is:

```powershell
$encodedArchitecture = [Convert]::ToBase64String([IO.File]::ReadAllBytes($architectureJsonPath))
$explorerHtml = [IO.File]::ReadAllText($explorerTemplatePath).Replace(
    '__ARCHITECTURE_DATA_BASE64__',
    $encodedArchitecture)
[IO.File]::WriteAllText($explorerOutputPath, $explorerHtml, [Text.UTF8Encoding]::new($false))
```

Use equivalent environment-native file and Base64 operations on other platforms. This is data substitution, not a build pipeline. The template contains pinned CDN references for Cytoscape.js, ELK.js, the Cytoscape ELK adapter, and Mermaid; a browser executes them directly.

Do not put secrets, environment values, source excerpts, or other sensitive material in the HTML. It is a plain file whose Base64 payload is encoding, not encryption.

## Interaction contract

The viewer provides pan/zoom/fit, ELK re-layout, direction/density/routing presets, search/focus, evidence filtering, tabbed views, element/relationship details, collapse/expand, theme selection, session-only dragging, overview/detail navigation, and high-resolution PNG export of the active view. In sequence views, search highlights matching participant chips instead of graph nodes, and Enter opens the first match's details. Layout changes are session-only and scoped per view; theme changes and tab switches preserve them. `Re-layout` applies the current preferences. `Reset view` clears search, evidence filtering, collapse state, and the current view's layout overrides, restoring its documented defaults. It does not add, delete, or edit architecture claims. Node pinning is intentionally not part of this contract until constrained layout is proven safe.

Export downloads only the active graph or sequence, without explorer controls or the details panel. Each export receives a run-scoped unique filename to avoid collisions with inspected or locked PNGs. Treat the image as disposable and keep it outside the repository unless the user explicitly requests a repository artifact. It may be used in a pull request description, issue, document, or other sharing; this skill does not prescribe a publication workflow.

Source evidence renders as a visible repository-relative `path:line` plus an absolute `vscode://file/...:line:column` link. The visible path is the fallback when the browser blocks the VS Code protocol.

## Theming contract

- Colours, shadows, and grid lines are defined exactly once each in `:root` (light) and `:root[data-theme="dark"]`. An inline head script sets `data-theme` on `<html>` before first paint, and the Theme control plus the `prefers-color-scheme` change listener keep it updated. There is no CSS `@media` mirror to keep in sync.
- `color-scheme` follows `data-theme`, so native controls (inputs, select, scrollbars) match the chosen theme rather than the OS default.
- Renderers source every colour and font from `palette()`, which reads the CSS custom properties at render time. Never hard-code colours or font stacks in Cytoscape styles, generated SVG node cards, or Mermaid configuration.
- Change-context colours are defined in the same light and dark palette blocks as evidence colours. In comparison mode, change colours override ordinary evidence colouring while inferred and conflict glyphs remain visible.
- Mermaid sequence diagrams use `theme: 'base'` with `themeVariables` derived from `palette()`; stock Mermaid themes (`neutral`, `dark`) clash with the shell.
- Re-render the active view after every theme change so `palette()` is re-read; the re-render preserves search text, collapsed boundaries, and selection. New visual states ship light and dark values together in the `:root` blocks, and no meaning may depend on colour alone.

## Verification gate

Before delivery:

1. Confirm browser-side data validation passes and the page has no console errors.
2. For every graph view, inspect ELK layout, nested boundaries, cross-boundary routing, labels, and initial fit. Pan and zoom when compound structure makes the fit view dense; split the view if it still cannot answer its question.
   For change context, confirm the comparison range is visible, every graph element has a change state, the scenario remains the obvious focus, unchanged context recedes, removed elements remain legible, and the hero relationship is not competing with another edge.
3. Select representative nodes and relationships. Confirm descriptions, evidence states, confidence, and source links are correct.
4. Exercise search, evidence filtering, collapse/expand, fit, layout presets, re-layout, reset, tab changes, overview/detail navigation, and session-only drag.
5. For every Process view, confirm Mermaid renders and participant detail buttons preserve shared catalog identity.
6. Check light and dark themes. Confirm inferred/conflict states remain distinguishable without colour, and that Cytoscape nodes, generated SVG node cards, and Mermaid sequence diagrams all follow the shell palette in both themes.
7. Export a representative view for each renderer present. Open each PNG and confirm the complete active view is present at a useful resolution without explorer chrome. When the user requested a PNG, inspect the exact exported file.
8. Open the final output as a local file, preferably in VS Code's built-in browser. No local server should be required.
9. Confirm the dependency-failure state explains the missing renderer and the prose fallback rather than silently switching formats. Append `?fail-deps=1` during testing to simulate this state. When regenerating an artifact after preview-host recovery, compare `window.__architectureExplorer.diagnostics().artifact` so stale content, schema failure, Mermaid failure, and unavailable dependencies remain distinct.

For agent-owned quality evaluation, use the optional headless verifier when a browser surface is unavailable. It should render the complete view family at a fixed target viewport, exercise controls, export each renderer, collect diagnostics, and preserve the best candidate within a bounded attempt/time budget. Complexity diagnostics are advisory; the agent must inspect the exact exported PNG before calling a candidate verified.

The graph diagnostics check exact rendered leaf-node overlap and clearance, relationship-label collisions and clearance, label-geometry availability, fitted viewport containment, valid edge endpoints, configured taxi direction, and straight-edge obstruction by nodes or other relationship labels. An exact geometry error fails automated verification; node clearance below 8 pixels and label clearance below 4 pixels remain warnings. For orthogonal views, Cytoscape's taxi renderer does not expose its internal bend points. Diagnostics therefore report `taxi-endpoints-and-labels-only`, and taxi segment obstruction, shared corridors, border runs, and route rhythm must remain pending visual inspection rather than being inferred from ELK's discarded route geometry.

If any visible claim lacks evidence, remove it or label the inference before delivery.

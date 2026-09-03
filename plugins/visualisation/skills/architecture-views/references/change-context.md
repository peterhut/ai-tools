# Change context

Use a change-context projection to show where one change scenario sits in the system. The diff is evidence to search, not a checklist of elements to display.

## Establish the comparison

Record one artifact-level `comparison` with concise `base` and `head` labels and, when useful, the exact range.

- For a branch or pull request, compare its merge base with the named base branch to `HEAD`. Do not compare against the current tip of the base branch.
- For an explicitly uncommitted change, compare `HEAD` with the working tree, including staged, unstaged, and relevant untracked files.
- When the request is ambiguous, infer the range that best matches it and disclose the choice.

Use ordinary repository tools such as `git diff --find-renames`, `git status`, and targeted source reads. Do not add a change-analysis CLI, generated map, cache, correction file, or repository diagram folder.

## Let the scenario select the view

The scenario names the primary change. Include the smallest connected architectural slice that carries its intent, plus the minimum unchanged context needed to locate it in the system.

Use this vocabulary while selecting that slice:

- **Primary change**: the elements carrying the intended behavior, responsibility, or contract change.
- **Touched elements**: elements whose backing implementation changed; this is observed from the comparison.
- **Affected context**: unchanged dependencies or dependents needed to understand where the primary change sits and where effects could plausibly propagate.
- **Supporting changes**: tests, migrations, configuration, generated code, or documentation that enable, verify, deploy, or describe the primary change.
- **Incidental churn**: edits that do not help explain the scenario.
- **Change fan-out**: how widely the primary change spreads across meaningful elements or boundaries; this is descriptive, not a risk or smell finding.

Classification follows intent, not file type. Tests and schema changes are often supporting, but become primary when they carry the purpose of the change.

Show changed elements and relationships that carry the scenario. Add only the few unchanged anchors that answer "where is this?", such as an entry point, owning boundary, store, broker, or external system. One dependency hop is a cap, not a quota. Omit supporting changes and incidental churn unless they materially change how the scenario is built, deployed, persisted, or verified. Do not optimize for diff coverage.

If the comparison contains multiple unrelated changes, use the dominant coherent scenario for the default view. Add another view only when a second scenario is independently important.

## Choose the perspective

- Use **Logical** when responsibilities or relationships are the useful story.
- Use **Development** when the useful answer is where an implementation-only change belongs. Tests may appear when they explain coverage or ownership.
- Use **Physical** when deployment, configuration, startup, or runtime placement is the change.
- Use **Process** only when order, concurrency, or asynchronous behavior is essential. The graph overlay is the first-class change view; a Process sequence may focus on a new, changed, or retired path without per-message delta styling.

Keep each view perspective-pure. Comparison is artifact context, not another C4 level or 4+1 perspective.

## Represent the overlay

Presence of the root `comparison` object activates transition rendering:

```json
{
  "comparison": {
    "base": "origin/main merge base (a1b2c3d)",
    "head": "HEAD (d4e5f6a)",
    "range": "a1b2c3d...d4e5f6a"
  }
}
```

Every catalog element and relationship visible in a graph view then requires `changeState`:

- `added`: introduced by the comparison.
- `modified`: the element's backing implementation changed, or the relationship's meaning or mechanism changed.
- `removed`: retired by the comparison.
- `unchanged`: context that locates the scenario; render it as **Context**.

Change state owns colour and visual emphasis. Evidence remains available in details; only inferred (`?`) and conflict (`!`) need graph-level marks. A visible unchanged neighbor is plausible structural context, not proof of behavioral impact, elevated risk, or required retesting.

Set `emphasis: "hero"` on exactly one relationship when the graph has relationships. If one relationship would materially misrepresent the change, narrow or split the scenario. Use `emphasis: "muted"` sparingly for a relationship that must remain visible but should recede.

There is no `primary` field or badge. The scenario, selection, layout, change colour, and hero relationship establish focus. If they do not, narrow the view.

## Keep the handoff light

The title and comparison range are enough around a clear diagram. Add prose, fan-out counts, or supporting-change summaries only when they prevent a material misreading.

The explorer's Export control downloads a high-resolution PNG of the active view without the explorer controls or details panel. It is suitable for a pull request description, issue, document, or other sharing. The skill does not prescribe an attachment or publication workflow, and exported artifacts remain disposable and outside the repository unless the user explicitly requests otherwise.

The change-context exploration is complete when the scenario is visually clear, every visible graph element has a valid change state, the unchanged context is no larger than necessary, the comparison is visible, and any requested PNG has been inspected.

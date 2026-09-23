# AI Tools

Small tools and Codex plugins for working with coding agents.

This repository currently contains:

- `AgentAwake`, a Windows utility that keeps the computer awake while Codex or OpenCode is active.
- A repo-local plugin marketplace for both Codex and the GitHub Copilot CLI.
- The `visualisation` plugin, whose first skill is `architecture-views`.

## Repository layout

```text
.
├── .agents/plugins/marketplace.json       # Repo-local Codex marketplace
├── .github/plugin/marketplace.json         # Repo-local Copilot CLI marketplace
├── plugins/visualisation/                  # Marketplace plugin package
│   ├── .codex-plugin/plugin.json           # Codex plugin manifest
│   ├── plugin.json                         # Copilot CLI plugin manifest
│   └── skills/architecture-views/          # Skill, references, and HTML asset
├── tools/AgentAwake.cs                     # File-based .NET utility
└── README.md
```

## AgentAwake

`tools/AgentAwake.cs` is a file-based .NET 10 program for Windows. It requests
`PowerRequestSystemRequired` while either coding agent is working, so the PC does not
enter system sleep during an active session. It does not request `DisplayRequired`,
so the normal display timeout remains unchanged.

The program polls once per minute and considers Codex active when it finds either:

- A Codex session transcript under `%USERPROFILE%\.codex\sessions` updated within the last minute.
- A Codex thread-writer lock under `%USERPROFILE%\.codex\thread-writer-locks` whose matching
  transcript has been updated within the last 35 minutes. This grace period is slightly
  longer than T3 Code's roughly 33-minute session-reaper window.
- A recent timestamp from the optional `--touch` signal file.

The utility still reports a held writer lock whose matching transcript has been idle for
at least 35 minutes, but labels it **held but idle** and does not keep the computer awake
for that lock. The lock file is never deleted or modified.

It considers OpenCode active when a local `opencode.exe` server reports a non-idle
session through `/session/status`. An idle OpenCode server alone does not keep the PC
awake.

When both agents become idle, the utility reads the active Windows power plan's
AC/battery **Sleep after** setting and keeps the system awake for that remaining
interval. If the setting is **Never**, the request remains active until the program
stops. A final activity check occurs before releasing the request.

### Run

Requirements: Windows and the .NET 10 SDK.

```powershell
dotnet run .\tools\AgentAwake.cs
```

Stop it with `Ctrl+C`; this releases the power request immediately.

### Optional activity signal

Most desktop and IDE Codex activity updates session transcripts automatically. For a
CLI workflow that does not, call the lightweight signal mode from a Codex `notify`
hook or another wrapper:

```powershell
dotnet run .\tools\AgentAwake.cs -- --touch
```

This only updates `%USERPROFILE%\.codex\codex-awake.activity`. The long-running
instance observes the timestamp on its next poll; it does not modify Codex, OpenCode,
or Windows power-plan configuration.

### Self-test

The Windows power-request recovery behavior can be exercised without putting the PC
to sleep:

```powershell
dotnet run .\tools\AgentAwake.cs -- --self-test
```

## Codex plugin marketplace

The repository marketplace is defined in
[`.agents/plugins/marketplace.json`](.agents/plugins/marketplace.json). It currently
publishes one plugin:

### `visualisation`

The plugin is at [`plugins/visualisation`](plugins/visualisation) and its manifest is
[`plugin.json`](plugins/visualisation/.codex-plugin/plugin.json). It packages the
`architecture-views` skill from [personal-finance PR 11](https://github.com/peterhut/personal-finance/pull/11).

The skill can answer architecture questions about the current codebase and can also
place a branch, pull request, commit range, or working-tree change in context. It
produces disposable, evidence-backed views using:

- Graph views for logical, development, and physical perspectives.
- Mermaid sequence diagrams for process scenarios.
- Search, filtering, inspection, collapse/expand, themes, and PNG export in the HTML viewer.
- Session-only layout presets for direction, density, and routing, plus bounded overview/detail navigation for complex architecture questions.

The viewer uses pinned browser dependencies loaded from jsDelivr. If those dependencies
cannot load, the skill returns an evidence-backed prose fallback rather than silently
creating an unverified diagram. Generated explorer files are temporary unless a user
explicitly requests a repository artifact.

Run the browser-independent preflight before opening a generated artifact. It validates
schema references, graph endpoints, compound-parent cycles, duplicate entries, and
renderer-specific fields without needing Playwright or a browser:

```powershell
node .\tools\preflight-architecture-explorer.mjs --html .\path\to\architecture.html
```

Use optional `displayName` and `displayLabel` fields for concise rendered labels while
keeping full source terminology in the evidence details and search. Sequence input is
normalized to Mermaid-safe aliases and has a readable ordered fallback when rendering
still fails.

### Optional headless verification

When no collaborative browser is available, the agent can use the optional verifier to
open a generated local explorer, exercise the controls, export every view, and write a
diagnostic report. It requires the `playwright` package in the calling environment; the
explorer itself still requires no Node runtime or local server.

```powershell
node .\tools\verify-architecture-explorer.mjs --html .\path\to\architecture.html --output-dir .\verification
```

The verifier uses a fixed 1366×768 target by default. Override it with `--width` and
`--height` when the intended sharing surface has a different size. Exported files use
unique names per attempt so an inspected PNG cannot block a later verification run.
Use `--simulate-deps-failure` to verify the documented dependency-failure fallback.
The report distinguishes automated checks from visual approval and remains `visual inspection pending` after successful checks. It fails on exact post-layout node overlap, label collisions or unavailable label geometry, viewport overflow, invalid endpoints, mismatched taxi direction, and straight-edge obstruction; sub-8-pixel node clearance and sub-4-pixel label clearance are warnings. Cytoscape taxi bends are not exposed, so orthogonal segment obstruction, corridor, border-run, and route-rhythm checks remain explicitly pending visual inspection. Inspect each exact export and its 960-pixel-wide display preview; use `--display-width` to match another destination. A run has a 120-second ceiling. Set `ARCHITECTURE_PLAYWRIGHT_MODULE` to an installed Playwright entry module and `ARCHITECTURE_CHROMIUM_PATH` to an existing Chromium executable when normal package/browser discovery is unavailable.

Run `node tools/test-architecture-explorer.mjs` with the same dependencies to check delayed layout completion, routing/reset behavior, graph-first and sequence-first artifacts, short/full labels, sequence sanitization, schema preflight, sequence-only exports, and the dependency-failure path. Test artifacts are retained in a fresh temporary directory.

### Install locally (Codex)

From the repository root, add the repo marketplace and install the plugin:

```powershell
codex plugin marketplace add .
codex plugin add visualisation@ai-tools
```

After installing or updating a plugin, start a new Codex thread so the new skill
definition is picked up.

### Install locally (GitHub Copilot CLI)

The repository also ships Copilot CLI plugin manifests, so the same
`visualisation` plugin can be installed in the Copilot CLI. From the repository
root:

```powershell
copilot plugin marketplace add .
copilot plugin install visualisation@ai-tools
```

The plugin loads live from the working tree, so local edits take effect on the
next Copilot session. The Copilot manifests live at
[`.github/plugin/marketplace.json`](.github/plugin/marketplace.json) (marketplace)
and [`plugins/visualisation/plugin.json`](plugins/visualisation/plugin.json)
(plugin), and reuse the same `skills/` directory as the Codex package.

## Adding another plugin

Use the same package shape as `visualisation`:

```text
plugins/<plugin-name>/
├── .codex-plugin/plugin.json   # Codex manifest
├── plugin.json                 # Copilot CLI manifest
└── skills/<skill-name>/SKILL.md
```

The plugin folder and both `plugin.json` `name` values must use the same normalized
identifier. Register the plugin in both marketplaces:

- **Codex** — add an entry to `.agents/plugins/marketplace.json` with
  `source.source: "local"`, `source.path: "./plugins/<plugin-name>"`, an
  installation policy, an authentication policy, and a category.
- **Copilot CLI** — add an entry to `.github/plugin/marketplace.json`'s `plugins`
  array: `{ "name": "<plugin-name>", "source": "plugins/<plugin-name>", "description": "..." }`.

Keep marketplace paths relative to the repository root. Validate every new package
with the Codex plugin validator before publishing it.

## Validation

Run the utility's self-test on Windows:

```powershell
dotnet run .\tools\AgentAwake.cs -- --self-test
```

For plugin work, validate both the plugin manifest and each skill's front matter with
the Codex plugin and skill validators. The current `visualisation` package has passed
both validators.

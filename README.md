# AI Tools

Small tools and Codex plugins for working with coding agents.

This repository currently contains:

- `AgentAwake`, a Windows utility that keeps the computer awake while Codex or OpenCode is active.
- A repo-local Codex plugin marketplace.
- The `visualisation` plugin, whose first skill is `architecture-views`.

## Repository layout

```text
.
├── .agents/plugins/marketplace.json       # Repo-local Codex marketplace
├── plugins/visualisation/                  # Marketplace plugin package
│   ├── .codex-plugin/plugin.json           # Plugin manifest
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
- An active Codex thread-writer lock under `%USERPROFILE%\.codex\thread-writer-locks`.
- A recent timestamp from the optional `--touch` signal file.

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

The viewer uses pinned browser dependencies loaded from jsDelivr. If those dependencies
cannot load, the skill returns an evidence-backed prose fallback rather than silently
creating an unverified diagram. Generated explorer files are temporary unless a user
explicitly requests a repository artifact.

### Install locally

From the repository root, add the repo marketplace and install the plugin:

```powershell
codex plugin marketplace add .
codex plugin add visualisation@ai-tools
```

After installing or updating a plugin, start a new Codex thread so the new skill
definition is picked up.

## Adding another plugin

Use the same package shape as `visualisation`:

```text
plugins/<plugin-name>/
├── .codex-plugin/plugin.json
└── skills/<skill-name>/SKILL.md
```

The plugin folder and `plugin.json` `name` must use the same normalized identifier.
Add a corresponding entry to `.agents/plugins/marketplace.json` with:

- `source.source: "local"`
- `source.path: "./plugins/<plugin-name>"`
- An installation policy
- An authentication policy
- A category

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

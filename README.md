# CodexAwake

A small .NET 10 file-based Windows app inspired by [Insomnia](https://github.com/stanley-projects/Insomnia). It polls Codex session transcripts and OpenCode session status, keeping the **PC** awake while either coding agent is active and through a fresh post-activity sleep countdown. It requests `PowerRequestSystemRequired`, not `PowerRequestDisplayRequired`, so Windows may still turn the screens off using the normal display timeout.

## Run

```powershell
dotnet run .\CodexAwake.cs
```

Once per minute, the app checks `%USERPROFILE%\.codex\sessions\**\*.jsonl`, active Codex thread-writer locks, and local servers owned by `opencode.exe`. Codex is active if any transcript changed within the previous minute or a Codex thread-writer lock is currently held; this includes parallel sub-agents even when the main thread is idle or a sub-agent is quiet during a long-running tool call. OpenCode is active when its `/session/status` API reports a non-idle session. This works with direct OpenCode use and clients such as T3 Code without treating an idle, long-running OpenCode server as active. Once neither agent is active, a fresh sleep countdown begins. The app reads the active power plan's AC or battery **Sleep after** value, keeps its system-only power request for exactly that interval, and then releases it. It performs one final activity check before releasing the request. This avoids dropped filesystem events and Windows' separate hidden unattended-sleep timer while leaving the display timer and screen state unchanged.

If **Sleep after** is set to Never, the app retains the request while it runs. Stop the app with `Ctrl+C` to release the request immediately. CodexAwake reads the power plan but never changes it.

## Optional CLI activity signal

Desktop and IDE Codex activity updates the session transcripts. If a particular Codex CLI workflow does not, invoke the app's lightweight signal mode from a Codex `notify` hook:

```powershell
dotnet run .\CodexAwake.cs -- --touch
```

`--touch` only updates `%USERPROFILE%\.codex\codex-awake.activity`; the long-running instance sees that timestamp on its next poll, treats Codex as active, and subsequently starts a fresh Windows **Sleep after** countdown. OpenCode needs no hook because its local session-status API is detected directly. The app never changes Codex or OpenCode configuration or Windows power-plan settings.

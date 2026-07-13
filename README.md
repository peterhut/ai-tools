# CodexAwake

A small .NET 10 file-based Windows app inspired by [Insomnia](https://github.com/stanley-projects/Insomnia). It observes writes to Codex session transcripts and keeps the **PC** awake while Codex is active and through a fresh post-activity sleep countdown. It requests `PowerRequestSystemRequired`, not `PowerRequestDisplayRequired`, so Windows may still turn the screens off using the normal display timeout.

## Run

```powershell
dotnet run .\CodexAwake.cs
```

The app watches `%USERPROFILE%\.codex\sessions\**\*.jsonl`. Three minutes after the last write, Codex is considered idle and a fresh sleep countdown begins. The app reads the active power plan's AC or battery **Sleep after** value, keeps its system-only power request for exactly that interval, and then releases it. This avoids Windows' separate hidden unattended-sleep timer while leaving the display timer and screen state unchanged.

If **Sleep after** is set to Never, the app retains the request while it runs. Stop the app with `Ctrl+C` to release the request immediately. CodexAwake reads the power plan but never changes it.

## Optional CLI activity signal

Desktop and IDE Codex activity updates the session transcripts. If a particular Codex CLI workflow does not, invoke the app's lightweight signal mode from a Codex `notify` hook:

```powershell
dotnet run .\CodexAwake.cs -- --touch
```

`--touch` only updates `%USERPROFILE%\.codex\codex-awake.activity`; the long-running instance sees that update, restarts the three-minute Codex activity window, and subsequently starts a fresh Windows **Sleep after** countdown. The app never changes Codex configuration or Windows power-plan settings.

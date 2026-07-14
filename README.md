# CodexAwake

A small .NET 10 file-based Windows app inspired by [Insomnia](https://github.com/stanley-projects/Insomnia). It polls Codex session transcripts and keeps the **PC** awake while Codex is active and through a fresh post-activity sleep countdown. It requests `PowerRequestSystemRequired`, not `PowerRequestDisplayRequired`, so Windows may still turn the screens off using the normal display timeout.

## Run

```powershell
dotnet run .\CodexAwake.cs
```

Once per minute, the app checks `%USERPROFILE%\.codex\sessions\**\*.jsonl`. If any transcript changed within the previous minute, Codex is active. Otherwise, a fresh sleep countdown begins. The app reads the active power plan's AC or battery **Sleep after** value, keeps its system-only power request for exactly that interval, and then releases it. It performs one final transcript check before releasing the request. This avoids dropped filesystem events and Windows' separate hidden unattended-sleep timer while leaving the display timer and screen state unchanged.

If **Sleep after** is set to Never, the app retains the request while it runs. Stop the app with `Ctrl+C` to release the request immediately. CodexAwake reads the power plan but never changes it.

## Optional CLI activity signal

Desktop and IDE Codex activity updates the session transcripts. If a particular Codex CLI workflow does not, invoke the app's lightweight signal mode from a Codex `notify` hook:

```powershell
dotnet run .\CodexAwake.cs -- --touch
```

`--touch` only updates `%USERPROFILE%\.codex\codex-awake.activity`; the long-running instance sees that timestamp on its next poll, treats Codex as active, and subsequently starts a fresh Windows **Sleep after** countdown. The app never changes Codex configuration or Windows power-plan settings.

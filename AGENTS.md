# Repository instructions

## Plugin versioning

When changing any plugin skill, viewer, verifier, or other behavior, make a minor or patch
release decision and update both plugin manifests before handing off the change:

- Keep `plugins/<name>/plugin.json` and `plugins/<name>/.codex-plugin/plugin.json` on the same
  base semantic version.
- For the Codex manifest, run the repository plugin update flow's
  `update_plugin_cachebuster.py` helper so it has one fresh `+codex.<timestamp>` suffix.
- Leave marketplace metadata versions unchanged unless the marketplace definition itself changed.
- Validate the plugin after the version update with `validate_plugin.py`.

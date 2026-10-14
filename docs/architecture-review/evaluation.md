# Initial assessment evaluation

This is an experimental release. The evidence below exercises the procedure but is not maintainer-adjudicated ground truth or proof of general usefulness.

| Exercise | Evidence and outcome | What remains unknown |
| --- | --- | --- |
| [Photonic exploratory assessment](photonic-assessment.md) | Fresh main snapshot; latest 50 first-parent commits; three candidates investigated; no new recommendations. Package extraction, lifecycle consolidation, and generated delegation supplied counterevidence. | Whether important concerns were missed; whether maintainers agree with dismissals. This run informed the skill, so it is not an independent forward-test of the final instructions. |
| [ai-tools skill forward-test](ai-tools-trial.md) | Independent agent received the written skill and raw repository, without pilot conclusions or research notes. Assessed committed HEAD; identified one present validator-obligation concern, dismissed resolved duplication and unsupported activity/policy coupling. | Whether the inferred validator contract deserves maintenance. The recommendation awaits maintainer judgment; it does not establish worsening or user impact. |
| Photonic executable boundary rules | Three existing package-boundary tests passed with NU1903 warnings made nonfatal. The unmodified default run was blocked at restore by advisory warnings treated as errors. | Broader runtime correctness and the unresolved dependency advisories. |
| [Research-grounded skill and collector trial](photonic-signals-trial.md) | Independent agent used the revised skill and PowerShell collector on pinned Photonic. Two candidates investigated and dismissed; measured signal counts remained separate from design interpretations. | No maintainer adjudication or detection-sensitivity evidence. Earlier test results were not presented as newly executed tests. |

## Behavioral observations

The forward-test used the default window correctly, reported only 18 available first-parent commits in a non-shallow repository, separated dirty development files from assessed source, and checked whether history's apparent duplication had already been removed. It produced a bounded recommendation with counterevidence and a revisit condition rather than a refactoring plan. No actionable instruction defect was observed in this one trial.

No tokens, elapsed development time, regression reduction, precision, or recall are inferred from these runs. Tool use in the Photonic report describes investigation effort, not architectural quality.

## PowerShell collection verification

The dependency-free integration suite uses actual temporary Git repositories. It checks root and empty commits, first-parent merge integration without branch double-counting, broad-change exclusions and matching association-rule denominators, singleton denominators, explicit path filters, renames with Unicode/spaces, raw path identity, dirty state, bounded selection, pinned revisions, invalid option-like revisions, saved JSON, and shallow missing-parent omission. It passed on PowerShell 7.6.7 and Git in the current Linux environment. Windows execution has not been separately verified.

```powershell
pwsh -NoProfile -File plugins/architecture/skills/assess-maintainability/scripts/Test-MaintainabilityHistory.ps1
```

On pinned Photonic, default collection analyzed all 50 selected units. Of these, 36 qualified for pair analysis and 14 exceeded the 30-path cutoff. It observed 1,014 raw changed paths and 255 rename records; displayed path/pair/rename arrays were limited to 20 each. These counts are a reproducibility check, not evidence of architectural quality. The independent trial explains why moving names and broad redesigns constrain interpretation. No CodeScene service, commercial score, line churn, or complexity trend was measured.

## Maintainer adjudication

Review the single ai-tools recommendation without presuming it is correct. Classify it as useful, unsupported, already known, or a reasonable trade-off. Separately judge whether Photonic's three dismissals are justified and nominate a missed concern if one is known. Maintain the difference between an observed validation disagreement and an agreed requirement that both entry points accept the same input.

Before claiming detection value, add real historical cases with a maintainer-confirmed maintenance problem and comparable healthy changes. Pin their revisions and intended behavior, withhold expected outcomes from the assessing agent, then record useful recommendations, unsupported recommendations, missed known concerns, and investigation effort. A no-finding run can demonstrate restraint but does not establish sensitivity. Subsequent agent task-cost experiments remain a separate, later question.

## Packaging verification

The initial release was `0.1.0`; the research and collector update is a minor release to `0.2.0` in both plugin manifests before the Codex cachebuster suffix. Both repository marketplaces register `architecture`; existing marketplace metadata version and the visualisation package are unchanged.

Validation uses the repository-required original helpers recovered from [official Codex commit 985cf47](https://github.com/openai/codex/tree/985cf47a4eb6084b2ff6b30ebdb1216acda85bb4/codex-rs/skills/src/assets/samples/plugin-creator/scripts), plus the installed skill validator. Run the recovered helper paths from temporary storage:

```sh
python3 /tmp/original-codex-plugin-helpers/scripts/update_plugin_cachebuster.py plugins/architecture
python3 /tmp/original-codex-plugin-helpers/scripts/validate_plugin.py plugins/architecture
python3 /home/peter/.codex/skills/.system/skill-creator/scripts/quick_validate.py plugins/architecture/skills/assess-maintainability
```

The validators check package/frontmatter contracts; they do not establish assessment quality. No plugin installation, publish, issue creation, PR creation, or Photonic remediation is part of this release preparation.

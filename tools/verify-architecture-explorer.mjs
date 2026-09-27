#!/usr/bin/env node
// Keep the repository entry point while shipping the verifier with the skill.
export { normalizePlaywrightModule } from '../plugins/visualisation/skills/architecture-views/scripts/verify-architecture-explorer.mjs';
import '../plugins/visualisation/skills/architecture-views/scripts/verify-architecture-explorer.mjs';

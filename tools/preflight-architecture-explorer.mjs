#!/usr/bin/env node

import fs from 'node:fs/promises';

const allowedPerspectives = new Set(['Logical', 'Process', 'Development', 'Physical']);
const allowedRenderers = new Set(['graph', 'sequence']);
const allowedChangeStates = new Set(['added', 'modified', 'removed', 'unchanged']);
const allowedEvidenceStates = new Set(['observed', 'documented', 'inferred', 'conflict']);
const allowedConfidenceLevels = new Set(['high', 'medium', 'low']);
const allowedDirections = new Set(['RIGHT', 'DOWN']);
const allowedDensities = new Set(['COMPACT', 'BALANCED', 'SPACIOUS']);
const allowedRoutings = new Set(['ORTHOGONAL', 'STRAIGHT']);

export function fingerprintEncodedPayload(value) {
  let hash = 2166136261;
  for (const character of String(value)) {
    hash ^= character.charCodeAt(0);
    hash = Math.imul(hash, 16777619);
  }
  return `fnv1a-${(hash >>> 0).toString(16).padStart(8, '0')}`;
}

export function validateArchitectureData(data) {
  const errors = [];
  const object = value => value && typeof value === 'object' && !Array.isArray(value);
  const text = value => typeof value === 'string' && value.trim().length > 0;
  if (!object(data)) return ['The data root must be an object.'];
  for (const key of ['catalog', 'relationships', 'views']) if (!Array.isArray(data[key])) errors.push(`${key} must be an array.`);
  if (errors.length) return errors;
  if (!data.views.length) errors.push('views must contain at least one view.');
  if (!object(data.meta) || !text(data.meta.title) || !text(data.meta.question) || !text(data.meta.repositoryRoot)) errors.push('meta requires non-empty title, question, and repositoryRoot strings.');
  const transition = data.comparison !== undefined;
  if (transition && (!object(data.comparison) || !text(data.comparison.base) || !text(data.comparison.head))) errors.push('comparison requires non-empty base and head strings.');

  const catalog = new Map();
  data.catalog.forEach((item, index) => {
    if (!object(item) || !text(item.id) || !text(item.name) || !text(item.kind)) { errors.push(`catalog[${index}] requires non-empty id, name, and kind strings.`); return; }
    if (catalog.has(item.id)) errors.push(`Duplicate catalog id: ${item.id}`);
    catalog.set(item.id, item);
    if (!allowedEvidenceStates.has(item.evidenceState) || !allowedConfidenceLevels.has(item.confidence) || !Array.isArray(item.evidence)) errors.push(`catalog[${index}] requires valid evidenceState, confidence, and evidence.`);
    if (Array.isArray(item.evidence)) item.evidence.forEach((evidence, evidenceIndex) => { if (!object(evidence) || !text(evidence.path) || (evidence.line !== undefined && (!Number.isInteger(evidence.line) || evidence.line < 1))) errors.push(`catalog[${index}].evidence[${evidenceIndex}] is invalid.`); });
    if (item.displayName !== undefined && !text(item.displayName)) errors.push(`catalog[${index}].displayName must be a non-empty string when present.`);
    if (transition && !allowedChangeStates.has(item.changeState)) errors.push(`catalog[${index}] requires a valid changeState in comparison mode.`);
  });

  const relationships = new Map();
  data.relationships.forEach((relationship, index) => {
    if (!object(relationship) || !text(relationship.id) || !text(relationship.source) || !text(relationship.target) || !text(relationship.label)) { errors.push(`relationships[${index}] requires non-empty id, source, target, and label strings.`); return; }
    if (relationships.has(relationship.id)) errors.push(`Duplicate relationship id: ${relationship.id}`);
    relationships.set(relationship.id, relationship);
    if (!allowedEvidenceStates.has(relationship.evidenceState) || !allowedConfidenceLevels.has(relationship.confidence) || !Array.isArray(relationship.evidence)) errors.push(`relationships[${index}] requires valid evidenceState, confidence, and evidence.`);
    if (Array.isArray(relationship.evidence)) relationship.evidence.forEach((evidence, evidenceIndex) => { if (!object(evidence) || !text(evidence.path) || (evidence.line !== undefined && (!Number.isInteger(evidence.line) || evidence.line < 1))) errors.push(`relationships[${index}].evidence[${evidenceIndex}] is invalid.`); });
    if (relationship.displayLabel !== undefined && !text(relationship.displayLabel)) errors.push(`relationships[${index}].displayLabel must be a non-empty string when present.`);
    if (!catalog.has(relationship.source) || !catalog.has(relationship.target)) errors.push(`Relationship ${relationship.id} points to an unknown element.`);
    if (relationship.emphasis !== undefined && !['hero', 'muted'].includes(relationship.emphasis)) errors.push(`Relationship ${relationship.id} emphasis must be hero or muted.`);
    if (transition && !allowedChangeStates.has(relationship.changeState)) errors.push(`Relationship ${relationship.id} requires a valid changeState in comparison mode.`);
  });

  const views = new Map();
  data.views.forEach((view, index) => {
    if (!object(view) || !text(view.id) || !text(view.label) || !text(view.level) || typeof view.scenario !== 'string' || !allowedPerspectives.has(view.perspective) || !allowedRenderers.has(view.renderer)) { errors.push(`views[${index}] is incomplete or has an invalid perspective/renderer.`); return; }
    if (views.has(view.id)) errors.push(`Duplicate view id: ${view.id}`);
    views.set(view.id, view);
    if (view.direction !== undefined && !allowedDirections.has(view.direction)) errors.push(`View ${view.id} direction must be RIGHT or DOWN.`);
    if (view.layout !== undefined) {
      if (!object(view.layout)) errors.push(`View ${view.id}.layout must be an object.`);
      else {
        if (view.layout.direction !== undefined && !allowedDirections.has(view.layout.direction)) errors.push(`View ${view.id}.layout.direction must be RIGHT or DOWN.`);
        if (view.layout.density !== undefined && !allowedDensities.has(view.layout.density)) errors.push(`View ${view.id}.layout.density must be COMPACT, BALANCED, or SPACIOUS.`);
        if (view.layout.routing !== undefined && !allowedRoutings.has(view.layout.routing)) errors.push(`View ${view.id}.layout.routing must be ORTHOGONAL or STRAIGHT.`);
      }
    }
    if (view.renderer === 'graph') {
      if (!Array.isArray(view.nodes) || !Array.isArray(view.edges)) { errors.push(`Graph view ${view.id} requires nodes and edges arrays.`); return; }
      const nodeIds = new Set();
      for (const node of view.nodes) {
        if (!object(node) || !text(node.id)) { errors.push(`Graph view ${view.id} contains an invalid node.`); continue; }
        if (!catalog.has(node.id)) errors.push(`View ${view.id} references unknown node ${node.id}.`);
        if (nodeIds.has(node.id)) errors.push(`View ${view.id} includes duplicate node ${node.id}.`);
        nodeIds.add(node.id);
      }
      const visiting = new Set();
      const visited = new Set();
      const visit = id => {
        if (visiting.has(id)) { errors.push(`View ${view.id} contains a compound-parent cycle at ${id}.`); return; }
        if (visited.has(id)) return;
        visiting.add(id);
        const parent = view.nodes.find(node => node?.id === id)?.parent;
        if (parent && !nodeIds.has(parent)) errors.push(`View ${view.id} parent ${parent} must also appear in the view.`);
        if (parent && nodeIds.has(parent)) visit(parent);
        visiting.delete(id);
        visited.add(id);
      };
      for (const node of view.nodes) if (node?.id) visit(node.id);
      const edgeIds = new Set();
      let heroCount = 0;
      for (const id of view.edges) {
        if (edgeIds.has(id)) errors.push(`View ${view.id} includes duplicate relationship ${id}.`);
        edgeIds.add(id);
        const relationship = relationships.get(id);
        if (!relationship) { errors.push(`View ${view.id} references unknown relationship ${id}.`); continue; }
        if (!nodeIds.has(relationship.source) || !nodeIds.has(relationship.target)) errors.push(`View ${view.id} relationship ${id} requires both endpoints in the view.`);
        if (relationship.emphasis === 'hero') heroCount += 1;
      }
      if (heroCount > 1) errors.push(`Graph view ${view.id} may contain at most one hero relationship.`);
      if (transition && view.edges.length && heroCount !== 1) errors.push(`Transition graph view ${view.id} requires exactly one hero relationship when it has edges.`);
    } else if (!Array.isArray(view.participants) || !view.participants.length || typeof view.mermaid !== 'string' || !view.mermaid.trim()) {
      errors.push(`Sequence view ${view.id} requires a non-empty participants array and mermaid source.`);
    } else {
      const participantIds = new Set();
      for (const id of view.participants) {
        if (!catalog.has(id)) errors.push(`View ${view.id} references unknown participant ${id}.`);
        if (participantIds.has(id)) errors.push(`View ${view.id} includes duplicate participant ${id}.`);
        participantIds.add(id);
      }
    }
    if (view.drilldowns !== undefined && !Array.isArray(view.drilldowns)) errors.push(`View ${view.id}.drilldowns must be an array when present.`);
    for (const drilldown of Array.isArray(view.drilldowns) ? view.drilldowns : []) {
      if (!object(drilldown) || !text(drilldown.element) || !text(drilldown.targetView)) { errors.push(`View ${view.id} contains an invalid drilldown.`); continue; }
      if (drilldown.label !== undefined && !text(drilldown.label)) errors.push(`View ${view.id} contains a drilldown with an invalid label.`);
      const members = [...(Array.isArray(view.nodes) ? view.nodes.map(node => node?.id) : []), ...(Array.isArray(view.participants) ? view.participants : [])];
      if (!members.includes(drilldown.element)) errors.push(`View ${view.id} drilldown element ${drilldown.element} must be present in the view.`);
    }
  });
  for (const view of data.views) {
    if (!object(view)) continue;
    if (view.backView !== undefined && (!text(view.backView) || !views.has(view.backView))) errors.push(`View ${view.id}.backView must reference an existing view.`);
    for (const drilldown of Array.isArray(view.drilldowns) ? view.drilldowns : []) {
      if (text(drilldown?.targetView) && !views.has(drilldown.targetView)) errors.push(`View ${view.id} drilldown target ${drilldown.targetView} must reference an existing view.`);
    }
  }
  if (data.meta?.primaryView !== undefined && !views.has(data.meta.primaryView)) errors.push('meta.primaryView must reference an existing view.');
  return errors;
}

export function decodeArchitectureHtml(html) {
  const match = String(html).match(/<script type="text\/plain" id="architecture-data">([\s\S]*?)<\/script>/i);
  if (!match) throw new Error('The HTML does not contain an architecture-data payload.');
  return JSON.parse(Buffer.from(match[1].trim(), 'base64').toString('utf8'));
}

async function main() {
  const args = process.argv.slice(2);
  const htmlIndex = args.indexOf('--html');
  const jsonIndex = args.indexOf('--json');
  if ((htmlIndex === -1) === (jsonIndex === -1)) throw new Error('Specify exactly one of --html or --json.');
  const sourcePath = args[htmlIndex >= 0 ? htmlIndex + 1 : jsonIndex + 1];
  if (!sourcePath) throw new Error('A source path is required.');
  const source = await fs.readFile(sourcePath, 'utf8');
  const data = htmlIndex >= 0 ? decodeArchitectureHtml(source) : JSON.parse(source);
  const errors = validateArchitectureData(data);
  console.log(JSON.stringify({ valid: errors.length === 0, errors }, null, 2));
  if (errors.length) process.exitCode = 1;
}

if (process.argv[1]?.endsWith('preflight-architecture-explorer.mjs')) main().catch(error => { console.error(error.message); process.exitCode = 2; });

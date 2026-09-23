import assert from 'node:assert/strict';
import fs from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { execFile } from 'node:child_process';
import { promisify } from 'node:util';
import { normalizePlaywrightModule } from './verify-architecture-explorer.mjs';

const run = promisify(execFile);
const namedPlaywright = { chromium: { launch: () => {} } };
const defaultPlaywright = { default: namedPlaywright };
assert.equal(normalizePlaywrightModule(namedPlaywright), namedPlaywright);
assert.equal(normalizePlaywrightModule(defaultPlaywright), namedPlaywright);
const directory = await fs.mkdtemp(path.join(os.tmpdir(), 'architecture-regression-'));
const template = await fs.readFile('plugins/visualisation/skills/architecture-views/assets/architecture-explorer.html', 'utf8');
const catalog = [
  { id: 'client', name: 'Client application / browser shell', displayName: 'Client', kind: 'Container', evidenceState: 'observed', confidence: 'high', evidence: [] },
  { id: 'api', name: 'API service (HTTP boundary)', displayName: 'API', kind: 'Container', evidenceState: 'observed', confidence: 'high', evidence: [] }
];
const graph = { id: 'graph', label: 'Graph', level: 'Container', perspective: 'Logical', scenario: '', renderer: 'graph', direction: 'RIGHT', nodes: catalog.map(({ id }) => ({ id })), edges: ['calls'] };
const sequence = { id: 'sequence', label: 'Sequence', level: 'Container', perspective: 'Process', scenario: 'Request', renderer: 'sequence', participants: ['client', 'api'], mermaid: 'sequenceDiagram\n participant client as Client (browser)\n participant api as API/service\n client->>api: fetchData(request, ...);\n api->>api: cache.set("request")' };
for (const [name, views, failure] of [['graph-first', [graph, sequence], null], ['sequence-first', [sequence, graph], null], ['sequence-only', [sequence], null], ['geometry-defect', [graph], 'geometry'], ['dependency-failure', [graph], 'dependency']]) {
  const data = { meta: { title: name, question: 'Request path', repositoryRoot: 'C:/fixture', artifactId: name }, catalog, relationships: [{ id: 'calls', source: 'client', target: 'api', label: 'Requests data with full evidence', displayLabel: 'Requests data', evidenceState: 'observed', confidence: 'high', evidence: [] }], views };
  // Delay completion beyond the old 250ms sleep to regress premature exports.
  // The defect fixture instead forces two leaves to the same final position so
  // the verifier must fail on exact post-layout geometry.
  const stop = failure === 'geometry'
    ? 'stop: () => { const testLeaves = cy.nodes().filter(node => !node.isParent()); testLeaves[1].position(testLeaves[0].position());'
    : 'stop: async () => { await new Promise(resolve => setTimeout(resolve, 700));';
  const html = template.replace('stop: () => {', stop);
  const htmlPath = path.join(directory, `${name}.html`);
  await fs.writeFile(htmlPath, html.replace('__ARCHITECTURE_DATA_BASE64__', Buffer.from(JSON.stringify(data)).toString('base64')));
  let stdout;
  try {
    ({ stdout } = await run(process.execPath, ['tools/verify-architecture-explorer.mjs', '--html', htmlPath, '--output-dir', path.join(directory, name), ...(failure === 'dependency' ? ['--simulate-deps-failure'] : [])], { timeout: 150000 }));
  } catch (error) {
    if (!failure) throw error;
    assert.equal(error.code, failure === 'dependency' ? 2 : 1);
    stdout = error.stdout;
  }
  const report = JSON.parse(stdout);
  assert.equal(report.status, failure === 'dependency' ? 'verification unavailable' : failure === 'geometry' ? 'verification failed' : 'visual inspection pending');
  if (failure !== 'dependency') {
    assert.equal(report.automatedChecks, failure === 'geometry' ? 'failed' : 'passed');
    assert.equal(report.geometry.errors > 0, failure === 'geometry');
    if (failure === 'geometry') assert.equal(report.geometry.issues.some(issue => issue.code === 'node-overlap'), true);
    if (views[0].renderer === 'graph') {
      assert.deepEqual(report.controlCandidate.edgeRouting, [{ direction: 'downward', curve: 'straight' }]);
      assert.deepEqual(report.views[0].edgeRouting, [{ direction: 'rightward', curve: 'taxi' }]);
      assert.equal(report.views[0].geometry.routeCoverage, 'taxi-endpoints-and-labels-only');
      assert.equal(report.views[0].geometry.metrics.labels, 1);
    }
    assert.deepEqual(report.views.map(view => view.activeView), views.map(view => view.id));
    for (const view of report.views) {
      assert.equal(view.renderRevision, view.completedRevision);
      assert.equal(view.visualInspection, 'pending');
      assert.equal((await fs.readFile(view.displayPreview)).readUInt32BE(16), 960);
    }
  }
  console.log(`${name}: passed`);
}

const invalidData = { meta: { title: 'invalid', question: 'Request path', repositoryRoot: 'C:/fixture' }, catalog, relationships: [{ id: 'calls', source: 'client', target: 'missing', label: 'Broken', evidenceState: 'observed', confidence: 'high', evidence: [] }], views: [{ ...graph, edges: ['calls', 'calls'], nodes: [{ id: 'client', parent: 'api' }, { id: 'api', parent: 'client' }] }] };
const invalidHtmlPath = path.join(directory, 'schema-invalid.html');
await fs.writeFile(invalidHtmlPath, template.replace('__ARCHITECTURE_DATA_BASE64__', Buffer.from(JSON.stringify(invalidData)).toString('base64')));
let invalidStdout;
try {
  ({ stdout: invalidStdout } = await run(process.execPath, ['tools/verify-architecture-explorer.mjs', '--html', invalidHtmlPath, '--output-dir', path.join(directory, 'schema-invalid')], { timeout: 150000 }));
} catch (error) {
  assert.equal(error.code, 1);
  invalidStdout = error.stdout;
}
const invalidReport = JSON.parse(invalidStdout);
assert.equal(invalidReport.status, 'artifact invalid');
assert.equal(invalidReport.failureKind, 'schema');
console.log('schema-invalid: passed');
console.log(`Artifacts retained at ${directory}`);

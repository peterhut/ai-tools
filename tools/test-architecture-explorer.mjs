import assert from 'node:assert/strict';
import fs from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { execFile } from 'node:child_process';
import { promisify } from 'node:util';

const run = promisify(execFile);
const directory = await fs.mkdtemp(path.join(os.tmpdir(), 'architecture-regression-'));
const template = await fs.readFile('plugins/visualisation/skills/architecture-views/assets/architecture-explorer.html', 'utf8');
const catalog = ['client', 'api'].map(id => ({ id, name: id, kind: 'Container', evidenceState: 'observed', confidence: 'high', evidence: [] }));
const graph = { id: 'graph', label: 'Graph', level: 'Container', perspective: 'Logical', scenario: '', renderer: 'graph', direction: 'RIGHT', nodes: catalog.map(({ id }) => ({ id })), edges: ['calls'] };
const sequence = { id: 'sequence', label: 'Sequence', level: 'Container', perspective: 'Process', scenario: 'Request', renderer: 'sequence', participants: ['client', 'api'], mermaid: 'sequenceDiagram\n participant client\n participant api\n client->>api: Requests data' };
for (const [name, views, failure] of [['graph-first', [graph, sequence], false], ['sequence-first', [sequence, graph], false], ['sequence-only', [sequence], false], ['dependency-failure', [graph], true]]) {
  const data = { meta: { title: name, question: 'Request path', repositoryRoot: 'C:/fixture' }, catalog, relationships: [{ id: 'calls', source: 'client', target: 'api', label: 'Requests data', evidenceState: 'observed', confidence: 'high', evidence: [] }], views };
  // Delay completion beyond the old 250ms sleep to regress premature exports.
  const html = template.replace('stop: () => {', 'stop: async () => { await new Promise(resolve => setTimeout(resolve, 700));');
  const htmlPath = path.join(directory, `${name}.html`);
  await fs.writeFile(htmlPath, html.replace('__ARCHITECTURE_DATA_BASE64__', Buffer.from(JSON.stringify(data)).toString('base64')));
  let stdout;
  try {
    ({ stdout } = await run(process.execPath, ['tools/verify-architecture-explorer.mjs', '--html', htmlPath, '--output-dir', path.join(directory, name), ...(failure ? ['--simulate-deps-failure'] : [])], { timeout: 150000 }));
  } catch (error) {
    if (!failure) throw error;
    assert.equal(error.code, 2);
    stdout = error.stdout;
  }
  const report = JSON.parse(stdout);
  assert.equal(report.status, failure ? 'verification unavailable' : 'visual inspection pending');
  if (!failure) {
    assert.equal(report.automatedChecks, 'passed');
    if (views[0].renderer === 'graph') {
      assert.deepEqual(report.controlCandidate.edgeRouting, [{ direction: 'downward', curve: 'straight' }]);
      assert.deepEqual(report.views[0].edgeRouting, [{ direction: 'rightward', curve: 'taxi' }]);
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
console.log(`Artifacts retained at ${directory}`);

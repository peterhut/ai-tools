import assert from 'node:assert/strict';
import fs from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { pathToFileURL } from 'node:url';
import { validateArchitectureData } from './preflight-architecture-explorer.mjs';

const modulePath = process.env.ARCHITECTURE_PLAYWRIGHT_MODULE || 'playwright';
const { chromium } = await import(path.isAbsolute(modulePath) ? pathToFileURL(modulePath).href : modulePath);
const directory = await fs.mkdtemp(path.join(os.tmpdir(), 'architecture-interactions-'));
const catalog = ['boundary', 'client', 'api', 'store', 'unrelated'].map(id => ({
  id, name: id, kind: id === 'boundary' ? 'Boundary' : 'Container',
  evidenceState: 'observed', confidence: 'high', evidence: []
}));
const relationships = [['request', 'client', 'api'], ['persist', 'api', 'store']].map(([id, source, target]) => ({
  id, source, target, label: id, evidenceState: 'observed', confidence: 'high', evidence: []
}));
const data = {
  meta: { title: 'Connection highlighting', question: 'Inspect direct relationships', repositoryRoot: 'C:/fixture' },
  catalog, relationships,
  views: [{ id: 'graph', label: 'Graph', level: 'Container', perspective: 'Logical', scenario: '', renderer: 'graph',
    nodes: catalog.map(({ id }) => ({ id, ...(id === 'client' || id === 'api' ? { parent: 'boundary' } : {}) })),
    edges: relationships.map(({ id }) => id) }]
};
const invalidType = structuredClone(data);
invalidType.relationships[0].type = 'unknown';
assert.ok(validateArchitectureData(invalidType).some(error => error.includes('.type')));
invalidType.relationships[0].type = 'calls';
invalidType.relationships[0].asynchronous = 'yes';
assert.ok(validateArchitectureData(invalidType).some(error => error.includes('.asynchronous')));
let template = await fs.readFile('plugins/visualisation/skills/architecture-views/assets/architecture-explorer.html', 'utf8');
// Only this fixture exposes the graph for precise event and position assertions.
template = template.replace('window.__architectureExplorer = {', 'window.__testGraph = () => state.cy;\n    window.__architectureExplorer = {');
const htmlPath = path.join(directory, 'connections.html');
await fs.writeFile(htmlPath, template.replace('__ARCHITECTURE_DATA_BASE64__', Buffer.from(JSON.stringify(data)).toString('base64')));
const browser = await chromium.launch();
try {
  const page = await browser.newPage({ viewport: { width: 1366, height: 900 } });
  page.setDefaultTimeout(15000);
  await page.goto(pathToFileURL(htmlPath).href);
  const ready = () => page.waitForFunction(() => window.__architectureExplorer?.diagnostics().ready);
  await ready();
  const diagnostics = () => page.evaluate(() => window.__architectureExplorer.diagnostics());
  assert.equal((await diagnostics()).layout.routing, 'STRAIGHT');
  assert.ok((await diagnostics()).edgeRouting.every(edge => edge.curve === 'straight'));
  assert.equal((await diagnostics()).geometry.errors, 0);
  const position = await page.evaluate(() => window.__testGraph().getElementById('client').renderedPosition());
  const bounds = await page.locator('#graph').boundingBox();
  await page.mouse.move(bounds.x + position.x, bounds.y + position.y);
  await page.waitForFunction(() => window.__architectureExplorer.diagnostics().hover.connected.includes('client'));
  assert.deepEqual((await diagnostics()).hover.connected.sort(), ['api', 'boundary', 'client', 'request']);
  assert.deepEqual((await diagnostics()).hover.dimmed.sort(), ['persist', 'store', 'unrelated']);
  await page.screenshot({ path: path.join(directory, 'hover.png') });
  await page.mouse.move(0, 0);
  await page.waitForFunction(() => window.__architectureExplorer.diagnostics().hover.connected.length === 0);
  assert.equal((await diagnostics()).hover.connected.length, 0);
  await page.evaluate(() => { window.__testGraph().getElementById('persist').emit('mouseover'); });
  assert.deepEqual((await diagnostics()).hover.connected.sort(), ['api', 'boundary', 'persist', 'store']);
  await page.evaluate(() => { window.__testGraph().getElementById('persist').emit('mouseout'); });
  await page.locator('#tools-open').click();
  await page.locator('#search').fill('client');
  await page.evaluate(() => { window.__testGraph().getElementById('client').emit('mouseover'); });
  assert.equal(await page.evaluate(() => window.__testGraph().getElementById('api').hasClass('dimmed')), true);
  await page.evaluate(() => { window.__testGraph().getElementById('client').emit('mouseout'); });
  assert.equal(await page.evaluate(() => window.__testGraph().getElementById('api').hasClass('dimmed')), true);
  await page.locator('#search').fill('');
  for (const [routing, curve] of [['STRAIGHT', 'straight'], ['ORTHOGONAL', 'taxi'], ['CURVED', 'unbundled-bezier']]) {
    await page.locator('#layout-routing').selectOption(routing);
    await ready();
    assert.ok((await diagnostics()).edgeRouting.every(edge => edge.curve === curve));
  }
  await page.locator('[data-close="tools-dialog"]').click();
  await page.screenshot({ path: path.join(directory, 'curved.png') });
  const comparison = structuredClone(data);
  comparison.meta.repository = 'fixture';
  comparison.meta.generatedAt = '2026-10-03T09:00:00Z';
  comparison.comparison = { base: 'before', head: 'after' };
  comparison.catalog.forEach((item, index) => { item.changeState = ['unchanged', 'added', 'modified', 'removed', 'unchanged'][index]; });
  Object.assign(comparison.relationships[0], { type: 'calls', changeState: 'added', evidenceState: 'inferred', emphasis: 'hero' });
  Object.assign(comparison.relationships[1], { type: 'reads', changeState: 'removed', asynchronous: true, evidenceState: 'conflict' });
  const comparisonPath = path.join(directory, 'comparison.html');
  await fs.writeFile(comparisonPath, template.replace('__ARCHITECTURE_DATA_BASE64__', Buffer.from(JSON.stringify(comparison)).toString('base64')));
  await page.goto(pathToFileURL(comparisonPath).href);
  await ready();
  const styles = () => page.evaluate(() => window.__testGraph().edges().map(edge => ({
    id: edge.id(), line: edge.style('line-color'), pattern: edge.style('line-style'),
    halo: edge.style('underlay-color'), label: edge.data('label')
  })));
  const beforeHover = await styles();
  assert.equal(beforeHover[0].line, 'rgb(96,165,250)');
  assert.equal(beforeHover[0].pattern, 'solid');
  assert.match(beforeHover[0].label, /^\+ /);
  assert.match(beforeHover[0].label, /\?/);
  assert.equal(beforeHover[1].line, 'rgb(45,212,191)');
  assert.equal(beforeHover[1].pattern, 'dashed');
  assert.match(beforeHover[1].label, /^− /);
  const compactLegend = await page.locator('#canvas-legend').innerText();
  assert.match(compactLegend, /Calls/);
  assert.match(compactLegend, /Reads/);
  assert.match(compactLegend, /Added/);
  assert.match(compactLegend, /Removed/);
  assert.match(compactLegend, /Modified/);
  assert.doesNotMatch(compactLegend, /Writes|Sends|Implements/);
  await page.evaluate(() => { window.__testGraph().getElementById('request').emit('mouseover'); });
  assert.deepEqual((await styles()).map(({ line, halo }) => ({ line, halo })), beforeHover.map(({ line, halo }) => ({ line, halo })));
  await page.evaluate(() => { window.__testGraph().getElementById('request').emit('mouseout'); });
  await page.locator('#export').click();
  await page.locator('#export-dialog[open]').waitFor();
  await page.locator('#export-image').evaluate(image => image.decode());
  const metadata = (await diagnostics()).exportMetadata;
  assert.equal(metadata.generatedAt, comparison.meta.generatedAt);
  assert.ok(Number.isFinite(Date.parse(metadata.exportedAt)));
  assert.equal(metadata.question, comparison.meta.question);
  assert.equal(metadata.comparison, 'before → after');
  assert.equal(metadata.level, 'Container');
  const downloadPromise = page.waitForEvent('download');
  await page.locator('#export-download').click();
  await (await downloadPromise).saveAs(path.join(directory, 'comparison.png'));
  await page.screenshot({ path: path.join(directory, 'comparison-export.png') });
  await page.locator('[data-close="export-dialog"]').click();
  await page.setViewportSize({ width: 320, height: 568 });
  await page.locator('#fit').click();
  assert.ok(await page.evaluate(() => document.documentElement.scrollWidth <= innerWidth));
  assert.ok(await page.evaluate(() => document.querySelector('.canvas').clientHeight > innerHeight * .4));
  assert.equal(await page.locator('#canvas-legend').isVisible(), true);
  await page.screenshot({ path: path.join(directory, 'comparison-mobile.png') });
  console.log(`Hover, routing, semantic styles, change halos, metadata export and mobile legend: passed. Artifacts: ${directory}`);
} catch (error) {
  console.error(error);
  throw error;
} finally {
  await browser.close();
}

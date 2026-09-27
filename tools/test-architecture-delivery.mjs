// Real-browser regression coverage for remote/mobile delivery. No live T3 server is used.
import assert from 'node:assert/strict';
import fs from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import http from 'node:http';
import { pathToFileURL } from 'node:url';

const modulePath = process.env.ARCHITECTURE_PLAYWRIGHT_MODULE || 'playwright';
const playwright = await import(path.isAbsolute(modulePath) ? pathToFileURL(modulePath).href : modulePath);
const engine = process.env.ARCHITECTURE_TEST_BROWSER || 'chromium';
const browser = await playwright[engine].launch(engine === 'chromium' && process.env.ARCHITECTURE_CHROMIUM_PATH ? { executablePath: process.env.ARCHITECTURE_CHROMIUM_PATH } : {});
const directory = await fs.mkdtemp(path.join(os.tmpdir(), 'architecture-delivery-'));
const template = await fs.readFile('plugins/visualisation/skills/architecture-views/assets/architecture-explorer.html', 'utf8');
const evidence = [{ path: '/tmp/evidence/example.ts', line: 1, note: 'Test fixture, not an architecture claim.' }];
const catalog = ['client', 'api', 'group'].map(id => ({ id, name: id, kind: id === 'group' ? 'Boundary' : 'Container', evidenceState: 'observed', confidence: 'high', evidence }));
const graph = { id: 'graph', label: 'Delivery structure', level: 'Container', perspective: 'Logical', scenario: 'A long scenario that must not push the controls or page outside the viewport', renderer: 'graph', direction: 'DOWN', nodes: [{ id: 'client' }, { id: 'api', parent: 'group' }, { id: 'group' }], edges: ['calls'] };
const sequence = { id: 'sequence', label: 'Delivery sequence', level: 'Container', perspective: 'Process', scenario: 'Request', renderer: 'sequence', participants: ['client', 'api'], mermaid: 'sequenceDiagram\n participant client as iPhone client\n participant api as Mini PC host\n client->>api: Requests generated HTML\n api-->>client: HTML with embedded model\n Note over client,api: The preview must remain available if downloads are blocked' };
const data = { meta: { title: 'HTML delivery from the Mini PC to the iPhone app with a long title', question: 'How does generated HTML reach the app?', repositoryRoot: '/tmp/fixture' }, catalog, relationships: [{ id: 'calls', source: 'client', target: 'api', label: 'Requests HTML', evidenceState: 'observed', confidence: 'high', evidence }], views: [graph, sequence] };
const html = template.replace('__ARCHITECTURE_DATA_BASE64__', Buffer.from(JSON.stringify(data)).toString('base64'));
const filePath = path.join(directory, 'index.html');
await fs.writeFile(filePath, html);
const server = http.createServer((request, response) => {
  response.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8', 'Content-Security-Policy': 'sandbox allow-scripts allow-forms allow-popups allow-modals' });
  response.end(html);
});
await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
const ready = page => page.waitForFunction(() => window.__architectureExplorer?.diagnostics().ready);
async function close(page, id) { await page.locator(`[data-close="${id}"]`).click(); }
async function assertCanvas(page) {
  const metrics = await page.evaluate(() => {
    const rect = document.querySelector('.canvas').getBoundingClientRect();
    return { width: innerWidth, height: innerHeight, scrollWidth: document.documentElement.scrollWidth, canvasHeight: rect.height, canvasBottom: rect.bottom, dialogs: document.querySelectorAll('dialog[open]').length };
  });
  assert.ok(metrics.scrollWidth <= metrics.width, `horizontal page overflow: ${JSON.stringify(metrics)}`);
  assert.ok(metrics.canvasHeight >= metrics.height * .5, `canvas too small: ${JSON.stringify(metrics)}`);
  assert.ok(metrics.canvasBottom <= metrics.height);
  assert.equal(metrics.dialogs, 0);
  await page.waitForFunction(() => {
    const { graphBounds: b, canvasSize: c } = window.__architectureExplorer.diagnostics();
    return !b || (b.x1 >= -1 && b.y1 >= -1 && b.x2 <= c.width + 1 && b.y2 <= c.height + 1);
  });
  for (const id of ['fit', 'details-open', 'tools-open', 'export', 'zoom-in', 'zoom-out']) {
    const box = await page.locator(`#${id}`).boundingBox();
    assert.ok(box && box.width >= 44 && box.height >= 44, `${id} needs a reachable touch target`);
    assert.ok(box.x >= 0 && box.x + box.width <= metrics.width + 1, `${id} outside screen`);
  }
}
try {
  for (const sandbox of [false, true]) {
    const context = await browser.newContext({ viewport: { width: 390, height: 700 }, isMobile: true, hasTouch: true, colorScheme: 'light' });
    const page = await context.newPage();
    page.setDefaultTimeout(15000);
    const errors = [];
    const warnings = [];
    page.on('pageerror', e => errors.push(e.message));
    page.on('console', m => { if (m.type() === 'warning' || m.type() === 'error') warnings.push(m.text()); });
    await page.goto(sandbox ? `http://127.0.0.1:${server.address().port}/` : pathToFileURL(filePath).href);
    await ready(page);
    assert.equal(await page.evaluate(() => document.documentElement.dataset.theme), 'dark', 'delivery defaults to dark even on a light host');
    for (const viewport of [{ width: 320, height: 568 }, { width: 390, height: 700 }, { width: 844, height: 390 }, { width: 1366, height: 768 }]) {
      await page.setViewportSize(viewport);
      await page.locator('#fit').click();
      await assertCanvas(page);
    }
    await page.setViewportSize({ width: 390, height: 700 });
    await page.locator('#fit').click();
    await page.locator('#details-open').click();
    assert.equal(await page.locator('#legend').isVisible(), true);
    await close(page, 'details-dialog');
    assert.equal(await page.evaluate(() => document.activeElement.id), 'details-open');
    await page.locator('#tools-open').click();
    await page.locator('#search').fill('api');
    await page.locator('#search').press('Enter');
    assert.equal(await page.locator('#details-dialog').isVisible(), true);
    assert.equal(await page.locator('#tools-dialog').isVisible(), false);
    assert.match(await page.locator('.source-link').first().getAttribute('href'), /^vscode:\/\/file\/\/tmp\/evidence\/example.ts/);
    await close(page, 'details-dialog');
    await page.locator('#tools-open').click();
    await page.locator('#reset').click();
    await ready(page);
    await page.locator('#theme').click();
    await ready(page);
    for (let attempt = 0; attempt < 3 && await page.evaluate(() => window.__architectureExplorer.diagnostics().theme !== 'dark'); attempt += 1) {
      await page.locator('#theme').click();
      await ready(page);
    }
    await close(page, 'tools-dialog');
    await page.locator('#fit').click();
    await assertCanvas(page);
    await page.screenshot({ path: path.join(directory, `${sandbox ? 'sandbox' : 'file'}-graph.png`) });
    const zoom = await page.evaluate(() => window.__architectureExplorer.diagnostics().zoom);
    await page.locator('#zoom-in').click();
    assert.ok(await page.evaluate(() => window.__architectureExplorer.diagnostics().zoom) > zoom);
    await page.locator('#fit').click();

    for (const id of ['graph', 'sequence']) {
      await page.locator(`.tab[data-view="${id}"]`).click();
      await ready(page);
      if (id === 'sequence') {
        const fitted = await page.locator('#sequence svg').boundingBox();
        await page.locator('#zoom-in').click();
        assert.ok((await page.locator('#sequence svg').boundingBox()).width > fitted.width);
        await page.locator('#fit').click();
        await page.locator('[data-participant="client"]').click();
        assert.equal(await page.locator('#details-dialog').isVisible(), true);
        await close(page, 'details-dialog');
      }
      await page.locator('#export').click();
      await page.locator('#export-dialog[open]').waitFor();
      const png = await page.locator('#export-image').evaluate(async image => {
        await image.decode();
        const canvas = document.createElement('canvas');
        canvas.width = canvas.height = 1;
        const ctx = canvas.getContext('2d');
        ctx.drawImage(image, 0, 0);
        return { src: image.src, width: image.naturalWidth, height: image.naturalHeight, size: (await (await fetch(image.src)).blob()).size, background: [...ctx.getImageData(0, 0, 1, 1).data] };
      });
      assert.ok(png.src.startsWith('blob:'), 'both renderers must use Blob URLs');
      assert.ok(png.width > 100 && png.height > 100 && png.size > 1000);
      assert.deepEqual(png.background, [17, 24, 22, 255], 'PNG has an opaque dark background');
      assert.ok(png.width <= 4096 && png.height <= 4096 && png.width * png.height <= 8000000);
      assert.equal(await page.locator('#export-download').getAttribute('href'), png.src);
      assert.equal(await page.locator('#export-open').getAttribute('href'), png.src);
      assert.doesNotMatch(await page.locator('#status').textContent(), /exported|saved/i);
      if (!sandbox) {
        const downloadPromise = page.waitForEvent('download');
        await page.locator('#export-download').click();
        const download = await downloadPromise;
        const output = path.join(directory, `${id}.png`);
        await download.saveAs(output);
        const bytes = await fs.readFile(output);
        assert.equal(bytes.readUInt32BE(16), png.width);
        assert.equal(bytes.readUInt32BE(20), png.height);
      } else {
        await page.locator('#export-download').click();
        assert.match(await page.locator('#export-message').textContent(), /Download requested.*may block/);
        assert.equal(await page.locator('#export-image').isVisible(), true);
        assert.equal(await page.locator('#export-image').evaluate(image => image.complete && image.naturalWidth > 0), true);
      }
      await close(page, 'export-dialog');
    }
    assert.deepEqual(errors, []);
    // Engines differ in whether a blocked download produces a console event.
    // The observable contract is a decoded preview and truthful request status.
    assert.ok(warnings.every(w => /[Dd]ownload is disallowed/.test(w)), JSON.stringify(warnings));
    await context.close();
    console.log(`${engine} ${sandbox ? 'T3 sandbox' : 'local file'}: responsive layout, rotation, dialogs, zoom and both PNG exports passed`);
  }
  // Exercise the second-tap native share path, including cancellation, without an OS share sheet.
  const context = await browser.newContext();
  await context.addInitScript(() => {
    Object.defineProperty(navigator, 'canShare', { value: ({ files }) => files?.[0]?.type === 'image/png' });
    Object.defineProperty(navigator, 'share', { value: async ({ files }) => {
      window.sharedFile = { name: files[0].name, size: files[0].size, active: navigator.userActivation.isActive };
      throw new DOMException('Cancelled', 'AbortError');
    }});
  });
  const page = await context.newPage();
  await page.goto(pathToFileURL(filePath).href);
  await ready(page);
  await page.locator('#export').click();
  await page.locator('#export-share').click();
  assert.ok(await page.evaluate(() => window.sharedFile.active && window.sharedFile.size > 0));
  assert.match(await page.locator('#export-message').textContent(), /cancelled/);
  assert.equal(await page.locator('#export-image').isVisible(), true);
  await context.close();
  console.log(`${engine} share: file and user activation preserved; cancellation retains preview`);
} finally {
  await browser.close();
  await new Promise(resolve => server.close(resolve));
}
console.log(`Artifacts retained at ${directory}`);

#!/usr/bin/env node

import fs from 'node:fs/promises';
import os from 'node:os';
import path from 'node:path';
import { pathToFileURL } from 'node:url';

function usage() {
  console.error('Usage: node tools/verify-architecture-explorer.mjs --html <path> [--output-dir <path>] [--width <pixels>] [--height <pixels>] [--display-width <pixels>] [--simulate-deps-failure]');
}

function argumentsFrom(argv) {
  const values = { width: 1366, height: 768, display_width: 960 };
  for (let index = 0; index < argv.length; index += 1) {
    const argument = argv[index];
    if (['--html', '--output-dir', '--width', '--height', '--display-width'].includes(argument)) {
      const value = argv[index + 1];
      if (!value) throw new Error(`${argument} requires a value.`);
      values[argument.slice(2).replace('-', '_')] = value;
      index += 1;
    } else if (argument === '--simulate-deps-failure') {
      values.simulate_deps_failure = true;
    } else if (argument === '--help' || argument === '-h') {
      usage();
      process.exit(0);
    } else {
      throw new Error(`Unknown argument: ${argument}`);
    }
  }
  if (!values.html) throw new Error('--html is required.');
  values.width = Number(values.width);
  values.height = Number(values.height);
  values.display_width = Number(values.display_width);
  if (!Number.isInteger(values.display_width) || values.display_width < 320 || values.display_width > 3840) throw new Error('--display-width must be between 320 and 3840.');
  if (!Number.isInteger(values.width) || values.width < 640 || !Number.isInteger(values.height) || values.height < 480) {
    throw new Error('--width must be at least 640 and --height must be at least 480.');
  }
  return values;
}

async function loadPlaywright() {
  try {
    const configuredModule = process.env.ARCHITECTURE_PLAYWRIGHT_MODULE;
    const moduleSpecifier = configuredModule && path.isAbsolute(configuredModule)
      ? pathToFileURL(configuredModule).href
      : configuredModule || 'playwright';
    return await import(moduleSpecifier);
  } catch (error) {
    throw new Error(`The optional headless verifier needs the Playwright package. Install it in the calling environment, or set ARCHITECTURE_PLAYWRIGHT_MODULE to its entry module, then retry. Original error: ${error.message}`);
  }
}

function uniqueName(prefix, extension) {
  const stamp = new Date().toISOString().replace(/[-:.TZ]/g, '').slice(0, 14);
  const suffix = Math.random().toString(36).slice(2, 8);
  return `${prefix}-${stamp}-${suffix}.${extension}`;
}

async function waitForExplorer(page, viewId = null) {
  await page.waitForFunction(id => {
    const failure = document.querySelector('#failure.visible');
    const state = window.__architectureExplorer?.diagnostics?.();
    return Boolean(failure) || (state?.ready && (!id || state.activeView === id));
  }, viewId, { timeout: 30000 });
  const failure = page.locator('#failure.visible');
  if (await failure.count()) {
    const body = await page.locator('#failure-body').innerText();
    throw new Error(`Explorer failed to initialize: ${body}`);
  }
  await page.locator('#export').waitFor({ state: 'visible', timeout: 10000 });
}

async function diagnostics(page) {
  return page.evaluate(() => window.__architectureExplorer?.diagnostics?.() || { ready: false, reason: 'diagnostics unavailable' });
}

async function exportActiveView(page, outputDir, index) {
  const downloadPromise = page.waitForEvent('download', { timeout: 20000 });
  await page.locator('#export').click();
  const download = await downloadPromise;
  const target = path.join(outputDir, uniqueName(`view-${String(index + 1).padStart(2, '0')}`, 'png'));
  await download.saveAs(target);
  return target;
}

async function main() {
  let values;
  try {
    values = argumentsFrom(process.argv.slice(2));
  } catch (error) {
    usage();
    throw error;
  }

  const outputDir = values.output_dir
    ? path.resolve(values.output_dir)
    : await fs.mkdtemp(path.join(os.tmpdir(), 'architecture-explorer-verify-'));
  await fs.mkdir(outputDir, { recursive: true });
  let browser;
  let deadlineTimer;
  const consoleErrors = [];
  const pageErrors = [];

  const report = {
    html: path.resolve(values.html),
    viewport: { width: values.width, height: values.height },
    outputDir,
    views: [],
    consoleErrors,
    pageErrors,
    status: 'visual inspection pending',
    automatedChecks: 'pending',
    displayWidth: values.display_width,
    visualCriteria: 'Inspect each exact PNG at displayWidth: essential text should be at least 12 CSS pixels, no obscured labels, complete boundaries, and the original question preserved.',
    visualInspection: { status: 'pending', reason: 'Automated rendering cannot establish visual readability.' }
  };

  try {
    const { chromium } = await loadPlaywright();
    browser = await chromium.launch(process.env.ARCHITECTURE_CHROMIUM_PATH ? { executablePath: process.env.ARCHITECTURE_CHROMIUM_PATH } : undefined);
    deadlineTimer = setTimeout(() => { report.error = 'Verification exceeded the 120-second run budget.'; void browser.close(); }, 120000);
    const context = await browser.newContext({ viewport: { width: values.width, height: values.height } });
    const page = await context.newPage();
    page.on('console', message => { if (message.type() === 'error') consoleErrors.push(message.text()); });
    page.on('pageerror', error => pageErrors.push(error.message));
    const htmlUrl = new URL(pathToFileURL(path.resolve(values.html)).href);
    if (values.simulate_deps_failure) htmlUrl.searchParams.set('fail-deps', '1');
    await page.goto(htmlUrl.href, { waitUntil: 'load' });
    await waitForExplorer(page);

    const initial = await diagnostics(page);
    if (!initial.ready) throw new Error(`Explorer diagnostics are not ready: ${JSON.stringify(initial)}`);

    const direction = page.locator('#layout-direction');
    const density = page.locator('#layout-density');
    const routing = page.locator('#layout-routing');
    if (await direction.isEnabled()) {
      await direction.selectOption('DOWN');
      await waitForExplorer(page);
      await density.selectOption('SPACIOUS');
      await waitForExplorer(page);
      await routing.selectOption('STRAIGHT');
      await waitForExplorer(page);
      await page.locator('#layout').click();
      await waitForExplorer(page);
      report.controlCandidate = await diagnostics(page);
      await page.locator('#fit').click();
      await page.locator('#reset').click();
      await waitForExplorer(page);
    }

    await page.locator('#search').fill('');
    if (await page.locator('#evidence-filter').isEnabled()) await page.locator('#evidence-filter').selectOption('all');
    await page.locator('#theme').click();
    await waitForExplorer(page);

    const tabs = page.locator('.tab');
    for (let index = 0; index < await tabs.count(); index += 1) {
      const viewId = await tabs.nth(index).getAttribute('data-view');
      await tabs.nth(index).click();
      await waitForExplorer(page, viewId);
      const viewDiagnostics = await diagnostics(page);
      const png = await exportActiveView(page, outputDir, index);
      const bytes = await fs.readFile(png);
      const preview = await context.newPage();
      await preview.setViewportSize({ width: values.display_width, height: 768 });
      await preview.setContent(`<body style="margin:0"><img style="display:block;width:100%;height:auto" src="data:image/png;base64,${bytes.toString('base64')}"></body>`);
      await preview.locator('img').evaluate(img => img.decode());
      const displayPreview = path.join(outputDir, uniqueName(`display-${index + 1}`, 'png'));
      await preview.locator('img').screenshot({ path: displayPreview });
      await preview.close();
      report.views.push({ ...viewDiagnostics, png, displayPreview, pngWidth: bytes.readUInt32BE(16), pngHeight: bytes.readUInt32BE(20), visualInspection: 'pending' });
    }

    report.screenshot = path.join(outputDir, uniqueName('explorer', 'png'));
    await page.screenshot({ path: report.screenshot, fullPage: true });
    report.automatedChecks = consoleErrors.length || pageErrors.length ? 'failed' : 'passed';
    if (report.automatedChecks === 'failed') report.status = 'verification failed';
  } catch (error) {
    report.status = 'verification unavailable';
    report.error ||= error.message;
  } finally {
    clearTimeout(deadlineTimer);
    const reportPath = path.join(outputDir, uniqueName('verification', 'json'));
    await fs.writeFile(reportPath, `${JSON.stringify(report, null, 2)}\n`, 'utf8');
    await browser?.close();
    console.log(JSON.stringify({ ...report, report: reportPath }, null, 2));
  }

  if (report.status === 'verification unavailable') process.exitCode = 2;
  else if (report.status === 'verification failed') process.exitCode = 1;
}

main().catch(error => {
  console.error(error.message);
  process.exitCode = 2;
});

const { chromium } = require('playwright');
const path = require('path');
const fs = require('fs');

const DESIGNS_DIR = __dirname;

// Design configurations
const designs = [
  { name: 'design-1-command-center', html: 'design-1-command-center.html' },
  { name: 'design-2-visual-builder', html: 'design-2-visual-builder.html' },
  { name: 'design-3-project-navigator', html: 'design-3-project-navigator.html' },
  { name: 'design-4-workflow-pipeline', html: 'design-4-workflow-pipeline.html' },
  { name: 'design-5-modular-grid', html: 'design-5-modular-grid.html' },
  { name: 'design-6-ai-chat-composer', html: 'design-6-ai-chat-composer.html' },
  { name: 'design-7-repository-manager', html: 'design-7-repository-manager.html' },
  { name: 'design-8-solution-composer', html: 'design-8-solution-composer.html' },
  { name: 'design-9-source-explorer', html: 'design-9-source-explorer.html' },
  { name: 'design-10-unified-hub', html: 'design-10-unified-hub.html' },
];

// Viewport configurations
const viewports = {
  mobile: { width: 375, height: 812 },
  desktop: { width: 1440, height: 900 },
};

async function renderDesign(browser, design, viewport, suffix) {
  const htmlPath = path.join(DESIGNS_DIR, design.html);
  const pngPath = path.join(DESIGNS_DIR, `${design.name}-${suffix}.png`);

  if (!fs.existsSync(htmlPath)) {
    console.log(`Skipping ${design.html} - file not found`);
    return;
  }

  const page = await browser.newPage();
  await page.setViewportSize(viewport);

  // Load the HTML file
  const fileUrl = `file://${htmlPath}`;
  await page.goto(fileUrl, { waitUntil: 'networkidle' });

  // Wait for fonts and styles to load
  await page.waitForTimeout(500);

  // Take a full page screenshot
  await page.screenshot({
    path: pngPath,
    fullPage: true,
  });

  console.log(`Rendered: ${pngPath}`);
  await page.close();
}

async function main() {
  console.log('Starting design rendering...\n');

  const browser = await chromium.launch({
    headless: true,
  });

  for (const design of designs) {
    console.log(`Processing: ${design.name}`);

    // Render mobile version
    await renderDesign(browser, design, viewports.mobile, 'mobile');

    // Render desktop version
    await renderDesign(browser, design, viewports.desktop, 'desktop');

    console.log('');
  }

  await browser.close();
  console.log('All designs rendered successfully!');
}

main().catch(console.error);

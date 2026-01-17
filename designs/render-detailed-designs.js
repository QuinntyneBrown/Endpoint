const { chromium } = require('playwright');
const path = require('path');
const fs = require('fs');

const DESIGNS_DIR = path.join(__dirname, 'detailed-designs-solution-composer');

// Design configurations for detailed designs
const designs = [
  { name: '01-initial-empty-layout', html: '01-initial-empty-layout.html' },
  { name: '02-composing-alacarte-input', html: '02-composing-alacarte-input.html' },
  { name: '03-repository-config-management', html: '03-repository-config-management.html' },
  { name: '04-saved-configurations-list', html: '04-saved-configurations-list.html' },
  { name: '05-save-repository-configuration', html: '05-save-repository-configuration.html' },
  { name: '06-folder-configuration-drag-order', html: '06-folder-configuration-drag-order.html' },
  { name: '07-select-folder-filesystem', html: '07-select-folder-filesystem.html' },
  { name: '08-git-repo-folder-navigator', html: '08-git-repo-folder-navigator.html' },
  { name: '09-alacarte-output-preview', html: '09-alacarte-output-preview.html' },
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
  console.log('Starting detailed design rendering...\n');

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
  console.log('All detailed designs rendered successfully!');
}

main().catch(console.error);

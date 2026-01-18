const { chromium } = require('playwright');
const path = require('path');
const fs = require('fs');

/**
 * Generate screenshots from HTML mockup files
 * This script takes screenshots at different viewport sizes to match the design system
 */
async function captureScreenshots() {
  console.log('Starting screenshot generation...');
  
  const browser = await chromium.launch({
    headless: true,
  });
  
  // HTML files to capture
  const files = [
    '00-style-guide.html',
    '01-global-header.html',
    '02-rail.html',
    '03-alacarte-wizard.html',
    '04-full-layout.html'
  ];

  // Viewport configurations matching the design system
  const viewports = [
    { name: 'desktop', width: 1440, height: 900 },
    { name: 'mobile', width: 375, height: 812 }
  ];

  const designsDir = __dirname;
  
  for (const file of files) {
    const baseName = file.replace('.html', '');
    console.log(`\nProcessing ${file}...`);
    
    for (const viewport of viewports) {
      console.log(`  - Capturing ${viewport.name} view (${viewport.width}x${viewport.height})`);
      
      // Create a new page with the viewport size
      const page = await browser.newPage({
        viewport: { 
          width: viewport.width, 
          height: viewport.height 
        }
      });
      
      // Navigate to the HTML file
      const filePath = path.join(designsDir, file);
      await page.goto(`file://${filePath}`, {
        waitUntil: 'networkidle'
      });
      
      // Wait a bit for any animations or dynamic content
      await page.waitForTimeout(500);
      
      // Take full page screenshot
      const outputPath = path.join(designsDir, `${baseName}-${viewport.name}.png`);
      await page.screenshot({
        path: outputPath,
        fullPage: true
      });
      
      console.log(`    ✓ Saved to ${baseName}-${viewport.name}.png`);
      
      // Close the page
      await page.close();
    }
  }

  await browser.close();
  console.log('\n✓ All screenshots generated successfully!');
}

// Run the script
captureScreenshots().catch(error => {
  console.error('Error generating screenshots:', error);
  process.exit(1);
});

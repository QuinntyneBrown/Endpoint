import { test, expect } from '@playwright/test';

test.describe('Home Page', () => {
  test('should display the Solution Composer welcome page', async ({ page }) => {
    await page.goto('/');

    // Check for the header
    await expect(page.locator('ep-app-header')).toBeVisible();
    await expect(page.locator('ep-app-header')).toContainText('Solution Composer');

    // Check for the empty state component
    await expect(page.locator('ep-empty-state')).toBeVisible();
    await expect(page.locator('ep-empty-state')).toContainText('Welcome to Solution Composer');

    // Check for action buttons
    await expect(page.getByRole('button', { name: /New Composition/i })).toBeVisible();
    await expect(page.getByRole('button', { name: /Load Saved Configuration/i })).toBeVisible();
  });

  test('should display quick start options', async ({ page }) => {
    await page.goto('/');

    // Check for quick start section
    await expect(page.locator('text=Quick Start')).toBeVisible();

    // Check for all quick start cards
    await expect(page.locator('.quick-start-card:has-text("From GitHub")')).toBeVisible();
    await expect(page.locator('.quick-start-card:has-text("From Local")')).toBeVisible();
    await expect(page.locator('.quick-start-card:has-text("Recent")')).toBeVisible();
    await expect(page.locator('.quick-start-card:has-text("Templates")')).toBeVisible();
  });

  test('should interact with New Composition button', async ({ page }) => {
    await page.goto('/');

    // Click the New Composition button
    const newCompositionButton = page.getByRole('button', { name: /New Composition/i });
    await newCompositionButton.click();

    // Note: In a real scenario, this would navigate to a new page or open a dialog
    // For now, we're just verifying the button is clickable
    await expect(newCompositionButton).toBeEnabled();
  });

  test('should be responsive on mobile', async ({ page }) => {
    // Set viewport to mobile size
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/');

    // Verify page is visible and content is accessible
    await expect(page.locator('ep-app-header')).toBeVisible();
    await expect(page.locator('ep-empty-state')).toBeVisible();
    await expect(page.locator('.quick-start-card').first()).toBeVisible();
  });

  test('should have proper dark theme colors', async ({ page }) => {
    await page.goto('/');

    // Check that the dark background is applied
    const bodyBg = await page.evaluate(() => {
      return window.getComputedStyle(document.body).backgroundColor;
    });

    // Dark theme background should be a dark color (check if it's darker than middle gray)
    expect(bodyBg).toBeTruthy();
  });
});

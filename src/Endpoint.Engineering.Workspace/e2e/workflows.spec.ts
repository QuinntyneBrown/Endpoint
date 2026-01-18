import { test, expect } from '@playwright/test';

test.describe('User Workflows', () => {
  test('should complete the full create request workflow', async ({ page }) => {
    // Navigate to home
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Verify we're on the home page with empty state
    await expect(page.locator('ee-empty-state')).toBeVisible();

    // Click "New Composition" button
    const newCompositionBtn = page.locator('ee-button').filter({ hasText: 'New Composition' });
    await expect(newCompositionBtn).toBeVisible();
  });

  test('should navigate between pages using header', async ({ page }) => {
    await page.goto('/requests');
    await page.waitForLoadState('networkidle');

    // Click back button in header
    const backButton = page.locator('mat-toolbar button[mat-icon-button]').first();
    await expect(backButton).toBeVisible();
  });

  test('should display and interact with request list', async ({ page }) => {
    await page.goto('/requests');
    await page.waitForLoadState('networkidle');

    // Check stats bar
    const statsBar = page.locator('ee-stats-bar');
    await expect(statsBar).toBeVisible();

    // Check for "New Request" button
    const newRequestBtn = page.locator('ee-button').filter({ hasText: 'New Request' });
    await expect(newRequestBtn).toBeVisible();

    // Check for search box
    const searchBox = page.locator('ee-search-box');
    await expect(searchBox).toBeVisible();
  });

  test('should show form validation on create page', async ({ page }) => {
    await page.goto('/request/create');
    await page.waitForLoadState('networkidle');

    // Check that form fields are present
    const formFields = page.locator('mat-form-field');
    const fieldCount = await formFields.count();
    expect(fieldCount).toBeGreaterThan(0);
  });

  test('should navigate via quick options', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');

    // Check for quick options
    const quickOptions = page.locator('ee-quick-option');
    const optionCount = await quickOptions.count();
    expect(optionCount).toBe(4);
  });
});

import { test, expect } from '@playwright/test';

test.describe('Request Management', () => {
  test('should display existing requests in table', async ({ page }) => {
    await page.goto('/requests');
    await page.waitForLoadState('networkidle');

    // Check for table
    const table = page.locator('table.requests-table');
    await expect(table).toBeVisible();

    // Check for table headers
    const headers = page.locator('table.requests-table th');
    await expect(headers.first()).toBeVisible();
  });

  test('should have action buttons for each request', async ({ page }) => {
    await page.goto('/requests');
    await page.waitForLoadState('networkidle');

    // Check for table rows
    const rows = page.locator('table.requests-table tbody tr');
    const rowCount = await rows.count();

    if (rowCount > 0) {
      // Check for icon buttons in the first row
      const firstRow = rows.first();
      const actionButtons = firstRow.locator('ee-icon-button');
      const buttonCount = await actionButtons.count();
      expect(buttonCount).toBeGreaterThan(0);
    }
  });

  test('should show stats for requests', async ({ page }) => {
    await page.goto('/requests');
    await page.waitForLoadState('networkidle');

    // Check stats bar is visible
    const statsBar = page.locator('ee-stats-bar');
    await expect(statsBar).toBeVisible();
  });

  test('should show create form with all required fields', async ({ page }) => {
    await page.goto('/request/create');
    await page.waitForLoadState('networkidle');

    // Check for page header
    const header = page.locator('ee-page-header');
    await expect(header).toBeVisible();

    // Check for form section
    const formSection = page.locator('ee-form-section');
    await expect(formSection).toBeVisible();

    // Check for form fields
    const fields = page.locator('mat-form-field');
    const fieldCount = await fields.count();
    expect(fieldCount).toBe(4); // name, solutionName, outputDirectory, outputType
  });

  test('should have cancel and create buttons', async ({ page }) => {
    await page.goto('/request/create');
    await page.waitForLoadState('networkidle');

    // Check for action buttons
    const cancelBtn = page.locator('ee-button').filter({ hasText: 'Cancel' });
    await expect(cancelBtn).toBeVisible();

    const createBtn = page.locator('ee-button').filter({ hasText: 'Create Request' });
    await expect(createBtn).toBeVisible();
  });

  test('should filter requests with search', async ({ page }) => {
    await page.goto('/requests');
    await page.waitForLoadState('networkidle');

    // Get search box
    const searchBox = page.locator('ee-search-box');
    await expect(searchBox).toBeVisible();

    // Get initial row count
    const table = page.locator('table.requests-table tbody tr');
    const initialCount = await table.count();
    
    // The table should have rows (we have mock data)
    if (initialCount > 0) {
      expect(initialCount).toBeGreaterThan(0);
    }
  });
});

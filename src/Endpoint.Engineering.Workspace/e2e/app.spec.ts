import { test, expect } from '@playwright/test';

test.describe('Endpoint Engineering App', () => {
  test('should load the application', async ({ page }) => {
    await page.goto('/');
    await expect(page).toHaveTitle(/EndpointEngineering/);
  });

  test('should display the main content', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('app-root')).toBeVisible();
  });
});

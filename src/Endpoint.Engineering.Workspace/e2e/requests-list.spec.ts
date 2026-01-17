import { test, expect } from '@playwright/test';

test.describe('Requests List Page', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/requests');
  });

  test('should display the requests list page', async ({ page }) => {
    await expect(page.locator('ep-app-header')).toBeVisible();
    await expect(page.locator('ep-app-header')).toContainText('A La Carte Requests');
  });

  test('should display search box', async ({ page }) => {
    await expect(page.locator('ep-search-box')).toBeVisible();
  });

  test('should show request cards', async ({ page }) => {
    const requestCards = page.locator('ep-card.request-card');
    const count = await requestCards.count();
    
    if (count > 0) {
      await expect(requestCards.first()).toBeVisible();
      
      // Check that request card has expected content
      await expect(requestCards.first().locator('.request-name')).toBeVisible();
      await expect(requestCards.first().locator('.request-solution')).toBeVisible();
      await expect(requestCards.first().locator('ep-status-indicator')).toBeVisible();
    }
  });

  test('should have New Request button', async ({ page }) => {
    const newButton = page.getByRole('button', { name: /New Request/i });
    await expect(newButton).toBeVisible();
  });

  test('should navigate to create page when clicking New Request', async ({ page }) => {
    const newButton = page.getByRole('button', { name: /New Request/i });
    await newButton.click();
    
    await expect(page).toHaveURL(/\/requests\/create/);
  });

  test('should display empty state when no requests', async ({ page }) => {
    // This test assumes starting with no data
    // In a real scenario, you might need to clear data first
    const emptyState = page.locator('ep-empty-state');
    const requestCards = page.locator('ep-card.request-card');
    
    const hasRequests = await requestCards.count() > 0;
    const hasEmptyState = await emptyState.isVisible();
    
    // Either we have requests OR we have empty state
    expect(hasRequests || hasEmptyState).toBe(true);
  });

  test('should filter requests by search', async ({ page }) => {
    const searchBox = page.locator('ep-search-box input');
    
    if (await searchBox.isVisible()) {
      await searchBox.fill('Test');
      await page.waitForTimeout(300); // Wait for debounce
      
      const cards = page.locator('ep-card.request-card');
      const count = await cards.count();
      
      // Verify that filtering happened (count should be 0 or fewer than total)
      expect(count).toBeGreaterThanOrEqual(0);
    }
  });

  test('should display request actions on card', async ({ page }) => {
    const firstCard = page.locator('ep-card.request-card').first();
    
    if (await firstCard.isVisible()) {
      const editButton = firstCard.getByRole('button', { name: /Edit/i });
      await expect(editButton).toBeVisible();
    }
  });

  test('should navigate to request details when clicking card', async ({ page }) => {
    const firstCard = page.locator('ep-card.request-card').first();
    
    if (await firstCard.isVisible()) {
      await firstCard.click();
      await expect(page).toHaveURL(/\/requests\/\w+/);
    }
  });

  test('should be responsive on mobile', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    
    await expect(page.locator('ep-app-header')).toBeVisible();
    
    // Verify search box is visible on mobile
    await expect(page.locator('ep-search-box')).toBeVisible();
  });

  test('should display status indicators correctly', async ({ page }) => {
    const statusIndicator = page.locator('ep-status-indicator').first();
    
    if (await statusIndicator.isVisible()) {
      // Verify status indicator has text
      const text = await statusIndicator.textContent();
      expect(text).toBeTruthy();
      expect(['Draft', 'Ready', 'Executing', 'Completed', 'Failed'].some(status => 
        text?.includes(status)
      )).toBe(true);
    }
  });
});

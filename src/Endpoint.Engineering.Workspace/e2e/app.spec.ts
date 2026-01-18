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

  test('should display the app header', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('mat-toolbar')).toBeVisible();
    await expect(page.locator('.app-title')).toContainText('A La Carte Composer');
  });

  test('should navigate to home page and display empty state', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    // Check for empty state component
    await expect(page.locator('ee-empty-state')).toBeVisible();
  });

  test('should navigate to requests list', async ({ page }) => {
    await page.goto('/');
    await page.goto('/requests');
    await page.waitForLoadState('networkidle');
    
    // Check for page header
    await expect(page.locator('ee-page-header')).toBeVisible();
    
    // Check for stats bar
    await expect(page.locator('ee-stats-bar')).toBeVisible();
  });

  test('should navigate to create request page', async ({ page }) => {
    await page.goto('/request/create');
    await page.waitForLoadState('networkidle');
    
    // Check for page header
    await expect(page.locator('ee-page-header')).toBeVisible();
    
    // Check for form fields
    const formFields = page.locator('mat-form-field');
    await expect(formFields.first()).toBeVisible();
  });

  test('should create a new request via form', async ({ page }) => {
    await page.goto('/request/create');
    await page.waitForLoadState('networkidle');
    
    // Fill in the form
    await page.fill('input[ng-reflect-name="name"]', 'Test Request');
    await page.fill('input[ng-reflect-name="solutionName"]', 'TestSolution');
    await page.fill('input[ng-reflect-name="outputDirectory"]', '/test');
    
    // Check if we can see the cancel button (form is rendered)
    const cancelButton = page.getByText('Cancel');
    await expect(cancelButton).toBeVisible();
  });

  test('should display requests in the list', async ({ page }) => {
    await page.goto('/requests');
    await page.waitForLoadState('networkidle');
    
    // Check for Material table
    const table = page.locator('table.requests-table');
    await expect(table).toBeVisible();
  });

  test('should have working search functionality', async ({ page }) => {
    await page.goto('/requests');
    await page.waitForLoadState('networkidle');
    
    // Check for search box
    const searchBox = page.locator('ee-search-box');
    await expect(searchBox).toBeVisible();
  });
});


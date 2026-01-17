import { test, expect } from '@playwright/test';

test.describe('Composition Wizard', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/compose');
  });

  test('should display wizard with steps', async ({ page }) => {
    await expect(page.locator('ep-app-header')).toContainText('New A La Carte Composition');
    
    // Verify wizard steps component is visible
    await expect(page.locator('ep-wizard-steps')).toBeVisible();
  });

  test('should show step 1 (Basic Info) by default', async ({ page }) => {
    await expect(page.locator('text=Project Information')).toBeVisible();
    await expect(page.locator('text=Target Framework')).toBeVisible();
    await expect(page.locator('text=Generation Options')).toBeVisible();
  });

  test('should have required form fields on step 1', async ({ page }) => {
    await expect(page.locator('ep-form-input:has-text("Solution Name")')).toBeVisible();
    await expect(page.locator('ep-form-input:has-text("Output Directory")')).toBeVisible();
  });

  test('should have framework radio buttons', async ({ page }) => {
    await expect(page.locator('.radio-item:has-text(".NET 8")')).toBeVisible();
    await expect(page.locator('.radio-item:has-text(".NET 7")')).toBeVisible();
    await expect(page.locator('.radio-item:has-text(".NET 6")')).toBeVisible();
  });

  test('should have generation option checkboxes', async ({ page }) => {
    await expect(page.locator('.checkbox-item:has-text("Include Unit Tests")')).toBeVisible();
    await expect(page.locator('.checkbox-item:has-text("Generate Documentation")')).toBeVisible();
    await expect(page.locator('.checkbox-item:has-text("Add Docker Support")')).toBeVisible();
    await expect(page.locator('.checkbox-item:has-text("CI/CD Configuration")')).toBeVisible();
  });

  test('should be able to fill out basic info', async ({ page }) => {
    const solutionNameInput = page.locator('ep-form-input:has-text("Solution Name") input');
    const outputDirInput = page.locator('ep-form-input:has-text("Output Directory") input');
    
    await solutionNameInput.fill('MyTestSolution');
    await outputDirInput.fill('/test/output');
    
    await expect(solutionNameInput).toHaveValue('MyTestSolution');
    await expect(outputDirInput).toHaveValue('/test/output');
  });

  test('should be able to select framework', async ({ page }) => {
    const net7Option = page.locator('.radio-item:has-text(".NET 7")');
    await net7Option.click();
    
    await expect(net7Option).toHaveClass(/selected/);
  });

  test('should be able to toggle checkboxes', async ({ page }) => {
    const dockerCheckbox = page.locator('.checkbox-item:has-text("Add Docker Support")');
    const initialState = await dockerCheckbox.getAttribute('class');
    
    await dockerCheckbox.click();
    
    const newState = await dockerCheckbox.getAttribute('class');
    expect(initialState).not.toBe(newState);
  });

  test('should have Next button', async ({ page }) => {
    const nextButton = page.getByRole('button', { name: /Next/i });
    await expect(nextButton).toBeVisible();
  });

  test('should have Cancel button', async ({ page }) => {
    const cancelButton = page.getByRole('button', { name: /Cancel/i });
    await expect(cancelButton).toBeVisible();
  });

  test('should navigate to next step when Next is clicked', async ({ page }) => {
    // Fill required fields first
    await page.locator('ep-form-input:has-text("Solution Name") input').fill('TestSolution');
    await page.locator('ep-form-input:has-text("Output Directory") input').fill('/test');
    
    const nextButton = page.getByRole('button', { name: /Next/i });
    await nextButton.click();
    
    // Should show step 2 content
    await expect(page.locator('text=Select Components')).toBeVisible();
  });

  test('should navigate back when Previous is clicked', async ({ page }) => {
    // First go to step 2
    await page.locator('ep-form-input:has-text("Solution Name") input').fill('TestSolution');
    await page.locator('ep-form-input:has-text("Output Directory") input').fill('/test');
    await page.getByRole('button', { name: /Next/i }).click();
    
    // Then click Previous
    const prevButton = page.getByRole('button', { name: /Previous/i });
    await prevButton.click();
    
    // Should be back on step 1
    await expect(page.locator('text=Project Information')).toBeVisible();
  });

  test('should navigate to home when Cancel is clicked', async ({ page }) => {
    const cancelButton = page.getByRole('button', { name: /Cancel/i });
    await cancelButton.click();
    
    await expect(page).toHaveURL('/');
  });

  test('should be responsive on mobile', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    
    await expect(page.locator('ep-app-header')).toBeVisible();
    await expect(page.locator('ep-wizard-steps')).toBeVisible();
    await expect(page.locator('text=Project Information')).toBeVisible();
  });
});

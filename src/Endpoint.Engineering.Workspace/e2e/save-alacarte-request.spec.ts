import { test, expect } from '@playwright/test';

test.describe('Save ALaCarteRequest via Wizard', () => {
  test('should complete wizard and save a new ALaCarteRequest', async ({ page }) => {
    // Navigate to create request page
    await page.goto('/request/create');
    await page.waitForLoadState('networkidle');

    // Step 1: Basic Info
    console.log('Step 1: Filling basic info...');

    // Fill in the name field
    const nameInput = page.locator('mat-form-field').filter({ hasText: 'Request Name' }).locator('input');
    await nameInput.fill('Test ALaCarte Request');

    // Fill in solution name
    const solutionInput = page.locator('mat-form-field').filter({ hasText: 'Solution Name' }).locator('input');
    await solutionInput.fill('TestSolution.sln');

    // Fill in output directory
    const outputDirInput = page.locator('mat-form-field').filter({ hasText: 'Output Directory' }).locator('input');
    await outputDirInput.fill('C:/output/test-project');

    // Output type defaults to .NET Solution which is fine

    // Click Next
    const nextButton = page.locator('ee-button').filter({ hasText: 'Next' });
    await nextButton.click();
    await page.waitForTimeout(500);

    // Step 2: Add Repository
    console.log('Step 2: Adding repository...');

    // Click Add Git Repository button
    const addGitRepoButton = page.locator('button.add-button').filter({ hasText: 'Add Git Repository' });
    await addGitRepoButton.click();
    await page.waitForTimeout(300);

    // Fill in repository URL
    const repoUrlInput = page.locator('ee-repository-card input[placeholder*="url" i], ee-repository-card mat-form-field').first().locator('input');
    await repoUrlInput.fill('https://github.com/example/test-repo');

    // Branch defaults to 'main' which is fine

    // Click Next
    await nextButton.click();
    await page.waitForTimeout(500);

    // Step 3: Add Folder Mapping
    console.log('Step 3: Adding folder mappings...');

    // Click Add Folder Mapping button
    const addFolderButton = page.locator('ee-folder-mapping-list .add-button');
    await addFolderButton.click();
    await page.waitForTimeout(500);

    // Click the edit button on the new folder to enter edit mode
    const editButton = page.locator('ee-folder-mapping-list .folder-item__action-btn').first();
    await editButton.click();
    await page.waitForTimeout(300);

    // Fill in source path
    const sourceInput = page.locator('ee-folder-mapping-list mat-form-field').filter({ hasText: 'Source Path' }).locator('input');
    await sourceInput.fill('src/Core');

    // Fill in destination path
    const destInput = page.locator('ee-folder-mapping-list mat-form-field').filter({ hasText: 'Destination Path' }).locator('input');
    await destInput.fill('src/Core');

    // Click done button to finish editing
    const doneButton = page.locator('ee-folder-mapping-list .folder-item__done-btn');
    await doneButton.click();
    await page.waitForTimeout(300);

    // Click Next
    await nextButton.click();
    await page.waitForTimeout(500);

    // Step 4: Review and Create
    console.log('Step 4: Reviewing and creating request...');

    // Verify we're on the review step
    const summaryCard = page.locator('.summary-card');
    await expect(summaryCard).toBeVisible();

    // Click Create Request
    const createButton = page.locator('ee-button').filter({ hasText: 'Create Request' });
    await createButton.click();

    // Wait for navigation to requests list
    await page.waitForURL('**/requests**', { timeout: 10000 });

    console.log('ALaCarteRequest saved successfully!');

    // Verify we're on the requests page
    await expect(page).toHaveURL(/.*requests/);
  });
});

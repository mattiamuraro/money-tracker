import { expect, test } from '@playwright/test';

test.describe('Auth and navigation smoke', () => {
  test('renders login form', async ({ page }) => {
    await page.goto('/login');

    await expect(page.getByRole('heading', { name: 'Sign In' })).toBeVisible();
    await expect(page.getByLabel('Username')).toBeVisible();
    await expect(page.getByLabel('Password')).toBeVisible();
  });

  test('handles optional registration mode toggle', async ({ page }) => {
    await page.goto('/login');

    const createAccountToggle = page.getByRole('button', { name: 'Create Account' });
    if (await createAccountToggle.count()) {
      await createAccountToggle.click();
      await expect(page.getByRole('heading', { name: 'Create Account' })).toBeVisible();
      await expect(page.getByLabel('Confirm Password')).toBeVisible();
      return;
    }

    await expect(page.getByRole('heading', { name: 'Sign In' })).toBeVisible();
    await expect(createAccountToggle).toHaveCount(0);
  });
});

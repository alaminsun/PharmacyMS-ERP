import { test, expect } from '@playwright/test';
const email=process.env.PHARMACY_E2E_EMAIL;
const password=process.env.PHARMACY_E2E_PASSWORD;
// Only mutate when user explicitly opts in. This suite must run against a TEST database.
const mutating=process.env.PHARMACY_MUTATING_E2E==='1';
test.describe('PharmacyMS end-to-end',()=>{
  test.beforeEach(async ({page})=>{
    test.skip(!email||!password,'Set PHARMACY_E2E_EMAIL and PHARMACY_E2E_PASSWORD.');
    await page.goto('/login');
    await page.getByLabel('Email address').fill(email!);
    await page.getByLabel('Password').fill(password!);
    await page.getByRole('button',{name:/Sign in/}).click();
    await expect(page.getByText(/Signed in successfully/)).toBeVisible();
  });
  test('signs in and displays permitted navigation',async ({page})=>{
    await expect(page.locator('.workspace')).toBeVisible();
    await expect(page.getByRole('navigation',{name:'Main navigation'})).toBeVisible();
  });
  test('sale form allocates available medicine stock using FEFO (test DB only)',async ({page})=>{
    test.skip(!mutating,'Opt in with PHARMACY_MUTATING_E2E=1, on a TEST database only.');
    await page.goto('/sales');
    await page.getByRole('button',{name:/New sale/}).click();
    const modal=page.locator('dialog[open]');
    await expect(modal).toBeVisible();
    const select=modal.locator('select[name="medicine-sale-0"]');
    const options=select.locator('option');
    test.skip(await options.count()<2,'Test database must have available, unexpired stock.');
    await select.selectOption({index:1});
    await expect(modal.getByText(/FEFO:/)).toBeVisible();
    await modal.getByRole('button',{name:'Complete sale'}).click();
    await expect(page.getByText(/recorded successfully/)).toBeVisible();
  });
});

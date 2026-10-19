import { defineConfig, devices } from '@playwright/test';
export default defineConfig({
  testDir:'./e2e',
  timeout:45000,
  expect:{timeout:10000},
  retries:0,
  workers:1,
  use:{baseURL:process.env.PHARMACY_UI_URL ?? 'http://127.0.0.1:4200',trace:'retain-on-failure',screenshot:'only-on-failure'},
  projects:[{name:'chromium',use:{...devices['Desktop Chrome']}}]
});

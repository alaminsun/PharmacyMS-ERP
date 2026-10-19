import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
const source = relative => readFileSync(resolve(import.meta.dirname, '..', relative), 'utf8');

test('authentication uses real ASP.NET Core JWT contract, not nonexistent CSRF cookie endpoints', () => {
  const auth=source('src/app/core/auth.service.ts');
  const interceptor=source('src/app/core/auth.interceptor.ts');
  assert.match(auth,/\/auth\/login/);
  assert.match(auth,/\/auth\/me\/permissions/);
  assert.match(interceptor,/Authorization: `Bearer \$\{token\}`/);
  assert.doesNotMatch(auth + interceptor,/\/auth\/csrf|\/auth\/logout/);
});
test('admin commands map to actual backend controllers', () => {
  const users=source('src/app/pages/users.component.ts');
  const roles=source('src/app/pages/roles.component.ts');
  for(const path of ['/admin/users','/admin/users/assign-role','/admin/users/remove-role','/admin/users/status'])
    assert.ok(users.includes(path),`Missing ${path}`);
  for(const path of ['/admin/roles','/admin/roles/permissions','/admin/roles/grant-permission','/admin/roles/revoke-permission'])
    assert.ok(roles.includes(path),`Missing ${path}`);
});
test('reports and stock use real DTOs and API routes', () => {
  const report=source('src/app/pages/reports.component.ts');
  const dashboard=source('src/app/pages/dashboard.component.ts');
  assert.ok(report.includes('fromUtc')&&report.includes('toUtc')&&report.includes('/reports/sales-details'));
  assert.ok(dashboard.includes('totalSales')&&dashboard.includes('lowStockMedicines')&&dashboard.includes('/stock/low-stock'));
});
test('permission guard never trusts role names alone', () => {
  const guard=source('src/app/app.routes.ts');
  const auth=source('src/app/core/auth.service.ts');
  assert.ok(guard.includes("${meta.permission}.Read"));
  assert.match(auth,/\.permissions\.includes\(permission\)/);
});
test('transaction input does not transmit client-supplied sale prices', () => {
  const trans=source('src/app/pages/transactions.component.ts');
  assert.match(trans,/allocateFefo\(this\.batches,Number\(line\['medicineId'\]\),Number\(line\['quantity'\]\)\)/);
  assert.doesNotMatch(trans,/unitPrice:this\.sellingPrice\(line\)/);
});

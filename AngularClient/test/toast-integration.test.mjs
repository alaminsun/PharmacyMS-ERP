import test from 'node:test';
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
const source = relative => readFileSync(resolve(import.meta.dirname, '..', relative), 'utf8');

test('the notification outlet is mounted globally and has dismiss control', () => {
  const app = source('src/app/app.component.ts');
  const outlet = source('src/app/shared/toast-outlet.component.ts');
  assert.match(app, /<app-toast-outlet\/>/);
  assert.match(outlet, /notifications\.dismiss\(toast\.id\)/);
  assert.match(outlet, /toast\.kind === 'error' \? 'alert' : 'status'/);
  assert.match(outlet, /@for \(toast of notifications\.toasts\(\); track toast\.id\)/);
});
test('API failures produce global error notifications with the shared error formatter', () => {
  const main = source('src/main.ts');
  const interceptor = source('src/app/core/toast.interceptor.ts');
  assert.match(main, /withInterceptors\(\[authInterceptor, loadingInterceptor, toastErrorInterceptor\]\)/);
  assert.match(interceptor, /toast\.error\(errorMessage\(error\)\)/);
  assert.match(interceptor, /error instanceof HttpErrorResponse/);
});
test('notification service limits the stack, allows dismissal and auto-removes messages', () => {
  const service = source('src/app/core/toast.service.ts');
  assert.match(service, /current\.length >= 4/);
  assert.match(service, /setTimeout\(\(\) => this\.dismiss\(id\), durationMs\)/);
  assert.match(service, /clearTimeout\(timer\)/);
  assert.match(service, /kind === 'error' && this\.items\(\)\.some\(item => item\.kind === kind && item\.message === value\)/);
});
test('successful mutations notify the user across all major modules', () => {
  const sources = [
    'src/app/pages/login.component.ts',
    'src/app/pages/catalog.component.ts',
    'src/app/pages/transactions.component.ts',
    'src/app/pages/users.component.ts',
    'src/app/pages/roles.component.ts',
    'src/app/pages/reports.component.ts'
  ];
  for (const file of sources) assert.match(source(file), /this\.toast\.success\(/, `${file} missing success toast`);
  const catalog = source(sources[1]);
  assert.match(catalog, /this\.toast\.success\(`\$\{this\.meta\.title\} \$\{this\.meta\.key===/); // delete / deactivate
  const users = source(sources[3]);
  for (const key of ['/assign-role', '/remove-role', '/status']) assert.ok(users.includes(key));
});
test('loading on navigation is silent; manual refreshes show explicit informational notifications', () => {
  for(const file of ['dashboard','catalog','transactions','stock']){
    const content = source(`src/app/pages/${file}.component.ts`);
    assert.match(content, /async load\(notify=false\)/, `${file} needs silent initial load`);
    assert.match(content, /\(click\)="load\(true\)"/, `${file} needs explicit refresh`);
  }
});

import test from 'node:test';
import assert from 'node:assert/strict';
import { isoStart, isoEndExclusive, safeCsv, money, todayLocal } from '../src/app/shared/helpers.ts';

test('report ToUtc is exclusive but covers the selected last day', () => {
  assert.equal(isoStart('2026-10-01'), '2026-10-01T00:00:00Z');
  assert.equal(isoEndExclusive('2026-10-18'), '2026-10-19T00:00:00.000Z');
  assert.equal(isoEndExclusive('2026-12-31'), '2027-01-01T00:00:00.000Z');
});
test('CSV protects formula-like cells and escapes quotes and commas', () => {
  const csv = safeCsv([{ name: '=SUM(A1:A2)', amount: 200, note: 'One, "two"' }], ['name','amount','note']);
  assert.ok(csv.startsWith('\uFEFF'));
  assert.ok(csv.includes('"\'=SUM(A1:A2)"'));
  assert.ok(csv.includes('"One, ""two"""'));
});
test('money is formatted to two decimal places', () => {
  assert.match(money(200), /200[.,]00/);
});
test('local today follows ISO date syntax', () => {
  assert.match(todayLocal(), /^\d{4}-\d{2}-\d{2}$/);
});

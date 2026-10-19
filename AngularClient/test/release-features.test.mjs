import test from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
import {resolve} from 'node:path';
import {allocateFefo} from '../src/app/shared/fefo.ts';
const file=p=>readFileSync(resolve(import.meta.dirname,'..',p),'utf8');
const batch=(stockBatchId,medicineId,expiryDate,quantityOnHand,isExpired=false)=>({stockBatchId,medicineId,expiryDate,quantityOnHand,isExpired,medicineName:'Test',batchNumber:'B'+stockBatchId,medicineCode:'M'});
test('FEFO distributes quantity across earliest non-expired stock batches',()=>{
  const batches=[batch(3,1,'2028-05-01',10),batch(2,1,'2027-12-01',3),batch(5,1,'2027-01-01',6,true),batch(4,2,'2027-01-01',5)];
  assert.deepEqual(allocateFefo(batches,1,7),[{stockBatchId:2,quantity:3},{stockBatchId:3,quantity:4}]);
  assert.equal(batches[0].quantityOnHand,10,'allocation must not mutate inventory');
});
test('FEFO rejects zero, fractional, and insufficient quantities',()=>{
  const stock=[batch(1,1,'2028-03-01',3)];
  assert.throws(()=>allocateFefo(stock,1,0));
  assert.throws(()=>allocateFefo(stock,1,1.5));
  assert.throws(()=>allocateFefo(stock,1,5),/Not enough stock/);
});
test('catalog and transaction pages request server pagination',()=>{
  for(const component of ['catalog','transactions']){
    const s=file(`src/app/pages/${component}.component.ts`);
    assert.match(s,/pageSize/);assert.match(s,/sortBy/);assert.match(s,/totalCount/);
  }
  assert.match(file('src/app/pages/catalog.component.ts'),/\/catalog-pages\//);
  assert.match(file('src/app/pages/transactions.component.ts'),/\/transaction-pages\//);
});
test('confirm dialog is async and never calls window.confirm',()=>{
  const content=file('src/app/pages/catalog.component.ts')+file('src/app/pages/users.component.ts');
  assert.doesNotMatch(content,/window\.confirm/);
  assert.match(content,/this\.confirm\.ask/);
  assert.match(file('src/app/app.component.ts'),/app-confirm-outlet/);
});
test('receipts are backend-sourced, printable and use stored line prices',()=>{
  const component=file('src/app/pages/receipt.component.ts');
  assert.match(component,/\/receipts\//);assert.match(component,/window\.print\(\)/);
  assert.match(component,/item\.unitPrice/);assert.match(component,/item\.lineTotal/);
});
test('audit page, expiry alerts and progress indicator are connected',()=>{
  assert.match(file('src/app/app.routes.ts'),/path:'audit'/);
  assert.match(file('src/app/pages/stock.component.ts'),/expiryAlerts\(\)/);
  assert.match(file('src/main.ts'),/loadingInterceptor/);
});
test('backend permissions and business endpoints are distinct',()=>{
  const backend=p=>readFileSync(resolve(import.meta.dirname,'../../Backend',p),'utf8');
  assert.match(backend('PharmacyMS.API/Controllers/PagedController.cs'),/Skip\(\(page-1\)\*pageSize\)\.Take\(pageSize\)/);
  assert.match(backend('PharmacyMS.API/Controllers/AuditController.cs'),/HasPermission\(Permissions\.UsersManage\)/);
  assert.match(backend('PharmacyMS.API/Controllers/ReceiptsController.cs'),/AuthorizeAsync/);
  assert.match(backend('PharmacyMS.API/Auditing/AuditTrailMiddleware.cs'),/context\.Response\.StatusCode<200/);
});

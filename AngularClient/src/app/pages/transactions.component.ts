import { Component, ElementRef, ViewChild, OnInit, inject } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ApiService, errorMessage } from '../core/api.service';
import { AuthService } from '../core/auth.service';
import { Medicine, StockBatch, Transaction, PurchaseLineInput, SaleLineInput, PageResult, Item } from '../core/models';
import { money, todayLocal } from '../shared/helpers';
import { ToastService } from '../core/toast.service';
import { allocateFefo } from '../shared/fefo';
import { RouterLink } from '@angular/router';
@Component({selector:'app-transactions',standalone:true,imports:[FormsModule, DecimalPipe, RouterLink],template:`
<div class="page-heading"><div><p class="eyebrow">{{isPurchase?'SUPPLY MANAGEMENT':'CHECKOUT & BILLING'}}</p><h1>{{isPurchase?'Purchases':'Sales'}}</h1><p class="muted">{{isPurchase?'Record supplier deliveries and increase inventory.':'Record sales and deduct the correct stock batches.'}}</p></div>
@if(canWrite()){<button class="button" (click)="openNew()">+ {{isPurchase?'Record purchase':'New sale'}}</button>}</div>
@if(error){<p class="form-error" role="alert">{{error}}</p>}
<section class="panel"><div class="panel-heading"><div><h2>Recorded {{isPurchase?'purchases':'sales'}}</h2><p class="muted">Transactions are posted by the server.</p></div><button class="button small secondary" (click)="load(true)">↻ Refresh</button></div>
<div class="toolbar"><form class="search-form" (ngSubmit)="page=1;load()"><input aria-label="Search invoices" name="search" [(ngModel)]="search" placeholder="Search invoice number…"><button class="button small" type="submit">Search</button></form><span class="badge">{{totalCount}} records</span></div>
@if(loading){<div class="skeleton-wrap"><div class="skeleton" style="height:22px;width:50%"></div><div class="skeleton" style="height:170px;margin-top:16px"></div></div>}
@else if(rows.length){<div class="table-wrap"><table><thead><tr><th>ID</th><th><button class="sort-button" type="button" (click)="sort('invoiceNumber')">Invoice {{sortBy==='invoiceNumber'?(descending?'↓':'↑'):''}}</button></th><th>{{isPurchase?'Purchased':'Sold'}} at (UTC)</th><th>{{isPurchase?'Supplier':'Customer'}} ID</th><th>Lines</th><th><button class="sort-button" type="button" (click)="sort('grandTotal')">Grand total {{sortBy==='grandTotal'?(descending?'↓':'↑'):''}}</button></th><th>Receipt</th></tr></thead><tbody>
@for(row of rows;track row.id){<tr><td>#{{row.id}}</td><td>{{row.invoiceNumber}}</td><td>{{isPurchase?row.purchasedAtUtc:row.soldAtUtc}}</td><td>{{isPurchase?row.supplierId:(row.customerId??'Walk-in')}}</td><td>{{row.lineCount}}</td><td>{{money(row.grandTotal)}}</td><td><a class="button small secondary" [routerLink]="['/receipt',isPurchase?'purchases':'sales',row.id]">Print / PDF</a></td></tr>}
</tbody></table></div>}@else{<div class="empty-state compact"><h3>No transactions yet</h3><p class="muted">Your recorded invoices will appear here.</p></div>}
<div class="pagination"><span class="muted">Page {{page}} / {{pages()}} · {{totalCount}} invoices</span><div class="button-group"><label for="txn-size">Rows</label><select id="txn-size" [(ngModel)]="pageSize" (ngModelChange)="page=1;load()"><option [ngValue]="10">10</option><option [ngValue]="20">20</option><option [ngValue]="50">50</option></select>
<button class="button small secondary" [disabled]="page<=1||loading" (click)="page=page-1;load()">Previous</button><button class="button small secondary" [disabled]="page>=pages()||loading" (click)="page=page+1;load()">Next</button></div></div></section>
<dialog class="wide" #transDialog><div id="modal-content"><div class="modal-header"><div><p class="eyebrow">PHARMACY WORKSPACE</p><h2>{{isPurchase?'Record a purchase':'New sale'}}</h2><p class="muted">Review all quantities before posting; server validates stock and totals.</p></div>
<button class="icon-button" aria-label="Close" type="button" (click)="close()">×</button></div>
@if(saveError){<p role="alert" class="form-error">{{saveError}}</p>}
<form (ngSubmit)="submit()">
<div class="form-grid">
<div class="field"><label for="invoice">Invoice number *</label><input id="invoice" name="invoice" [(ngModel)]="invoiceNumber" required maxlength="50"></div>
<div class="field"><label for="party">{{isPurchase?'Supplier':'Customer (optional)'}}</label><select id="party" name="party" [(ngModel)]="partyId" [required]="isPurchase"><option [ngValue]="null">{{isPurchase?'Choose supplier':'Walk-in customer'}}</option>
@for(p of parties;track p['id']){<option [ngValue]="p['id']">{{p['name']}}</option>}</select></div>
<div class="field"><label for="discount">Discount</label><input id="discount" type="number" name="discount" [(ngModel)]="discount" min="0" step="0.01" required></div>
<div class="field"><label for="notes">Notes</label><input id="notes" name="notes" [(ngModel)]="notes" maxlength="500"></div>
</div>
<div class="subheading"><h3>Line items</h3><button type="button" class="button small secondary" (click)="addLine()">+ Add line</button></div>
@for(line of lines;track $index;let i=$index){
<div class="transaction-line">
<div class="line-grid">
@if(isPurchase){
  <div class="field"><label>Medicine *</label><select [name]="'medicine-'+i" [(ngModel)]="line['medicineId']" required><option [ngValue]="0">Select medicine</option>
    @for(m of medicines;track m.id){@if(m.isActive){<option [ngValue]="m.id">{{m.code}} · {{m.name}}</option>}}</select></div>
  <div class="field"><label>Batch number *</label><input [name]="'batch-'+i" [(ngModel)]="line['batchNumber']" required maxlength="50"></div>
  <div class="field"><label>Expiry date *</label><input type="date" [name]="'expiry-'+i" [(ngModel)]="line['expiryDate']" [min]="tomorrow" required></div>
  <div class="field"><label>Quantity *</label><input type="number" [name]="'qty-'+i" [(ngModel)]="line['quantity']" min="1" step="1" required></div>
  <div class="field"><label>Unit cost *</label><input type="number" [name]="'price-'+i" [(ngModel)]="line['unitPrice']" min="0" step="0.01" required></div>
}@else{
  <div class="field"><label>Medicine (FEFO) *</label><select [name]="'medicine-sale-'+i" [(ngModel)]="line['medicineId']" required><option [ngValue]="0">Select medicine</option>
    @for(m of medicines;track m.id){@if(m.isActive&&availableByMedicine(m.id)>0){<option [ngValue]="m.id">{{m.name}} · {{availableByMedicine(m.id)}} units</option>}}</select></div>
  <div class="field"><label>Quantity *</label><input type="number" [name]="'qty-'+i" [(ngModel)]="line['quantity']" min="1" step="1" required></div>
  <div class="field"><label>Total stock</label><input readonly [value]="stockAvailable(line)"></div>
  <div class="field"><label>Unit sell price</label><input readonly [value]="sellingPrice(line) | number:'1.2-2'"></div>
  <div class="form-hint fefo-hint">{{fefoPreview(line)}}</div>
}
<div class="line-total"><small>Line total (estimate)</small><strong>{{money(lineTotal(line))}}</strong></div>
<button type="button" class="button small danger-ghost remove-line" (click)="removeLine(i)" [disabled]="lines.length===1">Remove</button>
</div></div>
}
<div class="transaction-bottom"><div class="form-hint">{{isPurchase?'Purchase increases batch quantity.':'FEFO allocates earliest-expiring valid batches first. Actual totals use server prices.'}}</div>
<div class="totals-box"><p>Estimated subtotal <strong>{{money(subtotal())}}</strong></p><p>Discount <strong>{{money(discount)}}</strong></p><p>Estimated total <strong>{{money(subtotal()-discount)}}</strong></p></div></div>
<div class="form-actions"><button type="button" class="button secondary" (click)="close()">Cancel</button><button type="submit" class="button" [disabled]="saving">{{saving?'Saving…':isPurchase?'Record purchase':'Complete sale'}}</button></div>
</form></div></dialog>
`})
export class TransactionsComponent implements OnInit {
  private api=inject(ApiService);readonly auth=inject(AuthService);private toast=inject(ToastService);private route=inject(ActivatedRoute);
  readonly money=money;readonly today=todayLocal(); readonly tomorrow=(()=>{const d=new Date(this.today+'T00:00:00Z');d.setUTCDate(d.getUTCDate()+1);return d.toISOString().slice(0,10);})();
  @ViewChild('transDialog') transDialog!:ElementRef<HTMLDialogElement>;
  isPurchase=false;page=1;pageSize=20;totalCount=0;search='';sortBy='date';descending=true;rows:Transaction[]=[];parties:Item[]=[];medicines:Medicine[]=[];batches:StockBatch[]=[];
  invoiceNumber='';notes='';partyId:number|null=null;discount=0;lines:Item[]=[];
  loading=true;saving=false;error='';saveError='';
  ngOnInit(){this.isPurchase=this.route.snapshot.routeConfig?.path==='purchases';void this.load();
    if(this.route.snapshot.queryParamMap.get('new')==='1' && this.canWrite())setTimeout(()=>void this.openNew(),0);
  }
  canWrite(){return this.auth.can(this.isPurchase?'Purchases.Write':'Sales.Write');}
  async load(notify=false){this.error='';this.loading=true;try{const result=await this.api.get<PageResult<Transaction>>(`/transaction-pages/${this.isPurchase?'purchases':'sales'}`,{page:this.page,pageSize:this.pageSize,search:this.search,sortBy:this.sortBy,descending:this.descending});this.rows=result.items;this.totalCount=result.totalCount;if(notify)this.toast.info(`${this.isPurchase?'Purchase':'Sales'} records refreshed.`);}
    catch(e){this.error=errorMessage(e);}finally{this.loading=false;}}
  pages(){return Math.max(1,Math.ceil(this.totalCount/this.pageSize));}
  sort(field:string){if(this.sortBy===field)this.descending=!this.descending;else{this.sortBy=field;this.descending=false;}this.page=1;void this.load();}
  addLine(){this.lines.push(this.isPurchase?{medicineId:0,batchNumber:'',expiryDate:this.tomorrow,quantity:1,unitPrice:0}:{medicineId:0,quantity:1});}
  removeLine(i:number){if(this.lines.length>1)this.lines.splice(i,1);}
  async openNew(){if(!this.canWrite())return;this.saveError='';this.notes='';this.discount=0;this.partyId=null;
    this.invoiceNumber=`${this.isPurchase?'PUR':'SAL'}-${new Date().toISOString().replace(/[-:.TZ]/g,'').slice(0,14)}`;this.lines=[];this.addLine();
    try{const endpoint=this.isPurchase?'/suppliers':'/customers';this.parties=await this.api.get<Item[]>(endpoint);
      if(this.isPurchase)this.medicines=await this.api.get<Medicine[]>('/medicines');
      else {this.batches=await this.api.get<StockBatch[]>('/stock',{includeZero:false});this.medicines=await this.api.get<Medicine[]>('/medicines');}
      this.transDialog.nativeElement.showModal();
    }catch(e){this.error=errorMessage(e);}}
  close(){this.transDialog.nativeElement.close();}
  availableByMedicine(id:number){return this.batches.filter(b=>b.medicineId===id&&!b.isExpired&&b.quantityOnHand>0).reduce((total,b)=>total+b.quantityOnHand,0);}
  stockAvailable(line:Item){return this.availableByMedicine(Number(line['medicineId']));}
  sellingPrice(line:Item){return this.medicines.find(m=>m.id===Number(line['medicineId']))?.sellingPrice??0;}
  fefoPreview(line:Item){const id=Number(line['medicineId']);if(!id)return 'Select a medicine to view FEFO batches.';try{return 'FEFO: '+allocateFefo(this.batches,id,Number(line['quantity'])).map(x=>{const b=this.batches.find(b=>b.stockBatchId===x.stockBatchId);return `${b?.batchNumber} (${x.quantity}, exp ${b?.expiryDate})`;}).join(' → ');}catch(e){return errorMessage(e);}}
  lineTotal(line:Item){return Number(line['quantity']||0)*(this.isPurchase?Number(line['unitPrice']||0):this.sellingPrice(line));}
  subtotal(){return this.lines.reduce((sum,line)=>sum+this.lineTotal(line),0);}
  async submit(){if(!this.canWrite())return;this.saveError='';
    if(this.isPurchase&&!this.partyId){this.saveError='Select a supplier.';this.toast.warning(this.saveError);return;}
    if(!this.lines.length||this.lines.some(line=>!Number.isInteger(Number(line['quantity']))||Number(line['quantity'])<=0)) {this.saveError='Use positive whole-number quantities.';this.toast.warning(this.saveError);return;}
    if(this.isPurchase && this.lines.some(line=>!Number(line['medicineId'])||!line['batchNumber']||line['expiryDate']<=this.today)){this.saveError='Choose active medicine, valid batch and future expiry date.';this.toast.warning(this.saveError);return;}
    if(!this.isPurchase && (this.lines.some(line=>!Number(line['medicineId'])||Number(line['quantity'])>this.stockAvailable(line)) || new Set(this.lines.map(x=>Number(x['medicineId']))).size!==this.lines.length)) {this.saveError='Choose different medicines per line and check their available stock.';this.toast.warning(this.saveError);return;}
    if(this.discount>this.subtotal()){this.saveError='Discount cannot exceed subtotal.';this.toast.warning(this.saveError);return;}
    this.saving=true;
    try{
      if(this.isPurchase){const items:PurchaseLineInput[]=this.lines.map(line=>({medicineId:Number(line['medicineId']),batchNumber:String(line['batchNumber']),expiryDate:String(line['expiryDate']),quantity:Number(line['quantity']),unitPrice:Number(line['unitPrice'])}));
        await this.api.post('/purchases',{invoiceNumber:this.invoiceNumber,supplierId:this.partyId,notes:this.notes,discount:this.discount,items});}
      else {const items:SaleLineInput[]=this.lines.flatMap(line=>allocateFefo(this.batches,Number(line['medicineId']),Number(line['quantity'])));
        await this.api.post('/sales',{invoiceNumber:this.invoiceNumber,customerId:this.partyId,notes:this.notes,discount:this.discount,items});}
      this.close();this.toast.success(`${this.isPurchase?'Purchase':'Sale'} ${this.invoiceNumber} recorded successfully.`);await this.load();
    }catch(e){this.saveError=errorMessage(e);}finally{this.saving=false;}
  }
}

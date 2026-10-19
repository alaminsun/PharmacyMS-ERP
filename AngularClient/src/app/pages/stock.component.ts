import { Component, OnInit, inject } from '@angular/core';
import { ApiService, errorMessage } from '../core/api.service';
import { LowStock, StockBatch } from '../core/models';
import { ToastService } from '../core/toast.service';
import { FormsModule } from '@angular/forms';
@Component({selector:'app-stock',standalone:true,imports:[FormsModule],template:`
<div class="page-heading"><div><p class="eyebrow">INVENTORY</p><h1>Stock overview</h1><p class="muted">Batch-level inventory and medicines below their minimum stock levels.</p></div><button class="button secondary" (click)="load(true)">↻ Refresh</button></div>
@if(error){<p class="form-error" role="alert">{{error}}</p>}
<section class="panel" style="margin-bottom:23px"><div class="panel-heading"><div><h2>Medicine stock alerts</h2><p class="muted">Expired batches are not counted toward available units.</p></div><span class="badge amber">{{alerts.length}} alerts</span></div>
@if(loading){<div class="skeleton-wrap"><div class="skeleton" style="height:100px"></div></div>}
@else if(alerts.length){<div class="table-wrap"><table><thead><tr><th>Code</th><th>Medicine</th><th>Available</th><th>Minimum</th><th>Status</th></tr></thead><tbody>
@for(row of alerts;track row.medicineId){<tr><td>{{row.medicineCode}}</td><td>{{row.medicineName}}</td><td>{{row.availableUnits}}</td><td>{{row.minimumStockLevel}}</td><td><span class="badge amber">{{row.status==='OutOfStock'?'Out of stock':'Low stock'}}</span></td></tr>}
</tbody></table></div>}@else{<div class="empty-state compact">No low-stock medicines.</div>}</section>
<section class="panel" style="margin-bottom:23px"><div class="panel-heading"><div><h2>Expiry alerts</h2><p class="muted">Batches approaching expiry (including expired). Quantity > 0 only.</p></div>
<label class="field" style="max-width:160px"><span>Alert window</span><select [(ngModel)]="expiryDays" aria-label="Expiry alert window"><option [ngValue]="7">Next 7 days</option><option [ngValue]="30">Next 30 days</option><option [ngValue]="60">Next 60 days</option></select></label></div>
@if(loading){<div class="skeleton-wrap"><div class="skeleton" style="height:110px"></div></div>}
@else if(expiryAlerts().length){<div class="table-wrap"><table><thead><tr><th>Medicine</th><th>Batch</th><th>Expiry</th><th>Units</th><th>Alert</th></tr></thead><tbody>
@for(row of expiryAlerts();track row.stockBatchId){<tr><td>{{row.medicineName}}</td><td>{{row.batchNumber}}</td><td>{{row.expiryDate}}</td><td>{{row.quantityOnHand}}</td><td><span class="badge amber">{{row.isExpired?'Expired':(daysUntilExpiry(row.expiryDate)+' days left')}}</span></td></tr>}</tbody></table></div>}@else{<div class="empty-state compact">No batches expiring in the selected window.</div>}</section>
<section class="panel"><div class="panel-heading"><div><h2>Batch inventory</h2><p class="muted">Available quantities and expiry dates.</p></div><label class="check-label"><input type="checkbox" #include [checked]="includeZero" (change)="includeZero=include.checked;load(true)">Include zero stock</label></div>
@if(loading){<div class="skeleton-wrap"><div class="skeleton" style="height:25px;width:55%"></div><div class="skeleton" style="height:160px;margin-top:15px"></div></div>}
@else if(batches.length){<div class="table-wrap"><table><thead><tr><th>Batch ID</th><th>Medicine</th><th>Batch number</th><th>Expiry date</th><th>Quantity</th><th>Condition</th></tr></thead><tbody>
@for(row of batches;track row.stockBatchId){<tr><td>#{{row.stockBatchId}}</td><td>{{row.medicineName}}</td><td>{{row.batchNumber}}</td><td>{{row.expiryDate}}</td><td>{{row.quantityOnHand}}</td><td><span class="badge" [class.amber]="row.isExpired">{{row.isExpired?'Expired':'Valid'}}</span></td></tr>}
</tbody></table></div>}@else{<div class="empty-state compact">No inventory batches found.</div>}</section>
`})
export class StockComponent implements OnInit {
  readonly api=inject(ApiService);private toast=inject(ToastService);batches:StockBatch[]=[];alerts:LowStock[]=[];includeZero=false;expiryDays=30;loading=true;error='';
  ngOnInit(){void this.load();}
  daysUntilExpiry(date:string){const now=new Date();const today=Date.UTC(now.getUTCFullYear(),now.getUTCMonth(),now.getUTCDate());const d=new Date(date.slice(0,10)+'T00:00:00Z').getTime();return Math.round((d-today)/86400000);}
  expiryAlerts(){return this.batches.filter(b=>b.quantityOnHand>0 && this.daysUntilExpiry(b.expiryDate)<=this.expiryDays).sort((a,b)=>a.expiryDate.localeCompare(b.expiryDate));}
  async load(notify=false){this.error='';this.loading=true;try{
    const [batches,alerts]=await Promise.all([this.api.get<StockBatch[]>('/stock',{includeZero:this.includeZero}),this.api.get<LowStock[]>('/stock/low-stock')]);
    this.batches=batches;this.alerts=alerts;if(notify)this.toast.info('Inventory refreshed.');
  }catch(e){this.error=errorMessage(e);}finally{this.loading=false;}}
}

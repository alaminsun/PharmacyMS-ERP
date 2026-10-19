import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, errorMessage } from '../core/api.service';
import { Report, SaleReportDetail } from '../core/models';
import { downloadCsv, isoEndExclusive, isoStart, money, todayLocal } from '../shared/helpers';
import { ToastService } from '../core/toast.service';
@Component({selector:'app-reports',standalone:true,imports:[FormsModule],template:`
<div class="page-heading"><div><p class="eyebrow">INSIGHTS</p><h1>Reports</h1><p class="muted">Review sales, purchases and saved invoice prices without changing historical transactions.</p></div>
@if(summary){<button class="button secondary" (click)="exportCsv()">↓ Export sales CSV</button>}</div>
@if(error){<p class="form-error" role="alert">{{error}}</p>}
<section class="panel"><div class="report-header"><form class="report-filters" (ngSubmit)="load(true)">
<div class="field"><label for="reportFrom">From (UTC)</label><input id="reportFrom" type="date" name="from" [(ngModel)]="from" required></div>
<div class="field"><label for="reportTo">To (inclusive, UTC)</label><input id="reportTo" type="date" name="to" [(ngModel)]="to" required></div>
<button class="button secondary" type="submit">Apply dates</button></form><span class="badge">UTC date range</span></div>
@if(loading){<div class="skeleton-wrap"><div class="skeleton" style="width:40%;height:22px"></div><div class="skeleton" style="height:120px;margin-top:18px"></div></div>}
@if(summary){
<section class="stats-grid report-stats">
@for(item of cards();track item.label){<article class="stat-card"><div class="stat-top"><p>{{item.label}}</p><span class="stat-icon">{{item.symbol}}</span></div><strong>{{item.money?money(item.value):item.value}}</strong><small>{{item.note}}</small></article>}
</section>
<div class="panel-heading"><div><h2>Saved sale invoice details</h2><p class="muted">The unit price below is stored with each SaleLine; current Medicine SellingPrice is not used.</p></div><span class="badge">{{details.length}} invoices</span></div>
@if(details.length){<div class="table-wrap"><table><thead><tr><th>Invoice</th><th>Sold at (UTC)</th><th>Medicine</th><th>Batch ID</th><th>Qty</th><th>Unit price</th><th>Line total</th><th>Invoice net</th></tr></thead><tbody>
@for(invoice of details;track invoice.saleId){
  @for(line of invoice.items;track $index){<tr><td>{{invoice.invoiceNumber}}</td><td>{{invoice.soldAtUtc}}</td><td>{{line.medicineName}}</td><td>#{{line.stockBatchId}}</td><td>{{line.quantity}}</td><td>{{money(line.unitPrice)}}</td><td>{{money(line.lineTotal)}}</td><td>{{money(invoice.grandTotal)}}</td></tr>}
}
</tbody></table></div>}@else{<div class="empty-state compact">No sales for this reporting period.</div>}
<p class="report-note form-hint">Sales total is the net sum of saved GrandTotal values. Report dates use [From UTC, next-day To UTC), so the selected final day is included. Monetary values follow your backend data.</p>
}</section>
`})
export class ReportsComponent implements OnInit {
  private api=inject(ApiService);private toast=inject(ToastService);readonly money=money;readonly today=todayLocal();
  from=this.today.slice(0,7)+'-01';to=this.today; summary:Report|null=null;details:SaleReportDetail[]=[];loading=true;error='';
  ngOnInit(){void this.load();}
  async load(notify=false){this.error='';if(!this.from||!this.to||this.from>this.to){this.error='The start date must be on or before the end date.';this.toast.warning(this.error);return;}
    const start=isoStart(this.from);const end=isoEndExclusive(this.to);
    const range={fromUtc:start,toUtc:end};this.loading=true;
    try{[this.summary,this.details]=await Promise.all([this.api.get<Report>('/reports',range),this.api.get<SaleReportDetail[]>('/reports/sales-details',range)]);if(notify)this.toast.success('Report updated for the selected date range.');}
    catch(e){this.summary=null;this.details=[];this.error=errorMessage(e);}finally{this.loading=false;}
  }
  cards(){if(!this.summary)return [];
    return [
      {label:'Net sales',value:this.summary.salesTotal,money:true,symbol:'↗',note:'Saved sales GrandTotal'},
      {label:'Sales subtotal',value:this.summary.salesSubtotal,money:true,symbol:'▥',note:'Before sales discounts'},
      {label:'Sales discount',value:this.summary.salesDiscount,money:true,symbol:'−',note:'Discounts applied'},
      {label:'Sale count',value:this.summary.saleCount,money:false,symbol:'◫',note:'Recorded invoices'},
      {label:'Net purchases',value:this.summary.purchasesTotal,money:true,symbol:'↙',note:'Saved purchase GrandTotal'},
      {label:'Purchase subtotal',value:this.summary.purchasesSubtotal,money:true,symbol:'▥',note:'Before purchase discounts'},
      {label:'Purchase discount',value:this.summary.purchasesDiscount,money:true,symbol:'−',note:'Discounts applied'},
      {label:'Purchase count',value:this.summary.purchaseCount,money:false,symbol:'◫',note:'Recorded invoices'}
    ];
  }
  exportCsv(){if(!this.details.length){this.toast.info('No sales details to export for this date range.');return;}
    const rows:Record<string,unknown>[]=[];
    for(const sale of this.details)for(const line of sale.items)rows.push({invoiceNumber:sale.invoiceNumber,soldAtUtc:sale.soldAtUtc,medicineName:line.medicineName,stockBatchId:line.stockBatchId,quantity:line.quantity,unitPrice:line.unitPrice,lineTotal:line.lineTotal,invoiceSubtotal:sale.subtotal,invoiceDiscount:sale.discount,invoiceGrandTotal:sale.grandTotal});
    const columns=['invoiceNumber','soldAtUtc','medicineName','stockBatchId','quantity','unitPrice','lineTotal','invoiceSubtotal','invoiceDiscount','invoiceGrandTotal'];
    downloadCsv(rows,columns,`pharmacy-sales-${this.from}-${this.to}.csv`);this.toast.success(`${rows.length} sales line${rows.length===1?'':'s'} exported to CSV.`);
  }
}

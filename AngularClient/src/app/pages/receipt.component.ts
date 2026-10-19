import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService, errorMessage } from '../core/api.service';
import { Receipt } from '../core/models';
import { money } from '../shared/helpers';
@Component({selector:'app-receipt',standalone:true,imports:[RouterLink],template:`
<div class="page-heading no-print"><div><p class="eyebrow">DOCUMENTS</p><h1>Invoice / receipt</h1><p class="muted">Print this record or choose “Save as PDF” in the browser print dialog.</p></div>
<div class="button-group"><a class="button secondary" [routerLink]="kind==='sales'?'/sales':'/purchases'">← Back</a><button type="button" class="button" [disabled]="loading||!receipt" (click)="print()">Print / Save PDF</button></div></div>
@if(error){<p class="form-error no-print">{{error}}</p>}
@if(loading){<section class="panel"><div class="skeleton-wrap"><div class="skeleton" style="width:60%;height:26px"></div><div class="skeleton" style="height:220px;margin-top:22px"></div></div></section>}
@if(receipt;as r){<article class="invoice-sheet" id="invoice-print">
<header class="invoice-head"><div><div class="receipt-brand"><span class="receipt-plus">+</span> PharmacyMS</div><p>Pharmacy Management System</p></div><div class="invoice-meta"><h1>{{r.kind}} Invoice</h1><strong>{{r.invoiceNumber}}</strong><p>{{r.dateUtc}} (UTC)</p></div></header>
<div class="invoice-party"><div><span>BILLED {{kind==='sales'?'TO':'FROM'}}</span><strong>{{r.partyName}}</strong></div><div><span>TRANSACTION ID</span><strong>#{{r.id}}</strong></div></div>
<table class="invoice-lines"><thead><tr><th>Medicine / batch</th><th>Quantity</th><th>Unit price</th><th>Amount</th></tr></thead><tbody>
@for(item of r.items;track $index){<tr><td><strong>{{item.medicineName}}</strong><small>{{item.medicineCode}} · Batch {{item.batchNumber}}</small></td><td>{{item.quantity}}</td><td>{{money(item.unitPrice)}}</td><td>{{money(item.lineTotal)}}</td></tr>}</tbody></table>
<div class="invoice-totals"><div><span>Subtotal</span><strong>{{money(r.subtotal)}}</strong></div><div><span>Discount</span><strong>{{money(r.discount)}}</strong></div><div class="net"><span>Grand total</span><strong>{{money(r.grandTotal)}}</strong></div></div>
<footer class="invoice-footer">Generated from saved PharmacyMS transaction records. Prices and quantities are historical; this document is not an accounting tax invoice unless your pharmacy's legal requirements are satisfied.</footer>
</article>}
`})
export class ReceiptComponent implements OnInit {
  private route=inject(ActivatedRoute);private api=inject(ApiService);
  readonly money=money;kind='sales';receipt:Receipt|null=null;loading=true;error='';
  ngOnInit(){this.kind=this.route.snapshot.paramMap.get('kind')==='purchases'?'purchases':'sales';void this.load();}
  async load(){this.loading=true;try{const id=Number(this.route.snapshot.paramMap.get('id'));if(!Number.isSafeInteger(id)||id<=0)throw new Error('Invalid transaction ID.');
    this.receipt=await this.api.get<Receipt>(`/receipts/${this.kind}/${id}`);
  }catch(e){this.error=errorMessage(e);}finally{this.loading=false;}}
  print(){if(this.receipt)window.print();}
}

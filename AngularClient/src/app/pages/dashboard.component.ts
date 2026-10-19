import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService, errorMessage } from '../core/api.service';
import { AuthService } from '../core/auth.service';
import { Dashboard, LowStock } from '../core/models';
import { money } from '../shared/helpers';
import { ToastService } from '../core/toast.service';
@Component({selector:'app-dashboard',standalone:true,imports:[RouterLink],template:`
<div class="page-heading"><div><p class="eyebrow">YOUR PHARMACY, AT A GLANCE</p><h1>Overview</h1><p class="muted">Your inventory, sales and supply in one place.</p></div><button class="button secondary small" (click)="load(true)">↻ Refresh</button></div>
@if(error){<p class="form-error" role="alert">{{error}}</p>}
@if(loading){<div class="stats-grid">@for(x of [1,2,3,4];track x){<article class="stat-card"><div class="skeleton" style="width:45%;height:14px"></div><div class="skeleton" style="width:70%;height:30px;margin-top:25px"></div></article>}</div>}
@if(data){
<section class="welcome-banner"><div><span class="badge lime">READY FOR THE DAY</span><h2>Good care starts<br>with a clear overview.</h2><p>Keep your shelves ready and your daily work moving.</p>
<div class="quick-actions">@if(auth.can('Sales.Write')){<a class="button" routerLink="/sales" [queryParams]="{new:1}">↗ New sale</a>}
@if(auth.can('Purchases.Write')){<a class="button secondary" routerLink="/purchases" [queryParams]="{new:1}">↙ Record purchase</a>}</div></div>
<div class="hero-graphic" aria-hidden="true"><div class="graphic-cross">+</div><span class="graphic-pill pill-one"></span><span class="graphic-pill pill-two"></span><span class="graphic-orbit"></span></div></section>
<section class="stats-grid">
@for(stat of stats();track stat.title){<article class="stat-card"><div class="stat-top"><p>{{stat.title}}</p><span class="stat-icon">{{stat.icon}}</span></div>
<strong>{{stat.currency ? money(stat.value):stat.value.toLocaleString()}}</strong><small>{{stat.caption}}</small></article>}
</section>
@if(auth.can('Stock.Read')){<div class="dashboard-bottom"><section class="panel"><div class="panel-heading"><div><h2>Stock to keep an eye on</h2><p class="muted">Medicine stock at/below minimum; expired batches excluded.</p></div><span class="badge amber">{{alerts.length}} items</span></div>
@if(alerts.length){<div class="table-wrap"><table><thead><tr><th>Medicine</th><th>Available</th><th>Minimum</th><th>Status</th></tr></thead><tbody>
@for(row of alerts.slice(0,8);track row.medicineId){<tr><td>{{row.medicineName}}</td><td>{{row.availableUnits}}</td><td>{{row.minimumStockLevel}}</td><td><span class="badge amber">{{row.status === 'OutOfStock' ? 'Out of stock':'Low stock'}}</span></td></tr>}
</tbody></table></div>}@else{<div class="empty-state compact">No low-stock or out-of-stock medicines.</div>}
<a class="text-link" style="padding:0 23px 20px" routerLink="/stock">See inventory →</a></section>
@if(auth.can('Purchases.Read')){<aside class="panel summary-panel"><p class="eyebrow">SUPPLY OVERVIEW</p><span class="large-symbol">↙</span><h2>Purchases recorded</h2><strong class="summary-value">{{money(data.totalPurchases)}}</strong><p class="muted">Net purchase amount.</p><a class="text-link" routerLink="/purchases">View purchases →</a></aside>}
</div>}
}
`})
export class DashboardComponent implements OnInit {
  readonly api=inject(ApiService);readonly auth=inject(AuthService);readonly money=money;private toast=inject(ToastService);
  data:Dashboard|null=null;alerts:LowStock[]=[];loading=true;error='';
  ngOnInit(){void this.load();}
  async load(notify=false){this.loading=true;this.error='';try{
    const dashboard=await this.api.get<Dashboard>('/dashboard');this.data=dashboard;
    this.alerts=this.auth.can('Stock.Read')?await this.api.get<LowStock[]>('/stock/low-stock'):[];if(notify)this.toast.info('Dashboard refreshed.');
  }catch(e){this.error=errorMessage(e);}finally{this.loading=false;}}
  stats(){if(!this.data)return [];
    const d=this.data;
    return [
      {title:'Medicines',value:d.medicines,caption:'Active catalog items',icon:'✧',currency:false},
      {title:'Available units',value:d.availableUnits,caption:'Unexpired stock',icon:'▤',currency:false},
      {title:'Low stock',value:d.lowStockMedicines,caption:`${d.outOfStockMedicines} out of stock`,icon:'!',currency:false},
      {title:'Sales total',value:d.totalSales,caption:'Recorded net sales',icon:'↗',currency:true},
      {title:'Suppliers',value:d.suppliers,caption:'Supply partners',icon:'♧',currency:false},
      {title:'Customers',value:d.customers,caption:'People you serve',icon:'○',currency:false},
      {title:'Purchases',value:d.totalPurchases,caption:'Recorded net purchases',icon:'↙',currency:true},
      {title:'Expired units',value:d.expiredUnits,caption:'Excluded from available stock',icon:'⌛',currency:false}
    ].filter(x=>x.title==='Medicines' ? this.auth.can('Medicines.Read') : x.title==='Suppliers' ? this.auth.can('Suppliers.Read') : x.title==='Customers' ? this.auth.can('Customers.Read') : ['Available units','Low stock','Expired units'].includes(x.title)?this.auth.can('Stock.Read') : x.title==='Sales total'?this.auth.can('Sales.Read'):this.auth.can('Purchases.Read'));
  }
}

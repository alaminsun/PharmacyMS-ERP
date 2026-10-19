import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, errorMessage } from '../core/api.service';
import { AuditEvent, PageResult } from '../core/models';
@Component({selector:'app-audit',standalone:true,imports:[FormsModule],template:`
<div class="page-heading"><div><p class="eyebrow">ADMINISTRATION</p><h1>Audit history</h1><p class="muted">Successful changes to inventory, sales, users and permissions — metadata only.</p></div><button class="button secondary" (click)="load()">↻ Refresh</button></div>
@if(error){<p class="form-error" role="alert">{{error}}</p>}
<section class="panel"><div class="toolbar"><form class="search-form" (ngSubmit)="page=1;load()"><input name="actor" [(ngModel)]="actorEmail" placeholder="Actor email" aria-label="Actor email"><input name="resource" [(ngModel)]="resource" placeholder="API path" aria-label="API path"><button class="button small" type="submit">Filter</button></form><span class="badge">{{totalCount}} events</span></div>
@if(loading){<div class="skeleton-wrap"><div class="skeleton" style="height:28px"></div><div class="skeleton" style="height:180px;margin-top:15px"></div></div>}
@else if(items.length){<div class="table-wrap"><table><thead><tr><th>UTC Time</th><th>Actor</th><th>Action</th><th>API Path</th><th>Status</th><th>Trace</th></tr></thead><tbody>
@for(item of items;track item.id){<tr><td>{{item.occurredAtUtc}}</td><td>{{item.actorEmail}}</td><td><span class="badge">{{item.action}}</span></td><td>{{item.path}}</td><td>{{item.statusCode}}</td><td><code>{{item.traceId}}</code></td></tr>}</tbody></table></div>}
@else{<div class="empty-state compact">No matching audit events yet.</div>}
<div class="pagination"><span class="muted">Page {{page}} of {{pages()}} · {{totalCount}} records</span><div class="button-group"><button class="button small secondary" [disabled]="page<=1||loading" (click)="page=page-1;load()">Previous</button><button class="button small secondary" [disabled]="page>=pages()||loading" (click)="page=page+1;load()">Next</button></div></div></section>
`})
export class AuditComponent implements OnInit {
  private api=inject(ApiService);items:AuditEvent[]=[];page=1;pageSize=25;totalCount=0;actorEmail='';resource='';loading=true;error='';
  ngOnInit(){void this.load();}
  pages(){return Math.max(1,Math.ceil(this.totalCount/this.pageSize));}
  async load(){this.loading=true;this.error='';try{const result=await this.api.get<PageResult<AuditEvent>>('/audit',{page:this.page,pageSize:this.pageSize,actorEmail:this.actorEmail,resource:this.resource});this.items=result.items;this.totalCount=result.totalCount;}
    catch(e){this.error=errorMessage(e);}finally{this.loading=false;}}
}

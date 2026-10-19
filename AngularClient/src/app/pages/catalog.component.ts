import { Component, ElementRef, OnInit, ViewChild, inject, DestroyRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ApiService, errorMessage } from '../core/api.service';
import { AuthService } from '../core/auth.service';
import { CATALOGS } from '../core/catalog';
import { CatalogDefinition, Item } from '../core/models';
import { display, humanize } from '../shared/helpers';
import { ConfirmService } from '../core/confirm.service';
import { PageResult } from '../core/models';
import { ToastService } from '../core/toast.service';
@Component({selector:'app-catalog',standalone:true,imports:[FormsModule],template:`
<div class="page-heading"><div><p class="eyebrow">CATALOG</p><h1>{{meta?.title}}</h1><p class="muted">Keep your pharmacy catalog organized and up to date.</p></div>
@if(canWrite()){<button class="button" (click)="openEditor()">+ Add {{meta?.title}}</button>}</div>
@if(error){<p class="form-error" role="alert">{{error}}</p>}
<section class="panel"><div class="toolbar"><form class="search-form" (ngSubmit)="applySearch()"><span aria-hidden="true">⌕</span><input name="search" aria-label="Search records" [(ngModel)]="search" placeholder="Search records…"><button type="submit" class="search-submit">Search</button></form><span class="badge">{{totalCount}} records</span></div>
@if(loading){<div class="skeleton-wrap"><div class="skeleton" style="height:22px;width:50%"></div><div class="skeleton" style="height:160px;margin-top:15px"></div></div>}
@else if(filtered.length){<div class="table-wrap" tabindex="0"><table><thead><tr>
@for(column of meta?.columns || [];track column){<th>@if(isSortable(column)){<button class="sort-button" type="button" (click)="sort(column)" [attr.aria-label]="'Sort by '+humanize(column)">{{humanize(column)}} @if(sortBy===column){<span>{{descending?'↓':'↑'}}</span>}</button>}@else{<span>{{humanize(column)}}</span>}</th>}
@if(canWrite()){<th>Actions</th>}</tr></thead><tbody>
@for(item of filtered;track item['id']){<tr>@for(column of meta?.columns || [];track column){<td>{{display(item[column])}}</td>}
@if(canWrite()){<td class="table-actions"><button class="button small secondary" (click)="openEditor(item)">Edit</button>
<button class="button small danger-ghost" (click)="remove(item)">{{meta?.key==='medicines'?'Deactivate':'Delete'}}</button></td>}</tr>}
</tbody></table></div>}@else{<div class="empty-state compact"><h3>No matching records</h3><p class="muted">Add a new record or change your search.</p></div>}
<div class="pagination"><span class="muted">Showing {{filtered.length}} of {{totalCount}} · Page {{page}} / {{pages()}}</span>
<div class="button-group"><label class="muted" for="pageSize">Rows</label><select id="pageSize" aria-label="Rows per page" [(ngModel)]="pageSize" (ngModelChange)="page=1;load()"><option [ngValue]="10">10</option><option [ngValue]="20">20</option><option [ngValue]="50">50</option></select>
<button class="button small secondary" [disabled]="page===1||loading" (click)="page=page-1;load()">← Previous</button><button class="button small secondary" [disabled]="page>=pages()||loading" (click)="page=page+1;load()">Next →</button><button class="button small secondary" (click)="load(true)">↻ Refresh</button></div></div></section>
<dialog #editDialog class="wide"><div id="modal-content"><div class="modal-header"><div><p class="eyebrow">PHARMACY WORKSPACE</p><h2>{{editing['id']?'Edit':'Add'}} {{meta?.title}}</h2><p class="muted">Fields marked * are required.</p></div>
<button type="button" class="icon-button" aria-label="Close" (click)="close()">×</button></div>
@if(saveError){<p class="form-error" role="alert">{{saveError}}</p>}
<form (ngSubmit)="save()"><div class="form-grid">
@for(field of meta?.fields || [];track field.key){
  @if(field.type==='checkbox'){
    <label class="check-label"><input type="checkbox" [name]="field.key" [(ngModel)]="editing[field.key]">{{field.label}}</label>
  }@else{
    <div class="field"><label [for]="'catalog-'+field.key">{{field.label}} @if(field.required){<span class="required">*</span>}</label>
      @if(field.type==='select'){
        <select [id]="'catalog-'+field.key" [name]="field.key" [(ngModel)]="editing[field.key]">
          <option [ngValue]="null">— None —</option>
          @for(option of options[field.options || ''] || [];track option['id']){<option [ngValue]="option['id']">{{option['name']}} {{option['number'] ? '('+option['number']+')':''}}</option>}
        </select>
      }@else if(field.type==='textarea'){
        <textarea [id]="'catalog-'+field.key" [name]="field.key" [(ngModel)]="editing[field.key]" rows="3"></textarea>
      }@else{
        <input [id]="'catalog-'+field.key" [name]="field.key" [type]="field.type || 'text'" [(ngModel)]="editing[field.key]" [required]="!!field.required" [attr.min]="field.type==='number'?'0':null" [attr.step]="field.type==='number'?'any':null" [attr.maxlength]="field.type==='number'?null:200">
      }
    </div>
  }
}
</div><div class="form-actions"><button class="button secondary" type="button" (click)="close()">Cancel</button><button class="button" type="submit" [disabled]="saving">{{saving?'Saving…':'Save changes'}}</button></div></form>
</div></dialog>
`})
export class CatalogComponent {
  readonly api=inject(ApiService);readonly auth=inject(AuthService);private toast=inject(ToastService);private confirm=inject(ConfirmService); readonly display=display;readonly humanize=humanize;
  private route=inject(ActivatedRoute);private destroyRef=inject(DestroyRef);
  @ViewChild('editDialog') editDialog!:ElementRef<HTMLDialogElement>;
  meta:CatalogDefinition|undefined;rows:Item[]=[];filtered:Item[]=[];search='';page=1;pageSize=20;totalCount=0;sortBy='name';descending=false;loading=false;error='';saveError='';saving=false;editing:Item={};options:Record<string,Item[]>={};
  constructor(){this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params=>{
    this.meta=CATALOGS.find(x=>x.key===params.get('key'));this.search='';this.page=1;this.sortBy='name';this.descending=false;void this.load();
  });}
  canWrite(){return !!this.meta && this.auth.can(this.meta.permission+'.Write');}
  async load(notify=false){if(!this.meta)return;this.loading=true;this.error='';try{
    const result=await this.api.get<PageResult<Item>>(`/catalog-pages/${this.meta.key}`,{page:this.page,pageSize:this.pageSize,search:this.search,sortBy:this.sortBy,descending:this.descending});
    this.rows=result.items;this.filtered=result.items;this.totalCount=result.totalCount;if(notify)this.toast.info(`${this.meta.title} records refreshed.`);
  }catch(e){this.error=errorMessage(e);this.rows=[];this.filtered=[];}finally{this.loading=false;}}
  pages(){return Math.max(1,Math.ceil(this.totalCount/this.pageSize));}
  applySearch(){this.page=1;void this.load();}
  isSortable(column:string){return ['id','name','code','number','buyingPrice','sellingPrice','minimumStockLevel'].includes(column);}
  sort(column:string){if(!this.isSortable(column))return;if(this.sortBy===column)this.descending=!this.descending;else{this.sortBy=column;this.descending=false;}this.page=1;void this.load();}
  async openEditor(row?:Item){if(!this.canWrite()||!this.meta)return;
    this.editing={};for(const field of this.meta.fields) this.editing[field.key]=row?.[field.key] ?? field.defaultValue ?? (field.type==='select'?null:field.type==='number'?0:field.type==='checkbox'?false:'');
    if(row)this.editing['id']=row['id'];this.saveError='';this.options={};
    for(const field of this.meta.fields.filter(f=>f.type==='select'&&f.options)){
      const key=field.options!; const info=CATALOGS.find(x=>x.key===key);
      if(info && this.auth.can(info.permission+'.Read')){try{this.options[key]=await this.api.get<Item[]>(info.endpoint);}catch{this.options[key]=[];}}
    }
    this.editDialog.nativeElement.showModal();
  }
  close(){this.editDialog.nativeElement.close();}
  async save(){if(!this.meta||!this.canWrite())return;this.saveError='';this.saving=true;
    const payload:Item={};for(const field of this.meta.fields){const value=this.editing[field.key];payload[field.key]=value===''?null:value;}
    try{
      const updating=Boolean(this.editing['id']);
      if(updating)await this.api.put(`${this.meta.endpoint}/${this.editing['id']}`,payload);
      else await this.api.post(this.meta.endpoint,payload);
      this.close();this.toast.success(`${this.meta.title} ${updating?'updated':'created'} successfully.`);await this.load();
    }catch(e){this.saveError=errorMessage(e);}finally{this.saving=false;}
  }
  async remove(row:Item){if(!this.meta||!this.canWrite())return;
    if(!await this.confirm.ask(`Are you sure you want to ${this.meta.key==='medicines'?'deactivate':'delete'} ${row['name']||row['code']}?`, 'Confirm catalog change',this.meta.key==='medicines'?'Deactivate':'Delete'))return;
    try{await this.api.delete(`${this.meta.endpoint}/${row['id']}`);this.toast.success(`${this.meta.title} ${this.meta.key==='medicines'?'deactivated':'deleted'} successfully.`);await this.load();}catch(e){this.error=errorMessage(e);}
  }
}

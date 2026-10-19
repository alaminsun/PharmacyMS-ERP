import { Component, ElementRef, OnInit, ViewChild, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, errorMessage } from '../core/api.service';
import { AuthService } from '../core/auth.service';
import { TeamRole } from '../core/models';
import { ToastService } from '../core/toast.service';
@Component({selector:'app-roles',standalone:true,imports:[FormsModule],template:`
<div class="page-heading"><div><p class="eyebrow">ADMINISTRATION</p><h1>Roles & permissions</h1><p class="muted">Assign fine-grained permissions without editing application code.</p></div>
<button class="button" (click)="openNew()">+ Create role</button></div>
@if(error){<p class="form-error" role="alert">{{error}}</p>}
@if(success){<p class="success-text" role="status">{{success}}</p>}
<section class="panel"><div class="panel-heading"><div><h2>Access roles</h2><p class="muted">Default roles are available, but permissions can be managed dynamically.</p></div><span class="badge">{{roles.length}} roles</span></div>
@if(loading){<div class="skeleton-wrap"><div class="skeleton" style="height:130px"></div></div>}
@else if(roles.length){<div class="table-wrap"><table><thead><tr><th>Role</th><th>Permissions</th><th>Actions</th></tr></thead><tbody>
@for(role of roles;track role.name){<tr><td><strong>{{role.name}}</strong></td><td><span class="badge">{{role.permissions.length}} permissions</span></td><td>
<button class="button small secondary" (click)="openRole(role)">{{role.name.toLowerCase()==='admin'?'View protected role':'Manage permissions'}}</button></td></tr>}
</tbody></table></div>}@else{<div class="empty-state compact">No roles found. Create one to begin.</div>}</section>
<dialog class="wide" #roleDialog><div id="modal-content"><div class="modal-header"><div><p class="eyebrow">ACCESS CONTROL</p><h2>{{existing?'Edit role permissions':'Create a role'}}</h2><p class="muted">The .NET Backend authorizes requests using effective permissions stored in the database.</p></div>
<button class="icon-button" aria-label="Close" type="button" (click)="close()">×</button></div>
@if(saveError){<p role="alert" class="form-error">{{saveError}}</p>}
<form (ngSubmit)="save()"><div class="field"><label for="roleName">Role name *</label><input id="roleName" name="name" [(ngModel)]="roleName" required maxlength="100" [disabled]="!!existing"></div>
<h3 class="section-title">Permissions</h3>
<div class="permissions-grid">
@for(group of groups;track group.name){<fieldset class="permission-group"><legend>{{group.name}}</legend>
@for(permission of group.values;track permission){<label class="check-label"><input type="checkbox" [checked]="selected.has(permission)" [disabled]="roleName.toLowerCase()==='admin'" (change)="toggle(permission,$event)">{{permission.split('.')[1]}}</label>}
</fieldset>}
</div>
@if(roleName.toLowerCase()==='admin'){<p class="form-hint">The Admin role is protected by the Backend. Its permissions cannot be edited here.</p>}
<div class="form-actions"><button class="button secondary" type="button" (click)="close()">Cancel</button>
@if(roleName.toLowerCase()!=='admin'){<button class="button" type="submit" [disabled]="saving">{{saving?'Saving…':'Save permissions'}}</button>}</div>
</form></div></dialog>
`})
export class RolesComponent implements OnInit {
  readonly api=inject(ApiService);readonly auth=inject(AuthService);private toast=inject(ToastService);
  @ViewChild('roleDialog') roleDialog!:ElementRef<HTMLDialogElement>;
  roles:TeamRole[]=[];allPermissions:string[]=[];existing:TeamRole|null=null;roleName='';selected=new Set<string>();loading=true;saving=false;error='';saveError='';success='';
  ngOnInit(){void this.load();}
  async load(){this.error='';this.loading=true;try{[this.roles,this.allPermissions]=await Promise.all([
    this.api.get<TeamRole[]>('/admin/roles'),this.api.get<string[]>('/admin/roles/permissions')
  ]);}catch(e){this.error=errorMessage(e);}finally{this.loading=false;}}
  get groups(){const groups=new Map<string,string[]>();for(const p of this.allPermissions){const module=p.split('.')[0];groups.set(module,[...(groups.get(module)||[]),p]);}
    return [...groups].map(([name,values])=>({name,values}));}
  openNew(){this.existing=null;this.roleName='';this.selected=new Set();this.saveError='';this.roleDialog.nativeElement.showModal();}
  openRole(role:TeamRole){this.existing=role;this.roleName=role.name;this.selected=new Set(role.permissions);this.saveError='';this.roleDialog.nativeElement.showModal();}
  close(){this.roleDialog.nativeElement.close();}
  toggle(permission:string,event:Event){const checked=(event.target as HTMLInputElement).checked;
    if(checked)this.selected.add(permission);else this.selected.delete(permission);this.selected=new Set(this.selected);}
  async save(){if(!this.roleName.trim()||this.roleName.toLowerCase()==='admin')return;this.saving=true;this.saveError='';this.success='';
    const name=this.roleName.trim();const before=new Set(this.existing?.permissions||[]);
    try{
      if(!this.existing)await this.api.post('/admin/roles',{name});
      const revoke=[...before].filter(p=>!this.selected.has(p));
      const grant=[...this.selected].filter(p=>!before.has(p));
      for(const permission of revoke)await this.api.post('/admin/roles/revoke-permission',{role:name,permission});
      for(const permission of grant)await this.api.post('/admin/roles/grant-permission',{role:name,permission});
      this.close();this.success=`Saved ${name}. Permission changes are effective on the next API request.`;this.toast.success(this.success);await this.load();await this.auth.reloadAccess();
    }catch(e){this.saveError=errorMessage(e)+' Changes already applied remain saved; refresh the role before retrying.';this.toast.warning('Some role changes may have been saved. Refresh before retrying.');await this.load();}
    finally{this.saving=false;}
  }
}

import { Component, ElementRef, OnInit, ViewChild, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiService, errorMessage } from '../core/api.service';
import { AuthService } from '../core/auth.service';
import { TeamRole, TeamUser } from '../core/models';
import { ConfirmService } from '../core/confirm.service';
import { ToastService } from '../core/toast.service';
@Component({selector:'app-users',standalone:true,imports:[FormsModule],template:`
<div class="page-heading"><div><p class="eyebrow">ADMINISTRATION</p><h1>Team members</h1><p class="muted">Create accounts, assign and remove roles, or change activation status.</p></div><button class="button" (click)="openNew()">+ Add team member</button></div>
@if(error){<p class="form-error" role="alert">{{error}}</p>}
@if(success){<p class="success-text" role="status">{{success}}</p>}
<section class="panel"><div class="panel-heading"><div><h2>People & access</h2><p class="muted">Role changes revoke a user's old JWT on the server.</p></div><span class="badge">{{users.length}} members</span></div>
@if(loading){<div class="skeleton-wrap"><div class="skeleton" style="height:130px"></div></div>}
@else if(users.length){<div class="table-wrap"><table><thead><tr><th>Email address</th><th>Roles</th><th>Status</th><th>Actions</th></tr></thead><tbody>
@for(user of users;track user.id){<tr><td>{{user.email}}</td><td><div class="badge-list">@for(role of user.roles;track role){<span class="badge">{{role}}</span>}</div></td>
<td><span class="badge" [class.lime]="user.isActive">{{user.isActive?'Active':'Inactive'}}</span></td><td>
<button class="button small secondary" (click)="openUser(user)">Manage access</button></td></tr>}
</tbody></table></div>}@else{<div class="empty-state compact">No users yet</div>}</section>
<dialog #userDialog><div id="modal-content"><div class="modal-header"><div><p class="eyebrow">PHARMACY TEAM</p><h2>{{existing?'Manage team access':'Add a team member'}}</h2><p class="muted">Role and activation changes apply to backend security checks.</p></div><button class="icon-button" aria-label="Close" type="button" (click)="close()">×</button></div>
@if(saveError){<p class="form-error" role="alert">{{saveError}}</p>}
@if(!existing){<form (ngSubmit)="create()"><div class="form-grid">
<div class="field"><label for="email">Email *</label><input id="email" type="email" name="email" [(ngModel)]="email" required autocomplete="off"></div>
<div class="field"><label for="password">Temporary password *</label><input id="password" type="password" name="password" [(ngModel)]="password" required minlength="12" autocomplete="new-password"></div>
<div class="field"><label for="newRole">Initial role *</label><select id="newRole" name="role" [(ngModel)]="newRole" required><option value="">Select role</option>@for(role of roles;track role.name){<option [value]="role.name">{{role.name}}</option>}</select></div>
</div><div class="form-actions"><button class="button secondary" type="button" (click)="close()">Cancel</button><button class="button" [disabled]="saving" type="submit">Create account</button></div></form>}
@else{
<div class="field"><label>Account</label><input readonly [value]="existing.email"></div>
<div class="form-grid"><div class="field"><label for="assign">Assign role</label><select id="assign" name="assign" [(ngModel)]="assignRole"><option value="">Select role</option>
@for(role of roles;track role.name){@if(!existing.roles.includes(role.name)){<option [value]="role.name">{{role.name}}</option>}}</select></div><div class="field"><label>&nbsp;</label><button class="button secondary" [disabled]="!assignRole||saving" (click)="assign()">Assign role</button></div></div>
<h3 class="section-title">Current roles</h3><div class="badge-list" style="margin:16px 0">
@for(role of existing.roles;track role){<span class="badge">{{role}} <button type="button" class="pill-remove" [disabled]="saving" [attr.aria-label]="'Remove role '+role" (click)="removeRole(role)">×</button></span>}
</div>
<div class="form-actions" style="justify-content:space-between"><button class="button" [class.danger]="existing.isActive" [disabled]="saving" (click)="setActive(!existing.isActive)">{{existing.isActive?'Deactivate account':'Activate account'}}</button>
<button class="button secondary" type="button" (click)="close()">Close</button></div>
}
</div></dialog>
`})
export class UsersComponent implements OnInit {
  readonly api=inject(ApiService); readonly auth=inject(AuthService);private toast=inject(ToastService);private confirm=inject(ConfirmService);
  @ViewChild('userDialog') userDialog!:ElementRef<HTMLDialogElement>;
  users:TeamUser[]=[];roles:TeamRole[]=[];existing:TeamUser|null=null;
  email='';password='';newRole='';assignRole='';loading=true;saving=false;error='';saveError='';success='';
  ngOnInit(){void this.load();}
  async load(){this.error='';this.loading=true;try{[this.users,this.roles]=await Promise.all([
    this.api.get<TeamUser[]>('/admin/users'),this.api.get<TeamRole[]>('/admin/roles')
  ]);}catch(e){this.error=errorMessage(e);}finally{this.loading=false;}}
  openNew(){this.existing=null;this.email='';this.password='';this.newRole='';this.saveError='';this.userDialog.nativeElement.showModal();}
  openUser(user:TeamUser){this.existing={...user,roles:[...user.roles]};this.assignRole='';this.saveError='';this.userDialog.nativeElement.showModal();}
  close(){this.password='';this.userDialog.nativeElement.close();}
  async create(){this.saveError='';this.saving=true;try{
    await this.api.post('/admin/users',{email:this.email.trim(),password:this.password,role:this.newRole});this.close();this.success='User created.';this.toast.success(`Account ${this.email.trim()} created successfully.`);await this.load();
  }catch(e){this.saveError=errorMessage(e);}finally{this.saving=false;}}
  async assign(){if(!this.existing||!this.assignRole)return;this.saving=true;this.saveError='';try{
    await this.api.post('/admin/users/assign-role',{email:this.existing.email,role:this.assignRole});this.toast.success(`${this.assignRole} role assigned to ${this.existing.email}.`);await this.refreshUser();
  }catch(e){this.saveError=errorMessage(e);}finally{this.saving=false;}}
  async removeRole(role:string){if(!this.existing||!await this.confirm.ask(`Remove ${role} role from ${this.existing.email}?`,'Remove role','Remove'))return;
    this.saving=true;this.saveError='';try{await this.api.post('/admin/users/remove-role',{email:this.existing.email,role});this.toast.success(`${role} role removed from ${this.existing.email}.`);await this.refreshUser();}
    catch(e){this.saveError=errorMessage(e);}finally{this.saving=false;}}
  async setActive(active:boolean){if(!this.existing||!await this.confirm.ask(`${active?'Activate':'Deactivate'} ${this.existing.email}?`,'Change account status',active?'Activate':'Deactivate',!active))return;
    this.saving=true;this.saveError='';try{await this.api.put('/admin/users/status',{email:this.existing.email,isActive:active});this.toast.success(`Account ${this.existing.email} ${active?'activated':'deactivated'}.`);await this.refreshUser();}
    catch(e){this.saveError=errorMessage(e);}finally{this.saving=false;}}
  private async refreshUser(){const email=this.existing?.email;await this.load();
    this.existing=this.users.find(u=>u.email===email)||null;this.assignRole='';
    if(email===this.auth.access()?.email){await this.auth.reloadAccess().catch(()=>this.auth.logout());}
  }
}

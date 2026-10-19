import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';
import { AuthService } from '../core/auth.service';
import { CATALOGS } from '../core/catalog';
import { ToastService } from '../core/toast.service';
@Component({selector:'app-shell',standalone:true,imports:[RouterLink,RouterLinkActive,RouterOutlet],template:`
<div class="workspace">
  <aside class="sidebar" id="sidebar" [class.open]="open()">
    <a class="brand" routerLink="/welcome"><span class="brand-mark">+</span><span>pharmacy<small>WORKSPACE</small></span></a>
    <nav aria-label="Main navigation">
      <p class="nav-group">WORKSPACE</p>
      <a class="nav-link" routerLink="/welcome" routerLinkActive="active" (click)="open.set(false)"><span class="nav-symbol">⌂</span>Welcome</a>
      @if(auth.can('Dashboard.Read')){<a class="nav-link" routerLink="/dashboard" routerLinkActive="active" (click)="open.set(false)"><span class="nav-symbol">◫</span>Overview</a>}
      @if(auth.can('Sales.Read')){<a class="nav-link" routerLink="/sales" routerLinkActive="active" (click)="open.set(false)"><span class="nav-symbol">↗</span>Sales</a>}
      @if(auth.can('Purchases.Read')){<a class="nav-link" routerLink="/purchases" routerLinkActive="active" (click)="open.set(false)"><span class="nav-symbol">↙</span>Purchases</a>}
      @if(auth.can('Stock.Read')){<a class="nav-link" routerLink="/stock" routerLinkActive="active" (click)="open.set(false)"><span class="nav-symbol">▤</span>Stock overview</a>}
      @if(auth.can('Reports.Read')){<a class="nav-link" routerLink="/reports" routerLinkActive="active" (click)="open.set(false)"><span class="nav-symbol">▥</span>Reports</a>}
      <p class="nav-group">CATALOG</p>
      @for(meta of catalogs;track meta.key){@if(auth.can(meta.permission+'.Read')){
        <a class="nav-link" [routerLink]="['/catalog',meta.key]" routerLinkActive="active" (click)="open.set(false)"><span class="nav-symbol">○</span>{{meta.title}}</a>
      }}
      @if(auth.can('Users.Manage')||auth.can('Roles.Manage')){<p class="nav-group">ADMINISTRATION</p>}
      @if(auth.can('Users.Manage')){<a class="nav-link" routerLink="/users" routerLinkActive="active" (click)="open.set(false)"><span class="nav-symbol">♧</span>Team members</a>}
      @if(auth.can('Users.Manage')){<a class="nav-link" routerLink="/audit" routerLinkActive="active" (click)="open.set(false)"><span class="nav-symbol">◷</span>Audit history</a>}
      @if(auth.can('Roles.Manage')){<a class="nav-link" routerLink="/roles" routerLinkActive="active" (click)="open.set(false)"><span class="nav-symbol">◇</span>Roles & permissions</a>}
    </nav>
    <div class="sidebar-bottom"><p class="eyebrow">SIGNED IN AS</p><strong>{{auth.access()?.email}}</strong>
      <small>{{auth.access()?.roles?.join(' · ') || 'Team member'}}</small>
      <button class="signout" (click)="signOut()">Sign out ↗</button>
    </div>
  </aside>
  <div class="main-area"><header class="topbar">
    <div class="breadcrumb"><button class="icon-button menu-toggle" aria-label="Toggle menu" [attr.aria-expanded]="open()" aria-controls="sidebar" (click)="open.set(!open())">☰</button>
    <span>Workspace</span><span class="separator">/</span><strong>{{page}}</strong></div>
    <div class="topbar-right"><span class="today">{{today}}</span><span class="avatar" [title]="auth.access()?.email || ''">{{(auth.access()?.email || 'U').slice(0,1).toUpperCase()}}</span></div>
  </header><main id="content"><router-outlet/></main><footer class="page-footer">Pharmacy Workspace<span>A little more clarity. Every day.</span></footer></div>
</div>
`})
export class ShellComponent {
  readonly auth = inject(AuthService); readonly open=signal(false); readonly catalogs=CATALOGS;
  private router=inject(Router);private toast=inject(ToastService);
  page='Welcome';today=new Date().toLocaleDateString(undefined,{weekday:'short',day:'numeric',month:'short',year:'numeric'});
  signOut(){this.auth.logout();this.toast.info('You have signed out.');}
  constructor(){
    this.router.events.pipe(filter((event: unknown):event is NavigationEnd=> event instanceof NavigationEnd)).subscribe(()=>{
      const path=this.router.url.split('/').filter(Boolean);const last=path.at(-1)||'welcome';
      this.page=CATALOGS.find(x=>x.key===last)?.title || last[0].toUpperCase()+last.slice(1);
      this.open.set(false);
    });
  }
}

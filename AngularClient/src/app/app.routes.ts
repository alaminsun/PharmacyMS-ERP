import { Routes, CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from './core/auth.service';
import { CATALOGS } from './core/catalog';

const secure: CanActivateFn = async (route) => {
  const auth = inject(AuthService); const router = inject(Router);
  if (auth.initializing()) await auth.initialize();
  if (!auth.authenticated()) return router.parseUrl('/login');
  const permission: string | undefined = route.data['permission'];
  if (permission && !auth.can(permission)) return router.parseUrl('/welcome');
  if(route.routeConfig?.path==='receipt/:kind/:id'){const kind=route.params['kind'];if(!['sales','purchases'].includes(kind)||!auth.can(kind==='sales'?'Sales.Read':'Purchases.Read'))return router.parseUrl('/welcome');}
  const key = route.params['key'];
  if (key) {
    const meta = CATALOGS.find(x => x.key === key);
    if (!meta || !auth.can(`${meta.permission}.Read`)) return router.parseUrl('/welcome');
  }
  return true;
};
export const appRoutes: Routes = [
  {path:'login',loadComponent:()=>import('./pages/login.component').then(m=>m.LoginComponent)},
  {path:'',loadComponent:()=>import('./pages/shell.component').then(m=>m.ShellComponent),canActivate:[secure],children:[
    {path:'welcome',loadComponent:()=>import('./pages/welcome.component').then(m=>m.WelcomeComponent)},
    {path:'dashboard',data:{permission:'Dashboard.Read'},canActivate:[secure],loadComponent:()=>import('./pages/dashboard.component').then(m=>m.DashboardComponent)},
    {path:'catalog/:key',canActivate:[secure],loadComponent:()=>import('./pages/catalog.component').then(m=>m.CatalogComponent)},
    {path:'stock',data:{permission:'Stock.Read'},canActivate:[secure],loadComponent:()=>import('./pages/stock.component').then(m=>m.StockComponent)},
    {path:'sales',data:{permission:'Sales.Read'},canActivate:[secure],loadComponent:()=>import('./pages/transactions.component').then(m=>m.TransactionsComponent)},
    {path:'purchases',data:{permission:'Purchases.Read'},canActivate:[secure],loadComponent:()=>import('./pages/transactions.component').then(m=>m.TransactionsComponent)},
    {path:'receipt/:kind/:id',canActivate:[secure],loadComponent:()=>import('./pages/receipt.component').then(m=>m.ReceiptComponent)},
    {path:'audit',data:{permission:'Users.Manage'},canActivate:[secure],loadComponent:()=>import('./pages/audit.component').then(m=>m.AuditComponent)},
    {path:'reports',data:{permission:'Reports.Read'},canActivate:[secure],loadComponent:()=>import('./pages/reports.component').then(m=>m.ReportsComponent)},
    {path:'users',data:{permission:'Users.Manage'},canActivate:[secure],loadComponent:()=>import('./pages/users.component').then(m=>m.UsersComponent)},
    {path:'roles',data:{permission:'Roles.Manage'},canActivate:[secure],loadComponent:()=>import('./pages/roles.component').then(m=>m.RolesComponent)},
    {path:'',pathMatch:'full',redirectTo:'welcome'}
  ]},
  {path:'**',redirectTo:'welcome'}
];

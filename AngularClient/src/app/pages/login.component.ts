import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../core/auth.service';
import { errorMessage } from '../core/api.service';
import { ToastService } from '../core/toast.service';
@Component({selector:'app-login',standalone:true,imports:[FormsModule],template:`
<main class="login-layout">
  <section class="login-intro">
    <div class="brand"><span class="brand-mark">+</span><span>pharmacy<small>WORKSPACE</small></span></div>
    <div><p class="eyebrow">CARE STARTS WITH CLARITY</p><h1>A healthier way<br>to manage<br>your pharmacy.</h1>
    <p class="intro-description">Inventory, purchases, sales and your team. One organized workspace for the everyday work of care.</p></div>
    <div class="intro-footer"><span class="status-dot"></span>Your pharmacy. Connected.</div>
  </section>
  <section class="login-main"><div class="login-card"><span class="badge lime">WELCOME BACK</span>
    <h2>Sign in to your workspace</h2><p class="muted">Use the account provided by your administrator.</p>
    @if (error) { <p class="form-error" role="alert">{{error}}</p> }
    <form class="login-form" (ngSubmit)="signIn()">
      <div class="field"><label for="email">Email address <span class="required">*</span></label>
      <input id="email" type="email" name="email" required autocomplete="username" [(ngModel)]="email" placeholder="you@pharmacy.com"></div>
      <div class="field"><label for="password">Password <span class="required">*</span></label>
      <input id="password" type="password" name="password" required autocomplete="current-password" [(ngModel)]="password" placeholder="Enter your password"></div>
      <button class="button full" type="submit" [disabled]="submitting">{{submitting ? 'Signing in…' : 'Sign in →'}}</button>
    </form><p class="login-note">Access is based on the permissions assigned by your administrator.</p>
  </div></section>
</main>`})
export class LoginComponent {
  private auth = inject(AuthService); private router=inject(Router); private toast=inject(ToastService);
  email='';password='';error='';submitting=false;
  async signIn(){
    this.error='';this.submitting=true;
    try {await this.auth.login(this.email.trim(),this.password);this.password=''; await this.router.navigateByUrl(this.auth.can('Dashboard.Read')?'/dashboard':'/welcome');this.toast.success('Signed in successfully. Welcome back!');}
    catch(error){this.error=errorMessage(error);this.password='';}
    finally{this.submitting=false;}
  }
}

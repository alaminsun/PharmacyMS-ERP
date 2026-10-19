import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../core/auth.service';
@Component({selector:'app-welcome',standalone:true,imports:[RouterLink],template:`
<div class="page-heading"><div><p class="eyebrow">YOUR PHARMACY, YOUR WORKSPACE</p><h1>Welcome back</h1><p class="muted">Your permitted modules appear in the left navigation.</p></div></div>
<section class="welcome-banner"><div><span class="badge lime">READY FOR THE DAY</span><h2>Good care starts<br>with a clear overview.</h2><p>Keep your shelves ready and your daily work moving.</p>
<div class="quick-actions">
@if(auth.can('Dashboard.Read')){<a class="button" routerLink="/dashboard">Open dashboard →</a>}
@if(auth.can('Sales.Read')){<a class="button secondary" routerLink="/sales">View sales →</a>}
</div></div><div class="hero-graphic" aria-hidden="true"><div class="graphic-cross">+</div><span class="graphic-pill pill-one"></span><span class="graphic-pill pill-two"></span><span class="graphic-orbit"></span></div></section>
<section class="panel"><div class="panel-heading"><div><h2>Signed in successfully</h2><p class="muted">Permissions are enforced by your .NET API, not just hidden buttons.</p></div><span class="badge lime">{{auth.access()?.roles?.join(', ') || 'Team member'}}</span></div></section>`})
export class WelcomeComponent{readonly auth=inject(AuthService);}

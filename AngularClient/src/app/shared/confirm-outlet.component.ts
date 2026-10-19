import { Component, HostListener, inject } from '@angular/core';
import { ConfirmService } from '../core/confirm.service';
@Component({selector:'app-confirm-outlet',standalone:true,template:`
@if(confirm.current();as request){
<div class="confirm-backdrop" (click)="confirm.cancel()">
<section class="confirm-card" role="alertdialog" aria-modal="true" aria-labelledby="confirm-title" aria-describedby="confirm-body" (click)="$event.stopPropagation()">
<div class="confirm-mark" aria-hidden="true">!</div>
<h2 id="confirm-title">{{request.title}}</h2>
<p id="confirm-body">{{request.message}}</p>
<div class="confirm-actions"><button class="button secondary" type="button" (click)="confirm.cancel()">Cancel</button>
<button #confirmButton class="button" [class.danger]="request.danger" type="button" (click)="confirm.answer(true)" autofocus>{{request.confirmLabel}}</button></div>
</section></div>
}`})
export class ConfirmOutletComponent {
  readonly confirm=inject(ConfirmService);
  @HostListener('document:keydown.escape') onEscape(){if(this.confirm.current())this.confirm.cancel();}
}

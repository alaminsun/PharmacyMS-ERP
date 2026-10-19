import { Injectable, signal } from '@angular/core';
export interface ConfirmRequest { title:string; message:string; confirmLabel:string; danger:boolean; }
@Injectable({providedIn:'root'})
export class ConfirmService {
  readonly current=signal<ConfirmRequest|null>(null);
  private resolve: ((result:boolean)=>void)|null=null;
  ask(message:string, title='Confirm action', confirmLabel='Confirm', danger=true):Promise<boolean> {
    if(this.resolve)this.cancel();
    this.current.set({title,message,confirmLabel,danger});
    return new Promise<boolean>(resolve=>this.resolve=resolve);
  }
  answer(value:boolean):void { const resolve=this.resolve;this.resolve=null;this.current.set(null);resolve?.(value); }
  cancel():void {this.answer(false);}
}

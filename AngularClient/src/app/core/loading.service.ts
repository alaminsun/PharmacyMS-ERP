import { Injectable, computed, signal } from '@angular/core';
@Injectable({providedIn:'root'})
export class LoadingService {
  private readonly count=signal(0);
  readonly busy=computed(()=>this.count()>0);
  begin(){this.count.update(x=>x+1);}
  end(){this.count.update(x=>Math.max(0,x-1));}
}

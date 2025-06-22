// loading.service.ts
import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class LoadingService {
  private loadingSubject = new BehaviorSubject(false);
  loading$ = this.loadingSubject.asObservable();

  private requestCount = 0;
  private delay = 300; // ms
  private delayTimeout: any;

  show() {
    this.requestCount++;
    console.log('[LoadingService] show() → count:', this.requestCount);
    if (this.requestCount === 1) {
      this.delayTimeout = setTimeout(() => {
        this.loadingSubject.next(true);
      }, this.delay);
    }
  }

  hide() {
    this.requestCount = Math.max(this.requestCount - 1, 0);
    if (this.requestCount === 0) {
      clearTimeout(this.delayTimeout);
      this.loadingSubject.next(false);
    }
  }
}

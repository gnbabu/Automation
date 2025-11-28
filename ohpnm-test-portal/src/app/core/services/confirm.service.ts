import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ConfirmService {
  private confirmState = new Subject<{
    title: string;
    message: string;
  }>();
  private resolver?: (value: boolean) => void;

  confirm(title: string, message: string): Promise<boolean> {
    return new Promise<boolean>((resolve) => {
      this.resolver = resolve;
      this.confirmState.next({ title, message });
    });
  }

  onConfirmState() {
    return this.confirmState.asObservable();
  }

  resolve(result: boolean) {
    if (this.resolver) {
      this.resolver(result);
      this.resolver = undefined;
    }
  }
}

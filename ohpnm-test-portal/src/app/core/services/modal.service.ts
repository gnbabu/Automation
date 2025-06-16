import { Injectable } from '@angular/core';
import * as bootstrap from 'bootstrap';

@Injectable({
  providedIn: 'root'
})
export class ModalService {
  private modals: { [id: string]: bootstrap.Modal } = {};

  register(id: string, element: HTMLElement): void {
    if (!this.modals[id]) {
      this.modals[id] = new bootstrap.Modal(element, { backdrop: 'static' });
    }
  }

  open(id: string): void {
    const modal = this.modals[id];
    if (modal) {
      modal.show();
    } else {
      console.warn(`Modal with id '${id}' not registered.`);
    }
  }

  close(id: string): void {
    const modal = this.modals[id];
    if (modal) {
      modal.hide();
    }
  }
}

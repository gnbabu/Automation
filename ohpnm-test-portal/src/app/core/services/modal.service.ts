import { Injectable } from '@angular/core';
import * as bootstrap from 'bootstrap';

@Injectable({
  providedIn: 'root'
})
export class ModalService {
  private modals: { [id: string]: bootstrap.Modal } = {};

  register(id: string, element: HTMLElement): void {
    // Always (re-)create, rather than "only if not already registered" - this service is
    // a root-provided singleton that outlives any single page visit, but modal host
    // components (e.g. ScheduleTestcasesDialogComponent on the lazily-loaded, routed
    // test-case-execution-panel page) are destroyed/recreated every time their page is
    // navigated away from and back to. The old guard meant that after the *first* visit,
    // every later registration under the same id was silently skipped - leaving the
    // freshly-rendered element never wired to any bootstrap.Modal controller at all, while
    // open()/close() kept operating on the previous visit's now-detached element/instance
    // (which is exactly why a dialog on a routed page could become impossible to close,
    // or misbehave, the second time its page was visited). Dispose the previous instance
    // first so it doesn't leak a stale backdrop/event listeners.
    this.modals[id]?.dispose();
    this.modals[id] = new bootstrap.Modal(element, { backdrop: 'static' });
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

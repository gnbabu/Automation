import {
  Component,
  ElementRef,
  ViewChild,
  AfterViewInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ILoginUserModel } from '@interfaces';
import { ModalService } from '@services';
import { AppDropdownComponent } from 'app/core/components/app-dropdown/app-dropdown.component';

// Now always opens for Run Now/Bulk Run Now (see test-case-execution-panel.component.
// ts's onRunNow/onBulkRunNow) - previously only shown when the environment being run
// against had RequiresAuthentication = true, with no dialog (and no way to pick a
// browser) at all otherwise. The Login User field stays conditional on loginUsers being
// non-empty; Browser is always offered. Mirrors ScheduleTestcasesDialogComponent's
// ModalService open(callback)/submit() pattern.
@Component({
  selector: 'app-run-now-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule, AppDropdownComponent],
  styleUrl: './run-now-dialog.component.css',
  templateUrl: './run-now-dialog.component.html',
})
export class RunNowDialogComponent implements AfterViewInit {
  @ViewChild('runNowModal') modalElement!: ElementRef;

  loginUsers: ILoginUserModel[] = [];
  loginUserId: number | null = null;
  browser: string = 'Chrome';
  browserOptions = ['Chrome', 'Edge'];
  // Options here are plain strings, not objects - AppDropdownComponent's default
  // textAccessor ('name') would look up a `.name` field that doesn't exist on a
  // string, so this just returns the option itself.
  identityTextAccessor = (opt: string) => opt;

  private callback!: (data: { loginUserId?: number; browser: string }) => void;

  constructor(private modalService: ModalService) {}

  ngAfterViewInit() {
    setTimeout(() => {
      this.modalService.register('runNowModal', this.modalElement.nativeElement);
    });
  }

  /** Open modal with this environment's active login users (empty if auth isn't
   * required) and pass a callback. */
  open(
    loginUsers: ILoginUserModel[],
    cb: (data: { loginUserId?: number; browser: string }) => void
  ) {
    this.loginUsers = loginUsers.filter((lu) => lu.isActive);
    this.loginUserId = null;
    this.browser = 'Chrome';
    this.callback = cb;
    this.modalService.open('runNowModal');
  }

  close() {
    this.modalService.close('runNowModal');
  }

  get requiresLoginUser(): boolean {
    return this.loginUsers.length > 0;
  }

  submit() {
    if (this.requiresLoginUser && !this.loginUserId) return;

    if (this.callback) {
      this.callback({
        loginUserId: this.loginUserId ?? undefined,
        browser: this.browser,
      });
    }

    this.loginUserId = null;
    this.close();
  }

  // Self-service - every entry is always the current user's own credential, so no
  // portalUserName suffix is needed anymore (was previously used to distinguish whose
  // credential was whose when everyone's credentials were shown together).
  loginUserLabel(lu: ILoginUserModel): string {
    return `${lu.userRole} - ${lu.userName}`;
  }
}

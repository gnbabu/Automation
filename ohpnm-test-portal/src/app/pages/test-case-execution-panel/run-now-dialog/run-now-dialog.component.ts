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

// Shown only when the environment being run against has RequiresAuthentication = true
// (see test-case-execution-panel.component.ts's onRunNow/onBulkRunNow) - "Run Now" has
// no dialog at all otherwise, unchanged from before this feature existed. Mirrors
// ScheduleTestcasesDialogComponent's ModalService open(callback)/submit() pattern.
@Component({
  selector: 'app-run-now-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  styleUrl: './run-now-dialog.component.css',
  templateUrl: './run-now-dialog.component.html',
})
export class RunNowDialogComponent implements AfterViewInit {
  @ViewChild('runNowModal') modalElement!: ElementRef;

  loginUsers: ILoginUserModel[] = [];
  loginUserId: number | null = null;

  private callback!: (data: { loginUserId: number }) => void;

  constructor(private modalService: ModalService) {}

  ngAfterViewInit() {
    setTimeout(() => {
      this.modalService.register('runNowModal', this.modalElement.nativeElement);
    });
  }

  /** Open modal with this environment's active login users and pass a callback. */
  open(loginUsers: ILoginUserModel[], cb: (data: { loginUserId: number }) => void) {
    this.loginUsers = loginUsers.filter((lu) => lu.isActive);
    this.loginUserId = null;
    this.callback = cb;
    this.modalService.open('runNowModal');
  }

  close() {
    this.modalService.close('runNowModal');
  }

  submit() {
    if (!this.loginUserId) return;

    if (this.callback) {
      this.callback({ loginUserId: this.loginUserId });
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

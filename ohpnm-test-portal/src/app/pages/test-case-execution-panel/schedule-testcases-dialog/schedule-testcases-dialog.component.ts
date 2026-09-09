import {
  Component,
  ElementRef,
  EventEmitter,
  Output,
  ViewChild,
  AfterViewInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ILoginUserModel } from '@interfaces';
import { ModalService } from '@services';

@Component({
  selector: 'app-schedule-testcases-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  styleUrl: './schedule-testcases-dialog.component.css',
  templateUrl: './schedule-testcases-dialog.component.html',
})
export class ScheduleTestcasesDialogComponent implements AfterViewInit {
  @ViewChild('scheduleModal') modalElement!: ElementRef;

  browser: string = 'Chrome';
  date: string = '';
  time: string = '';

  // Populated by the caller only when the target environment's RequiresAuthentication
  // is true (see test-case-execution-panel.component.ts's onSchedule/onBulkSchedule) -
  // empty otherwise, in which case no Login User field is shown at all, unchanged from
  // before this feature existed.
  loginUsers: ILoginUserModel[] = [];
  loginUserId: number | null = null;

  private callback!: (data: any) => void;

  constructor(private modalService: ModalService) {}

  ngAfterViewInit() {
    setTimeout(() => {
      this.modalService.register(
        'scheduleTestcasesModal',
        this.modalElement.nativeElement
      );
    });
  }

  /** Open modal and pass callback - loginUsers empty when the environment doesn't
   * require authentication. */
  open(cb: (data: any) => void, loginUsers: ILoginUserModel[] = []) {
    this.callback = cb;
    this.loginUsers = loginUsers.filter((lu) => lu.isActive);
    this.loginUserId = null;
    this.modalService.open('scheduleTestcasesModal');
  }

  /** Close modal */
  close() {
    this.modalService.close('scheduleTestcasesModal');
  }

  get requiresLoginUser(): boolean {
    return this.loginUsers.length > 0;
  }

  // Self-service - every entry is always the current user's own credential, so no
  // portalUserName suffix is needed anymore (was previously used to distinguish whose
  // credential was whose when everyone's credentials were shown together).
  loginUserLabel(lu: ILoginUserModel): string {
    return `${lu.userRole} - ${lu.userName}`;
  }

  /** Submit form back to parent */
  submit() {
    if (!this.browser || !this.date || !this.time) return;
    if (this.isDateTimeInvalid) return;
    if (this.requiresLoginUser && !this.loginUserId) return;

    if (this.callback) {
      this.callback({
        browser: this.browser,
        date: this.date,
        time: this.time,
        loginUserId: this.loginUserId ?? undefined,
      });
    }

    // Reset form values  ✔ FIX
    this.browser = 'Chrome';
    this.date = '';
    this.time = '';
    this.loginUserId = null;

    this.close();
  }

  get isDateTimeInvalid(): boolean {
    if (!this.date || !this.time) return false; // do not show error initially

    const selected = new Date(this.date + 'T' + this.time);
    const now = new Date();

    return selected < now;
  }
}

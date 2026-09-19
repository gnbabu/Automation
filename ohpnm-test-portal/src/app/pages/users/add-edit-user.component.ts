import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import {
  IPriorityStatus,
  ITimeZone,
  IUser,
  IUserRole,
  IUserStatus,
} from '@interfaces';
import { Mappers } from '@mappers';
import { CommonToasterService, UsersService } from '@services';
import { Router } from '@angular/router';
import { AppDropdownComponent } from 'app/core/components/app-dropdown/app-dropdown.component';

@Component({
  selector: 'app-add-edit-user',
  standalone: true,
  imports: [CommonModule, FormsModule, AppDropdownComponent],
  templateUrl: './add-edit-user.component.html',
  styleUrl: './add-edit-user.component.css',
})
export class AddEditUserComponent implements OnInit {
  @Input() user: IUser = Mappers.UserMapper.empty();
  @Output() saved = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  profileFile?: File;
  profilePreviewUrl?: string;
  roles: IUserRole[] = [];
  timeZones: ITimeZone[] = [];
  statuses: IUserStatus[] = [];
  priorities: IPriorityStatus[] = [];

  // Two-Factor - options are the exact same literal strings the original native
  // <select value="true"/value="false"> used (not real booleans) - user.twoFactor is
  // typed `boolean` but a plain `value="..."` attribute always coerces to a string at
  // runtime once picked, same pre-existing behavior preserved deliberately here (see
  // AGENTS.md "AppDropdown migration" - onSubmit's own `? true : false` coercion has a
  // real, narrow pre-existing bug for the "No" case, left exactly as-is on request).
  twoFactorOptions = ['true', 'false'];
  twoFactorLabel = (opt: string) => (opt === 'true' ? 'Yes' : 'No');

  // Time Zone / Status - the originals used a plain `[value]="tz.timeZoneId"` (always
  // string-coerced), not `[ngValue]`, even though IUser.timeZone/.status are typed
  // `number` - deliberately preserved exactly via a string-returning bindValue rather
  // than "fixed" to a real number, per explicit instruction.
  timeZoneStringBindValue = (tz: ITimeZone) => String(tz.timeZoneId);
  statusStringBindValue = (s: IUserStatus) => String(s.statusId);

  constructor(
    private router: Router,
    private usersService: UsersService,
    private toaster: CommonToasterService
  ) {}

  ngOnInit(): void {
    this.loadUserRoles();
    this.loadTimeZones();
    this.loadUserStatuses();
    this.loadPriorityStatuses();
  }

  loadUserRoles(): void {
    this.usersService.getUserRoles().subscribe({
      next: (data) => (this.roles = data),
      error: (err) => console.error('Error loading roles:', err),
    });
  }

  loadTimeZones(): void {
    this.usersService.getTimeZones().subscribe({
      next: (data) => (this.timeZones = data),
      error: (err) => console.error('Error loading time zones:', err),
    });
  }

  loadUserStatuses(): void {
    this.usersService.getUserStatuses().subscribe({
      next: (data) => (this.statuses = data),
      error: (err) => console.error('Error loading statuses:', err),
    });
  }

  loadPriorityStatuses(): void {
    this.usersService.getPriorityStatuses().subscribe({
      next: (data) => (this.priorities = data),
      error: (err) => console.error('Error loading priorities:', err),
    });
  }

  onFileChange(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      this.profileFile = input.files[0];
      const reader = new FileReader();

      reader.onload = () => {
        this.profilePreviewUrl = reader.result as string;

        // Strip prefix and assign base64 string to model
        const base64 = this.profilePreviewUrl.split(',')[1];
        this.user.photo = base64;
      };

      reader.readAsDataURL(this.profileFile);
    }
  }

  onSubmit(form: NgForm) {
    if (!form.valid) return;
    this.user.twoFactor = this.user.twoFactor ? true : false;
    const isNewUser = !this.user.userId || this.user.userId === 0;
    const save$ = isNewUser
      ? this.usersService.create(this.user)
      : this.usersService.update(this.user);

    save$.subscribe({
      next: () => {
        this.toaster.success(
          `User ${isNewUser ? 'created' : 'updated'} successfully`
        );
        this.saved.emit();
      },
      error: (err) => {
        console.error(
          `Error ${isNewUser ? 'creating' : 'updating'} user:`,
          err
        );
        this.toaster.error(
          `Error ${isNewUser ? 'creating' : 'updating'} user: ${err}`
        );
      },
    });
  }

  getPhotoUrl(photo?: string): string {
    if (!photo) {
      return 'assets/images/default-user.png';
    }

    return photo.startsWith('data:image')
      ? photo
      : `data:image/png;base64,${photo}`;
  }
}

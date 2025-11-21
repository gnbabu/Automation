import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { IUser, IUserRole } from '@interfaces';
import { Mappers } from '@mappers';
import { CommonToasterService, UsersService } from '@services';
import { Router } from '@angular/router';

@Component({
  selector: 'app-add-edit-user',
  standalone: true,
  imports: [CommonModule, FormsModule],
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

  timeZones = [
    { id: 1, name: 'UTC' },
    { id: 2, name: 'Eastern Standard Time' },
    { id: 3, name: 'Central Standard Time' },
    { id: 4, name: 'India Standard Time' },
    { id: 5, name: 'Pacific Standard Time' },
  ];

  constructor(
    private router: Router,
    private usersService: UsersService,
    private toaster: CommonToasterService
  ) {}

  ngOnInit(): void {
    this.loadUserRoles();
  }

  loadUserRoles(): void {
    this.usersService.getUserRoles().subscribe({
      next: (data) => (this.roles = data),
      error: (err) => console.error('Error loading roles:', err),
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

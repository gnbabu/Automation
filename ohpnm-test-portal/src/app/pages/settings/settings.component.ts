import { Component, OnInit } from '@angular/core';
import { IUser } from '@interfaces';
import { AuthService, UsersService, CommonToasterService } from '@services';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.css',
})
export class SettingsComponent implements OnInit {
  user!: IUser;
  changePassword = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
  };

  passwordStrength: 'Weak' | 'Medium' | 'Strong' = 'Weak';
  passwordStrengthMessage = '';
  passwordStrengthClass = '';
  passwordStrengthPercent = 0;

  showPasswordForm = false;

  constructor(
    private authService: AuthService,
    private usersService: UsersService,
    private toaster: CommonToasterService
  ) {}

  ngOnInit(): void {
    this.loadUserDetails();
  }

  checkPasswordStrength(password: string): void {
    let strength = 0;

    if (password.length >= 8) strength++;
    if (/[A-Z]/.test(password)) strength++;
    if (/[a-z]/.test(password)) strength++;
    if (/[0-9]/.test(password)) strength++;
    if (/[^A-Za-z0-9]/.test(password)) strength++;

    switch (true) {
      case strength >= 4:
        this.passwordStrength = 'Strong';
        this.passwordStrengthMessage = 'Strong password';
        this.passwordStrengthClass = 'text-success';
        this.passwordStrengthPercent = 100;
        break;
      case strength >= 3:
        this.passwordStrength = 'Medium';
        this.passwordStrengthMessage = 'Medium strength password';
        this.passwordStrengthClass = 'text-warning';
        this.passwordStrengthPercent = 60;
        break;
      default:
        this.passwordStrength = 'Weak';
        this.passwordStrengthMessage = 'Weak password';
        this.passwordStrengthClass = 'text-danger';
        this.passwordStrengthPercent = 30;
        break;
    }
  }

  loadUserDetails() {
    const userId = this.authService.getLoggedInUserId();
    this.usersService.getUserById(userId).subscribe({
      next: (data) => (this.user = data),
      error: (err) => console.error('Failed to load user', err),
    });
  }

  toggleChangePassword() {
    this.showPasswordForm = !this.showPasswordForm;
    this.changePassword = {
      currentPassword: '',
      newPassword: '',
      confirmPassword: '',
    };

    this.passwordStrengthMessage = '';
    this.passwordStrengthClass = '';
    this.passwordStrengthPercent = 0;
  }

  getPhotoUrl(photo?: string): string {
    if (!photo) {
      return 'default-user.png';
    }

    return photo.startsWith('data:image')
      ? photo
      : `data:image/png;base64,${photo}`;
  }

  onChangePassword(form: NgForm) {
    if (
      form.invalid ||
      this.changePassword.newPassword !== this.changePassword.confirmPassword
    ) {
      return;
    }

    this.usersService
      .changePassword({
        userId: this.user.userId,
        oldPassword: this.changePassword.currentPassword,
        newPassword: this.changePassword.newPassword,
      })
      .subscribe({
        next: () => {
          this.toaster.success(
            'Password updated successfully! Please login again'
          );
          this.showPasswordForm = false;
          this.changePassword = {
            currentPassword: '',
            newPassword: '',
            confirmPassword: '',
          };
          this.authService.logout();
        },
        error: (err) => {
          this.toaster.error(
            `Failed to change password : ${err.error.message}`
          );
        },
      });
  }
}

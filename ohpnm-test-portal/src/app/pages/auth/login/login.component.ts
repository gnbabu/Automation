import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { ForgotPasswordModalComponent } from '../forgot-password/forgot-password-modal.component';
import { ForgotUsernameModalComponent } from '../forgot-username/forgot-username-modal.component';
import { LoginTipsModalComponent } from '../login-tips/login-tips-modal.component';
import { AuthService, ModalService, CommonToasterService } from '@services';
import { LoginRequest } from '@interfaces';
import { LoadingOverlayComponent } from 'app/core/components/loader/loading-overlay.component';
import { LoginCarouselViewComponent } from '../login-carousel-view/login-carousel-view.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ForgotPasswordModalComponent,
    ForgotUsernameModalComponent,
    LoginTipsModalComponent,
    LoadingOverlayComponent,
    LoginCarouselViewComponent,
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent {
  loginForm: FormGroup;
  showPassword = false;
  isSubmitting = false;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private modalService: ModalService,
    private authService: AuthService,
    private toaster: CommonToasterService
  ) {
    this.loginForm = this.fb.group({
      username: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(5)]],
      rememberMe: [false],
    });
  }

  get username() {
    return this.loginForm.get('username');
  }

  get password() {
    return this.loginForm.get('password');
  }

  onSubmit() {
    if (this.isSubmitting) return;

    if (!this.loginForm.valid) {
      this.toaster.info('Please enter valid username and password');
      this.loginForm.markAllAsTouched();
      return;
    }

    const loginRequest: LoginRequest = {
      username: this.loginForm.value.username,
      password: this.loginForm.value.password,
    };

    this.isSubmitting = true;

    this.authService.login(loginRequest).subscribe({
      next: (response) => {
        this.isSubmitting = false;
        this.toaster.success('Login successful!');
        this.router.navigate(['/dashboard'], { replaceUrl: true });
      },
      error: (err: any) => {
        this.isSubmitting = false;
        if (
          err.status === 401 ||
          err.status === 400 ||
          err.error?.message?.includes('Invalid')
        ) {
          this.toaster.error('Invalid username or password.');
        } else {
          this.toaster.error('An error occurred. Please try again.');
        }
      },
    });
  }

  openForgotPassword(): void {
    this.modalService.open('forgotPasswordModal');
  }

  openForgotUsername(): void {
    this.modalService.open('forgotUsernameModal');
  }

  openLoginTips(): void {
    this.modalService.open('loginTipsModal');
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  onRegisterClick() {
    this.router.navigate(['/register']);
  }
}

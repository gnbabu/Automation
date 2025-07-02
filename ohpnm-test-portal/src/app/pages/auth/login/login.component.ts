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
import { AuthService, ModalService, CommonToasterService } from '@services';
import { LoginRequest } from '@interfaces';
import { LoadingOverlayComponent } from 'app/core/components/loader/loading-overlay.component';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ForgotPasswordModalComponent,
    LoadingOverlayComponent,
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent {
  loginForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private modalService: ModalService,
    private authService: AuthService,
    private toaster: CommonToasterService
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
      rememberMe: [false],
    });
  }

  onSubmit() {
    if (this.loginForm.valid) {
      const loginRequest: LoginRequest = {
        email: this.loginForm.value.email,
        password: this.loginForm.value.password,
      };

      this.authService.login(loginRequest).subscribe({
        next: (response) => {
          console.log('Login successful', response);
          this.toaster.success('login successfully');
          this.router.navigate(['/dashboard'], { replaceUrl: true });
        },
        error: (err: any) => {
          console.log(err.error);
        },
        complete: () => {},
      });
    } else {
      this.toaster.info('Please enter valid email and password');
      this.loginForm.markAllAsTouched();
    }
  }
  openForgotPassword(): void {
    this.modalService.open('forgotPasswordModal');
  }
}

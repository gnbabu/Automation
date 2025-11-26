import { Component } from '@angular/core';
import {
  FormBuilder,
  Validators,
  AbstractControl,
  ValidationErrors,
  FormGroup,
  ReactiveFormsModule,
} from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { RegisterRequest } from '../../../core/interfaces';
import {
  AuthService,
  ModalService,
  CommonToasterService,
} from '../../../core/services';
import { Router } from '@angular/router';
import { RegisterTipsModalComponent } from '../register-tips/register-tips-modal.component';
import { LoginCarouselViewComponent } from '../login-carousel-view/login-carousel-view.component';
import { LoadingOverlayComponent } from 'app/core/components/loader/loading-overlay.component';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RegisterTipsModalComponent,
    LoginCarouselViewComponent,
    LoadingOverlayComponent,
  ],
})
export class RegisterComponent {
  successMessage = '';
  errorMessage = '';
  registerForm: FormGroup;

  message: string = '';
  messageType: 'success' | 'error' | '' = '';
  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private authService: AuthService,
    private toaster: CommonToasterService,
    private router: Router,
    private modalService: ModalService
  ) {
    this.registerForm = this.fb.group(
      {
        username: ['', [Validators.required]],
        email: ['', [Validators.required, Validators.email]],
        password: [
          '',
          [
            Validators.required,
            Validators.minLength(12),
            Validators.pattern(
              '^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^a-zA-Z0-9]).+$'
            ),
          ],
        ],
        confirmPassword: ['', [Validators.required]],
        terms: [false],
      },
      { validators: this.passwordsMatch }
    );
  }

  passwordsMatch(group: AbstractControl): ValidationErrors | null {
    const password = group.get('password')?.value;
    const confirm = group.get('confirmPassword')?.value;
    return password === confirm ? null : { notMatching: true };
  }

  onSubmit() {
    if (!this.registerForm.valid) {
      this.toaster.info('Please enter valid details');
      this.registerForm.markAllAsTouched();
      return;
    }

    const registerRequest: RegisterRequest = this.registerForm.value;

    this.authService.register(registerRequest).subscribe({
      next: (response) => {
        if (response?.result) {
          this.toaster.success('Registration successful');
          this.router.navigate(['/login'], { replaceUrl: true });
        } else {
          this.toaster.error(
            response?.message || 'Registration failed',
            'error'
          );
        }
      },
      error: (err) => {
        const errorMessage =
          err?.error?.message || err?.error || 'An unexpected error occurred';

        this.toaster.error(`Error: ${errorMessage}`);
      },
    });
  }

  onSignInClick() {
    this.router.navigate(['/login']);
  }

  openRegisterTips(): void {
    this.modalService.open('registerTipsModal');
  }
}

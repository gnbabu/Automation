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
  registerForm: FormGroup;

  showPassword = false;
  showConfirmPassword = false;
  isSubmitting = false;

  passwordStrength: 'Weak' | 'Medium' | 'Strong' = 'Weak';
  passwordStrengthMessage = '';
  passwordStrengthClass = '';
  passwordStrengthPercent = 0;

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
            Validators.minLength(8),
            Validators.pattern(
              '^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^a-zA-Z0-9]).+$'
            ),
          ],
        ],
        confirmPassword: ['', [Validators.required]],
        terms: [false, [Validators.requiredTrue]],
      },
      { validators: this.passwordsMatch }
    );

    this.registerForm.get('password')?.valueChanges.subscribe((value) => {
      this.checkPasswordStrength(value ?? '');
    });
  }

  passwordsMatch(group: AbstractControl): ValidationErrors | null {
    const password = group.get('password')?.value;
    const confirm = group.get('confirmPassword')?.value;
    return password === confirm ? null : { notMatching: true };
  }

  // Mirrors settings.component.ts's checkPasswordStrength exactly, so "Weak"/"Medium"/
  // "Strong" mean the same thing everywhere in the app rather than two different scales.
  checkPasswordStrength(password: string): void {
    let strength = 0;

    if (password.length >= 8) strength++;
    if (/[a-z]/.test(password) && /[A-Z]/.test(password)) strength++;
    if (/\d/.test(password)) strength++;
    if (/[^a-zA-Z0-9]/.test(password)) strength++;

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
        this.passwordStrengthMessage = password
          ? 'Weak password'
          : '';
        this.passwordStrengthClass = 'text-danger';
        this.passwordStrengthPercent = password ? 30 : 0;
        break;
    }
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  onSubmit() {
    if (this.isSubmitting) return;

    if (!this.registerForm.valid) {
      this.toaster.info('Please enter valid details');
      this.registerForm.markAllAsTouched();
      return;
    }

    const registerRequest: RegisterRequest = this.registerForm.value;
    this.isSubmitting = true;

    this.authService.register(registerRequest).subscribe({
      next: (response) => {
        this.isSubmitting = false;
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
        this.isSubmitting = false;
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

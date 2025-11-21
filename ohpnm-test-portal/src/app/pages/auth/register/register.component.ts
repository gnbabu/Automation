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

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RegisterTipsModalComponent,
    LoginCarouselViewComponent,
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
    if (this.registerForm.valid) {
      const registerRequest: RegisterRequest = {
        username: this.registerForm.value.username,
        email: this.registerForm.value.email,
        password: this.registerForm.value.password,
        confirmPassword: this.registerForm.value.confirmPassword,
      };

      this.authService.register(registerRequest).subscribe({
        next: (response) => {
          if (response.result == true) {
            this.message = response.message;
            this.messageType = 'success';

            setTimeout(() => {
              this.message = '';
              this.messageType = '';
            }, 6000);

            this.router.navigate(['/login'], { replaceUrl: true });
          } else {
            this.message = response.message;
            this.messageType = 'error';

            setTimeout(() => {
              this.message = '';
              this.messageType = '';
            }, 6000);
          }
        },
        error: (err: any) => {
          console.log('Registration failed', err);
          this.message = err.error;
          this.messageType = 'error';

          setTimeout(() => {
            this.message = '';
            this.messageType = '';
          }, 6000);
        },
        complete: () => {},
      });
    } else {
      this.toaster.info('Please enter valid details');
      this.registerForm.markAllAsTouched();
    }
  }

  onSignInClick() {
    this.router.navigate(['/login']);
  }

  openRegisterTips(): void {
    this.modalService.open('registerTipsModal');
  }
}

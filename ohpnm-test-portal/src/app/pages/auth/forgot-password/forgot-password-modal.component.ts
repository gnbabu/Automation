import { Component, AfterViewInit, ElementRef } from '@angular/core';
import { AuthService, CommonToasterService, ModalService } from '@services';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-forgot-password-modal',
  standalone: true,
  templateUrl: './forgot-password-modal.component.html',
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  styleUrl: './forgot-password-modal.component.css',
})
export class ForgotPasswordModalComponent implements AfterViewInit {
  forgotpasswordForm: FormGroup;

  constructor(
    private elRef: ElementRef,
    private fb: FormBuilder,
    private router: Router,
    private modalService: ModalService,
    private authService: AuthService,
    private toaster: CommonToasterService
  ) {
    this.forgotpasswordForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });
  }

  ngAfterViewInit(): void {
    const modalElement = this.elRef.nativeElement.querySelector(
      '#forgotPasswordModal'
    );
    if (modalElement) {
      this.modalService.register('forgotPasswordModal', modalElement);
    }
  }

  get email() {
    return this.forgotpasswordForm.get('email');
  }

  submitEmail(): void {
    const email = this.forgotpasswordForm.value.email;

    if (!email || !this.validateEmail(email)) {
      this.toaster.info('Please enter a valid email address');
      this.forgotpasswordForm.markAllAsTouched();
      return;
    }

    this.authService.forgotPassword(email).subscribe({
      next: (response) => {
        this.toaster.success(response.message);
        this.modalService.close('forgotPasswordModal');
      },
      error: (err: any) => {
        if (err.status === 404) {
          this.toaster.error(err.error.message);
        } else {
          this.toaster.error('Unable to process request. Please try again.');
        }
      },
    });
  }

  validateEmail(email: string): boolean {
    // Simple email validation
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  }

  returnToLogin() {
    this.modalService.close('forgotPasswordModal');
    this.router.navigate(['/login']);
  }
}

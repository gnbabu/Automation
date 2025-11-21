import { Component, AfterViewInit, ElementRef } from '@angular/core';
import { AuthService, CommonToasterService, ModalService } from '@services';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-forgot-username-modal',
  standalone: true,
  templateUrl: './forgot-username-modal.component.html',
  styleUrls: ['./forgot-username-modal.component.css'],
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
})
export class ForgotUsernameModalComponent implements AfterViewInit {
  forgotusernameForm: FormGroup;

  constructor(
    private elRef: ElementRef,
    private fb: FormBuilder,
    private router: Router,
    private modalService: ModalService,
    private authService: AuthService,
    private toaster: CommonToasterService
  ) {
    this.forgotusernameForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });
  }

  ngAfterViewInit(): void {
    const modalElement = this.elRef.nativeElement.querySelector(
      '#forgotUsernameModal'
    );
    if (modalElement) {
      this.modalService.register('forgotUsernameModal', modalElement);
    }
  }

  get email() {
    return this.forgotusernameForm.get('email');
  }

  submitEmail(): void {
    if (!this.forgotusernameForm.value.email || !this.validateEmail(this.forgotusernameForm.value.email)) {
      this.toaster.info('Please enter valid username and username');
      this.forgotusernameForm.markAllAsTouched();
      return;
    }

    this.authService.forgotusername(this.forgotusernameForm.value.email).subscribe({
      next: (response) => {
        if (response === true) {
          this.toaster.success('Username reset link sent successfully');
          this.modalService.close('forgotUsernameModal');
        }
      },
      error: (err: any) => {
        console.log(err.error);
      },
      complete: () => { },
    });
  }

  validateEmail(email: string): boolean {
    // Simple email validation
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
  }

  returnToLogin() {
    this.modalService.close('forgotUsernameModal');
    this.router.navigate(['/login']);
  }
}

import { Component, AfterViewInit, ElementRef } from '@angular/core';
import { AuthService, CommonToasterService, ModalService } from '@services';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login-tips-modal',
  standalone: true,
  templateUrl: './login-tips-modal.component.html',
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  styleUrl: './login-tips-modal.component.css',
})
export class LoginTipsModalComponent implements AfterViewInit {
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
      '#loginTipsModal'
    );
    if (modalElement) {
      this.modalService.register('loginTipsModal', modalElement);
    }
  }

  returnToLogin() {
    this.modalService.close('loginTipsModal');
    this.router.navigate(['/login']);
  }
}

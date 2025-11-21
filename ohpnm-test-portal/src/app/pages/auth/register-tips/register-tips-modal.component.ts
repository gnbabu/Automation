import { Component, AfterViewInit, ElementRef } from '@angular/core';
import { AuthService, CommonToasterService, ModalService } from '@services';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register-tips-modal',
  standalone: true,
  templateUrl: './register-tips-modal.component.html',
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  styleUrl: './register-tips-modal.component.css',
})
export class RegisterTipsModalComponent implements AfterViewInit {
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
      '#registerTipsModal'
    );
    if (modalElement) {
      this.modalService.register('registerTipsModal', modalElement);
    }
  }

  returnToRegister() {
    this.modalService.close('registerTipsModal');
    this.router.navigate(['/register']);
  }
}

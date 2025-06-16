import { Component, AfterViewInit, ElementRef } from '@angular/core';
import { ModalService } from '@services';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-forgot-password-modal',
  standalone: true,
  templateUrl: './forgot-password-modal.component.html',
  imports: [CommonModule, FormsModule],
})
export class ForgotPasswordModalComponent implements AfterViewInit {
  email: string = '';

  constructor(private elRef: ElementRef, public modalService: ModalService) {}

  ngAfterViewInit(): void {
    const modalElement = this.elRef.nativeElement.querySelector(
      '#forgotPasswordModal'
    );
    if (modalElement) {
      this.modalService.register('forgotPasswordModal', modalElement);
    }
  }

  submitEmail(): void {
    console.log('Reset password for:', this.email);
    this.modalService.close('forgotPasswordModal');
  }
}

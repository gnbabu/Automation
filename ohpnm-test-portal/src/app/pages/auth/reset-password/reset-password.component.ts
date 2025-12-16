import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import {
  FormBuilder,
  Validators,
  ReactiveFormsModule,
  FormGroup,
} from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '@services';

@Component({
  selector: 'app-reset-password',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.css',
})
export class ResetPasswordComponent implements OnInit {
  token!: string;
  loading = false;
  form!: FormGroup;

  constructor(
    private route: ActivatedRoute,
    private fb: FormBuilder,
    private authService: AuthService,
    private toaster: ToastrService,
    private router: Router
  ) {
    this.form = this.fb.group({
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', Validators.required],
    });
  }

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParamMap.get('token') || '';

    if (!this.token) {
      this.toaster.error('Invalid or missing reset token');
      this.router.navigate(['/login']);
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    if (this.form.value.newPassword !== this.form.value.confirmPassword) {
      this.toaster.error('Passwords do not match');
      return;
    }

    this.loading = true;

    this.authService
      .resetPassword({
        token: this.token,
        newPassword: this.form.value.newPassword!,
      })
      .subscribe({
        next: () => {
          this.toaster.success('Password reset successfully');
          this.router.navigate(['/login']);
        },
        error: (err) => {
          this.toaster.error(err?.error || 'Reset link expired or invalid');
        },
        complete: () => (this.loading = false),
      });
  }
}

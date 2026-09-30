import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import {
  AuthService,
  CommonToasterService,
  EnvironmentService,
} from '@services';
import { IEnvironmentRequestDto, IEnvironmentModel } from '@interfaces';
import { AppDropdownComponent } from 'app/core/components/app-dropdown/app-dropdown.component';

@Component({
  standalone: true,
  selector: 'app-environment-form',
  imports: [CommonModule, FormsModule, AppDropdownComponent],
  templateUrl: './environment-form.component.html',
  styleUrl: './environment-form.component.css',
})
export class EnvironmentFormComponent implements OnInit {
  model: IEnvironmentRequestDto = {
    environmentName: '',
    description: '',
    isActive: true,
    createdBy: 0,
    environmentUrl: '',
    requiresAuthentication: true,
    enableSso: false,
  };

  // Options are plain booleans, not objects - textAccessor here just labels each one.
  activeStatusOptions = [true, false];
  activeStatusLabel = (isActive: boolean) => (isActive ? 'Active' : 'Inactive');

  isEdit = false;
  environmentId!: number;
  isSaving = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private envService: EnvironmentService,
    private authService: AuthService,
    private toaster: CommonToasterService,
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (id) {
      this.isEdit = true;
      this.environmentId = +id;
      this.loadEnvironment();
    }
  }

  loadEnvironment(): void {
    this.envService.getById(this.environmentId).subscribe({
      next: (env: IEnvironmentModel) => {
        this.model = {
          environmentId: env.environmentId,
          environmentName: env.environmentName,
          description: env.description,
          isActive: env.isActive,
          createdBy: env.createdBy,
          environmentUrl: env.environmentUrl ?? '',
          requiresAuthentication: env.requiresAuthentication,
          enableSso: env.enableSso,
        };
      },
      error: (err) => {
        console.error('Failed to load environment:', err);
        this.toaster.error(
          err?.error?.message ?? err?.error ?? 'Environment not found.'
        );
        this.router.navigate(['/environment-management']);
      },
    });
  }

  save(): void {
    if (this.isInvalid) return;

    this.model.createdBy = this.authService.getLoggedInUserId();
    this.isSaving = true;
    const isNewEnvironment = !this.isEdit;

    const request$ = this.isEdit
      ? this.envService.update(this.model)
      : this.envService.create(this.model);

    request$.subscribe({
      next: () => {
        this.toaster.success(
          `Environment ${isNewEnvironment ? 'created' : 'updated'} successfully`,
        );

        this.router.navigate(['/environment-management']);
      },
      error: (err) => {
        console.error('Failed to save environment:', err);
        this.isSaving = false;
        this.toaster.error(
          err?.error?.message ?? err?.error ?? 'Failed to save environment.'
        );
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/environment-management']);
  }

  // Authentication Required and Enable SSO are mutually exclusive - checking one
  // unchecks the other, since a manual username/password login and "no login screen at
  // all" can't both be true for the same environment.
  onRequiresAuthenticationChange(requiresAuthentication: boolean): void {
    if (requiresAuthentication) {
      this.model.enableSso = false;
    }
  }

  onEnableSsoChange(enableSso: boolean): void {
    if (enableSso) {
      this.model.requiresAuthentication = false;
    }
  }

  get isInvalid(): boolean {
    return !this.model.environmentName?.trim();
  }
}

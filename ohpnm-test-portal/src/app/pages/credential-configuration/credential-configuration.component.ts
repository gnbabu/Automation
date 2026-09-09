import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  IEnvironmentModel,
  ILoginUserModel,
  ILoginUserRequestDto,
} from '@interfaces';
import {
  CommonToasterService,
  ConfirmService,
  EnvironmentService,
  LoginUserService,
} from '@services';

// Self-service: every user manages only their own login credential per environment -
// no Portal User picker, no visibility into other users' credentials (see AGENTS.md).
// Reached from its own top-level sidebar tab ("Credential Configuration") - not nested
// under a specific environment anymore, hence the on-page Environment dropdown below
// (mirrors TestDataManagementComponent's exact "pick an environment first" pattern).
@Component({
  selector: 'app-credential-configuration',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './credential-configuration.component.html',
  styleUrl: './credential-configuration.component.css',
})
export class CredentialConfigurationComponent implements OnInit {
  environments: IEnvironmentModel[] = [];
  selectedEnvironment?: IEnvironmentModel;

  loginUsers: ILoginUserModel[] = [];
  loading = false;
  isSaving = false;

  // Null while adding a new row; set to the row being edited otherwise. Password is
  // deliberately never pre-filled here even when editing - leaving it blank on save
  // keeps the existing password unchanged (see LoginUserRequestDto.password).
  editingLoginUserId: number | null = null;

  model: ILoginUserRequestDto = this.emptyModel();

  constructor(
    private envService: EnvironmentService,
    private loginUserService: LoginUserService,
    private toaster: CommonToasterService,
    private confirmService: ConfirmService,
  ) {}

  ngOnInit(): void {
    this.envService.getAll().subscribe({
      next: (res) => (this.environments = res || []),
      error: (err) => console.error('Failed to load environments:', err),
    });
  }

  private emptyModel(): ILoginUserRequestDto {
    return {
      environmentId: this.selectedEnvironment?.environmentId ?? 0,
      userRole: '',
      userName: '',
      password: '',
      isActive: true,
    };
  }

  onEnvironmentChange(): void {
    this.startAdd();
    this.loginUsers = [];
    if (this.selectedEnvironment) {
      this.loadLoginUsers();
    }
  }

  loadLoginUsers(): void {
    if (!this.selectedEnvironment) return;

    this.loading = true;
    this.loginUserService
      .getMineForEnvironment(this.selectedEnvironment.environmentId)
      .subscribe({
        next: (res) => {
          this.loginUsers = res || [];
          this.loading = false;
        },
        error: (err) => {
          console.error('Failed to load login users:', err);
          this.loading = false;
          this.toaster.error(
            err?.error?.message ?? err?.error ?? 'Failed to load login users.'
          );
        },
      });
  }

  startAdd(): void {
    this.editingLoginUserId = null;
    this.model = this.emptyModel();
  }

  startEdit(loginUser: ILoginUserModel): void {
    this.editingLoginUserId = loginUser.loginUserId;
    this.model = {
      loginUserId: loginUser.loginUserId,
      environmentId: this.selectedEnvironment!.environmentId,
      userRole: loginUser.userRole,
      userName: loginUser.userName,
      password: '', // never pre-filled - blank keeps the existing password unchanged
      isActive: loginUser.isActive,
    };
  }

  cancelEdit(): void {
    this.startAdd();
  }

  save(): void {
    if (this.isInvalid || !this.selectedEnvironment) return;

    this.isSaving = true;
    const isNew = this.editingLoginUserId == null;
    this.model.environmentId = this.selectedEnvironment.environmentId;

    if (isNew) {
      this.model.isActive = true;
    }

    const request$ = isNew
      ? this.loginUserService.create(this.model)
      : this.loginUserService.update(this.model);

    request$.subscribe({
      next: () => {
        this.toaster.success(
          `Login user ${isNew ? 'added' : 'updated'} successfully`,
        );
        this.isSaving = false;
        this.startAdd();
        this.loadLoginUsers();
      },
      error: (err) => {
        console.error('Failed to save login user:', err);
        this.isSaving = false;
        this.toaster.error(
          err?.error?.message ?? err?.error ?? 'Failed to save login user.'
        );
      },
    });
  }

  async delete(loginUser: ILoginUserModel): Promise<void> {
    const confirmed = await this.confirmService.confirm(
      'Delete Login User',
      `Permanently delete "${loginUser.userRole} - ${loginUser.userName}"? This cannot be undone.`
    );
    if (!confirmed) return;

    this.loginUserService.hardDelete(loginUser.loginUserId).subscribe({
      next: () => {
        this.toaster.success('Login user deleted successfully');
        this.loadLoginUsers();
      },
      error: (err) => {
        console.error('Failed to delete login user:', err);
        this.toaster.error(
          err?.error?.message ?? err?.error ?? 'Failed to delete login user. It may already be in use by a queued/scheduled run - disable it instead if so.'
        );
      },
    });
  }

  get isInvalid(): boolean {
    return !this.model.userRole?.trim() || !this.model.userName?.trim() ||
      (this.editingLoginUserId == null && !this.model.password?.trim());
  }

  get isEditing(): boolean {
    return this.editingLoginUserId != null;
  }
}

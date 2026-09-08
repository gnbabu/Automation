import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import {
  IEnvironmentModel,
  ILoginUserModel,
  ILoginUserRequestDto,
  IUser,
} from '@interfaces';
import {
  AuthService,
  CommonToasterService,
  ConfirmService,
  EnvironmentService,
  LoginUserService,
  UsersService,
} from '@services';

@Component({
  selector: 'app-environment-login-users',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './environment-login-users.component.html',
  styleUrl: './environment-login-users.component.css',
})
export class EnvironmentLoginUsersComponent implements OnInit {
  environmentId!: number;
  environment: IEnvironmentModel | null = null;

  loginUsers: ILoginUserModel[] = [];
  portalUsers: IUser[] = [];
  loading = false;
  isSaving = false;

  // Null while adding a new row; set to the row being edited otherwise. Password is
  // deliberately never pre-filled here even when editing - leaving it blank on save
  // keeps the existing password unchanged (see LoginUserRequestDto.password).
  editingLoginUserId: number | null = null;

  model: ILoginUserRequestDto = this.emptyModel();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private envService: EnvironmentService,
    private loginUserService: LoginUserService,
    private usersService: UsersService,
    private authService: AuthService,
    private toaster: CommonToasterService,
    private confirmService: ConfirmService,
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.router.navigate(['/environment-management']);
      return;
    }

    this.environmentId = +id;
    this.model.environmentId = this.environmentId;
    this.loadEnvironment();
    this.loadLoginUsers();
    this.loadPortalUsers();
  }

  private emptyModel(): ILoginUserRequestDto {
    return {
      environmentId: this.environmentId,
      portalUserId: null,
      userRole: '',
      userName: '',
      password: '',
      isActive: true,
    };
  }

  loadEnvironment(): void {
    this.envService.getById(this.environmentId).subscribe({
      next: (env) => (this.environment = env),
      error: (err) => {
        console.error('Failed to load environment:', err);
        this.toaster.error(
          err?.error?.message ?? err?.error ?? 'Environment not found.'
        );
        this.router.navigate(['/environment-management']);
      },
    });
  }

  loadLoginUsers(): void {
    this.loading = true;
    this.loginUserService.getByEnvironment(this.environmentId).subscribe({
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

  loadPortalUsers(): void {
    this.usersService.getAll().subscribe({
      next: (res) => (this.portalUsers = res || []),
      error: (err) => console.error('Failed to load portal users:', err),
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
      environmentId: this.environmentId,
      portalUserId: loginUser.portalUserId ?? null,
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
    if (this.isInvalid) return;

    this.isSaving = true;
    const isNew = this.editingLoginUserId == null;

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
    return !this.model.portalUserId || !this.model.userRole?.trim() || !this.model.userName?.trim() ||
      (this.editingLoginUserId == null && !this.model.password?.trim());
  }

  get isEditing(): boolean {
    return this.editingLoginUserId != null;
  }

  back(): void {
    this.router.navigate(['/environment-management']);
  }
}

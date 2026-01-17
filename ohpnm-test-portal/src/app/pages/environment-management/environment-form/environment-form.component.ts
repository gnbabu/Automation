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

@Component({
  standalone: true,
  selector: 'app-environment-form',
  imports: [CommonModule, FormsModule],
  templateUrl: './environment-form.component.html',
  styleUrl: './environment-form.component.css',
})
export class EnvironmentFormComponent implements OnInit {
  model: IEnvironmentRequestDto = {
    environmentName: '',
    description: '',
    isActive: true,
    createdBy: 0,
  };

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
    this.envService
      .getById(this.environmentId)
      .subscribe((env: IEnvironmentModel) => {
        this.model = {
          environmentId: env.environmentId,
          environmentName: env.environmentName,
          description: env.description,
          isActive: env.isActive,
          createdBy: env.createdBy,
        };
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
      error: () => {
        this.isSaving = false;
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/environment-management']);
  }

  get isInvalid(): boolean {
    return !this.model.environmentName?.trim();
  }
}

import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { IEnvironmentModel, IReleaseRequestDto } from '@interfaces';
import {
  AuthService,
  CommonToasterService,
  EnvironmentService,
  ReleaseService,
} from '@services';

@Component({
  standalone: true,
  selector: 'app-release-form',
  imports: [CommonModule, FormsModule],
  templateUrl: './release-form.component.html',
  styleUrl: './release-form.component.css',
})
export class ReleaseFormComponent implements OnInit {
  model: IReleaseRequestDto = {
    releaseName: '',
    version: '',
    environmentId: undefined,
    description: '',
  };

  allEnvironments: IEnvironmentModel[] = [];
  isEdit = false;
  releaseId!: number;
  isSaving = false;

  // Once a release leaves Draft, its Name/Version/Environment are locked (they're baked
  // into the release folder path and any recorded test results); only Description remains
  // editable.
  existingLifecycle = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private releaseService: ReleaseService,
    private envService: EnvironmentService,
    private authService: AuthService,
    private toaster: CommonToasterService,
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEdit = true;
      this.releaseId = +idParam;
    }

    this.envService.getAll().subscribe({
      next: (res) => (this.allEnvironments = res || []),
      error: () => (this.allEnvironments = []),
    });

    if (this.isEdit) {
      this.loadRelease();
    }
  }

  loadRelease(): void {
    this.releaseService.getById(this.releaseId).subscribe({
      next: (r) => {
        this.model = {
          releaseId: r.releaseId,
          releaseName: r.releaseName,
          version: r.version,
          environmentId: r.environmentId,
          description: r.description,
        };
        this.existingLifecycle = r.releaseLifecycle;
      },
      error: () => {
        this.toaster.error('Release not found.');
        this.router.navigate(['/release-management']);
      },
    });
  }

  // Name/Version/Environment are only editable when creating, or when editing a release
  // that is still in Draft.
  get canEditIdentity(): boolean {
    return !this.isEdit || (this.existingLifecycle || '').toLowerCase() === 'draft';
  }

  // Active environments, plus the currently-assigned one even if it has since been
  // deactivated (so it isn't silently dropped from the dropdown while editing).
  get selectableEnvironments(): IEnvironmentModel[] {
    return this.allEnvironments.filter(
      (e) => e.isActive || e.environmentId === this.model.environmentId,
    );
  }

  get isInvalid(): boolean {
    return (
      !this.model.releaseName?.trim() ||
      !this.model.version?.trim() ||
      !this.model.environmentId
    );
  }

  save(): void {
    if (this.isInvalid) {
      this.toaster.error('Release Name, Version and Environment are required.');
      return;
    }

    const user = this.authService.getLoggedInUser();
    this.isSaving = true;

    if (this.isEdit) {
      this.model.modifiedBy = user?.userName ?? 'system';
      this.releaseService.update(this.releaseId, this.model).subscribe({
        next: () => {
          this.toaster.success('Release updated successfully');
          this.router.navigate(['/release-management', this.releaseId]);
        },
        error: () => {
          this.isSaving = false;
        },
      });
      return;
    }

    this.model.createdBy = user?.userName ?? 'system';
    this.releaseService.create(this.model).subscribe({
      next: (created) => {
        this.toaster.success('Release created successfully');
        this.router.navigate(['/release-management', created.releaseId]);
      },
      error: () => {
        this.isSaving = false;
      },
    });
  }

  cancel(): void {
    if (this.isEdit) {
      this.router.navigate(['/release-management', this.releaseId]);
    } else {
      this.router.navigate(['/release-management']);
    }
  }
}

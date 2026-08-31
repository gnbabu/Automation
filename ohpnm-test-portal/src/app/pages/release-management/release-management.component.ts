import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { IEnvironmentModel, IReleaseModel, IReleaseRequestDto } from '@interfaces';
import {
  CommonToasterService,
  EnvironmentService,
  ReleaseService,
} from '@services';

@Component({
  selector: 'app-release-management',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './release-management.component.html',
  styleUrl: './release-management.component.css',
})
export class ReleaseManagementComponent implements OnInit {
  releases: IReleaseModel[] = [];
  environments: IEnvironmentModel[] = [];
  themes = ['green', 'orange', 'blue', 'purple', 'teal'];

  // filters
  search = '';
  environmentFilter = '';
  statusFilter = '';

  loading = false;

  constructor(
    private releaseService: ReleaseService,
    private envService: EnvironmentService,
    private toaster: CommonToasterService,
  ) {}

  ngOnInit(): void {
    this.loadEnvironments();
    this.loadReleases();
  }

  loadEnvironments(): void {
    this.envService.getAll().subscribe({
      next: (res) => (this.environments = res || []),
      error: () => (this.environments = []),
    });
  }

  loadReleases(): void {
    this.loading = true;
    this.releaseService.getAll().subscribe({
      next: (res) => {
        this.releases = res || [];
        this.loading = false;
      },
      error: () => {
        this.releases = [];
        this.loading = false;
      },
    });
  }

  refresh(): void {
    this.loadReleases();
    this.toaster.info('Releases refreshed');
  }

  // Soft delete / reactivate: toggles IsActive without touching Name/Version/Environment,
  // so it works regardless of lifecycle stage (unlike identity edits, which lock post-Draft).
  toggle(r: IReleaseModel): void {
    const isDisabling = r.isActive;
    const dto: IReleaseRequestDto = {
      releaseId: r.releaseId,
      releaseName: r.releaseName,
      version: r.version,
      environmentId: r.environmentId,
      description: r.description,
      isActive: !r.isActive,
    };

    this.releaseService.update(r.releaseId, dto).subscribe({
      next: () => {
        r.isActive = !r.isActive;
        this.toaster.success(
          `Release ${isDisabling ? 'deactivated' : 'activated'} successfully`,
        );
      },
    });
  }

  // Permanent delete — server only allows this while the release is still Draft.
  delete(r: IReleaseModel): void {
    if (!confirm(`Permanently delete release "${r.releaseName}" (${r.version})?`)) {
      return;
    }

    this.releaseService.delete(r.releaseId).subscribe({
      next: () => {
        this.toaster.success('Release deleted successfully');
        this.loadReleases();
      },
    });
  }

  get filteredReleases(): IReleaseModel[] {
    const term = this.search.trim().toLowerCase();
    return this.releases.filter((r) => {
      const matchesTerm =
        !term ||
        r.releaseName?.toLowerCase().includes(term) ||
        r.version?.toLowerCase().includes(term);
      const matchesEnv =
        !this.environmentFilter || r.environmentName === this.environmentFilter;
      const matchesStatus =
        !this.statusFilter || r.releaseLifecycle === this.statusFilter;
      return matchesTerm && matchesEnv && matchesStatus;
    });
  }

  // Same hash-based theme function as Environment Management, keyed by EnvironmentName
  // so a release's card header matches the color of the environment it belongs to
  // (visually groups releases by environment). Lifecycle status is conveyed separately
  // via the status pill, not the header color.
  getTheme(name: string): string {
    let hash = 0;
    for (let i = 0; i < (name || '').length; i++) {
      hash += name.charCodeAt(i);
    }
    return this.themes[hash % this.themes.length];
  }

  statusPillClass(lifecycle: string): string {
    switch ((lifecycle || '').toLowerCase()) {
      case 'active':
        return 'bg-success';
      case 'completed':
        return 'bg-primary';
      case 'rejected':
        return 'bg-danger';
      case 'draft':
        return 'bg-secondary';
      default:
        return 'bg-info text-dark';
    }
  }

  signOffPillClass(status: string): string {
    switch ((status || '').toLowerCase()) {
      case 'approved':
        return 'bg-success';
      case 'rejected':
        return 'bg-danger';
      default:
        return 'bg-secondary';
    }
  }
}

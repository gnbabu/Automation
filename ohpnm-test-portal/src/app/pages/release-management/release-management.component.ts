import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
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
export class ReleaseManagementComponent implements OnInit, OnDestroy {
  releases: IReleaseModel[] = [];
  environments: IEnvironmentModel[] = [];
  themes = ['green', 'orange', 'blue', 'purple', 'teal'];

  // filters
  search = '';
  environmentFilter = '';
  statusFilter = '';

  loading = false;

  // Auto-refresh: keeps DLL readiness badges / test summaries live across all cards
  // without requiring a manual "Refresh" click (mirrors test-case-execution-panel's
  // pattern). dllFileCount/folderReady is a cheap, non-reflective file count, so no
  // lifecycle-based pausing is needed here (unlike the Details page's readiness check).
  private refreshInterval: any = null;
  private readonly refreshSeconds = 10;
  isUserPerformingAction = false;

  constructor(
    private releaseService: ReleaseService,
    private envService: EnvironmentService,
    private toaster: CommonToasterService,
  ) {}

  ngOnInit(): void {
    this.loadEnvironments();
    this.loadReleases();
    this.startAutoRefresh();
  }

  ngOnDestroy(): void {
    this.stopAutoRefresh();
  }

  startAutoRefresh(): void {
    this.refreshInterval = setInterval(() => {
      if (this.isUserPerformingAction) {
        return;
      }
      this.loadReleases(true);
    }, this.refreshSeconds * 1000);
  }

  stopAutoRefresh(): void {
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval);
      this.refreshInterval = null;
    }
  }

  loadEnvironments(): void {
    this.envService.getAll().subscribe({
      next: (res) => (this.environments = res || []),
      error: () => (this.environments = []),
    });
  }

  // `silent` skips the loading spinner — used by the background auto-refresh tick so
  // it doesn't flicker the list every 10s; the manual Refresh button still shows it.
  loadReleases(silent = false): void {
    if (!silent) {
      this.loading = true;
    }
    this.releaseService.getAll().subscribe({
      next: (res) => {
        this.releases = res || [];
        this.loading = false;
      },
      error: () => {
        if (!silent) {
          this.releases = [];
        }
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
    this.isUserPerformingAction = true;
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
        this.isUserPerformingAction = false;
        this.toaster.success(
          `Release ${isDisabling ? 'deactivated' : 'activated'} successfully`,
        );
      },
      error: () => {
        this.isUserPerformingAction = false;
      },
    });
  }

  // Permanent delete — server only allows this while the release is still Draft.
  delete(r: IReleaseModel): void {
    if (!confirm(`Permanently delete release "${r.releaseName}" (${r.version})?`)) {
      return;
    }

    this.isUserPerformingAction = true;
    this.releaseService.delete(r.releaseId).subscribe({
      next: () => {
        this.isUserPerformingAction = false;
        this.toaster.success('Release deleted successfully');
        this.loadReleases();
      },
      error: () => {
        this.isUserPerformingAction = false;
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

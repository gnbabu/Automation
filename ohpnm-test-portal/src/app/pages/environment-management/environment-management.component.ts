import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { IEnvironmentModel, IEnvironmentRequestDto } from '@interfaces';
import {
  CommonToasterService,
  ConfirmService,
  EnvironmentService,
} from '@services';
import { pairBadgeTextColor } from 'app/core/utils/badge-class.util';

@Component({
  selector: 'app-environment-management',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './environment-management.component.html',
  styleUrl: './environment-management.component.css',
})
export class EnvironmentManagementComponent implements OnInit {
  environments: IEnvironmentModel[] = [];
  themes = ['green', 'orange', 'blue', 'purple', 'teal'];
  loading = false;

  // Search (name/description) + status filter, same pattern already used by
  // release-management.component.ts's own filtering.
  search = '';
  statusFilter: 'All' | 'Active' | 'Inactive' = 'All';

  // Exposed to the template so the Active/Inactive status pill pairs its bg-* class with
  // legible text color, same fix already applied everywhere else in the app this session.
  pairBadgeTextColor = pairBadgeTextColor;

  constructor(
    private envService: EnvironmentService,
    private toaster: CommonToasterService,
    private confirmService: ConfirmService,
  ) {}

  ngOnInit(): void {
    this.loadEnvironments();
  }

  loadEnvironments(): void {
    this.loading = true;
    this.envService.getAll().subscribe({
      next: (res) => {
        this.environments = res || [];
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load environments:', err);
        this.loading = false;
        this.toaster.error(
          err?.error?.message ?? err?.error ?? 'Failed to load environments.'
        );
      },
    });
  }

  get filteredEnvironments(): IEnvironmentModel[] {
    const term = this.search.trim().toLowerCase();
    return this.environments.filter((env) => {
      const matchesSearch =
        !term ||
        env.environmentName?.toLowerCase().includes(term) ||
        env.description?.toLowerCase().includes(term);

      const matchesStatus =
        this.statusFilter === 'All' ||
        (this.statusFilter === 'Active' && env.isActive) ||
        (this.statusFilter === 'Inactive' && !env.isActive);

      return matchesSearch && matchesStatus;
    });
  }

  getTheme(name: string): string {
    let hash = 0;
    for (let i = 0; i < name.length; i++) {
      hash += name.charCodeAt(i);
    }
    return this.themes[hash % this.themes.length];
  }

  toggle(env: IEnvironmentModel): void {
    const isDisabling = env.isActive;

    // Disable goes through softDelete() (previously dead code - the toggle always called
    // the generic update() for both directions) so "disable" and "edit name/description"
    // are two distinct, clearly-named server actions instead of one overloaded update()
    // call. Re-enabling still has to go through update() - there's no "un-soft-delete"
    // endpoint.
    const request$ = isDisabling
      ? this.envService.softDelete(env.environmentId)
      : this.envService.update({
          environmentId: env.environmentId,
          environmentName: env.environmentName,
          description: env.description,
          createdBy: env.createdBy,
          isActive: true,
        } as IEnvironmentRequestDto);

    request$.subscribe({
      next: () => {
        env.isActive = !env.isActive;
        this.toaster.success(
          `Environment ${isDisabling ? 'inactivated' : 'activated'} successfully`,
        );
      },
      error: (err) => {
        console.error('Failed to toggle environment:', err);
        this.toaster.error(
          err?.error?.message ?? err?.error ?? 'Failed to update environment status.'
        );
      },
    });
  }

  async delete(env: IEnvironmentModel): Promise<void> {
    // Defense in depth - the Delete button is already disabled in the template while
    // releaseCount > 0 (server-side guard also enforces this in
    // usp_EnvironmentHardDelete), but this method can still be reached in edge cases
    // (e.g. a stale releaseCount before a refresh), so check again before even asking to
    // confirm - no point offering a Yes/No prompt for a delete that will just fail anyway.
    if (env.releaseCount > 0) {
      this.toaster.error(
        `"${env.environmentName}" is used by ${env.releaseCount} release(s) and cannot be permanently deleted. Disable it instead.`
      );
      return;
    }

    const confirmed = await this.confirmService.confirm(
      'Delete Environment',
      `Permanently delete "${env.environmentName}"? This cannot be undone.`
    );
    if (!confirmed) return;

    this.envService.hardDelete(env.environmentId).subscribe({
      next: () => {
        this.toaster.success('Environment deleted successfully');
        this.loadEnvironments();
      },
      error: (err) => {
        console.error('Failed to delete environment:', err);
        this.toaster.error(
          err?.error?.message ?? err?.error ?? 'Failed to delete environment.'
        );
      },
    });
  }
}

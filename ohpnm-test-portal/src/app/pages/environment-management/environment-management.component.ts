import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { IEnvironmentModel, IEnvironmentRequestDto } from '@interfaces';
import { CommonToasterService, EnvironmentService } from '@services';
import { pairBadgeTextColor } from 'app/core/utils/badge-class.util';

@Component({
  selector: 'app-environment-management',
  imports: [CommonModule, RouterModule],
  templateUrl: './environment-management.component.html',
  styleUrl: './environment-management.component.css',
})
export class EnvironmentManagementComponent {
  environments: IEnvironmentModel[] = [];
  themes = ['green', 'orange', 'blue', 'purple', 'teal'];

  // Exposed to the template so the Active/Inactive status pill pairs its bg-* class with
  // legible text color, same fix already applied everywhere else in the app this session.
  pairBadgeTextColor = pairBadgeTextColor;

  constructor(
    private envService: EnvironmentService,
    private toaster: CommonToasterService,
  ) {}

  ngOnInit(): void {
    this.loadEnvironments();
  }

  loadEnvironments(): void {
    this.envService.getAll().subscribe((res) => {
      this.environments = res;
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
    const dto: IEnvironmentRequestDto = {
      environmentId: env.environmentId,
      environmentName: env.environmentName,
      description: env.description,
      createdBy: env.createdBy,
      isActive: !env.isActive,
    };

    this.envService.update(dto).subscribe(() => {
      env.isActive = !env.isActive;
      this.toaster.success(
        `Environment ${isDisabling ? 'inactivated' : 'activated'} successfully`,
      );
    });
  }

  delete(id: number): void {
    if (confirm('Permanently delete this environment?')) {
      this.envService.hardDelete(id).subscribe({
        next: () => {
          this.toaster.success(`Environment  deleted successfully`);

          this.loadEnvironments();
        },
      });
    }
  }
}

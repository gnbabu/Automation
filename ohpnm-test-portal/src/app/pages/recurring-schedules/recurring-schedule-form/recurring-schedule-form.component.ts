import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { IAssignmentOption, ILoginUserModel, IRecurringScheduleRequest } from '@interfaces';
import {
  CommonToasterService,
  EnvironmentService,
  LoginUserService,
  RecurringScheduleService,
} from '@services';
import { AppDropdownComponent } from 'app/core/components/app-dropdown/app-dropdown.component';

interface IEnvironmentFilterOption {
  environmentId: number;
  environment: string;
}

interface IReleaseFilterOption {
  releaseId: number;
  releaseName: string;
}

@Component({
  standalone: true,
  selector: 'app-recurring-schedule-form',
  imports: [CommonModule, FormsModule, AppDropdownComponent],
  templateUrl: './recurring-schedule-form.component.html',
  styleUrl: './recurring-schedule-form.component.css',
})
export class RecurringScheduleFormComponent implements OnInit {
  assignmentOptions: IAssignmentOption[] = [];
  filteredAssignmentOptions: IAssignmentOption[] = [];
  assignmentTextAccessor = (opt: IAssignmentOption) =>
    `${opt.assignmentName} — ${opt.environment} (${opt.releaseName})`;

  // Extends test-case-execution-panel.component.ts's own Release-filter-narrows-
  // Assignment pattern (selectedReleaseFilter/filteredAssignments) with an
  // Environment level above it - with 30+ assignments in a flat list, drilling down
  // Environment -> Release -> Assignment makes the final dropdown actually usable.
  environmentFilterOptions: IEnvironmentFilterOption[] = [];
  selectedEnvironmentFilter: IEnvironmentFilterOption | null = null;
  environmentFilterTextAccessor = (opt: IEnvironmentFilterOption) => opt.environment;

  releaseFilterOptions: IReleaseFilterOption[] = [];
  selectedReleaseFilter: IReleaseFilterOption | null = null;
  releaseFilterTextAccessor = (opt: IReleaseFilterOption) => opt.releaseName;

  // Options here are plain strings, not objects - AppDropdownComponent's default
  // textAccessor ('name') would look up a `.name` field that doesn't exist on a
  // string, so this just returns the option itself. Matches the exact convention
  // already used by ScheduleTestcasesDialogComponent/ReleaseManagementComponent.
  identityTextAccessor = (opt: string) => opt;

  assignmentId: number | undefined = undefined;
  recurrenceType = 'Daily';
  recurrenceTypeOptions = ['Daily', 'Weekly', 'Monthly'];

  dayOptions = [
    { value: 0, label: 'Sun' },
    { value: 1, label: 'Mon' },
    { value: 2, label: 'Tue' },
    { value: 3, label: 'Wed' },
    { value: 4, label: 'Thu' },
    { value: 5, label: 'Fri' },
    { value: 6, label: 'Sat' },
  ];
  selectedDays: number[] = [];
  dayOfMonth: number | null = null;
  timeOfDay = '22:00';
  browser = 'Chrome';
  browserOptions = ['Chrome', 'Edge'];
  loginUsers: ILoginUserModel[] = [];
  loginUserId: number | null = null;
  endDate = '';

  isSaving = false;

  constructor(
    private router: Router,
    private recurringScheduleService: RecurringScheduleService,
    private environmentService: EnvironmentService,
    private loginUserService: LoginUserService,
    private toaster: CommonToasterService,
  ) {}

  ngOnInit(): void {
    this.recurringScheduleService.getAssignmentOptions().subscribe({
      next: (res) => {
        this.assignmentOptions = res || [];
        this.buildEnvironmentFilterOptions();
        this.applyFilters();
      },
      error: () => {
        this.assignmentOptions = [];
        this.applyFilters();
      },
    });
  }

  private buildEnvironmentFilterOptions(): void {
    const seen = new Map<number, string>();
    for (const a of this.assignmentOptions) {
      if (a.environmentId != null && !seen.has(a.environmentId)) seen.set(a.environmentId, a.environment);
    }
    this.environmentFilterOptions = Array.from(seen, ([environmentId, environment]) => ({ environmentId, environment }));
  }

  private buildReleaseFilterOptions(source: IAssignmentOption[]): void {
    const seen = new Map<number, string>();
    for (const a of source) {
      if (!seen.has(a.releaseId)) seen.set(a.releaseId, a.releaseName);
    }
    this.releaseFilterOptions = Array.from(seen, ([releaseId, releaseName]) => ({ releaseId, releaseName }));
  }

  // Re-derives the Release options (scoped to the current Environment filter, if any)
  // and the final Assignment list (scoped to both filters, if set) from scratch every
  // time either filter changes - simpler and less error-prone than trying to patch
  // each level's list incrementally.
  private applyFilters(): void {
    let source = this.assignmentOptions;
    if (this.selectedEnvironmentFilter) {
      source = source.filter((a) => a.environmentId === this.selectedEnvironmentFilter!.environmentId);
    }

    this.buildReleaseFilterOptions(source);
    if (this.selectedReleaseFilter && !this.releaseFilterOptions.some((r) => r.releaseId === this.selectedReleaseFilter!.releaseId)) {
      // The previously-selected release no longer exists within the newly-chosen
      // Environment (e.g. switching Environment after already picking a Release) -
      // clear it rather than silently keeping a filter that no longer applies.
      this.selectedReleaseFilter = null;
    }
    if (this.selectedReleaseFilter) {
      source = source.filter((a) => a.releaseId === this.selectedReleaseFilter!.releaseId);
    }

    this.filteredAssignmentOptions = source;
  }

  // Keeps the Environment/Release dropdowns visually in sync with whichever Assignment
  // actually ends up selected - covers picking an Assignment directly without touching
  // the filters above it first, so the form never shows "All Environments"/"All
  // Releases" while a specific one is really in effect. Only updates the filter
  // dropdowns themselves (via object-reference lookups, since AppDropdownComponent's
  // default matching mode compares by reference) - deliberately does not call
  // applyFilters()/resetAssignmentSelection(), since that would wipe out the very
  // Assignment selection this is reacting to.
  private syncFiltersToAssignment(assignment: IAssignmentOption): void {
    this.selectedEnvironmentFilter =
      this.environmentFilterOptions.find((e) => e.environmentId === assignment.environmentId) ?? null;

    const environmentScoped = this.selectedEnvironmentFilter
      ? this.assignmentOptions.filter((a) => a.environmentId === this.selectedEnvironmentFilter!.environmentId)
      : this.assignmentOptions;
    this.buildReleaseFilterOptions(environmentScoped);
    this.selectedReleaseFilter =
      this.releaseFilterOptions.find((r) => r.releaseId === assignment.releaseId) ?? null;
  }

  onEnvironmentFilterChange(): void {
    this.resetAssignmentSelection();
    this.applyFilters();
  }

  onReleaseFilterChange(): void {
    this.resetAssignmentSelection();
    this.applyFilters();
  }

  private resetAssignmentSelection(): void {
    this.assignmentId = undefined;
    this.loginUsers = [];
    this.loginUserId = null;
  }

  // Same self-service resolution ScheduleTestcasesDialogComponent already uses -
  // resolves the current user's own configured Login User for the selected
  // assignment's environment (only shown/required if that environment actually
  // requires authentication).
  onAssignmentChange(): void {
    this.loginUsers = [];
    this.loginUserId = null;
    const selected = this.assignmentOptions.find((a) => a.assignmentId === this.assignmentId);
    if (selected) this.syncFiltersToAssignment(selected);

    const environmentId = selected?.environmentId;
    if (!environmentId) return;

    this.environmentService.getById(environmentId).subscribe({
      next: (env) => {
        if (!env.requiresAuthentication) return;
        this.loginUserService.getMineForEnvironment(environmentId).subscribe({
          next: (loginUsers) => (this.loginUsers = loginUsers.filter((lu) => lu.isActive)),
          error: () => (this.loginUsers = []),
        });
      },
      error: () => (this.loginUsers = []),
    });
  }

  get requiresLoginUser(): boolean {
    return this.loginUsers.length > 0;
  }

  loginUserLabel(lu: ILoginUserModel): string {
    return `${lu.userRole} - ${lu.userName}`;
  }

  toggleDay(day: number): void {
    const idx = this.selectedDays.indexOf(day);
    if (idx >= 0) this.selectedDays.splice(idx, 1);
    else this.selectedDays.push(day);
  }

  isDaySelected(day: number): boolean {
    return this.selectedDays.includes(day);
  }

  get isInvalid(): boolean {
    if (!this.assignmentId || !this.timeOfDay || !this.browser) return true;
    if (this.recurrenceType === 'Weekly' && this.selectedDays.length === 0) return true;
    if (this.recurrenceType === 'Monthly' && (!this.dayOfMonth || this.dayOfMonth < 1 || this.dayOfMonth > 31)) return true;
    if (this.requiresLoginUser && !this.loginUserId) return true;
    return false;
  }

  save(): void {
    if (this.isInvalid) {
      this.toaster.error('Please fill in all required fields.');
      return;
    }

    const request: IRecurringScheduleRequest = {
      assignmentId: this.assignmentId!,
      recurrenceType: this.recurrenceType,
      daysOfWeek: this.recurrenceType === 'Weekly' ? this.selectedDays.join(',') : undefined,
      dayOfMonth: this.recurrenceType === 'Monthly' ? this.dayOfMonth! : undefined,
      timeOfDay: this.timeOfDay,
      browser: this.browser,
      loginUserId: this.loginUserId ?? undefined,
      endDate: this.endDate || undefined,
    };

    this.isSaving = true;
    this.recurringScheduleService.create(request).subscribe({
      next: () => {
        this.toaster.success('Recurring schedule created successfully');
        this.router.navigate(['/recurring-schedules']);
      },
      error: () => {
        this.isSaving = false;
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/recurring-schedules']);
  }
}

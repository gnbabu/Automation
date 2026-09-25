import { CommonModule } from '@angular/common';
import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { GridColumn, IRecurringSchedule } from '@interfaces';
import { CommonToasterService, ConfirmService, RecurringScheduleService } from '@services';
import { DataGridComponent } from 'app/core/components/data-grid/data-grid.component';
import { pairBadgeTextColor } from 'app/core/utils/badge-class.util';

interface IRecurringScheduleRow extends IRecurringSchedule {
  recurrenceSummary: string;
}

@Component({
  standalone: true,
  selector: 'app-recurring-schedules',
  imports: [CommonModule, RouterModule, DataGridComponent],
  templateUrl: './recurring-schedules.component.html',
  styleUrl: './recurring-schedules.component.css',
})
export class RecurringSchedulesComponent implements OnInit {
  @ViewChild('nextRunTemplate', { static: true }) nextRunTemplate!: TemplateRef<any>;
  @ViewChild('lastRunTemplate', { static: true }) lastRunTemplate!: TemplateRef<any>;
  @ViewChild('createdByTemplate', { static: true }) createdByTemplate!: TemplateRef<any>;
  @ViewChild('statusTemplate', { static: true }) statusTemplate!: TemplateRef<any>;
  @ViewChild('actionsTemplate', { static: true }) actionsTemplate!: TemplateRef<any>;

  columns: GridColumn[] = [];
  schedules: IRecurringScheduleRow[] = [];
  loading = false;

  private readonly dayOptions = [
    { value: 0, label: 'Sun' },
    { value: 1, label: 'Mon' },
    { value: 2, label: 'Tue' },
    { value: 3, label: 'Wed' },
    { value: 4, label: 'Thu' },
    { value: 5, label: 'Fri' },
    { value: 6, label: 'Sat' },
  ];

  constructor(
    private recurringScheduleService: RecurringScheduleService,
    private confirmService: ConfirmService,
    private toaster: CommonToasterService,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.setupColumns();
    this.load();
  }

  private setupColumns(): void {
    this.columns = [
      { field: 'assignmentName', header: 'Assignment', sortable: true },
      { field: 'environment', header: 'Environment', sortable: true },
      { field: 'releaseName', header: 'Release', sortable: true },
      { field: 'recurrenceSummary', header: 'Recurrence', sortable: false },
      { field: 'nextRunDate', header: 'Next Run', sortable: true, cellTemplate: this.nextRunTemplate },
      { field: 'lastRunDate', header: 'Last Run', sortable: true, cellTemplate: this.lastRunTemplate },
      { field: 'runCount', header: 'Runs', sortable: true },
      { field: 'createdBy', header: 'Created By', sortable: true, cellTemplate: this.createdByTemplate },
      { field: 'isActive', header: 'Status', sortable: true, cellTemplate: this.statusTemplate },
      { field: 'recurringScheduleId', header: 'Actions', sortable: false, cellTemplate: this.actionsTemplate },
    ];
  }

  load(): void {
    this.loading = true;
    this.recurringScheduleService.getAll().subscribe({
      next: (res) => {
        this.schedules = (res || []).map((s) => ({ ...s, recurrenceSummary: this.summarize(s) }));
        this.loading = false;
      },
      error: () => {
        this.schedules = [];
        this.loading = false;
      },
    });
  }

  private summarize(s: IRecurringSchedule): string {
    const time = s.timeOfDay?.substring(0, 5) ?? '';
    if (s.recurrenceType === 'Daily') return `Daily at ${time}`;
    if (s.recurrenceType === 'Weekly') {
      const names = (s.daysOfWeek || '')
        .split(',')
        .filter((d) => d !== '')
        .map((d) => this.dayOptions.find((o) => o.value === +d)?.label)
        .filter(Boolean)
        .join(', ');
      return `Weekly on ${names} at ${time}`;
    }
    if (s.recurrenceType === 'Monthly') return `Monthly on day ${s.dayOfMonth} at ${time}`;
    return s.recurrenceType;
  }

  pause(row: IRecurringScheduleRow): void {
    this.recurringScheduleService.pause(row.recurringScheduleId).subscribe({
      next: () => {
        this.toaster.success('Schedule paused.');
        this.load();
      },
    });
  }

  resume(row: IRecurringScheduleRow): void {
    this.recurringScheduleService.resume(row.recurringScheduleId).subscribe({
      next: () => {
        this.toaster.success('Schedule resumed.');
        this.load();
      },
    });
  }

  async delete(row: IRecurringScheduleRow): Promise<void> {
    const confirmed = await this.confirmService.confirm(
      'Delete Recurring Schedule',
      `Are you sure you want to permanently delete the recurring schedule for "${row.assignmentName}"?`
    );
    if (!confirmed) return;

    this.recurringScheduleService.delete(row.recurringScheduleId).subscribe({
      next: () => {
        this.toaster.success('Recurring schedule deleted.');
        this.load();
      },
    });
  }

  statusPillClass(isActive: boolean): string {
    return isActive ? pairBadgeTextColor('bg-success') : pairBadgeTextColor('bg-secondary');
  }

  viewHistory(row: IRecurringScheduleRow): void {
    this.router.navigate(['/recurring-schedules', row.recurringScheduleId, 'history']);
  }

  edit(row: IRecurringScheduleRow): void {
    this.router.navigate(['/recurring-schedules', row.recurringScheduleId, 'edit']);
  }
}

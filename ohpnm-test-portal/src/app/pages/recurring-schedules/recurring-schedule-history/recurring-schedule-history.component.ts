import { CommonModule } from '@angular/common';
import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { GridColumn, IRecurringSchedule, IRecurringScheduleRunHistory } from '@interfaces';
import { RecurringScheduleService } from '@services';
import { DataGridComponent } from 'app/core/components/data-grid/data-grid.component';
import { pairBadgeTextColor } from 'app/core/utils/badge-class.util';

@Component({
  standalone: true,
  selector: 'app-recurring-schedule-history',
  imports: [CommonModule, RouterModule, DataGridComponent],
  templateUrl: './recurring-schedule-history.component.html',
  styleUrl: './recurring-schedule-history.component.css',
})
export class RecurringScheduleHistoryComponent implements OnInit {
  @ViewChild('runDateTemplate', { static: true }) runDateTemplate!: TemplateRef<any>;
  @ViewChild('resultTemplate', { static: true }) resultTemplate!: TemplateRef<any>;
  @ViewChild('outcomeTemplate', { static: true }) outcomeTemplate!: TemplateRef<any>;
  @ViewChild('detailTemplate', { static: true }) detailTemplate!: TemplateRef<any>;

  columns: GridColumn[] = [];
  history: IRecurringScheduleRunHistory[] = [];
  schedule: IRecurringSchedule | null = null;
  scheduleId!: number;
  loading = false;

  // Rolled up across every *resolved* firing - gives an at-a-glance lifetime totals view
  // (mirrors Dashboard's own 4-card KPI summary look/feel).
  totalPassed = 0;
  totalFailed = 0;
  totalSkipped = 0;
  pendingCount = 0;

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
    private route: ActivatedRoute,
    private router: Router,
    private recurringScheduleService: RecurringScheduleService,
  ) {}

  ngOnInit(): void {
    this.scheduleId = +(this.route.snapshot.paramMap.get('id') ?? 0);
    this.setupColumns();
    this.load();
  }

  private setupColumns(): void {
    this.columns = [
      { field: 'runDate', header: 'Run Date', sortable: true, cellTemplate: this.runDateTemplate },
      { field: 'result', header: 'Result', sortable: true, cellTemplate: this.resultTemplate },
      { field: 'testCasesQueuedCount', header: 'Test Cases Queued', sortable: true },
      { field: 'resolutionStatus', header: 'Outcome', sortable: false, cellTemplate: this.outcomeTemplate },
      { field: 'detail', header: 'Detail', sortable: false, cellTemplate: this.detailTemplate },
    ];
  }

  load(): void {
    this.loading = true;
    // getAll() is reused here for the header summary rather than adding a dedicated
    // GetById endpoint - this is a low-traffic internal admin page and the schedule list
    // is never large enough for this extra round trip to matter.
    this.recurringScheduleService.getAll().subscribe({
      next: (schedules) => {
        this.schedule = (schedules || []).find((s) => s.recurringScheduleId === this.scheduleId) ?? null;
      },
    });

    this.recurringScheduleService.getHistory(this.scheduleId).subscribe({
      next: (res) => {
        this.history = res || [];
        this.computeTotals();
        this.loading = false;
      },
      error: () => {
        this.history = [];
        this.loading = false;
      },
    });
  }

  private computeTotals(): void {
    this.totalPassed = this.history.reduce((sum, h) => sum + (h.passedCount ?? 0), 0);
    this.totalFailed = this.history.reduce((sum, h) => sum + (h.failedCount ?? 0), 0);
    this.totalSkipped = this.history.reduce((sum, h) => sum + (h.skippedCount ?? 0), 0);
    this.pendingCount = this.history.filter((h) => h.resolutionStatus === 'Pending').length;
  }

  outcomeTotal(row: IRecurringScheduleRunHistory): number {
    return (row.passedCount ?? 0) + (row.failedCount ?? 0) + (row.skippedCount ?? 0);
  }

  outcomePct(count: number | undefined, row: IRecurringScheduleRunHistory): number {
    const total = this.outcomeTotal(row);
    return total > 0 ? ((count ?? 0) / total) * 100 : 0;
  }

  resultIcon(result: string): string {
    if (result === 'Queued') return 'fa-play-circle';
    if (result === 'Paused') return 'fa-pause-circle';
    return 'fa-info-circle';
  }

  recurrenceSummary(s: IRecurringSchedule): string {
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

  loginUserLabel(s: IRecurringSchedule): string {
    if (!s.loginUserId) return '—';
    return s.loginUserRole || s.loginUserName ? `${s.loginUserRole} - ${s.loginUserName}` : '—';
  }

  statusPillClass(isActive: boolean): string {
    return isActive ? pairBadgeTextColor('bg-success') : pairBadgeTextColor('bg-secondary');
  }

  resultPillClass(result: string): string {
    if (result === 'Queued') return pairBadgeTextColor('bg-success');
    if (result === 'Paused') return pairBadgeTextColor('bg-secondary');
    return pairBadgeTextColor('bg-warning');
  }

  back(): void {
    this.router.navigate(['/recurring-schedules']);
  }
}

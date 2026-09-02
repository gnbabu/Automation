import { CommonModule } from '@angular/common';
import {
  Component,
  OnDestroy,
  OnInit,
  TemplateRef,
  ViewChild,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  GridColumn,
  IAssignedTestCase,
  IReleaseModel,
  ITestCaseAssignmentEntity,
  ITestScreenshot,
} from '@interfaces';
import {
  AuthService,
  CommonToasterService,
  ConfirmService,
  ReleaseService,
  ScreenshotService,
  TestCaseAssignmentService,
  TestCaseExecutionLogsService,
  TestCaseExecutionService,
} from '@services';
import { AppDropdownComponent } from 'app/core/components/app-dropdown/app-dropdown.component';
import { DataGridComponent } from 'app/core/components/data-grid/data-grid.component';
import { ConfirmDialogComponent } from 'app/core/modals/confirm-dialog/confirm-dialog.component';
import { ScheduleTestcasesDialogComponent } from './schedule-testcases-dialog/schedule-testcases-dialog.component';
import { TestScreenshotGalleryComponent } from './test-screenshot-gallery/test-screenshot-gallery.component';
import { ExecutionLogsDialogComponent } from 'app/common-modals/execution-logs-dialog/execution-logs-dialog.component';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-test-case-execution',
  imports: [
    AppDropdownComponent,
    CommonModule,
    FormsModule,
    DataGridComponent,
    ScheduleTestcasesDialogComponent,
    TestScreenshotGalleryComponent,
    ExecutionLogsDialogComponent,
  ],
  standalone: true,
  templateUrl: './test-case-execution-panel.component.html',
  styleUrl: './test-case-execution-panel.component.css',
})
export class TestCaseExecutionPanelComponent implements OnInit, OnDestroy {
  constructor(
    private authService: AuthService,
    private toaster: CommonToasterService,
    private testCaseAssignmentService: TestCaseAssignmentService,
    private confirmService: ConfirmService,
    private testCaseExecutionService: TestCaseExecutionService,
    private screenshotService: ScreenshotService,
    private executionLogsService: TestCaseExecutionLogsService,
    private releaseService: ReleaseService
  ) {}

  @ViewChild('testCaseIdTemplate', { static: true })
  testCaseIdTemplate!: TemplateRef<any>;

  @ViewChild('priorityTemplate', { static: true })
  priorityTemplate!: TemplateRef<any>;

  @ViewChild('statusTemplate', { static: true })
  statusTemplate!: TemplateRef<any>;

  @ViewChild('actionsTemplate', { static: true })
  actionsTemplate!: TemplateRef<any>;

  @ViewChild('confirmDialog') confirmDialog!: ConfirmDialogComponent;

  @ViewChild('scheduleDialog')
  scheduleDialog!: ScheduleTestcasesDialogComponent;

  @ViewChild(TestScreenshotGalleryComponent)
  gallery!: TestScreenshotGalleryComponent;

  @ViewChild('logsDialog')
  executionLogsDialog!: ExecutionLogsDialogComponent;

  assignments: ITestCaseAssignmentEntity[] = [];
  filteredAssignments: ITestCaseAssignmentEntity[] = [];
  selectedAssignment: ITestCaseAssignmentEntity | null = null;

  releases: IReleaseModel[] = [];
  releaseFilterOptions: IReleaseModel[] = [];
  selectedReleaseFilter: IReleaseModel | null = null;
  selectedAssignmentRelease: IReleaseModel | null = null;

  columns: GridColumn[] = [];
  testCases: IAssignedTestCase[] = [];
  selectedTestCases: IAssignedTestCase[] = [];
  screenshots: ITestScreenshot[] = [];

  stats = {
    totalAssigned: 0,
    pendingExecution: 0,
    completed: 0,
  };

  refreshInterval: any = null;
  isUserPerformingAction = false;
  refreshSeconds = 10; // every 10 seconds
  lastUpdated: Date | null = null;

  ngOnInit(): void {
    this.loadAssignments();
    this.setupColumns();
    this.startAutoRefresh();
  }

  startAutoRefresh() {
    this.refreshInterval = setInterval(() => {
      if (this.isUserPerformingAction) {
        console.log('⏸ Auto-refresh paused due to user action');
        return;
      }

      console.log('♻ Auto-refreshing test cases...');
      this.loadAssignedTestCases();
      // Refresh Release lifecycles too, so the Active-only execution guard stays
      // accurate without requiring a manual page reload.
      this.loadReleases();
      this.lastUpdated = new Date(); // update timestamp
    }, this.refreshSeconds * 1000);
  }

  stopAutoRefresh() {
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval);
      this.refreshInterval = null;
    }
  }

  loadAssignments() {
    const userId = this.authService?.getLoggedInUserId(); // or however you store logged-in user ID

    if (!userId) {
      console.error('UserId is required to load assignments');
      return;
    }

    forkJoin({
      releases: this.releaseService.getAll(),
      assignments: this.testCaseAssignmentService.getAssignmentsByUserId(userId),
    }).subscribe({
      next: ({ releases, assignments }) => {
        this.releases = releases || [];
        this.assignments = assignments || [];
        this.buildReleaseFilterOptions();

        // Auto-select the first Release filter option by default, same convention as
        // Dashboard's "Select Release" dropdown (onReleaseChange(this.releases[0])) -
        // previously this picked the first *assignment* while leaving the Release
        // dropdown on its "All Releases" placeholder, so the dropdown didn't reflect
        // what was actually being shown. Falls back to showing all assignments
        // unfiltered only if the user genuinely has none scoped to any release.
        if (this.releaseFilterOptions.length > 0) {
          this.onReleaseFilterChange(this.releaseFilterOptions[0]);
        } else {
          this.filteredAssignments = this.assignments;
          if (this.filteredAssignments.length > 0) {
            this.onAssignmentChange(this.filteredAssignments[0]);
          }
        }
      },
      error: (err) => console.error('Failed to load assignments:', err),
    });
  }

  loadReleases() {
    this.releaseService.getAll().subscribe({
      next: (res) => {
        this.releases = res || [];
        this.updateSelectedAssignmentRelease();
      },
      error: (err) => console.error('Failed to refresh releases:', err),
    });
  }

  buildReleaseFilterOptions() {
    const releaseIds = new Set(
      this.assignments.map((a) => a.releaseId).filter((id) => !!id)
    );
    this.releaseFilterOptions = this.releases.filter((r) =>
      releaseIds.has(r.releaseId)
    );
  }

  updateSelectedAssignmentRelease() {
    this.selectedAssignmentRelease = this.selectedAssignment
      ? this.releases.find(
          (r) => r.releaseId === this.selectedAssignment!.releaseId
        ) ?? null
      : null;
  }

  onReleaseFilterChange(release: IReleaseModel | null) {
    this.selectedReleaseFilter = release;
    this.filteredAssignments = release
      ? this.assignments.filter((a) => a.releaseId === release.releaseId)
      : this.assignments;

    if (this.filteredAssignments.length > 0) {
      this.onAssignmentChange(this.filteredAssignments[0]);
    } else {
      this.selectedAssignment = null;
      this.selectedAssignmentRelease = null;
      this.testCases = [];
      this.resetStats();
    }
  }

  onAssignmentChange(assignment: ITestCaseAssignmentEntity) {
    this.selectedAssignment = assignment;
    this.updateSelectedAssignmentRelease();

    this.stopAutoRefresh(); // clear old interval
    this.loadAssignedTestCases(); // load immediately

    this.startAutoRefresh(); // 🔥 start polling for this assignment
    console.log('Selected Assignment:', assignment);
  }

  isReleaseActive(): boolean {
    return this.selectedAssignmentRelease?.releaseLifecycle === 'Active';
  }

  isViewer(): boolean {
    return this.authService.isViewer();
  }

  // Viewers have read-only access — Run Now/Schedule (single + bulk) stay disabled for
  // them regardless of the release's lifecycle. Combined with isReleaseActive() so
  // there's a single guard used everywhere (buttons, row selectability, submit handlers).
  canExecuteTests(): boolean {
    return this.isReleaseActive() && !this.isViewer();
  }

  // Same convention as release-management.component.ts's statusPillClass, so the
  // lifecycle badge here matches the color used everywhere else in the app.
  releaseLifecycleBadgeClass(lifecycle?: string): string {
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

  loadAssignedTestCases() {
    if (!this.selectedAssignment) {
      this.testCases = [];
      return;
    }

    this.testCaseAssignmentService
      .getTestCasesByAssignmentAndUser(
        this.authService.getLoggedInUserId(),
        this.selectedAssignment.assignmentName
      )
      .subscribe({
        next: (data) => {
          this.testCases = data;
          this.calculateStats();
        },
        error: (err) => {
          console.error('Error loading test cases:', err);
        },
      });
  }
  onSelectionChanged(selectedRows: IAssignedTestCase[]) {}

  setupColumns() {
    this.columns = [
      {
        field: 'testCaseId',
        header: 'Test Case ID',
        sortable: false,
        cellTemplate: this.testCaseIdTemplate,
      },
      {
        field: 'methodName',
        header: 'Test Case Name',
        sortable: true,
      },
      {
        field: 'testCaseDescription',
        header: 'Description',
        sortable: true,
      },
      {
        field: 'environment',
        header: 'Environment',
        sortable: false,
      },
      {
        field: 'priority',
        header: 'Priority',
        sortable: true,
        cellTemplate: this.priorityTemplate,
      },
      {
        field: 'testCaseStatus',
        header: 'Status',
        sortable: false,
        cellTemplate: this.statusTemplate,
      },
      {
        field: '',
        header: 'Actions',
        sortable: false,
        cellTemplate: this.actionsTemplate, // 🔥 new template
        width: '180px',
      },
    ];
  }

  getBadgeClass(status?: string): string {
    switch (status) {
      case 'Assigned':
        return 'bg-primary text-white';

      case 'Queued':
        return 'bg-light text-dark border';

      case 'Scheduled':
        return 'bg-info text-dark';

      case 'InProgress':
        return 'bg-warning text-dark';

      case 'Passed':
        return 'bg-success text-white';

      case 'Failed':
        return 'bg-danger text-white';

      case 'Cancelled':
        return 'bg-dark text-white';

      case 'Skipped':
        return 'bg-secondary text-white';

      case 'Inconclusive':
        return 'bg-warning text-dark';

      default:
        return 'bg-light text-dark border';
    }
  }

  calculateStats() {
    this.stats.totalAssigned = this.testCases.length;

    this.stats.completed = this.testCases.filter(
      (x) => x.testCaseStatus === 'Passed'
    ).length;

    this.stats.pendingExecution = this.testCases.filter(
      (x) => x.testCaseStatus === 'Assigned'
    ).length;
  }

  resetStats() {
    this.stats = {
      totalAssigned: 0,
      pendingExecution: 0,
      completed: 0,
    };
  }

  async onRunNow(testCase: IAssignedTestCase) {
    if (!this.canExecuteTests()) {
      this.toaster.error(this.getBlockedExecutionMessage());
      return;
    }

    this.isUserPerformingAction = true;

    const confirmed = await this.confirmService.confirm(
      'Run Test Case',
      `Are you sure you want to run Test Case "${testCase.testCaseId}" now?`
    );

    if (!confirmed) {
      this.isUserPerformingAction = false;
      return;
    }

    const payload = {
      assignmentId: this.selectedAssignment?.assignmentId!,
      assignmentTestCaseId: testCase.assignmentTestCaseId,
      browser: 'Chrome',
    };

    this.testCaseExecutionService.singleRunNow(payload).subscribe({
      next: () => {
        this.toaster.success('Test case added to execution queue.');
        this.loadAssignedTestCases();
        this.isUserPerformingAction = false; // resume refresh
      },
      error: () => {
        this.toaster.error('Failed to queue test case.');
        this.isUserPerformingAction = false;
      },
    });
  }

  combineDateAndTime(date: string, time: string): Date {
    const dateObj = new Date(date);
    const [hours, minutes] = time.split(':').map(Number);

    dateObj.setHours(hours);
    dateObj.setMinutes(minutes);
    dateObj.setSeconds(0);
    dateObj.setMilliseconds(0);

    return dateObj;
  }

  private formatLocalDateTime(date: Date): string {
    const pad = (n: number) => n.toString().padStart(2, '0');

    const year = date.getFullYear();
    const month = pad(date.getMonth() + 1);
    const day = pad(date.getDate());

    const hours = pad(date.getHours());
    const minutes = pad(date.getMinutes());
    const seconds = pad(date.getSeconds());

    // ISO without timezone — .NET accepts this perfectly
    return `${year}-${month}-${day}T${hours}:${minutes}:${seconds}`;
  }

  onSchedule(testCase: IAssignedTestCase) {
    if (!this.canExecuteTests()) {
      this.toaster.error(this.getBlockedExecutionMessage());
      return;
    }

    this.isUserPerformingAction = true;

    this.scheduleDialog.open((data: any) => {
      const scheduleDate = this.combineDateAndTime(data.date, data.time);

      const payload = {
        assignmentId: this.selectedAssignment?.assignmentId!,
        assignmentTestCaseId: testCase.assignmentTestCaseId,
        scheduleDate: this.formatLocalDateTime(scheduleDate),
        browser: data.browser,
      };

      this.testCaseExecutionService.singleSchedule(payload).subscribe({
        next: () => {
          this.toaster.success('Test case scheduled successfully.');
          this.loadAssignedTestCases();
          this.isUserPerformingAction = false;
        },
        error: () => {
          this.toaster.error('Failed to schedule test case.');
          this.isUserPerformingAction = false;
        },
      });
    });
  }

  async onBulkRunNow() {
    if (!this.canExecuteTests()) {
      this.toaster.error(this.getBlockedExecutionMessage());
      return;
    }

    this.isUserPerformingAction = true;

    if (!this.selectedTestCases.length) {
      this.isUserPerformingAction = false;
      return;
    }

    const testCaseIds = this.selectedTestCases
      .map((t) => t.testCaseId)
      .join(', ');

    const confirmed = await this.confirmService.confirm(
      'Bulk Run Now',
      `Are you sure you want to run these test cases?\n\n${testCaseIds}`
    );

    if (!confirmed) {
      this.isUserPerformingAction = false;
      return;
    }

    const payload = {
      assignmentId: this.selectedAssignment?.assignmentId!,
      assignmentTestCaseIds: this.selectedTestCases.map(
        (t) => t.assignmentTestCaseId
      ),
      browser: 'Chrome',
    };

    this.testCaseExecutionService.bulkRunNow(payload).subscribe({
      next: () => {
        this.toaster.success('Selected test cases queued successfully.');
        this.loadAssignedTestCases();
        this.isUserPerformingAction = false;
      },
      error: () => {
        this.toaster.error('Failed to queue test cases.');
        this.isUserPerformingAction = false;
      },
    });
  }

  onBulkSchedule() {
    if (!this.canExecuteTests()) {
      this.toaster.error(this.getBlockedExecutionMessage());
      return;
    }

    this.isUserPerformingAction = true;

    if (!this.selectedTestCases || this.selectedTestCases.length === 0) {
      this.isUserPerformingAction = false;
      return;
    }

    this.scheduleDialog.open((data: any) => {
      const scheduleDate = this.combineDateAndTime(data.date, data.time);

      const payload = {
        assignmentId: this.selectedAssignment?.assignmentId!,
        assignmentTestCaseIds: this.selectedTestCases.map(
          (t) => t.assignmentTestCaseId
        ),
        scheduleDate: this.formatLocalDateTime(scheduleDate),
        browser: data.browser,
      };

      this.testCaseExecutionService.bulkSchedule(payload).subscribe({
        next: () => {
          this.toaster.success('Bulk schedule created successfully.');
          this.loadAssignedTestCases();
          this.isUserPerformingAction = false;
        },
        error: () => {
          this.toaster.error('Failed to bulk schedule test cases.');
          this.isUserPerformingAction = false;
        },
      });
    });
  }

  // Arrow function (not a regular method) so `this` stays bound to the component
  // instance even when DataGridComponent invokes it directly as a plain callback
  // (via [rowSelectableFn]) without preserving the calling context.
  isTestCaseSelectable = (row: any): boolean => {
    const disabledStatuses = [
      'Queued',
      'Scheduled',
      'InProgress',
      'Passed',
      'Failed',
      'Cancelled',
      // NUnit's own outcomes ([Ignore]/[Explicit] -> Skipped; an unresolved assertion ->
      // Inconclusive), now that the runner reports these honestly instead of forcing
      // everything into Pass/Fail - see AGENTS.md.
      'Skipped',
      'Inconclusive',
    ];

    return (
      !disabledStatuses.includes(row.testCaseStatus ?? '') &&
      this.canExecuteTests()
    );
  };

  private getBlockedExecutionMessage(): string {
    if (this.isViewer()) {
      return 'Viewers have read-only access and cannot run or schedule test executions.';
    }

    return `This release is ${this.selectedAssignmentRelease?.releaseLifecycle}. New executions are disabled.`;
  }

  ngOnDestroy(): void {
    this.stopAutoRefresh();
  }

  onViewScreenshots(testCase: any) {
    this.screenshotService
      .getScreenshotsByAssignmentTestCaseIdAsync(testCase.assignmentTestCaseId)
      .subscribe({
        next: (res) => {
          this.screenshots = res;

          // Wait for child to receive input and render modal DOM
          setTimeout(() => {
            this.gallery?.open();
          }, 150);
        },
        error: (err) => console.error('Failed to load screenshots:', err),
      });
  }

  onViewLogs(row: any) {
    this.executionLogsService
      .getTestCaseLogs(row.assignmentId, row.assignmentTestCaseId)
      .subscribe((logs) => {
        this.executionLogsDialog.open(logs);
      });
  }
}

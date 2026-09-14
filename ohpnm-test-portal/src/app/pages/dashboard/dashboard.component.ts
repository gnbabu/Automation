import { CommonModule } from '@angular/common';
import {
  Component,
  OnDestroy,
  OnInit,
  TemplateRef,
  ViewChild,
} from '@angular/core';

import {
  GridColumn,
  IAssignedTestCase,
  IReleaseModel,
  ITestCaseExecutionLog,
  ITestCaseModel,
  ITestScreenshot,
  LibraryInfo,
} from '@interfaces';
import {
  AuthService,
  CommonToasterService,
  ReleaseService,
  ScreenshotService,
  TestCaseAssignmentService,
  TestCaseExecutionLogsService,
  TestSuitesService,
  UsersService,
} from '@services';
import { DataGridComponent } from 'app/core/components/data-grid/data-grid.component';
import { AppDropdownComponent } from 'app/core/components/app-dropdown/app-dropdown.component';
import { forkJoin, map } from 'rxjs';
import { TestScreenshotGalleryComponent } from '../test-case-execution-panel/test-screenshot-gallery/test-screenshot-gallery.component';
import { ExecutionLogsViewerComponent } from 'app/common-components/execution-logs-viewer/execution-logs-viewer.component';
import { ExecutionLogsDialogComponent } from 'app/common-modals/execution-logs-dialog/execution-logs-dialog.component';
import { pairBadgeTextColor } from 'app/core/utils/badge-class.util';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    AppDropdownComponent,
    DataGridComponent,
    TestScreenshotGalleryComponent,
    ExecutionLogsViewerComponent,
    ExecutionLogsDialogComponent,
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
})
export class DashboardComponent implements OnInit, OnDestroy {
  @ViewChild('testCaseIdTemplate', { static: true })
  testCaseIdTemplate!: TemplateRef<any>;

  @ViewChild('priorityTemplate', { static: true })
  priorityTemplate!: TemplateRef<any>;

  @ViewChild('statusTemplate', { static: true })
  statusTemplate!: TemplateRef<any>;

  @ViewChild('actionsTemplate', { static: true })
  actionsTemplate!: TemplateRef<any>;

  @ViewChild(TestScreenshotGalleryComponent)
  gallery!: TestScreenshotGalleryComponent;
  testCases: IAssignedTestCase[] = [];
  columns: GridColumn[] = [];
  pageSize = 10;

  releases: IReleaseModel[] = [];
  selectedRelease: IReleaseModel | null = null;

  screenshots: ITestScreenshot[] = [];

  totalCases = 0;
  assignedCount = 0;
  unassignedCount = 0;

  passedCount = 0;
  failedCount = 0;
  runningCount = 0;
  skippedCount = 0;

  // Testers see a personalized "My Results" view scoped to just their own assigned
  // test cases, instead of the full release-wide table Admin/Manager/Viewer see -
  // Viewers are deliberately excluded from this (see AuthService.isTester()) even
  // though the assignment UI doesn't technically prevent assigning one, since they're
  // meant to be read-only overseers, not executors. Computed client-side from data
  // already loaded for the release-wide view - no extra API call needed, since
  // IAssignedTestCase already carries assignedUserId per row.
  myTestCases: IAssignedTestCase[] = [];
  myTotalCases = 0;
  myPassedCount = 0;
  myFailedCount = 0;
  myRunningSkippedCount = 0;

  executionLogs: ITestCaseExecutionLog[] = [];
  recentLogs: ITestCaseExecutionLog[] = [];
  showFullLogs = false;

  overallStartTime: Date | null = null;
  overallEndTime: Date | null = null;
  runStatusLabel: 'Not Started' | 'In Progress' | 'Completed' = 'Not Started';
  executionDurationLabel = '—';
  testersInvolvedCount = 0;
  averageTestDurationLabel = '—';

  // Freshness indicator - previously data was only ever as fresh as the last manual
  // Refresh click, with no indication of how stale it might be. lastUpdatedLabel ticks
  // forward on its own (tickTimer) independent of any actual data refresh, so "2
  // minutes ago" keeps advancing even if nothing new has happened. Separately,
  // pollTimer auto-refreshes the data itself, but only while the release is actually
  // "In Progress" - a Completed or Not Started release has nothing new to fetch, so
  // polling then would just be wasted requests.
  lastUpdatedAt: Date | null = null;
  lastUpdatedLabel = '—';
  private tickTimer?: ReturnType<typeof setInterval>;
  private pollTimer?: ReturnType<typeof setInterval>;
  private readonly TICK_INTERVAL_MS = 15000;
  private readonly POLL_INTERVAL_MS = 30000;

  @ViewChild('logsDialog')
  executionLogsDialog!: ExecutionLogsDialogComponent;

  // You can add any logic for the dashboard component here if needed
  constructor(
    private testSuitesService: TestSuitesService,
    private authService: AuthService,
    private toaster: CommonToasterService,
    private userService: UsersService,
    private testCaseAssignmentService: TestCaseAssignmentService,
    private screenshotService: ScreenshotService,
    private executionLogsService: TestCaseExecutionLogsService,
    private releaseService: ReleaseService
  ) {}

  ngOnInit(): void {
    this.loadReleases();
    this.setupColumns();
    this.tickTimer = setInterval(
      () => this.updateLastUpdatedLabel(),
      this.TICK_INTERVAL_MS
    );
  }

  loadReleases() {
    this.releaseService.getAll().subscribe({
      next: (res) => {
        this.releases = (res || []).filter((r) =>
          ['Active', 'Completed'].includes(r.releaseLifecycle)
        );

        // Auto-select the most recently created release (GET /api/Release is already
        // sorted by CreatedOn DESC) so the page shows data immediately, matching the
        // same "auto-select first item" convention used on the Assignment/Execution
        // Panel screens instead of starting on an empty state.
        if (this.releases.length > 0) {
          this.onReleaseChange(this.releases[0]);
        }
      },
      error: () => (this.releases = []),
    });
  }

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
        field: 'duration',
        header: 'Duration',
        sortable: false,
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

  onReleaseChange(release: IReleaseModel | null) {
    this.selectedRelease = release;

    this.testCases = [];
    this.executionLogs = [];
    this.recentLogs = [];
    this.resetSummaryCounts();
    this.resetRunTimeline();

    if (!release) return;

    this.refreshReleaseData();
  }

  // Backs both the initial load-on-select and the manual Refresh button, so either
  // path re-pulls the same data for whichever Release is currently selected.
  refreshReleaseData() {
    if (!this.selectedRelease) return;

    const releaseId = this.selectedRelease.releaseId;

    this.mergeReleaseTestCases(releaseId).subscribe((merged) => {
      this.testCases = merged;

      this.passedCount = merged.filter(
        (tc) => tc.testCaseStatus === 'Passed'
      ).length;

      this.failedCount = merged.filter(
        (tc) => tc.testCaseStatus === 'Failed'
      ).length;

      this.runningCount = merged.filter(
        (tc) =>
          tc.testCaseStatus === 'InProgress' ||
          tc.testCaseStatus === 'Scheduled' ||
          tc.testCaseStatus === 'Queued'
      ).length;

      this.skippedCount = merged.filter(
        (tc) =>
          tc.testCaseStatus === 'Skipped' || tc.testCaseStatus === 'Cancelled'
      ).length;

      this.computeMyCounts(merged);
      this.computeRunTimeline(merged);

      this.lastUpdatedAt = new Date();
      this.updateLastUpdatedLabel();
      this.startPollingIfRunning();
    });

    this.loadReleaseExecutionLogs(releaseId);
  }

  get isPersonalizedView(): boolean {
    return this.authService.isTester();
  }

  // Pure client-side filter against data already loaded for the release-wide view -
  // no extra API call needed, IAssignedTestCase already carries assignedUserId per row.
  private computeMyCounts(merged: IAssignedTestCase[]): void {
    const loggedInUserId = this.authService.getLoggedInUserId();
    this.myTestCases = merged.filter(
      (tc) => tc.assignedUserId === loggedInUserId
    );
    this.myTotalCases = this.myTestCases.length;
    this.myPassedCount = this.myTestCases.filter(
      (tc) => tc.testCaseStatus === 'Passed'
    ).length;
    this.myFailedCount = this.myTestCases.filter(
      (tc) => tc.testCaseStatus === 'Failed'
    ).length;
    this.myRunningSkippedCount = this.myTestCases.filter(
      (tc) =>
        tc.testCaseStatus === 'InProgress' ||
        tc.testCaseStatus === 'Scheduled' ||
        tc.testCaseStatus === 'Queued' ||
        tc.testCaseStatus === 'Skipped' ||
        tc.testCaseStatus === 'Cancelled'
    ).length;
  }

  // --- Export ---
  // Exports whichever table is actually on screen - myTestCases for the personalized
  // Tester view, the full testCases otherwise - so what gets exported always matches
  // what's visible, not a different/hidden dataset.
  private get exportRows(): IAssignedTestCase[] {
    return this.isPersonalizedView ? this.myTestCases : this.testCases;
  }

  private get exportFileNamePrefix(): string {
    const releaseName = this.selectedRelease?.releaseName ?? 'release';
    const safeName = releaseName.replace(/[^a-z0-9]+/gi, '_');
    return this.isPersonalizedView ? `${safeName}_my_results` : `${safeName}_results`;
  }

  private buildCsvValue(value: string | number | undefined | null): string {
    const text = (value ?? '').toString();
    // Quote any field containing a comma, quote, or newline - and escape embedded
    // quotes by doubling them - the standard CSV escaping rules (RFC 4180), needed
    // since test descriptions/error messages can freely contain any of these.
    if (/[",\n]/.test(text)) {
      return `"${text.replace(/"/g, '""')}"`;
    }
    return text;
  }

  onExportCsv(): void {
    if (!this.selectedRelease) return;

    const rows = this.exportRows;
    if (rows.length === 0) {
      this.toaster.info('There is nothing to export for the current view.');
      return;
    }

    const headers = [
      'Test Case ID',
      'Test Case Name',
      'Description',
      'Environment',
      'Priority',
      'Status',
      'Duration (s)',
      'Assigned To',
    ];

    const lines = [headers.map((h) => this.buildCsvValue(h)).join(',')];
    for (const tc of rows) {
      lines.push(
        [
          tc.testCaseId,
          tc.methodName,
          tc.testCaseDescription,
          tc.environment,
          tc.priority,
          tc.testCaseStatus,
          tc.duration != null ? tc.duration.toFixed(2) : '',
          tc.assignedUserName,
        ]
          .map((v) => this.buildCsvValue(v))
          .join(',')
      );
    }

    // Prepending a UTF-8 BOM so Excel (the most common consumer of a "CSV export"
    // button) doesn't mis-render special characters in test names/descriptions.
    const blob = new Blob(['\uFEFF' + lines.join('\r\n')], {
      type: 'text/csv;charset=utf-8;',
    });
    this.downloadBlob(blob, `${this.exportFileNamePrefix}.csv`);
    this.toaster.success('CSV exported.');
  }

  onExportPdf(): void {
    if (!this.selectedRelease) return;

    const rows = this.exportRows;
    if (rows.length === 0) {
      this.toaster.info('There is nothing to export for the current view.');
      return;
    }

    const doc = new jsPDF({ orientation: 'landscape' });
    const release = this.selectedRelease;

    doc.setFontSize(14);
    doc.text(
      `${release.releaseName} v${release.version} - ${this.isPersonalizedView ? 'My Results' : 'Test Case Results'}`,
      14,
      15
    );

    doc.setFontSize(10);
    const total = this.isPersonalizedView ? this.myTotalCases : this.totalCases;
    const passed = this.isPersonalizedView ? this.myPassedCount : this.passedCount;
    const failed = this.isPersonalizedView ? this.myFailedCount : this.failedCount;
    const runningSkipped = this.isPersonalizedView
      ? this.myRunningSkippedCount
      : this.runningCount + this.skippedCount;
    doc.text(
      `Total: ${total}   Passed: ${passed}   Failed: ${failed}   Running/Skipped: ${runningSkipped}   Exported: ${new Date().toLocaleString()}`,
      14,
      22
    );

    autoTable(doc, {
      startY: 28,
      head: [
        [
          'Test Case ID',
          'Test Case Name',
          'Description',
          'Environment',
          'Priority',
          'Status',
          'Duration (s)',
          'Assigned To',
        ],
      ],
      body: rows.map((tc) => [
        tc.testCaseId,
        tc.methodName,
        tc.testCaseDescription,
        tc.environment,
        tc.priority,
        tc.testCaseStatus,
        tc.duration != null ? tc.duration.toFixed(2) : '',
        tc.assignedUserName,
      ]),
      styles: { fontSize: 8 },
      headStyles: { fillColor: [108, 52, 131] }, // matches the app's purple branding
    });

    doc.save(`${this.exportFileNamePrefix}.pdf`);
    this.toaster.success('PDF exported.');
  }

  private downloadBlob(blob: Blob, fileName: string): void {
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    link.click();
    URL.revokeObjectURL(url);
  }

  onRefreshClick() {
    if (!this.selectedRelease) return;
    this.refreshReleaseData();
  }

  private updateLastUpdatedLabel(): void {
    if (!this.lastUpdatedAt) {
      this.lastUpdatedLabel = '—';
      return;
    }

    const seconds = Math.floor(
      (Date.now() - this.lastUpdatedAt.getTime()) / 1000
    );

    if (seconds < 10) {
      this.lastUpdatedLabel = 'just now';
    } else if (seconds < 60) {
      this.lastUpdatedLabel = `${seconds}s ago`;
    } else if (seconds < 3600) {
      this.lastUpdatedLabel = `${Math.floor(seconds / 60)}m ago`;
    } else {
      this.lastUpdatedLabel = `${Math.floor(seconds / 3600)}h ago`;
    }
  }

  // Only polls while there's actually something that could change - a Completed or
  // Not Started release has nothing new to fetch, so auto-refreshing then would just
  // be wasted requests. Safe to call repeatedly: always clears any existing timer
  // first, so switching releases (or the same release finishing mid-poll) can't leave
  // two overlapping intervals running.
  private startPollingIfRunning(): void {
    this.stopPolling();

    if (this.runStatusLabel === 'In Progress') {
      this.pollTimer = setInterval(
        () => this.refreshReleaseData(),
        this.POLL_INTERVAL_MS
      );
    }
  }

  private stopPolling(): void {
    if (this.pollTimer) {
      clearInterval(this.pollTimer);
      this.pollTimer = undefined;
    }
  }

  resetSummaryCounts() {
    this.totalCases = 0;
    this.assignedCount = 0;
    this.unassignedCount = 0;
    this.passedCount = 0;
    this.failedCount = 0;
    this.runningCount = 0;
    this.skippedCount = 0;
    this.myTestCases = [];
    this.myTotalCases = 0;
    this.myPassedCount = 0;
    this.myFailedCount = 0;
    this.myRunningSkippedCount = 0;
  }

  resetRunTimeline() {
    this.overallStartTime = null;
    this.overallEndTime = null;
    this.runStatusLabel = 'Not Started';
    this.executionDurationLabel = '—';
    this.testersInvolvedCount = 0;
    this.averageTestDurationLabel = '—';

    // Runs on every release switch/clear (see onReleaseChange) - stop any polling for
    // the previous release and reset freshness, refreshReleaseData() will restart both
    // correctly if the new release warrants it.
    this.stopPolling();
    this.lastUpdatedAt = null;
    this.lastUpdatedLabel = '—';
  }

  // Derives an overall "release execution window" (first test started -> last test
  // finished) from the already-loaded, Release-scoped test cases - no fabricated data,
  // just honest Not Started / In Progress / Completed states.
  computeRunTimeline(testCases: IAssignedTestCase[]) {
    const started = testCases.filter((tc) => !!tc.startTime);

    if (started.length === 0) {
      this.resetRunTimeline();
      return;
    }

    this.overallStartTime = new Date(
      Math.min(...started.map((tc) => new Date(tc.startTime!).getTime()))
    );

    const testerNames = new Set(
      started.map((tc) => tc.assignedUserName).filter((name) => !!name)
    );
    this.testersInvolvedCount = testerNames.size;

    const durations = testCases
      .map((tc) => tc.duration)
      .filter((d): d is number => d != null);
    this.averageTestDurationLabel = durations.length
      ? this.formatDuration(
          (durations.reduce((sum, d) => sum + d, 0) / durations.length) * 1000
        )
      : '—';

    const finished = started.filter((tc) => !!tc.endTime);

    if (finished.length < started.length) {
      this.overallEndTime = null;
      this.runStatusLabel = 'In Progress';
      this.executionDurationLabel = 'Running…';
      return;
    }

    this.overallEndTime = new Date(
      Math.max(...finished.map((tc) => new Date(tc.endTime!).getTime()))
    );
    this.runStatusLabel = 'Completed';
    this.executionDurationLabel = this.formatDuration(
      this.overallEndTime.getTime() - this.overallStartTime.getTime()
    );
  }

  formatDuration(ms: number): string {
    const safeMs = Math.max(0, ms);

    // Sub-second durations (common for quick/API-driven test executions) would
    // otherwise round down to "0m 0s", making a real, meaningful duration look like
    // nothing happened - show decimal-second precision instead.
    if (safeMs < 1000) {
      return `${(safeMs / 1000).toFixed(2)}s`;
    }

    const totalSeconds = Math.round(safeMs / 1000);
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;
    return minutes > 0 ? `${minutes}m ${seconds}s` : `${seconds}s`;
  }

  // Flattens every library discovered in the Release's own folder into the same flat
  // shape the backend's GetAllTestCasesByLibrary would return for a single library.
  private flattenLibraries(libraries: LibraryInfo[]): ITestCaseModel[] {
    const result: ITestCaseModel[] = [];
    for (const lib of libraries || []) {
      for (const cls of lib.classes || []) {
        for (const method of cls.methods || []) {
          result.push({
            libraryName: lib.libraryName,
            className: cls.className,
            methodName: method.methodName,
            description: method.description ?? '',
            priority: method.priority ?? '',
            testCaseId: method.testCaseId ?? '',
            assignedUsers: [],
            assignedUserName: '',
          });
        }
      }
    }
    return result;
  }

  mergeReleaseTestCases(releaseId: number) {
    this.resetSummaryCounts();

    const allCases$ = this.testSuitesService
      .getLibraries(releaseId)
      .pipe(map((libraries) => this.flattenLibraries(libraries)));

    const assignedCases$ =
      this.testCaseAssignmentService.getAllAssignedTestCasesForRelease(
        releaseId
      );

    return forkJoin([allCases$, assignedCases$]).pipe(
      map(([allCases, assigned]) => {
        this.totalCases = allCases.length;
        this.assignedCount = assigned.length;
        this.unassignedCount = this.totalCases - this.assignedCount;

        const mapAssigned = new Map(assigned.map((a) => [a.testCaseId, a]));

        const merged: IAssignedTestCase[] = allCases.map((tc) => {
          const assignedRow = mapAssigned.get(tc.testCaseId);
          if (assignedRow) return assignedRow;

          return {
            assignmentTestCaseId: 0,
            assignmentId: 0,
            testCaseId: tc.testCaseId,
            testCaseDescription: tc.description,
            testCaseStatus: 'Unassigned',
            className: tc.className,
            libraryName: tc.libraryName,
            methodName: tc.methodName,
            priority: tc.priority,
            startTime: undefined,
            endTime: undefined,
            duration: undefined,
            errorMessage: '',
            assignedUserId: 0,
            assignedUserName: '',
            environment: '',
            hasScreenshots: false,
            hasLogs: false,
          } as IAssignedTestCase;
        });

        return merged;
      })
    );
  }

  // Same convention as release-management.component.ts's statusPillClass /
  // test-case-execution-panel.component.ts's releaseLifecycleBadgeClass, so the
  // lifecycle badge here matches the color used everywhere else in the app.
  releaseLifecycleBadgeClass(lifecycle?: string): string {
    switch ((lifecycle || '').toLowerCase()) {
      case 'active':
        return pairBadgeTextColor('bg-success');
      case 'completed':
        return pairBadgeTextColor('bg-primary');
      case 'rejected':
        return pairBadgeTextColor('bg-danger');
      case 'draft':
        return pairBadgeTextColor('bg-secondary');
      default:
        return pairBadgeTextColor('bg-info');
    }
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

      case 'Unassigned':
        return 'bg-secondary text-white';

      default:
        return 'bg-light text-dark border';
    }
  }
  ngOnDestroy(): void {
    if (this.tickTimer) clearInterval(this.tickTimer);
    this.stopPolling();
  }

  loadReleaseExecutionLogs(releaseId: number): void {
    if (!releaseId) return;

    this.executionLogsService.getReleaseLogs(releaseId).subscribe({
      next: (logs) => {
        this.executionLogs = logs;

        // High-level summary (latest 5 logs)
        this.recentLogs = [...logs]
          .sort(
            (a, b) =>
              new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
          )
          .slice(0, 5);
      },
      error: () => {
        this.executionLogs = [];
        this.recentLogs = [];
      },
    });
  }

  openFullLogsModal() {
    this.executionLogsDialog.open(this.executionLogs);
  }

  onViewLogs(row: any) {
    this.executionLogsService
      .getTestCaseLogs(row.assignmentId, row.assignmentTestCaseId)
      .subscribe((logs) => {
        this.executionLogsDialog.open(logs);
      });
  }
}

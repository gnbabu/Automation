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

  executionLogs: ITestCaseExecutionLog[] = [];
  recentLogs: ITestCaseExecutionLog[] = [];
  showFullLogs = false;

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
  }

  loadReleases() {
    this.releaseService.getAll().subscribe({
      next: (res) => {
        this.releases = (res || []).filter((r) =>
          ['Active', 'Completed'].includes(r.releaseLifecycle)
        );
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
    });

    this.loadReleaseExecutionLogs(releaseId);
  }

  onRefreshClick() {
    if (!this.selectedRelease) return;
    this.refreshReleaseData();
  }

  resetSummaryCounts() {
    this.totalCases = 0;
    this.assignedCount = 0;
    this.unassignedCount = 0;
    this.passedCount = 0;
    this.failedCount = 0;
    this.runningCount = 0;
    this.skippedCount = 0;
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
  ngOnDestroy(): void {}

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

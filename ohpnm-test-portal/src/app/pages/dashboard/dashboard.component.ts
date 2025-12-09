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
  ITestScreenshot,
  LibraryInfo,
} from '@interfaces';
import {
  AuthService,
  CommonToasterService,
  ScreenshotService,
  TestCaseAssignmentService,
  TestSuitesService,
  UsersService,
} from '@services';
import { DataGridComponent } from 'app/core/components/data-grid/data-grid.component';
import { AppDropdownComponent } from 'app/core/components/app-dropdown/app-dropdown.component';
import { forkJoin, map } from 'rxjs';
import { TestScreenshotGalleryComponent } from '../test-case-execution-panel/test-screenshot-gallery/test-screenshot-gallery.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    AppDropdownComponent,
    DataGridComponent,
    TestScreenshotGalleryComponent,
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

  libraries: LibraryInfo[] = [];
  selectedLibrary: LibraryInfo | null = null;

  screenshots: ITestScreenshot[] = [];

  totalCases = 0;
  assignedCount = 0;
  unassignedCount = 0;

  passedCount = 0;
  failedCount = 0;
  runningCount = 0;
  skippedCount = 0;

  // You can add any logic for the dashboard component here if needed
  constructor(
    private testSuitesService: TestSuitesService,
    private authService: AuthService,
    private toaster: CommonToasterService,
    private userService: UsersService,
    private testCaseAssignmentService: TestCaseAssignmentService,
    private screenshotService: ScreenshotService
  ) {}

  ngOnInit(): void {
    this.loadTestSuites();
    this.setupColumns();
  }

  loadTestSuites() {
    this.testSuitesService.getLibraries().subscribe({
      next: (response) => {
        this.libraries = response || [];

        if (this.libraries.length > 0) {
          this.selectedLibrary = this.libraries[0];
          this.onLibraryChange(this.selectedLibrary); // Auto-load data
        }
      },
      error: () => (this.libraries = []),
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

  onLibraryChange(library: LibraryInfo | null) {
    this.selectedLibrary = library;

    if (!library) return;

    // Load merged test cases (assigned + unassigned)
    this.mergeLibraryTestCases(library.libraryName).subscribe((merged) => {
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
      console.log('Merged Test Cases → ', merged);
      // Bind to UI / table later
    });
  }

  mergeLibraryTestCases(libraryName: string) {
    this.totalCases = 0;
    this.assignedCount = 0;
    this.unassignedCount = 0;

    const allCases$ =
      this.testSuitesService.getAllTestCasesByLibraryName(libraryName);

    const assignedCases$ =
      this.testCaseAssignmentService.getAllAssignedTestCasesInLibrary(
        libraryName
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
          } as IAssignedTestCase;
        });
        debugger;
        return merged;
      })
    );
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

      default:
        return 'bg-light text-dark border';
    }
  }
  ngOnDestroy(): void {}
}

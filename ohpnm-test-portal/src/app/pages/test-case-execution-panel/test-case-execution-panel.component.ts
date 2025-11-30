import { CommonModule } from '@angular/common';
import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  GridColumn,
  IAssignedTestCase,
  ITestCaseAssignmentEntity,
} from '@interfaces';
import {
  AuthService,
  CommonToasterService,
  ConfirmService,
  TestCaseAssignmentService,
  TestCaseExecutionService,
  UsersService,
} from '@services';
import { AppDropdownComponent } from 'app/core/components/app-dropdown/app-dropdown.component';
import { DataGridComponent } from 'app/core/components/data-grid/data-grid.component';
import { ConfirmDialogComponent } from 'app/core/modals/confirm-dialog/confirm-dialog.component';
import { ScheduleTestcasesDialogComponent } from './schedule-testcases-dialog/schedule-testcases-dialog.component';

@Component({
  selector: 'app-test-case-execution',
  imports: [
    AppDropdownComponent,
    CommonModule,
    FormsModule,
    DataGridComponent,
    ScheduleTestcasesDialogComponent,
  ],
  standalone: true,
  templateUrl: './test-case-execution-panel.component.html',
  styleUrl: './test-case-execution-panel.component.css',
})
export class TestCaseExecutionPanelComponent implements OnInit {
  constructor(
    private authService: AuthService,
    private toaster: CommonToasterService,
    private testCaseAssignmentService: TestCaseAssignmentService,
    private confirmService: ConfirmService,
    private testCaseExecutionService: TestCaseExecutionService
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

  assignments: ITestCaseAssignmentEntity[] = [];
  selectedAssignment: ITestCaseAssignmentEntity | null = null;

  columns: GridColumn[] = [];
  testCases: IAssignedTestCase[] = [];
  selectedTestCases: IAssignedTestCase[] = [];

  stats = {
    totalAssigned: 0,
    pendingExecution: 0,
    completed: 0,
  };

  ngOnInit(): void {
    this.loadAssignments();
    this.setupColumns();
  }

  loadAssignments() {
    const userId = this.authService?.getLoggedInUserId(); // or however you store logged-in user ID

    if (!userId) {
      console.error('UserId is required to load assignments');
      return;
    }

    this.testCaseAssignmentService.getAssignmentsByUserId(userId).subscribe({
      next: (res) => {
        this.assignments = res;

        // ✅ Select first assignment by default
        if (this.assignments.length > 0) {
          this.selectedAssignment = this.assignments[0];
          this.onAssignmentChange(this.selectedAssignment); // Trigger selection event
        }
      },
      error: (err) => console.error('Failed to load assignments:', err),
    });
  }

  onAssignmentChange(assignment: ITestCaseAssignmentEntity) {
    this.selectedAssignment = assignment;
    this.loadAssignedTestCases();
    console.log('Selected Assignment:', assignment);
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

      case 'Completed':
        return 'bg-success text-white';

      case 'Failed':
        return 'bg-danger text-white';

      case 'Cancelled':
        return 'bg-dark text-white';

      default:
        return 'bg-light text-dark border';
    }
  }

  calculateStats() {
    this.stats.totalAssigned = this.testCases.length;

    this.stats.completed = this.testCases.filter(
      (x) => x.testCaseStatus === 'Completed'
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
    const confirmed = await this.confirmService.confirm(
      'Run Test Case',
      `Are you sure you want to run Test Case "${testCase.testCaseId}" now?`
    );

    if (!confirmed) return;

    const payload = {
      assignmentId: this.selectedAssignment?.assignmentId!,
      assignmentTestCaseId: testCase.assignmentTestCaseId,
    };

    this.testCaseExecutionService.singleRunNow(payload).subscribe({
      next: (res) => {
        this.toaster.success('Test case added to execution queue.');
        this.loadAssignedTestCases(); // Refresh UI
      },
      error: () => this.toaster.error('Failed to queue test case.'),
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
    this.scheduleDialog.open((data: any) => {
      const scheduleDate = this.combineDateAndTime(data.date, data.time);
      const payload = {
        assignmentId: this.selectedAssignment?.assignmentId!,
        assignmentTestCaseId: testCase.assignmentTestCaseId,
        scheduleDate: this.formatLocalDateTime(scheduleDate),
      };

      this.testCaseExecutionService.singleSchedule(payload).subscribe({
        next: () => {
          this.toaster.success('Test case scheduled successfully.');
          this.loadAssignedTestCases();
        },
        error: () => this.toaster.error('Failed to schedule test case.'),
      });
    });
  }

  async onBulkRunNow() {
    if (!this.selectedTestCases || this.selectedTestCases.length === 0) {
      return;
    }

    const testCaseIds = this.selectedTestCases
      .map((t) => t.testCaseId)
      .join(', ');

    const confirmed = await this.confirmService.confirm(
      'Bulk Run Now',
      `Are you sure you want to run these test cases?\n\n${testCaseIds}`
    );

    if (!confirmed) return;

    const payload = {
      assignmentId: this.selectedAssignment?.assignmentId!,
      assignmentTestCaseIds: this.selectedTestCases.map(
        (t) => t.assignmentTestCaseId
      ),
    };

    this.testCaseExecutionService.bulkRunNow(payload).subscribe({
      next: () => {
        this.toaster.success('Selected test cases queued successfully.');
        this.loadAssignedTestCases();
      },
      error: () => this.toaster.error('Failed to queue test cases.'),
    });
  }

  onBulkSchedule() {
    if (!this.selectedTestCases || this.selectedTestCases.length === 0) return;
    this.scheduleDialog.open((data: any) => {
      const scheduleDate = this.combineDateAndTime(data.date, data.time);

      const payload = {
        assignmentId: this.selectedAssignment?.assignmentId!,
        assignmentTestCaseIds: this.selectedTestCases.map(
          (t) => t.assignmentTestCaseId
        ),
        scheduleDate: this.formatLocalDateTime(scheduleDate),
      };

      this.testCaseExecutionService.bulkSchedule(payload).subscribe({
        next: () => {
          this.toaster.success('Bulk schedule created successfully.');
          this.loadAssignedTestCases();
        },
        error: () => this.toaster.error('Failed to bulk schedule test cases.'),
      });
    });
  }

  isTestCaseSelectable(row: any): boolean {
    const disabledStatuses = [
      'Queued',
      'Scheduled',
      'InProgress',
      'Completed',
      'Failed',
      'Cancelled',
    ];

    return !disabledStatuses.includes(row.testCaseStatus ?? '');
  }
}

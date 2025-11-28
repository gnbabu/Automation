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
  UsersService,
} from '@services';
import { AppDropdownComponent } from 'app/core/components/app-dropdown/app-dropdown.component';
import { DataGridComponent } from 'app/core/components/data-grid/data-grid.component';
import { ConfirmDialogComponent } from 'app/core/modals/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-test-case-execution',
  imports: [AppDropdownComponent, CommonModule, FormsModule, DataGridComponent],
  standalone: true,
  templateUrl: './test-case-execution-panel.component.html',
  styleUrl: './test-case-execution-panel.component.css',
})
export class TestCaseExecutionPanelComponent implements OnInit {
  constructor(
    private authService: AuthService,
    private toaster: CommonToasterService,
    private userService: UsersService,
    private testCaseAssignmentService: TestCaseAssignmentService,
    private confirmService: ConfirmService
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

  assignments: ITestCaseAssignmentEntity[] = [];
  selectedAssignment: ITestCaseAssignmentEntity | null = null;

  environments: any[] = [];
  selectedEnvironment: any = null;

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
    this.loadEnvironments();
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

  loadEnvironments() {
    this.environments = [
      { environmentName: 'DEV' },
      { environmentName: 'QA' },
      { environmentName: 'UAT' },
      { environmentName: 'PROD' },
    ];
    this.selectedEnvironment = this.environments[0];
  }

  onEnvironmentChange(env: any) {
    this.selectedEnvironment = env;
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
      case 'New':
        return 'bg-info';
      case 'Completed':
        return 'bg-success';
      case 'Running':
        return 'bg-warning text-dark';
      case 'Failed':
        return 'bg-danger';
      case 'Pending':
        return 'bg-secondary';
      default:
        return 'bg-light text-dark border';
    }
  }

  calculateStats() {
    this.stats.totalAssigned = this.testCases.length;

    this.stats.completed = this.testCases.filter(
      (x) => x.testCaseStatus === 'Completed'
    ).length;

    this.stats.pendingExecution =
      this.stats.totalAssigned - this.stats.completed;
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

    console.log('Confirmed! Running:', testCase);

    // TODO: API call here
  }

  onSchedule(testCase: IAssignedTestCase) {
    console.log('Schedule clicked:', testCase);
    // TODO: open scheduling modal
  }
}

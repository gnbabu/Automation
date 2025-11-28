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
  TestCaseAssignmentService,
  UsersService,
} from '@services';
import { AppDropdownComponent } from 'app/core/components/app-dropdown/app-dropdown.component';
import { DataGridComponent } from 'app/core/components/data-grid/data-grid.component';

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
    private testCaseAssignmentService: TestCaseAssignmentService
  ) {}

  @ViewChild('testCaseIdTemplate', { static: true })
  testCaseIdTemplate!: TemplateRef<any>;

  @ViewChild('priorityTemplate', { static: true })
  priorityTemplate!: TemplateRef<any>;

  @ViewChild('statusTemplate', { static: true })
  statusTemplate!: TemplateRef<any>;

  assignments: ITestCaseAssignmentEntity[] = [];
  selectedAssignment: ITestCaseAssignmentEntity | null = null;

  environments: any[] = [];
  selectedEnvironment: any = null;

  columns: GridColumn[] = [];
  testCases: IAssignedTestCase[] = [];
  selectedTestCases: IAssignedTestCase[] = [];

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
        sortable: true,
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
}

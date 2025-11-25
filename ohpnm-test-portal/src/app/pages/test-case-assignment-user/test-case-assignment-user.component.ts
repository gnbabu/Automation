import { CommonModule } from '@angular/common';
import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  GridColumn,
  IAssignedTestCase,
  IAssignmentCreateUpdateRequest,
  ITestCaseModel,
  IUser,
  LibraryInfo,
} from '@interfaces';
import {
  AuthService,
  CommonToasterService,
  TestCaseManagerService,
  TestSuitesService,
  UsersService,
} from '@services';
import { AppDropdownComponent } from 'app/core/components/app-dropdown/app-dropdown.component';
import { DataGridComponent } from 'app/core/components/data-grid/data-grid.component';

@Component({
  selector: 'app-test-case-assignment-user',
  standalone: true,
  imports: [AppDropdownComponent, CommonModule, FormsModule, DataGridComponent],
  templateUrl: './test-case-assignment-user.component.html',
  styleUrl: './test-case-assignment-user.component.css',
})
export class TestCaseAssignmentUserComponent implements OnInit {
  @ViewChild('testCaseIdTemplate', { static: true })
  testCaseIdTemplate!: TemplateRef<any>;

  @ViewChild('priorityTemplate', { static: true })
  priorityTemplate!: TemplateRef<any>;

  @ViewChild('assignedUsersTemplate', { static: true })
  assignedUsersTemplate!: TemplateRef<any>;

  @ViewChild('assignTesterTemplate', { static: true })
  assignTesterTemplate!: TemplateRef<any>;

  libraries: LibraryInfo[] = [];
  assignmentStatuses: any[] = [];
  selectedLibrary: LibraryInfo | null = null;
  selectedAssignmentStatus: any = null;
  users: IUser[] = [];
  selectedUser: IUser | null = null;

  environments: any[] = [];
  selectedEnvironment: any = null;

  testCases: ITestCaseModel[] = [];
  assignedTestCases: IAssignedTestCase[] = [];
  selectedMethods: ITestCaseModel[] = [];

  columns: GridColumn[] = [];
  totalCases = 0;
  assignedCount = 0;
  unassignedCount = 0;

  constructor(
    private testSuitesService: TestSuitesService,
    private authService: AuthService,
    private toaster: CommonToasterService,
    private userService: UsersService,
    private testCaseManagerService: TestCaseManagerService
  ) {}

  ngOnInit(): void {
    this.loadTestSuites();
    this.loadAssignmentStatuses();
    this.loadUsers();
    this.loadEnvironments();
    this.setupColumns();
  }

  setupColumns() {
    this.columns = [
      {
        field: 'testCaseId',
        header: 'Test Case ID',
        sortable: true,
        cellTemplate: this.testCaseIdTemplate,
      },
      {
        field: 'description',
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
        field: 'assignedUserName',
        header: 'Current Status',
        sortable: false,
        cellTemplate: this.assignedUsersTemplate,
      },
    ];
  }
  onLibraryChange(library: LibraryInfo | null) {
    this.selectedLibrary = library;
    const libName = library?.libraryName || undefined;
    this.tryLoadTestCases();
  }

  onUserChange(user: IUser | null) {
    this.selectedUser = user;
    this.tryLoadTestCases();
  }

  onAssignmentStatusChange(status: any) {
    this.selectedAssignmentStatus = status;
  }

  loadTestSuites() {
    this.testSuitesService.getLibraries().subscribe({
      next: (response) => (this.libraries = response || []),
      error: () => (this.libraries = []),
    });
  }

  // Load assignment statuses
  loadAssignmentStatuses() {
    this.assignmentStatuses = [
      { assignmentStatus: 'Assigned' },
      { assignmentStatus: 'Unassigned' },
    ];
  }

  loadUsers() {
    this.userService.getAll().subscribe({
      next: (res) => (this.users = res),
      error: (err) => console.error('Failed to load users:', err),
    });
  }

  loadEnvironments() {
    this.environments = [
      { environmentName: 'DEV' },
      { environmentName: 'QA' },
      { environmentName: 'UAT' },
      { environmentName: 'PROD' },
    ];
  }

  onEnvironmentChange(env: any) {
    debugger;
    this.selectedEnvironment = env;
    this.tryLoadTestCases();
  }

  tryLoadTestCases() {
    if (
      !this.selectedLibrary ||
      !this.selectedUser ||
      !this.selectedEnvironment
    ) {
      this.testCases = [];
      this.selectedMethods = [];
      return;
    }

    const assignmentName =
      `${this.selectedUser.userName}-` +
      `${this.selectedLibrary.libraryName}-` +
      `${this.selectedEnvironment.environmentName}`;

    // STEP 1: Get ALL test cases for selected Library
    this.testSuitesService
      .getAllTestCasesByLibraryName(this.selectedLibrary.libraryName)
      .subscribe({
        next: (libraryCases) => {
          this.testCases = libraryCases.map((tc) => ({
            ...tc,
            selected: false,
          }));

          // STEP 2: Load ALL assigned testcases (for ALL users)
          this.testCaseManagerService
            .getAssignedTestCasesForLibrary(
              this.selectedLibrary?.libraryName ?? '',
              this.selectedEnvironment.environmentName
            )
            .subscribe({
              next: (allAssigned) => {
                // TestCases assigned to ANY user
                const allAssignedIds = new Set(
                  allAssigned.map((a) => a.testCaseId)
                );

                // STEP 3: Load only assignments for CURRENT USER
                this.testCaseManagerService
                  .getTestCasesByAssignmentAndUser(
                    this.selectedUser?.userId ?? 0,
                    assignmentName
                  )
                  .subscribe({
                    next: (myAssigned) => {
                      const myAssignedIds = new Set(
                        myAssigned.map((a) => a.testCaseId)
                      );

                      // STEP 4: Filter test cases available for CURRENT USER:
                      // - Available = Not assigned OR assigned to this user
                      this.testCases = this.testCases.filter(
                        (tc) =>
                          !allAssignedIds.has(tc.testCaseId) || // unassigned
                          myAssignedIds.has(tc.testCaseId) // or assigned to current user
                      );

                      // STEP 5: Mark selected (only for current user)
                      this.testCases.forEach((tc) => {
                        tc.selected = myAssignedIds.has(tc.testCaseId);
                      });

                      // STEP 6: Populate selectedMethods
                      Promise.resolve().then(() => {
                        this.selectedMethods = this.testCases.filter(
                          (tc) => tc.selected
                        );
                      });
                    },
                    error: (err) => console.error(err),
                  });
              },
              error: (err) => console.error(err),
            });
        },
        error: (err) => console.error(err),
      });
  }

  onSaveAssignments() {
    debugger;
    if (
      !this.selectedLibrary ||
      !this.selectedUser ||
      !this.selectedEnvironment
    ) {
      this.toaster.error('Please select User, Library, and Environment.');
      return;
    }

    if (this.selectedMethods.length === 0) {
      this.toaster.error('No test cases selected.');
      return;
    }
    debugger;

    const request: IAssignmentCreateUpdateRequest = {
      assignedUser: this.selectedUser.userId ?? 0,
      assignmentStatus: 'New',
      releaseName: this.selectedLibrary.libraryName,
      environment: this.selectedEnvironment.environmentName,
      assignedBy: this.authService.getLoggedInUserId(),
      testCases: this.selectedMethods.map((tc) => ({
        testCaseId: tc.testCaseId,
        testCaseDescription: tc.description,
        testCaseStatus: 'New',
        className: tc.className,
        libraryName: tc.libraryName,
        methodName: tc.methodName,
        priority: tc.priority,
      })),
    };
    debugger;
    this.testCaseManagerService.saveAssignmentNew(request).subscribe({
      next: () => {
        debugger;
        this.toaster.success('Assignments saved successfully.');
        this.tryLoadTestCases(); // Refresh grid
      },
      error: (err) => {
        console.error(err);
        this.toaster.error('Failed to save assignments.');
      },
    });
  }

  onResetAssignments() {
    if (
      !this.selectedLibrary ||
      !this.selectedUser ||
      !this.selectedEnvironment
    ) {
      this.toaster.error('Please select User, Library, and Environment.');
      return;
    }

    const assignmentName =
      `${this.selectedUser.userName}-` +
      `${this.selectedLibrary.libraryName}-` +
      `${this.selectedEnvironment.environmentName}`;

    const request: IAssignmentCreateUpdateRequest = {
      assignedUser: this.selectedUser.userId ?? 0,
      assignmentStatus: 'Removed',
      releaseName: this.selectedLibrary.libraryName,
      environment: this.selectedEnvironment.environmentName,
      assignedBy: this.authService.getLoggedInUserId(),
      testCases: [], // EMPTY → Reset all
    };

    this.testCaseManagerService.saveAssignmentNew(request).subscribe({
      next: () => {
        this.toaster.success('All assignments reset.');
        this.selectedMethods = [];
        this.tryLoadTestCases(); // reload
      },
      error: (err) => {
        console.error(err);
        this.toaster.error('Failed to reset assignments.');
      },
    });
  }
}

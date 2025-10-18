import { CommonModule } from '@angular/common';
import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  GridColumn,
  ITestCaseAssignmentDeleteRequest,
  ITestCaseModel,
  IUser,
  LibraryInfo,
} from '@interfaces';
import { AppMultiselectDropdownComponent } from 'app/core/components/app-multiselect-dropdown/app-multiselect-dropdown.component';
import {
  AuthService,
  CommonToasterService,
  TestCaseManagerService,
  TestSuitesService,
  UsersService,
} from '@services';
import { AppDropdownComponent } from 'app/core/components/app-dropdown/app-dropdown.component';
import { DataGridComponent } from 'app/core/components/data-grid/data-grid.component';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-test-case-assignment',
  imports: [
    AppDropdownComponent,
    AppMultiselectDropdownComponent,
    CommonModule,
    FormsModule,
    DataGridComponent,
  ],
  templateUrl: './test-case-assignment.component.html',
  styleUrl: './test-case-assignment.component.css',
})
export class TestCaseAssignmentComponent implements OnInit {
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

  testCases: ITestCaseModel[] = [];
  filteredTestCases: ITestCaseModel[] = [];
  users: IUser[] = [];

  columns: GridColumn[] = [];
  totalCases = 0;
  assignedCount = 0;
  unassignedCount = 0;

  previousAssignments = new Map<string, IUser[]>(); // key = testCaseId

  constructor(
    private testSuitesService: TestSuitesService,
    private authService: AuthService,
    private toaster: CommonToasterService,
    private userService: UsersService,
    private testCaseManagerService: TestCaseManagerService
  ) {}

  ngOnInit(): void {
    // Define columns for DataGrid
    this.columns = [
      { field: 'testCaseId', header: 'Test Case ID', sortable: true },
      { field: 'description', header: 'Description', sortable: true },
      {
        field: 'priority',
        header: 'Priority',
        sortable: true,
        cellTemplate: this.priorityTemplate,
      },
      {
        field: 'assignedUsers',
        header: 'Current Status',
        sortable: false,
        cellTemplate: this.assignedUsersTemplate,
      },
      {
        field: 'assignTester',
        header: 'Assign Tester',
        sortable: false,
        cellTemplate: this.assignTesterTemplate,
      },
    ];

    this.loadTestSuites();
    this.loadAssignmentStatuses();
    this.loadUsersAndTestCases();
  }

  // Load libraries
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

  // Load users and test cases together to ensure proper binding
  loadUsersAndTestCases() {
    forkJoin({
      users: this.userService.getAll(),
      testCases: this.testSuitesService.getAllTestCases(),
    }).subscribe({
      next: ({ users, testCases }) => {
        debugger;
        this.users = users || [];

        this.testCases = (testCases || []).map((tc) => ({
          ...tc,
          assignedUsers: (tc.assignedUsers || [])
            .map((u: any) => {
              if (typeof u === 'object') return u; // already IUser
              const id = Number(u); // normalize string → number
              return (
                this.users.find((user) => Number(user.userId) === id) || null
              );
            })
            .filter(Boolean),
        }));

        this.testCases.forEach((tc) => {
          this.previousAssignments.set(tc.testCaseId, [...tc.assignedUsers]);
        });

        this.filteredTestCases = [...this.testCases];
        this.updateStats();
      },
      error: (err) => {
        console.error(err);
        this.users = [];
        this.testCases = [];
        this.filteredTestCases = [];
        this.updateStats();
      },
    });
  }

  // Stats update
  updateStats() {
    this.totalCases = this.filteredTestCases.length;
    this.assignedCount = this.filteredTestCases.filter(
      (t) => t.assignedUsers?.length
    ).length;
    this.unassignedCount = this.totalCases - this.assignedCount;
  }

  // Filters
  onLibraryChange(lib: LibraryInfo | null) {
    this.selectedLibrary = lib;
    this.filterTestCases();
  }

  onAssignmentStatusChange(status: any) {
    this.selectedAssignmentStatus = status;
    this.filterTestCases();
  }

  filterTestCases() {
    this.filteredTestCases = this.testCases.filter((tc) => {
      let matchLibrary = true;
      let matchStatus = true;

      if (this.selectedLibrary)
        matchLibrary = tc.libraryName === this.selectedLibrary.libraryName;

      if (this.selectedAssignmentStatus) {
        matchStatus =
          this.selectedAssignmentStatus.assignmentStatus === 'Assigned'
            ? tc.assignedUsers?.length > 0
            : !tc.assignedUsers?.length;
      }

      return matchLibrary && matchStatus;
    });

    this.updateStats();
  }

  // Get user name for display
  getUserName(user: IUser | number | string): string {
    if (!user) return 'Unknown';
    if (typeof user === 'object') return (user as IUser).userName;
    const found = this.users.find((u) => u.userId === Number(user));
    return found ? found.userName : 'Unknown';
  }

  // Handle selection change in multiselect dropdown
  onUsersChange(selectedUsers: IUser[], row: ITestCaseModel) {
    debugger;
    const prevUsers = this.previousAssignments.get(row.testCaseId) || [];

    // Users that were removed
    const removedUsers = prevUsers.filter(
      (u) => !selectedUsers.some((s) => s.userId === u.userId)
    );

    // Users that were newly added
    const addedUsers = selectedUsers.filter(
      (u) => !prevUsers.some((s) => s.userId === u.userId)
    );

    row.assignedUsers = [...selectedUsers]; // update locally

    // Update previousAssignments for next change
    this.previousAssignments.set(row.testCaseId, [...selectedUsers]);

    if (removedUsers.length > 0) {
      const payload: ITestCaseAssignmentDeleteRequest = {
        userIds: removedUsers.map((u) => u.userId!).filter((id) => id != null),
        libraryName: row.libraryName,
        className: row.className,
        methodName: row.methodName,
      };

      // 3️⃣ Call API to delete assignment
      this.testCaseManagerService
        .deleteAssignmentsByCriteria(payload)
        .subscribe({
          next: () => {
            this.toaster.success(
              `User ${removedUsers[0].userName} unassigned from test case successfully.`
            );
          },
          error: (err) => {
            console.error('Error unassigning user:', err);
          },
        });
    } else {
      // Prepare assignment payload
      const assignments = row.assignedUsers.map((user) => ({
        userId: user.userId,
        libraryName: row.libraryName!,
        className: row.className!,
        methodName: row.methodName!,
        assignedBy: this.authService.getLoggedInUser()?.userId ?? 0,
      }));

      // Save to backend
      this.testCaseManagerService.saveAssignments(assignments).subscribe({
        next: () =>
          this.toaster.success(`Assignments updated for ${row.testCaseId}`),
        error: () => this.toaster.error(`Failed to update ${row.testCaseId}`),
      });
    }
    this.updateStats();
  }

  // Unassign user from a test case
  unassignUser(testCase: ITestCaseModel, user: IUser) {
    debugger;
    testCase.assignedUsers = testCase.assignedUsers.filter(
      (u: IUser) => u.userId !== user.userId
    );
    // 2️⃣ Prepare payload for API
    const payload: ITestCaseAssignmentDeleteRequest = {
      userIds: user.userId != null ? [user.userId] : [], // avoid undefined
      libraryName: testCase.libraryName,
      className: testCase.className,
      methodName: testCase.methodName,
    };

    // 3️⃣ Call API to delete assignment
    this.testCaseManagerService.deleteAssignmentsByCriteria(payload).subscribe({
      next: () => {
        this.toaster.success(
          `User ${user.userName} unassigned from test case successfully.`
        );
      },
      error: (err) => {
        console.error('Error unassigning user:', err);
      },
    });

    this.updateStats();
  }

  onSelectAll(selectedUsers: IUser[], row: ITestCaseModel) {
    // Update row locally
    row.assignedUsers = [...selectedUsers];
    // Update previousAssignments
    this.previousAssignments.set(row.testCaseId, [...selectedUsers]);

    // Prepare payload to save all selected users
    const newAssignments = selectedUsers.map((user) => ({
      userId: user.userId!,
      libraryName: row.libraryName!,
      className: row.className!,
      methodName: row.methodName!,
      assignedBy: this.authService.getLoggedInUser()?.userId ?? 0,
    }));

    this.testCaseManagerService.saveAssignments(newAssignments).subscribe({
      next: () =>
        this.toaster.success(
          `All users assigned to test case ${row.testCaseId}`
        ),
      error: () =>
        this.toaster.error(
          `Failed to assign users to test case ${row.testCaseId}`
        ),
    });
  }

  onClearAll(row: ITestCaseModel) {
    // Get previous assigned users
    const prevUsers = this.previousAssignments.get(row.testCaseId) || [];

    if (prevUsers.length === 0) return; // nothing to clear

    // Prepare payload to delete all users
    const deletePayload: ITestCaseAssignmentDeleteRequest = {
      userIds: prevUsers.map((u) => u.userId!).filter((id) => id != null),
      libraryName: row.libraryName,
      className: row.className,
      methodName: row.methodName,
    };

    // Call API
    this.testCaseManagerService
      .deleteAssignmentsByCriteria(deletePayload)
      .subscribe({
        next: () => {
          // Clear local assignedUsers
          row.assignedUsers = [];
          // Update previousAssignments
          this.previousAssignments.set(row.testCaseId, []);
          this.toaster.info(
            `All users removed from test case ${row.testCaseId}`
          );
        },
        error: () =>
          this.toaster.error(
            `Failed to remove users from test case ${row.testCaseId}`
          ),
      });
  }
}

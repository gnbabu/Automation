import { CommonModule } from '@angular/common';
import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { GridColumn, ITestCaseModel, IUser, LibraryInfo } from '@interfaces';
import { AppMultiselectDropdownComponent } from 'app/core/components/app-multiselect-dropdown/app-multiselect-dropdown.component';
import {
  AuthService,
  CommonToasterService,
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

  constructor(
    private testSuitesService: TestSuitesService,
    private authService: AuthService,
    private toaster: CommonToasterService,
    private userService: UsersService
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
    row.assignedUsers = selectedUsers;
    console.log(
      'Updated assigned users for row:',
      row.testCaseId,
      row.assignedUsers
    );
  }

  // Unassign user from a test case
  unassignUser(testCase: ITestCaseModel, user: IUser) {
    testCase.assignedUsers = testCase.assignedUsers.filter(
      (u: IUser) => u.userId !== user.userId
    );
  }

  onSelectAll(selected: IUser[]) {
    console.log('Select All clicked:', selected);
  }

  onClearAll() {
    console.log('Clear clicked');
  }
}

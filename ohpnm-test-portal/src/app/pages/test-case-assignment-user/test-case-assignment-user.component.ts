import { CommonModule } from '@angular/common';
import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  GridColumn,
  IAssignedTestCase,
  IAssignmentCreateUpdateRequest,
  IReleaseModel,
  ITestCaseModel,
  IUser,
  LibraryInfo,
} from '@interfaces';
import {
  AuthService,
  CommonToasterService,
  ReleaseService,
  TestCaseAssignmentService,
  TestSuitesService,
  UsersService,
} from '@services';
import { AppDropdownComponent } from 'app/core/components/app-dropdown/app-dropdown.component';
import { DataGridComponent } from 'app/core/components/data-grid/data-grid.component';
import { forkJoin } from 'rxjs';

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

  releases: IReleaseModel[] = [];
  selectedRelease: IReleaseModel | null = null;

  libraries: LibraryInfo[] = [];
  assignmentStatuses: any[] = [];
  selectedLibrary: LibraryInfo | null = null;
  selectedAssignmentStatus: any = null;
  users: IUser[] = [];
  selectedUser: IUser | null = null;

  testCases: ITestCaseModel[] = [];
  assignedTestCases: IAssignedTestCase[] = [];
  selectedMethods: ITestCaseModel[] = [];

  columns: GridColumn[] = [];
  totalCases = 0;
  assignedCount = 0;
  unassignedCount = 0;
  showGrid = false;

  constructor(
    private testSuitesService: TestSuitesService,
    private authService: AuthService,
    private toaster: CommonToasterService,
    private userService: UsersService,
    private testCaseAssignmentService: TestCaseAssignmentService,
    private releaseService: ReleaseService
  ) {}

  ngOnInit(): void {
    this.loadReleases();
    this.loadAssignmentStatuses();
    this.loadUsers();
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
  onReleaseChange(release: IReleaseModel | null) {
    this.selectedRelease = release;

    this.selectedLibrary = null;
    this.selectedUser = null;
    this.selectedAssignmentStatus = null;

    this.libraries = [];
    this.testCases = [];
    this.selectedMethods = [];

    this.showGrid = false;

    this.totalCases = 0;
    this.assignedCount = 0;
    this.unassignedCount = 0;

    // Do NOT call API if no release is selected
    if (!release || !release.releaseId) {
      return;
    }
    this.loadTestSuites(release.releaseId);
  }

  onLibraryChange(library: LibraryInfo | null) {
    this.selectedLibrary = library;

    this.selectedUser = null;
    this.selectedAssignmentStatus = null;

    this.testCases = [];
    this.selectedMethods = [];

    this.showGrid = false;

    this.totalCases = 0;
    this.assignedCount = 0;
    this.unassignedCount = 0;

    // Do NOT call API if no library is selected
    if (!library || !library.libraryName || !this.selectedRelease) {
      return;
    }
    this.loadLibraryTestCaseCounts(this.selectedLibrary?.libraryName ?? '');
  }

  onUserChange(user: IUser | null) {
    this.selectedUser = user;

    this.selectedAssignmentStatus = null;

    this.testCases = [];
    this.selectedMethods = [];

    this.tryLoadTestCases();
  }

  onAssignmentStatusChange(status: any) {
    this.selectedAssignmentStatus = status;
  }

  loadReleases() {
    this.releaseService.getAll().subscribe({
      next: (response) => {
        this.releases = (response || []).filter((r) =>
          ['Active', 'Completed'].includes(r.releaseLifecycle)
        );
      },
      error: () => (this.releases = []),
    });
  }

  loadTestSuites(releaseId: number) {
    this.testSuitesService.getLibraries(releaseId).subscribe({
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

  tryLoadTestCases() {
    if (
      !this.selectedRelease ||
      !this.selectedLibrary ||
      !this.selectedUser
    ) {
      this.testCases = [];
      this.selectedMethods = [];
      this.showGrid = false;
      return;
    }

    const assignmentName =
      `${this.selectedUser.userName}-` +
      `${this.selectedLibrary.libraryName}-` +
      `${this.selectedRelease.environmentName}-` +
      `${this.selectedRelease.releaseName}`;

    // STEP 1: Get ALL test cases for selected Library (scoped to the Release's own folder)
    this.testSuitesService
      .getAllTestCasesByLibraryName(
        this.selectedRelease.releaseId,
        this.selectedLibrary.libraryName
      )
      .subscribe({
        next: (libraryCases) => {
          this.testCases = libraryCases.map((tc) => ({
            ...tc,
            selected: false,
          }));

          // STEP 2: Load ALL assigned testcases (for ALL users) scoped to this Release
          this.testCaseAssignmentService
            .getAssignedTestCasesForLibraryAndRelease(
              this.selectedLibrary?.libraryName ?? '',
              this.selectedRelease?.releaseId ?? 0
            )
            .subscribe({
              next: (allAssigned) => {
                // TestCases assigned to ANY user
                const allAssignedIds = new Set(
                  allAssigned.map((a) => a.testCaseId)
                );

                // STEP 3: Load only assignments for CURRENT USER
                this.testCaseAssignmentService
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
                      this.testCases = this.testCases.filter(
                        (tc) =>
                          !allAssignedIds.has(tc.testCaseId) ||
                          myAssignedIds.has(tc.testCaseId)
                      );

                      // STEP 5: Assign correct assignedUserName for ALL test cases
                      this.testCases.forEach((tc) => {
                        const assignedEntry = allAssigned.find(
                          (a) => a.testCaseId === tc.testCaseId
                        );

                        if (assignedEntry) {
                          // 🟦 Test case is assigned → show the correct user name
                          tc.assignedUserName =
                            assignedEntry.assignedUserName || '';
                        } else {
                          // 🟪 Not assigned → show Unassigned
                          tc.assignedUserName = '';
                        }

                        // Mark selected only if assigned to THIS user
                        tc.selected = myAssignedIds.has(tc.testCaseId);
                      });

                      // STEP 6: Populate selectedMethods
                      Promise.resolve().then(() => {
                        this.selectedMethods = this.testCases.filter(
                          (tc) => tc.selected
                        );
                      });
                      this.showGrid = true;
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
    if (!this.selectedRelease || !this.selectedLibrary || !this.selectedUser) {
      this.toaster.error('Please select Release, Library, and User.');
      return;
    }

    if (this.selectedMethods.length === 0) {
      this.toaster.error('No test cases selected.');
      return;
    }

    const request: IAssignmentCreateUpdateRequest = {
      assignedUser: this.selectedUser.userId ?? 0,
      assignmentStatus: 'New',
      releaseName: this.selectedLibrary.libraryName,
      environment: this.selectedRelease.environmentName,
      releaseId: this.selectedRelease.releaseId,
      assignedBy: this.authService.getLoggedInUserId(),
      testCases: this.selectedMethods.map((tc) => ({
        testCaseId: tc.testCaseId,
        testCaseDescription: tc.description,
        testCaseStatus: 'Assigned',
        className: tc.className,
        libraryName: tc.libraryName,
        methodName: tc.methodName,
        priority: tc.priority,
      })),
    };

    this.testCaseAssignmentService.saveAssignment(request).subscribe({
      next: () => {
        this.toaster.success('Assignments saved successfully.');
        this.tryLoadTestCases(); // Refresh grid
        this.loadLibraryTestCaseCounts(this.selectedLibrary?.libraryName ?? '');
      },
      error: (err) => {
        console.error(err);
        this.toaster.error('Failed to save assignments.');
      },
    });
  }

  onResetAssignments() {
    if (!this.selectedRelease || !this.selectedLibrary || !this.selectedUser) {
      this.toaster.error('Please select Release, Library, and User.');
      return;
    }

    const request: IAssignmentCreateUpdateRequest = {
      assignedUser: this.selectedUser.userId ?? 0,
      assignmentStatus: 'Removed',
      releaseName: this.selectedLibrary.libraryName,
      environment: this.selectedRelease.environmentName,
      releaseId: this.selectedRelease.releaseId,
      assignedBy: this.authService.getLoggedInUserId(),
      testCases: [], // EMPTY → Reset all
    };

    this.testCaseAssignmentService.saveAssignment(request).subscribe({
      next: () => {
        this.toaster.success('All assignments reset.');
        this.selectedMethods = [];
        this.tryLoadTestCases(); // reload
        this.loadLibraryTestCaseCounts(this.selectedLibrary?.libraryName ?? '');
      },
      error: (err) => {
        console.error(err);
        this.toaster.error('Failed to reset assignments.');
      },
    });
  }

  onSelectionChanged(selectedRows: ITestCaseModel[]) {
    if (!this.selectedUser) return;

    const currentUserName = this.selectedUser.userName;

    // 1️⃣ Assign username to selected rows
    selectedRows.forEach((r) => {
      r.assignedUserName = currentUserName;
    });

    // 2️⃣ Clear user from unselected rows
    this.testCases.forEach((r) => {
      if (!selectedRows.some((s) => s.methodName === r.methodName)) {
        r.assignedUserName = '';
      }
    });
  }

  loadLibraryTestCaseCounts(libraryName: string) {
    this.totalCases = 0;
    this.assignedCount = 0;
    this.unassignedCount = 0;

    if (!this.selectedRelease) {
      return;
    }

    const allCases$ = this.testSuitesService.getAllTestCasesByLibraryName(
      this.selectedRelease.releaseId,
      libraryName
    );
    const assignedCases$ =
      this.testCaseAssignmentService.getAllAssignedTestCasesInLibrary(
        libraryName
      );

    forkJoin([allCases$, assignedCases$]).subscribe({
      next: ([allCases, assigned]) => {
        this.totalCases = allCases.length;
        this.assignedCount = assigned.length;
        this.unassignedCount = this.totalCases - this.assignedCount;
      },
      error: (err) => console.error('Failed to load counts', err),
    });
  }
}

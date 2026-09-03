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
import { pairBadgeTextColor } from 'app/core/utils/badge-class.util';
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

  // tryLoadTestCases() chains 3 sequential HTTP calls. Switching the User (or Library/
  // Release) dropdown starts a brand-new chain without cancelling whatever chain is still
  // in flight from the *previous* selection - if that older chain's later steps resolve
  // after the newer one's, its (now-stale, wrong-tester) data would silently overwrite the
  // rows that already reflect the current selection. Each call captures the current
  // requestId and checks it's still current before applying results, so a late-arriving
  // stale response is discarded instead of corrupting the current view.
  private loadRequestId = 0;

  // Snapshot of the current tester's assignment set as of the last successful load
  // (tryLoadTestCases()'s Step 3 "myAssigned"), kept around purely so onSaveAssignments()
  // can diff the current selection against it to report real "N added, M removed" counts
  // instead of a generic "saved successfully" message.
  private originallyAssignedIds = new Set<string>();

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

  // Same convention as test-case-execution-panel.component.ts's releaseLifecycleBadgeClass/
  // release-management.component.ts's statusPillClass, so this badge matches the color
  // used everywhere else in the app.
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

  // Anything past 'Assigned' means it's entered the execution pipeline at all (not just
  // finished) - Queued/Scheduled/InProgress/Passed/Failed/Cancelled all lock it. Mirrors
  // the DB-side enforcement in usp_CreateOrUpdateAssignmentWithTestCases (the actual
  // source of truth); this is purely so the UI shows *why* a row can't be touched instead
  // of letting the user interact with something the server will silently ignore.
  private static readonly LOCKED_STATUSES = new Set([
    'Queued',
    'Scheduled',
    'InProgress',
    'Passed',
    'Failed',
    'Cancelled',
    // NUnit's own outcomes ([Ignore]/[Explicit] -> Skipped; unresolved assertion ->
    // Inconclusive), now that the runner reports these honestly - see AGENTS.md.
    'Skipped',
    'Inconclusive',
  ]);

  private isLocked(status?: string): boolean {
    return !!status && TestCaseAssignmentUserComponent.LOCKED_STATUSES.has(status);
  }

  // Arrow function (not a regular method) so `this` stays bound to the component
  // instance even when DataGridComponent invokes it directly as a plain callback (via
  // [rowSelectableFn]) without preserving the calling context - same convention as
  // test-case-execution-panel.component.ts's isTestCaseSelectable. Safe to key off
  // row.testCaseStatus directly (no wrong-tester ambiguity) since Step 4's filter means a
  // visible row is only ever unassigned or assigned to the currently selected tester.
  isTestCaseSelectable = (row: ITestCaseModel): boolean => {
    return !this.isLocked(row.testCaseStatus);
  };

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
      this.loadRequestId++; // invalidate any still-in-flight chain from a prior selection
      this.testCases = [];
      this.selectedMethods = [];
      this.showGrid = false;
      return;
    }

    // Bump the generation counter and capture it locally. Every step below checks this
    // before applying its results/writing to this.testCases - if the User/Library/Release
    // selection changes again while this chain is still in flight, loadRequestId moves on
    // and this chain's (now-stale) results are discarded instead of being applied on top
    // of whatever the newer selection already loaded.
    const requestId = ++this.loadRequestId;
    const requestedUser = this.selectedUser;
    const requestedRelease = this.selectedRelease;
    const requestedLibrary = this.selectedLibrary;

    const assignmentName =
      `${requestedUser.userName}-` +
      `${requestedLibrary.libraryName}-` +
      `${requestedRelease.environmentName}-` +
      `${requestedRelease.releaseName}`;

    // STEP 1: Get ALL test cases for selected Library (scoped to the Release's own folder)
    this.testSuitesService
      .getAllTestCasesByLibraryName(
        requestedRelease.releaseId,
        requestedLibrary.libraryName
      )
      .subscribe({
        next: (libraryCases) => {
          if (requestId !== this.loadRequestId) return; // stale - a newer selection is loading

          const loadedTestCases: ITestCaseModel[] = libraryCases.map((tc) => ({
            ...tc,
            selected: false,
          }));

          // STEP 2: Load ALL assigned testcases (for ALL users) scoped to this Release
          this.testCaseAssignmentService
            .getAssignedTestCasesForLibraryAndRelease(
              requestedLibrary.libraryName ?? '',
              requestedRelease.releaseId ?? 0
            )
            .subscribe({
              next: (allAssigned) => {
                if (requestId !== this.loadRequestId) return;

                // STEP 3: Load only assignments for CURRENT USER
                this.testCaseAssignmentService
                  .getTestCasesByAssignmentAndUser(
                    requestedUser.userId ?? 0,
                    assignmentName
                  )
                  .subscribe({
                    next: (myAssigned) => {
                      if (requestId !== this.loadRequestId) return;

                      const myAssignedIds = new Set(
                        myAssigned.map((a) => a.testCaseId)
                      );
                      this.originallyAssignedIds = myAssignedIds;

                      // TestCases assigned to ANY user (reverted to the original,
                      // pre-disable-checkbox behavior per explicit request - see AGENTS.md).
                      const allAssignedIds = new Set(
                        allAssigned.map((a) => a.testCaseId)
                      );

                      // STEP 4: Filter test cases available for CURRENT USER
                      let filteredTestCases = loadedTestCases.filter(
                        (tc) =>
                          !allAssignedIds.has(tc.testCaseId) ||
                          myAssignedIds.has(tc.testCaseId)
                      );

                      // STEP 5: Assign correct assignedUserName/testCaseStatus for ALL
                      // test cases. Unambiguous here (unlike an earlier, reverted
                      // attempt) because Step 4 already guarantees a visible row is only
                      // ever unassigned or assigned to the current tester - never someone
                      // else's - so `assignedEntry` (from allAssigned) is never a
                      // different tester's data by the time we get here.
                      filteredTestCases.forEach((tc) => {
                        const assignedEntry = allAssigned.find(
                          (a) => a.testCaseId === tc.testCaseId
                        );

                        if (assignedEntry) {
                          // 🟦 Test case is assigned → show the correct user name
                          tc.assignedUserName =
                            assignedEntry.assignedUserName || '';
                          tc.testCaseStatus = assignedEntry.testCaseStatus || '';
                        } else {
                          // 🟪 Not assigned → show Unassigned
                          tc.assignedUserName = '';
                          tc.testCaseStatus = '';
                        }

                        // Mark selected only if assigned to THIS user
                        tc.selected = myAssignedIds.has(tc.testCaseId);
                      });

                      // Only now, once every step of THIS request has been confirmed
                      // current, actually publish the results to the grid.
                      this.testCases = filteredTestCases;

                      // STEP 6: Populate selectedMethods
                      Promise.resolve().then(() => {
                        if (requestId !== this.loadRequestId) return;
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
        // Preserve the real status for anything already assigned (the server ignores
        // this anyway once a test case is locked, but sending the real value - instead
        // of always hardcoding 'Assigned' - keeps the request honest and avoids relying
        // solely on the server-side guard). Brand-new selections have no status yet, so
        // they still default to 'Assigned'.
        testCaseStatus: tc.testCaseStatus || 'Assigned',
        className: tc.className,
        libraryName: tc.libraryName,
        methodName: tc.methodName,
        priority: tc.priority,
      })),
    };

    // Diff against the assignment set as it was when this screen last loaded, so the
    // success toast can report exactly what changed instead of a generic message -
    // computed here (before the save call) since selectedMethods is the "after" state.
    const selectedIds = new Set(this.selectedMethods.map((tc) => tc.testCaseId));
    const addedCount = [...selectedIds].filter(
      (id) => !this.originallyAssignedIds.has(id)
    ).length;
    const removedCount = [...this.originallyAssignedIds].filter(
      (id) => !selectedIds.has(id)
    ).length;

    this.testCaseAssignmentService.saveAssignment(request).subscribe({
      next: (res: any) => {
        this.showSaveResultToast(
          res,
          'Assignments saved successfully.',
          addedCount,
          removedCount
        );
        this.tryLoadTestCases(); // Refresh grid
        this.loadLibraryTestCaseCounts(this.selectedLibrary?.libraryName ?? '');
      },
      error: (err) => {
        console.error(err);
        this.toaster.error('Failed to save assignments.');
      },
    });
  }

  // Server-side lock enforcement (usp_CreateOrUpdateAssignmentWithTestCases) reports back
  // how many test cases it left untouched because they'd already entered the execution
  // pipeline - surfaced here so a Save/Reset that silently skipped something isn't a
  // silent no-op from the user's point of view. addedCount/removedCount (Save only - Reset
  // doesn't pass these) give a concrete "what actually changed" summary instead of a
  // generic success message.
  private showSaveResultToast(
    res: any,
    defaultMessage: string,
    addedCount = 0,
    removedCount = 0
  ): void {
    const lockedCount = res?.lockedCount ?? 0;
    if (lockedCount > 0) {
      this.toaster.info(
        `${lockedCount} test case(s) could not be changed because they have already been executed.`
      );
    } else if (addedCount > 0 || removedCount > 0) {
      const parts: string[] = [];
      if (addedCount > 0) parts.push(`${addedCount} added`);
      if (removedCount > 0) parts.push(`${removedCount} removed`);
      this.toaster.success(`Assignments saved: ${parts.join(', ')}.`);
    } else {
      this.toaster.success(defaultMessage);
    }
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
      next: (res: any) => {
        this.showSaveResultToast(res, 'All assignments reset.');
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
    // Release-scoped (not getAllAssignedTestCasesInLibrary, which counts assignments
    // across every Release that ever used this library name) - using the non-scoped
    // endpoint here let assignedCount exceed totalCases, so unassignedCount went
    // negative whenever a library had more historical assignments from other Releases
    // than the current Release has test cases. Same endpoint tryLoadTestCases() already
    // uses correctly.
    const assignedCases$ =
      this.testCaseAssignmentService.getAssignedTestCasesForLibraryAndRelease(
        libraryName,
        this.selectedRelease.releaseId
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

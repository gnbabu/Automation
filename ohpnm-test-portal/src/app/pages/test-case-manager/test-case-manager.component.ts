import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import {
  ClassInfo,
  GridColumn,
  ITestCaseAssignment,
  IUser,
  LibraryInfo,
  LibraryMethodInfo,
} from '@interfaces';
import {
  AuthService,
  CommonToasterService,
  TestCaseManagerService,
  TestRunnerService,
  TestSuitesService,
  UsersService,
} from '@services';
import { AppDropdownComponent } from 'app/core/components/app-dropdown/app-dropdown.component';
import { DataGridComponent } from 'app/core/components/data-grid/data-grid.component';
import { forkJoin } from 'rxjs';

type LibraryMethodRow = LibraryMethodInfo & {
  libraryName?: string;
  className?: string;
  assigned?: boolean;
  selected?: boolean;
};

@Component({
  selector: 'app-test-case-manager',
  standalone: true,
  imports: [CommonModule, FormsModule, AppDropdownComponent, DataGridComponent],
  templateUrl: './test-case-manager.component.html',
  styleUrl: './test-case-manager.component.css',
})
export class TestCaseManagerComponent implements OnInit {
  libraries: LibraryInfo[] = [];
  classes: ClassInfo[] = [];
  // use enriched row type for methods & filteredMethods
  methods: LibraryMethodRow[] = [];
  filteredMethods: LibraryMethodRow[] = [];

  users: IUser[] = [];
  selectedUser: IUser | null = null;
  selectedLibrary: LibraryInfo | null = null;
  selectedClass: ClassInfo | null = null;
  selectedMethod: LibraryMethodInfo | null = null;

  columns: GridColumn[] = [];
  pageSize = 10;
  selectedMethods: any[] = [];

  constructor(
    private router: Router,
    private testSuitesService: TestSuitesService,
    private authService: AuthService,
    private testRunnerService: TestRunnerService,
    private toaster: CommonToasterService,
    private userService: UsersService,
    private testCaseManagerService: TestCaseManagerService
  ) {}

  ngOnInit(): void {
    this.columns = [
      { field: 'methodName', header: 'Method Name', sortable: true },
      { field: 'libraryName', header: 'Library', sortable: true },
      { field: 'className', header: 'Class', sortable: true },
    ];
    this.loadUsers();
  }

  loadUsers() {
    this.userService.getAll().subscribe({
      next: (res) => (this.users = res),
      error: (err) => console.error('Failed to load users:', err),
    });
  }
  getUserFullName(user: any): string {
    return `${user.firstName} ${user.lastName}`;
  }

  onUserChange(user: IUser | null) {
    this.selectedUser = user;
    this.resetSelections(['library', 'class', 'method']);

    if (!user) {
      this.libraries = [];
      this.filteredMethods = [];
      this.selectedMethods = [];
      return;
    }

    this.loadLibrariesAndAssignments(user.userId);
  }

  loadLibrariesAndAssignments(userId: any) {
    forkJoin({
      libraries: this.testSuitesService.getLibraries(),
      assignments: this.testCaseManagerService.getAssignmentsByUserId(userId),
    }).subscribe({
      next: ({ libraries, assignments }) => {
        this.libraries = libraries || [];

        // Flatten all methods across all libraries
        const allMethods = this.mapMethodsWithContext(this.libraries);

        // Mark assigned methods based on API response
        this.filteredMethods = allMethods.map((m) => {
          const assigned = assignments.some(
            (a) =>
              a.libraryName === m.libraryName &&
              a.className === m.className &&
              a.methodName === m.methodName
          );
          return { ...m, assigned, selected: assigned };
        });
        // Set the selected rows for checkboxes
        this.selectedMethods = this.filteredMethods.filter((m) => m.assigned);
      },
      error: (err) => {
        console.error(err);
        this.libraries = [];
        this.filteredMethods = [];
        this.selectedMethods = [];
      },
    });
  }

  loadLibraries() {
    this.testSuitesService.getLibraries().subscribe({
      next: (res) => {
        this.libraries = res || [];
        // When a user is selected, show ALL methods across ALL libraries (enriched)
        this.filteredMethods = this.mapMethodsWithContext(this.libraries);
      },
      error: (err) => {
        console.error('Failed to load libraries:', err);
        this.libraries = [];
        this.filteredMethods = [];
      },
    });
  }

  onLibraryChange(lib: LibraryInfo | null) {
    this.selectedLibrary = lib;
    this.resetSelections(['class', 'method']);

    if (lib) {
      this.classes = lib.classes || [];
      const methods = this.mapLibraryMethods(lib);
      this.filteredMethods = this.preserveSelections(methods);
    } else if (this.selectedUser) {
      const methods = this.mapMethodsWithContext(this.libraries);
      this.filteredMethods = this.preserveSelections(methods);
      this.classes = [];
    } else {
      this.classes = [];
      this.filteredMethods = [];
    }

    // Update selectedMethods after rebuilding filteredMethods
    this.selectedMethods = this.filteredMethods.filter((m) => m.selected);
  }

  onClassChange(cls: ClassInfo | null) {
    this.selectedClass = cls;

    let newMethods: LibraryMethodRow[] = [];

    if (cls && this.selectedLibrary) {
      // enrich class methods
      newMethods = (cls.methods || []).map(
        (m) =>
          ({
            ...(m as any),
            libraryName: this.selectedLibrary?.libraryName,
            className: cls.className,
          } as LibraryMethodRow)
      );
      this.methods = newMethods; // keep for dropdown
    } else if (this.selectedLibrary) {
      newMethods = this.mapLibraryMethods(this.selectedLibrary);
    } else if (this.selectedUser) {
      newMethods = this.mapMethodsWithContext(this.libraries);
    }

    this.filteredMethods = this.preserveSelections(newMethods);
    this.selectedMethods = this.filteredMethods.filter((m) => m.selected);
  }

  onMethodChange(method: LibraryMethodInfo | null) {
    this.selectedMethod = method;

    let newMethods: LibraryMethodRow[] = [];

    if (method && this.selectedClass && this.selectedLibrary) {
      // single enriched row
      newMethods = [
        {
          ...(method as any),
          libraryName: this.selectedLibrary.libraryName,
          className: this.selectedClass.className,
        } as LibraryMethodRow,
      ];
    } else if (this.selectedClass && this.selectedLibrary) {
      // all methods of the selected class
      newMethods = (this.selectedClass.methods || []).map(
        (m) =>
          ({
            ...(m as any),
            libraryName: this.selectedLibrary?.libraryName,
            className: this.selectedClass?.className,
          } as LibraryMethodRow)
      );
    } else if (this.selectedLibrary) {
      newMethods = this.mapLibraryMethods(this.selectedLibrary);
    } else if (this.selectedUser) {
      newMethods = this.mapMethodsWithContext(this.libraries);
    }

    // Preserve previous selections
    this.filteredMethods = newMethods.map((m) => {
      const isSelected = this.selectedMethods.some(
        (s) =>
          s.libraryName === m.libraryName &&
          s.className === m.className &&
          s.methodName === m.methodName
      );
      return { ...m, selected: isSelected, assigned: isSelected };
    });

    // Update selectedMethods to match current filteredMethods
    this.selectedMethods = this.filteredMethods.filter((m) => m.selected);
  }

  // 🔹 Utility: enrich methods across ALL libraries
  private mapMethodsWithContext(libs: LibraryInfo[]): LibraryMethodRow[] {
    return (libs || []).flatMap((lib) =>
      (lib.classes || []).flatMap((cls) =>
        (cls.methods || []).map(
          (m) =>
            ({
              ...(m as any),
              libraryName: lib.libraryName,
              className: cls.className,
            } as LibraryMethodRow)
        )
      )
    );
  }

  // 🔹 Utility: enrich methods from a SINGLE library
  private mapLibraryMethods(lib: LibraryInfo): LibraryMethodRow[] {
    return (lib?.classes || []).flatMap((cls) =>
      (cls.methods || []).map(
        (m) =>
          ({
            ...(m as any),
            libraryName: lib.libraryName,
            className: cls.className,
          } as LibraryMethodRow)
      )
    );
  }

  private resetSelections(levels: ('library' | 'class' | 'method')[]) {
    if (levels.includes('library')) {
      this.selectedLibrary = null;
      this.classes = [];
    }
    if (levels.includes('class')) {
      this.selectedClass = null;
      this.methods = [];
    }
    if (levels.includes('method')) {
      this.selectedMethod = null;
    }
  }
  preserveSelections(newMethods: LibraryMethodRow[]): LibraryMethodRow[] {
    return newMethods.map((m) => {
      const alreadySelected = this.selectedMethods.some(
        (s) =>
          s.libraryName === m.libraryName &&
          s.className === m.className &&
          s.methodName === m.methodName
      );
      return { ...m, selected: alreadySelected, assigned: alreadySelected };
    });
  }

  onSaveAssignments() {
    if (!this.selectedUser || this.selectedUser.userId == null) {
      this.toaster.error('Select a valid user first');
      return;
    }

    const selectedAssignments: ITestCaseAssignment[] = this.selectedMethods
      //.filter((m) => m.assigned)
      .map((m) => ({
        userId: this.selectedUser?.userId, // now guaranteed to exist
        libraryName: m.libraryName!,
        className: m.className!,
        methodName: m.methodName!,
        assignedBy: this.authService.getLoggedInUser()?.userId ?? 0,
      }));

    this.testCaseManagerService.saveAssignments(selectedAssignments).subscribe({
      next: () => {
        this.toaster.success('Assignments saved');
        this.loadLibrariesAndAssignments(this.selectedUser?.userId);
      },
      error: (err) => {
        console.error(err);
        this.toaster.error('Failed to save assignments');
      },
    });
  }
  onResetAssignments() {
    if (!this.selectedUser) {
      this.toaster.error('Select a user first');
      return;
    }

    this.testCaseManagerService
      .deleteAssignments(this.selectedUser.userId)
      .subscribe({
        next: () => {
          this.toaster.success('Assignments removed');
          this.loadLibrariesAndAssignments(this.selectedUser?.userId);
        },
        error: (err) => {
          console.error(err);
          this.toaster.error('Failed to remove assignments');
        },
      });
  }
}

import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import {
  ClassInfo,
  GridColumn,
  IUser,
  LibraryInfo,
  LibraryMethodInfo,
} from '@interfaces';
import {
  AuthService,
  CommonToasterService,
  TestRunnerService,
  TestSuitesService,
  UsersService,
} from '@services';
import { AppDropdownComponent } from 'app/core/components/app-dropdown/app-dropdown.component';
import { DataGridComponent } from 'app/core/components/data-grid/data-grid.component';

type LibraryMethodRow = LibraryMethodInfo & {
  libraryName?: string;
  className?: string;
  assigned?: boolean;
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

  constructor(
    private router: Router,
    private testSuitesService: TestSuitesService,
    private authService: AuthService,
    private testRunnerService: TestRunnerService,
    private toaster: CommonToasterService,
    private userService: UsersService
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

    if (user) {
      this.loadLibraries();
    } else {
      this.libraries = [];
      this.filteredMethods = [];
    }
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
      this.filteredMethods = this.mapLibraryMethods(lib);
    } else if (this.selectedUser) {
      this.filteredMethods = this.mapMethodsWithContext(this.libraries);
      this.classes = [];
    } else {
      this.classes = [];
      this.filteredMethods = [];
    }
  }

  onClassChange(cls: ClassInfo | null) {
    this.resetSelections(['method']);
    this.selectedClass = cls;

    if (cls && this.selectedLibrary) {
      this.methods = (cls.methods || []).map(
        (m) =>
          ({
            ...(m as any),
            libraryName: this.selectedLibrary!.libraryName,
            className: cls.className,
          } as LibraryMethodRow)
      );
      this.filteredMethods = [...this.methods];
    } else if (this.selectedLibrary) {
      this.filteredMethods = this.mapLibraryMethods(this.selectedLibrary);
    } else if (this.selectedUser) {
      this.filteredMethods = this.mapMethodsWithContext(this.libraries);
    } else {
      this.methods = [];
      this.filteredMethods = [];
    }
  }

  onMethodChange(method: LibraryMethodInfo | null) {
    this.selectedMethod = method;

    if (method && this.selectedClass && this.selectedLibrary) {
      // single enriched row
      this.filteredMethods = [
        {
          ...(method as any),
          libraryName: this.selectedLibrary.libraryName,
          className: this.selectedClass.className,
        } as LibraryMethodRow,
      ];
    } else if (this.selectedClass && this.selectedLibrary) {
      // all methods of class enriched
      this.filteredMethods = (this.selectedClass.methods || []).map(
        (m) =>
          ({
            ...(m as any),
            libraryName: this.selectedLibrary!.libraryName,
            className: this.selectedClass!.className,
          } as LibraryMethodRow)
      );
    } else if (this.selectedLibrary) {
      this.filteredMethods = this.mapLibraryMethods(this.selectedLibrary);
    } else if (this.selectedUser) {
      this.filteredMethods = this.mapMethodsWithContext(this.libraries);
    } else {
      this.filteredMethods = [];
    }
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
}

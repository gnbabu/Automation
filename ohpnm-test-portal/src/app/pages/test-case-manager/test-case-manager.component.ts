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
  methods: LibraryMethodInfo[] = [];
  filteredMethods: LibraryMethodInfo[] = [];

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
    ];
    this.loadUsers();
  }

  loadUsers() {
    this.userService.getAll().subscribe({
      next: (res) => (this.users = res),
      error: (err) => console.error('Failed to load users:', err),
    });
  }

  onUserChange(user: IUser | null) {
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
      next: (res) => (this.libraries = res),
      error: (err) => console.error('Failed to load libraries:', err),
    });
  }

  onLibraryChange(lib: LibraryInfo | null) {
    this.resetSelections(['class', 'method']);
    if (lib) {
      this.classes = lib.classes;
      this.filteredMethods = lib.classes.flatMap((c) => c.methods);
    } else {
      this.classes = [];
      this.filteredMethods = [];
    }
  }

  onClassChange(cls: ClassInfo | null) {
    this.resetSelections(['method']);
    if (cls) {
      this.methods = cls.methods;
      this.filteredMethods = [...cls.methods];
    } else if (this.selectedLibrary) {
      // If library is selected but class is deselected, show all library methods
      this.methods = [];
      this.filteredMethods = this.selectedLibrary.classes.flatMap(
        (c) => c.methods
      );
    } else {
      this.methods = [];
      this.filteredMethods = [];
    }
  }

  onMethodChange(method: LibraryMethodInfo | null) {
    if (method) {
      this.filteredMethods = [method];
    } else if (this.selectedClass) {
      this.filteredMethods = [...this.selectedClass.methods];
    } else if (this.selectedLibrary) {
      this.filteredMethods = this.selectedLibrary.classes.flatMap(
        (c) => c.methods
      );
    } else {
      this.filteredMethods = [];
    }
  }

  /**
   * Reset dependent selections based on type
   */
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

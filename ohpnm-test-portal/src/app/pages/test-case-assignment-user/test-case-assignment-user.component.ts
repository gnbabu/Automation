import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { GridColumn, IUser, LibraryInfo } from '@interfaces';
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
  imports: [AppDropdownComponent, CommonModule, FormsModule],
  templateUrl: './test-case-assignment-user.component.html',
  styleUrl: './test-case-assignment-user.component.css',
})
export class TestCaseAssignmentUserComponent implements OnInit {
  libraries: LibraryInfo[] = [];
  assignmentStatuses: any[] = [];
  selectedLibrary: LibraryInfo | null = null;
  selectedAssignmentStatus: any = null;
  users: IUser[] = [];
  selectedUser: IUser | null = null;

  environments: any[] = [];
  selectedEnvironment: any = null;

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
  }

  onLibraryChange(library: LibraryInfo | null) {
    this.selectedLibrary = library;
    const libName = library?.libraryName || undefined;
  }

  onAssignmentStatusChange(status: any) {
    this.selectedAssignmentStatus = status;
  }

  onUserChange(user: IUser | null) {
    this.selectedUser = user;
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
    this.selectedEnvironment = env;
  }
}

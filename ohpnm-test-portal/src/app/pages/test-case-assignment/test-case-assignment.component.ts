import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LibraryInfo } from '@interfaces';
import { AppMultiselectDropdownComponent } from 'app/core/components/app-multiselect-dropdown/app-multiselect-dropdown.component';
import {
  AuthService,
  CommonToasterService,
  TestCaseManagerService,
  TestSuitesService,
  UsersService,
} from '@services';
import { AppDropdownComponent } from 'app/core/components/app-dropdown/app-dropdown.component';

@Component({
  selector: 'app-test-case-assignment',
  imports: [
    AppDropdownComponent,
    AppMultiselectDropdownComponent,
    CommonModule,
    FormsModule,
  ],
  templateUrl: './test-case-assignment.component.html',
  styleUrl: './test-case-assignment.component.css',
})
export class TestCaseAssignmentComponent implements OnInit {
  libraries: LibraryInfo[] = [];
  assignmentStatuses: any[] = [];
  selectedLibrary: LibraryInfo | null = null;
  selectedAssignmentStatus: any = null;

  selectedUsers: any[] = [];
  users = [
    { id: 1, name: 'John Doe', email: 'john@example.com' },
    { id: 2, name: 'Jane Smith', email: 'jane@example.com' },
    { id: 3, name: 'Robert Brown', email: 'robert@example.com' },
    { id: 4, name: 'Alice Johnson', email: 'alice@example.com' },
  ];

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
  }

  loadAssignmentStatuses() {
    this.assignmentStatuses = [
      { assignmentStatus: 'Assigned' },
      { assignmentStatus: 'Unassigned' },
    ];
  }

  loadTestSuites() {
    this.testSuitesService.getLibraries().subscribe({
      next: (response) => {
        this.libraries = response || [];
      },
      error: (err) => {
        console.error(err);
        this.libraries = [];
      },
    });
  }

  onLibraryChange(lib: LibraryInfo | null) {
    this.selectedLibrary = lib;
  }

  onAssignmentStatusChange(event: any) {
    this.selectedAssignmentStatus = event;
    console.log('Selected Status:', event);
  }

  onUsersChange(selected: any[]) {
    console.log('Users selected:', selected);
    // Call your API here if needed
  }

  onSelectAll(selected: any[]) {
    console.log('Select All clicked:', selected);
  }

  onClearAll() {
    console.log('Clear clicked');
  }
}

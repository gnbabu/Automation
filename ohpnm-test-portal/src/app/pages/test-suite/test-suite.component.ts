import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ClassInfo,
  GridColumn,
  IQueueInfo,
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
} from '@services';
import { Router } from '@angular/router';
import { QueueInfoMapper } from '@mappers';
import { DataGridComponent } from 'app/core/components/data-grid/data-grid.component';
import { forkJoin } from 'rxjs';
import { FormsModule } from '@angular/forms';
import { AppDropdownComponent } from 'app/core/components/app-dropdown/app-dropdown.component';

@Component({
  selector: 'app-test-suite',
  standalone: true,
  imports: [CommonModule, FormsModule, DataGridComponent, AppDropdownComponent],
  templateUrl: './test-suite.component.html',
  styleUrls: ['./test-suite.component.css'],
})
export class TestSuiteComponent implements OnInit {
  columns: GridColumn[] = [];
  pageSize = 10;
  @ViewChild('indexTemplate', { static: true })
  indexTemplate!: TemplateRef<any>;

  loggedInUser: IUser | null = null;
  libraries: LibraryInfo[] = [];
  selectedMethods: LibraryMethodInfo[] = [];

  expandedLibraries = new Set<string>();
  expandedClasses = new Set<string>();

  selectedLibraryName: string | null = null;
  selectedClassName: string | null = null;
  selectedMethodName: string | null = null;

  // Environments list (binds to app-dropdown)
  environments = [
    { id: 'qa', name: 'QA' },
    { id: 'staging', name: 'Staging' },
    { id: 'prod', name: 'Production' },
  ];

  // Selected environment
  selectedEnvironment: any = null;

  // Selected browser (radio binding)
  selectedBrowser: string = 'Chrome'; // default

  constructor(
    private router: Router,
    private testSuitesService: TestSuitesService,
    private authService: AuthService,
    private testRunnerService: TestRunnerService,
    private toaster: CommonToasterService,
    private testCaseManagerService: TestCaseManagerService
  ) {}

  ngOnInit(): void {
    this.columns = [
      // {
      //   field: 'index',
      //   header: '#',
      //   sortable: false,
      //   cellTemplate: this.indexTemplate, // show i+1
      // },
      {
        field: 'methodName',
        header: 'Method Name',
        sortable: true,
      },
    ];

    this.loggedInUser = this.authService.getLoggedInUser();

    if (this.authService.isAdmin()) {
      this.loadLibraries();
    } else {
      this.loadLibrariesAndAssignments(this.loggedInUser?.userId);
    }
  }

  loadLibrariesAndAssignments(userId: any) {
    forkJoin({
      libraries: this.testSuitesService.getLibraries(),
      assignments: this.testCaseManagerService.getAssignmentsByUserId(userId),
    }).subscribe({
      next: ({ libraries, assignments }) => {
        // Build a map for quick lookup: { libraryName -> { className -> [methodName] } }
        const assignmentMap = new Map<string, Map<string, Set<string>>>();

        assignments.forEach((a) => {
          if (!assignmentMap.has(a.libraryName)) {
            assignmentMap.set(a.libraryName, new Map());
          }
          const classMap = assignmentMap.get(a.libraryName)!;
          if (!classMap.has(a.className)) {
            classMap.set(a.className, new Set());
          }
          classMap.get(a.className)!.add(a.methodName);
        });

        // Filter libraries, classes, and methods based on assignments
        this.libraries = (libraries || [])
          .map((lib: any) => {
            if (!assignmentMap.has(lib.libraryName)) return null;

            const classMap = assignmentMap.get(lib.libraryName)!;
            const filteredClasses = (lib.classes || [])
              .map((cls: any) => {
                if (!classMap.has(cls.className)) return null;

                const assignedMethods = classMap.get(cls.className)!;
                const filteredMethods = (cls.methods || []).filter((m: any) =>
                  assignedMethods.has(m.methodName)
                );

                if (filteredMethods.length === 0) return null;

                return { ...cls, methods: filteredMethods };
              })
              .filter((c: any) => c !== null);

            if (filteredClasses.length === 0) return null;
            return { ...lib, classes: filteredClasses };
          })
          .filter((l: any) => l !== null);
      },
      error: (err) => {
        console.error(err);
        this.libraries = [];
      },
    });
  }

  loadLibraries() {
    this.testSuitesService.getLibraries().subscribe({
      next: (res) => {
        this.libraries = res;
      },
      error: (err) => {
        console.error('Failed to load libraries:', err);
      },
    });
  }

  toggleLibrary(library: string) {
    this.expandedLibraries.has(library)
      ? this.expandedLibraries.delete(library)
      : this.expandedLibraries.add(library);
  }

  toggleClass(cls: string) {
    this.expandedClasses.has(cls)
      ? this.expandedClasses.delete(cls)
      : this.expandedClasses.add(cls);
  }

  isLibraryExpanded(library: string) {
    return this.expandedLibraries.has(library);
  }

  isClassExpanded(cls: string) {
    return this.expandedClasses.has(cls);
  }

  onLibraryClick(lib: LibraryInfo) {
    this.selectedLibraryName = lib.libraryName;
    this.selectedClassName = null;
    this.selectedMethodName = null;
    this.selectedMethods = lib.classes.flatMap((c) => c.methods);
  }

  onClassClick(cls: ClassInfo) {
    this.selectedClassName = cls.className;
    this.selectedLibraryName = null;
    this.selectedMethodName = null;
    this.selectedMethods = [...cls.methods];
  }

  onMethodClick(method: LibraryMethodInfo) {
    this.selectedMethodName = method.methodName;
    this.selectedLibraryName = null;
    this.selectedClassName = null;
    this.selectedMethods = [method];
  }

  runTests() {
    const methods = this.selectedMethods.map((m) => m.methodName).join(', ');
    alert('Running tests for:\n' + methods);
    console.log('Running tests for:', methods);
  }

  navigateToTestData() {
    this.router.navigate(['/configure-test-data']);
  }

  runLibrary(lib: any) {
    const queue: IQueueInfo = {
      ...QueueInfoMapper.empty(),
      queueName: `Run ${lib.libraryName}`,
      queueDescription: `Running all tests in ${lib.libraryName}`,
      empId: this.loggedInUser?.userName,
      productLine: lib.libraryName,
      queueStatus: 'New', // Change to New New -> Running ->Completed -> Failed
      libraryName: lib.libraryName,
      userId: this.loggedInUser?.userId,
      browser: this.selectedBrowser,
    };

    this.testRunnerService.runTest(queue).subscribe({
      next: (msg) => {
        this.toaster.success(`Library tests queued successfully`);
      },
      error: (err) => {
        this.toaster.error(`Error on Library tests queued : ${err.message}`);
      },
    });
  }

  runClass(lib: any, cls: any) {
    const queue: IQueueInfo = {
      ...QueueInfoMapper.empty(),
      queueName: `Run ${cls.className}`,
      queueDescription: `Running all tests in ${lib.libraryName}.${cls.className}`,
      empId: this.loggedInUser?.userName,
      productLine: 'Core',
      queueStatus: 'New',
      libraryName: lib.libraryName,
      className: cls.className,
      userId: this.loggedInUser?.userId,
      browser: this.selectedBrowser,
    };

    this.testRunnerService.runTest(queue).subscribe({
      next: (msg) => {
        this.toaster.success(`Class tests queued successfully`);
      },
      error: (err) => {
        this.toaster.error(`Error on class tests queued : ${err.message}`);
      },
    });
  }

  runMethod(lib: any, cls: any, method: any) {
    const queue: IQueueInfo = {
      ...QueueInfoMapper.empty(),
      productLine: 'Core',
      queueStatus: 'New',
      queueName: `Run ${method.methodName}`,
      queueDescription: `Running ${lib.libraryName}.${cls.className}.${method.methodName}`,
      empId: this.loggedInUser?.userName,
      libraryName: lib.libraryName,
      className: cls.className,
      methodName: method.methodName,
      userId: this.loggedInUser?.userId,
      browser: this.selectedBrowser,
    };

    this.testRunnerService.runTest(queue).subscribe({
      next: (msg) => {
        this.toaster.success(`Test method queued successfully`);
      },
      error: (err) => {
        this.toaster.error(`Error on cTest method queued : ${err.message}`);
      },
    });
  }
}

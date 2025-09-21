import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import {
  GridColumn,
  IQueueInfo,
  ITestResult,
  ITestScreenshot,
} from '@interfaces';
import { QueueService, ScreenshotService, TestResultService } from '@services';
import { DataGridComponent } from 'app/core/components/data-grid/data-grid.component';
import { TestScreenshotGalleryComponent } from './test-screenshot-gallery/test-screenshot-gallery.component';

@Component({
  selector: 'app-queue-detail',
  standalone: true,
  imports: [CommonModule, DataGridComponent, TestScreenshotGalleryComponent],
  templateUrl: './queue-detail.component.html',
  styleUrl: './queue-detail.component.css',
})
export class QueueDetailsComponent implements OnInit {
  queue: IQueueInfo | null = null;
  screenshots: ITestScreenshot[] = [];
  @ViewChild('statusTemplate', { static: true })
  statusTemplate!: TemplateRef<any>;

  @ViewChild('screenshotTemplate', { static: true })
  screenshotTemplate!: TemplateRef<any>;

  columns: GridColumn[] = [];

  testResults: ITestResult[] = [];
  totalRecords = 0;
  pageSize = 10;
  selectedTestId: number | null = null;

  constructor(
    private queueService: QueueService,
    private testResultService: TestResultService,
    private route: ActivatedRoute,
    private router: Router,
    private screenshotService: ScreenshotService
  ) {}

  ngOnInit(): void {
    this.columns = [
      {
        field: 'name',
        header: 'Name',
        sortable: true,
      },
      {
        field: 'resultStatus',
        header: 'Status',
        sortable: true,
        cellTemplate: this.statusTemplate,
      },
      { field: 'duration', header: 'Duration', sortable: true },
      {
        field: 'startTime',
        header: 'Start Time',
        sortable: true,
        type: 'datetime',
      },
      {
        field: 'endTime',
        header: 'End Time',
        sortable: true,
        type: 'datetime',
      },
      {
        field: 'screenshots',
        header: 'Screenshots',
        cellTemplate: this.screenshotTemplate,
      },
    ];

    const queueId = this.route.snapshot.paramMap.get('queueId');

    this.queueService.getQueueById(queueId!).subscribe({
      next: (data) => {
        this.queue = data;
      },
      error: (err) => console.error('Failed to load queue', err),
    });

    this.testResultService.getTestResultsByQueueId(queueId).subscribe({
      next: (res) => {
        this.testResults = res ?? [];
        this.totalRecords = this.testResults.length;
      },
      error: (err) => {
        console.error('Failed to load test results:', err);
        this.testResults = [];
        this.totalRecords = 0;
      },
    });
  }

  getBadgeClass(status?: string): string {
    switch (status) {
      case 'New':
        return 'bg-info';
      case 'Completed':
        return 'bg-success';
      case 'Running':
        return 'bg-warning text-dark';
      case 'Failed':
        return 'bg-danger';
      case 'Pending':
        return 'bg-secondary';
      default:
        return 'bg-light text-dark border';
    }
  }
  goBack(): void {
    this.router.navigate(['/queue-manager']);
  }

  viewSceenshots(data: any) {
    debugger;
    this.screenshotService
      .getScreenshotsByTestResultIdAsync(data.queueId, data.name)
      .subscribe({
        next: (res) => {
          debugger;
          this.screenshots = res ?? [];
        },
        error: (err) => {
          console.error('Failed to load test results:', err);
          this.testResults = [];
          this.totalRecords = 0;
        },
      });
  }
}

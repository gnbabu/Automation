import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { GridColumn, IQueueInfo, ITestResult } from '@interfaces';
import { QueueService, TestResultService } from '@services';
import { DataGridComponent } from 'app/core/components/data-grid/data-grid.component';

@Component({
  selector: 'app-queue-detail',
  standalone: true,
  imports: [CommonModule, DataGridComponent],
  templateUrl: './queue-detail.component.html',
  styleUrl: './queue-detail.component.css',
})
export class QueueDetailsComponent implements OnInit {
  queue: IQueueInfo | null = null;

  @ViewChild('statusTemplate', { static: true })
  statusTemplate!: TemplateRef<any>;

  columns: GridColumn[] = [];

  testResults: ITestResult[] = [];
  totalRecords = 0;
  pageSize = 10;

  constructor(
    private queueService: QueueService,
    private testResultService: TestResultService,
    private route: ActivatedRoute,
    private router: Router
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
}

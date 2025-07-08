import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgChartsModule } from 'ng2-charts';
import { ChartOptions } from 'chart.js';
import { GridColumn, IQueueInfo, IQueueReportFilterRequest } from '@interfaces';
import { AuthService, QueueService } from '@services';
import { DataGridComponent } from 'app/core/components/data-grid/data-grid.component';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule, NgChartsModule, DataGridComponent],
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.css'],
})
export class ReportsComponent implements OnInit {
  columns: GridColumn[] = [];
  @ViewChild('statusTemplate', { static: true })
  statusTemplate!: TemplateRef<any>;

  pageSize = 10;
  queueReports: IQueueInfo[] = [];
  filteredQueueReports: IQueueInfo[] = [];

  queueFilters = {
    status: '',
    fromDate: '',
    toDate: '',
  };

  queuePieLabels = [
    'Completed',
    'Failed',
    'In Progress',
    'Awaiting',
    'Retried',
    'Cancelled',
  ];

  pieChartData = {
    labels: this.queuePieLabels,
    datasets: [
      {
        data: [0, 0, 0, 0, 0, 0],
        backgroundColor: [
          '#198754', // Completed
          '#DC3545FF', // Failed
          '#FFC107', // In Progress
          '#6C757D', // Awaiting
          '#0DCAF0', // Retried
          '#212529', // Cancelled
        ],
        hoverBackgroundColor: [
          '#198754CC', // Completed - 80% opacity
          '#DC3545CC', // Failed
          '#FFC107CC', // In Progress
          '#6C757DCC', // Awaiting
          '#0DCAF0CC', // Retried
          '#212529CC', // Cancelled
        ],
        hoverOffset: 0,
      },
    ],
  };

  pieChartOptions: ChartOptions<'pie'> = {
    responsive: true,
    plugins: {
      legend: {
        position: 'right',
      },
    },
  };

  constructor(
    private queueService: QueueService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.columns = [
      { field: 'queueName', header: 'Queue Name', sortable: true },
      {
        field: 'queueStatus',
        header: 'Status',
        sortable: true,
        cellTemplate: this.statusTemplate,
      },
      { field: 'userName', header: 'Employee', sortable: true },
      { field: 'productLine', header: 'Product Line', sortable: true },
      { field: 'libraryName', header: 'Library', sortable: true },
      { field: 'className', header: 'Class', sortable: true },
      { field: 'methodName', header: 'Method', sortable: true },
      {
        field: 'createdDate',
        header: 'Created',
        sortable: true,
        type: 'datetime',
      },
    ];

    this.loadQueueReports();
  }

  loadQueueReports(): void {
    const toNullableString = (value?: string): string | undefined =>
      value?.trim() ? value : undefined;

    const filter: IQueueReportFilterRequest = {
      userId: this.authService.getLoggedInUserId(),
      status: this.queueFilters.status,
      fromDate: toNullableString(this.queueFilters.fromDate),
      toDate: toNullableString(this.queueFilters.toDate),
      page: 1,
      pageSize: 0,
      sortColumn: 'CreatedDate',
      sortDirection: 'DESC', // Optional: ASC or DESC
    };

    this.queueService.getQueueReports(filter).subscribe((res) => {
      this.queueReports = res.data;
      this.filteredQueueReports = res.data;
      this.updateQueueChart();
    });
  }

  updateQueueChart(): void {
    const counts: Record<string, number> = {
      Completed: 0,
      Failed: 0,
      'In Progress': 0,
      Awaiting: 0,
      Retried: 0,
      Cancelled: 0,
    };

    for (const report of this.queueReports) {
      const status = report.queueStatus;
      if (status && counts[status] !== undefined) {
        counts[status]++;
      }
    }

    const data = this.queuePieLabels.map((label) => counts[label]);

    this.pieChartData = {
      labels: this.queuePieLabels,
      datasets: [
        {
          data,
          backgroundColor: [
            '#198754', // Completed
            '#DC3545FF', // Failed
            '#FFC107', // In Progress
            '#6C757D', // Awaiting
            '#0DCAF0', // Retried
            '#212529', // Cancelled
          ],
          hoverBackgroundColor: [
            '#198754CC', // Completed - 80% opacity
            '#DC3545CC', // Failed
            '#FFC107CC', // In Progress
            '#6C757DCC', // Awaiting
            '#0DCAF0CC', // Retried
            '#212529CC', // Cancelled
          ],
          hoverOffset: 0,
        },
      ],
    };
  }

  applyQueueFilters(): void {
    this.loadQueueReports();
  }

  resetQueueFilters(): void {
    this.queueFilters = {
      status: '',
      fromDate: '',
      toDate: '',
    };
    this.loadQueueReports();
  }

  exportQueueCSV(): void {
    const rows = this.filteredQueueReports.map((q) => ({
      QueueName: q.queueName ?? '',
      Status: q.queueStatus ?? '',
      TagName: q.tagName ?? '',
      EmpId: q.empId ?? '',
      ProductLine: q.productLine ?? '',
      StartedAt: q.createdDate ?? '',
    }));

    const csvContent = [
      Object.keys(rows[0] || {}).join(','),
      ...rows.map((r) => Object.values(r).join(',')),
    ].join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv' });
    const a = document.createElement('a');
    a.href = URL.createObjectURL(blob);
    a.download = 'QueueReports.csv';
    a.click();
  }

  getStatusBadge(status: string | undefined): string {
    const map: Record<string, string> = {
      Completed: 'bg-success',
      Failed: 'bg-danger',
      'In Progress': 'bg-warning text-dark',
      Awaiting: 'bg-secondary',
      Retried: 'bg-info',
      Cancelled: 'bg-dark',
    };
    return 'badge ' + (status ? map[status] ?? 'bg-secondary' : 'bg-secondary');
  }
}

import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgChartsModule } from 'ng2-charts';
import { GridColumn, IQueueInfo, QueueReportFilterRequest } from '@interfaces';
import { QueueService } from '@services';
import { ChartOptions } from 'chart.js';
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

  pieChartOptions: ChartOptions<'pie'> = {
    responsive: true,
    plugins: {
      legend: {
        position: 'right' as const, // 👈 Explicitly cast to correct literal type
      },
    },
  };

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
  queuePieData = [0, 0, 0, 0, 0, 0];

  constructor(private queueService: QueueService) {}

  ngOnInit(): void {
    this.columns = [
      {
        field: 'queueName',
        header: 'Queue Name',
        sortable: true,
      },
      {
        field: 'queueStatus',
        header: 'Status',
        sortable: true,
        cellTemplate: this.statusTemplate,
      },
      {
        field: 'empId',
        header: 'Employee',
        sortable: true,
      },
      {
        field: 'productLine',
        header: 'Product Line',
        sortable: true,
      },
      {
        field: 'libraryName',
        header: 'Library',
        sortable: true,
      },
      {
        field: 'className',
        header: 'Class',
        sortable: true,
      },
      {
        field: 'methodName',
        header: 'Method',
        sortable: true,
      },
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
    // const filter: QueueReportFilterRequest = {
    //   status: this.queueFilters.status,
    //   fromDate: this.queueFilters.fromDate,
    //   toDate: this.queueFilters.toDate,
    //   pageSize: 0,
    //   page: 0,
    // };
    const toNullableString = (value?: string): string | undefined =>
      value?.trim() ? value : undefined;

    const filter: QueueReportFilterRequest = {
      status: this.queueFilters.status,
      fromDate: toNullableString(this.queueFilters.fromDate),
      toDate: toNullableString(this.queueFilters.toDate),
      page: 1,
      pageSize: 0,
    };

    debugger;
    this.queueService.getQueueReports(filter).subscribe((res) => {
      debugger;
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

    this.queuePieData = this.queuePieLabels.map((label) => counts[label]);
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
      //DurationSeconds: q.durationInSeconds ?? '',
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

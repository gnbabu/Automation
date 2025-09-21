import { CommonModule } from '@angular/common';
import {
  Component,
  OnDestroy,
  OnInit,
  TemplateRef,
  ViewChild,
} from '@angular/core';
import { Router } from '@angular/router';
import { GridColumn, IQueueInfo, IQueueSearchPayload } from '@interfaces';
import { AuthService, QueueService } from '@services';
import { DataGridComponent } from 'app/core/components/data-grid/data-grid.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, DataGridComponent],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
})
export class DashboardComponent implements OnInit, OnDestroy {
  columns: GridColumn[] = [];
  pageSize = 10;
  @ViewChild('statusTemplate', { static: true })
  statusTemplate!: TemplateRef<any>;
  @ViewChild('detailsTemplate', { static: true })
  detailsTemplate!: TemplateRef<any>;

  // You can add any logic for the dashboard component here if needed
  constructor(
    public queueService: QueueService,
    private authService: AuthService,
    private router: Router
  ) {}
  stats = [
    { title: 'Total Queues', count: 0, bg: 'bg-secondary' },
    { title: 'In Progress', count: 0, bg: 'bg-warning' },
    { title: 'Completed', count: 0, bg: 'bg-success' },
    { title: 'Failed', count: 0, bg: 'bg-danger' },
    { title: 'Awaiting Execution', count: 0, bg: 'bg-info' },
    { title: 'Retried / Cancelled', count: 0, bg: 'bg-dark' },
  ];
  recentQueues: any[] = [];
  refreshInterval!: ReturnType<typeof setInterval>;
  refreshMessage: string | null = null;

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
        field: 'createdDate',
        header: 'Created',
        sortable: true,
        type: 'datetime',
      },
      {
        field: 'actions',
        header: 'Actions',
        cellTemplate: this.detailsTemplate, // edit button template
      },
    ];

    this.loadQueues();
    this.refreshInterval = setInterval(() => this.loadQueues(), 20000); // Refresh every 20s
  }

  ngOnDestroy(): void {
    clearInterval(this.refreshInterval);
  }

  loadQueues(): void {
    const payload: IQueueSearchPayload = {
      userId: this.authService.getLoggedInUserId(),
      page: 1,
      pageSize: 0,
      sortColumn: 'CreatedDate',
      sortDirection: 'DESC', // Optional: ASC or DESC
    };

    this.queueService.search(payload).subscribe((response) => {
      var data = response.data;
      this.refreshMessage = 'Queue data updated successfully.';
      this.recentQueues = data.slice(0, 10);

      this.stats[0].count = data.length;
      this.stats[1].count = data.filter(
        (q) => q.queueStatus === 'InProgress'
      ).length;
      this.stats[2].count = data.filter(
        (q) => q.queueStatus === 'Completed'
      ).length;
      this.stats[3].count = data.filter(
        (q) => q.queueStatus === 'Failed'
      ).length;
      this.stats[4].count = data.filter((q) => q.queueStatus === 'New').length;
      this.stats[5].count = data.filter((q) =>
        ['Retried', 'Cancelled'].includes(q.queueStatus ?? '')
      ).length;

      setTimeout(() => {
        this.refreshMessage = null;
      }, 2000);
    });
  }

  getBadgeClass(status: string): string {
    return (
      {
        Completed: 'bg-success text-white',
        InProgress: 'bg-warning text-dark',
        Failed: 'bg-danger text-white',
        Awaiting: 'bg-info text-white',
        Cancelled: 'bg-dark text-white',
        Retried: 'bg-dark text-white',
      }[status] || 'bg-secondary text-white'
    );
  }

  viewQueueDetails(queue: IQueueInfo) {
    this.router.navigate(['/queue-manager', queue.queueId, 'details']);
  }
}

import { CommonModule } from '@angular/common';
import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { GridColumn, IQueueInfo, IQueueSearchPayload } from '@interfaces';
import { AuthService, QueueService } from '@services';
import { DataGridComponent } from 'app/core/components/data-grid/data-grid.component';

@Component({
  selector: 'app-queue-manager',
  standalone: true,
  imports: [CommonModule, RouterModule, DataGridComponent],
  templateUrl: './queue-manager.component.html',
  styleUrl: './queue-manager.component.css',
})
export class QueueManagerComponent implements OnInit {
  columns: GridColumn[] = [];
  queues: IQueueInfo[] = [];
  pageSize = 10;

  @ViewChild('statusTemplate', { static: true })
  statusTemplate!: TemplateRef<any>;
  @ViewChild('queueNameTemplate', { static: true })
  queueNameTemplate!: TemplateRef<any>;

  constructor(
    private queueService: QueueService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.columns = [
      {
        field: 'queueName',
        header: 'Queue Name',
        sortable: true,
        cellTemplate: this.queueNameTemplate,
      },
      {
        field: 'queueStatus',
        header: 'Status',
        sortable: true,
        cellTemplate: this.statusTemplate,
      },
      {
        field: 'userName',
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

    this.loadQueues();
  }

  loadQueues(): void {
    const payload: IQueueSearchPayload = {
      userId: this.authService.getLoggedInUserId(),
      page: 1,
      pageSize: 0,
      sortColumn: 'CreatedDate',
      sortDirection: 'DESC', // Optional: ASC or DESC
    };

    this.queueService.search(payload).subscribe({
      next: (response) => {
        this.queues = response.data;
      },
      error: (err) => {
        console.error('Failed to load queues:', err);
      },
    });
  }

  viewQueueDetails(queue: IQueueInfo) {
    this.router.navigate(['/queue-manager', queue.queueId, 'details']);
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

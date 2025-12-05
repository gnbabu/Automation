import { CommonModule } from '@angular/common';
import {
  Component,
  OnDestroy,
  OnInit,
  TemplateRef,
  ViewChild,
} from '@angular/core';
import { Router } from '@angular/router';
import { GridColumn } from '@interfaces';
import { AuthService } from '@services';
import { DataGridComponent } from 'app/core/components/data-grid/data-grid.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
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
  constructor(private authService: AuthService, private router: Router) {}
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

  ngOnInit(): void {}

  ngOnDestroy(): void {}
}

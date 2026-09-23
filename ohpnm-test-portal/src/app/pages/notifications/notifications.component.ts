import { CommonModule } from '@angular/common';
import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { GridColumn, IUserNotification } from '@interfaces';
import { CommonToasterService, NotificationService } from '@services';
import { DataGridComponent } from 'app/core/components/data-grid/data-grid.component';
import { pairBadgeTextColor } from 'app/core/utils/badge-class.util';

@Component({
  standalone: true,
  selector: 'app-notifications',
  imports: [CommonModule, FormsModule, RouterModule, DataGridComponent],
  templateUrl: './notifications.component.html',
  styleUrl: './notifications.component.css',
})
export class NotificationsComponent implements OnInit {
  @ViewChild('titleTemplate', { static: true })
  titleTemplate!: TemplateRef<any>;

  @ViewChild('typeTemplate', { static: true })
  typeTemplate!: TemplateRef<any>;

  @ViewChild('statusTemplate', { static: true })
  statusTemplate!: TemplateRef<any>;

  @ViewChild('dateTemplate', { static: true })
  dateTemplate!: TemplateRef<any>;

  columns: GridColumn[] = [];
  notifications: IUserNotification[] = [];
  loading = false;
  unreadOnly = false;

  constructor(
    private notificationService: NotificationService,
    private router: Router,
    private toaster: CommonToasterService,
  ) {}

  ngOnInit(): void {
    this.setupColumns();
    this.load();
  }

  private setupColumns(): void {
    this.columns = [
      { field: 'notificationType', header: 'Type', sortable: true, cellTemplate: this.typeTemplate },
      { field: 'title', header: 'Notification', sortable: true, cellTemplate: this.titleTemplate },
      { field: 'isRead', header: 'Status', sortable: true, cellTemplate: this.statusTemplate },
      { field: 'createdOn', header: 'Received', sortable: true, cellTemplate: this.dateTemplate },
    ];
  }

  load(): void {
    this.loading = true;
    this.notificationService.getMine(this.unreadOnly).subscribe({
      next: (res) => {
        this.notifications = res || [];
        this.loading = false;
      },
      error: () => {
        this.notifications = [];
        this.loading = false;
      },
    });
  }

  onUnreadOnlyChange(): void {
    this.load();
  }

  open(notification: IUserNotification): void {
    if (!notification.isRead) {
      this.notificationService.markRead(notification.notificationId).subscribe(() => {
        notification.isRead = true;
      });
    }
    if (notification.linkUrl) {
      this.router.navigateByUrl(notification.linkUrl);
    }
  }

  markAllAsRead(): void {
    this.notificationService.markAllRead().subscribe(() => {
      this.notifications = this.notifications.map((n) => ({ ...n, isRead: true }));
      this.toaster.success('All notifications marked as read');
    });
  }

  typePillClass(type: string): string {
    switch ((type || '').toLowerCase()) {
      case 'activatedfortesting':
      case 'releaseapproved':
        return pairBadgeTextColor('bg-success');
      case 'readytoactivate':
        return pairBadgeTextColor('bg-primary');
      case 'releaserejected':
      case 'scheduledrunfailed':
        return pairBadgeTextColor('bg-danger');
      default:
        return pairBadgeTextColor('bg-secondary');
    }
  }
}

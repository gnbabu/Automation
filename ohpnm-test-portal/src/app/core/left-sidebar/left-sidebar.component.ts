import {
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Input,
  OnDestroy,
  OnInit,
  Output,
  Signal,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterModule } from '@angular/router';
import { AuthService, NotificationService } from '@services';
import { IUser, IUserNotification } from '@interfaces';
import { environment } from 'environments/environment';
import { pairBadgeTextColor } from 'app/core/utils/badge-class.util';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-left-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterModule],
  templateUrl: './left-sidebar.component.html',
  styleUrl: './left-sidebar.component.css',
})
export class LeftSidebarComponent implements OnInit, OnDestroy {
  @Output() toggle = new EventEmitter<void>();
  @Input() isMobileOpen = false;
  @Output() navigate = new EventEmitter<void>();
  isAdmin: boolean;
  canAccessManagerFeatures: boolean;
  isViewer: boolean;
  user: IUser | null;
  private userSub?: Subscription;

  unreadCount = 0;
  isNotificationPanelOpen = false;
  recentNotifications: IUserNotification[] = [];
  private readonly UNREAD_POLL_INTERVAL_MS = 30000;
  private unreadPollTimer?: ReturnType<typeof setInterval>;

  constructor(
    private authService: AuthService,
    private notificationService: NotificationService,
    private router: Router,
    private elementRef: ElementRef,
  ) {
    this.isAdmin = this.authService.isAdmin();
    this.canAccessManagerFeatures = this.authService.canAccessManagerFeatures();
    this.isViewer = this.authService.isViewer();
    this.user = this.authService.getLoggedInUser();
  }

  ngOnInit(): void {
    this.userSub = this.authService.currentUser$.subscribe((user) => {
      this.user = user;
      this.isAdmin = this.authService.isAdmin();
      this.canAccessManagerFeatures =
        this.authService.canAccessManagerFeatures();
      this.isViewer = this.authService.isViewer();
    });

    this.refreshUnreadCount();
    this.unreadPollTimer = setInterval(
      () => this.refreshUnreadCount(),
      this.UNREAD_POLL_INTERVAL_MS,
    );
  }

  ngOnDestroy(): void {
    this.userSub?.unsubscribe();
    if (this.unreadPollTimer) clearInterval(this.unreadPollTimer);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (
      this.isNotificationPanelOpen &&
      !this.elementRef.nativeElement.contains(event.target)
    ) {
      this.isNotificationPanelOpen = false;
    }
  }

  refreshUnreadCount(): void {
    this.notificationService.getUnreadCount().subscribe((res) => {
      this.unreadCount = res.count;
    });
  }

  toggleNotificationPanel(): void {
    this.isNotificationPanelOpen = !this.isNotificationPanelOpen;
    if (this.isNotificationPanelOpen) {
      this.notificationService.getMine().subscribe((notifications) => {
        this.recentNotifications = notifications.slice(0, 10);
      });
    }
  }

  onNotificationClick(notification: IUserNotification): void {
    this.isNotificationPanelOpen = false;
    if (!notification.isRead) {
      this.notificationService
        .markRead(notification.notificationId)
        .subscribe(() => {
          this.refreshUnreadCount();
        });
    }
    if (notification.linkUrl) {
      this.router.navigateByUrl(notification.linkUrl);
    }
  }

  markAllAsRead(event: Event): void {
    event.stopPropagation();
    this.notificationService.markAllRead().subscribe(() => {
      this.recentNotifications = this.recentNotifications.map((n) => ({
        ...n,
        isRead: true,
      }));
      this.unreadCount = 0;
    });
  }

  viewAllNotifications(): void {
    this.isNotificationPanelOpen = false;
    this.router.navigateByUrl('/notifications');
  }

  logout() {
    this.authService.logout();
  }

  getProfilePhotoUrl(): string {
    const photo = this.user?.photo;
    if (!photo) {
      return 'assets/images/default-user.png';
    }

    return photo.startsWith('data:image')
      ? photo
      : `data:image/png;base64,${photo}`;
  }

  environmentDisplayName = environment.displayName;

  get environmentBadgeClass(): string {
    switch (environment.environmentName.toLowerCase()) {
      case 'development':
        return pairBadgeTextColor('bg-success'); // green
      case 'qa':
        return pairBadgeTextColor('bg-warning'); // yellow
      case 'production':
        return pairBadgeTextColor('bg-danger'); // red
      default:
        return pairBadgeTextColor('bg-secondary'); // fallback gray
    }
  }
}

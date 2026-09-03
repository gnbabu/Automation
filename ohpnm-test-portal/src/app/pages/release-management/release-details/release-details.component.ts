import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import {
  GridColumn,
  IReleaseModel,
  IReleaseNotification,
  IReleaseReadiness,
  IReleaseSignOff,
  IReleaseSignOffRequest,
} from '@interfaces';
import {
  AuthService,
  CommonToasterService,
  ReleaseService,
} from '@services';
import { DataGridComponent } from 'app/core/components/data-grid/data-grid.component';
import { pairBadgeTextColor } from 'app/core/utils/badge-class.util';

@Component({
  standalone: true,
  selector: 'app-release-details',
  imports: [CommonModule, FormsModule, RouterModule, DataGridComponent],
  templateUrl: './release-details.component.html',
  styleUrl: './release-details.component.css',
})
export class ReleaseDetailsComponent implements OnInit, OnDestroy {
  @ViewChild('signOffStatusTemplate', { static: true })
  signOffStatusTemplate!: TemplateRef<any>;

  @ViewChild('notificationStatusTemplate', { static: true })
  notificationStatusTemplate!: TemplateRef<any>;

  @ViewChild('dateTemplate', { static: true })
  dateTemplate!: TemplateRef<any>;

  signOffColumns: GridColumn[] = [];
  notificationColumns: GridColumn[] = [];

  releaseId!: number;
  release: IReleaseModel | null = null;
  readiness: IReleaseReadiness | null = null;
  signOffHistory: IReleaseSignOff[] = [];
  notifications: IReleaseNotification[] = [];

  activating = false;
  signingOff = false;
  refreshingReadiness = false;
  signOffComments = '';

  // Auto-refresh: keeps DLL readiness / test summary / lifecycle live without
  // requiring a manual "Refresh" click (mirrors test-case-execution-panel's pattern).
  private refreshInterval: any = null;
  private readonly refreshSeconds = 10;
  isUserPerformingAction = false;
  lastUpdated: Date | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private releaseService: ReleaseService,
    private authService: AuthService,
    private toaster: CommonToasterService,
  ) {}

  ngOnInit(): void {
    this.releaseId = +(this.route.snapshot.paramMap.get('id') ?? 0);
    this.setupColumns();
    this.load();
    this.startAutoRefresh();
  }

  private setupColumns(): void {
    this.signOffColumns = [
      { field: 'signOffStatus', header: 'Status', sortable: true, cellTemplate: this.signOffStatusTemplate },
      { field: 'signOffBy', header: 'By', sortable: true },
      { field: 'signOffOn', header: 'On', sortable: true, cellTemplate: this.dateTemplate },
      { field: 'comments', header: 'Comments', sortable: false },
    ];

    this.notificationColumns = [
      { field: 'notificationType', header: 'Type', sortable: true },
      { field: 'recipientEmail', header: 'Recipient', sortable: true },
      { field: 'status', header: 'Status', sortable: true, cellTemplate: this.notificationStatusTemplate },
      { field: 'createdOn', header: 'Created', sortable: true, cellTemplate: this.dateTemplate },
    ];
  }

  // Same red/green pairing convention as sign-off - "Sent"/"Delivered" success-ish,
  // "Failed" danger, anything else neutral.
  notificationStatusPillClass(status?: string): string {
    switch ((status || '').toLowerCase()) {
      case 'sent':
      case 'delivered':
        return pairBadgeTextColor('bg-success');
      case 'failed':
        return pairBadgeTextColor('bg-danger');
      default:
        return pairBadgeTextColor('bg-secondary');
    }
  }

  ngOnDestroy(): void {
    this.stopAutoRefresh();
  }

  startAutoRefresh(): void {
    this.refreshInterval = setInterval(() => {
      if (this.isUserPerformingAction) {
        return;
      }
      this.silentRefresh();
    }, this.refreshSeconds * 1000);
  }

  stopAutoRefresh(): void {
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval);
      this.refreshInterval = null;
    }
  }

  // Silent background refresh: always refresh the release (lifecycle/test
  // summary/sign-off status), but only re-run the reflection-heavy readiness
  // check while the release is still Draft (once activated, DLL readiness no
  // longer gates anything, so skip the extra folder scan).
  private silentRefresh(): void {
    this.releaseService.getById(this.releaseId).subscribe({
      next: (r) => {
        this.release = r;
        this.lastUpdated = new Date();

        if ((r.releaseLifecycle || '').toLowerCase() === 'draft') {
          this.releaseService.getReadiness(this.releaseId).subscribe({
            next: (ready) => (this.readiness = ready),
          });
        }
      },
    });
  }

  load(): void {
    this.releaseService.getById(this.releaseId).subscribe({
      next: (r) => {
        this.release = r;
        // Only run the reflection-heavy readiness scan while still Draft, same guard
        // silentRefresh() already uses - once Active/Completed/Rejected, DLL readiness
        // no longer gates anything, so re-scanning the folder on every load() call
        // (including right after activate()) was both wasted work and why the
        // "Release Readiness" card kept showing "READY FOR ACTIVATION" forever, with no
        // awareness that the release had already moved past Draft.
        if ((r.releaseLifecycle || '').toLowerCase() !== 'draft') {
          this.readiness = null;
        } else {
          this.refreshReadiness();
        }
      },
      error: () => (this.release = null),
    });
    this.releaseService.getSignOffHistory(this.releaseId).subscribe({
      next: (h) => (this.signOffHistory = h || []),
    });
    this.releaseService.getNotifications(this.releaseId).subscribe({
      next: (n) => (this.notifications = n || []),
    });
  }

  refreshReadiness(): void {
    this.refreshingReadiness = true;
    this.releaseService.getReadiness(this.releaseId).subscribe({
      next: (r) => {
        this.readiness = r;
        this.refreshingReadiness = false;
      },
      error: () => (this.refreshingReadiness = false),
    });
  }

  get isDraft(): boolean {
    return (this.release?.releaseLifecycle || '').toLowerCase() === 'draft';
  }

  get canActivate(): boolean {
    if (!this.release) return false;
    const lc = (this.release.releaseLifecycle || '').toLowerCase();
    return lc !== 'active' && lc !== 'completed' && !!this.readiness?.isReady;
  }

  get testingComplete(): boolean {
    if (!this.release) return false;
    return this.release.totalTests > 0 && this.release.runningTests === 0;
  }

  get canSignOff(): boolean {
    if (!this.release) return false;
    const lc = (this.release.releaseLifecycle || '').toLowerCase();
    return lc === 'active' && this.testingComplete;
  }

  activate(): void {
    if (!this.release) return;
    this.activating = true;
    this.isUserPerformingAction = true;
    const user = this.authService.getLoggedInUser();
    this.releaseService
      .activate(this.releaseId, { activatedBy: user?.userName ?? 'system' })
      .subscribe({
        next: (res) => {
          this.activating = false;
          this.isUserPerformingAction = false;
          const n = res?.notification;
          this.toaster.success(
            `Release activated.` +
              (n ? ` Notified ${n.sent}/${n.recipients} managers.` : ''),
          );
          this.load();
        },
        error: () => {
          this.activating = false;
          this.isUserPerformingAction = false;
        },
      });
  }

  signOff(status: 'Approved' | 'Rejected'): void {
    if (!this.release) return;
    if (status === 'Rejected' && !this.signOffComments.trim()) {
      this.toaster.error('Please provide comments when rejecting.');
      return;
    }
    const user = this.authService.getLoggedInUser();
    const request: IReleaseSignOffRequest = {
      signOffStatus: status,
      signOffBy: user?.userName ?? 'system',
      comments: this.signOffComments,
    };
    this.signingOff = true;
    this.isUserPerformingAction = true;
    this.releaseService.signOff(this.releaseId, request).subscribe({
      next: () => {
        this.signingOff = false;
        this.isUserPerformingAction = false;
        this.signOffComments = '';
        this.toaster.success(`Release ${status.toLowerCase()}.`);
        this.load();
      },
      error: () => {
        this.signingOff = false;
        this.isUserPerformingAction = false;
      },
    });
  }

  back(): void {
    this.router.navigate(['/release-management']);
  }

  // Exposed to the template so inline badges can pair a bg-* class with legible text
  // color without duplicating the switch-based helpers below.
  pairBadgeTextColor = pairBadgeTextColor;

  // Same convention as release-management.component.ts's statusPillClass/signOffPillClass,
  // so lifecycle/sign-off badges match the color used everywhere else in the app.
  lifecyclePillClass(lifecycle?: string): string {
    switch ((lifecycle || '').toLowerCase()) {
      case 'active':
        return pairBadgeTextColor('bg-success');
      case 'completed':
        return pairBadgeTextColor('bg-primary');
      case 'rejected':
        return pairBadgeTextColor('bg-danger');
      case 'draft':
        return pairBadgeTextColor('bg-secondary');
      default:
        return pairBadgeTextColor('bg-info');
    }
  }

  signOffPillClass(status?: string): string {
    switch ((status || '').toLowerCase()) {
      case 'approved':
        return pairBadgeTextColor('bg-success');
      case 'rejected':
        return pairBadgeTextColor('bg-danger');
      default:
        return pairBadgeTextColor('bg-secondary');
    }
  }
}

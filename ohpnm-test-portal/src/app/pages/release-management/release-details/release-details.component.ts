import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import {
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

@Component({
  standalone: true,
  selector: 'app-release-details',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './release-details.component.html',
  styleUrl: './release-details.component.css',
})
export class ReleaseDetailsComponent implements OnInit, OnDestroy {
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
    this.load();
    this.startAutoRefresh();
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
      next: (r) => (this.release = r),
      error: () => (this.release = null),
    });
    this.releaseService.getSignOffHistory(this.releaseId).subscribe({
      next: (h) => (this.signOffHistory = h || []),
    });
    this.releaseService.getNotifications(this.releaseId).subscribe({
      next: (n) => (this.notifications = n || []),
    });
    this.refreshReadiness();
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
}

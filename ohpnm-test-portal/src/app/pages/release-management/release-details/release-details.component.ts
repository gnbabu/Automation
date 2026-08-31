import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
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
export class ReleaseDetailsComponent implements OnInit {
  releaseId!: number;
  release: IReleaseModel | null = null;
  readiness: IReleaseReadiness | null = null;
  signOffHistory: IReleaseSignOff[] = [];
  notifications: IReleaseNotification[] = [];

  activating = false;
  signingOff = false;
  refreshingReadiness = false;
  signOffComments = '';

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
    const user = this.authService.getLoggedInUser();
    this.releaseService
      .activate(this.releaseId, { activatedBy: user?.userName ?? 'system' })
      .subscribe({
        next: (res) => {
          this.activating = false;
          const n = res?.notification;
          this.toaster.success(
            `Release activated.` +
              (n ? ` Notified ${n.sent}/${n.recipients} managers.` : ''),
          );
          this.load();
        },
        error: () => {
          this.activating = false;
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
    this.releaseService.signOff(this.releaseId, request).subscribe({
      next: () => {
        this.signingOff = false;
        this.signOffComments = '';
        this.toaster.success(`Release ${status.toLowerCase()}.`);
        this.load();
      },
      error: () => {
        this.signingOff = false;
      },
    });
  }

  back(): void {
    this.router.navigate(['/release-management']);
  }
}

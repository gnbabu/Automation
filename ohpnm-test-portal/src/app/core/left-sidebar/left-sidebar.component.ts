import { Component, EventEmitter, OnDestroy, OnInit, Output, Signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterModule } from '@angular/router';
import { AuthService } from '@services';
import { IUser } from '@interfaces';
import { environment } from 'environments/environment';
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
  isAdmin: boolean;
  canAccessManagerFeatures: boolean;
  isViewer: boolean;
  user: IUser | null;
  private userSub?: Subscription;

  constructor(private authService: AuthService) {
    this.isAdmin = this.authService.isAdmin();
    this.canAccessManagerFeatures = this.authService.canAccessManagerFeatures();
    this.isViewer = this.authService.isViewer();
    this.user = this.authService.getLoggedInUser();
  }

  ngOnInit(): void {
    // Lives outside <router-outlet> (see LayoutComponent) so it's created once for the
    // whole session - subscribing to currentUser$ (rather than only reading it once above)
    // is what lets it pick up changes made elsewhere, e.g. Settings' "Edit Profile" save,
    // without needing a page reload.
    this.userSub = this.authService.currentUser$.subscribe((user) => {
      this.user = user;
      this.isAdmin = this.authService.isAdmin();
      this.canAccessManagerFeatures = this.authService.canAccessManagerFeatures();
      this.isViewer = this.authService.isViewer();
    });
  }

  ngOnDestroy(): void {
    this.userSub?.unsubscribe();
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
        return 'bg-success'; // green
      case 'qa':
        return 'bg-warning text-dark'; // yellow
      case 'production':
        return 'bg-danger'; // red
      default:
        return 'bg-secondary'; // fallback gray
    }
  }
}

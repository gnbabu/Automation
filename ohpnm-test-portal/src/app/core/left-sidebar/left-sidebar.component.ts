import { Component, EventEmitter, Output, Signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterModule } from '@angular/router';
import { AuthService } from '@services';
import { IUser } from '@interfaces';
import { environment } from 'environments/environment';

@Component({
  selector: 'app-left-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterModule],
  templateUrl: './left-sidebar.component.html',
  styleUrl: './left-sidebar.component.css',
})
export class LeftSidebarComponent {
  @Output() toggle = new EventEmitter<void>();
  isAdmin: boolean;
  user: IUser | null;
  constructor(private authService: AuthService) {
    this.isAdmin = this.authService.isAdmin();
    this.user = this.authService.getLoggedInUser();
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

import { Component, EventEmitter, Output, Signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterModule } from '@angular/router';
import { AuthService } from '@services';

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
  constructor(private authService: AuthService) {
    this.isAdmin = this.authService.isAdmin();
  }
  logout() {
    this.authService.logout();
  }
}

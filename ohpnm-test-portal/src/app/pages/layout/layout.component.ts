import { Component } from '@angular/core';
import { LeftSidebarComponent } from '../../core/left-sidebar/left-sidebar.component';
import { CommonModule } from '@angular/common';
import { NavigationStart, Router, RouterOutlet } from '@angular/router';
import { LoadingOverlayComponent } from 'app/core/components/loader/loading-overlay.component';
import { ConfirmDialogComponent } from 'app/core/modals/confirm-dialog/confirm-dialog.component';
import { MobileHeaderComponent } from 'app/core/mobile-header/mobile-header.component';
import { filter } from 'rxjs';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    LeftSidebarComponent,
    CommonModule,
    RouterOutlet,
    LoadingOverlayComponent,
    ConfirmDialogComponent,
    MobileHeaderComponent,
  ],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.css',
})
export class LayoutComponent {
  isCollapsed = false;
  // Off-canvas drawer state, mobile-only (<768px - see styles.css) - the hamburger in
  // MobileHeaderComponent opens it; LeftSidebarComponent's (navigate) fires on any nav
  // click/its own close button, and the overlay's click, both close it here.
  isMobileMenuOpen = false;

  constructor(router: Router) {
    // Belt-and-suspenders close on route change (e.g. browser back/forward, or a
    // programmatic navigate() elsewhere) in addition to the explicit (navigate) event -
    // avoids the drawer being left open over a new page in any path that doesn't go
    // through a plain sidebar link click.
    router.events.pipe(filter((e) => e instanceof NavigationStart)).subscribe(() => {
      this.isMobileMenuOpen = false;
    });
  }

  toggleSidebar() {
    this.isCollapsed = !this.isCollapsed;
  }

  toggleMobileMenu(): void {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen = false;
  }
}

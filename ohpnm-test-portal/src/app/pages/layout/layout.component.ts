import { Component } from '@angular/core';
import { LeftSidebarComponent } from '../../core/left-sidebar/left-sidebar.component';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { LoadingOverlayComponent } from 'app/core/components/loader/loading-overlay.component';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    LeftSidebarComponent,
    CommonModule,
    RouterOutlet,
    LoadingOverlayComponent,
  ],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.css',
})
export class LayoutComponent {
  isCollapsed = false;

  toggleSidebar() {
    this.isCollapsed = !this.isCollapsed;
  }
}

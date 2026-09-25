import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Output } from '@angular/core';

// Fixed top bar shown only below the tablet breakpoint (<768px, see .mobile-header's own
// d-md-none in the template) - the desktop/tablet sidebar experience (including the
// existing expand/collapse toggle) is untouched; this is purely the mobile entry point
// for opening the off-canvas nav (see LayoutComponent/LeftSidebarComponent).
@Component({
  selector: 'app-mobile-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './mobile-header.component.html',
  styleUrl: './mobile-header.component.css',
})
export class MobileHeaderComponent {
  @Output() menuToggle = new EventEmitter<void>();
}

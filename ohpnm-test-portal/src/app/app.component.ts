import { Component } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { RouterOutlet } from '@angular/router';
import { environment } from '../environments/environment';
import { LeftSidebarComponent } from './core/left-sidebar/left-sidebar.component';
import { MainComponent } from './core/main/main.component';
import { CommonModule } from '@angular/common';
import { LoadingOverlayComponent } from './core/components/loader/loading-overlay.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent {
  constructor(title: Title) {
    title.setTitle(environment.appTitle);
  }
}

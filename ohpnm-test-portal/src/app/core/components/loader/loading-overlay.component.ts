import { Component } from '@angular/core';
import { NgIf, AsyncPipe } from '@angular/common';
import { LoadingService } from '@services';

@Component({
  selector: 'app-loading-overlay',
  standalone: true,
  imports: [NgIf, AsyncPipe],
  templateUrl: './loading-overlay.component.html',
  styleUrls: ['./loading-overlay.component.css'],
})
export class LoadingOverlayComponent {
  constructor(public loader: LoadingService) {}
}

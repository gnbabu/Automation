import { Component, Input, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ITestScreenshot } from '@interfaces';

declare var bootstrap: any;

@Component({
  selector: 'app-test-screenshot-gallery',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './test-screenshot-gallery.component.html',
  styleUrls: ['./test-screenshot-gallery.component.css'],
})
export class TestScreenshotGalleryComponent {
  @Input() screenshots: ITestScreenshot[] = [];

  @ViewChild('galleryModal', { static: false }) galleryModal!: ElementRef;
  modalInstance: any;

  open() {
    if (!this.galleryModal) return;

    const modalEl = this.galleryModal.nativeElement;
    this.modalInstance = new bootstrap.Modal(modalEl);
    this.modalInstance.show();
  }

  close() {
    if (this.modalInstance) {
      this.modalInstance.hide();
    }
  }
}

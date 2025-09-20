import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ITestScreenshot } from '@interfaces';

declare var bootstrap: any; // Bootstrap JS modal

// Angular-side model matching your C# TestScreenshot

@Component({
  selector: 'app-test-screenshot-gallery',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './test-screenshot-gallery.component.html',
  styleUrl: './test-screenshot-gallery.component.css',
})
export class TestScreenshotGalleryComponent {
  @Input() screenshots: ITestScreenshot[] = [];

  selectedImage: ITestScreenshot | null = null;
  modalInstance: any;

  openModal(img: ITestScreenshot) {
    this.selectedImage = img;
    const modalElement = document.getElementById('imageModal');
    this.modalInstance = new bootstrap.Modal(modalElement);
    this.modalInstance.show();
  }

  closeModal() {
    if (this.modalInstance) {
      this.modalInstance.hide();
    }
  }
}

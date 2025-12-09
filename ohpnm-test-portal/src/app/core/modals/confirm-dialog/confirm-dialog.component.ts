import { Component, ElementRef, ViewChild } from '@angular/core';
import { ConfirmService } from '@services';
import { Subscription } from 'rxjs';
declare var bootstrap: any;

@Component({
  selector: 'app-confirm-dialog',
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.css',
})
export class ConfirmDialogComponent {
  @ViewChild('globalConfirmModal') modalElement!: ElementRef;

  title: string = '';
  message: string = '';

  private modalInstance: any;
  private subscription!: Subscription;

  constructor(private confirmService: ConfirmService) {}

  ngAfterViewInit() {
    this.modalInstance = new bootstrap.Modal(this.modalElement.nativeElement);

    // Ensure NO duplicate subscriptions
    this.subscription = this.confirmService
      .onConfirmState()
      .subscribe((data) => {
        this.title = data.title;
        this.message = data.message;
        this.modalInstance.show();
      });
  }

  confirm() {
    this.confirmService.resolve(true);
    this.modalInstance.hide();
  }

  cancel() {
    this.confirmService.resolve(false);
    this.modalInstance.hide();
  }

  ngOnDestroy() {
    this.subscription?.unsubscribe(); // <--- FIX: removes duplicates
  }
}

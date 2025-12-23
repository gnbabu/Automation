import { Component, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ITestCaseExecutionLog } from '@interfaces';
import { ExecutionLogsViewerComponent } from 'app/common-components/execution-logs-viewer/execution-logs-viewer.component';

declare var bootstrap: any;

@Component({
  selector: 'app-execution-logs-dialog',
  standalone: true,
  imports: [CommonModule, ExecutionLogsViewerComponent],
  templateUrl: './execution-logs-dialog.component.html',
  styleUrls: ['./execution-logs-dialog.component.css'],
})
export class ExecutionLogsDialogComponent {
  @ViewChild('executionLogsModal', { static: false })
  modalElement!: ElementRef;

  logs: ITestCaseExecutionLog[] = [];
  private modalInstance: any;

  /** Open modal with logs */
  open(logs: ITestCaseExecutionLog[]): void {
    this.logs = logs ?? [];

    if (!this.modalInstance) {
      this.modalInstance = new bootstrap.Modal(
        this.modalElement.nativeElement,
        {
          backdrop: 'static',
          keyboard: false,
        }
      );
    }

    this.modalInstance.show();
  }

  /** Close modal */
  close(): void {
    if (this.modalInstance) {
      this.modalInstance.hide();
    }
  }
}

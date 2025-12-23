import { Component, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ModalService } from '@services';
import { ITestCaseExecutionLog } from '@interfaces';
import { ExecutionLogsViewerComponent } from 'app/common-components/execution-logs-viewer/execution-logs-viewer.component';

@Component({
  selector: 'app-execution-logs-dialog',
  standalone: true,
  imports: [CommonModule, ExecutionLogsViewerComponent],
  templateUrl: './execution-logs-dialog.component.html',
  styleUrls: ['./execution-logs-dialog.component.css'],
})
export class ExecutionLogsDialogComponent implements AfterViewInit {
  @ViewChild('executionLogsModal') modalElement!: ElementRef;

  logs: ITestCaseExecutionLog[] = [];

  constructor(private modalService: ModalService) {}

  ngAfterViewInit(): void {
    setTimeout(() => {
      this.modalService.register(
        'executionLogsModal',
        this.modalElement.nativeElement
      );
    });
  }

  /** Open modal with logs */
  open(logs: ITestCaseExecutionLog[]): void {
    this.logs = logs ?? [];
    this.modalService.open('executionLogsModal');
  }

  /** Close modal */
  close(): void {
    this.modalService.close('executionLogsModal');
  }
}

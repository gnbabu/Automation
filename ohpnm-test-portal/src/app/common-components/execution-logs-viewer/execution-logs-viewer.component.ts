import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { ITestCaseExecutionLog } from '@interfaces';

@Component({
  selector: 'app-execution-logs-viewer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './execution-logs-viewer.component.html',
  styleUrls: ['./execution-logs-viewer.component.css'],
})
export class ExecutionLogsViewerComponent {
  @Input() logs: ITestCaseExecutionLog[] = [];

  getIcon(level: string): string {
    switch (level) {
      case 'Pass':
        return 'bi bi-check-circle-fill text-success';
      case 'Fail':
        return 'bi bi-x-circle-fill text-danger';
      case 'Warning':
        return 'bi bi-exclamation-triangle-fill text-warning';
      default:
        return 'bi bi-info-circle-fill text-secondary';
    }
  }
  trackByLogId(_: number, log: any): number {
    return log.logId;
  }
}

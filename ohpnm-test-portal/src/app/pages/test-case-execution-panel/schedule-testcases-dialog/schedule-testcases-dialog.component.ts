import {
  Component,
  ElementRef,
  EventEmitter,
  Output,
  ViewChild,
  AfterViewInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ModalService } from '@services';

@Component({
  selector: 'app-schedule-testcases-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  styleUrl: './schedule-testcases-dialog.component.css',
  templateUrl: './schedule-testcases-dialog.component.html',
})
export class ScheduleTestcasesDialogComponent implements AfterViewInit {
  @ViewChild('scheduleModal') modalElement!: ElementRef;

  browser: string = 'Chrome';
  date: string = '';
  time: string = '';

  private callback!: (data: any) => void;

  constructor(private modalService: ModalService) {}

  ngAfterViewInit() {
    this.modalService.register(
      'scheduleTestcasesModal',
      this.modalElement.nativeElement
    );
  }

  /** Open modal and pass callback */
  open(cb: (data: any) => void) {
    this.callback = cb;
    this.modalService.open('scheduleTestcasesModal');
  }

  /** Close modal */
  close() {
    this.modalService.close('scheduleTestcasesModal');
  }

  /** Submit form back to parent */
  submit() {
    if (this.callback) {
      this.callback({
        browser: this.browser,
        date: this.date,
        time: this.time,
      });
    }
    this.close();
  }
}

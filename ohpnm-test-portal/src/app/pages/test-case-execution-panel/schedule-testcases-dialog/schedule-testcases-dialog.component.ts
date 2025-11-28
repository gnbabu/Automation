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
  @ViewChild('scheduleModal') modalRef!: ElementRef;

  @Output() submitSchedule = new EventEmitter<{
    browser: string;
    date: string;
    time: string;
  }>();

  browser: string = 'Chrome';
  date: string = '';
  time: string = '';

  modalId = 'scheduleTestcasesModal';

  constructor(private modalService: ModalService) {}

  ngAfterViewInit(): void {
    this.modalService.register(this.modalId, this.modalRef.nativeElement);
  }

  open() {
    this.modalService.open(this.modalId);
  }

  close() {
    this.modalService.close(this.modalId);
  }

  submit() {
    this.submitSchedule.emit({
      browser: this.browser,
      date: this.date,
      time: this.time,
    });

    this.close();
  }
}

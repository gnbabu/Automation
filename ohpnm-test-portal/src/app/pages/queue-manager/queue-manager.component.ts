import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { Route, Router, RouterLink, RouterModule } from '@angular/router';
import { IQueueInfo } from '@interfaces';
import { QueueService } from '@services';

@Component({
  selector: 'app-queue-manager',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './queue-manager.component.html',
  styleUrl: './queue-manager.component.css',
})
export class QueueManagerComponent implements OnInit {
  queues: IQueueInfo[] = [];

  constructor(private queueService: QueueService, private router: Router) {}

  ngOnInit(): void {
    this.queueService.getAllQueues().subscribe({
      next: (data) => {
        this.queues = data;
      },
      error: (err) => {
        console.error('Failed to load queues:', err);
      },
    });
  }

  viewQueueDetails(queue: IQueueInfo) {
    this.router.navigate(['/queue-manager', queue.queueId, 'details']);
  }
}

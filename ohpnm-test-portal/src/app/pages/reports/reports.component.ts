import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.css']  
})
export class ReportsComponent {

    reports = [
        { name: 'Sales Report', date: '2025-04-25', status: 'Completed', generatedBy: 'Admin' },
        { name: 'User Activity', date: '2025-04-24', status: 'In Progress', generatedBy: 'John Doe' },
        { name: 'Monthly Overview', date: '2025-04-20', status: 'Failed', generatedBy: 'System' }
      ];
    
      getStatusClass(status: string): string {
        switch (status) {
          case 'Completed': return 'bg-success text-white';
          case 'In Progress': return 'bg-warning text-dark';
          case 'Failed': return 'bg-danger text-white';
          default: return 'bg-secondary text-white';
        }
      }
}
import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AppMultiselectDropdownComponent } from 'app/core/components/app-multiselect-dropdown/app-multiselect-dropdown.component';

@Component({
  selector: 'app-test-case-assignment',
  imports: [AppMultiselectDropdownComponent, CommonModule, FormsModule],
  templateUrl: './test-case-assignment.component.html',
  styleUrl: './test-case-assignment.component.css',
})
export class TestCaseAssignmentComponent {
  selectedUsers: any[] = [];
  users = [
    { id: 1, name: 'John Doe', email: 'john@example.com' },
    { id: 2, name: 'Jane Smith', email: 'jane@example.com' },
    { id: 3, name: 'Robert Brown', email: 'robert@example.com' },
    { id: 4, name: 'Alice Johnson', email: 'alice@example.com' },
  ];

  onUsersChange(selected: any[]) {
    console.log('Users selected:', selected);
    // Call your API here if needed
  }

  onSelectAll(selected: any[]) {
    console.log('Select All clicked:', selected);
  }

  onClearAll() {
    console.log('Clear clicked');
  }
}

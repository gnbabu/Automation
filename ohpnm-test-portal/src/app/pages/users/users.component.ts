import { Component } from '@angular/core';
import { UserListComponent } from './user-list.component';
import { IUser, IUserFilter } from '@interfaces';
import { CommonModule } from '@angular/common';
import { AddEditUserComponent } from './add-edit-user.component';
import { Mappers } from '@mappers';
import { UsersService } from '@services';
import { FormsModule } from '@angular/forms';
import { AppDropdownComponent } from 'app/core/components/app-dropdown/app-dropdown.component';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [
    UserListComponent,
    AddEditUserComponent,
    CommonModule,
    FormsModule,
    AppDropdownComponent,
  ],
  templateUrl: './users.component.html',
  styleUrl: './users.component.css',
})
export class UsersComponent {
  selectedUser: IUser | null = null;
  filteredUsers: IUser[] = [];
  filters: IUserFilter = {
    search: '',
    status: 0,
    role: 0,
    priority: 0,
  };

  statusFilterOptions = [
    { value: 0, label: 'All Status' },
    { value: 1, label: 'Active' },
    { value: 2, label: 'Suspended' },
    { value: 3, label: 'Pending' },
  ];
  roleFilterOptions = [
    { value: 0, label: 'All Roles' },
    { value: 1, label: 'Admin' },
    { value: 2, label: 'Tester' },
    { value: 3, label: 'Manager' },
    { value: 4, label: 'Viewer' },
  ];
  priorityFilterOptions = [
    { value: 0, label: 'All Tiers' },
    { value: 1, label: 'Tier 1' },
    { value: 2, label: 'Tier 2' },
    { value: 3, label: 'Tier 3' },
  ];

  showForm = false;

  constructor(private usersService: UsersService) {}

  onAddUser() {
    this.selectedUser = Mappers.UserMapper.empty();
    this.showForm = true;
  }

  onEditUser(user: IUser) {
    this.selectedUser = { ...user };
    this.showForm = true;
  }

  onFormSaved() {
    this.showForm = false;
    this.selectedUser = null;
  }

  onCancel() {
    this.showForm = false;
    this.selectedUser = null;
  }

  onFilterChange() {
    const safeFilters: IUserFilter = this.filters ?? {
      search: '',
      status: 0,
      role: 0,
      priority: 0,
    };

    this.usersService.getFilteredUsers(safeFilters).subscribe({
      next: (users) => {
        this.filteredUsers = users.map(Mappers.UserMapper.fromApi);
        this.showForm = false;
        this.selectedUser = null;
      },
      error: (err) => {
        console.error('Error fetching filtered users:', err);
      },
    });
  }
}

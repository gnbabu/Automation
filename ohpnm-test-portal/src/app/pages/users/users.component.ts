import { Component } from '@angular/core';
import { UserListComponent } from './user-list.component';
import { IUser } from '@interfaces';
import { CommonModule } from '@angular/common';
import { AddEditUserComponent } from './add-edit-user.component';
import { Mappers } from '@mappers';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [UserListComponent, AddEditUserComponent, CommonModule],
  templateUrl: './users.component.html',
  styleUrl: './users.component.css',
})
export class UsersComponent {
  selectedUser: IUser | null = null;
  showForm = false;

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
}

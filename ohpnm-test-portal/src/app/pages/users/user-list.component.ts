import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IUser } from '@interfaces';
import { UsersService } from '@services';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './user-list.component.html',
})
export class UserListComponent {
  @Output() editUser = new EventEmitter<IUser>();
  users: IUser[] = [];

  constructor(private usersService: UsersService) {
    this.loadUsers();
  }

  loadUsers() {
    this.usersService.getAll().subscribe((data) => {
      this.users = data;
    });
  }

  toggleActive(user: IUser) {
    let request = {
      userId: user.userId,
      active: !user.active,
    };
    this.usersService.activate(request).subscribe((data) => {
      this.loadUsers();
    });
  }

  editUserClicked(user: IUser) {
    this.editUser.emit(user);
  }
}

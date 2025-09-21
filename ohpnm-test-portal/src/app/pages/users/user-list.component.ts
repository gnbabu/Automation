import {
  Component,
  EventEmitter,
  OnInit,
  Output,
  TemplateRef,
  ViewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { GridColumn, IUser } from '@interfaces';
import { UsersService } from '@services';
import { DataGridComponent } from 'app/core/components/data-grid/data-grid.component';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, DataGridComponent],
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.css'],
})
export class UserListComponent implements OnInit {
  @Output() editUser = new EventEmitter<IUser>();
  users: IUser[] = [];
  columns: GridColumn[] = [];
  pageSize = 10;

  @ViewChild('nameTemplate', { static: true })
  nameTemplate!: TemplateRef<any>;

  @ViewChild('activeTemplate', { static: true })
  activeTemplate!: TemplateRef<any>;

  @ViewChild('actionsTemplate', { static: true })
  actionsTemplate!: TemplateRef<any>;

  constructor(private usersService: UsersService) {}

  ngOnInit(): void {
    this.columns = [
      {
        field: 'userName',
        header: 'Username',
        sortable: true,
      },
      {
        field: 'email',
        header: 'Email',
        sortable: true,
      },
      {
        field: 'roleName',
        header: 'Role',
        sortable: true,
      },
      {
        field: 'fullName',
        header: 'Name',
        sortable: true,
        cellTemplate: this.nameTemplate, // custom template (firstName + lastName)
      },
      {
        field: 'active',
        header: 'Active',
        sortable: false,
        cellTemplate: this.activeTemplate, // toggle switch template
      },
      {
        field: 'actions',
        header: 'Actions',
        cellTemplate: this.actionsTemplate, // edit button template
      },
    ];

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

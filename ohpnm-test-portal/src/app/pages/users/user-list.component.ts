import {
  Component,
  EventEmitter,
  Input,
  OnInit,
  Output,
  TemplateRef,
  ViewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { GridColumn, IUser } from '@interfaces';
import { UsersService, CommonToasterService } from '@services';
import { DataGridComponent } from 'app/core/components/data-grid/data-grid.component';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, DataGridComponent],
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.css'],
})
export class UserListComponent implements OnInit {
  @Input() users: IUser[] = [];
  @Output() editUser = new EventEmitter<IUser>();
  columns: GridColumn[] = [];
  pageSize = 10;

  @ViewChild('nameTemplate', { static: true })
  nameTemplate!: TemplateRef<any>;

  @ViewChild('statusTemplate', { static: true })
  statusTemplate!: TemplateRef<any>;

  @ViewChild('actionsTemplate', { static: true })
  actionsTemplate!: TemplateRef<any>;

  @ViewChild('priorityTemplate', { static: true })
  priorityTemplate!: TemplateRef<any>;

  @ViewChild('lastLoginTemplate', { static: true })
  lastLoginTemplate!: TemplateRef<any>;

  constructor(
    private usersService: UsersService,
    private toaster: CommonToasterService
  ) {}

  ngOnInit(): void {
    this.columns = [
      {
        field: 'userId',
        header: 'ID',
        sortable: true,
      },
      {
        field: 'fullName',
        header: 'Name & Email',
        sortable: true,
        cellTemplate: this.nameTemplate,
      },
      {
        field: 'roleName',
        header: 'Role',
        sortable: true,
      },
      {
        field: 'priorityName',
        header: 'Priority',
        sortable: true,
        cellTemplate: this.priorityTemplate,
      },
      {
        field: 'statusName',
        header: 'Status',
        sortable: false,
        cellTemplate: this.statusTemplate,
      },
      {
        field: 'lastLogin',
        header: 'Last Login',
        sortable: true,
        cellTemplate: this.lastLoginTemplate,
      },
      {
        field: 'actions',
        header: 'Actions',
        cellTemplate: this.actionsTemplate,
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

  deleteUserClicked(userId: number) {
    this.usersService.delete(userId).subscribe(() => {
      this.toaster.success('User deleted successfully');
      this.loadUsers();
    });
  }
}

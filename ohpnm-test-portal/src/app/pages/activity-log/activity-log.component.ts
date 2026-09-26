import { CommonModule } from '@angular/common';
import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { GridColumn, IAuditLogEntry, IUser } from '@interfaces';
import { AuditLogService, UsersService } from '@services';
import { DataGridComponent } from 'app/core/components/data-grid/data-grid.component';
import { AppDropdownComponent } from 'app/core/components/app-dropdown/app-dropdown.component';

// Fixed, known set of Action values actually written by the backend today (see
// AGENTS.md's Audit Log section) - kept as a small hard-coded list rather than a
// distinct-values-seen-so-far endpoint (unlike Entity Type below) since a brand new
// app install has logged nothing yet and would otherwise show an empty, useless filter.
const KNOWN_ACTIONS = [
  'Created', 'Updated', 'Deleted', 'Enabled', 'Disabled', 'Paused', 'Resumed', 'PasswordChanged',
];

@Component({
  standalone: true,
  selector: 'app-activity-log',
  imports: [CommonModule, FormsModule, DataGridComponent, AppDropdownComponent],
  templateUrl: './activity-log.component.html',
  styleUrl: './activity-log.component.css',
})
export class ActivityLogComponent implements OnInit {
  @ViewChild('actorTemplate', { static: true }) actorTemplate!: TemplateRef<any>;
  @ViewChild('actionTemplate', { static: true }) actionTemplate!: TemplateRef<any>;
  @ViewChild('entityTemplate', { static: true }) entityTemplate!: TemplateRef<any>;
  @ViewChild('detailsTemplate', { static: true }) detailsTemplate!: TemplateRef<any>;
  @ViewChild('whenTemplate', { static: true }) whenTemplate!: TemplateRef<any>;

  columns: GridColumn[] = [];
  entries: IAuditLogEntry[] = [];
  loading = false;

  entityTypes: string[] = [];
  actions = KNOWN_ACTIONS;
  users: IUser[] = [];

  filters: {
    entityType?: string;
    action?: string;
    actorUserId?: number;
    fromDate?: string;
    toDate?: string;
  } = {};

  // Row currently expanded to show its full Details diff - only one at a time, matching
  // the compact "click to expand" pattern used elsewhere (e.g. execution logs).
  expandedRowId: number | null = null;

  // Entity Type / Action filter options are plain strings, not objects.
  identityTextAccessor = (opt: string) => opt;
  userTextAccessor = (u: IUser) => u.userName;

  constructor(
    private auditLogService: AuditLogService,
    private usersService: UsersService,
  ) {}

  ngOnInit(): void {
    this.setupColumns();
    this.load();
    this.auditLogService.getEntityTypes().subscribe((types) => (this.entityTypes = types || []));
    this.usersService.getAll().subscribe((users) => (this.users = users || []));
  }

  private setupColumns(): void {
    this.columns = [
      { field: 'createdOn', header: 'When', sortable: true, cellTemplate: this.whenTemplate },
      { field: 'actorUserName', header: 'Actor', sortable: true, cellTemplate: this.actorTemplate },
      { field: 'action', header: 'Action', sortable: true, cellTemplate: this.actionTemplate },
      { field: 'entityType', header: 'Entity Type', sortable: true },
      { field: 'entityName', header: 'Entity', sortable: true, cellTemplate: this.entityTemplate },
      { field: 'details', header: 'Details', cellTemplate: this.detailsTemplate },
    ];
  }

  load(): void {
    this.loading = true;
    this.auditLogService
      .getPaged({
        entityType: this.filters.entityType,
        action: this.filters.action,
        actorUserId: this.filters.actorUserId,
        fromDate: this.filters.fromDate,
        toDate: this.filters.toDate,
        pageNumber: 1,
        // Fetches one large batch and lets DataGridComponent paginate client-side -
        // matches every other grid in the app (none actually wire up its
        // fetchServerData/server paging mode, even though the component supports it).
        pageSize: 500,
      })
      .subscribe({
        next: (res) => {
          this.entries = res?.items || [];
          this.loading = false;
        },
        error: () => {
          this.entries = [];
          this.loading = false;
        },
      });
  }

  clearFilters(): void {
    this.filters = {};
    this.load();
  }

  toggleExpand(row: IAuditLogEntry): void {
    this.expandedRowId = this.expandedRowId === row.auditLogId ? null : row.auditLogId;
  }

  actionBadgeClass(action: string): string {
    switch ((action || '').toLowerCase()) {
      case 'created':
      case 'enabled':
      case 'resumed':
        return 'bg-success';
      case 'deleted':
      case 'disabled':
      case 'paused':
        return 'bg-danger';
      case 'updated':
      case 'passwordchanged':
        return 'bg-primary';
      default:
        return 'bg-secondary';
    }
  }

  // Renders a parsed Details JSON blob as a short, readable list rather than raw JSON -
  // either a changes[] diff (Update actions) or a plain snapshot object (Create/Delete).
  parsedDetails(details?: string): { label: string; value: string }[] {
    if (!details) return [];
    try {
      const parsed = JSON.parse(details);
      if (Array.isArray(parsed?.changes)) {
        return parsed.changes.map((c: any) => ({
          label: c.field,
          value: `${c.old ?? '—'} → ${c.new ?? '—'}`,
        }));
      }
      return Object.entries(parsed).map(([key, value]) => ({ label: key, value: String(value ?? '—') }));
    } catch {
      return [];
    }
  }
}

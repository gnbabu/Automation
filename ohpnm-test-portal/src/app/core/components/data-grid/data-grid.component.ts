import {
  Component,
  Input,
  TemplateRef,
  ViewChildren,
  QueryList,
  AfterViewInit,
  OnInit,
  OnChanges,
  SimpleChanges,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { GridColumn } from '@interfaces';

@Component({
  selector: 'app-data-grid',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './data-grid.component.html',
  styleUrls: ['./data-grid.component.css'],
})
export class DataGridComponent implements AfterViewInit, OnInit, OnChanges {
  @Input() columns: GridColumn[] = [];
  @Input() data: any[] = [];
  @Input() pagingEnabled: boolean = true;
  @Input() pagingMode: 'client' | 'server' = 'client';
  @Input() sortingMode: 'client' | 'server' = 'client';
  @Input() noDataMessage: string = 'No records found';

  @Input() pageSize = 5;
  @Input() totalRecords = 0;

  @Input() fetchServerData?: (
    page: number,
    size: number,
    sort?: string,
    dir?: 'asc' | 'desc'
  ) => void;

  currentPage = 1;
  sortField = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  templates: { [key: string]: TemplateRef<any> } = {};

  @ViewChildren(TemplateRef) templateRefs!: QueryList<TemplateRef<any>>;

  ngAfterViewInit() {
    // Map template reference names to TemplateRef instances (optional)
    this.templateRefs.forEach((templateRef: TemplateRef<any>) => {
      const element = (templateRef as any)?._declarationTContainer
        ?.localNames?.[0];
      if (element) {
        this.templates[element] = templateRef;
      }
    });
  }

  ngOnInit() {
    this.pageSize = Number(this.pageSize) || 5;
    if (this.pagingMode === 'server' || this.sortingMode === 'server') {
      this.loadServerData();
    }
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['pageSize'] && !changes['pageSize'].firstChange) {
      this.pageSize = Number(this.pageSize) || 1;
      this.currentPage = 1;

      if (this.pagingMode === 'server') {
        this.loadServerData();
      } else {
        this.clampCurrentPage();
      }
    }

    if (changes['data'] && !changes['data'].firstChange) {
      this.clampCurrentPage();
    }
  }

  get pagedData(): any[] {
    if (!this.pagingEnabled) {
      return this.sortingMode === 'client' ? this.sortedData : this.data;
    }

    if (this.pagingMode === 'server') return this.data;

    const sorted = this.sortedData;
    const start = (this.currentPage - 1) * this.pageSize;
    return sorted.slice(start, start + this.pageSize);
  }

  get sortedData(): any[] {
    if (this.sortingMode === 'server' || !this.sortField) return this.data;
    return [...this.data].sort((a, b) => {
      const valA = a[this.sortField];
      const valB = b[this.sortField];
      const factor = this.sortDirection === 'asc' ? 1 : -1;
      if (valA == null && valB == null) return 0;
      if (valA == null) return -factor;
      if (valB == null) return factor;
      return valA > valB ? factor : valA < valB ? -factor : 0;
    });
  }

  changePage(page: number) {
    const total = this.totalPages();
    if (page < 1 || page > total) return;
    this.currentPage = page;
    if (this.pagingMode === 'server') {
      this.loadServerData();
    }
  }

  changePageSize(size: number) {
    const newSize = Number(size);
    if (isNaN(newSize) || newSize <= 0) return;
    if (newSize === this.pageSize) return;
    this.pageSize = newSize;
    this.currentPage = 1;
    if (this.pagingMode === 'server') {
      this.loadServerData();
    }
  }

  changeSort(field: string) {
    if (this.sortField === field) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortField = field;
      this.sortDirection = 'asc';
    }
    if (this.sortingMode === 'server') {
      this.loadServerData();
    } else {
      this.currentPage = 1;
    }
  }

  loadServerData() {
    if (this.fetchServerData) {
      this.fetchServerData(
        this.currentPage,
        this.pageSize,
        this.sortField,
        this.sortDirection
      );
    }
  }

  totalPages(): number {
    if (!this.pagingEnabled) return 1;
    const total =
      this.pagingMode === 'server' ? this.totalRecords : this.data.length;
    return Math.max(1, Math.ceil(total / this.pageSize));
  }

  private clampCurrentPage() {
    const total = this.totalPages();
    if (this.currentPage < 1) this.currentPage = 1;
    else if (this.currentPage > total) this.currentPage = total;
  }
}

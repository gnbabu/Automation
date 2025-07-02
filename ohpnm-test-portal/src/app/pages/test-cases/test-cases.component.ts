import { Component, OnInit, TemplateRef, ViewChild } from '@angular/core';
import { GridColumn, ITestResult, TestResultPayload } from '@interfaces';
import { DataGridComponent } from 'app/core/components/data-grid/data-grid.component';
import { TestResultService } from '@services';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-test-cases',
  standalone: true,
  imports: [DataGridComponent, CommonModule, FormsModule],
  templateUrl: './test-cases.component.html',
  styleUrls: ['./test-cases.component.css'],
})
export class TestCasesComponent implements OnInit {
  constructor(private testResultService: TestResultService) {}

  @ViewChild('statusTemplate', { static: true })
  statusTemplate!: TemplateRef<any>;

  columns: GridColumn[] = [];

  testResults: ITestResult[] = [];
  totalRecords = 0;
  pageSize = 10;

  ngOnInit(): void {
    this.columns = [
      {
        field: 'name',
        header: 'Name',
        sortable: true,
      },
      {
        field: 'resultStatus',
        header: 'Status',
        sortable: true,
        cellTemplate: this.statusTemplate,
      },
      { field: 'duration', header: 'Duration', sortable: true },
      {
        field: 'startTime',
        header: 'Start Time',
        sortable: true,
        type: 'datetime',
      },
      {
        field: 'endTime',
        header: 'End Time',
        sortable: true,
        type: 'datetime',
      },
    ];
  }

  fetchServerData = (
    page: number,
    pageSize: number,
    sortColumn?: string,
    sortDirection?: 'asc' | 'desc'
  ) => {
    const payload: TestResultPayload = {
      page,
      pageSize,
      sortColumn,
      sortDirection,
    };

    this.testResultService.search(payload).subscribe((res) => {
      this.testResults = res.data;
      this.totalRecords = res.totalCount;
    });
  };
}

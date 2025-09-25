import { Injectable } from '@angular/core';
import {
  IQueueInfo,
  ITestCaseAssignment,
  ITestResult,
  ITestScreenshot,
  PagedResult,
  TestResultPayload,
} from '@interfaces';
import { Mappers } from '@mappers';
import { HttpService } from '@services';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class TestCaseManagerService {
  constructor(private httpService: HttpService) {}

  getAssignmentsByUserId(userId: any): Observable<ITestCaseAssignment[]> {
    return this.httpService.get<ITestCaseAssignment[]>(
      `TestCaseAssignments/${userId}`
    );
  }

  saveAssignments(assignments: ITestCaseAssignment[]): Observable<any> {
    return this.httpService.post(
      `TestCaseAssignments/bulk-insert`,
      assignments
    );
  }

  deleteAssignments(userId: any): Observable<any> {
    return this.httpService.delete<any>(`TestCaseAssignments/${userId}`);
  }
}

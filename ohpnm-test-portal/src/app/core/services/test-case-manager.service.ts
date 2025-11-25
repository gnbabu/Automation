import { Injectable } from '@angular/core';
import {
  IAssignedTestCase,
  IAssignmentCreateUpdateRequest,
  IQueueInfo,
  ITestCaseAssignment,
  ITestCaseAssignmentDeleteRequest,
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

  saveAssignmentsOld(assignments: ITestCaseAssignment[]): Observable<any> {
    return this.httpService.post(
      `TestCaseAssignments/bulk-insert-old`,
      assignments
    );
  }

  deleteAssignments(userId: any): Observable<any> {
    return this.httpService.delete<any>(`TestCaseAssignments/${userId}`);
  }

  deleteAssignmentsByCriteria(
    request: ITestCaseAssignmentDeleteRequest
  ): Observable<any> {
    return this.httpService.post(
      `TestCaseAssignments/delete-assignments`,
      request
    );
  }
  //________________________

  saveAssignmentNew(request: IAssignmentCreateUpdateRequest): Observable<any> {
    return this.httpService.post(
      `TestCaseAssignments/create-or-update`,
      request
    );
  }

  getTestCasesByAssignmentAndUser(
    assignedUserId: number,
    assignmentName: string
  ): Observable<IAssignedTestCase[]> {
    return this.httpService.get<IAssignedTestCase[]>(
      `TestCaseAssignments/${assignedUserId}/${encodeURIComponent(
        assignmentName
      )}`
    );
  }
}

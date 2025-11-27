import { Injectable } from '@angular/core';
import {
  IAssignedTestCase,
  IAssignmentCreateUpdateRequest,
  ITestCaseAssignment,
} from '@interfaces';
import { HttpService } from '@services';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class TestCaseAssignmentService {
  constructor(private httpService: HttpService) {}

  getAssignmentsByUserId(userId: any): Observable<ITestCaseAssignment[]> {
    return this.httpService.get<ITestCaseAssignment[]>(
      `TestCaseAssignments/${userId}`
    );
  }

  saveAssignment(request: IAssignmentCreateUpdateRequest): Observable<any> {
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

  getAllAssignedTestCasesInLibrary(
    libraryName: string
  ): Observable<IAssignedTestCase[]> {
    return this.httpService.get<IAssignedTestCase[]>(
      `TestCaseAssignments/library-assigned-testcases?libraryName=${encodeURIComponent(
        libraryName
      )}`
    );
  }

  getAssignedTestCasesForLibraryAndEnvironment(
    libraryName: string,
    environment: string
  ): Observable<IAssignedTestCase[]> {
    return this.httpService.get<IAssignedTestCase[]>(
      `TestCaseAssignments/library-environment-assigned-testcases?libraryName=${encodeURIComponent(
        libraryName
      )}&environment=${encodeURIComponent(environment)}`
    );
  }
}

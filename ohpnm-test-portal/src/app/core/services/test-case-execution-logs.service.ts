import { Injectable } from '@angular/core';
import { HttpService } from '@services';
import { ITestCaseExecutionLog } from '@interfaces';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class TestCaseExecutionLogsService {
  constructor(private httpService: HttpService) {}

  addLog(log: ITestCaseExecutionLog): Observable<void> {
    return this.httpService.post<void>(`TestCaseExecutionLogs`, log);
  }

  getTestCaseLogs(
    assignmentId: number,
    assignmentTestCaseId: number
  ): Observable<ITestCaseExecutionLog[]> {
    return this.httpService.get<any[]>(
      `TestCaseExecutionLogs/assignments/${assignmentId}/testcases/${assignmentTestCaseId}/logs`
    );
  }

  getAssignmentLogs(assignmentId: number): Observable<ITestCaseExecutionLog[]> {
    return this.httpService.get<any[]>(
      `TestCaseExecutionLogs/assignments/${assignmentId}/logs`
    );
  }

  getReleaseLogs(releaseName: string): Observable<ITestCaseExecutionLog[]> {
    return this.httpService.get<any[]>(
      `TestCaseExecutionLogs/releases/${encodeURIComponent(releaseName)}/logs`
    );
  }
}

import { Injectable } from '@angular/core';
import {
  IQueueInfo,
  ITestResult,
  PagedResult,
  TestResultPayload,
} from '@interfaces';
import { Mappers } from '@mappers';
import { HttpService } from '@services';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class TestResultService {
  constructor(private httpService: HttpService) {}

  search(payload: TestResultPayload): Observable<PagedResult<ITestResult>> {
    return this.httpService.post<PagedResult<ITestResult>>(
      'TestResults/search',
      payload
    );
  }

  getTestResultsByQueueId(queueId: any): Observable<ITestResult[]> {
    return this.httpService.get<ITestResult[]>(`TestResults/${queueId}`);
  }
}

import { Injectable } from '@angular/core';
import {
  IQueueInfo,
  ITestResult,
  PagedResult,
  TestResultPayload,
} from '@interfaces';

import { HttpService } from '@services';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class TestResultService {
  constructor(private httpService: HttpService) {}

  getTestResultsByQueueId(queueId: any): Observable<ITestResult[]> {
    return this.httpService.get<ITestResult[]>(`TestResults/${queueId}`);
  }
}

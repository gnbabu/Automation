import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import {
  ISingleRunNowRequest,
  IBulkRunNowRequest,
  ISingleScheduleRequest,
  IBulkScheduleRequest,
  IQueueCreateResponse,
} from '@interfaces';
import { HttpService } from './http.service';

@Injectable({
  providedIn: 'root',
})
export class TestCaseExecutionService {
  constructor(private httpService: HttpService) {}

  singleRunNow(
    payload: ISingleRunNowRequest
  ): Observable<IQueueCreateResponse> {
    return this.httpService.post<IQueueCreateResponse>(
      `TestCaseExecutionQueue/single-run`,
      payload
    );
  }

  bulkRunNow(payload: IBulkRunNowRequest): Observable<{ success: boolean }> {
    return this.httpService.post<{ success: boolean }>(
      `TestCaseExecutionQueue/bulk-run`,
      payload
    );
  }

  singleSchedule(
    payload: ISingleScheduleRequest
  ): Observable<IQueueCreateResponse> {
    return this.httpService.post<IQueueCreateResponse>(
      `TestCaseExecutionQueue/single-schedule`,
      payload
    );
  }

  bulkSchedule(
    payload: IBulkScheduleRequest
  ): Observable<{ success: boolean }> {
    return this.httpService.post<{ success: boolean }>(
      `TestCaseExecutionQueue/bulk-schedule`,
      payload
    );
  }
}

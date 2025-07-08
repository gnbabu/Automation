import { Injectable, signal } from '@angular/core';
import {
  IQueueInfo,
  IQueueSearchPayload,
  PagedResult,
  IQueueReportFilterRequest,
} from '@interfaces';
import { Mappers } from '@mappers';
import { HttpService } from '@services';
import { Observable } from 'rxjs';
import { environment } from 'environments/environment';

@Injectable({
  providedIn: 'root',
})
export class QueueService {
  AppTitle = signal<string>(environment.displayName);

  constructor(private httpService: HttpService) {}

  search(payload: IQueueSearchPayload): Observable<PagedResult<IQueueInfo>> {
    return this.httpService.post<PagedResult<IQueueInfo>>(
      'Queue/search',
      payload,
      undefined,
      Mappers.QueueInfoMapper.mapPagedResult
    );
  }

  getQueueById(queueId: string): Observable<IQueueInfo> {
    return this.httpService.get<IQueueInfo>(
      `Queue/${queueId}`,
      {},
      Mappers.QueueInfoMapper.fromApi
    );
  }

  getQueueReports(
    payload: IQueueReportFilterRequest
  ): Observable<PagedResult<IQueueInfo>> {
    return this.httpService.post<PagedResult<IQueueInfo>>(
      'Queue/queue-reports',
      payload
    );
  }
}

import { Injectable } from '@angular/core';
import { IQueueInfo, PagedResult, QueueReportFilterRequest } from '@interfaces';
import { Mappers } from '@mappers';
import { HttpService } from '@services';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class QueueService {
  constructor(private httpService: HttpService) {}

  getAllQueues(): Observable<IQueueInfo[]> {
    return this.httpService.get<IQueueInfo[]>(`Queue/all`, {}, (res: any[]) =>
      res.map(Mappers.QueueInfoMapper.fromApi)
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
    payload: QueueReportFilterRequest
  ): Observable<PagedResult<IQueueInfo>> {
    return this.httpService.post<PagedResult<IQueueInfo>>(
      'Queue/queue-reports',
      payload
    );
  }
}

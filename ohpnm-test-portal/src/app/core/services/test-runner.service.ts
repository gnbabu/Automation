import { Injectable } from '@angular/core';
import { IQueueInfo } from '@interfaces';
import { Mappers } from '@mappers';
import { HttpService } from '@services';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class TestRunnerService {
  constructor(private httpService: HttpService) {}

  runTest(queue: IQueueInfo): Observable<string> {
    return this.httpService.post(
      `TestRunner/run-test`,
      queue,
      Mappers.QueueInfoMapper.toApi
    );
  }
}

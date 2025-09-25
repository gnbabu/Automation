import { Injectable } from '@angular/core';
import {
  IQueueInfo,
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
export class ScreenshotService {
  constructor(private httpService: HttpService) {}

  getScreenshotsByTestResultIdAsync(
    queueId: any,
    methodName: any
  ): Observable<ITestScreenshot[]> {
    return this.httpService.get<ITestScreenshot[]>(
      `TestScreenshots/screenshots?queueId=${queueId}&methodName=${methodName}`
    );
  }
}

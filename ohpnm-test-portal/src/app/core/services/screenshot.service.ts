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
//https://localhost:7147/api/TestScreenshots/screenshots?queueId=7cc3873d-f7cf-474c-bb7a-f02c9db40601&methodName=RegisterEmployee_ValidDocuments_ReturnsTrue

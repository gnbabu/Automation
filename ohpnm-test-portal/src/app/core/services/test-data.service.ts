import { Injectable } from '@angular/core';
import {
  ITestDataCopyAllRequest,
  ITestDataCopySource,
  ITestDataDocument,
  ITestDataFlowSchema,
  ITestDataSectionSaveRequest,
  ITestDataTestCase,
} from '@interfaces';
import { HttpService } from './http.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class TestDataService {
  constructor(private httpService: HttpService) {}

  getMyTestCases(environmentId: number): Observable<ITestDataTestCase[]> {
    return this.httpService.get<ITestDataTestCase[]>(`TestData/my-testcases`, { environmentId });
  }

  getSchema(assignmentTestCaseId: number): Observable<ITestDataFlowSchema | null> {
    return this.httpService.get<ITestDataFlowSchema | null>(`TestData/schema`, { assignmentTestCaseId });
  }

  getData(
    assignmentTestCaseId: number,
    environmentId: number,
    from?: ITestDataCopySource,
  ): Observable<ITestDataDocument> {
    const params: Record<string, string | number> = { assignmentTestCaseId, environmentId };
    if (from) {
      params['fromLibrary'] = from.libraryName;
      params['fromClass'] = from.className;
      params['fromMethod'] = from.methodName;
    }
    return this.httpService.get<ITestDataDocument>(`TestData/data`, params);
  }

  getCopySources(assignmentTestCaseId: number, environmentId: number): Observable<ITestDataCopySource[]> {
    return this.httpService.get<ITestDataCopySource[]>(`TestData/copy-sources`, {
      assignmentTestCaseId,
      environmentId,
    });
  }

  copyAll(request: ITestDataCopyAllRequest): Observable<{ copiedSections: string[] }> {
    return this.httpService.post<{ copiedSections: string[] }>(`TestData/copy-all`, request);
  }

  saveSection(request: ITestDataSectionSaveRequest): Observable<any> {
    return this.httpService.put(`TestData/section`, request, undefined);
  }
}

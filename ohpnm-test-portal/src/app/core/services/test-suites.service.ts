import { Injectable } from '@angular/core';
import { HttpService } from '@services';
import { ITestCaseModel, LibraryInfo } from '@interfaces';
import { Observable } from 'rxjs';
import { Mappers } from '@mappers';

@Injectable({
  providedIn: 'root',
})
export class TestSuitesService {
  constructor(private httpService: HttpService) {}

  getLibraries(): Observable<LibraryInfo[]> {
    return this.httpService.get<any[]>(
      'TestSuites/libraries',
      {},
      (res: any[]) => res.map(Mappers.LibraryInfoMapper.fromApi)
    );
  }

  getAllTestCases(
    libraryName?: string,
    assigned?: boolean
  ): Observable<ITestCaseModel[]> {
    let url = 'TestSuites/GetAllTestCases';

    const query: string[] = [];
    if (libraryName)
      query.push(`libraryName=${encodeURIComponent(libraryName)}`);
    if (assigned !== undefined) query.push(`assigned=${assigned}`);

    if (query.length) {
      url += '?' + query.join('&');
    }

    return this.httpService.get<ITestCaseModel[]>(url);
  }
}

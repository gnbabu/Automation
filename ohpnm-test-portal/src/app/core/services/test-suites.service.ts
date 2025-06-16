import { Injectable } from '@angular/core';
import { HttpService } from '@services';
import { LibraryInfo } from '@interfaces';
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
}

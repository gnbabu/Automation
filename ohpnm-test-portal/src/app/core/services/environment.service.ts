import { Injectable } from '@angular/core';
import {
  IAssignedTestCase,
  IAssignmentCreateUpdateRequest,
  IEnvironmentModel,
  IEnvironmentRequestDto,
  ITestCaseAssignmentEntity,
} from '@interfaces';
import { HttpService } from '@services';
import { map, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class EnvironmentService {
  constructor(private httpService: HttpService) {}

  getAll(): Observable<IEnvironmentModel[]> {
    return this.httpService.get<IEnvironmentModel[]>(`Environment`);
  }

  getById(id: number): Observable<IEnvironmentModel> {
    return this.httpService.get<IEnvironmentModel>(`Environment/${id}`);
  }

  create(request: IEnvironmentRequestDto): Observable<void> {
    return this.httpService
      .post<{ environmentId: number }>(`Environment`, request)
      .pipe(map(() => void 0));
  }

  update(request: IEnvironmentRequestDto): Observable<void> {
    return this.httpService.put<void>(`Environment`, request);
  }

  softDelete(id: number): Observable<void> {
    return this.httpService.delete<void>(`Environment/${id}/soft`);
  }

  hardDelete(id: number): Observable<void> {
    return this.httpService.delete<void>(`Environment/${id}/hard`);
  }
}

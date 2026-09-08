import { Injectable } from '@angular/core';
import { ILoginUserModel, ILoginUserRequestDto } from '@interfaces';
import { HttpService } from '@services';
import { map, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class LoginUserService {
  constructor(private httpService: HttpService) {}

  getByEnvironment(environmentId: number): Observable<ILoginUserModel[]> {
    return this.httpService.get<ILoginUserModel[]>(
      `LoginUser/environment/${environmentId}`
    );
  }

  create(request: ILoginUserRequestDto): Observable<void> {
    return this.httpService
      .post<{ loginUserId: number }>(`LoginUser`, request)
      .pipe(map(() => void 0));
  }

  update(request: ILoginUserRequestDto): Observable<void> {
    return this.httpService.put<void>(`LoginUser`, request);
  }

  softDelete(id: number): Observable<void> {
    return this.httpService.delete<void>(`LoginUser/${id}/soft`);
  }

  hardDelete(id: number): Observable<void> {
    return this.httpService.delete<void>(`LoginUser/${id}/hard`);
  }
}

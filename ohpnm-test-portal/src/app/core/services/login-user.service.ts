import { Injectable } from '@angular/core';
import { ILoginUserModel, ILoginUserRequestDto } from '@interfaces';
import { HttpService } from '@services';
import { map, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class LoginUserService {
  constructor(private httpService: HttpService) {}

  // Unfiltered - every login user for the environment, any owner. No longer called by
  // any part of the Portal's UI after the self-service change (kept, unused, rather
  // than removed - see AGENTS.md).
  getByEnvironment(environmentId: number): Observable<ILoginUserModel[]> {
    return this.httpService.get<ILoginUserModel[]>(
      `LoginUser/environment/${environmentId}`
    );
  }

  // Self-service: only the caller's own login user(s) for this environment. Used by
  // Credential Configuration and by Run Now/Schedule's dropdown resolution.
  getMineForEnvironment(environmentId: number): Observable<ILoginUserModel[]> {
    return this.httpService.get<ILoginUserModel[]>(
      `LoginUser/environment/${environmentId}/mine`
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

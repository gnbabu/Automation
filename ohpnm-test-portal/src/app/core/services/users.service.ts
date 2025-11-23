import { Injectable } from '@angular/core';
import { HttpService } from './http.service';
import { Observable } from 'rxjs';
import {
  IChangePasswordRequest,
  IPriorityStatus,
  ITimeZone,
  IUser,
  IUserFilter,
  IUserRole,
  IUserStatus,
} from '@interfaces';
import { Mappers } from '@mappers';

@Injectable({
  providedIn: 'root',
})
export class UsersService {
  constructor(private httpService: HttpService) {}

  getAll(): Observable<IUser[]> {
    return this.httpService.get<IUser[]>(`Users`, {}, (res: any[]) =>
      res.map(Mappers.UserMapper.fromApi)
    );
  }

  getFilteredUsers(filters: IUserFilter): Observable<IUser[]> {
    return this.httpService.post<IUser[]>('Users/Filters', filters);
  }

  getUserById(userId: number): Observable<IUser> {
    return this.httpService.get<IUser>(
      `Users/${userId}`,
      {},
      Mappers.UserMapper.fromApi
    );
  }

  create(user: IUser): Observable<any> {
    return this.httpService.post(`Users`, user, Mappers.UserMapper.toApi);
  }

  update(user: IUser): Observable<any> {
    return this.httpService.put(`Users`, user, Mappers.UserMapper.toApi);
  }

  getUserRoles(): Observable<IUserRole[]> {
    return this.httpService.get<IUserRole[]>(`Users/roles`, {}, (res: any[]) =>
      res.map(Mappers.UserRoleMapper.fromApi)
    );
  }

  getUserStatuses(): Observable<IUserStatus[]> {
    return this.httpService.get<IUserStatus[]>(
      `Users/status`,
      {},
      (res: any[]) => res.map(Mappers.UserStatusMapper.fromApi)
    );
  }

  getTimeZones(): Observable<ITimeZone[]> {
    return this.httpService.get<ITimeZone[]>(
      `Users/timezones`,
      {},
      (res: any[]) => res.map(Mappers.TimeZoneMapper.fromApi)
    );
  }

  getPriorityStatuses(): Observable<IPriorityStatus[]> {
    return this.httpService.get<IPriorityStatus[]>(
      `Users/priorities`,
      {},
      (res: any[]) => res.map(Mappers.PriorityStatusMapper.fromApi)
    );
  }

  activate(request: any): Observable<any> {
    return this.httpService.post(`Users/activate`, request);
  }

  changePassword(request: IChangePasswordRequest): Observable<any> {
    return this.httpService.post(`Users/change-password`, request);
  }

  delete(userId: number): Observable<any> {
    return this.httpService.delete(`Users/${userId}`);
  }
}

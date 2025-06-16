import { Injectable } from '@angular/core';
import { HttpService } from './http.service';
import { Observable } from 'rxjs';
import { IChangePasswordRequest, IUser, IUserRole } from '@interfaces';
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

  activate(request: any): Observable<any> {
    return this.httpService.post(`Users/activate`, request);
  }

  changePassword(request: IChangePasswordRequest): Observable<any> {
    return this.httpService.post(`Users/change-password`, request);
  }
}

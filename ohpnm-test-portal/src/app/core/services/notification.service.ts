import { Injectable } from '@angular/core';
import { IUserNotification } from '@interfaces';
import { HttpService } from '@services';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  constructor(private httpService: HttpService) {}

  getMine(unreadOnly = false): Observable<IUserNotification[]> {
    return this.httpService.get<IUserNotification[]>('Notification', { unreadOnly });
  }

  getUnreadCount(): Observable<{ count: number }> {
    return this.httpService.get<{ count: number }>('Notification/unread-count');
  }

  markRead(id: number): Observable<any> {
    return this.httpService.post<any>(`Notification/${id}/mark-read`, {});
  }

  markAllRead(): Observable<any> {
    return this.httpService.post<any>('Notification/mark-all-read', {});
  }
}

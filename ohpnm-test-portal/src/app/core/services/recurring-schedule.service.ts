import { Injectable } from '@angular/core';
import {
  IAssignmentOption,
  IRecurringSchedule,
  IRecurringScheduleRequest,
} from '@interfaces';
import { HttpService } from '@services';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class RecurringScheduleService {
  constructor(private httpService: HttpService) {}

  getAll(): Observable<IRecurringSchedule[]> {
    return this.httpService.get<IRecurringSchedule[]>('RecurringSchedule');
  }

  getAssignmentOptions(): Observable<IAssignmentOption[]> {
    return this.httpService.get<IAssignmentOption[]>('RecurringSchedule/assignments');
  }

  create(request: IRecurringScheduleRequest): Observable<{ recurringScheduleId: number }> {
    return this.httpService.post<{ recurringScheduleId: number }>('RecurringSchedule', request);
  }

  pause(id: number): Observable<any> {
    return this.httpService.post<any>(`RecurringSchedule/${id}/pause`, {});
  }

  resume(id: number): Observable<any> {
    return this.httpService.post<any>(`RecurringSchedule/${id}/resume`, {});
  }

  delete(id: number): Observable<any> {
    return this.httpService.delete<any>(`RecurringSchedule/${id}`);
  }
}

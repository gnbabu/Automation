import { Injectable } from '@angular/core';
import { IExecutionTrendPoint } from '@interfaces';
import { Observable } from 'rxjs';
import { HttpService } from './http.service';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  constructor(private httpService: HttpService) {}

  getExecutionTrend(releaseId: number, days: number): Observable<IExecutionTrendPoint[]> {
    return this.httpService.get<IExecutionTrendPoint[]>(`Dashboard/execution-trend`, { releaseId, days });
  }
}

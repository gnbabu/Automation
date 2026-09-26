import { Injectable } from '@angular/core';
import { IAuditLogFilter, IAuditLogPagedResult } from '@interfaces';
import { HttpService } from '@services';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuditLogService {
  constructor(private httpService: HttpService) {}

  getPaged(filter: IAuditLogFilter): Observable<IAuditLogPagedResult> {
    // Strips undefined keys before building HttpParams - HttpParams.fromObject would
    // otherwise stringify an unset filter as the literal text "undefined" and send it
    // as a real query value instead of omitting it.
    const params: Record<string, string | number> = {};
    Object.entries(filter).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        params[key] = value;
      }
    });
    return this.httpService.get<IAuditLogPagedResult>('AuditLog', params);
  }

  getEntityTypes(): Observable<string[]> {
    return this.httpService.get<string[]>('AuditLog/entity-types');
  }
}

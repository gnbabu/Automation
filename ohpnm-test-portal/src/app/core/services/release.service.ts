import { Injectable } from '@angular/core';
import {
  IReleaseActivateRequest,
  IReleaseModel,
  IReleaseNotification,
  IReleaseReadiness,
  IReleaseRequestDto,
  IReleaseSignOff,
  IReleaseSignOffRequest,
} from '@interfaces';
import { HttpService } from '@services';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ReleaseService {
  constructor(private httpService: HttpService) {}

  getAll(): Observable<IReleaseModel[]> {
    return this.httpService.get<IReleaseModel[]>(`Release`);
  }

  getById(id: number): Observable<IReleaseModel> {
    return this.httpService.get<IReleaseModel>(`Release/${id}`);
  }

  create(request: IReleaseRequestDto): Observable<IReleaseModel> {
    return this.httpService.post<IReleaseModel>(`Release`, request);
  }

  update(id: number, request: IReleaseRequestDto): Observable<IReleaseModel> {
    return this.httpService.put<IReleaseModel>(`Release/${id}`, request);
  }

  activate(id: number, request: IReleaseActivateRequest): Observable<any> {
    return this.httpService.post<any>(`Release/${id}/activate`, request);
  }

  signOff(id: number, request: IReleaseSignOffRequest): Observable<IReleaseModel> {
    return this.httpService.post<IReleaseModel>(`Release/${id}/signoff`, request);
  }

  getSignOffHistory(id: number): Observable<IReleaseSignOff[]> {
    return this.httpService.get<IReleaseSignOff[]>(`Release/${id}/signoff-history`);
  }

  getNotifications(id: number): Observable<IReleaseNotification[]> {
    return this.httpService.get<IReleaseNotification[]>(`Release/${id}/notifications`);
  }

  // Read-only readiness check (DLLs are placed by the existing controlled
  // build/deployment process; this reuses the existing discovery mechanism).
  getReadiness(id: number): Observable<IReleaseReadiness> {
    return this.httpService.get<IReleaseReadiness>(`Release/${id}/readiness`);
  }

  // Permanent delete — only allowed while the release is still in Draft (server-enforced).
  delete(id: number): Observable<any> {
    return this.httpService.delete<any>(`Release/${id}`);
  }
}

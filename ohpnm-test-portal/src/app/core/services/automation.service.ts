import { Injectable } from '@angular/core';
import {
  IAutomationData,
  IAutomationDataRequest,
  IAutomationDataSection,
  IAutomationDataSectionRequest,
  IAutomationFlow,
} from '@interfaces';
import { Mappers } from '@mappers';
import { HttpService } from '@services';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AutomationService {
  constructor(private httpService: HttpService) {}

  getFlows(): Observable<IAutomationFlow[]> {
    return this.httpService.get<any[]>(`Automation/flows`, {}, (res: any[]) =>
      res.map(Mappers.AutomationFlowMapper.fromApi)
    );
  }

  getSections(flowName: string): Observable<IAutomationDataSection[]> {
    return this.httpService.get<any[]>(
      `Automation/sections/${flowName}`,
      {},
      (res: any[]) => res.map(Mappers.AutomationDataSectionMapper.fromApi)
    );
  }

  getAutomationData(
    sectionId: number,
    userId: number,
    environmentId: number
  ): Observable<IAutomationData> {
    return this.httpService.get<IAutomationData>(
      `Automation/sections/data?sectionId=${sectionId}&userId=${userId}&environmentId=${environmentId}`,
      {},
      Mappers.AutomationDataMapper.fromApi
    );
  }

  updateAutomationData(data: IAutomationDataRequest): Observable<any> {
    return this.httpService.put(`Automation/data/`, data, undefined);
  }

  createAutomationData(data: IAutomationDataRequest): Observable<any> {
    return this.httpService.post(`Automation/data/`, data, undefined);
  }

  // Returns every AutomationData row saved for any section in this flow, across ALL
  // users/environments (not scoped like getAutomationData) - matches the exact scope
  // of the backend's own delete-guard check (usp_CountAutomationDataForSection also
  // counts across all users/environments), so it's the right source for the "Has
  // data"/"Empty" badges in Flow & Section Management.
  getAutomationDataByFlowName(flowName: string): Observable<IAutomationData[]> {
    return this.httpService.get<any[]>(
      `Automation/data/flow/${flowName}`,
      {},
      (res: any[]) => (res || []).map(Mappers.AutomationDataMapper.fromApi)
    );
  }

  // Section CRUD - wraps backend endpoints that already existed but were never
  // called by any frontend code before this (see AGENTS.md "Flow & Section
  // management"). There is no separate Flow entity/table - creating a "new Flow" is
  // just creating a Section whose FlowName doesn't exist yet.
  createSection(request: IAutomationDataSectionRequest): Observable<number> {
    return this.httpService.post(`Automation/sections`, request, undefined);
  }

  updateSection(request: IAutomationDataSectionRequest): Observable<any> {
    return this.httpService.put(`Automation/sections`, request, undefined);
  }

  deleteSection(sectionId: number, cascade = false): Observable<any> {
    const suffix = cascade ? '?cascade=true' : '';
    return this.httpService.delete(`Automation/sections/${sectionId}${suffix}`);
  }
}

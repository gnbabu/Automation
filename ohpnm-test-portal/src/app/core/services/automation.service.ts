import { Injectable } from '@angular/core';
import {
  IAutomationData,
  IAutomationDataRequest,
  IAutomationDataSection,
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
    userId: number
  ): Observable<IAutomationData> {
    return this.httpService.get<IAutomationData>(
      `Automation/sections/data?sectionId=${sectionId}&userId=${userId}`,
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
}

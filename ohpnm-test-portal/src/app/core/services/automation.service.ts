import { Injectable } from '@angular/core';
import {
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

  getAutomationSectionDataByFlowName(
    flowName: string
  ): Observable<IAutomationDataSection[]> {
    return this.httpService.get<any[]>(
      `Automation/data/flow/${flowName}`,
      {},
      (res: any[]) => res.map(Mappers.AutomationDataSectionMapper.fromApi)
    );
  }

  updateAutomationData(data: IAutomationDataRequest): Observable<any> {
    return this.httpService.put(`Automation/data/`, data, undefined);
  }
}

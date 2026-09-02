import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';

import {
  IAutomationData,
  IAutomationDataRequest,
  IAutomationDataSection,
  IAutomationFlow,
  IEnvironmentModel,
} from '@interfaces';
import {
  AuthService,
  AutomationService,
  CommonToasterService,
  EnvironmentService,
} from '@services';
@Component({
  selector: 'app-test-data-management',
  imports: [CommonModule, FormsModule],
  templateUrl: './test-data-management.component.html',
  styleUrl: './test-data-management.component.css',
})
export class TestDataManagementComponent implements OnInit {
  flows: IAutomationFlow[] = [];
  sections: IAutomationDataSection[] = [];
  environments: IEnvironmentModel[] = [];

  selectedFlow?: IAutomationFlow;
  selectedSection?: IAutomationDataSection;
  selectedEnvironment?: IEnvironmentModel;
  testContent: string = '';
  existingSectionData?: IAutomationData;

  constructor(
    private automationService: AutomationService,
    private toaster: CommonToasterService,
    private authService: AuthService,
    private environmentService: EnvironmentService
  ) {}

  ngOnInit(): void {
    this.loadFlows();
    this.loadEnvironments();
  }

  loadFlows() {
    this.automationService.getFlows().subscribe({
      next: (res) => (this.flows = res),
      error: (err) => console.error('Error loading flows:', err),
    });
  }

  loadEnvironments() {
    this.environmentService.getAll().subscribe({
      next: (res) => (this.environments = (res || []).filter((e) => e.isActive)),
      error: (err) => console.error('Error loading environments:', err),
    });
  }

  onFlowChange() {
    this.selectedSection = undefined;
    this.sections = [];
    this.testContent = '';
    this.existingSectionData = undefined;

    if (this.selectedFlow) {
      this.automationService.getSections(this.selectedFlow.flowName).subscribe({
        next: (res) => {
          this.sections = res;
        },
        error: (err) => console.error('Error loading sections:', err),
      });
    }
  }

  onEnvironmentChange() {
    this.selectedFlow = undefined;
    this.selectedSection = undefined;
    this.sections = [];
    this.testContent = '';
    this.existingSectionData = undefined;
  }

  onSectionChange() {
    this.testContent = '';
    this.existingSectionData = undefined;

    this.tryLoadAutomationData();
  }

  private tryLoadAutomationData() {
    if (!this.selectedFlow || !this.selectedSection || !this.selectedEnvironment) {
      return;
    }

    const userId = this.authService.getLoggedInUserId();
    const sectionId = this.selectedSection.sectionId;
    const environmentId = this.selectedEnvironment.environmentId;

    this.automationService
      .getAutomationData(sectionId, userId, environmentId)
      .subscribe({
        next: (res) => {
          this.existingSectionData = res;
          this.testContent = res.testContent;
        },
        error: (err) => console.error('Error loading sections:', err),
      });
  }

  isViewer(): boolean {
    return this.authService.isViewer();
  }

  onSubmit(form: NgForm): void {
    if (this.isViewer()) return;
    if (!form.valid || !this.selectedSection || !this.selectedEnvironment) return;

    if (!this.validateTestContentFormat(this.testContent)) {
      this.toaster.info('Test content format is invalid!');
      return;
    }

    let request: IAutomationDataRequest;
    let request$;

    if (this.existingSectionData?.id) {
      request = {
        id: this.existingSectionData.id,
        testContent: this.testContent,
      };
      request$ = this.automationService.updateAutomationData(request);
    } else {
      request = {
        sectionId: this.selectedSection.sectionId,
        testContent: this.testContent,
        userId: this.authService.getLoggedInUserId(),
        environmentId: this.selectedEnvironment.environmentId,
      };
      request$ = this.automationService.createAutomationData(request);
    }

    request$.subscribe({
      next: () => {
        this.toaster.success('Test content saved successfully');
      },
      error: (err) => {
        console.error('Error saving test content:', err);
      },
    });
  }

  validateTestContentFormat(content: string): boolean {
    if (!content) return false;

    const lines = content.trim().split(/\r?\n/);

    for (const line of lines) {
      const parts = line.split('|');
      if (parts.length !== 2) return false;

      const key = parts[0].trim();
      const value = parts[1].trim();

      if (!key || !value) return false;
    }

    return true;
  }
}

import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import {
  IAutomationData,
  IAutomationDataRequest,
  IAutomationDataSection,
  IAutomationFlow,
} from '@interfaces';
import {
  AuthService,
  AutomationService,
  CommonToasterService,
} from '@services';

@Component({
  selector: 'app-configure-test-data',
  imports: [CommonModule, FormsModule],
  templateUrl: './configure-test-data.component.html',
  styleUrl: './configure-test-data.component.css',
})
export class ConfigureTestDataComponent implements OnInit {
  flows: IAutomationFlow[] = [];
  sections: IAutomationDataSection[] = [];

  selectedFlow?: IAutomationFlow;
  selectedSection?: IAutomationDataSection;
  testContent: string = '';
  existingSectionData?: IAutomationData;

  constructor(
    private router: Router,
    private automationService: AutomationService,
    private toaster: CommonToasterService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadFlows();
  }

  loadFlows() {
    this.automationService.getFlows().subscribe({
      next: (res) => (this.flows = res),
      error: (err) => console.error('Error loading flows:', err),
    });
  }

  onFlowChange() {
    this.selectedSection = undefined;
    this.sections = [];
    this.testContent = '';

    if (this.selectedFlow) {
      this.automationService.getSections(this.selectedFlow.flowName).subscribe({
        next: (res) => {
          this.sections = res;
        },
        error: (err) => console.error('Error loading sections:', err),
      });
    }
  }

  onSectionChange() {
    this.testContent = '';
    this.existingSectionData = undefined;

    if (this.selectedFlow && this.selectedSection) {
      const userId = this.authService.getLoggedInUserId();
      const sectionId = this.selectedSection.sectionId;
      this.automationService.getAutomationData(sectionId, userId).subscribe({
        next: (res) => {
          this.existingSectionData = res;
          this.testContent = res.testContent;
        },
        error: (err) => console.error('Error loading sections:', err),
      });
    }
  }
  onSubmit(form: NgForm): void {
    if (!form.valid || !this.selectedSection) return;

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
      };
      request$ = this.automationService.createAutomationData(request);
    }

    request$.subscribe({
      next: () => {
        this.toaster.success('Test content saved successfully');
        this.router.navigate(['/test-suite']);
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

  goBack() {
    this.router.navigate(['/test-suite']);
  }
}

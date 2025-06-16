import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import {
  IAutomationDataRequest,
  IAutomationDataSection,
  IAutomationFlow,
} from '@interfaces';
import { AutomationService, CommonToasterService } from '@services';

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

  constructor(
    private router: Router,
    private automationService: AutomationService,
    private toaster: CommonToasterService
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
      this.automationService
        .getAutomationSectionDataByFlowName(this.selectedFlow.flowName)
        .subscribe({
          next: (res) => (this.sections = res),
          error: (err) => console.error('Error loading sections:', err),
        });
    }
  }

  onSectionChange() {
    this.testContent = '';

    if (this.selectedFlow && this.selectedSection) {
      const formattedJson = JSON.stringify(
        JSON.parse(this.selectedSection.testContent),
        null,
        2
      );
      this.testContent = formattedJson;
    }
  }
  onSubmit(form: NgForm) {
    if (!form.valid || !this.selectedSection) return;

    if (!this.isValidJson()) {
      this.toaster.info('Test content is not valid!');
      return;
    }

    const request: IAutomationDataRequest = {
      sectionID: this.selectedSection.sectionId,
      testContent: this.testContent,
    };

    this.automationService.updateAutomationData(request).subscribe({
      next: () => {
        this.toaster.success('Test content saved successfully');
        this.router.navigate(['/test-suite']);
      },
      error: (err) => {
        console.error('Error saving test content:', err);
      },
    });
  }

  isValidJson(): boolean {
    try {
      JSON.parse(this.testContent); // Try parsing testContent as JSON
      return true; // Return true if valid JSON
    } catch (e) {
      return false; // Return false if invalid JSON
    }
  }

  goBack() {
    this.router.navigate(['/test-suite']);
  }
}

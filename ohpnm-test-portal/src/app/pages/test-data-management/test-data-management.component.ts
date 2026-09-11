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
  ConfirmService,
  EnvironmentService,
} from '@services';

interface ITestDataRow {
  key: string;
  value: string;
  showValue: boolean;
}

// Simple keyword heuristic (not a real secrets feature - see AGENTS.md) used only to
// decide whether a row's Value should default to masked, matching the same show/hide
// eye-icon pattern already used on Login/Register.
const SENSITIVE_KEY_PATTERN = /password|pwd|secret/i;

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
  existingSectionData?: IAutomationData;

  rows: ITestDataRow[] = [];
  isSubmitting = false;

  // Deliberately NOT using NgForm's own `submitted` flag for row-level error display -
  // confirmed by direct testing that it stays true forever once a form has been
  // submitted once (success or failure) and never resets, so every row added
  // afterward (via addRow()) would be "born" already showing red "required" errors
  // before the user had any chance to type into it. This flag is reset whenever a new
  // row is added, so only rows that existed at the last actual submit attempt (and are
  // still incomplete) get flagged.
  submitAttempted = false;

  constructor(
    private automationService: AutomationService,
    private toaster: CommonToasterService,
    private authService: AuthService,
    private environmentService: EnvironmentService,
    private confirmService: ConfirmService
  ) {}

  ngOnInit(): void {
    this.loadFlows();
    this.loadEnvironments();
  }

  loadFlows() {
    this.automationService.getFlows().subscribe({
      next: (res) => (this.flows = res),
      error: (err) => {
        console.error('Error loading flows:', err);
        this.toaster.error('Failed to load flows. Please refresh and try again.');
      },
    });
  }

  loadEnvironments() {
    this.environmentService.getAll().subscribe({
      next: (res) => (this.environments = (res || []).filter((e) => e.isActive)),
      error: (err) => {
        console.error('Error loading environments:', err);
        this.toaster.error('Failed to load environments. Please refresh and try again.');
      },
    });
  }

  onFlowChange() {
    this.selectedSection = undefined;
    this.sections = [];
    this.resetRows();

    if (this.selectedFlow) {
      this.automationService.getSections(this.selectedFlow.flowName).subscribe({
        next: (res) => {
          this.sections = res;
        },
        error: (err) => {
          console.error('Error loading sections:', err);
          this.toaster.error('Failed to load sections for this flow.');
        },
      });
    }
  }

  onEnvironmentChange() {
    this.selectedFlow = undefined;
    this.selectedSection = undefined;
    this.sections = [];
    this.resetRows();
  }

  onSectionChange() {
    this.resetRows();
    this.tryLoadAutomationData();
  }

  private resetRows() {
    this.rows = [];
    this.existingSectionData = undefined;
    this.submitAttempted = false;
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
          this.rows = this.parseRows(res.testContent);
        },
        error: (err) => {
          console.error('Error loading test data:', err);
          this.toaster.error('Failed to load existing test data for this section.');
        },
      });
  }

  // Parses the exact same "key | value" per line format the backend has always stored
  // - the table is a presentation layer over the identical wire format, not a new data
  // shape (no backend/API changes anywhere in this feature).
  private parseRows(content: string): ITestDataRow[] {
    if (!content) return [];

    return content
      .trim()
      .split(/\r?\n/)
      .filter((line) => line.trim().length > 0)
      .map((line) => {
        const parts = line.split('|');
        const key = (parts[0] ?? '').trim();
        const value = (parts.slice(1).join('|') ?? '').trim();
        return { key, value, showValue: !SENSITIVE_KEY_PATTERN.test(key) };
      });
  }

  private serializeRows(rows: ITestDataRow[]): string {
    return rows.map((r) => `${r.key.trim()} | ${r.value.trim()}`).join('\n');
  }

  isViewer(): boolean {
    return this.authService.isViewer();
  }

  get fieldCount(): number {
    return this.rows.length;
  }

  isSensitiveKey(key: string): boolean {
    return SENSITIVE_KEY_PATTERN.test(key ?? '');
  }

  addRow(): void {
    this.rows.push({ key: '', value: '', showValue: true });
    this.submitAttempted = false;
  }

  removeRow(index: number): void {
    this.rows.splice(index, 1);
  }

  toggleRowValueVisibility(row: ITestDataRow): void {
    row.showValue = !row.showValue;
  }

  // Pressing Enter in the last row's Value field adds a new row automatically
  // (spreadsheet-style), instead of requiring a mouse click every time.
  onValueKeydown(event: KeyboardEvent, index: number): void {
    if (event.key === 'Enter' && index === this.rows.length - 1) {
      event.preventDefault();
      this.addRow();
    }
  }

  isDuplicateKey(index: number): boolean {
    const key = this.rows[index]?.key?.trim().toLowerCase();
    if (!key) return false;
    return this.rows.some(
      (r, i) => i !== index && r.key.trim().toLowerCase() === key
    );
  }

  get hasValidationErrors(): boolean {
    if (this.rows.length === 0) return true;
    return this.rows.some(
      (r, i) => !r.key.trim() || !r.value.trim() || this.isDuplicateKey(i)
    );
  }

  async onSubmit(form: NgForm): Promise<void> {
    if (this.isViewer() || this.isSubmitting) return;
    if (!this.selectedSection || !this.selectedEnvironment) return;

    this.submitAttempted = true;

    if (this.hasValidationErrors) {
      this.toaster.info(
        this.rows.length === 0
          ? 'Add at least one field before saving.'
          : 'Please fix the highlighted fields before saving.'
      );
      return;
    }

    const isUpdate = !!this.existingSectionData?.id;
    if (isUpdate) {
      const confirmed = await this.confirmService.confirm(
        'Update Test Data',
        'This will overwrite the existing test data saved for this section. Continue?'
      );
      if (!confirmed) return;
    }

    const testContent = this.serializeRows(this.rows);
    this.isSubmitting = true;

    let request$;
    if (isUpdate) {
      const request: IAutomationDataRequest = {
        id: this.existingSectionData!.id,
        testContent,
      };
      request$ = this.automationService.updateAutomationData(request);
    } else {
      const request: IAutomationDataRequest = {
        sectionId: this.selectedSection.sectionId,
        testContent,
        userId: this.authService.getLoggedInUserId(),
        environmentId: this.selectedEnvironment.environmentId,
      };
      request$ = this.automationService.createAutomationData(request);
    }

    request$.subscribe({
      next: () => {
        this.isSubmitting = false;
        this.toaster.success('Test content saved successfully');
        this.tryLoadAutomationData();
      },
      error: (err) => {
        this.isSubmitting = false;
        console.error('Error saving test content:', err);
        this.toaster.error('Failed to save test content. Please try again.');
      },
    });
  }
}

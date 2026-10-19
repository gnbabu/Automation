import { CommonModule } from '@angular/common';
import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormsModule } from '@angular/forms';

import {
  IEnvironmentModel,
  ITestDataCopySource,
  ITestDataDocument,
  ITestDataEffectiveDocument,
  ITestDataFieldSchema,
  ITestDataFlowSchema,
  ITestDataSectionSchema,
  ITestDataTestCase,
  TestDataRow,
} from '@interfaces';
import { CommonToasterService, ConfirmService, TestDataService } from '@services';
import { AppDropdownComponent } from 'app/core/components/app-dropdown/app-dropdown.component';
import { MAX_JSON_BYTES, rootSection, validateDocumentJson, validateSectionJson } from './json-validator';

const SENSITIVE_KEY_PATTERN = /password|pwd|secret/i;

export type TestDataView = 'form' | 'section' | 'whole';

@Component({
  selector: 'app-model-test-data',
  standalone: true,
  imports: [CommonModule, FormsModule, AppDropdownComponent],
  templateUrl: './model-test-data.component.html',
  styleUrl: './model-test-data.component.css',
})
export class ModelTestDataComponent implements OnChanges {
  @Input({ required: true }) testCase!: ITestDataTestCase;
  @Input({ required: true }) environment!: IEnvironmentModel;
  @Input() environments: IEnvironmentModel[] = [];
  @Input() readOnly = false;

  schema?: ITestDataFlowSchema;
  schemaError = '';
  loading = false;
  isSaving = false;

  private saved = new Map<number, TestDataRow[]>();
  private version: number | null = null;
  selectedSection?: ITestDataSectionSchema;
  rows: TestDataRow[] = [];
  private snapshot = '';
  revealed = new Set<string>();

  readonly showCopyFromEnvironment = false;

  view: TestDataView = 'form';
  jsonText = '';
  private jsonSnapshot = '';
  jsonErrors: string[] = [];
  pendingConversions: string[] = [];
  appliedConversions: string[] = [];
  serverErrors: string[] = [];
  jsonLoading = false;
  private effective?: ITestDataEffectiveDocument;
  private validateTimer?: ReturnType<typeof setTimeout>;

  copySources: ITestDataCopySource[] = [];
  copyFromTestCase?: ITestDataCopySource;
  copyFromEnvironment?: IEnvironmentModel;

  copySourceLabel = (s: ITestDataCopySource) => s.testCaseId || `${s.className}.${s.methodName}`;

  constructor(
    private testDataService: TestDataService,
    private toaster: CommonToasterService,
    private confirmService: ConfirmService,
  ) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['testCase'] || changes['environment']) this.load();
  }

  private load(): void {
    this.schema = undefined;
    this.schemaError = '';
    this.saved.clear();
    this.version = null;
    this.selectedSection = undefined;
    this.rows = [];
    this.snapshot = '';
    this.copySources = [];
    this.effective = undefined;
    this.clearJson();
    if (!this.testCase || !this.environment) return;

    this.loading = true;
    this.testDataService.getSchema(this.testCase.assignmentTestCaseId).subscribe({
      next: (schema) => {
        if (!schema) {
          this.loading = false;
          this.schemaError = 'This test case has no test data model.';
          return;
        }
        this.schema = schema;
        this.loadData();
        this.loadCopySources();
      },
      error: (err) => {
        this.loading = false;
        this.schemaError =
          typeof err?.error === 'string' ? err.error : 'Could not read the test data model for this test case.';
      },
    });
  }

  private loadData(selectSectionId?: number): void {
    this.testDataService.getData(this.testCase.assignmentTestCaseId, this.environment.environmentId).subscribe({
      next: (doc) => {
        this.loading = false;
        this.saved.clear();
        this.version = doc?.version ?? null;
        (doc?.sections || []).forEach((s) => this.saved.set(s.sectionId, s.rows || []));
        const target =
          this.schema?.sections.find((s) => s.sectionId === (selectSectionId ?? this.selectedSection?.sectionId)) ??
          this.schema?.sections[0];
        if (target) this.openSection(target);
        if (this.view !== 'form') this.loadJson();
      },
      error: (err) => {
        this.loading = false;
        console.error('Error loading test data:', err);
        this.toaster.error('Failed to load test data for this test case.');
      },
    });
  }

  private loadCopySources(): void {
    this.testDataService
      .getCopySources(this.testCase.assignmentTestCaseId, this.environment.environmentId)
      .subscribe({ next: (res) => (this.copySources = res || []), error: () => (this.copySources = []) });
  }

  get otherEnvironments(): IEnvironmentModel[] {
    return this.environments.filter((e) => e.environmentId !== this.environment?.environmentId);
  }

  isConfigured(section: ITestDataSectionSchema): boolean {
    return (this.saved.get(section.sectionId)?.length ?? 0) > 0;
  }

  usesDefaults(section: ITestDataSectionSchema): boolean {
    return !this.isConfigured(section) && (section.defaultRows?.length ?? 0) > 0;
  }

  savedRowCount(section: ITestDataSectionSchema): number {
    return this.saved.get(section.sectionId)?.length ?? 0;
  }

  get configuredCount(): number {
    return this.schema?.sections.filter((s) => this.isConfigured(s)).length ?? 0;
  }

  async selectSection(section: ITestDataSectionSchema): Promise<void> {
    if (section.sectionId === this.selectedSection?.sectionId) return;
    if (this.hasUnsavedChanges() && !(await this.confirmDiscardChanges())) return;
    this.openSection(section);
  }

  private openSection(section: ITestDataSectionSchema): void {
    this.selectedSection = section;
    this.revealed.clear();
    const saved = this.saved.get(section.sectionId) ?? [];
    this.setRows(section, saved.length > 0 ? saved : section.defaultRows ?? []);
    this.snapshot = this.serialize(this.rows);
    if (this.view === 'section' && this.effective) this.fillJson();
  }

  async setView(view: TestDataView): Promise<void> {
    if (view === this.view) return;
    if (this.hasUnsavedChanges() && !(await this.confirmDiscardChanges())) return;
    if (this.view === 'form' && this.selectedSection) this.openSection(this.selectedSection);
    this.view = view;
    this.clearJson();
    if (view !== 'form') this.loadJson();
  }

  private loadJson(): void {
    this.jsonLoading = true;
    this.testDataService.getDocument(this.testCase.assignmentTestCaseId, this.environment.environmentId).subscribe({
      next: (doc) => {
        this.jsonLoading = false;
        this.effective = doc;
        this.version = doc?.version ?? null;
        this.fillJson();
      },
      error: () => (this.jsonLoading = false),
    });
  }

  private fillJson(): void {
    if (!this.effective || !this.schema) return;
    const document = this.effective.document ?? {};
    let value: unknown = document;
    if (this.view === 'section' && this.selectedSection) {
      const section = this.selectedSection;
      value =
        section === rootSection(this.schema)
          ? Object.fromEntries(section.fields.map((f) => [f.name, document[f.name] ?? null]))
          : document[section.sectionName];
    }
    this.jsonText = JSON.stringify(value ?? null, null, 2);
    this.jsonSnapshot = this.jsonText;
    this.serverErrors = [];
    this.validateJson();
  }

  private clearJson(): void {
    this.jsonText = '';
    this.jsonSnapshot = '';
    this.jsonErrors = [];
    this.pendingConversions = [];
    this.appliedConversions = [];
    this.serverErrors = [];
  }

  get jsonScopeLabel(): string {
    return this.view === 'whole' ? 'Whole request' : (this.selectedSection?.sectionName ?? '');
  }

  get isJsonDirty(): boolean {
    return this.view !== 'form' && this.jsonText !== this.jsonSnapshot;
  }

  onJsonChange(): void {
    this.serverErrors = [];
    this.appliedConversions = [];
    clearTimeout(this.validateTimer);
    this.validateTimer = setTimeout(() => this.validateJson(), 300);
  }

  private validateJson(): ReturnType<typeof validateDocumentJson> | undefined {
    if (!this.schema || this.view === 'form') return undefined;
    const result =
      this.view === 'whole' || !this.selectedSection
        ? validateDocumentJson(this.schema, this.jsonText)
        : validateSectionJson(this.schema, this.selectedSection, this.jsonText);
    this.jsonErrors = result.errors;
    this.pendingConversions = result.conversions;
    return result;
  }

  formatJson(): void {
    const result = this.validateJson();
    if (!result || result.errors.length) return;
    const converted = result.conversions.length;
    this.jsonText = JSON.stringify(result.value ?? null, null, 2);
    this.validateJson();
    if (converted) this.toaster.info(`Formatted. Converted ${converted} value${converted === 1 ? '' : 's'} to match the model.`);
  }

  resetEdits(): void {
    this.jsonText = this.jsonSnapshot;
    this.serverErrors = [];
    this.appliedConversions = [];
    this.validateJson();
  }

  downloadJson(): void {
    const env = this.environment.environmentName.replace(/[^\w-]+/g, '_');
    const scope = this.view === 'section' && this.selectedSection ? `_${this.selectedSection.sectionName}` : '';
    const blob = new Blob([this.jsonText], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${this.testCaseLabel.replace(/[^\w-]+/g, '_')}_${env}${scope}.json`;
    a.click();
    URL.revokeObjectURL(url);
  }

  onJsonFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;
    if (!file.name.toLowerCase().endsWith('.json')) {
      this.toaster.warning('Choose a .json file.');
      return;
    }
    if (file.size > MAX_JSON_BYTES) {
      this.toaster.warning(`The file is too large (${Math.round(file.size / 1024)} KB). The limit is ${MAX_JSON_BYTES / 1024} KB.`);
      return;
    }
    const reader = new FileReader();
    reader.onload = () => {
      this.jsonText = String(reader.result ?? '');
      this.serverErrors = [];
      this.appliedConversions = [];
      this.validateJson();
      this.toaster.info(`Loaded ${file.name}. Review it and click Save to keep it.`);
    };
    reader.readAsText(file);
  }

  saveJson(): void {
    if (this.readOnly || this.isSaving || this.view === 'form') return;
    const result = this.validateJson();
    if (!result || result.errors.length) return;

    const sectionName = this.view === 'section' ? (this.selectedSection?.sectionName ?? null) : null;
    this.isSaving = true;
    this.testDataService
      .saveJson({
        assignmentTestCaseId: this.testCase.assignmentTestCaseId,
        environmentId: this.environment.environmentId,
        sectionName,
        json: this.jsonText,
        expectedVersion: this.version,
      })
      .subscribe({
        next: (res) => {
          this.isSaving = false;
          const sections = res?.sections ?? [];
          this.toaster.success(
            sectionName
              ? `${sectionName} saved for this test case.`
              : `Saved ${sections.length} section${sections.length === 1 ? '' : 's'} for this test case.`,
          );
          this.jsonSnapshot = this.jsonText;
          this.loadData(this.selectedSection?.sectionId);
          this.loadCopySources();
          this.appliedConversions = res?.conversions ?? [];
        },
        error: (err) => {
          this.isSaving = false;
          if (err?.status === 400) {
            const errors = err?.error?.errors;
            this.serverErrors = Array.isArray(errors) && errors.length ? errors : [err?.error?.message ?? 'The JSON was rejected.'];
          }
        },
      });
  }

  async resetToDefaults(section?: ITestDataSectionSchema): Promise<void> {
    if (this.readOnly) return;
    if (this.hasUnsavedChanges() && !(await this.confirmDiscardChanges())) return;
    const scope = section ? `the ${section.sectionName} section` : 'all sections';
    const ok = await this.confirmService.confirm(
      'Reset to default values?',
      `Remove the saved data for ${scope} of ${this.testCaseLabel} in ${this.environment.environmentName}? ` +
        `Test runs will use the model's default values from code instead.`,
    );
    if (!ok) return;

    this.testDataService
      .reset({
        assignmentTestCaseId: this.testCase.assignmentTestCaseId,
        environmentId: this.environment.environmentId,
        sectionName: section?.sectionName ?? null,
        expectedVersion: this.version,
      })
      .subscribe({
        next: (res) => {
          const n = res?.sections?.length ?? 0;
          this.toaster.success(n ? `Reset ${n} section${n === 1 ? '' : 's'} to the model's default values.` : 'Nothing was saved - already using defaults.');
          this.jsonSnapshot = this.jsonText;
          this.snapshot = this.serialize(this.rows);
          this.loadData(this.selectedSection?.sectionId);
          this.loadCopySources();
        },
      });
  }

  private setRows(section: ITestDataSectionSchema, source: TestDataRow[]): void {
    const toRow = (r: TestDataRow | undefined): TestDataRow => {
      const row: TestDataRow = {};
      section.fields.forEach((f) => (row[f.name] = this.toInputValue(f, r?.[f.name] ?? '')));
      return row;
    };
    this.rows = section.isList ? source.map(toRow) : [toRow(source[0])];
  }

  get unusedFields(): string[] {
    if (!this.selectedSection) return [];
    const known = new Set(this.selectedSection.fields.map((f) => f.name));
    const savedRows = this.saved.get(this.selectedSection.sectionId) ?? [];
    return [...new Set(savedRows.flatMap((r) => Object.keys(r)).filter((k) => !known.has(k)))];
  }

  addRow(): void {
    if (!this.selectedSection?.isList) return;
    const row: TestDataRow = {};
    this.selectedSection.fields.forEach((f) => (row[f.name] = ''));
    this.rows.push(row);
  }

  removeRow(index: number): void {
    this.rows.splice(index, 1);
  }

  inputType(field: ITestDataFieldSchema): string {
    if (this.isSensitive(field) && !this.revealed.has(field.name)) return 'password';
    switch (field.type) {
      case 'integer':
      case 'decimal':
        return 'number';
      case 'date':
        return 'date';
      default:
        return 'text';
    }
  }

  isSensitive(field: ITestDataFieldSchema): boolean {
    return SENSITIVE_KEY_PATTERN.test(field.name);
  }

  toggleReveal(field: ITestDataFieldSchema): void {
    this.revealed.has(field.name) ? this.revealed.delete(field.name) : this.revealed.add(field.name);
  }

  private toInputValue(field: ITestDataFieldSchema, value: string | null): string {
    const v = value ?? '';
    return field.type === 'date' && v.length > 10 ? v.substring(0, 10) : v;
  }

  copyRowsFromEnvironment(): void {
    if (!this.copyFromEnvironment) return;
    const env = this.copyFromEnvironment;
    this.copyFromEnvironment = undefined;
    this.testDataService
      .getData(this.testCase.assignmentTestCaseId, env.environmentId)
      .subscribe({
        next: (data) => this.applyCopy(data, env.environmentName),
        error: () => this.toaster.error('Failed to copy test data from that environment.'),
      });
  }

  copyRowsFromTestCase(): void {
    if (!this.copyFromTestCase) return;
    const source = this.copyFromTestCase;
    this.copyFromTestCase = undefined;
    this.testDataService
      .getData(this.testCase.assignmentTestCaseId, this.environment.environmentId, source)
      .subscribe({
        next: (data) => this.applyCopy(data, this.copySourceLabel(source)),
        error: () => this.toaster.error('Failed to copy test data from that test case.'),
      });
  }

  async copyAllFromTestCase(): Promise<void> {
    if (!this.copyFromTestCase) return;
    const source = this.copyFromTestCase;
    await this.copyAll(this.copySourceLabel(source), {
      fromLibrary: source.libraryName,
      fromClass: source.className,
      fromMethod: source.methodName,
    });
    this.copyFromTestCase = undefined;
  }

  async copyAllFromEnvironment(): Promise<void> {
    if (!this.copyFromEnvironment) return;
    const env = this.copyFromEnvironment;
    await this.copyAll(`${this.testCaseLabel} in ${env.environmentName}`, { fromEnvironmentId: env.environmentId });
    this.copyFromEnvironment = undefined;
  }

  private get testCaseLabel(): string {
    return this.testCase.testCaseId || `${this.testCase.className}.${this.testCase.methodName}`;
  }

  private async copyAll(
    sourceLabel: string,
    source: { fromLibrary?: string; fromClass?: string; fromMethod?: string; fromEnvironmentId?: number },
  ): Promise<void> {
    if (this.readOnly) return;
    if (this.hasUnsavedChanges() && !(await this.confirmDiscardChanges())) return;

    const ok = await this.confirmService.confirm(
      'Copy all test data?',
      `Replace all saved test data for ${this.testCaseLabel} in ${this.environment.environmentName} with the data from ${sourceLabel}? ` +
        `Sections not saved there will go back to the model's default values.`,
    );
    if (!ok) return;

    this.testDataService
      .copyAll({
        assignmentTestCaseId: this.testCase.assignmentTestCaseId,
        environmentId: this.environment.environmentId,
        expectedVersion: this.version,
        ...source,
      })
      .subscribe({
        next: (res) => {
          const n = res?.copiedSections?.length ?? 0;
          this.toaster.success(`Copied ${n} section${n === 1 ? '' : 's'} from ${sourceLabel}.`);
          this.loadData(this.selectedSection?.sectionId);
          this.loadCopySources();
        },
        error: (err) => {
          const message = typeof err?.error === 'string' ? err.error : 'Failed to copy test data.';
          if (err?.status === 409) this.toaster.warning(message);
          else this.toaster.error(message);
        },
      });
  }

  private applyCopy(doc: ITestDataDocument, sourceLabel: string): void {
    if (!this.selectedSection) return;
    const section = this.selectedSection;
    const rows = (doc?.sections || []).find((s) => s.sectionId === section.sectionId)?.rows ?? [];
    if (rows.length === 0) {
      this.toaster.info(`No ${section.sectionName} data found in ${sourceLabel}.`);
      return;
    }
    this.setRows(section, rows);
    this.toaster.success(
      `Copied ${rows.length} row${rows.length === 1 ? '' : 's'} of ${section.sectionName} from ${sourceLabel}. Review and Save to keep them.`,
    );
  }

  async save(): Promise<void> {
    if (this.readOnly || this.isSaving || !this.selectedSection) return;
    const section = this.selectedSection;

    if (this.unusedFields.length > 0) {
      const ok = await this.confirmService.confirm(
        'Remove fields no longer in code?',
        `${this.unusedFields.join(', ')} ${this.unusedFields.length === 1 ? 'is' : 'are'} no longer in the ${this.schema?.flowName} model and will be removed when you save. Continue?`,
      );
      if (!ok) return;
    }

    this.isSaving = true;
    this.testDataService
      .saveSection({
        assignmentTestCaseId: this.testCase.assignmentTestCaseId,
        environmentId: this.environment.environmentId,
        sectionId: section.sectionId,
        rows: this.rows,
        expectedVersion: this.version,
      })
      .subscribe({
        next: () => {
          this.isSaving = false;
          this.toaster.success(`${section.sectionName} saved for this test case.`);
          this.loadData(section.sectionId);
          this.loadCopySources();
        },
        error: (err) => {
          this.isSaving = false;
          console.error('Error saving test data:', err);
          const message = typeof err?.error === 'string' ? err.error : 'Failed to save test data.';
          if (err?.status === 409) this.toaster.warning(message);
          else this.toaster.error(message);
        },
      });
  }

  hasUnsavedChanges(): boolean {
    if (this.isJsonDirty) return true;
    return this.view === 'form' && !!this.selectedSection && this.serialize(this.rows) !== this.snapshot;
  }

  private confirmDiscardChanges(): Promise<boolean> {
    return this.confirmService.confirm(
      'Discard Unsaved Changes?',
      'You have unsaved test data changes for this section. Leaving now will discard them. Continue?',
    );
  }

  private serialize(rows: TestDataRow[]): string {
    return JSON.stringify(rows);
  }

  trackByIndex = (i: number) => i;
}

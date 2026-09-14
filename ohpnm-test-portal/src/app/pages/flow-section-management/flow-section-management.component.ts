import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';

import { IAutomationDataSection, IAutomationFlow } from '@interfaces';
import {
  AuthService,
  AutomationService,
  CommonToasterService,
  ConfirmService,
} from '@services';

// There is no separate "Flow" entity/table anywhere in the backend - a Flow is just
// the distinct set of FlowName values across aut.AutomationDataSections
// (usp_get_AutomationFlowNames is literally "SELECT DISTINCT FlowName"). So "creating a
// new Flow" here is really "creating a Section whose FlowName doesn't exist yet" - see
// AGENTS.md "Flow & Section management" for the full backend investigation.
@Component({
  selector: 'app-flow-section-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './flow-section-management.component.html',
  styleUrl: './flow-section-management.component.css',
})
export class FlowSectionManagementComponent implements OnInit {
  flows: IAutomationFlow[] = [];
  selectedFlowName?: string;

  sections: IAutomationDataSection[] = [];
  loadingSections = false;

  // Saved-data row count per section - across ALL users/environments, matching the
  // exact scope of the backend's own delete-guard check (usp_CountAutomationDataForSection
  // also counts across all users/environments), not the current-user-only scope used
  // by Test Data Management's own section overview. A count (not just a boolean) so a
  // cascade-delete confirmation can say how many entries will be deleted.
  sectionDataCounts = new Map<number, number>();

  showNewFlowInput = false;
  newFlowName = '';

  newSectionName = '';
  isSubmittingNewSection = false;

  // Inline-rename working copies, keyed by sectionId - edited independently of the
  // loaded `sections` array so a rename-in-progress doesn't affect the "has changed"
  // check until Save is actually clicked.
  editedSectionNames = new Map<number, string>();
  savingSectionId?: number;
  deletingSectionId?: number;
  isDeletingFlow = false;

  // "Delete Flow" gets its own dedicated type-to-confirm modal (not the shared
  // ConfirmService used everywhere else in this app for plain Yes/No confirms) -
  // deliberately not extending that shared service just for this one destructive,
  // much-larger-blast-radius action (a single click can span dozens of sections and
  // every saved test data entry across them, for every user/environment - unlike a
  // single section delete, which stays contained to one section). Matches the same
  // "type the exact name to unlock the button" pattern used for deleting a GitHub repo.
  showDeleteFlowConfirm = false;
  deleteFlowConfirmText = '';

  constructor(
    private automationService: AutomationService,
    private toaster: CommonToasterService,
    private authService: AuthService,
    private confirmService: ConfirmService
  ) {}

  ngOnInit(): void {
    this.loadFlows();
  }

  isViewer(): boolean {
    return this.authService.isViewer();
  }

  loadFlows(): void {
    this.automationService.getFlows().subscribe({
      next: (res) => (this.flows = res),
      error: (err) => console.error('Error loading flows:', err),
    });
  }

  onFlowChange(): void {
    this.showNewFlowInput = false;
    this.newSectionName = '';
    this.loadSectionsForFlow();
  }

  toggleNewFlowInput(): void {
    this.showNewFlowInput = !this.showNewFlowInput;
    this.newFlowName = '';
    if (this.showNewFlowInput) {
      this.selectedFlowName = undefined;
      this.sections = [];
      this.sectionDataCounts.clear();
    }
  }

  // A "new flow" only really exists once its first Section is actually created - this
  // just switches the page into "adding sections to a not-yet-real flow" mode.
  // Typed names are validated against the already-loaded `flows` list (case-
  // insensitive, trimmed) so you can't "create" a flow that already exists - that
  // would just silently merge into the existing one, which reads like a successful
  // creation but isn't.
  get isDuplicateFlowName(): boolean {
    const name = this.newFlowName.trim().toLowerCase();
    if (!name) return false;
    return this.flows.some(
      (f) => f.flowName?.trim().toLowerCase() === name
    );
  }

  startNewFlow(): void {
    if (!this.newFlowName.trim() || this.isDuplicateFlowName) return;
    this.selectedFlowName = this.newFlowName.trim();
    this.sections = [];
    this.sectionDataCounts.clear();
    this.showNewFlowInput = false;
    this.newSectionName = '';
  }

  private loadSectionsForFlow(): void {
    if (!this.selectedFlowName) return;

    this.loadingSections = true;
    this.automationService.getSections(this.selectedFlowName).subscribe({
      next: (res) => {
        this.sections = res;
        this.editedSectionNames.clear();
        res.forEach((s) => this.editedSectionNames.set(s.sectionId, s.sectionName));
        this.loadSectionDataStatus();
      },
      error: (err) => {
        console.error('Error loading sections:', err);
        this.loadingSections = false;
      },
    });
  }

  private loadSectionDataStatus(): void {
    if (!this.selectedFlowName) {
      this.loadingSections = false;
      return;
    }

    this.automationService.getAutomationDataByFlowName(this.selectedFlowName).subscribe({
      next: (dataRows) => {
        this.sectionDataCounts.clear();
        dataRows.forEach((d) => {
          if (d.testContent?.trim()) {
            this.sectionDataCounts.set(
              d.sectionId,
              (this.sectionDataCounts.get(d.sectionId) ?? 0) + 1
            );
          }
        });
        this.loadingSections = false;
      },
      error: (err) => {
        console.error('Error loading section data status:', err);
        this.loadingSections = false;
      },
    });
  }

  hasData(sectionId: number): boolean {
    return this.dataCountFor(sectionId) > 0;
  }

  dataCountFor(sectionId: number): number {
    return this.sectionDataCounts.get(sectionId) ?? 0;
  }

  addSection(): void {
    if (this.isViewer() || !this.selectedFlowName || !this.newSectionName.trim()) return;

    this.isSubmittingNewSection = true;
    this.automationService
      .createSection({
        sectionName: this.newSectionName.trim(),
        flowName: this.selectedFlowName,
      })
      .subscribe({
        next: () => {
          this.isSubmittingNewSection = false;
          this.toaster.success('Section created.');
          this.newSectionName = '';
          // Refreshes the Flow list too, in case this was actually a brand-new flow
          // (its very first Section) rather than an addition to an existing one.
          this.loadFlows();
          this.loadSectionsForFlow();
        },
        error: (err) => {
          this.isSubmittingNewSection = false;
          console.error('Error creating section:', err);
        },
      });
  }

  hasUnsavedRename(section: IAutomationDataSection): boolean {
    const edited = this.editedSectionNames.get(section.sectionId)?.trim();
    return !!edited && edited !== section.sectionName;
  }

  saveRename(section: IAutomationDataSection): void {
    if (this.isViewer() || !this.selectedFlowName) return;

    const newName = this.editedSectionNames.get(section.sectionId)?.trim();
    if (!newName || newName === section.sectionName) return;

    this.savingSectionId = section.sectionId;
    this.automationService
      .updateSection({
        sectionId: section.sectionId,
        sectionName: newName,
        flowName: this.selectedFlowName,
      })
      .subscribe({
        next: () => {
          this.savingSectionId = undefined;
          this.toaster.success('Section renamed.');
          this.loadSectionsForFlow();
        },
        error: (err) => {
          this.savingSectionId = undefined;
          console.error('Error renaming section:', err);
        },
      });
  }

  async deleteSection(section: IAutomationDataSection): Promise<void> {
    if (this.isViewer()) return;

    const dataCount = this.dataCountFor(section.sectionId);
    const confirmed = await this.confirmService.confirm(
      'Delete Section',
      dataCount > 0
        ? `"${section.sectionName}" has ${dataCount} saved test data entr${dataCount === 1 ? 'y' : 'ies'} which will be permanently deleted along with it - for every user and environment. Continue?`
        : `Delete section "${section.sectionName}"? This cannot be undone.`
    );
    if (!confirmed) return;

    this.deletingSectionId = section.sectionId;
    this.automationService.deleteSection(section.sectionId, dataCount > 0).subscribe({
      next: () => {
        this.deletingSectionId = undefined;
        this.toaster.success('Section deleted.');
        this.loadSectionsForFlow();
      },
      error: (err) => {
        this.deletingSectionId = undefined;
        console.error('Error deleting section:', err);
      },
    });
  }

  // A Flow has no row of its own - it's just the distinct FlowName across its
  // Sections - so "deleting a flow" means deleting all of its sections. Matches
  // deleteSection's own cascade behavior below (a section with saved data can be
  // deleted, with confirmation, and its data goes with it) rather than the stricter
  // "only if every section is already empty" rule this originally had, which made
  // "Delete Flow" permanently disabled for any flow with real data even though
  // deleting each of its sections individually, one at a time, would have worked.
  get totalDataCountForSelectedFlow(): number {
    return [...this.sectionDataCounts.values()].reduce((sum, c) => sum + c, 0);
  }

  get canDeleteSelectedFlow(): boolean {
    return (
      !!this.selectedFlowName &&
      !this.showNewFlowInput &&
      !this.loadingSections &&
      this.sections.length > 0
    );
  }

  // Opens the type-to-confirm modal rather than deleting directly - the actual
  // deletion happens in confirmDeleteFlowFromModal() once the typed text matches.
  openDeleteFlowConfirm(): void {
    if (this.isViewer() || !this.canDeleteSelectedFlow) return;
    this.deleteFlowConfirmText = '';
    this.showDeleteFlowConfirm = true;
  }

  cancelDeleteFlowConfirm(): void {
    this.showDeleteFlowConfirm = false;
    this.deleteFlowConfirmText = '';
  }

  get deleteFlowConfirmTextMatches(): boolean {
    return this.deleteFlowConfirmText === this.selectedFlowName;
  }

  confirmDeleteFlowFromModal(): void {
    if (!this.deleteFlowConfirmTextMatches || this.isDeletingFlow) return;

    this.showDeleteFlowConfirm = false;
    this.isDeletingFlow = true;
    const deletions = this.sections.map((s) =>
      this.automationService.deleteSection(s.sectionId, this.dataCountFor(s.sectionId) > 0)
    );

    forkJoin(deletions).subscribe({
      next: () => {
        this.isDeletingFlow = false;
        this.toaster.success(`Flow "${this.selectedFlowName}" deleted.`);
        this.selectedFlowName = undefined;
        this.sections = [];
        this.sectionDataCounts.clear();
        this.editedSectionNames.clear();
        this.deleteFlowConfirmText = '';
        this.loadFlows();
      },
      error: (err) => {
        // forkJoin aborts on the first failure - sections already deleted stay
        // deleted; reload so the list reflects whatever actually remains rather than
        // leaving stale rows on screen.
        this.isDeletingFlow = false;
        console.error('Error deleting flow:', err);
        this.loadSectionsForFlow();
        this.loadFlows();
      },
    });
  }
}

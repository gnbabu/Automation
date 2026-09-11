// unsaved-changes.guard.ts
import { CanDeactivateFn } from '@angular/router';

// Standard Angular CanDeactivate pattern - the guard receives the leaving component
// instance itself and defers the actual "is there unsaved work, and should we let the
// user leave" decision to it, since only the component knows what "unsaved" means for
// its own data (see TestDataManagementComponent's isDirty()/confirmDiscard()).
export interface IConfirmsUnsavedChanges {
  hasUnsavedChanges(): boolean;
  confirmDiscardChanges(): Promise<boolean>;
}

export const unsavedChangesGuard: CanDeactivateFn<IConfirmsUnsavedChanges> = async (
  component
) => {
  if (!component.hasUnsavedChanges()) return true;
  return await component.confirmDiscardChanges();
};

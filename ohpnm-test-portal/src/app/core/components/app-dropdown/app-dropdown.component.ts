import { CommonModule } from '@angular/common';
import {
  Component,
  EventEmitter,
  forwardRef,
  HostBinding,
  Input,
  Output,
  OnChanges,
  SimpleChanges,
} from '@angular/core';
import {
  ControlValueAccessor,
  FormsModule,
  NG_VALUE_ACCESSOR,
} from '@angular/forms';

@Component({
  selector: 'app-dropdown',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './app-dropdown.component.html',
  styleUrls: ['./app-dropdown.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => AppDropdownComponent),
      multi: true,
    },
  ],
})
export class AppDropdownComponent implements ControlValueAccessor, OnChanges {
  @Input() options: any[] = [];
  @Input() placeholder: string = 'Select...';
  // Defaults to true (unchanged existing behavior) - set to false for fields that
  // always hold a real default value and should never be selectable back to an
  // empty/null state (see app-dropdown.component.html).
  @Input() showPlaceholder: boolean = true;
  @Input() textAccessor: string | ((option: any) => string) = 'name';
  @Input() disabled: boolean = false;
  @Input() selected: any = null; // parent can bind with [(selected)]
  // Bootstrap validation styling - applied to the real inner <select> for the visual
  // red border, and mirrored onto this component's own host element (see
  // isInvalidHostClass below) so a sibling `.invalid-feedback` block still shows via
  // Bootstrap's plain `.is-invalid ~ .invalid-feedback { display: block; }` rule,
  // which matches on class alone regardless of element type.
  @Input() isInvalid: boolean = false;
  // Optional - when unset (default), this component behaves exactly as before:
  // [(selected)]/ngModel bind to the whole option object from `options`. When set,
  // the bound/emitted value is instead extracted from each option via this accessor
  // (a field name or a function, same convention as textAccessor) - e.g.
  // bindValue="loginUserId" so [(ngModel)]="loginUserId" gets just the id, not the
  // whole LoginUser object. Deliberately NOT named the same as the pre-existing,
  // never-actually-wired-up `valueAccessor` attribute already sitting (as dead
  // markup) on Dashboard's <app-dropdown> - reusing that name would have silently
  // turned a no-op into live behavior for existing, working code.
  @Input() bindValue: string | ((option: any) => any) | null = null;
  @Output() selectedChange = new EventEmitter<any>();
  @Output() selectionChange = new EventEmitter<any>();

  // Always the full option object from `options` (or null) - what the *internal*
  // <select>'s [ngValue] bindings match against, regardless of bindValue mode.
  selectedValue: any = null;
  isDisabled = false;

  onChange: any = () => {};
  onTouched: any = () => {};

  @HostBinding('class.is-invalid')
  get isInvalidHostClass(): boolean {
    return this.isInvalid;
  }

  ngOnChanges(changes: SimpleChanges) {
    // Re-resolve whenever the externally-bound value OR the options list changes -
    // the latter matters in bindValue mode specifically: `selected`/ngModel's value
    // can legitimately arrive before `options` has finished loading (e.g. an async
    // dropdown fetch), in which case the matching option isn't findable yet and must
    // be re-resolved once `options` actually populates.
    if (changes['selected']) {
      this.selectedValue = this.resolveOptionForValue(this.selected);
    } else if (changes['options'] && this.bindValue != null) {
      this.selectedValue = this.resolveOptionForValue(this.selected);
    }
  }

  writeValue(value: any): void {
    this.selectedValue = this.resolveOptionForValue(value);
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.isDisabled = isDisabled;
  }

  onSelectChange(val: any) {
    this.selectedValue = val;
    const emitValue = this.getBoundValue(val);
    this.selected = emitValue;
    this.onChange(emitValue);
    this.onTouched();
    this.selectedChange.emit(emitValue);
    this.selectionChange.emit(emitValue);
  }

  getOptionText(option: any) {
    if (!option) return '';
    if (typeof this.textAccessor === 'function') {
      return this.textAccessor(option); // call the function
    }
    return option[this.textAccessor]; // use property name
  }

  get isDropdownDisabled(): boolean {
    return this.isDisabled || this.disabled;
  }

  // Converts a full option object to whatever should actually be bound/emitted -
  // itself, unchanged, unless bindValue is set (see @Input() bindValue above).
  private getBoundValue(option: any): any {
    if (option == null) return null;
    if (this.bindValue == null) return option;
    return typeof this.bindValue === 'function'
      ? this.bindValue(option)
      : option[this.bindValue];
  }

  // Inverse of getBoundValue - given an externally-bound value (ngModel/[selected]),
  // finds the matching option from `options` so the internal <select>'s [ngValue]
  // bindings (which always compare against full option objects) highlight correctly.
  // In default (no bindValue) mode, the external value already IS the option.
  private resolveOptionForValue(value: any): any {
    // Normalizes `undefined` to `null` - the internal <select>'s placeholder
    // <option> is always bound to [ngValue]="null" (see app-dropdown.component.
    // html), and Angular's option matching treats null and undefined as genuinely
    // different values (no match => nothing highlighted at all, not even the
    // placeholder). Several real callers declare their bound field as `foo?: T`
    // with no initializer (plain `undefined`, e.g. TestDataManagementComponent's
    // selectedEnvironment) or explicitly reset it via `= undefined` rather than
    // `= null` - confirmed by direct testing this produced a blank-looking dropdown
    // instead of showing the placeholder text.
    if (this.bindValue == null) {
      if (value == null) return null;
      // Strict match covers the common whole-object case (e.g. selectedEnvironment
      // pointing at one of the real objects in `options` - reference equality is
      // exactly right there, unchanged from before this fix). Falls back to a
      // string-based match for primitive-option cases where the *initial* value's
      // real type doesn't match the options' own type - e.g. AddEditUserComponent's
      // twoFactor: a real boolean (`false`, from the User model/backend) matched
      // against options deliberately kept as the literal strings 'true'/'false' (to
      // preserve an existing native <select> string-coercion quirk on request - see
      // AGENTS.md "AppDropdown migration"). Falls through to the original value
      // unchanged if no option matches even loosely, same as before this fix.
      if (this.options.some((opt) => opt === value)) return value;
      const looseMatch = this.options.find(
        (opt) => String(opt) === String(value),
      );
      return looseMatch ?? value;
    }
    if (value == null) return null;
    // Strict match first (the common case - e.g. a real numeric id matching another
    // real numeric id). Falls back to a string-based comparison so a value that
    // legitimately arrives as a different-but-equivalent type still highlights
    // correctly - e.g. a boolean/number value coming from the backend (JSON) being
    // matched against options whose bindValue intentionally mirrors an existing
    // native <select>'s plain `value="..."` attribute coercion (always a string) -
    // see AGENTS.md "AppDropdown migration" for why some fields deliberately keep
    // that pre-existing string-coercion behavior instead of being fixed to real types.
    return (
      this.options.find((opt) => this.getBoundValue(opt) === value) ??
      this.options.find(
        (opt) => String(this.getBoundValue(opt)) === String(value),
      ) ??
      null
    );
  }
}

import { CommonModule } from '@angular/common';
import {
  Component,
  ElementRef,
  EventEmitter,
  forwardRef,
  HostListener,
  Input,
  Output,
} from '@angular/core';
import {
  ControlValueAccessor,
  FormsModule,
  NG_VALUE_ACCESSOR,
} from '@angular/forms';

@Component({
  selector: 'app-multiselect-dropdown',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app-multiselect-dropdown.component.html',
  styleUrls: ['./app-multiselect-dropdown.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => AppMultiselectDropdownComponent),
      multi: true,
    },
  ],
})
export class AppMultiselectDropdownComponent implements ControlValueAccessor {
  @Input() options: any[] = [];
  @Input() placeholder: string = 'Select...';
  @Input() textAccessor: string | ((option: any) => string) = 'name';
  @Input() disabled: boolean = false;
  @Output() selectionChange = new EventEmitter<any[]>();

  selectedValues: any[] = [];
  isOpen = false;

  onChange: any = () => {};
  onTouched: any = () => {};

  constructor(private elRef: ElementRef) {}

  // ControlValueAccessor
  writeValue(value: any): void {
    this.selectedValues = value || [];
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  toggleDropdown() {
    if (this.disabled) return;
    this.isOpen = !this.isOpen;
  }

  isSelected(option: any): boolean {
    return this.selectedValues.includes(option);
  }

  toggleSelection(option: any) {
    if (this.isSelected(option)) {
      this.selectedValues = this.selectedValues.filter((x) => x !== option);
    } else {
      this.selectedValues = [...this.selectedValues, option];
    }
    this.emitChanges();
  }

  selectAll() {
    this.selectedValues = [...this.options];
    this.emitChanges();
  }

  clearAll() {
    this.selectedValues = [];
    this.emitChanges();
  }

  emitChanges() {
    this.onChange(this.selectedValues);
    this.selectionChange.emit(this.selectedValues);
  }

  getSelectedText(): string {
    // Always show placeholder
    return this.placeholder;
  }

  getOptionText(option: any) {
    if (typeof this.textAccessor === 'function') {
      return this.textAccessor(option);
    }
    return option[this.textAccessor];
  }

  // Close dropdown when clicking outside
  @HostListener('document:click', ['$event.target'])
  onClickOutside(targetElement: any) {
    const clickedInside = this.elRef.nativeElement.contains(targetElement);
    if (!clickedInside) {
      this.isOpen = false;
    }
  }
}

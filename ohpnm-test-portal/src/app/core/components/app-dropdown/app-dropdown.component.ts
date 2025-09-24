import { CommonModule } from '@angular/common';
import {
  Component,
  EventEmitter,
  forwardRef,
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
  @Input() textAccessor: string | ((option: any) => string) = 'name';
  @Input() disabled: boolean = false;
  @Input() selected: any = null; // parent can bind with [(selected)]
  @Output() selectedChange = new EventEmitter<any>();
  @Output() selectionChange = new EventEmitter<any>();

  selectedValue: any = null;
  isDisabled = false;

  onChange: any = () => {};
  onTouched: any = () => {};

  ngOnChanges(changes: SimpleChanges) {
    if (changes['selected']) {
      this.selectedValue = this.selected;
    }
  }

  writeValue(value: any): void {
    this.selectedValue = value;
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
    this.selected = val;
    this.onChange(val);
    this.onTouched();
    this.selectedChange.emit(val);
    this.selectionChange.emit(val);
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
}

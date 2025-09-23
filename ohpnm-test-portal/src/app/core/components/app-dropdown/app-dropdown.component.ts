import { CommonModule } from '@angular/common';
import {
  Component,
  EventEmitter,
  forwardRef,
  Input,
  Output,
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
export class AppDropdownComponent implements ControlValueAccessor {
  @Input() options: any[] = [];
  @Input() placeholder: string = 'Select...';
  @Input() textAccessor: string = 'name'; // property name for display text
  @Input() disabled: boolean = false; // allow disabling from parent
  @Input() selected: any; // input to bind initial value
  @Output() selectedChange = new EventEmitter<any>(); // emit selected object
  @Output() selectionChange = new EventEmitter<any>(); // optional extra event

  selectedValue: any = null; // internal model
  isDisabled = false;

  // ControlValueAccessor callbacks
  onChange: any = () => {};
  onTouched: any = () => {};

  ngOnChanges() {
    if (this.selected) {
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

  // Called when dropdown value changes
  onSelectChange(val: any) {
    this.selectedValue = val;
    this.selected = val;
    this.onChange(val);
    this.onTouched();
    this.selectedChange.emit(val); // bind to parent [(selected)]
    this.selectionChange.emit(val); // optional event
  }

  getOptionValue(option: any) {
    return option; // return full object
  }

  getOptionText(option: any) {
    return option[this.textAccessor];
  }

  get isDropdownDisabled(): boolean {
    return this.isDisabled || this.disabled;
  }
}

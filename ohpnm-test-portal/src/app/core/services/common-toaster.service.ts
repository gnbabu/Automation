import { Injectable } from '@angular/core';
import { ToastrService } from 'ngx-toastr';

@Injectable({
  providedIn: 'root',
})
export class CommonToasterService {
  constructor(private toastr: ToastrService) {}

  success(message: string, title?: string) {
    this.toastr.success(message, title || 'Success');
  }

  error(message: string, title?: string) {
    this.toastr.error(message, title || 'Error');
  }

  info(message: string, title?: string) {
    this.toastr.info(message, title || 'Info');
  }

  warning(message: string, title?: string) {
    this.toastr.warning(message, title || 'Warning');
  }
}

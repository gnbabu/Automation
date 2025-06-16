import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class HttpService {
  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Content-Type': 'application/json',
    });
  }

  private fullUrl(url: string): string {
    // Automatically appends the base URL if not already absolute
    return url.startsWith('http') ? url : `${environment.apiUrl}/${url}`;
  }

  get<T>(
    url: string,
    params: any = {},
    fromApi?: (json: any) => T
  ): Observable<T> {
    return this.http
      .get<any>(this.fullUrl(url), {
        headers: this.getHeaders(),
        params: new HttpParams({ fromObject: params }),
      })
      .pipe(
        map((res) => (fromApi ? fromApi(res) : res)),
        catchError(this.handleError)
      );
  }

  post<T>(
    url: string,
    model: any,
    toApi?: (model: any) => any,
    fromApi?: (json: any) => T
  ): Observable<T> {
    const payload = toApi ? toApi(model) : model;
    return this.http
      .post<any>(this.fullUrl(url), payload, {
        headers: this.getHeaders(),
      })
      .pipe(
        map((res) => (fromApi ? fromApi(res) : res)),
        catchError(this.handleError)
      );
  }

  put<T>(
    url: string,
    model: any,
    toApi?: (model: any) => any,
    fromApi?: (json: any) => T
  ): Observable<T> {
    const payload = toApi ? toApi(model) : model;
    return this.http
      .put<any>(this.fullUrl(url), payload, {
        headers: this.getHeaders(),
      })
      .pipe(
        map((res) => (fromApi ? fromApi(res) : res)),
        catchError(this.handleError)
      );
  }

  delete<T>(url: string): Observable<T> {
    return this.http
      .delete<any>(this.fullUrl(url), {
        headers: this.getHeaders(),
      })
      .pipe(catchError(this.handleError));
  }

  private handleError(error: any) {
    console.error('HTTP error:', error);
    return throwError(() => error);
  }
}

import { TemplateRef } from '@angular/core';

// grid-column.model.ts
export interface GridColumn {
  field: string;
  header: string;
  sortable?: boolean;
  cellTemplate?: TemplateRef<any> | null; // template reference name for custom cell templates
  type?: 'text' | 'number' | 'date' | 'datetime'; // optional for future extensibility
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface IUser {
  userId?: number;
  userName: string;
  password?: string;
  firstName: string;
  lastName: string;
  email: string;
  photo?: string;
  active: boolean;
  roleName: string;
  roleId: number | null | undefined;
}

export interface IUserRole {
  roleId: number;
  roleName: string;
}

export interface IChangePasswordRequest {
  oldPassword: string;
  newPassword: string;
  userId: number | undefined;
}

export interface LibraryInfo {
  libraryName: string;
  classes: ClassInfo[];
}

export interface LibraryMethodInfo {
  methodName: string;
}

export interface ClassInfo {
  className: string;
  methods: LibraryMethodInfo[];
}

export interface IAutomationFlow {
  flowName: string;
}

export interface IAutomationDataSection {
  id: number;
  sectionId: number;
  testContent: string;
  sectionName: string;
}

export interface IAutomationDataRequest {
  sectionID: number;
  testContent: string;
}

export interface IAutomationData {
  id: number;
  sectionId: number;
  testContent?: string;
  sectionName?: string;
}

export enum QueueStatus {
  New = 'New',
  Running = 'Running',
  Completed = 'Completed',
  Failed = 'Failed',
  Pending = 'Pending', // Optional fallback
}

export interface IQueueInfo {
  id: number;
  queueId?: string;
  tagName?: string;
  empId?: string;
  queueName?: string;
  queueDescription?: string;
  productLine?: string;
  queueStatus?: string;
  createdDate?: Date;
  libraryName?: string;
  className?: string;
  methodName?: string;
}

export interface ITestResult {
  id: number;
  name?: string;
  resultStatus?: string;
  duration?: string;
  startTime?: Date;
  endTime?: Date;
  message?: string;
  className?: string;
  queueId?: string;
}

export interface TestResultPayload {
  page: number;
  pageSize: number;
  sortColumn?: string;
  sortDirection?: 'asc' | 'desc';
}
export interface PagedResult<T> {
  data: T[];
  totalCount: number;
}

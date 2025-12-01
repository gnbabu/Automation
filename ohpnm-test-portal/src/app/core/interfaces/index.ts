import { TemplateRef } from '@angular/core';

// grid-column.model.ts
export interface GridColumn {
  field: string;
  header: string;
  sortable?: boolean;
  cellTemplate?: TemplateRef<any> | null; // template reference name for custom cell templates
  type?: 'text' | 'number' | 'date' | 'datetime'; // optional for future extensibility
  width?: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
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
  priorityId?: number;
  priorityName?: string;
  lastLogin?: Date;
  timeZone?: number;
  timeZoneName?: string;
  status?: number;
  statusName?: string;
  phoneNumber?: string;
  twoFactor?: boolean;
  teams?: string;
}

export interface IUserFilter {
  search?: string;
  status?: number;
  role?: number;
  priority?: number;
}

export interface IUserRole {
  roleId: number;
  roleName: string;
}
export interface IUserStatus {
  statusId: number;
  statusName: string;
}

export interface ITimeZone {
  timeZoneId: number;
  timeZoneName: string;
  utcOffsetMinutes: number;
  description?: string; // optional
}

export interface IPriorityStatus {
  priorityId: number;
  priorityName: string;
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
  testCaseId?: string;
  description?: string;
  priority?: string;
}

export interface ClassInfo {
  className: string;
  methods: LibraryMethodInfo[];
}

export interface IAutomationFlow {
  flowName: string;
}

export interface IAutomationDataSection {
  sectionId: number;
  flowName: string;
  sectionName: string;
}

export interface IAutomationDataRequest {
  id?: number;
  sectionId?: number;
  testContent: string;
  userId?: number;
}

export interface IAutomationData {
  id?: number;
  sectionId: number;
  testContent: string;
  sectionName?: string;
  userId?: number;
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
  userId?: number;
  userName?: string;
  browser?: string;
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
  userId: number;
  page: number;
  pageSize: number;
  sortColumn?: string;
  sortDirection?: 'asc' | 'desc';
}

export interface PagedResult<T> {
  data: T[];
  totalCount: number;
}

export interface IPage {
  page: number; // Current page number (1-based)
  pageSize: number; // Number of items per page
  sortColumn?: string; // Optional: name of the column to sort
  sortDirection: 'ASC' | 'DESC'; // Sort direction
}

export interface IQueueSearchPayload extends IPage {
  userId: number;
}

export interface IQueueReport {
  id: number;
  queueName: string;
  queueStatus:
    | 'Completed'
    | 'Failed'
    | 'In Progress'
    | 'Awaiting'
    | 'Retried'
    | 'Cancelled';
  startedAt: string;
  endedAt: string;
  durationInSeconds: number;
}

// models/queue-report-filter-request.model.ts
export interface IQueueReportFilterRequest extends IPage {
  userId: number;
  status?: string;
  fromDate?: string | null;
  toDate?: string | null;
}
export interface ITestScreenshot {
  id: number;
  queueId: string;
  className?: string;
  methodName?: string;
  caption: string;
  screenshot: string; // Base64 string (data:image/png;base64,...)
  takenAt: string; // ISO date string
}

export interface ITestCaseAssignment {
  assignmentId?: number;
  userId?: number;
  libraryName: string;
  className: string;
  methodName: string;
  assignedOn?: string; // ISO date string
  assignedBy?: number;
}

export interface ITestCaseModel {
  libraryName: string;
  className: string;
  methodName: string;
  description: string;
  priority: string;
  testCaseId: string;
  assignedUsers: any[];
  assignedUserName: string;
  selected?: boolean;
}

export interface IAssignmentCreateUpdateRequest {
  assignedUser: number;
  assignmentStatus: string;
  releaseName: string;
  environment: string;
  assignedBy: number;
  testCases: ITestCaseRequestModel[];
}

export interface ITestCaseRequestModel {
  testCaseId: string;
  testCaseDescription?: string;
  testCaseStatus: string;
  className?: string;
  libraryName?: string;
  methodName?: string;
  priority?: string;
}
export interface ITestCaseAssignmentEntity {
  assignmentId: number;
  assignmentName: string;
  assignmentStatus: string;
  assignedUser: number;
  releaseName: string;
  environment: string;
  assignedDate: string; // ISO string from API
  assignedBy: number;
  lastUpdatedDate: string; // ISO string from API
  assignedUserName?: string;
  assignedByUserName?: string;
}

export interface IAssignedTestCase {
  assignmentTestCaseId: number;
  assignmentId: number;
  testCaseId: string;
  testCaseDescription: string;
  testCaseStatus: string;
  className: string;
  libraryName: string;
  methodName: string;
  priority: string;
  startTime?: string; // ISO string for DateTime
  endTime?: string; // ISO string for DateTime
  duration?: number;
  errorMessage: string;
  assignedUserId: number;
  assignedUserName: string;
  environment: string;
}

// Execution Implementation
export interface ISingleRunNowRequest {
  assignmentId: number;
  assignmentTestCaseId: number;
  browser: string;
}

export interface IBulkRunNowRequest {
  assignmentId: number;
  assignmentTestCaseIds: number[];
  browser: string;
}

export interface ISingleScheduleRequest {
  assignmentId: number;
  assignmentTestCaseId: number;
  scheduleDate: string; // ISO string
  browser: string;
}

export interface IBulkScheduleRequest {
  assignmentId: number;
  assignmentTestCaseIds: number[];
  scheduleDate: string; // ISO string
  browser: string;
}

export interface IQueueCreateResponse {
  id: number;
  queueId: string; // GUID
}

export enum TestCaseExecutionQueueStatus {
  Queued = 1,
  Scheduled = 2,
  InProgress = 3,
  Completed = 4,
  Failed = 5,
  Cancelled = 6,
}

export enum TestCaseStatus {
  Assigned = 1,
  Queued = 2,
  Scheduled = 3,
  Executing = 4,
  Completed = 5,
  Failed = 6,
  Blocked = 7,
}

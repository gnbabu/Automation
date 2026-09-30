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

export interface IUpdateOwnProfileRequest {
  photo?: string;
  phoneNumber?: string;
  timeZone?: number;
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

export interface IAutomationDataSectionRequest {
  sectionId?: number;
  sectionName: string;
  flowName: string;
}

export interface IAutomationDataRequest {
  id?: number;
  sectionId?: number;
  testContent: string;
  userId?: number;
  environmentId?: number;
}

export interface IAutomationData {
  id?: number;
  sectionId: number;
  testContent: string;
  sectionName?: string;
  userId?: number;
  environmentId?: number;
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
  testCaseStatus?: string;
}

export interface IAssignmentCreateUpdateRequest {
  assignedUser: number;
  assignmentStatus: string;
  releaseName: string;
  environment: string;
  releaseId: number;
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
  releaseId?: number;
  environmentId?: number;
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
  hasScreenshots: boolean;
  hasLogs: boolean;
}

// Execution Implementation
export interface ISingleRunNowRequest {
  assignmentId: number;
  assignmentTestCaseId: number;
  browser: string;
  loginUserId?: number;
}

export interface IBulkRunNowRequest {
  assignmentId: number;
  assignmentTestCaseIds: number[];
  browser: string;
  loginUserId?: number;
}

export interface ISingleScheduleRequest {
  assignmentId: number;
  assignmentTestCaseId: number;
  scheduleDate: string; // ISO string
  browser: string;
  loginUserId?: number;
}

export interface IBulkScheduleRequest {
  assignmentId: number;
  assignmentTestCaseIds: number[];
  scheduleDate: string; // ISO string
  browser: string;
  loginUserId?: number;
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

export interface ITestScreenshot {
  id: number;
  queueId: string;
  className?: string;
  methodName?: string;
  caption: string;
  screenshot: string; // Base64 string (data:image/png;base64,...)
  takenAt: string; // ISO date string
}

export interface ITestCaseExecutionLog {
  logId: number;
  assignmentId: number;
  assignmentTestCaseId: number;
  testCaseId: string;
  stepName: string;
  logMessage: string;
  logLevel: 'Info' | 'Pass' | 'Warning' | 'Fail';
  executionStatus: 'Running' | 'Passed' | 'Failed';
  screenshotId?: number;
  errorStackTrace?: string;
  createdAt: string;
}

export interface IEnvironmentModel {
  environmentId: number;
  environmentName: string;
  description?: string;
  isActive: boolean;
  createdBy: number;
  createdOn: string;
  userName: string;
  email: string;
  modifiedByName?: string | null;
  releaseCount: number;
  environmentUrl?: string | null;
  requiresAuthentication: boolean;
  enableSso: boolean;
}

export interface IEnvironmentRequestDto {
  environmentId?: number;
  environmentName: string;
  description?: string;
  createdBy: number;
  isActive?: boolean;
  environmentUrl?: string | null;
  requiresAuthentication?: boolean;
  enableSso?: boolean;
}

export interface ILoginUserModel {
  loginUserId: number;
  environmentId: number;
  portalUserId?: number | null;
  portalUserName?: string | null;
  userRole: string;
  userName: string;
  isActive: boolean;
  createdOn: string;
  modifiedOn?: string | null;
}

export interface ILoginUserRequestDto {
  loginUserId?: number;
  environmentId: number;
  portalUserId?: number | null;
  userRole: string;
  userName: string;
  password?: string;
  isActive?: boolean;
}

// ============ Release Management ============
export interface IReleaseModel {
  releaseId: number;
  releaseName: string;
  version: string;
  environmentId?: number;
  environmentName: string;
  description: string;
  releaseFolderPath: string;
  releaseLifecycle: string;
  isActive: boolean;
  signOffStatus: string;
  signedOffBy: string;
  signedOffOn?: string;
  createdOn: string;
  createdBy: string;
  modifiedBy: string;
  modifiedOn?: string;
  activatedBy: string;
  activatedOn?: string;
  dllFileCount: number;
  folderReady: boolean;
  totalTests: number;
  passedTests: number;
  failedTests: number;
  skippedTests: number;
  runningTests: number;
  totalDiscoveredTests: number;
}

export interface IReleaseRequestDto {
  releaseId?: number;
  releaseName: string;
  version: string;
  environmentId?: number;
  description?: string;
  releaseLifecycle?: string;
  isActive?: boolean;
  createdBy?: string;
  modifiedBy?: string;
}

export interface IReleaseSignOffRequest {
  signOffStatus: 'Approved' | 'Rejected';
  signOffBy?: string;
  comments?: string;
}

export interface IReleaseActivateRequest {
  activatedBy?: string;
}

export interface IReleaseSignOff {
  releaseSignOffId: number;
  releaseId: number;
  signOffStatus: string;
  signOffBy: string;
  signOffOn?: string;
  comments: string;
  createdOn: string;
}

export interface IReleaseNotification {
  releaseNotificationId: number;
  releaseId: number;
  notificationType: string;
  recipientUserId?: number;
  recipientEmail: string;
  status: string;
  message: string;
  createdOn: string;
  sentOn?: string;
}

export interface IUserNotification {
  notificationId: number;
  userId: number;
  notificationType: string;
  title: string;
  message?: string;
  linkUrl?: string;
  sourceType: string;
  sourceId?: number;
  isRead: boolean;
  readOn?: string;
  createdOn: string;
}

export interface IAuditLogEntry {
  auditLogId: number;
  entityType: string;
  entityId?: number;
  entityName: string;
  action: string;
  actorUserId?: number;
  actorUserName: string;
  details?: string;
  createdOn: string;
}

export interface IAuditLogFilter {
  entityType?: string;
  entityId?: number;
  actorUserId?: number;
  action?: string;
  fromDate?: string;
  toDate?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface IAuditLogPagedResult {
  items: IAuditLogEntry[];
  totalCount: number;
}

export interface IRecurringSchedule {
  recurringScheduleId: number;
  assignmentId: number;
  assignmentName: string;
  environment: string;
  environmentId?: number;
  releaseName: string;
  releaseLifecycle: string;
  recurrenceType: string;
  daysOfWeek?: string;
  dayOfMonth?: number;
  timeOfDay: string;
  browser: string;
  loginUserId?: number;
  loginUserRole?: string;
  loginUserName?: string;
  isActive: boolean;
  pausedReason?: string;
  endDate?: string;
  nextRunDate: string;
  lastRunDate?: string;
  runCount: number;
  createdBy?: string;
  createdOn: string;
}

export interface IRecurringScheduleRunHistory {
  runHistoryId: number;
  recurringScheduleId: number;
  runDate: string;
  result: string; // 'Queued' | 'NoEligibleTestCases' | 'Paused'
  detail?: string;
  testCasesQueuedCount: number;
  resolutionStatus: string; // 'Pending' | 'Resolved' | 'NotApplicable'
  passedCount?: number;
  failedCount?: number;
  skippedCount?: number;
  resolvedOn?: string;
}

export interface IRecurringScheduleRequest {
  assignmentId: number;
  recurrenceType: string;
  daysOfWeek?: string;
  dayOfMonth?: number;
  timeOfDay: string;
  browser: string;
  loginUserId?: number;
  endDate?: string;
}

export interface IAssignmentOption {
  assignmentId: number;
  assignmentName: string;
  environment: string;
  environmentId?: number;
  releaseId: number;
  releaseName: string;
  releaseLifecycle: string;
}

export interface IReleaseReadiness {
  folderExists: boolean;
  dllFiles: string[];
  usableDllCount: number;
  isReady: boolean;
  message: string;
}

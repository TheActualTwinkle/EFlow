import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { catchError, map, throwError } from 'rxjs';

import type { components } from '../api/contracts';
import { bookingApiBaseUrl, dataImportApiBaseUrl } from './environment';
import { HttpActivityService } from './http-activity.service';
import { localizeApiErrorCode, localizeValidationErrorCode } from './api-error-localization';
import { StudentImportField, StudentsImportResult } from '../features/student-import/student-import.models';

type Schemas = components['schemas'];
type BookingRecordView = Schemas['BookingRecordView'];
type CreateStudentRequest = Schemas['CreateStudentRequest'];
type CreateSubmissionSlotRequest = Schemas['CreateSubmissionSlotRequest'];
type CreateTeacherRequest = Schemas['CreateTeacherRequest'];
type GroupView = Schemas['GroupView'];
type NotBookedStudentsView = Schemas['NotBookedStudentsView'];
type StudentView = Schemas['StudentView'];
type SubjectView = Schemas['SubjectView'];
type SubmissionSlotView = Schemas['SubmissionSlotView'];
type TeacherView = Schemas['TeacherView'];

interface ApiProblem {
  title?: string;
  detail?: string;
  status?: number;
  code?: string;
  fallbackCode?: string;
  errors?: Record<string, string[]>;
}

type PersonRequestWithMiddleName = {
  middleName?: string | null;
};

type UpdatePersonFormRequest = {
  firstName: string;
  lastName: string;
  middleName?: string | null;
  birthDate: string;
};

type UpdateSubjectFormRequest = {
  name: string;
  teacherId?: string;
  groupIds: string[];
};

type UpdateSubmissionSlotFormRequest = {
  subjectId?: string;
  teacherId?: string;
  startTime: string;
  endTime: string;
  maxStudents: number;
  allowAllGroups: boolean;
  allowedGroupIds: string[];
  location?: string | null;
  comment?: string | null;
};

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly httpActivity = inject(HttpActivityService);
  private readonly errorState = signal<string | null>(null);

  readonly busy = this.httpActivity.busy;
  readonly error = this.errorState.asReadonly();

  constructor(private readonly http: HttpClient) {}

  clearError(): void {
    this.errorState.set(null);
  }

  getGroups() {
    return this.http.get<GroupView[]>(`${bookingApiBaseUrl}/groups`).pipe(catchError((error) => this.fail(error)));
  }

  getStudents() {
    return this.http.get<StudentView[]>(`${bookingApiBaseUrl}/students`).pipe(catchError((error) => this.fail(error)));
  }

  getStudent(id: string) {
    return this.http.get<StudentView>(`${bookingApiBaseUrl}/students/${id}`).pipe(catchError((error) => this.fail(error)));
  }

  getTeachers() {
    return this.http.get<TeacherView[]>(`${bookingApiBaseUrl}/teachers`).pipe(catchError((error) => this.fail(error)));
  }

  getTeacher(id: string) {
    return this.http.get<TeacherView>(`${bookingApiBaseUrl}/teachers/${id}`).pipe(catchError((error) => this.fail(error)));
  }

  getSubjects() {
    return this.http.get<SubjectView[]>(`${bookingApiBaseUrl}/subjects`).pipe(catchError((error) => this.fail(error)));
  }

  getSubject(id: string) {
    return this.http.get<SubjectView>(`${bookingApiBaseUrl}/subjects/${id}`).pipe(catchError((error) => this.fail(error)));
  }

  getSubjectsByTeacher(teacherId: string) {
    return this.http.get<SubjectView[]>(`${bookingApiBaseUrl}/subjects/by-teacher/${teacherId}`).pipe(catchError((error) => this.fail(error)));
  }

  getSlots() {
    return this.http.get<SubmissionSlotView[]>(`${bookingApiBaseUrl}/submission-slots`).pipe(catchError((error) => this.fail(error)));
  }

  getSlot(id: string) {
    return this.http.get<SubmissionSlotView>(`${bookingApiBaseUrl}/submission-slots/${id}`).pipe(catchError((error) => this.fail(error)));
  }

  getSlotsByTeacher(teacherId: string) {
    return this.http.get<SubmissionSlotView[]>(`${bookingApiBaseUrl}/submission-slots/by-teacher/${teacherId}`).pipe(catchError((error) => this.fail(error)));
  }

  getBookingsByStudent(studentId: string) {
    return this.http.get<BookingRecordView[]>(`${bookingApiBaseUrl}/bookings/by-student/${studentId}`).pipe(catchError((error) => this.fail(error)));
  }

  getBookings() {
    return this.http.get<BookingRecordView[]>(`${bookingApiBaseUrl}/bookings`).pipe(catchError((error) => this.fail(error)));
  }

  getBookingsBySlot(slotId: string, fetchGroups = false) {
    const params = fetchGroups ? new HttpParams().set('fetchGroups', true) : undefined;
    return this.http.get<BookingRecordView[]>(`${bookingApiBaseUrl}/bookings/by-slot/${slotId}`, { params }).pipe(catchError((error) => this.fail(error)));
  }

  getNotBookedStudents(slotId: string) {
    return this.http
      .get<NotBookedStudentsView>(`${bookingApiBaseUrl}/bookings/${slotId}/not-booked-students`)
      .pipe(catchError((error) => this.fail(error)));
  }

  getSlotAllowedStudents(slotId: string) {
    return this.http
      .get<StudentView[]>(`${bookingApiBaseUrl}/submission-slots/${slotId}/allowed-students`)
      .pipe(catchError((error) => this.fail(error)));
  }

  createGroup(name: string) {
    return this.postLocation(`${bookingApiBaseUrl}/groups`, { name });
  }

  createTeacher(request: CreateTeacherRequest) {
    return this.postLocation(`${bookingApiBaseUrl}/teachers`, this.normalizePersonRequest(request));
  }

  createStudent(request: CreateStudentRequest) {
    return this.postLocation(`${bookingApiBaseUrl}/students`, this.normalizePersonRequest(request));
  }

  createSubject(name: string, teacherId: string, groupIds: string[]) {
    return this.postLocation(`${bookingApiBaseUrl}/subjects`, { name, teacherId, groupIds });
  }

  createSlot(request: CreateSubmissionSlotRequest) {
    return this.postLocation(`${bookingApiBaseUrl}/submission-slots`, request);
  }

  updateGroup(id: string, name: string) {
    return this.http.patch<void>(`${bookingApiBaseUrl}/groups/${id}`, { name }).pipe(catchError((error) => this.fail(error)));
  }

  updateTeacher(id: string, request: UpdatePersonFormRequest) {
    return this.http.patch<void>(`${bookingApiBaseUrl}/teachers/${id}`, this.normalizePersonRequest(request)).pipe(catchError((error) => this.fail(error)));
  }

  updateStudent(id: string, request: UpdatePersonFormRequest) {
    return this.http.patch<void>(`${bookingApiBaseUrl}/students/${id}`, this.normalizePersonRequest(request)).pipe(catchError((error) => this.fail(error)));
  }

  updateUserEmail(id: string, email: string) {
    return this.http.patch<void>(`${bookingApiBaseUrl}/users/${id}/email`, { email }).pipe(catchError((error) => this.fail(error)));
  }

  updateUserPassword(id: string, currentPassword: string | null, newPassword: string) {
    return this.http.patch<void>(`${bookingApiBaseUrl}/users/${id}/password`, { currentPassword, newPassword }).pipe(catchError((error) => this.fail(error)));
  }

  updateSubject(id: string, request: UpdateSubjectFormRequest) {
    return this.http.patch<void>(`${bookingApiBaseUrl}/subjects/${id}`, request).pipe(catchError((error) => this.fail(error)));
  }

  updateSlot(
    id: string,
    request: UpdateSubmissionSlotFormRequest,
  ) {
    return this.http
      .patch<void>(`${bookingApiBaseUrl}/submission-slots/${id}`, this.normalizeSubmissionSlotRequest(request))
      .pipe(catchError((error) => this.fail(error)));
  }

  deleteSlot(id: string) {
    return this.http.delete<void>(`${bookingApiBaseUrl}/submission-slots/${id}`).pipe(catchError((error) => this.fail(error)));
  }

  deleteGroup(id: string) {
    return this.http.delete<void>(`${bookingApiBaseUrl}/groups/${id}`).pipe(catchError((error) => this.fail(error)));
  }

  deleteTeacher(id: string) {
    return this.http.delete<void>(`${bookingApiBaseUrl}/teachers/${id}`).pipe(catchError((error) => this.fail(error)));
  }

  deleteStudent(id: string) {
    return this.http.delete<void>(`${bookingApiBaseUrl}/students/${id}`).pipe(catchError((error) => this.fail(error)));
  }

  deleteSubject(id: string) {
    return this.http.delete<void>(`${bookingApiBaseUrl}/subjects/${id}`).pipe(catchError((error) => this.fail(error)));
  }

  addAdmission(slotId: string, studentId: string) {
    return this.http.post(`${bookingApiBaseUrl}/submission-slots/${slotId}/admissions/${studentId}`, {}).pipe(catchError((error) => this.fail(error)));
  }

  removeAdmission(slotId: string, studentId: string) {
    return this.http.delete<void>(`${bookingApiBaseUrl}/submission-slots/${slotId}/admissions/${studentId}`).pipe(catchError((error) => this.fail(error)));
  }

  bookSlot(slotId: string, studentId: string) {
    return this.postLocation(`${bookingApiBaseUrl}/bookings`, { slotId, studentId });
  }

  cancelBooking(id: string) {
    return this.http.delete<void>(`${bookingApiBaseUrl}/bookings/${id}`).pipe(catchError((error) => this.fail(error)));
  }

  updateNotificationSettings(slotId: string, userId: string, submissionRemindTimes: number[], bookingNotificationMode: number | null) {
    return this.http
      .put<void>(`${bookingApiBaseUrl}/submission-slots/${slotId}/notification-settings`, {
        userId,
        submissionRemindTimes,
        bookingNotificationMode,
      })
      .pipe(catchError((error) => this.fail(error)));
  }

  importStudentsCsv(groupId: string, file: File, fields: StudentImportField[], hasHeaderRow: boolean) {
    const formData = new FormData();
    formData.append('File', file, file.name);

    for (const field of fields) {
      formData.append('Fields', field);
    }

    formData.append('HasHeaderRow', String(hasHeaderRow));

    return this.http
      .post<StudentsImportResult>(`${dataImportApiBaseUrl}/csv/students`, formData, {
        params: { groupId },
      })
      .pipe(catchError((error) => this.fail(error)));
  }

  private postLocation(url: string, body: unknown) {
    return this.http.post(url, body, { observe: 'response', responseType: 'text' }).pipe(
      map((response) => response.headers.get('Location') ?? ''),
      catchError((error) => this.fail(error)),
    );
  }

  private normalizePersonRequest<T extends PersonRequestWithMiddleName>(request: T): T {
    return {
      ...request,
      middleName: this.optionalString(request.middleName),
    };
  }

  private normalizeSubmissionSlotRequest<T extends UpdateSubmissionSlotFormRequest>(request: T): T {
    return {
      ...request,
      location: this.optionalString(request.location),
      comment: this.optionalString(request.comment),
    };
  }

  private optionalString(value: string | null | undefined): string | null {
    const normalized = value?.trim();
    return normalized ? normalized : null;
  }

  private fail(error: HttpErrorResponse) {
    this.errorState.set(this.formatError(error));
    return throwError(() => error);
  }

  private formatError(error: HttpErrorResponse): string {
    const body = error.error as ApiProblem | string | undefined;

    if (typeof body === 'string' && body.trim()) {
      const problem = this.tryParseProblem(body);

      return problem ? this.formatProblem(problem) : body;
    }

    if (body && typeof body !== 'string') {
      return this.formatProblem(body);
    }

    return 'Не удалось выполнить запрос.';
  }

  private tryParseProblem(body: string): ApiProblem | null {
    try {
      const parsed = JSON.parse(body) as unknown;

      return parsed && typeof parsed === 'object' ? parsed as ApiProblem : null;
    } catch {
      return null;
    }
  }

  private formatProblem(problem: ApiProblem): string {
    const localizedMessage = localizeApiErrorCode(problem.code, problem.fallbackCode) ?? localizeValidationErrorCode(problem.code);
    if (localizedMessage) {
      return localizedMessage;
    }

    const validationMessage = this.formatValidationErrors(problem.errors);
    if (validationMessage) {
      return validationMessage;
    }

    return problem.detail ?? problem.title ?? 'Неизвестная ошибка.';
  }

  private formatValidationErrors(errors: ApiProblem['errors']): string | null {
    if (!errors) {
      return null;
    }

    const messages = Object.entries(errors)
      .flatMap(([key, values]) => values.map((value) => this.translateValidationError(key, value)))
      .filter(Boolean);

    return messages.length ? messages.join('\n') : null;
  }

  private translateValidationError(key: string, value: string): string {
    if (key === 'PasswordMismatch' || value === 'Incorrect password.') {
      return 'Неверный текущий пароль.';
    }

    if (key === 'CurrentPassword' && value.toLowerCase().includes('invalid')) {
      return 'Неверный текущий пароль.';
    }

    if (key === 'CurrentPassword' && value.toLowerCase().includes('required')) {
      return 'Введите текущий пароль.';
    }

    return value;
  }
}

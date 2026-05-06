import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { catchError, map, of, throwError } from 'rxjs';

import type { components } from '../api/contracts';
import { apiBaseUrl } from './environment';
import { HttpActivityService } from './http-activity.service';

type Schemas = components['schemas'];
type BookingRecordView = Schemas['BookingRecordView'];
type CreateStudentRequest = Schemas['CreateStudentRequest'];
type CreateSubmissionSlotRequest = Schemas['CreateSubmissionSlotRequest'];
type CreateTeacherRequest = Schemas['CreateTeacherRequest'];
type GroupView = Schemas['GroupView'];
type StudentView = Schemas['StudentView'];
type SubjectView = Schemas['SubjectView'];
type SubmissionSlotView = Schemas['SubmissionSlotView'];
type TeacherView = Schemas['TeacherView'];
type UpdateStudentRequest = Schemas['UpdateStudentRequest'];
type UpdateSubjectRequest = Schemas['UpdateSubjectRequest'];
type UpdateSubmissionSlotRequest = Schemas['UpdateSubmissionSlotRequest'];
type UpdateTeacherRequest = Schemas['UpdateTeacherRequest'];

interface ApiProblem {
  title?: string;
  detail?: string;
  status?: number;
  errors?: Record<string, string[]>;
}

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
    return this.http.get<GroupView[]>(`${apiBaseUrl}/groups`).pipe(catchError((error) => this.fail(error)));
  }

  getStudents() {
    return this.http.get<StudentView[]>(`${apiBaseUrl}/students`).pipe(catchError((error) => this.fail(error)));
  }

  getStudent(id: string) {
    return this.http.get<StudentView>(`${apiBaseUrl}/students/${id}`).pipe(catchError((error) => this.fail(error)));
  }

  getTeachers() {
    return this.http.get<TeacherView[]>(`${apiBaseUrl}/teachers`).pipe(catchError((error) => this.fail(error)));
  }

  getTeacher(id: string) {
    return this.http.get<TeacherView>(`${apiBaseUrl}/teachers/${id}`).pipe(catchError((error) => this.fail(error)));
  }

  getSubjects() {
    return this.http.get<SubjectView[]>(`${apiBaseUrl}/subjects`).pipe(catchError((error) => this.fail(error)));
  }

  getSubjectsByTeacher(teacherId: string) {
    return this.http.get<SubjectView[]>(`${apiBaseUrl}/subjects/by-teacher/${teacherId}`).pipe(catchError((error) => this.fail(error)));
  }

  getSlots() {
    return this.http.get<SubmissionSlotView[]>(`${apiBaseUrl}/submission-slots`).pipe(catchError((error) => this.fail(error)));
  }

  getSlot(id: string) {
    return this.http.get<SubmissionSlotView>(`${apiBaseUrl}/submission-slots/${id}`).pipe(catchError((error) => this.fail(error)));
  }

  getSlotsByTeacher(teacherId: string) {
    return this.http.get<SubmissionSlotView[]>(`${apiBaseUrl}/submission-slots/by-teacher/${teacherId}`).pipe(catchError((error) => this.fail(error)));
  }

  getBookingsByStudent(studentId: string) {
    return this.http.get<BookingRecordView[]>(`${apiBaseUrl}/bookings/by-student/${studentId}`).pipe(catchError((error) => this.fail(error)));
  }

  getBookings() {
    return this.http.get<BookingRecordView[]>(`${apiBaseUrl}/bookings`).pipe(catchError((error) => this.fail(error)));
  }

  getBookingsBySlot(slotId: string, fetchGroups = false) {
    const params = fetchGroups ? new HttpParams().set('fetchGroups', true) : undefined;
    return this.http.get<BookingRecordView[]>(`${apiBaseUrl}/bookings/by-slot/${slotId}`, { params }).pipe(catchError((error) => this.fail(error)));
  }

  createGroup(name: string) {
    return this.postLocation(`${apiBaseUrl}/groups`, { name });
  }

  createTeacher(request: CreateTeacherRequest) {
    return this.postLocation(`${apiBaseUrl}/teachers`, request);
  }

  createStudent(request: CreateStudentRequest) {
    return this.postLocation(`${apiBaseUrl}/students`, request);
  }

  createSubject(name: string, teacherId: string, groupIds: string[]) {
    return this.postLocation(`${apiBaseUrl}/subjects`, { name, teacherId, groupIds });
  }

  createSlot(request: CreateSubmissionSlotRequest) {
    return this.postLocation(`${apiBaseUrl}/submission-slots`, request);
  }

  updateGroup(id: string, name: string) {
    return this.http.patch<void>(`${apiBaseUrl}/groups/${id}`, { name }).pipe(catchError((error) => this.fail(error)));
  }

  updateTeacher(id: string, request: UpdateTeacherRequest) {
    return this.http.patch<void>(`${apiBaseUrl}/teachers/${id}`, request).pipe(catchError((error) => this.fail(error)));
  }

  updateStudent(id: string, request: UpdateStudentRequest) {
    return this.http.patch<void>(`${apiBaseUrl}/students/${id}`, request).pipe(catchError((error) => this.fail(error)));
  }

  updateSubject(id: string, request: UpdateSubjectRequest) {
    return this.http.patch<void>(`${apiBaseUrl}/subjects/${id}`, request).pipe(catchError((error) => this.fail(error)));
  }

  updateSlot(
    id: string,
    request: UpdateSubmissionSlotRequest,
  ) {
    return this.http.patch<void>(`${apiBaseUrl}/submission-slots/${id}`, request).pipe(catchError((error) => this.fail(error)));
  }

  deleteSlot(id: string) {
    return this.http.delete<void>(`${apiBaseUrl}/submission-slots/${id}`).pipe(catchError((error) => this.fail(error)));
  }

  deleteGroup(id: string) {
    return this.http.delete<void>(`${apiBaseUrl}/groups/${id}`).pipe(catchError((error) => this.fail(error)));
  }

  deleteTeacher(id: string) {
    return this.http.delete<void>(`${apiBaseUrl}/teachers/${id}`).pipe(catchError((error) => this.fail(error)));
  }

  deleteStudent(id: string) {
    return this.http.delete<void>(`${apiBaseUrl}/students/${id}`).pipe(catchError((error) => this.fail(error)));
  }

  deleteSubject(id: string) {
    return this.http.delete<void>(`${apiBaseUrl}/subjects/${id}`).pipe(catchError((error) => this.fail(error)));
  }

  addAdmission(slotId: string, studentId: string) {
    return this.http.post(`${apiBaseUrl}/submission-slots/${slotId}/admissions/${studentId}`, {}).pipe(catchError((error) => this.fail(error)));
  }

  removeAdmission(slotId: string, studentId: string) {
    return this.http.delete<void>(`${apiBaseUrl}/submission-slots/${slotId}/admissions/${studentId}`).pipe(catchError((error) => this.fail(error)));
  }

  bookSlot(slotId: string, studentId: string) {
    return this.postLocation(`${apiBaseUrl}/bookings`, { slotId, studentId });
  }

  cancelBooking(id: string) {
    return this.http.delete<void>(`${apiBaseUrl}/bookings/${id}`).pipe(catchError((error) => this.fail(error)));
  }

  updateNotificationSettings(slotId: string, userId: string, submissionRemindTimes: number[], bookingNotificationMode: number | null) {
    return this.http
      .put<void>(`${apiBaseUrl}/submission-slots/${slotId}/notification-settings`, {
        userId,
        submissionRemindTimes,
        bookingNotificationMode,
      })
      .pipe(catchError((error) => this.fail(error)));
  }

  getAvailableSlots(fromDate: string) {
    const params = new HttpParams().set('fromDate', fromDate);
    return this.http.get<SubmissionSlotView[]>(`${apiBaseUrl}/submission-slots/available`, { params }).pipe(catchError(() => of([])));
  }

  private postLocation(url: string, body: unknown) {
    return this.http.post(url, body, { observe: 'response', responseType: 'text' }).pipe(
      map((response) => response.headers.get('Location') ?? ''),
      catchError((error) => this.fail(error)),
    );
  }

  private fail(error: HttpErrorResponse) {
    this.errorState.set(this.formatError(error));
    return throwError(() => error);
  }

  private formatError(error: HttpErrorResponse): string {
    const body = error.error as ApiProblem | string | undefined;

    if (typeof body === 'string' && body.trim()) {
      return body;
    }

    if (body && typeof body !== 'string') {
      return body.detail ?? body.title ?? 'Неизвестная ошибка.';
    }

    return 'Не удалось выполнить запрос.';
  }
}

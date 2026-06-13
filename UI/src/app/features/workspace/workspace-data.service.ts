import { Injectable } from '@angular/core';
import { forkJoin, map, Observable } from 'rxjs';

import type { components } from '../../api/contracts';
import { ApiService } from '../../core/api.service';
import { AdminView, WorkspaceData } from './workspace.models';

type Schemas = components['schemas'];
type CurrentUser = Schemas['CurrentUserView'];

@Injectable({ providedIn: 'root' })
export class WorkspaceDataService {
  constructor(private readonly api: ApiService) {}

  loadOverview(user: CurrentUser, current: WorkspaceData, roles: RoleFlags): Observable<Partial<WorkspaceData>> {
    if (roles.admin) {
      return forkJoin({
        groups: this.api.getGroups(),
        slots: this.api.getSlots(),
      });
    }

    if (roles.student) {
      return this.loadStudentSlots(user, current);
    }

    if (roles.teacher) {
      return forkJoin({
        teacher: this.api.getTeacher(user.id),
        slots: this.api.getSlotsByTeacher(user.id),
      }).pipe(map((workspace) => ({
        teachers: [workspace.teacher],
        slots: workspace.slots,
      })));
    }

    return this.api.getSlots().pipe(map((slots) => ({ slots })));
  }

  loadSlots(user: CurrentUser, current: WorkspaceData, roles: RoleFlags): Observable<Partial<WorkspaceData>> {
    if (roles.admin) {
      return this.api.getSlots().pipe(map((slots) => ({ slots })));
    }

    if (roles.teacher) {
      return this.api.getSlotsByTeacher(user.id).pipe(map((slots) => ({ slots })));
    }

    return this.loadStudentSlots(user, current);
  }

  loadBookings(user: CurrentUser, current: WorkspaceData, roles: RoleFlags): Observable<Partial<WorkspaceData>> {
    if (roles.student) {
      return this.loadStudentSlots(user, current);
    }

    if (roles.teacher) {
      return forkJoin({
        teacher: this.api.getTeacher(user.id),
        slots: this.api.getSlotsByTeacher(user.id),
      }).pipe(map((workspace) => ({
        teachers: [workspace.teacher],
        slots: workspace.slots,
      })));
    }

    if (roles.admin) {
      return forkJoin({
        slots: this.api.getSlots(),
      });
    }

    return this.api.getSlots().pipe(map((slots) => ({ slots })));
  }

  loadAdminSection(section: AdminView): Observable<Partial<WorkspaceData>> {
    if (section === 'groups') {
      return this.api.getGroups().pipe(map((groups) => ({ groups })));
    }

    if (section === 'users') {
      return forkJoin({
        groups: this.api.getGroups(),
        students: this.api.getStudents(),
        teachers: this.api.getTeachers(),
      });
    }

    return forkJoin({
      groups: this.api.getGroups(),
      teachers: this.api.getTeachers(),
      subjects: this.api.getSubjects(),
    });
  }

  private loadStudentSlots(user: CurrentUser, current: WorkspaceData): Observable<Partial<WorkspaceData>> {
    const cachedStudent = current.students.find((student) => student.id === user.id);

    if (cachedStudent) {
      return forkJoin({
        slots: this.api.getSlots(),
        bookings: this.api.getBookingsByStudent(user.id),
      });
    }

    return forkJoin({
      student: this.api.getStudent(user.id),
      slots: this.api.getSlots(),
      bookings: this.api.getBookingsByStudent(user.id),
    }).pipe(
      map((workspace) => ({
        students: [workspace.student],
        slots: workspace.slots,
        bookings: workspace.bookings,
      })),
    );
  }
}

export interface RoleFlags {
  admin: boolean;
  teacher: boolean;
  student: boolean;
}
